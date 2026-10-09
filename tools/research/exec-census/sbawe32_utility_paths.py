"""Reads what SBAWE32.ADV does for the sound utility's four driver functions (FND-EXE-501).

Usage: python -I sbawe32_utility_paths.py PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

Reads CD:SBAWE32.ADV through disc_adv_exec_census (identity checked against the manifest; its
census output is suppressed) and, decoding from the driver's function table entries 0064..0067
only (the functions FND-EXE-497 finds the utility requests), prints for each: the decoded byte
count, the calls to 0x01ED (the only direct caller of the MIDI dispatcher 0x1A45) with the
instructions that set up their arguments, and whether 0x1A45, 0x27FA, 0x2745 and 0x2C28 are
reached. It decodes function 0067 again without following the jump at 0x0DF8 into its sequence
loop, and prints what that visits. It then prints the shipped bytes of the init function's
controller, value and program tables (0x0263, 0x026C, 0x0274) and of the sequence count at
0x039E, every decoded instruction
that names 039E, every decoded store or block store that can reach 0x0263..0x026B, and the
DS:4428 rows the init function's control changes select. Nothing is written or executed.
"""
import io
import os
import re
import struct
import sys
from contextlib import redirect_stdout

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import xxhash  # noqa: E402
from capstone import CS_ARCH_X86, CS_MODE_16, Cs  # noqa: E402

with redirect_stdout(io.StringIO()):
    from disc_adv_exec_census import expected, files  # noqa: E402

md = Cs(CS_ARCH_X86, CS_MODE_16)
path = "CD:SBAWE32.ADV"
body = files[path]
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == expected[path]


def function_table():
    off = struct.unpack_from("<H", body, 0)[0]
    out = {}
    while True:
        number, target = struct.unpack_from("<HH", body, off)
        if number == 0xFFFF:
            return out
        out[number] = target
        off += 4


def walk(entries, cut=None):
    """Decodes from entries; a branch at offset `cut` is not followed."""
    seen, work = {}, list(entries)
    while work:
        p = work.pop()
        while p not in seen and 0 <= p < len(body):
            i = next(md.disasm(body[p:p + 16], p), None)
            if i is None:
                break
            seen[p] = i
            if i.mnemonic in ("ret", "retf", "iret", "hlt", "ljmp"):
                break
            if re.match(r"j|call|loop", i.mnemonic):
                if i.op_str.startswith("0x") and p != cut:
                    work.append(int(i.op_str, 16))
                if i.mnemonic == "jmp":
                    break
            p += i.size
    return seen


functions = function_table()
for number in (0x64, 0x65, 0x66, 0x67):
    seen = walk([functions[number]])
    items = sorted(seen.items())
    addrs = [a for a, _ in items]
    reached = {f"{k:04X}": k in seen for k in (0x1A45, 0x27FA, 0x2745, 0x2C28)}
    print(f"function {number:04X} at 0x{functions[number]:04X}: decoded {sum(i.size for i in seen.values())} bytes, "
          f"reaches {reached}")
    for p, i in items:
        if i.mnemonic == "call" and i.op_str == "0x1ed":
            k = addrs.index(p)
            setup = " | ".join(f"{x.mnemonic} {x.op_str}" for _, x in items[max(0, k - 12):k])
            print(f"    call 0x01ED at 0x{p:04X} after: {setup}")

no_loop = walk([functions[0x67]], cut=0x0DF8)
print("function 0067 without the jump at 0x0DF8 into its sequence loop visits "
      f"{' '.join(f'{p:04X}' for p in sorted(no_loop))}; calls: "
      f"{[f'0x{p:04X}' for p, i in sorted(no_loop.items()) if i.mnemonic == 'call'] or 'none'}")
print(f"shipped 0x0263..0x026B {body[0x263:0x26C].hex(' ')}; 0x026C..0x0274 {body[0x26C:0x275].hex(' ')}; "
      f"0x0274..0x027D {body[0x274:0x27E].hex(' ')}; 0x039E {body[0x39E:0x3A0].hex(' ')}")

starts = list(functions.values()) + [struct.unpack_from("<H", body, 0x1A28 + 4 * k + 1)[0] + 0x1A28 + 4 * k + 3
                                     for k in range(7)] + list(struct.unpack_from("<128H", body, 0x4428))
everything = walk(starts)
owners = {n: walk([t]) for n, t in functions.items()}
for p, i in sorted(everything.items()):
    if re.search(r"\[(?:[a-z]{2}(?: \+ [a-z]{2})? \+ )?0x39[ef]\]", i.op_str):
        print(f"    names 039E: 0x{p:04X} {i.mnemonic} {i.op_str}; in functions "
              f"{[f'{n:04X}' for n, s in sorted(owners.items()) if p in s]}")
items = sorted(everything.items())
for k, (p, i) in enumerate(items):
    dest = i.op_str.split(", ")[0]
    m = re.search(r"\[(?:[a-z]{2}(?: \+ [a-z]{2})? \+ )?(0x[0-9a-f]+)\]", dest)
    if m and "," in i.op_str and 0x263 <= int(m.group(1), 16) < 0x26C and i.mnemonic not in ("cmp", "test"):
        print(f"    store into 0x0263..0x026B: 0x{p:04X} {i.mnemonic} {i.op_str}")
    if i.mnemonic == "mov" and re.fullmatch(r"di, 0x[0-9a-f]+", i.op_str) and int(i.op_str[4:], 16) <= 0x26B:
        following = [f"{x.mnemonic} {x.op_str}" for _, x in items[k + 1:k + 4]]
        if any(f.startswith("rep") for f in following):
            print(f"    block store from di={i.op_str[4:]} at 0x{p:04X}: {following}")
rows = sorted(set(body[0x263:0x26C]))
print("DS:4428 rows for the init function's controllers:",
      " ".join(f"{c:02X}->{struct.unpack_from('<H', body, 0x4428 + 2 * c)[0]:04X}" for c in rows))

"""Follows the combatant table's segment word (DS:19CB) inside each decoded range of DSUN.EXE.

Usage: python -I table_flow.py PATH_TO_DSUN.EXE INVENTORY_TSV

Decodes inventory rows and the gaps between them as block_copies.py does. Within a range, the
table segment is "held" by ES after `les r, [0x19c9]` or `mov es, [0x19cb]`, and by a register
after `mov r, [0x19cb]`, until that register is written again or a call or jump ends the
straight-line run, so a value kept across a branch or in memory is not followed. A dword push of
[0x19c9] counts as a push of the segment. Nothing is written or executed. It prints:
- PUSH: the table segment is pushed (push [0x19cb], push dword [0x19c9], push es / push r while
  held), with the next call within 24 instructions and its resolved target;
- STORE: the segment is written to memory other than through ES-based field stores;
- MOVS: a string move while ES holds the segment.
"""
import collections
import re
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (634416, "e296af55ba2ecde7e77f555c90f33d0b")
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, relocs = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image_end = (pages - 1) * 512 + (cblp or 512)
relocated = {header + struct.unpack_from("<H", body, reloc_at + 4 * k + 2)[0] * 16 + struct.unpack_from("<H", body, reloc_at + 4 * k)[0] for k in range(relocs)}
table_offset, count = struct.unpack_from("<IH", body, image_end + 8)
rows = [struct.unpack_from("<HHHH", body, table_offset + 8 * i) for i in range(count)]
overlays = {}
for i, (seg, _, flags, _) in enumerate(rows):
    if flags == 3:
        payload, code_size, fixup_size = struct.unpack_from("<IHH", body, header + seg * 16 + 4)
        code = image_end + 16 + payload
        fixups = {code + struct.unpack_from("<H", body, code + code_size + 2 * k)[0] for k in range(fixup_size // 2)}
        overlays[i] = (code, code + code_size, fixups, seg)
md = Cs(CS_ARCH_X86, CS_MODE_16)


def overlay_of(at):
    return next((i for i, o in overlays.items() if o[0] <= at < o[1]), None)


def name(at, seg):
    if seg is None:
        o = overlay_of(at)
        return f"{o}+{at - overlays[o][0]:04X}" if o is not None else f"file {at:X}"
    return f"{seg:04X}:{at - header - (seg - 0x1000) * 16:04X}"


def target(ins, seg):
    raw = ins.bytes
    if raw[0] == 0x9A:
        off, word = struct.unpack_from("<HH", raw, 1)
        if seg is not None:
            return f"{word + 0x1000:04X}:{off:04X}" if ins.address + 3 in relocated else f"?{word:04X}:{off:04X}"
        o = overlay_of(ins.address)
        if o is None or ins.address + 3 not in overlays[o][2] or word % 8:
            return f"?{word:04X}:{off:04X}"
        d = word // 8
        if d in overlays:
            t = header + overlays[d][3] * 16 + off
            if body[t:t + 2] == b"\xcd\x3f":
                return f"{d}+{struct.unpack_from('<H', body, t + 2)[0]:04X}"
            return f"{d}:tramp{off:04X}"
        return f"{rows[d][0] + 0x1000:04X}:{off:04X}"
    if raw[0] == 0xE8:
        dest = ins.address + 3 + struct.unpack_from("<h", raw, 1)[0]
        if seg is None:
            return f"{overlay_of(ins.address)}+{dest - overlays[overlay_of(ins.address)][0]:04X}"
        base = header + (seg - 0x1000) * 16
        return f"{seg:04X}:{(dest - base) & 0xFFFF:04X}"
    return ins.mnemonic + " " + ins.op_str


SEG_READ = re.compile(r"word ptr \[0x19cb\]$")
pushes = collections.Counter()
lines = []


def scan(lo, hi, seg):
    decoded, at = [], lo
    while at < hi:
        part = list(md.disasm(body[at:hi], at))
        decoded += part
        end = part[-1].address + part[-1].size if part else at
        at = end + 1 if end < hi else hi
    held = set()
    for n, ins in enumerate(decoded):
        m, ops = ins.mnemonic, ins.op_str
        first = ops.split(",")[0].strip()
        pushed = False
        if m == "push" and ("[0x19cb]" in ops or "dword ptr [0x19c9]" in ops):
            pushed = True
        elif m == "push" and first in held:
            pushed = True
        if pushed:
            call = next((c for c in decoded[n + 1:n + 25] if c.mnemonic in ("lcall", "call")), None)
            t = target(call, seg) if call else "none"
            pushes[t] += 1
            lines.append(f"PUSH {name(ins.address, seg)}: {m} {ops} -> {t}")
        if m.startswith("rep") or m in ("movsb", "movsw"):
            if "es" in held and m.split()[-1] in ("movsb", "movsw"):
                lines.append(f"MOVS {name(ins.address, seg)}: {m} {ops}")
        if m == "mov" and "ptr [" in first and any(r in ops.split(",")[1] for r in held if r != "es") or (m == "mov" and "ptr [" in first and ops.endswith(", es") and "es" in held):
            lines.append(f"STORE {name(ins.address, seg)}: {m} {ops}")
        # Update what holds the segment.
        if m == "les" and ops.endswith("[0x19c9]"):
            held.add("es")
        elif m == "mov" and first == "es" and SEG_READ.search(ops):
            held.add("es")
        elif m == "mov" and SEG_READ.search(ops):
            held.add(first)
        elif m in ("call", "lcall", "ret", "retf", "jmp", "ljmp", "iret") or m.startswith("j"):
            held = set()
        else:
            if first in held:
                held.discard(first)
            if m in ("les", "pop") and first == "es":
                held.discard("es")
            if m == "les":
                held.discard("es")


covered = []
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    for item in line.split("\t")[2].split():
        low, high = item.split("..")
        if ":" in low:
            s, o = (int(p, 16) for p in low.split(":"))
            lo = header + (s - 0x1000) * 16 + o
            hi = header + (s - 0x1000) * 16 + int(high.split(":")[1], 16)
            scan(lo, hi, s)
        else:
            lo, hi, s = int(low, 16), int(high, 16), None
            scan(lo, hi, None)
        covered.append((lo, hi))
covered.sort()
for low, high, seg in [(header, image_end, 0x1000)] + [(o[0], o[1], None) for o in overlays.values()]:
    at = low
    for lo, hi in covered + [(high, high)]:
        if hi <= low or lo >= high or hi <= at:
            continue
        if lo > at:
            scan(at, min(lo, high), None if seg is None else 0x1000 + (at - header) // 16)
        at = max(at, hi)
print("\n".join(lines))
print("-- callees")
for t, c in pushes.most_common():
    print(c, t)

"""Kinds of entry FND-EXE-496's decode of the disc drivers did not start from, and a control (FND-EXE-573).

Usage: python -I disc_adv_entry_kinds.py INSTALL_DIR PATH_TO_BLD-GOG-EN-1.1.files.yaml

Decodes each CD:*.ADV driver from the same starts as disc_adv_flow_scan.py (function table,
far-pointer handlers, SBAWE32.ADV's two bounded tables) and prints, per driver:
- vectors it could install: decoded interrupt 21 requests with AH=25, every `CD 21` in the file
  with a `mov ah,25` or `mov ax,25xx` among the 8 bytes before it, and every decoded instruction
  that loads ES or DS from a register zeroed by one of the three instructions before it;
- transfers built on the stack: decoded calls or jumps through a BP- or SS-based operand, and
  decoded `ret`/`retf` right after a push;
- code written at run time: decoded stores whose CS-relative destination lies inside a decoded
  instruction, and decoded string stores (`stos`, `movs`) after `push cs; pop es` whose range, from
  the nearest `mov di` and `mov cx` immediates before them, covers a decoded instruction;
- the control: every `CD xx` byte pair in the file, as a decoded interrupt instruction, inside
  another decoded instruction, or in bytes the decode does not cover.
Everything is read in memory; nothing is executed.
"""
import io
import os
import re
import struct
import sys
from collections import Counter
from contextlib import redirect_stdout

root, manifest = sys.argv[1], sys.argv[2]
sys.argv = [sys.argv[0], os.path.join(root, "game.gog"), manifest]
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import xxhash  # noqa: E402
from capstone import CS_ARCH_X86, CS_MODE_16, Cs  # noqa: E402

with redirect_stdout(io.StringIO()) as census:
    from disc_adv_exec_census import expected, files  # noqa: E402
print(census.getvalue().splitlines()[0])

md = Cs(CS_ARCH_X86, CS_MODE_16)
EXTRA_TABLES = {"CD:SBAWE32.ADV": [(0x3B42, 7), (0x4428, 128)]}


# function_table, walk and far_seeds as in disc_adv_flow_scan.py, which runs its report on import.
def function_table(body):
    off = struct.unpack_from("<H", body, 0)[0]
    pairs = []
    while True:
        number, target = struct.unpack_from("<HH", body, off)
        if number == 0xFFFF:
            return pairs
        pairs.append((number, target))
        off += 4


def walk(body, entries):
    seen = {}
    work = list(entries)
    while work:
        p = work.pop()
        while p not in seen and 0 <= p < len(body):
            i = next(md.disasm(body[p:p + 16], p), None)
            if i is None:
                break
            seen[p] = i
            m = i.mnemonic
            if m in ("ret", "retf", "iret", "hlt", "ljmp"):
                break
            if m.startswith("j") or m in ("call", "loop", "loope", "loopne"):
                if i.op_str.startswith("0x"):
                    work.append(int(i.op_str, 16))
                if m == "jmp":
                    break
            p += i.size
    return seen


def far_seeds(seen):
    items = sorted(seen.items())
    out = set()
    for k, (p, i) in enumerate(items):
        nxt = [x[1] for x in items[k + 1:k + 3]]
        m = re.fullmatch(r"ax, (0x[0-9a-f]+)", i.op_str)
        if i.mnemonic == "mov" and m and len(nxt) == 2 and [(x.mnemonic, x.op_str) for x in nxt] == [
                ("push", "cs"), ("push", "ax")]:
            out.add(int(m.group(1), 16))
        m = re.fullmatch(r"word ptr cs:\[(0x[0-9a-f]+)\], (0x[0-9a-f]+)", i.op_str)
        if i.mnemonic == "mov" and m:
            pair = f"word ptr cs:[{hex(int(m.group(1), 16) + 2)}], cs"
            if any(x.mnemonic == "mov" and x.op_str == pair for _, x in items[max(0, k - 2):k + 3]):
                out.add(int(m.group(2), 16))
    return out


totals = Counter()
for path in sorted(files):
    body = files[path]
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != expected.get(path):
        print(f"{path}: identity mismatch")
        continue
    entries = [t for _, t in function_table(body)]
    for start, count in EXTRA_TABLES.get(path, []):
        entries += list(struct.unpack_from(f"<{count}H", body, start))
    while True:
        seen = walk(body, entries)
        new = {s for s in far_seeds(seen) - set(entries) if body[s:s + 4] != bytes(4)}
        if not new:
            break
        entries += sorted(new)
    items = sorted(seen.items())
    covered = {}
    for p, i in items:
        for b in range(p, p + i.size):
            covered[b] = p
    print(f"{path}: decoded {sum(i.size for i in seen.values())} of {len(body)} bytes")

    vectors = []
    for k, (p, i) in enumerate(items):
        if i.mnemonic == "int" and i.op_str == "0x21" and any(
                x.mnemonic == "mov" and re.fullmatch(r"a(h, 0x25|x, 0x25[0-9a-f]{2})", x.op_str) for _, x in items[max(0, k - 6):k]):
            vectors.append(f"decoded AH=25 at 0x{p:04X}")
        if i.mnemonic == "mov" and re.fullmatch(r"[ed]s, \w+", i.op_str):
            reg = i.op_str.split(", ")[1]
            if any((x.mnemonic == "xor" and x.op_str == f"{reg}, {reg}") or (x.mnemonic == "mov" and x.op_str == f"{reg}, 0")
                   for _, x in items[max(0, k - 3):k]):
                vectors.append(f"segment 0 loaded at 0x{p:04X} ({i.op_str})")
    installers = sorted({max(q for q in range(0, int(v.split()[4], 16)) if body[q:q + 3] == bytes.fromhex("558bec"))
                         for v in vectors if v.startswith("segment 0")})
    for start in installers:
        calls = [q for q in range(len(body) - 3) if body[q] == 0xE8
                 and (q + 3 + struct.unpack_from("<h", body, q + 1)[0]) & 0xFFFF == start]
        words = [q for q in range(len(body) - 1) if struct.unpack_from("<H", body, q)[0] == start]
        passed = []
        for q in calls:
            m = body[q - 6:q]
            passed.append(f"0x{q:04X} passes {struct.unpack_from('<H', m, 1)[0]:04X}" if m[0] == 0xB8 and m[3:] == bytes.fromhex("0e500e")
                          else f"0x{q:04X} passes no immediate")
        vectors.append(f"routine 0x{start:04X}: words equal to its offset {len(words)}; calls: {', '.join(passed)}")
    raw25 = [p for p in range(8, len(body) - 1) if body[p:p + 2] == b"\xcd\x21"
             and any(body[q] == 0xB4 and body[q + 1] == 0x25 or body[q] == 0xB8 and body[q + 2] == 0x25 for q in range(p - 8, p - 1))]
    for p in raw25:
        vectors.append(f"CD 21 after a 25 load at 0x{p:04X} ({'decoded' if p in seen else 'inside decoded code' if p in covered else 'undecoded bytes'})")

    stack = [f"0x{p:04X} {i.mnemonic} {i.op_str}" for p, i in items
             if (i.mnemonic in ("call", "lcall", "jmp", "ljmp") and re.search(r"(ss:|\[bp)", i.op_str))]
    stack += [f"0x{p:04X} {i.mnemonic} after a push" for k, (p, i) in enumerate(items)
              if i.mnemonic in ("ret", "retf") and k and items[k - 1][1].mnemonic in ("push", "pushf")]

    written, fills = [], []
    for k, (p, i) in enumerate(items):
        dest = i.op_str.split(", ")[0]
        m = re.fullmatch(r"(?:byte|word|dword) ptr cs:\[(0x[0-9a-f]+)\]", dest)
        if m and "," in i.op_str and i.mnemonic not in ("cmp", "test"):
            a = int(m.group(1), 16)
            hit = [covered[b] for b in range(a, a + 4 if dest.startswith("dword") else a + 2 if dest.startswith("word") else a + 1)
                   if b in covered]
            if hit:
                written.append(f"0x{p:04X} {i.mnemonic} {i.op_str} into the instruction at 0x{hit[0]:04X}")
        if re.match(r"(rep )?(stos|movs)", i.mnemonic) and "es:[di]" in i.op_str:
            before = [x for _, x in items[max(0, k - 12):k]]
            es_cs = any((a.mnemonic, a.op_str, b.mnemonic, b.op_str) == ("push", "cs", "pop", "es")
                        for a, b in zip(before, before[1:]))
            if not es_cs:
                continue
            di = next((int(x.op_str.split(", ")[1], 16) for x in reversed(before)
                       if x.mnemonic == "mov" and re.fullmatch(r"di, 0x[0-9a-f]+|di, \d+", x.op_str)), None)
            cx = next((int(x.op_str.split(", ")[1], 0) for x in reversed(before)
                       if x.mnemonic == "mov" and re.fullmatch(r"cx, (0x[0-9a-f]+|\d+)", x.op_str)), None)
            if di is None:
                written.append(f"0x{p:04X} {i.mnemonic} {i.op_str} into CS at a DI not loaded nearby")
                continue
            width = 2 if i.mnemonic.endswith("w") else 1
            span = range(di, di + (cx if i.mnemonic.startswith("rep") and cx is not None else 1) * width)
            hit = sorted({covered[b] for b in span if b in covered})
            fills.append(f"0x{p:04X} CS:{span.start:04X}..{span.stop:04X}")
            if hit:
                written.append(f"0x{p:04X} {i.mnemonic} fills CS:{span.start:04X}..{span.stop:04X}, over instructions at "
                               + ", ".join(f"0x{h:04X}" for h in hit[:4]))

    pairs = Counter()
    undecoded = []
    for p in range(len(body) - 1):
        if body[p] != 0xCD:
            continue
        if p in seen and seen[p].mnemonic == "int":
            pairs["decoded interrupt"] += 1
        elif p in covered:
            pairs["inside a decoded instruction"] += 1
        else:
            pairs["undecoded bytes"] += 1
            undecoded.append(p)
    totals.update(pairs)
    print(f"    vectors: {vectors or 'none'}")
    print(f"    stack-built transfers: {stack or 'none'}")
    print(f"    code written at run time: {written or 'none'}; CS fills checked: {len(fills)}")
    print(f"    CD byte pairs: {dict(pairs)}")
    for p in undecoded:
        start = max((q for q in covered if q < p), default=-1) + 1
        end = min((q for q in covered if q > p), default=len(body))
        print(f"      undecoded CD {body[p + 1]:02X} at 0x{p:04X}, in the undecoded span 0x{start:04X}..0x{end:04X}: "
              f"{body[max(start, p - 6):p + 8].hex(' ')}")
print(f"all drivers: {dict(totals)}")

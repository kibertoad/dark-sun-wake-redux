"""Follows control flow in the disc's Miles .ADV drivers from their own entry points (FND-EXE-496).

Usage: python -I disc_adv_flow_scan.py INSTALL_DIR PATH_TO_BLD-GOG-EN-1.1.files.yaml

For each CD:*.ADV file (identity checked against the manifest by disc_adv_exec_census, which
prints its own census first), decodes from:
- every offset in the driver's function table (the word at file offset 0 points to number/offset
  pairs ending at number FFFF);
- every handler the decoded code hands out as a far pointer, by `mov ax,imm; push cs; push ax`
  or by `mov cs:[X],imm` beside `mov cs:[X+2],cs`, unless its first four bytes are zero (a
  buffer);
- SBAWE32.ADV's two bounded near tables: CS:3B42 entries 0..6 (index (status >> 4) - 8 for
  status bytes below F0) and CS:4428 entries 0..127.
It follows direct jumps, calls and fall-through, stops at ret, retf, iret and jmp, and prints the
decoded byte count, every interrupt instruction with the nearest preceding immediate AX or AH
writer, every far transfer (with the `les` or `mov es` that set ES for one through ES), every near transfer
through memory, every decoded write to each CS-relative slot a far transfer reads, every
immediate operand with a CD byte, and every ret or retf right after a push. It then lists install and disc files whose names end in .SYS or .COM or whose
bytes contain ULTRAMID or MVSOUND. Everything is read in memory; nothing is executed.
"""
import io
import os
import re
import struct
import sys
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


def selector(items, k):
    for _, x in reversed(items[max(0, k - 12):k]):
        if x.mnemonic == "mov" and re.fullmatch(r"a[xh], 0x[0-9a-f]+", x.op_str):
            return x.op_str
    return "no immediate AX/AH writer in the 12 preceding instructions"


for path in sorted(files):
    body = files[path]
    if (len(body), xxhash.xxh3_128_hexdigest(body)) != expected.get(path):
        print(f"{path}: identity mismatch")
        continue
    entries = [t for _, t in function_table(body)]
    for start, count in EXTRA_TABLES.get(path, []):
        entries += list(struct.unpack_from(f"<{count}H", body, start))
    seeded = set()
    while True:
        seen = walk(body, entries)
        new = {s for s in far_seeds(seen) - set(entries) if body[s:s + 4] != bytes(4)}
        if not new:
            break
        seeded |= new
        entries += sorted(new)
    items = sorted(seen.items())
    print(f"{path}: {len(function_table(body))} functions, handlers {sorted(f'{s:04X}' for s in seeded)}, "
          f"decoded {sum(i.size for i in seen.values())} of {len(body)} bytes")
    slots = set()
    for k, (p, i) in enumerate(items):
        m, ops = i.mnemonic, i.op_str
        if m == "int":
            print(f"    0x{p:04X} int {ops}: {selector(items, k)}")
        elif m in ("lcall", "ljmp") or (m in ("call", "jmp") and "[" in ops):
            source = ""
            if ops.startswith("es:"):
                source = next((f" (ES from 0x{a:04X} {x.mnemonic} {x.op_str})" for a, x in reversed(items[max(0, k - 8):k])
                               if x.mnemonic == "les" or (x.mnemonic == "mov" and x.op_str.startswith("es,"))), "")
            guard = ""
            slot = re.search(r"cs:\[(0x[0-9a-f]+)\]$", ops)
            if slot:
                s = int(slot.group(1), 16)
                window = [f"{x.mnemonic} {x.op_str}" for a, x in items[max(0, k - 8):k]]
                if f"mov ax, word ptr cs:[{hex(s)}]" in window and f"or ax, word ptr cs:[{hex(s + 2)}]" in window:
                    guard = " (skipped when both slot words are zero)"
            print(f"    0x{p:04X} {m} {ops}{source}{guard}")
            slot = re.search(r"cs:\[(0x[0-9a-f]+)\]$", ops)
            if m in ("lcall", "ljmp") and slot:
                slots.add(int(slot.group(1), 16))
            slot = re.search(r"les di, (?:dword )?ptr cs:\[(0x[0-9a-f]+)\]", source)
            if slot:
                slots.add(int(slot.group(1), 16))
    cd_immediates, pushed_returns = [], []
    for k, (p, i) in enumerate(items):
        imm = i.op_str.split(", ")[-1]
        if re.fullmatch(r"0x[0-9a-f]+", imm) and not re.match(r"j|call|loop", i.mnemonic):
            if 0xCD in (int(imm, 16) & 0xFF, int(imm, 16) >> 8):
                cd_immediates.append(f"0x{p:04X} {i.mnemonic} {i.op_str}")
        if i.mnemonic in ("ret", "retf") and k and items[k - 1][1].mnemonic in ("push", "pushf"):
            pushed_returns.append(f"0x{p:04X}")
    print(f"    immediates with a CD byte: {cd_immediates or 'none'}; "
          f"returns right after a push: {pushed_returns or 'none'}")
    for s in sorted(slots):
        writers = [f"0x{p:04X} {i.mnemonic} {i.op_str}" for p, i in items
                   if i.mnemonic in ("mov", "les", "lds", "pop", "xchg", "add", "sub", "or", "and", "inc", "dec")
                   and re.match(r"(word ptr |dword ptr )?cs:\[(%s|%s)\]," % (hex(s), hex(s + 2)), i.op_str)]
        print(f"    slot cs:[{s:04X}] writers: {'; '.join(writers) or 'none'}; shipped bytes "
              f"{body[s:s + 4].hex(' ')}")

names, strings, checked = [], [], 0
needles = (b"ULTRAMID", b"MVSOUND")
for dirpath, _, filenames in os.walk(root):
    for name in filenames:
        full = os.path.join(dirpath, name)
        rel = os.path.relpath(full, root).replace("\\", "/")
        if rel == "game.gog":
            continue
        checked += 1
        if name.upper().endswith((".SYS", ".COM")):
            names.append(rel)
        data = open(full, "rb").read()
        strings += [(rel, n.decode()) for n in needles if n in data]

SECTOR, DATA = 2352, 24
disc_checked = 0
with open(os.path.join(root, "game.gog"), "rb") as f:
    def sectors(lba, count):
        out = bytearray()
        for n in range(count):
            f.seek((lba + n) * SECTOR + DATA)
            out += f.read(2048)
        return bytes(out)

    def tree(lba, length, prefix):
        global disc_checked
        data = sectors(lba, (length + 2047) // 2048)
        pos = 0
        while pos < length:
            n = data[pos]
            if n == 0:
                pos = (pos // 2048 + 1) * 2048
                continue
            ext = int.from_bytes(data[pos + 2:pos + 6], "little")
            size = int.from_bytes(data[pos + 10:pos + 14], "little")
            flags, idlen = data[pos + 25], data[pos + 32]
            ident = data[pos + 33:pos + 33 + idlen]
            pos += n
            if idlen == 1 and ident[0] in (0, 1):
                continue
            name = ident.decode("latin1").split(";")[0].rstrip(".")
            if flags & 2:
                tree(ext, size, f"{prefix}{name}/")
                continue
            disc_checked += 1
            if name.upper().endswith((".SYS", ".COM")):
                names.append(f"CD:{prefix}{name}")
            body = sectors(ext, (size + 2047) // 2048)[:size]
            strings.extend((f"CD:{prefix}{name}", s.decode()) for s in needles if s in body)

    pvd = sectors(16, 1)
    tree(int.from_bytes(pvd[158:162], "little"), int.from_bytes(pvd[166:170], "little"), "")

print(f"install files {checked}, disc files {disc_checked}; .SYS/.COM names: {names or 'none'}")
for rel, s in sorted(strings):
    print(f"    {rel} contains {s}")

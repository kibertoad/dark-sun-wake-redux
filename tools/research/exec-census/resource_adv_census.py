"""Static census of the ADV resources 10 and 20 of the installed RESOURCE.GFF (Q-EXE-028).

Usage: python -I resource_adv_census.py INSTALL_DIR PATH_TO_BLD-GOG-EN-1.1.files.yaml

Reads RESOURCE.GFF (size and XXH3-128 checked against the manifest) through its directory, as
installed_adv_resources.py does, and decodes each resource from its own entry points:
- a Copy driver (Copy at +2) runs at the offsets of its file: from every offset in its function
  table (the word at file offset 0 points to number/offset pairs ending at number FFFF), as
  disc_adv_flow_scan.py does for the disc drivers;
- a DIGPAK image (DIGPAK at +3) is laid out like a .COM program, its first byte at offset 0100, so
  file offset f runs at offset f + 0100: from offsets 0200 and 0203, the image bytes +0x100 and
  +0x103 that DSUN.EXE calls (4842:13FB, 4842:14EA; FND-EXE-585).
To these it adds every handler the decoded code hands out as a far pointer (`mov ax,imm; push cs;
push ax`, or a store of an immediate at X beside a store of CS at X+2, through CS, DS or ES), and
every entry of a bounded near jump table (`cmp ax,LO; jb; cmp ax,HI; ja` before `sub ax,LO; shl
ax,1; add ax,T`), and follows direct jumps, calls and fall-through, stopping at ret, retf, iret and
jmp. It prints the decoded byte count, every interrupt instruction with the nearest preceding
immediate AX or AH writer, every far transfer and near transfer through memory with the decoded
writers of the slot a far transfer reads, every decoded store through DS or ES right after it is
loaded with 0 (the interrupt vector table), every ret or retf right after a push, and the class of
every CD byte: a decoded interrupt instruction, inside a decoded instruction, or not decoded. For a
DIGPAK image it also decodes from the .COM program entry at 0100, which DSUN.EXE never calls, and
lists the interrupts only that path reaches. A far pointer in DX:BX with DX = CS passed to int 66
AX=068E (DIGPAK's callback registration) is added as a start too.
Nothing is written or executed.
"""
import os
import re
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

root, manifest = sys.argv[1], sys.argv[2]
md = Cs(CS_ARCH_X86, CS_MODE_16)

text = open(manifest, encoding="utf-8").read()
m = re.search(r"- path: RESOURCE\.GFF\n\s+format: [^\n]+\n\s+size: (\d+)\n\s+xxh3: ([0-9a-f]{32})", text)
gff = open(os.path.join(root, "RESOURCE.GFF"), "rb").read()
assert (len(gff), xxhash.xxh3_128_hexdigest(gff)) == (int(m.group(1)), m.group(2)), "RESOURCE.GFF"

at = struct.unpack_from("<I", gff, 0x0C)[0]
p, tables = at + 10, {}
for _ in range(struct.unpack_from("<H", gff, at + 8)[0]):
    name, word = gff[p:p + 4], struct.unpack_from("<I", gff, p + 4)[0]
    entries, indexed = word & 0x7FFFFFFF, word >> 31
    if indexed:
        index, ranges = struct.unpack_from("<II", gff, p + 12)
        pairs = [struct.unpack_from("<II", gff, p + 20 + 8 * k) for k in range(ranges)]
        tables[name] = ("indexed", index, [n for first, run in pairs for n in range(first, first + run)])
        p += 20 + 8 * ranges
    else:
        tables[name] = ("plain", [struct.unpack_from("<III", gff, p + 8 + 12 * k) for k in range(entries)])
        p += 8 + 12 * entries
_, index, numbers = tables[b"ADV "]
gffi = {n: (o, z) for n, o, z in tables[b"GFFI"][1]}
o, _ = gffi[index]
resources = {n: gff.__getitem__(slice(*(lambda a, s: (a, a + s))(*struct.unpack_from("<II", gff, o + 4 + 8 * k))))
             for k, n in enumerate(numbers)}


def decode(body, base, entries):
    seen = {}
    work = list(entries)
    while work:
        p = work.pop()
        while p not in seen and 0 <= p - base < len(body):
            i = next(md.disasm(body[p - base:p - base + 16], p), None)
            if i is None:
                break
            seen[p] = i
            if i.mnemonic in ("ret", "retf", "iret", "hlt", "ljmp"):
                break
            if i.mnemonic.startswith("j") or i.mnemonic in ("call", "loop", "loope", "loopne"):
                if i.op_str.startswith("0x"):
                    work.append(int(i.op_str, 16))
                if i.mnemonic == "jmp":
                    break
            p += i.size
    return seen


def seeds(body, base, seen):
    items = sorted(seen.items())
    out, why = set(), {}
    for k, (p, i) in enumerate(items):
        nxt = [(x.mnemonic, x.op_str) for _, x in items[k + 1:k + 3]]
        m = re.fullmatch(r"ax, (0x[0-9a-f]+)", i.op_str)
        if i.mnemonic == "mov" and m and nxt == [("push", "cs"), ("push", "ax")]:
            out.add(int(m.group(1), 16))
            why[int(m.group(1), 16)] = f"pushed as a far pointer at {p:04X}"
        m = re.fullmatch(r"word ptr ((?:cs:|es:)?)\[(0x[0-9a-f]+)\], (0x[0-9a-f]+)", i.op_str)
        if i.mnemonic == "mov" and m:
            pair = f"word ptr {m.group(1)}[{hex(int(m.group(2), 16) + 2)}], cs"
            if any(x.mnemonic == "mov" and x.op_str == pair for _, x in items[max(0, k - 2):k + 3]):
                out.add(int(m.group(3), 16))
                why[int(m.group(3), 16)] = f"stored with CS at {m.group(1)}[{m.group(2)}] by {p:04X}"
        if i.mnemonic == "int" and i.op_str == "0x66" and selector(items, k) == "ax, 0x68e":
            # DIGPAK function 068E takes a far pointer in DX:BX; a pointer into the image has DX = CS.
            window = [(x.mnemonic, x.op_str) for _, x in items[max(0, k - 6):k]]
            bx = next((int(o.split(", ")[1], 16) for mn, o in reversed(window) if mn == "mov" and re.fullmatch(r"bx, 0x[0-9a-f]+", o)), None)
            if bx is not None and (("mov", "dx, cs") in window or (("mov", "ax, cs") in window and ("mov", "dx, ax") in window)):
                out.add(bx)
                why[bx] = f"passed in DX:BX with DX = CS to int 66 AX=068E at {p:04X}"
        if i.mnemonic == "jmp" and re.fullmatch(r"word ptr cs:\[0x[0-9a-f]+\]", i.op_str):
            window = [(x.mnemonic, x.op_str) for _, x in items[max(0, k - 12):k]]
            lo = next((int(o.split(", ")[1], 16) for mn, o in window if mn == "cmp" and o.startswith("ax, 0x")), None)
            his = [int(o.split(", ")[1], 16) for mn, o in window if mn == "cmp" and o.startswith("ax, 0x")]
            table = next((int(o.split(", ")[1], 16) for mn, o in window if mn == "add" and o.startswith("ax, 0x")), None)
            if lo is not None and len(his) == 2 and table is not None and ("sub", f"ax, {hex(lo)}") in window:
                count = his[1] - lo + 1
                rows = struct.unpack_from(f"<{count}H", body, table - base)
                out |= set(rows)
                for r in rows:
                    why.setdefault(r, f"row of the table at {table:04X} ({count} rows, AX {lo:04X}..{his[1]:04X}) used at {p:04X}")
    return out, why


def selector(items, k):
    for _, x in reversed(items[max(0, k - 12):k]):
        if x.mnemonic == "mov" and re.fullmatch(r"a[xh], (0x[0-9a-f]+|[0-9])", x.op_str):
            return x.op_str
    return "no immediate AX/AH writer in the 12 preceding instructions"


for number in (10, 20):
    body = resources[number]
    digpak = body[3:9] == b"DIGPAK"
    if digpak:
        base, entries = 0x100, [0x200, 0x203]
    else:
        assert body[2:6] == b"Copy"
        base, entries, off = 0, [], struct.unpack_from("<H", body, 0)[0]
        while struct.unpack_from("<H", body, off)[0] != 0xFFFF:
            entries.append(struct.unpack_from("<H", body, off + 2)[0])
            off += 4
    found, why = set(), {}
    while True:
        seen = decode(body, base, entries)
        more, reasons = seeds(body, base, seen)
        new = {s for s in more - set(entries) if 0 <= s - base < len(body)}
        if not new:
            break
        found |= new
        why.update({s: reasons[s] for s in new})
        entries += sorted(new)
    items = sorted(seen.items())
    print(f"ADV {number}: {len(body)} bytes, {'DIGPAK image, run at file offset + 0100' if digpak else 'Copy driver'}, "
          f"{len(entries) - len(found)} entry points, decoded {sum(i.size for i in seen.values())} bytes")
    for s in sorted(found):
        print(f"    added start {s:04X}: {why[s]}")
    slots = set()
    for k, (p, i) in enumerate(items):
        mn, ops = i.mnemonic, i.op_str
        if mn == "int":
            print(f"    {p:04X} int {ops}: {selector(items, k)}")
        elif mn in ("lcall", "ljmp") or (mn in ("call", "jmp") and "[" in ops):
            print(f"    {p:04X} {mn} {ops}")
            slot = re.search(r"\[(0x[0-9a-f]+)\]$", ops)
            if mn in ("lcall", "ljmp") and slot:
                slots.add(int(slot.group(1), 16))
        elif mn == "mov" and ops in ("es, ax", "ds, ax") and any(
                (x.mnemonic, x.op_str) in (("xor", "ax, ax"), ("mov", "ax, 0")) for _, x in items[max(0, k - 2):k]):
            # A segment register loaded with 0 addresses the interrupt vector table: list the decoded
            # stores through it until the register is loaded again or the path leaves.
            seg = ops[:2]
            stores = []
            for q, x in items[k + 1:k + 24]:
                if x.mnemonic in ("ret", "retf", "iret", "jmp") or x.op_str.startswith(f"{seg},") or \
                        (x.mnemonic in ("les", "lds") and x.mnemonic[1:] == seg):
                    break
                dest = x.op_str.split(", ")[0]
                through = f"{seg}:[" in dest or (seg == "ds" and "[" in dest and ":[" not in dest)
                if (x.mnemonic == "mov" and "," in x.op_str and "[" in dest and through) or \
                        (seg == "es" and x.mnemonic.startswith("stos")):
                    stores.append(f"{q:04X} {x.mnemonic} {x.op_str}")
            if stores:
                print(f"    {p:04X} {seg.upper()} = 0, then stores: {'; '.join(stores)}")
    for s in sorted(slots):
        writers = [f"{p:04X} {i.mnemonic} {i.op_str}" for p, i in items
                   if i.mnemonic in ("mov", "les", "lds", "pop", "xchg", "add", "sub", "or", "and", "inc", "dec")
                   and re.match(r"(word ptr |dword ptr )?(cs:|es:)?\[(%s|%s)\]," % (hex(s), hex(s + 2)), i.op_str)]
        print(f"    slot [{s:04X}] writers: {'; '.join(writers) or 'none'}; file bytes "
              f"{body[s - base:s - base + 4].hex(' ')}")
    pushed = [f"{p:04X}" for k, (p, i) in enumerate(items)
              if i.mnemonic in ("ret", "retf") and k and items[k - 1][1].mnemonic in ("push", "pushf")]
    print(f"    returns right after a push: {pushed or 'none'}")
    inside = {a for p, i in items for a in range(p, p + i.size)}
    program = {}
    if digpak:
        # The image's first bytes are also a .COM program's entry at 0100; DSUN.EXE never calls it.
        program = decode(body, base, [0x100])
        extra = sorted(set(program) - set(seen))
        print(f"    decoded only from the program entry 0100: {sum(program[a].size for a in extra)} bytes")
        for k, (p, i) in enumerate(sorted(program.items())):
            if p in extra and i.mnemonic == "int":
                print(f"        {p:04X} int {i.op_str}: {selector(sorted(program.items()), k)}")
    kinds = {"decoded interrupt": [], "inside a decoded instruction": [], "an interrupt decoded only from the program entry": [],
             "not decoded": []}
    for f in (m.start() for m in re.finditer(b"\xCD", body)):
        a = f + base
        if a in seen and seen[a].mnemonic == "int":
            kinds["decoded interrupt"].append(a)
        elif a in inside:
            kinds["inside a decoded instruction"].append(a)
        elif a in program and program[a].mnemonic == "int":
            kinds["an interrupt decoded only from the program entry"].append(a)
        else:
            kinds["not decoded"].append(a)
    print("    CD bytes: " + "; ".join(f"{k} {len(v)}" + (f" ({' '.join(f'{a:04X}' for a in v)})" if k == "not decoded" and v else "")
                                   for k, v in kinds.items()))

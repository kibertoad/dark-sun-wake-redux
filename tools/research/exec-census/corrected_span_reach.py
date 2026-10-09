"""Traces near control flow from the owning entries of FND-EXE-174's spans not covered by FND-EXE-522/523.

Usage: python -I corrected_span_reach.py PATH_TO_DSUN.EXE PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

Same reading as fixup_span_reach.py (manifest checks, overlays from stubs, span holders, walks
from each owning entry over its own code block with offset 0 at its first byte per FND-EXE-520,
and the byte-offset scan of each fixup holder for direct near branches into its own fixups), over
the cases listed below. Nothing is written or executed.
"""
import re
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs
from capstone.x86 import X86_OP_IMM

SECTOR, DATA = 2352, 24
exe, image, manifest = sys.argv[1:4]
expected = {}
text = open(manifest, encoding="utf-8").read()
for m in re.finditer(r"- path: ((?:CD:)?DSUN\.EXE)\n(?:\s+format: [^\n]+\n)?\s+size: (\d+)\n\s+xxh3: ([0-9a-f]{32})", text):
    expected[m.group(1)] = (int(m.group(2)), m.group(3))


def sectors(f, lba, count):
    out = bytearray()
    for i in range(count):
        f.seek((lba + i) * SECTOR + DATA)
        out += f.read(2048)
    return bytes(out)


def disc_file(want):
    with open(image, "rb") as f:
        pvd = sectors(f, 16, 1)
        assert pvd[0] == 1 and pvd[1:6] == b"CD001"
        root_lba = int.from_bytes(pvd[158:162], "little")
        root_len = int.from_bytes(pvd[166:170], "little")
        data = sectors(f, root_lba, (root_len + 2047) // 2048)
        pos = 0
        while pos < root_len:
            n = data[pos]
            if n == 0:
                pos = (pos // 2048 + 1) * 2048
                continue
            lba = int.from_bytes(data[pos + 2:pos + 6], "little")
            size = int.from_bytes(data[pos + 10:pos + 14], "little")
            idlen = data[pos + 32]
            name = data[pos + 33:pos + 33 + idlen].decode("latin1").split(";")[0].rstrip(".")
            pos += n
            if name.upper() == want:
                return sectors(f, lba, (size + 2047) // 2048)[:size]
    raise SystemExit(f"{want} not in disc root")


def overlays(body):
    """Return (code start, code end, fixup end, stub file offset, trampoline targets) sorted by code start."""
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages = struct.unpack_from("<HH", body, 2)
    image_end = (pages - 1) * 512 + (cblp or 512)
    assert body[image_end:image_end + 4] == b"FBOV"
    base = image_end + 16
    rows = []
    for f in range(header, image_end - 0x20, 16):
        if body[f] == 0xCD and body[f + 1] == 0x3F:
            position, size, fixups, count = struct.unpack_from("<IHHH", body, f + 4)
            start = position + base
            if base <= start and start + size + fixups <= len(body):
                targets = [struct.unpack_from("<H", body, f + 0x22 + 5 * k)[0] for k in range(count)]
                rows.append((start, start + size, start + size + fixups, f, targets))
    return sorted(rows)


bodies = {"DSUN.EXE": open(exe, "rb").read(), "CD:DSUN.EXE": disc_file("DSUN.EXE")}
for path, body in bodies.items():
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == expected[path], f"{path} differs from manifest"
    print(f"{path}: {len(body)} bytes, XXH3-128 {expected[path][1]}")

# (file, owning entry, spans) for the pairs of FND-EXE-174's corrected-snapshot table that
# FND-EXE-522 and FND-EXE-523 do not already cover.
CASES = [
    ("DSUN.EXE", 0x000796DB, [(0x00077F40, 0x00077F41)]),
    ("DSUN.EXE", 0x00087859, [(0x0008716E, 0x000871B7)]),
    ("DSUN.EXE", 0x0008AA6A, [(0x000870C4, 0x00087143)]),
    ("DSUN.EXE", 0x00092A83, [(0x000499DC, 0x000499FE)]),
    ("DSUN.EXE", 0x0009439E, [(0x00094E46, 0x00094E49)]),
    ("CD:DSUN.EXE", 0x00069F69, [(0x0006AD00, 0x0006AD70)]),
    ("CD:DSUN.EXE", 0x0006B9C8, [(0x00072D6B, 0x00072E18), (0x00072E18, 0x00072E3B)]),
    ("CD:DSUN.EXE", 0x000796AB, [(0x00077F35, 0x00077F36)]),
    ("CD:DSUN.EXE", 0x00087929, [(0x000870C4, 0x0008710E)]),
]
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
BRANCH = {"jmp", "call", "ja", "jae", "jb", "jbe", "je", "jne", "jg", "jge", "jl", "jle", "jo", "jno",
          "js", "jns", "jp", "jnp", "jcxz", "loop", "loope", "loopne"}


def walk(code, entry):
    seen, decoded, work, notes = {}, {}, [entry], []
    while work:
        off = work.pop()
        while 0 <= off < len(code) and off not in seen:
            i = next(md.disasm(code[off:off + 16], off), None)
            if i is None:
                notes.append(f"undecodable at 0x{off:04X}")
                break
            seen[off] = i.size
            decoded[off] = i
            m = i.mnemonic
            table = re.fullmatch(r"word ptr cs:\[bx \+ (0x[0-9a-f]+|\d+)\]", i.op_str)
            if m == "jmp" and table:
                prior = [decoded[a] for a in sorted(decoded) if a < i.address][-4:]
                shape = [(x.mnemonic, x.op_str) for x in prior]
                bound = None
                if len(shape) >= 3 and shape[-1] == ("shl", "bx, 1") and shape[-2][0] == "ja" and shape[-3][0] == "cmp":
                    bound = re.fullmatch(r"bx, (0x[0-9a-f]+|\d+)", shape[-3][1])
                elif len(shape) == 4 and shape[-1] == ("shl", "bx, 1") and shape[-2][0] == "jmp" \
                        and shape[-3] == ("jbe", hex(i.address - 2)) and shape[-4][0] == "cmp":
                    bound = re.fullmatch(r"bx, (0x[0-9a-f]+|\d+)", shape[-4][1])
                # Key scan: "mov cx, N; mov bx, K", a loop comparing the word at cs:[bx] that adds 2 to BX
                # and leaves on "je" to this jump, so BX is K + 2k with k below N here.
                scan = [(x.mnemonic, x.op_str) for x in [decoded[a] for a in sorted(decoded) if a < i.address][-8:]]
                keys = None
                if len(scan) == 8 and scan[0][0] == "mov" and scan[0][1].startswith("cx, ") \
                        and scan[1][0] == "mov" and scan[1][1].startswith("bx, ") and scan[2] == ("mov", "ax, word ptr cs:[bx]") \
                        and scan[4] == ("je", hex(i.address)) and scan[5] == ("add", "bx, 2") and scan[6][0] == "loop":
                    keys = int(scan[0][1][4:], 0), int(scan[1][1][4:], 0)
                if keys:
                    count, first = keys
                    base = first + int(table.group(1), 0)
                    words = [int.from_bytes(code[base + 2 * k:base + 2 * k + 2], "little") for k in range(count)]
                    out = sorted({w for w in words if w >= len(code)})
                    notes.append(f"0x{i.address:04X} key-scan jmp cs:0x{base:04X}, {count} slots (keys at cs:0x{first:04X})"
                                 + (f"; targets outside code {[hex(w) for w in out]}" if out else ""))
                    work.extend(w for w in words if w < len(code))
                elif bound:
                    slots, base = int(bound.group(1), 0) + 1, int(table.group(1), 0)
                    words = [int.from_bytes(code[base + 2 * k:base + 2 * k + 2], "little") for k in range(slots)]
                    out = sorted({w for w in words if w >= len(code)})
                    notes.append(f"0x{i.address:04X} table jmp cs:0x{base:04X}, {slots} slots"
                                 + (f"; targets outside code {[hex(w) for w in out]}" if out else ""))
                    work.extend(w for w in words if w < len(code))
                else:
                    notes.append(f"0x{i.address:04X} computed jmp {i.op_str} with no bound read: {shape}")
                break
            if m in BRANCH:
                op = i.operands[0] if i.operands else None
                if op is not None and op.type == X86_OP_IMM:
                    target = op.imm & 0xFFFF
                    if target >= len(code):
                        notes.append(f"0x{i.address:04X} {m} to 0x{target:04X}, past code size")
                    else:
                        work.append(target)
                else:
                    notes.append(f"0x{i.address:04X} computed {m} {i.op_str}")
            elif m in ("int", "into", "ljmp", "iret"):
                notes.append(f"0x{i.address:04X} {m} {i.op_str}")
            if m in ("ret", "retf", "iret", "jmp", "ljmp"):
                break
            off += i.size
    return seen, notes


tables = {path: overlays(body) for path, body in bodies.items()}
for path, rows in tables.items():
    print(f"{path}: {len(rows)} overlays from stubs")


def where(path, offset):
    rows = tables[path]
    for k, (start, end, fix_end, stub, _) in enumerate(rows):
        following = rows[k + 1][0] if k + 1 < len(rows) else len(bodies[path])
        if start <= offset < end:
            return k, "code", start
        if end <= offset < fix_end:
            return k, "fixups", start
        if fix_end <= offset < following:
            return k, "padding" if not any(bodies[path][fix_end:following]) else "gap", start
    return None, "outside every overlay", None


for path, entry, spans in CASES:
    k, kind, start = where(path, entry)
    assert kind == "code"
    _, end, fix_end, stub, targets = tables[path][k]
    code = bodies[path][start:end]
    seen, notes = walk(code, entry - start)
    print(f"== {path} entry 0x{entry:08X}: overlay #{k} (stub file 0x{stub:05X}), code 0x{start:08X}..0x{end:08X}, "
          f"fixups ..0x{fix_end:08X}, entry offset 0x{entry - start:04X} "
          f"{'is' if entry - start in targets else 'is NOT'} a trampoline target")
    print(f"  reached {len(seen)} instructions, {sum(seen.values())} bytes")
    for lo, hi in spans:
        sk, skind, sstart = where(path, lo)
        hk, hkind, _ = where(path, hi - 1)
        hit = [o for o, s in seen.items() if start + o < hi and start + o + s > lo]
        if sk is None:
            print(f"  span 0x{lo:08X}..0x{hi:08X}: {skind} (load image or headers); "
                  f"{len(hit)} reached instructions overlap it; nonzero bytes "
                  f"{sum(1 for x in bodies[path][lo:hi] if x)} of {hi - lo}")
            continue
        holder = tables[path][sk]
        highest = max(holder[4]) if holder[4] else None
        print(f"  span 0x{lo:08X}..0x{hi:08X}: overlay #{sk} {skind} (to #{hk} {hkind}); "
              f"{'same overlay' if sk == k else 'other overlay'}; {len(hit)} reached instructions overlap it; "
              f"holder's {len(holder[4])} trampoline targets "
              f"{'all below' if highest is not None and highest < holder[1] - holder[0] else 'NOT all below'} "
              f"its code size {holder[1] - holder[0]}")
    for n in notes:
        print("  " + n)

# Byte-offset scan of each overlay holding a fixup span: every offset in its code whose byte is a
# direct near branch (E8, E9 rel16; EB, 70..7F, E0..E3 rel8) with a target inside its own fixup table.
for path in bodies:
    holders = sorted({where(path, lo)[0] for p, _, spans in CASES if p == path for lo, _ in spans
                      if where(path, lo)[1] == "fixups"})
    for k in holders:
        start, end, fix_end, stub, _ = tables[path][k]
        code, size = bodies[path][start:end], end - start
        hits = []
        for o in range(size):
            op, t = code[o], None
            if op in (0xE8, 0xE9) and o + 3 <= size:
                t = (o + 3 + struct.unpack_from("<h", code, o + 1)[0]) & 0xFFFF
            elif (op == 0xEB or 0x70 <= op <= 0x7F or 0xE0 <= op <= 0xE3) and o + 2 <= size:
                t = (o + 2 + struct.unpack_from("<b", code, o + 1)[0]) & 0xFFFF
            if t is not None and size <= t < fix_end - start:
                hits.append(f"0x{o:04X} byte {op:02X} -> 0x{t:04X}")
        print(f"{path} overlay #{k} code 0x{start:08X}..0x{end:08X} fixups ..0x{fix_end:08X}: "
              f"{len(hits)} byte offsets branching into its fixups {hits}")

# Holders not walked by FND-EXE-524: walk each from all its trampoline targets, print every computed
# jump with its table, and say whether a byte-offset candidate above is a reached instruction start.
NEW_HOLDERS = {"DSUN.EXE": [(0x000749B0, [0x3548]), (0x00083400, [])],
               "CD:DSUN.EXE": [(0x00068790, []), (0x0006FE20, []), (0x000749B0, [])]}
for path, entries in NEW_HOLDERS.items():
    rows = {row[0]: row for row in overlays(bodies[path])}
    for start, candidates in entries:
        _, end, fix_end, stub, targets = rows[start]
        code = bodies[path][start:end]
        seen, notes = {}, []
        for target in targets:
            s, n = walk(code, target)
            seen.update(s)
            notes += [x for x in n if x not in notes]
        print(f"== holder {path} code 0x{start:08X}..0x{end:08X} (stub 0x{stub:05X}), {len(targets)} trampoline "
              f"targets: reached {len(seen)} instructions, {sum(seen.values())} of {end - start} bytes")
        for c in candidates:
            inside = [o for o, n in seen.items() if o < c < o + n]
            print(f"  candidate 0x{c:04X}: {'a reached instruction start' if c in seen else 'not a reached instruction start'}"
                  + (f", inside the reached instruction at 0x{inside[0]:04X}" if inside else ""))
        for x in notes:
            print("  " + x)

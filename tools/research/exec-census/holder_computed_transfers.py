"""Walks every trampoline entry of the overlays that hold FND-EXE-173's fixup spans (Q-EXE-023).

Usage: python -I holder_computed_transfers.py PATH_TO_DSUN.EXE PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

Reads both DSUN.EXE copies as fixup_span_reach.py does, takes the overlays whose code starts at the
file offsets below (descriptors 180, 194, 203 and 209 installed; 188, 194, 197, 199, 210 and 212
disc, per the build's Code ranges), and walks near control flow from all of each overlay's
trampoline targets with offset 0 at its code block's first byte (FND-EXE-520). The walk is the one in
fixup_span_reach.py. It prints each computed transfer, each table's slots and any target at or past
the code size. Nothing is written or executed.
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

HOLDERS = {"DSUN.EXE": [0x000671E0, 0x00081130, 0x0008B240, 0x00093160],
           "CD:DSUN.EXE": [0x00072E80, 0x000810A0, 0x000835A0, 0x000879D0, 0x000950B0, 0x00098320]}
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


for path, starts in HOLDERS.items():
    rows = {row[0]: row for row in overlays(bodies[path])}
    for start in starts:
        _, end, fix_end, stub, targets = rows[start]
        code = bodies[path][start:end]
        seen, notes = {}, []
        for target in targets:
            s, n = walk(code, target)
            seen.update(s)
            notes += [x for x in n if x not in notes]
        print(f"== {path} code 0x{start:08X}..0x{end:08X} (stub 0x{stub:05X}), {len(targets)} trampoline targets: "
              f"reached {len(seen)} instructions, {sum(seen.values())} of {end - start} bytes")
        for x in notes:
            print("  " + x)

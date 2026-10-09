"""Traces near control flow from the four Q-EXE-021 owning entries in DSUN.EXE builds.

Usage: python -I anomalous_span_reach.py PATH_TO_DSUN.EXE PATH_TO_game.gog PATH_TO_BLD-GOG-EN-1.1.files.yaml

Reads the installed DSUN.EXE and CD:DSUN.EXE from the root directory of the disc image, checks
each against the build manifest's size and XXH3-128, and for each owning entry walks near control
flow recursively with offset 0 at the first byte of the entry's descriptor code block (FND-EXE-520).
Near jumps, conditional jumps and near calls are followed; far calls, interrupts and returns are
reported and not followed; a near jump or call through a register or memory is reported as
computed and not followed. Targets outside the block's code are reported. Nothing is written or
executed.
"""
import re
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


bodies = {"DSUN.EXE": open(exe, "rb").read(), "CD:DSUN.EXE": disc_file("DSUN.EXE")}
for path, body in bodies.items():
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == expected[path], f"{path} differs from manifest"
    print(f"{path}: {len(body)} bytes, XXH3-128 {expected[path][1]}")

# (file, descriptor, code block start, code block end, entry, span start, span end); blocks from
# the build's Code ranges (FND-EXE-003), spans from FND-EXE-173.
CASES = [
    ("DSUN.EXE", 183, 0x0006AFE0, 0x0006CE93, 0x0006B581, 0x0006D090, 0x0006D150),
    ("CD:DSUN.EXE", 211, 0x00095F30, 0x00098020, 0x0009674B, 0x00095F30, 0x000961E5),
    ("CD:DSUN.EXE", 211, 0x00095F30, 0x00098020, 0x00097BED, 0x00099380, 0x000995C3),
    ("CD:DSUN.EXE", 173, 0x0005A840, 0x0005E9AA, 0x0005E2ED, 0x00055519, 0x0005553D),
]
md = Cs(CS_ARCH_X86, CS_MODE_16)
md.detail = True
BRANCH = {"jmp", "call", "ja", "jae", "jb", "jbe", "je", "jne", "jg", "jge", "jl", "jle", "jo", "jno",
          "js", "jns", "jp", "jnp", "jcxz", "loop", "loope", "loopne"}
STOP = {"ret", "retf", "iret", "jmp"}
for path, descriptor, start, end, entry, span_start, span_end in CASES:
    code = bodies[path][start:end]
    seen, decoded, work, notes = {}, {}, [entry - start], []
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
            table = re.fullmatch(r"word ptr cs:\[bx \+ (0x[0-9a-f]+)\]", i.op_str)
            if m == "jmp" and table:
                # Bounded table: "cmp bx, N", then "ja" past the table or "jbe" over a "jmp" past it,
                # then "shl bx, 1" right before the jump, all among the instructions decoded before it.
                prior = [decoded[a] for a in sorted(decoded) if a < i.address][-4:]
                shape = [(x.mnemonic, x.op_str) for x in prior]
                bound = None
                if len(shape) >= 3 and shape[-1] == ("shl", "bx, 1") and shape[-2][0] == "ja" and shape[-3][0] == "cmp":
                    bound = re.fullmatch(r"bx, (0x[0-9a-f]+|\d+)", shape[-3][1])
                elif len(shape) == 4 and shape[-1] == ("shl", "bx, 1") and shape[-2][0] == "jmp" \
                        and shape[-3] == ("jbe", hex(i.address - 2)) and shape[-4][0] == "cmp":
                    bound = re.fullmatch(r"bx, (0x[0-9a-f]+|\d+)", shape[-4][1])
                if bound:
                    slots, base = int(bound.group(1), 0) + 1, int(table.group(1), 16)
                    words = [int.from_bytes(code[base + 2 * k:base + 2 * k + 2], "little") for k in range(slots)]
                    out = sorted({w for w in words if w >= len(code)})
                    notes.append(f"0x{i.address:04X} table jmp cs:0x{base:04X}, {slots} slots (unsigned bound "
                                 f"{slots - 1}), targets {', '.join(f'0x{w:04X}' for w in sorted(set(words)))}"
                                 + (f"; outside code {out}" if out else ""))
                    work.extend(w for w in words if w < len(code))
                else:
                    notes.append(f"0x{i.address:04X} computed jmp {i.op_str} with no bound read: {shape}")
                break
            if m in BRANCH or m.startswith("loop"):
                op = i.operands[0] if i.operands else None
                if op is not None and op.type == X86_OP_IMM and "far" not in i.op_str and ":" not in i.op_str:
                    target = op.imm & 0xFFFF
                    if target >= len(code):
                        notes.append(f"0x{i.address:04X} {m} to 0x{target:04X}, outside code")
                    else:
                        work.append(target)
                elif m == "lcall" or ":" in i.op_str and op.type == X86_OP_IMM:
                    notes.append(f"0x{i.address:04X} far {m} {i.op_str}")
                else:
                    notes.append(f"0x{i.address:04X} computed {m} {i.op_str}")
            elif m in ("lcall", "ljmp", "int", "into"):
                notes.append(f"0x{i.address:04X} {m} {i.op_str}")
            if m in STOP or m in ("ljmp", "retf", "iret"):
                break
            off += i.size
    low, high = min(seen), max(o + s for o, s in seen.items())
    print(f"== {path} descriptor {descriptor}, block 0x{start:08X}..0x{end:08X}, entry 0x{entry:08X} "
          f"(block offset 0x{entry - start:04X})")
    print(f"  reached {len(seen)} instructions, {sum(seen.values())} bytes, block offsets "
          f"0x{low:04X}..0x{high:04X} (file 0x{start + low:08X}..0x{start + high:08X})")
    inside = span_start >= start and span_end <= end
    hit = [o for o in seen if span_start <= start + o < span_end]
    print(f"  span 0x{span_start:08X}..0x{span_end:08X}: {'inside' if inside else 'outside'} the block's code; "
          f"{len(hit)} reached instructions in it")
    for n in notes:
        print("  " + n)

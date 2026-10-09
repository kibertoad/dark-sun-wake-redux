"""Compares the installed and disc DSUN.EXE overlay managers (FND-EXE-565).

Usage: python -I overlay_manager_editions.py INSTALLED_DSUN.EXE DISC_DSUN.EXE

Checks both files' sizes and XXH3-128 values and applies each one's MZ relocations for a load
image at segment 0x1000. The installed manager is 4AE5:0000..4AE5:1293; the disc's is the same
length starting 0xF1 bytes earlier in the file. Prints both segments' relocation offsets, every
differing relocated word with both values, and, for each other differing byte, the installed and
disc instructions that hold it in a linear 16-bit disassembly. Then prints, from each manager's
data segment (CS:0005 installed, CS:0004 disc), the INT 3Fh handler pointer at +0x0002, the
words at +0x0080 to +0x0088 and +0x0110, and the first segment-table record at +0x01A0 with the
record count up to +0x08C8 and their flags. Nothing is written or executed.
"""
import struct
import sys
from collections import Counter

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

EDITIONS = [(634416, "e296af55ba2ecde7e77f555c90f33d0b"), (634704, "318cd5ec0559901add3780097162a919")]
LENGTH, SHIFT = 0x1293, 0xF1


def load(path, identity):
    body = open(path, "rb").read()
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == identity, f"unexpected file {path}"
    header = struct.unpack_from("<H", body, 8)[0] * 16
    cblp, pages, count = struct.unpack_from("<HHH", body, 2)
    reloc_at = struct.unpack_from("<H", body, 0x18)[0]
    image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
    relocated = set()
    for k in range(count):
        off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
        relocated.add(seg * 16 + off)
        struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)
    return body, header, image, relocated


installed, disc = (load(p, e) for p, e in zip(sys.argv[1:3], EDITIONS))
starts = [0x5200 + 0x3AE5 * 16, 0x5200 + 0x3AE5 * 16 - SHIFT]
segs = []
for (body, header, image, relocated), start in zip((installed, disc), starts):
    linear = start - header
    print(f"file {start:#x}: segment {0x1000 + (linear + 15) // 16:04X}, first byte at offset {(16 - linear % 16) % 16:#x} below it")
    segs.append((bytes(body[start:start + LENGTH]), image, linear, {p - linear for p in relocated if 0 <= p - linear < LENGTH}))
(a, ia, la, ra), (b, ib, lb, rb) = segs
print("relocation offsets equal:", ra == rb, len(ra))
md = Cs(CS_ARCH_X86, CS_MODE_16)
words = sorted(r for r in ra if a[r:r + 2] != b[r:r + 2])
for r in words:
    print(f"relocated word +{r:04X}: installed {struct.unpack_from('<H', ia, la + r)[0]:04X} disc {struct.unpack_from('<H', ib, lb + r)[0]:04X}")
ins_a = list(md.disasm(a, 0))
seen = set()
for o in range(LENGTH):
    if a[o] == b[o] or any(o - r in (0, 1) for r in ra):
        continue
    i = next((x for x in ins_a if x.address <= o < x.address + x.size), None)
    if i is None or i.address in seen:
        continue
    seen.add(i.address)
    j = next(md.disasm(b[i.address:i.address + 8], i.address), None)
    print(f"+{i.address:04X} installed: {i.mnemonic} {i.op_str} | disc: {j.mnemonic + ' ' + j.op_str if j else '?'}")
# The data segment word is 5 bytes into the compared bytes in both files: CS:0005 installed, CS:0004 disc.
for name, (body, header, image, relocated), start in (("installed", installed, starts[0]), ("disc", disc, starts[1])):
    linear = start - header
    ds = struct.unpack_from("<H", image, linear + 5)[0]
    w = lambda off: struct.unpack_from("<H", image, (ds - 0x1000) * 16 + off)[0]
    flags = Counter(w(off + 4) for off in range(0x1A0, 0x8C8, 8))
    print(f"{name}: DS {ds:04X}, handler {w(4):04X}:{w(2):04X}, +0080 {w(0x80):04X} +0082 {w(0x82):04X}"
          f" +0084 {w(0x84):04X} +0086 {w(0x88):04X}:{w(0x86):04X}, +0110 {w(0x110):04X},"
          f" table at {ds + 0x1A:04X}:0000, {(0x8C8 - 0x1A0) // 8} records, flags {dict(sorted(flags.items()))}")

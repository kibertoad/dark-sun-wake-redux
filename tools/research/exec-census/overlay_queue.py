"""Reads DSUN.EXE's overlay buffer, its queue of loaded overlays and header words 0x1C and 0x1E
(FND-EXE-566).

Usage: python -I overlay_queue.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128 and applies its MZ relocations for a load
image at segment 0x1000. Disassembles the manager ranges FND-EXE-566 cites: the buffer setup
(4AE5:00F6..4AE5:0126), the preload (4AE5:031B..4AE5:03DB), the allocation loop
(4AE5:055A..4AE5:05A4), the probation walk (4AE5:05E9..4AE5:061A), the wrap-around
(4AE5:0637..4AE5:0672), the move and the append (4AE5:06E4..4AE5:0753) and the free-space and
size helpers (4AE5:0785..4AE5:07AD). Then lists every instruction of a linear disassembly of
4AE5:0010..4AE5:1258 whose operand text holds 0x1c or 0x1e, says where the sweep ends, and runs a
byte search over the same range for the operand encodings that can hold displacement 0x1C or
0x1E (a direct offset after A0 to A3, a modrm with a 16-bit direct offset, or a modrm with an
8-bit or 16-bit displacement), printing the instruction that holds each hit. Nothing is written or
executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
RANGES = [(0x00F6, 0x0126), (0x031B, 0x03DB), (0x055A, 0x05A4), (0x05E9, 0x061A), (0x0637, 0x0672),
          (0x06E4, 0x0753), (0x0785, 0x07AD)]

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)

md = Cs(CS_ARCH_X86, CS_MODE_16)
base = (0x4AE5 - 0x1000) * 16
for start, end in RANGES:
    print(f"-- 4AE5:{start:04X}..4AE5:{end:04X}")
    for i in md.disasm(bytes(image[base + start:base + end]), start):
        print(f"4AE5:{i.address:04X}  {i.bytes.hex():<14} {i.mnemonic} {i.op_str}")

code = bytes(image[base + 0x10:base + 0x1258])
sweep = list(md.disasm(code, 0x10))
print(f"-- linear sweep of 4AE5:0010..4AE5:1258 ends at 4AE5:{sweep[-1].address + sweep[-1].size:04X}")
for i in sweep:
    if "0x1c" in i.op_str or "0x1e" in i.op_str:
        print(f"4AE5:{i.address:04X}  {i.mnemonic} {i.op_str}")
m = bytes(image[base:base + 0x1258])
for d in (0x1C, 0x1E):
    print(f"-- encodings that can hold displacement {d:#04x}")
    for p in range(0x10, 0x1255):
        direct = m[p] in (0xA0, 0xA1, 0xA2, 0xA3) and m[p + 1] == d and m[p + 2] == 0
        mr = m[p]
        modrm = ((mr & 0xC7) == 0x06 and m[p + 1] == d and m[p + 2] == 0) or \
                ((mr & 0xC0) == 0x40 and (mr & 7) != 6 and m[p + 1] == d) or \
                ((mr & 0xC0) == 0x80 and m[p + 1] == d and m[p + 2] == 0)
        if direct or modrm:
            i = next(x for x in sweep if x.address <= p < x.address + x.size)
            print(f"byte 4AE5:{p:04X} in 4AE5:{i.address:04X}  {i.mnemonic} {i.op_str}")

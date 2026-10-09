"""Reads how DSUN.EXE's overlay manager finds its file and sizes its buffer (FND-EXE-568).

Usage: python -I overlay_file_buffer.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128 and applies its MZ relocations for a load
image at segment 0x1000. Disassembles 4AE5:0010..4AE5:0060 (the start of the startup routine),
4AE5:01B5..4AE5:028B (the file-name routines) and 4AE5:0D27..4AE5:0D82 (the buffer setup), and
prints the 13 bytes at 55CE:08C8, the 5 at 4AE5:0009 and the words at 57E0:3570, 55CE:0114,
55CE:0116 and 55CE:011A. It lists every pair of bytes 70 35 in the load image with the
instruction that holds it and every such pair in the FBOV pack, and computes the largest
overlay's paragraphs as the descriptor walk does (FND-EXE-561) and the buffer that 4AE5:0D27
asks for. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
RANGES = [(0x0010, 0x0060), (0x01B5, 0x028B), (0x0D27, 0x0D82)]
TABLE, PACK = 0x4B080, 0x57570

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)
at = lambda seg, off: (seg - 0x1000) * 16 + off
word = lambda seg, off: struct.unpack_from("<H", image, at(seg, off))[0]

md = Cs(CS_ARCH_X86, CS_MODE_16)
for start, end in RANGES:
    print(f"-- 4AE5:{start:04X}..4AE5:{end:04X}")
    for i in md.disasm(bytes(image[at(0x4AE5, start):at(0x4AE5, end)]), start):
        print(f"4AE5:{i.address:04X}  {i.bytes.hex():<14} {i.mnemonic} {i.op_str}")
print("55CE:08C8", bytes(image[at(0x55CE, 0x8C8):at(0x55CE, 0x8D5)]))
print("4AE5:0009", bytes(image[at(0x4AE5, 9):at(0x4AE5, 14)]))
print(f"57E0:3570 {word(0x57E0, 0x3570):04X}  55CE:0114 {word(0x55CE, 0x114):04X}"
      f"  55CE:0116 {word(0x55CE, 0x116):04X}  55CE:011A {word(0x55CE, 0x11A):04X}")
for p in range(len(image) - 1):
    if image[p] == 0x70 and image[p + 1] == 0x35:
        holder = None
        for back in range(1, 5):
            i = next(md.disasm(bytes(image[p - back:p + 8]), 0), None)
            if i is not None and back < i.size <= back + 4 and "0x3570" in i.op_str:
                holder = f"{i.mnemonic} {i.op_str}"
                break
        print(f"load image {0x1000 + p // 16:04X}:{p % 16:04X}: {holder or 'not an operand'}")
print("pack:", [hex(p) for p in range(PACK, len(body) - 1) if body[p] == 0x70 and body[p + 1] == 0x35])
largest = 0
for i in range(229):
    seg, unk_02, flags, _ = struct.unpack_from("<HHHH", body, TABLE + 8 * i)
    if flags & 2 and unk_02:
        code_size, fixup_size = struct.unpack_from("<HH", body, header + seg * 16 + 8)
        largest = max(largest, ((code_size + 0x11) >> 4) + ((fixup_size + 0x0F) >> 4))
kept = largest + 2
asked = (2 * kept if kept >= word(0x57E0, 0x3570) else word(0x57E0, 0x3570)) + 1
print(f"largest overlay {largest} paragraphs, [0x11A] {kept}, buffer asked {asked} paragraphs ({asked * 16} bytes)")

"""Reads which FBOV fields DSUN.EXE's overlay manager uses (FND-EXE-560 to FND-EXE-562).

Usage: python -I overlay_manager_fields.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128 and applies its MZ relocations for a load
image at segment 0x1000. Prints the manager data words the findings name (55CE:0002, 55CE:0080 to
55CE:0088, 55CE:0110), counts the segment-table descriptors at 55CE:01A0 by flags value, by
flags bit 1 and by a nonzero unk_02, and disassembles the manager ranges the findings cite:
startup (4AE5:0010..4AE5:0140), the vector swap (4AE5:0140..4AE5:0193), the header read
(4AE5:028B..4AE5:029B), the descriptor walk (4AE5:029B..4AE5:031B), the INT 3Fh handler and the
entry path (4AE5:04F4..4AE5:061F), unloading (4AE5:061F..4AE5:0637), the trampoline writer and
restorer (4AE5:0672..4AE5:06E4) and the frame walk (4AE5:0753..4AE5:0785). It also lists every
near call in 4AE5:0000..4AE5:1293 to the header read and the descriptor walk. Nothing is written
or executed.
"""
import struct
import sys
from collections import Counter

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
RANGES = [(0x0010, 0x0140), (0x0140, 0x0193), (0x028B, 0x029B), (0x029B, 0x031B), (0x04F4, 0x061F),
          (0x061F, 0x0637), (0x0672, 0x06E4), (0x0753, 0x0785)]

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages, count = struct.unpack_from("<HHH", body, 2)
reloc_at = struct.unpack_from("<H", body, 0x18)[0]
image = bytearray(body[header:(pages - 1) * 512 + (cblp or 512)])
for k in range(count):
    off, seg = struct.unpack_from("<HH", body, reloc_at + 4 * k)
    struct.pack_into("<H", image, seg * 16 + off, (struct.unpack_from("<H", image, seg * 16 + off)[0] + 0x1000) & 0xFFFF)

def word(seg, off):
    return struct.unpack_from("<H", image, (seg - 0x1000) * 16 + off)[0]

print(f"55CE:0002 handler {word(0x55CE, 4):04X}:{word(0x55CE, 2):04X}")
print(f"55CE:0080 {word(0x55CE, 0x80):04X} 55CE:0082 {word(0x55CE, 0x82):04X} 55CE:0084 {word(0x55CE, 0x84):04X}"
      f" 55CE:0086 far {word(0x55CE, 0x88):04X}:{word(0x55CE, 0x86):04X}")
print(f"55CE:0110 {word(0x55CE, 0x110):04X} (bytes {image[(0x55CE - 0x1000) * 16 + 0x110]:02X}"
      f" {image[(0x55CE - 0x1000) * 16 + 0x111]:02X})")
flags, walked = Counter(), 0
for off in range(0x1A0, 0x8C8, 8):
    seg, unk02, flag, unk06 = (word(0x55CE, off + 2 * i) for i in range(4))
    flags[flag] += 1
    if flag & 2 and unk02:
        walked += 1
        assert image[(seg - 0x1000) * 16 + 0x1A] != 0xFF
print(f"descriptors {(0x8C8 - 0x1A0) // 8}, by flags {dict(sorted(flags.items()))}, kept by the walk {walked}")

md = Cs(CS_ARCH_X86, CS_MODE_16)
base = (0x4AE5 - 0x1000) * 16
for start, end in RANGES:
    print(f"-- 4AE5:{start:04X}..4AE5:{end:04X}")
    for i in md.disasm(bytes(image[base + start:base + end]), start):
        print(f"4AE5:{i.address:04X}  {i.bytes.hex():<14} {i.mnemonic} {i.op_str}")
print("-- near calls to 4AE5:028B and 4AE5:029B in 4AE5:0000..4AE5:1293")
for p in range(0, 0x1293 - 2):
    if image[base + p] == 0xE8:
        target = (p + 3 + struct.unpack_from("<h", image, base + p + 1)[0]) & 0xFFFF
        if target in (0x028B, 0x029B):
            print(f"4AE5:{p:04X} -> 4AE5:{target:04X}")

"""Reads how DSUN.EXE's resident overlay manager places, loads and enters an overlay (FND-EXE-520).

Usage: python -I overlay_entry_cs.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128, prints the FBOV header after the MZ load
image, descriptor 198's stub header at 5768:0000 as shipped and with the FBOV base added, and a
linear 16-bit disassembly of the manager ranges the finding reads: startup
(4AE5:0010..4AE5:00D4), stub relocation (4AE5:029B..4AE5:031B), the code read
(4AE5:03E8..4AE5:0421), the fixup pass
(4AE5:0421..4AE5:0466), the loader (4AE5:04C6..4AE5:04F4), the placement publisher
(4AE5:055A..4AE5:05A4), the entry path (4AE5:05A4..4AE5:061F), the trampoline writer
(4AE5:0672..4AE5:06B1), the compaction move (4AE5:06E4..4AE5:0735) and its frame walk
(4AE5:0753..4AE5:0785). Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
RANGES = [(0x0010, 0x00D4), (0x029B, 0x031B), (0x03E8, 0x0421), (0x0421, 0x0466), (0x04C6, 0x04F4),
          (0x055A, 0x05A4), (0x05A4, 0x061F), (0x0672, 0x06B1), (0x06E4, 0x0735), (0x0753, 0x0785)]

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)


def file_offset(segment, offset):
    return header + (segment - 0x1000) * 16 + offset


print(f"MZ header 0x{header:X} bytes, load image ends at file offset 0x{image_end:X}")
print(f"FBOV header at 0x{image_end:X}: {body[image_end:image_end + 16].hex(' ')}; "
      f"payload starts at 0x{image_end + 16:X}")
stub = file_offset(0x5768, 0)
position, code, fixups, count = struct.unpack_from("<IHHH", body, stub + 4)
print(f"stub 5768:0000 (file 0x{stub:X}): {body[stub:stub + 4].hex(' ')}, +4 0x{position:08X}, "
      f"+8 code {code}, +0A fixups {fixups}, +0C trampolines {count}; +4 plus payload start "
      f"0x{position + image_end + 16:08X}")
md = Cs(CS_ARCH_X86, CS_MODE_16)
for start, end in RANGES:
    print(f"== 4AE5:{start:04X}..4AE5:{end:04X}")
    base = file_offset(0x4AE5, 0)
    for i in md.disasm(body[base + start:base + end], start):
        print(f"  4AE5:{i.address:04X} {i.mnemonic} {i.op_str}")

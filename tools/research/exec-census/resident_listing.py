"""Disassembles ranges of DSUN.EXE's resident load image with its MZ relocations applied.

Usage: python -I resident_listing.py PATH_TO_DSUN.EXE SEGMENT:START..SEGMENT:END [...]

Checks the installed DSUN.EXE's size and XXH3-128, applies the MZ relocations for a load image
at segment 0x1000, and disassembles each half-open range as 16-bit x86, addressed in the
range's segment. Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

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
for item in sys.argv[2:]:
    low, high = item.split("..")
    seg, start = (int(p, 16) for p in low.split(":"))
    end = int(high.split(":")[1], 16)
    base = (seg - 0x1000) * 16
    print(f"-- {item}")
    for ins in md.disasm(bytes(image[base + start:base + end]), start):
        print(f"{seg:04X}:{ins.address:04X}  {ins.bytes.hex():<14} {ins.mnemonic} {ins.op_str}")

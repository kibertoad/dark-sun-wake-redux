"""Reads descriptor 198's dispatch procedure and its four word tables in DSUN.EXE (Q-EXE-020).

Usage: python -I descriptor198_dispatch.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128, then takes descriptor 198's code block at
shipped offset 0x00087310 (1,468 bytes), the file position its stub at 5768:0000 names plus the
FBOV payload start (FND-EXE-520). It prints a linear 16-bit disassembly of block offsets
0x0149..0x022C with offset 0 at the block's first byte, which is where FND-EXE-520 puts CS offset
0, and the words of the tables at CS displacements 0x022C (11 slots), 0x0242 (18), 0x0266 (18)
and 0x028A (20), each with whether the target is an instruction start in that disassembly.
Nothing is written or executed.
"""
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
BLOCK, CODE_SIZE = 0x00087310, 1468
START, END = 0x0149, 0x022C
TABLES = [(0x022C, 11), (0x0242, 18), (0x0266, 18), (0x028A, 20)]

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
code = body[BLOCK:BLOCK + CODE_SIZE]
md = Cs(CS_ARCH_X86, CS_MODE_16)
starts = set()
print(f"== block offsets 0x{START:04X}..0x{END:04X} (file 0x{BLOCK + START:08X}..0x{BLOCK + END:08X})")
for i in md.disasm(code[START:END], START):
    starts.add(i.address)
    print(f"  {i.address:04X} {code[i.address:i.address + i.size].hex():<12} {i.mnemonic} {i.op_str}")
print(f"linear decoding ends at 0x{max(starts):04X}")
for displacement, slots in TABLES:
    words = struct.unpack_from(f"<{slots}H", code, displacement)
    print(f"== table at CS:0x{displacement:04X}, {slots} slots, file 0x{BLOCK + displacement:08X}"
          f"..0x{BLOCK + displacement + 2 * slots:08X}")
    for slot, word in enumerate(words):
        print(f"  slot {slot:2} -> 0x{word:04X} {'instruction start' if word in starts else 'NOT a linear start'}")

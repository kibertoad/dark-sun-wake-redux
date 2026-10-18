"""Prints two DSUN.EXE tables that give the hit routine its DATA number: the psionic lists and the area rows.

Usage: python -I hit_item_tables.py PATH_TO_DSUN.EXE [N]

Checks the installed DSUN.EXE's size and XXH3-128. Reads the load image (segment 0x1000 at the end
of the MZ header):
- the eight far pointers at 5284:0003, which overlay 174 +0604 indexes by byte 0x40 of an object's
  record less 1, and the signed bytes each points at up to the first 0xFF, with each byte plus 235,
  the DATA number overlay 174 +07A8 passes;
- the word at +0 of each 18-byte row from 51F1:0000 up to 51F1:07E0, where the segment's next
  field starts, which overlay 193 +12A8 passes as the DATA number for the row it is given.
With N, it prints whether any of those numbers is N. Nothing is written or executed.
"""
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
wanted = int(sys.argv[2]) if len(sys.argv) > 2 else None


def file_offset(segment, offset):
    return header + (segment - 0x1000) * 16 + offset


numbers = []
for index in range(8):
    offset, segment = struct.unpack_from("<HH", body, file_offset(0x5284, 3 + 4 * index))
    segment += 0x1000
    at = file_offset(segment, offset)
    values = []
    while body[at] != 0xFF and len(values) < 64:
        values.append(struct.unpack_from("<b", body, at)[0])
        at += 1
    numbers += [v + 235 for v in values]
    print("psionic list %d: %04X:%04X (file 0x%08X) %s -> %s" % (
        index, segment, offset, file_offset(segment, offset), values, [v + 235 for v in values]))

rows = []
for row in range(0x7E0 // 18):
    rows.append(struct.unpack_from("<h", body, file_offset(0x51F1, 18 * row))[0])
numbers += rows
print("area rows at 51F1:0000 (file 0x%08X), %d rows, word +0:" % (file_offset(0x51F1, 0), len(rows)))
print(" ".join(str(r) for r in rows))
if wanted is not None:
    print("%d %s" % (wanted, "found" if wanted in numbers else "not found"))

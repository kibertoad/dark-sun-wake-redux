"""Prints the fields of the 18-byte rows at 51F1:0000 that overlay 193 +0617 reads, with the DATA records naming each row.

Usage: python -I effect_rows.py PATH_TO_DSUN.EXE PATH_TO_RESOURCE.GFF

Checks the installed DSUN.EXE's size and XXH3-128. Reads the 112 rows from 51F1:0000 up to
51F1:07E0 in the load image (FND-PARTY-106) and prints for each row k: the signed word at +0, the
low four bits of byte +2, bits 2 and 3 of byte +5 (the sampler mode overlay 179 +0000 takes), bits
4 to 7 of byte +5, the low four bits of byte +6 read as a signed four-bit number, bits 0 and 1 of
byte +0xA, bits 2 to 7 of byte +0xA, the low and high four bits of byte +0xB, and the words at
+0xC, +0xE and +0x10. Then reads every DATA resource of the archive
(FMT-GFF-001 to FMT-GFF-007, as data_bytes.py does) and prints, for each row, the numbers of the
DATA records whose signed byte at +0x17 is k, and the records whose byte is negative other than -1
or 112 or more. Nothing is written or executed.
"""
import struct
import sys
from pathlib import Path

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
base = header + (0x51F1 - 0x1000) * 16

gff = Path(sys.argv[2]).read_bytes()
assert gff[:4] == b"GFFI"
at = struct.unpack_from("<I", gff, 0x0C)[0]
count = struct.unpack_from("<H", gff, at + 8)[0]
p, plain, indexed = at + 10, {}, []
for _ in range(count):
    tag, word = gff[p:p + 4].decode("latin-1"), struct.unpack_from("<I", gff, p + 4)[0]
    entries, is_indexed = word & 0x7FFFFFFF, word >> 31
    if is_indexed:
        index_number, ranges = struct.unpack_from("<II", gff, p + 12)
        pairs = [struct.unpack_from("<II", gff, p + 20 + 8 * k) for k in range(ranges)]
        indexed.append((tag, index_number, [n for first, n_count in pairs for n in range(first, first + n_count)]))
        p += 20 + 8 * ranges
    else:
        plain[tag] = [struct.unpack_from("<III", gff, p + 8 + 12 * k) for k in range(entries)]
        p += 8 + 12 * entries
resources = [(tag, number, offset, size) for tag, rows in plain.items() for number, offset, size in rows]
gffi = {number: (offset, size) for number, offset, size in plain.get("GFFI", [])}
for tag, index_number, numbers in indexed:
    o, _ = gffi[index_number]
    for k, number in enumerate(numbers):
        r_offset, r_size = struct.unpack_from("<II", gff, o + 4 + 8 * k)
        resources.append((tag, number, r_offset, r_size))
data = {number: gff[offset:offset + size] for tag, number, offset, size in resources if tag == "DATA"}

by_row = {}
odd = []
for number in sorted(data):
    record = data[number]
    if len(record) <= 0x17:
        odd.append((number, "short"))
        continue
    row = struct.unpack_from("<b", record, 0x17)[0]
    if 0 <= row < 112:
        by_row.setdefault(row, []).append(number)
    elif row != -1:
        odd.append((number, row))

print("row word0 b2lo mode b5hi b6lo b10lo b10hi b11lo b11hi w0C w0E w10 data_records")
for row in range(112):
    r = body[base + 18 * row:base + 18 * row + 18]
    word0 = struct.unpack_from("<h", r, 0)[0]
    b6 = r[6] & 0x0F
    b6 = b6 - 16 if b6 & 8 else b6
    w0c, w0e, w10 = struct.unpack_from("<HHH", r, 0x0C)
    print(row, word0, r[2] & 0x0F, (r[5] >> 2) & 3, (r[5] >> 4) & 0x0F, b6, r[0x0A] & 3, r[0x0A] >> 2, r[0x0B] & 0x0F, r[0x0B] >> 4, w0c, w0e, w10,
          ",".join(str(n) for n in by_row.get(row, [])) or "-")
print("DATA records with another byte at +0x17:", odd)

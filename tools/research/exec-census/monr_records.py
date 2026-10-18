"""Prints the records of MONR resource 1 of RESOURCE.GFF as overlay 204's request 8 reads them.

Usage: python -I monr_records.py RESOURCE.GFF

Finds MONR resource 1 through the archive's directory (FMT-GFF-001 to FMT-GFF-007) and splits it
into 42-byte records as FMT-ACTOR-003 gives them: a 16-bit key, then ten pairs of 16-bit words.
Prints the resource's size, the record count (the size divided by 42, as the reader computes
it), the bytes left over, and each record's key and pairs. Nothing is written or executed.
"""
import struct
import sys

body = open(sys.argv[1], "rb").read()
assert body[:4] == b"GFFI"
at = struct.unpack_from("<I", body, 0x0C)[0]
count = struct.unpack_from("<H", body, at + 8)[0]
p, tables = at + 10, {}
for _ in range(count):
    name, word = body[p:p + 4], struct.unpack_from("<I", body, p + 4)[0]
    entries, indexed = word & 0x7FFFFFFF, word >> 31
    if indexed:
        index, ranges = struct.unpack_from("<II", body, p + 12)
        pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
        tables[name] = ("indexed", index, [n for first, n_count in pairs for n in range(first, first + n_count)])
        p += 20 + 8 * ranges
    else:
        tables[name] = ("plain", [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)])
        p += 8 + 12 * entries
kind = tables[b"MONR"]
if kind[0] == "plain":
    offset, size = next((o, s) for n, o, s in kind[1] if n == 1)
else:
    gffi_offset = next(o for n, o, s in tables[b"GFFI"][1] if n == kind[1])
    offset, size = struct.unpack_from("<II", body, gffi_offset + 4 + 8 * kind[2].index(1))
data = body[offset:offset + size]
records, rest = divmod(size, 42)
print(f"MONR 1: offset 0x{offset:X}, size {size}, records {records}, left over {rest}")
for r in range(records):
    words = struct.unpack_from("<21h", data, 42 * r)
    pairs = " ".join(f"({words[1 + 2 * k]},{words[2 + 2 * k]})" for k in range(10))
    print(f"{r:2} key {words[0]:5}  {pairs}")

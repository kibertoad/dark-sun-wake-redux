"""Lists one byte of each DATA resource of an installed GFF archive.

Usage: python -I data_bytes.py PATH_TO_GFF OFFSET [VALUE ...]

Reads the archive's directory (FMT-GFF-001 to FMT-GFF-007) as details_chunks.py does and, for each
DATA resource, prints its number, size and the signed byte at OFFSET (hexadecimal). With VALUEs
(decimal), only the resources whose byte is one of them are printed, after a count of all and a
tally of the values seen. Nothing is written or executed.
"""
import struct
import sys
from collections import Counter
from pathlib import Path

body = Path(sys.argv[1]).read_bytes()
offset = int(sys.argv[2], 16)
wanted = {int(v) for v in sys.argv[3:]}
assert body[:4] == b"GFFI"
at = struct.unpack_from("<I", body, 0x0C)[0]
count = struct.unpack_from("<H", body, at + 8)[0]
p, plain, indexed = at + 10, {}, []
for _ in range(count):
    tag, word = body[p:p + 4].decode("latin-1"), struct.unpack_from("<I", body, p + 4)[0]
    entries, is_indexed = word & 0x7FFFFFFF, word >> 31
    if is_indexed:
        index_number, ranges = struct.unpack_from("<II", body, p + 12)
        pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
        indexed.append((tag, index_number, [n for first, n_count in pairs for n in range(first, first + n_count)]))
        p += 20 + 8 * ranges
    else:
        plain[tag] = [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)]
        p += 8 + 12 * entries
resources = [(tag, number, offset_, size) for tag, rows in plain.items() for number, offset_, size in rows]
gffi = {number: (o, size) for number, o, size in plain.get("GFFI", [])}
for tag, index_number, numbers in indexed:
    o, _ = gffi[index_number]
    for i, number in enumerate(numbers):
        r_offset, r_size = struct.unpack_from("<II", body, o + 4 + 8 * i)
        resources.append((tag, number, r_offset, r_size))
data = sorted((number, o, size) for tag, number, o, size in resources if tag == "DATA")
values = Counter()
rows = []
for number, o, size in data:
    value = struct.unpack_from("<b", body, o + offset)[0] if offset < size else None
    values[value] += 1
    rows.append((number, size, value))
print(f"{len(data)} DATA resources; values at 0x{offset:X}: {dict(sorted(values.items(), key=lambda kv: (kv[0] is None, kv[0])))}")
for number, size, value in rows:
    if not wanted or value in wanted:
        print(f"DATA {number} (size {size}): {value}")

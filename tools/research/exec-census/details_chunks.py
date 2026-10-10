"""Lists the type-3 details chunks in an installed GFF archive, with the dword at 0x04 of each.

Usage: python -I details_chunks.py PATH_TO_GFF [MIN]

Reads the archive's directory (FMT-GFF-001 to FMT-GFF-007) into a list of resources with tag,
number, offset and size, then finds every 10-byte chunk header of the form FMT-PARTY-006 gives a
type-3 details chunk: chunk_type 3, any target_chunk, kind 3, unk_03 0, any record_ref, field 15
and len_data 66. For each it prints the resource that holds it, the header's offset within the
resource, and the little-endian dword at 0x04 of the 66 data bytes. With MIN, only those whose
dword is at least MIN (decimal) are printed, after a count of all. A header the byte pattern
matches inside other data is printed too; check that it lies in a chunk chain. Nothing is written
or executed.
"""
import re
import struct
import sys
from pathlib import Path

body = Path(sys.argv[1]).read_bytes()
minimum = int(sys.argv[2]) if len(sys.argv) > 2 else None
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
resources = [(tag, number, offset, size) for tag, rows in plain.items() for number, offset, size in rows]
gffi = {number: (offset, size) for number, offset, size in plain.get("GFFI", [])}
for tag, index_number, numbers in indexed:
    offset, _ = gffi[index_number]
    for i, number in enumerate(numbers):
        r_offset, r_size = struct.unpack_from("<II", body, offset + 4 + 8 * i)
        resources.append((tag, number, r_offset, r_size))
resources.sort(key=lambda r: r[2])
pattern = re.compile(rb"\x03.\x03\x00..\x0f\x00\x42\x00", re.S)
hits = []
for m in pattern.finditer(body):
    holder = next((r for r in resources if r[2] <= m.start() < r[2] + r[3]), None)
    value = struct.unpack_from("<I", body, m.start() + 14)[0]
    hits.append((holder, m.start(), value))
print(f"{len(hits)} details chunk headers")
for holder, offset, value in hits:
    if minimum is not None and value < minimum:
        continue
    where = f"{holder[0]} {holder[1]} +0x{offset - holder[2]:X}" if holder else "outside any resource"
    print(f"{where} (file 0x{offset:08X}): {value}")

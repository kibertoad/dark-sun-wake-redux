"""Lists the type-3 details chunks in an installed GFF archive, with the dword at 0x04 of each.

Usage: python -I details_chunks.py PATH_TO_GFF [MIN | --levels | --byte OFFSET]

Reads the archive's directory (FMT-GFF-001 to FMT-GFF-007) into a list of resources with tag,
number, offset and size, then finds every 10-byte chunk header of the form FMT-PARTY-006 gives a
type-3 details chunk: chunk_type 3, any target_chunk, kind 3, unk_03 0, any record_ref, field 15
and len_data 66. For each it prints the resource that holds it, the header's offset within the
resource, and the little-endian dword at 0x04 of the 66 data bytes. With MIN, only those whose
dword is at least MIN (decimal) are printed, after a count of all. A header the byte pattern
matches inside other data is printed too; check that it lies in a chunk chain. With --levels, it
prints instead the class codes at 0x1B to 0x1D and the levels at 0x1E to 0x20 of each chunk's data,
and, when the 10 bytes 59 bytes before the header are a type-1 chunk header with len_data 49
(chunk_type 1, kind 2, len_data 0x31), the NUL-terminated name at 0x21 of that chunk's data.
With --byte OFFSET (hex), it prints the same, with the data byte at OFFSET after the levels.
Nothing is written or executed.
"""
import re
import struct
import sys
from pathlib import Path

body = Path(sys.argv[1]).read_bytes()
byte_at = int(sys.argv[3], 16) if sys.argv[2:3] == ["--byte"] else None
levels = sys.argv[2:] == ["--levels"] or byte_at is not None
minimum = int(sys.argv[2]) if len(sys.argv) > 2 and not levels else None
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
    if levels:
        data, before = offset + 10, body[offset - 59:offset - 49]
        name = ""
        if before[0] == 1 and before[2] == 2 and before[8:10] == bytes([0x31, 0]):
            name = body[offset - 49 + 0x21:offset - 49 + 0x30].split(bytes(1))[0].decode("latin-1")
        where = f"{holder[0]} {holder[1]}" if holder else "outside any resource"
        print(f"{where} (file 0x{offset:08X}): classes {list(body[data + 0x1B:data + 0x1E])}"
              f" levels {list(body[data + 0x1E:data + 0x21])}"
              + (f" byte 0x{byte_at:02X} {body[data + byte_at]}" if byte_at is not None else "") + f" {name}")
        continue
    if minimum is not None and value < minimum:
        continue
    where = f"{holder[0]} {holder[1]} +0x{offset - holder[2]:X}" if holder else "outside any resource"
    print(f"{where} (file 0x{offset:08X}): {value}")

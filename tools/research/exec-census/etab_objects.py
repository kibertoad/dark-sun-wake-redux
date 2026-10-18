"""Lists the region entity records that place given objects, over every installed GFF archive.

Usage: python -I etab_objects.py INSTALL_DIR OBJECT [OBJECT ...]

Reads every *.GFF in INSTALL_DIR through its directory (FMT-GFF-001 to FMT-GFF-003, FMT-GFF-006,
as data_bytes.py does), and for each ETAB resource (FMT-REGION-005) reads its 8-byte records
(FMT-REGION-006). Prints each record whose object word at 0x6 is one of the OBJECTs (decimal):
the archive, the ETAB number, the record's index, x, y and flags. Then prints how many archives,
ETAB resources and records it read. Nothing is written or executed.
"""
import struct
import sys
from pathlib import Path

directory, wanted = Path(sys.argv[1]), {int(v) for v in sys.argv[2:]}


def resources(body):
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
    found = [(tag, number, offset, size) for tag, rows in plain.items() for number, offset, size in rows]
    gffi = {number: (o, size) for number, o, size in plain.get("GFFI", [])}
    for tag, index_number, numbers in indexed:
        o, _ = gffi[index_number]
        for i, number in enumerate(numbers):
            r_offset, r_size = struct.unpack_from("<II", body, o + 4 + 8 * i)
            found.append((tag, number, r_offset, r_size))
    return found


archives = tables = records = 0
for path in sorted(directory.glob("*.GFF")):
    body = path.read_bytes()
    assert body[:4] == b"GFFI", path.name
    archives += 1
    for tag, number, offset, size in sorted(r for r in resources(body) if r[0] == "ETAB"):
        assert size % 8 == 0 and offset + size <= len(body), f"{path.name} ETAB {number}"
        tables += 1
        for k in range(size // 8):
            x, y, _, flags, obj = struct.unpack_from("<hhbBh", body, offset + 8 * k)
            records += 1
            if obj in wanted:
                print(f"{path.name} ETAB {number} record {k}: object {obj}, x {x}, y {y}, flags {flags}")
print(f"{archives} archives, {tables} ETAB resources, {records} records")

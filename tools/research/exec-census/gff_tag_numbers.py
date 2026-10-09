"""Lists which installed GFF archives hold a tag, and the resource numbers under it.

Usage: python -I gff_tag_numbers.py INSTALL_DIR TAG

Reads every *.GFF in INSTALL_DIR through its directory (FMT-GFF-001 to FMT-GFF-003, FMT-GFF-006)
and, for each archive with a table for TAG (four ASCII characters), prints the archive's name and
the resource numbers in that table, expanding an indexed table's number ranges. Prints the
archives that have no such table as a count. Nothing is written or executed.
"""
import struct
import sys
from pathlib import Path

directory, tag = Path(sys.argv[1]), sys.argv[2].encode().ljust(4)
without = 0
for path in sorted(directory.glob("*.GFF")):
    body = path.read_bytes()
    assert body[:4] == b"GFFI", path.name
    at = struct.unpack_from("<I", body, 0x0C)[0]
    count = struct.unpack_from("<H", body, at + 8)[0]
    p, numbers, found = at + 10, [], False
    for _ in range(count):
        name, word = body[p:p + 4], struct.unpack_from("<I", body, p + 4)[0]
        entries, indexed = word & 0x7FFFFFFF, word >> 31
        if indexed:
            ranges = struct.unpack_from("<I", body, p + 16)[0]
            pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
            if name == tag:
                found, numbers = True, [n for first, count in pairs for n in range(first, first + count)]
            p += 20 + 8 * ranges
        else:
            if name == tag:
                found = True
                numbers = [struct.unpack_from("<I", body, p + 8 + 12 * k)[0] for k in range(entries)]
            p += 8 + 12 * entries
    if found:
        print(f"{path.name}: {len(numbers)} resources {numbers}")
    else:
        without += 1
print(f"{without} archives have no {tag.decode()!r} table")

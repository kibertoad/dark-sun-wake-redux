"""Lists where byte patterns occur inside the resources of one tag in an installed GFF archive.

Usage: python -I gff_byte_search.py PATH_TO_GFF TAG HEX [HEX ...]

Reads the archive's directory (FMT-GFF-001 to FMT-GFF-007) as details_chunks.py does. For each
resource whose tag is TAG (padded with spaces to four characters), prints its number and size and,
for each pattern (hexadecimal bytes, such as e643), every offset inside the resource where it
occurs, with the two bytes before it. Nothing is written or executed.
"""
import re
import struct
import sys
from pathlib import Path


def resources(body):
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
            indexed.append((tag, index_number, [n for f, c in pairs for n in range(f, f + c)]))
            p += 20 + 8 * ranges
        else:
            plain[tag] = [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)]
            p += 8 + 12 * entries
    out = [(tag, n, o, s) for tag, rows in plain.items() for n, o, s in rows]
    gffi = {n: (o, s) for n, o, s in plain.get("GFFI", [])}
    for tag, index_number, numbers in indexed:
        offset, _ = gffi[index_number]
        for i, n in enumerate(numbers):
            ro, rs = struct.unpack_from("<II", body, offset + 4 + 8 * i)
            out.append((tag, n, ro, rs))
    return out



body = Path(sys.argv[1]).read_bytes()
tag = sys.argv[2].ljust(4)
patterns = [bytes.fromhex(p) for p in sys.argv[3:]]
for t, n, o, s in sorted(resources(body), key=lambda r: r[1]):
    if t != tag:
        continue
    data = body[o:o + s]
    hits = [(p.hex(), hex(m.start()), data[max(0, m.start() - 2):m.start()].hex())
            for p in patterns for m in re.finditer(re.escape(p), data)]
    print(t, n, s, hits)

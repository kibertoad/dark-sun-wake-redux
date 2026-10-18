"""Lists chosen colours of every PAL resource in the installed GFF archives.

Usage: python -I palette_entries.py INSTALL_DIR INDEX[..END] [...]

Reads every *.GFF in INSTALL_DIR through its directory (FMT-GFF-001 to FMT-GFF-003, FMT-GFF-006),
as gff_tag_numbers.py does, and for each `PAL ` resource (FMT-IMAGE-003, 768 bytes) prints the
archive, the resource number and the three components of each colour asked for: a single index,
or a half-open range START..END. Then it prints, for each index, how many distinct colours the
resources give it. Nothing is written or executed.
"""
import struct
import sys
from collections import defaultdict
from pathlib import Path

directory = Path(sys.argv[1])
indexes = []
for item in sys.argv[2:]:
    if ".." in item:
        low, high = (int(p) for p in item.split(".."))
        indexes.extend(range(low, high))
    else:
        indexes.append(int(item))
assert all(0 <= i < 256 for i in indexes), "an index is outside 0..255"


def resources(body, wanted):
    at = struct.unpack_from("<I", body, 0x0C)[0]
    count = struct.unpack_from("<H", body, at + 8)[0]
    p, plain, indexed = at + 10, {}, []
    for _ in range(count):
        tag, word = body[p:p + 4], struct.unpack_from("<I", body, p + 4)[0]
        entries, is_indexed = word & 0x7FFFFFFF, word >> 31
        if is_indexed:
            index_number, ranges = struct.unpack_from("<II", body, p + 12)
            pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
            numbers = [n for first, n_count in pairs for n in range(first, first + n_count)]
            indexed.append((tag, index_number, numbers))
            p += 20 + 8 * ranges
        else:
            plain[tag] = [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)]
            p += 8 + 12 * entries
    found = [(n, o, s) for n, o, s in plain.get(wanted, [])]
    gffi = {n: o for n, o, _ in plain.get(b"GFFI", [])}
    for tag, index_number, numbers in indexed:
        if tag == wanted:
            o = gffi[index_number]
            for i, n in enumerate(numbers):
                r_offset, r_size = struct.unpack_from("<II", body, o + 4 + 8 * i)
                found.append((n, r_offset, r_size))
    return sorted(found)


seen = defaultdict(set)
for path in sorted(directory.glob("*.GFF")):
    body = path.read_bytes()
    assert body[:4] == b"GFFI", path.name
    for number, offset, size in resources(body, b"PAL "):
        assert size == 768 and offset + size <= len(body), (path.name, number)
        colours = []
        for i in indexes:
            rgb = tuple(body[offset + 3 * i: offset + 3 * i + 3])
            seen[i].add(rgb)
            colours.append(f"{i}={rgb[0]},{rgb[1]},{rgb[2]}")
        print(f"{path.name} PAL/{number}: " + " ".join(colours))
print("distinct colours per index: " + " ".join(f"{i}:{len(seen[i])}" for i in indexes))

"""Prints the 23-byte records that the CHAR resources of CHARSAVE.GFF attach to their characters.

Usage: python -I char_item_records.py INSTALL_DIR

Reads each CHAR resource of INSTALL_DIR/CHARSAVE.GFF through the archive's directory (FMT-GFF-002)
and walks its chunks (FMT-PARTY-006): a 10-byte header, then the number of data bytes its word at
0x08 gives, until a chunk whose byte 0 is 0xFF. For each chunk of type 2 or 4 it prints the
resource number, the chunk's index, its type, target chunk and field word, and the 23 data bytes
as hex. It then prints, for each field of those records, reading 0x00 to 0x11 as signed words and
0x12 to 0x16 as bytes, how many distinct values it holds, their range and the four most common,
and whether OBJEX.GFF has an RDFF resource for each negated word at 0x00. Nothing is written or
executed.
"""
import collections
import struct
import sys
from pathlib import Path


def table(body, tag):
    at = struct.unpack_from("<I", body, 0x0C)[0]
    count = struct.unpack_from("<H", body, at + 8)[0]
    p = at + 10
    for _ in range(count):
        name, word = body[p:p + 4], struct.unpack_from("<I", body, p + 4)[0]
        entries, indexed = word & 0x7FFFFFFF, word >> 31
        if indexed:
            ranges = struct.unpack_from("<I", body, p + 16)[0]
            if name == tag:
                pairs = [struct.unpack_from("<II", body, p + 20 + 8 * k) for k in range(ranges)]
                return {n: None for first, count in pairs for n in range(first, first + count)}
            p += 20 + 8 * ranges
        else:
            rows = [struct.unpack_from("<III", body, p + 8 + 12 * k) for k in range(entries)]
            if name == tag:
                return {number: (offset, size) for number, offset, size in rows}
            p += 8 + 12 * entries
    return {}


directory = Path(sys.argv[1])
char = (directory / "CHARSAVE.GFF").read_bytes()
objex = (directory / "OBJEX.GFF").read_bytes()
rdff = table(objex, b"RDFF")
records = []
for number, (offset, size) in sorted(table(char, b"CHAR").items()):
    data, p, index = char[offset:offset + size], 0, 0
    while data[p] != 0xFF:
        kind, target = data[p], data[p + 1]
        field, length = struct.unpack_from("<HH", data, p + 6)
        if kind in (2, 4):
            body = data[p + 10:p + 10 + length]
            records.append(body)
            print(f"CHAR {number} chunk {index} type {kind} target {target} field {field} data {body.hex(' ')}")
        p += 10 + length
        index += 1
print(f"{len(records)} records")
fields = [(at, "<h") for at in range(0, 0x12, 2)] + [(at, "<B") for at in range(0x12, 0x17)]
for at, form in fields:
    values = collections.Counter(struct.unpack_from(form, r, at)[0] for r in records)
    common = ", ".join(f"{v} x{n}" for v, n in values.most_common(4))
    print(f"0x{at:02X} {'word' if form == '<h' else 'byte'}: {len(values)} values, "
          f"{min(values)} to {max(values)}; most common {common}")
objects = sorted({-struct.unpack_from("<h", r, 0)[0] for r in records})
print("objects", objects)
print("objects with no RDFF", [n for n in objects if n not in rdff])

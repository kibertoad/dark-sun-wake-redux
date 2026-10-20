"""Splits every RDFF resource of OBJEX.GFF into the chunk chain that 2D40:000A walks.

Usage: python -I rdff_chunks.py OBJEX.GFF [--combatant-byte HEX]

Finds the RDFF table through the archive's directory (FMT-GFF-001 to FMT-GFF-007) and walks each
resource as FMT-PARTY-006 gives a chunk: a 10-byte header whose byte 0 is the chunk type and
whose word at 8 is the number of data bytes after it, until a header whose byte 0 is 0xFF.
Prints the resource count, how many resources end exactly with a 10-byte end header, how many
do not, the largest chain, a count of chunks by type, kind (byte 2), byte 3, data length and
field (word 6), a count of chains by their sequence of types and lengths, and for chunks of
types 2 to 4 how many name in byte 1 an earlier chunk of the same resource. With
--combatant-byte, it also prints, over the data of every type-1 kind-2 chunk (the 49-byte
combatant record), how many hold each value of the byte at offset HEX, and for each nonzero value
the resource numbers with the record's name, the text at data offset 0x21 up to its first 0 byte.
Nothing is written or executed.
"""
import collections
import struct
import sys

body = open(sys.argv[1], "rb").read()
probe = int(sys.argv[sys.argv.index("--combatant-byte") + 1], 16) if "--combatant-byte" in sys.argv else None
values = collections.defaultdict(list)
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
kind = tables[b"RDFF"]
if kind[0] == "plain":
    resources = list(kind[1])
else:
    gffi_offset = next(o for n, o, s in tables[b"GFFI"][1] if n == kind[1])
    resources = [(n,) + struct.unpack_from("<II", body, gffi_offset + 4 + 8 * i) for i, n in enumerate(kind[2])]
chunks, chains, exact, longest, backward, forward = collections.Counter(), collections.Counter(), 0, 0, 0, 0
for number, offset, size in resources:
    data, q, chain = body[offset:offset + size], 0, []
    while q + 10 <= size and data[q] != 0xFF:
        ctype, target, ckind, b3, record, field, length = struct.unpack_from("<BBBBHHH", data, q)
        if ctype in (2, 3, 4):
            if target < len(chain):
                backward += 1
            else:
                forward += 1
        chunks[(ctype, ckind, b3, length, field)] += 1
        if probe is not None and ctype == 1 and ckind == 2:
            record = data[q + 10:q + 10 + length]
            name = record[0x21:].split(b"\x00")[0].decode("latin-1")
            values[record[probe]].append(f"{number} {name}")
        chain.append((ctype, length))
        q += 10 + length
    if q + 10 == size and data[q] == 0xFF:
        exact += 1
    else:
        print(f"RDFF {number}: size {size}, chain stops at {q}")
    longest = max(longest, len(chain))
    chains[tuple(chain)] += 1
print(f"{len(resources)} RDFF resources; {exact} end exactly with a 10-byte end header; "
      f"{len(resources) - exact} do not; the longest chain has {longest} chunks")
print("chunks by (type, kind, byte 3, data length, field):")
for key, n in sorted(chunks.items()):
    print(f"  {key}: {n}")
print(f"chunks of types 2 to 4 whose byte 1 names an earlier chunk: {backward}; any other: {forward}")
print("chains by (type, data length) sequence:")
for key, n in chains.most_common():
    print(f"  {n}: {key}")
if probe is not None:
    print(f"combatant byte 0x{probe:02X} over {sum(len(v) for v in values.values())} kind-2 chunks:")
    for value in sorted(values):
        print(f"  {value}: {len(values[value])}" + ("" if value == 0 else ": " + "; ".join(values[value])))

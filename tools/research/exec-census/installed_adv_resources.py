"""Lists the ADV resources of the installed RESOURCE.GFF and matches each with the disc's .ADV files (Q-EXE-027).

Usage: python -I installed_adv_resources.py INSTALL_DIR PATH_TO_BLD-GOG-EN-1.1.files.yaml

Checks the installed RESOURCE.GFF and each CD:*.ADV file (read from the ISO 9660 volume on the
MODE2/2352 data track of INSTALL_DIR/game.gog) against the build manifest's size and XXH3-128.
Reads RESOURCE.GFF's ADV tag table through its directory (FMT-GFF-001 to FMT-GFF-005: a plain
table, or an indexed one whose offsets and sizes are in a GFFI resource, FMT-GFF-006 and
FMT-GFF-007) and prints, for each resource, its number, offset, size and XXH3-128, the disc file whose
bytes it equals (or the disc file with the longest common prefix), and the two load signatures
DSUN.EXE's driver install routine 4842:0A23 tests (FND-EXE-585): the words at +3 spelling DIGPAK,
or the words at +2 and +4 spelling Copy. Then prints the values of the installed SOUND.INI's
MidiDriverChunkNumber and DigitalDriverChunkNumber keys. Nothing is written or executed.
"""
import os
import re
import struct
import sys

import xxhash

SECTOR, DATA = 2352, 24
root, manifest = sys.argv[1], sys.argv[2]

expected = {}
text = open(manifest, encoding="utf-8").read()
for m in re.finditer(r"- path: ([^\n]+)\n(?:\s+format: [^\n]+\n)?\s+size: (\d+)\n\s+xxh3: ([0-9a-f]{32})", text):
    expected[m.group(1)] = (int(m.group(2)), m.group(3))


def checked(path, body):
    assert (len(body), xxhash.xxh3_128_hexdigest(body)) == expected[path], path
    return body


def sectors(f, lba, count):
    out = bytearray()
    for i in range(count):
        f.seek((lba + i) * SECTOR + DATA)
        out += f.read(2048)
    return bytes(out)


disc = {}
with open(os.path.join(root, "game.gog"), "rb") as f:
    pvd = sectors(f, 16, 1)
    assert pvd[0] == 1 and pvd[1:6] == b"CD001"
    root_lba = int.from_bytes(pvd[158:162], "little")
    root_len = int.from_bytes(pvd[166:170], "little")
    data = sectors(f, root_lba, (root_len + 2047) // 2048)
    pos = 0
    while pos < root_len:
        n = data[pos]
        if n == 0:
            pos = (pos // 2048 + 1) * 2048
            continue
        lba = int.from_bytes(data[pos + 2:pos + 6], "little")
        size = int.from_bytes(data[pos + 10:pos + 14], "little")
        flags, idlen = data[pos + 25], data[pos + 32]
        name = data[pos + 33:pos + 33 + idlen].decode("latin1").split(";")[0].rstrip(".")
        pos += n
        if not flags & 2 and name.upper().endswith(".ADV"):
            disc["CD:" + name] = checked("CD:" + name, sectors(f, lba, (size + 2047) // 2048)[:size])
print(f"disc .ADV files {len(disc)}")

gff = checked("RESOURCE.GFF", open(os.path.join(root, "RESOURCE.GFF"), "rb").read())
assert gff[:4] == b"GFFI"
at = struct.unpack_from("<I", gff, 0x0C)[0]
count = struct.unpack_from("<H", gff, at + 8)[0]
p, tables = at + 10, {}
for _ in range(count):
    name, word = gff[p:p + 4], struct.unpack_from("<I", gff, p + 4)[0]
    entries, indexed = word & 0x7FFFFFFF, word >> 31
    if indexed:
        index, ranges = struct.unpack_from("<II", gff, p + 12)
        pairs = [struct.unpack_from("<II", gff, p + 20 + 8 * k) for k in range(ranges)]
        tables[name] = ("indexed", index, [n for first, run in pairs for n in range(first, first + run)])
        p += 20 + 8 * ranges
    else:
        tables[name] = ("plain", [struct.unpack_from("<III", gff, p + 8 + 12 * k) for k in range(entries)])
        p += 8 + 12 * entries
kind = tables[b"ADV "]
if kind[0] == "plain":
    table = kind[1]
else:
    # The offsets and sizes are in the GFFI resource numbered index_number (FMT-GFF-007).
    _, index, numbers = kind
    gffi = {n: (o, z) for n, o, z in tables[b"GFFI"][1]}
    o, z = gffi[index]
    assert struct.unpack_from("<I", gff, o)[0] == len(numbers) and z == 4 + 8 * len(numbers)
    table = [(n,) + struct.unpack_from("<II", gff, o + 4 + 8 * k) for k, n in enumerate(numbers)]


def signature(body):
    if body[3:9] == b"DIGPAK":
        return "DIGPAK at +3"
    if body[2:4] == b"Co" and body[4:6] == b"py":
        return "Copy at +2"
    return "neither"


for number, offset, size in table:
    body = gff[offset:offset + size]
    same = [k for k, v in disc.items() if v == body]
    if same:
        match = "equals " + ", ".join(same)
    else:
        best = max(disc, key=lambda k: next((i for i, (x, y) in enumerate(zip(disc[k], body)) if x != y),
                                            min(len(disc[k]), len(body))))
        common = next((i for i, (x, y) in enumerate(zip(disc[best], body)) if x != y), min(len(disc[best]), len(body)))
        match = f"no disc file equal; longest common prefix {common} bytes with {best} ({len(disc[best])} bytes)"
    print(f"ADV {number}: offset 0x{offset:X} size {size} xxh3 {xxhash.xxh3_128_hexdigest(body)} "
          f"{signature(body)}; {match}")

# The driver numbers the installed SOUND.INI's records can put in SOUND.CFG (FMT-CONFIG-002).
ini = checked("SOUND.INI", open(os.path.join(root, "SOUND.INI"), "rb").read()).decode("latin1")
for key in ("MidiDriverChunkNumber", "DigitalDriverChunkNumber"):
    values = sorted({int(v) for v in re.findall(r"\[" + key + r"\][ \t]*(-?\d+)", ini)})
    print(f"SOUND.INI {key}: {values}")

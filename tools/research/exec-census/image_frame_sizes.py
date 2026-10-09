"""Prints the width and height of each frame of one image resource in a GFF archive.

Usage: python -I image_frame_sizes.py ARCHIVE.GFF TAG NUMBER

Finds resource NUMBER under TAG (four ASCII characters) through the archive's directory
(FMT-GFF-001 to FMT-GFF-007), taking an indexed table's offsets and sizes from its GFFI resource,
and reads the resource as FMT-IMAGE-001: for each frame, the width and height words that start
it (FMT-IMAGE-002). For each frame it also prints the bytes and paragraphs that 1BF3:27A8 would
reserve for a rectangle (0, 0, width, height): ((width >> 2) + 1) * (height + 1) bytes, rounded
up to paragraphs. Nothing is written or executed.
"""
import struct
import sys

body = open(sys.argv[1], "rb").read()
tag, number = sys.argv[2].encode().ljust(4), int(sys.argv[3])
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
kind = tables[tag]
if kind[0] == "plain":
    offset, size = next((o, s) for n, o, s in kind[1] if n == number)
else:
    gffi_offset = next(o for n, o, s in tables[b"GFFI"][1] if n == kind[1])
    position = kind[2].index(number)
    offset, size = struct.unpack_from("<II", body, gffi_offset + 4 + 8 * position)
resource = body[offset:offset + size]
frames = struct.unpack_from("<H", resource, 4)[0]
print(f"{tag.decode()!r} {number}: offset 0x{offset:X}, size {size}, {frames} frames")
for i in range(frames):
    frame = struct.unpack_from("<I", resource, 6 + 4 * i)[0]
    width, height = struct.unpack_from("<HH", resource, frame)
    reserve = ((width >> 2) + 1) * (height + 1)
    print(f"frame {i}: width {width}, height {height}, reservation {reserve} bytes,"
          f" {(reserve + 15) >> 4} paragraphs")

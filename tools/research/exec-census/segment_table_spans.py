"""Tests whether each FBOV segment descriptor's unk_06..unk_02 is the span of its segment's bytes (Q-EXE-024).

Usage: python -I segment_table_spans.py PATH_TO_DSUN.EXE

Checks the installed DSUN.EXE's size and XXH3-128. Reads the 229 descriptors at file offset
0x4B080 (FMT-EXE-002) and, for each whose unk_02 is neither 0 nor 0xFFFF, takes the load-image
span segment * 16 + unk_06 .. segment * 16 + unk_02. Prints the counts of descriptors by flags
value and by kind of unk_02, then sorts the spans and reports every overlap, and every gap
between consecutive spans with its length and the distinct byte values in it. Prints the first
span's start, the last span's end and the load image's length. For the flags-0 and flags-1 spans that are not
empty, prints the last byte of each, the byte at its end, how many of the resident starts in
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv it holds and how many start a row of DSUN.EXE.regions.tsv.
Nothing is written or executed.
"""
import struct
import sys
from pathlib import Path
from collections import Counter

import xxhash

SIZE, XXH3, TABLE = 634416, "e296af55ba2ecde7e77f555c90f33d0b", 0x4B080

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image = body[header:(pages - 1) * 512 + (cblp or 512)]
rows = [struct.unpack_from("<HHHH", body, TABLE + 8 * i) for i in range(229)]
print("flags:", dict(sorted(Counter(r[2] for r in rows).items())))
print("unk_02 by flags:", dict(sorted(Counter((r[2], "0" if r[1] == 0 else "FFFF" if r[1] == 0xFFFF else "other")
                                              for r in rows).items())))
spans = sorted((seg * 16 + low, seg * 16 + high, i, flags) for i, (seg, high, flags, low) in enumerate(rows)
               if high not in (0, 0xFFFF))
print(len(spans), "spans; unk_06 > unk_02 in", sum(1 for s in spans if s[0] >= s[1]))
for (a0, a1, ai, af), (b0, b1, bi, bf) in zip(spans, spans[1:]):
    if b0 < a1:
        print(f"overlap: descriptor {ai} (flags {af}) ends {a1:#x}, descriptor {bi} (flags {bf}) starts {b0:#x}")
    elif b0 > a1:
        gap = image[a1:b0]
        print(f"gap {a1:#x}..{b0:#x} ({b0 - a1} bytes) after descriptor {ai}: bytes {sorted(set(gap))[:8]}")
print(f"first span starts {spans[0][0]:#x}, last ends {spans[-1][1]:#x}, load image {len(image):#x} bytes")
root = Path(__file__).resolve().parents[3] / "coverage/BLD-GOG-EN-1.1"
starts = []
for line in (root / "DSUN.EXE.tsv").read_text(encoding="utf-8").splitlines()[1:]:
    seg, _, off = line.split("\t")[0].partition(":")
    if off:
        starts.append(int(seg, 16) * 16 + int(off, 16) - 0x10000)
regions = {line.split("\t")[0] for line in (root / "DSUN.EXE.regions.tsv").read_text(encoding="utf-8").splitlines()[1:]}
for flags in (0, 1):
    held = [(seg * 16 + low, seg * 16 + high, seg, low) for seg, high, f, low in rows if f == flags and high > low]
    print(f"flags {flags}: {len(held)} non-empty spans; last byte {Counter(hex(image[b - 1]) for a, b, _, _ in held).most_common()};"
          f" byte at end {Counter(hex(image[b]) if b < len(image) else "image end" for a, b, _, _ in held).most_common(3)};"
          f" {sum(any(a <= s < b for a, b, _, _ in held) for s in starts)} of {len(starts)} resident inventory starts;"
          f" {sum(f'{0x1000 + seg:04X}:{low:04X}' in regions for _, _, seg, low in held)} start an inventory region")

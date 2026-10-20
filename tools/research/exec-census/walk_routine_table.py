"""Writes the routines a reach report's walk decoded as a routine table in the coverage TSV's layout.

Usage: python -I walk_routine_table.py PATH_TO_DSUN.EXE REACH_REPORT_JSON OUTPUT_TSV

Checks the installed DSUN.EXE's size and XXH3-128. Reads the `decodedRanges` of a
scientific-method-engine `reach` report (file offsets of the runs of instructions its walk decoded,
each with the file offset of its routine) and writes one row per routine: its start, as a resident
address in the segment the report's `declaredRegions` give it (load image at segment 0x1000) or as
a file offset `0xHEX` for overlay code, the number of bytes its ranges cover, and the ranges as
half-open file offsets `0xHEX..0xHEX`. ds_restore_census.py, segment_register_writes.py and
window_operands.py read the table in place of the coverage TSV, so that they cover the code the
walk reached, including code the coverage inventory lacks. Nothing in the repository is written
and nothing is executed.
"""
import json
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

dsun, report, out = sys.argv[1:4]
body = open(dsun, "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
cblp, pages = struct.unpack_from("<HH", body, 2)
image_end = (pages - 1) * 512 + (cblp or 512)
walk = json.load(open(report, encoding="utf-8"))
segments = [(r["start"], r["end"], r["segment"]) for r in walk["declaredRegions"] if r["segment"] < 0x9000]


def name(at):
    if header <= at < image_end:
        for low, high, segment in segments:
            if low <= at < high:
                return f"{segment:04X}:{at - header - (segment - 0x1000) * 16:04X}"
    return f"0x{at:08X}"


rows = {}
for r in walk["decodedRanges"]:
    rows.setdefault(r["routine"], []).append((r["start"], r["end"]))
with open(out, "w", encoding="utf-8", newline="") as f:
    f.write("start\tsize\tranges\n")
    for routine in sorted(rows):
        spans = sorted(rows[routine])
        size = sum(high - low for low, high in spans)
        f.write(f"{name(routine)}\t{size}\t" + " ".join(f"0x{low:08X}..0x{high:08X}" for low, high in spans) + "\n")
print(len(rows), "routines")

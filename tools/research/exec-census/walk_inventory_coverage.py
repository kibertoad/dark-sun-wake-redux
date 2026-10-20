"""Counts the instructions of a reach walk that lie outside every range of a DSUN.EXE inventory.

Usage: python -I walk_inventory_coverage.py PATH_TO_DSUN.EXE INVENTORY_TSV REACH_REPORT_JSON [--list N]

Checks the installed DSUN.EXE's size and XXH3-128. Decodes each of the report's `decodedRanges`
(half-open file offsets of runs of instructions the walk decoded) linearly with Capstone in 16-bit
mode and counts the instructions, those whose first byte lies in no range of the inventory
(coverage/<build>/DSUN.EXE.tsv, resident ranges as segment:offset with the load image at segment
0x1000, overlay ranges as file offsets) and those whose bytes are only partly in its ranges. It then
prints, per walk routine, the outside instructions, largest first, the first N of them with
--list. Nothing is written or executed.
"""
import json
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
limit = int(sys.argv[sys.argv.index("--list") + 1]) if "--list" in sys.argv else 0


def file_offset(text):
    if ":" not in text:
        return int(text, 16)
    segment, offset = (int(p, 16) for p in text.split(":"))
    return (segment - 0x1000) * 16 + offset + header


covered = bytearray(len(body))
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    for r in line.split("\t")[2].split():
        a, b = (file_offset(x) for x in r.split(".."))
        covered[a:b] = b"\x01" * (b - a)

md = Cs(CS_ARCH_X86, CS_MODE_16)
walk = json.load(open(sys.argv[3], encoding="utf-8"))
total = outside = partial = 0
per_routine = {}
for r in walk["decodedRanges"]:
    for i in md.disasm(body[r["start"]:r["end"]], r["start"]):
        total += 1
        if not covered[i.address]:
            outside += 1
            per_routine[r["routine"]] = per_routine.get(r["routine"], 0) + 1
        elif not all(covered[i.address:i.address + i.size]):
            partial += 1
print("walk instructions", total, "outside every range", outside, "partly outside", partial,
      "routines with outside instructions", len(per_routine))
for routine, n in sorted(per_routine.items(), key=lambda kv: -kv[1])[:limit]:
    print(f"routine 0x{routine:08X} outside {n}")

"""Tells which immediate_search.py or store_values.py candidates are instruction starts.

Usage: python -I candidate_starts.py PATH_TO_DSUN.EXE INVENTORY_TSV CANDIDATES_TXT

Checks the installed DSUN.EXE's size and XXH3-128. Reads the function inventory (coverage/<build>/
DSUN.EXE.tsv: resident ranges as SEGMENT:OFFSET for a load image at segment 0x1000, overlay ranges as
file offsets) and the candidate lines a search printed (SSSS:OOOO for the load image, "overlay N file
0xOFFSET" for overlay code). For each candidate it decodes the inventory range that holds it linearly
from the range's start and prints START when the candidate begins an instruction of that decode,
inside when it falls within another instruction, or NOFUNC when no inventory range holds it. A START
is an instruction of the linear decode, not proof that execution reaches it; a NOFUNC candidate stays
undecided. Nothing is written or executed.
"""
import csv
import re
import struct
import sys

import xxhash
from capstone import CS_ARCH_X86, CS_MODE_16, Cs

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"

body = open(sys.argv[1], "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
header = struct.unpack_from("<H", body, 8)[0] * 16
md = Cs(CS_ARCH_X86, CS_MODE_16)


def resident(seg, off):
    return header + (seg - 0x1000) * 16 + off


ranges = []
with open(sys.argv[2], encoding="utf-8") as inventory:
    for row in csv.DictReader(inventory, delimiter="\t"):
        for item in row["ranges"].split(" "):
            if ".." not in item:
                continue
            low, high = item.split("..")
            if ":" in low:
                start = resident(*(int(p, 16) for p in low.split(":")))
                end = resident(*(int(p, 16) for p in high.split(":")))
            else:
                start, end = int(low, 16), int(high, 16)
            if 0 <= start < end <= len(body):
                ranges.append((start, end, row["start"]))

decoded = {}
with open(sys.argv[3], encoding="utf-8") as candidates:
    for line in candidates:
        line = line.rstrip("\n")
        overlay = re.match(r"overlay \d+ file 0x([0-9A-Fa-f]+)\s", line)
        load = re.match(r"([0-9A-F]{4}):([0-9A-F]{4})\s", line)
        if overlay:
            at = int(overlay.group(1), 16)
        elif load:
            at = resident(int(load.group(1), 16), int(load.group(2), 16))
        else:
            continue
        holder = next((r for r in ranges if r[0] <= at < r[1]), None)
        if holder is None:
            print(f"NOFUNC 0x{at:08X} {line}")
            continue
        if holder not in decoded:
            decoded[holder] = {i.address for i in md.disasm(body[holder[0]:holder[1]], holder[0])}
        mark = "START " if at in decoded[holder] else "inside"
        print(f"{mark} 0x{at:08X} {holder[2]} {line}")

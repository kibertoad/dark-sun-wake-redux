"""Lists the routines a reach walk reached that load a party record pointer.

Usage: python -I reach_record_loads.py IMMEDIATE_TXT INVENTORY_TSV REACH_REPORT [REACH_REPORT ...]

IMMEDIATE_TXT is the output of `immediate_search.py <dsun> 19C9 19C5 039C 09CD`. Keeps its hits
whose memory operand is `[0x19c9]` (combatant table pointer), `[0x19c5]` (details table pointer),
`[... + 0x39c]` (SPST resources) or `[... + 0x9cd]` (PSST bytes), places each in the inventory row
that holds it, and for each `scientific_method_engine reach` report prints the rows whose start is
among its `reachedRoutines`, with up to four hits each. Nothing is written or executed.
"""
import json
import re
import sys

H = 0x5200
pat = re.compile(r"\[0x19c9\]|\[0x19c5\]|\+ 0x39c\]|\+ 0x9cd\]")


def res(s, o):
    return H + (s - 0x1000) * 16 + o


rows = []
for line in open(sys.argv[2], encoding="utf-8").read().splitlines()[1:]:
    start, _, ranges = line.split("\t")
    st = res(*(int(p, 16) for p in start.split(":"))) if ":" in start else int(start, 16)
    for item in ranges.split():
        lo, hi = item.split("..")
        if ":" in lo:
            s, o = (int(p, 16) for p in lo.split(":"))
            rows.append((res(s, o), res(s, int(hi.split(":")[1], 16)), st, start))
        else:
            rows.append((int(lo, 16), int(hi, 16), st, start))

hits = {}
for line in open(sys.argv[1], encoding="utf-8", errors="replace"):
    m = re.match(r"overlay \d+ file 0x([0-9A-F]+)\s+(.*)", line)
    if m:
        at, ins = int(m.group(1), 16), m.group(2)
    else:
        m = re.match(r"([0-9A-F]{4}):([0-9A-F]{4})\s+(.*)", line)
        if not m:
            continue
        at, ins = res(int(m.group(1), 16), int(m.group(2), 16)), m.group(3)
    if not pat.search(ins):
        continue
    r = next((r for r in rows if r[0] <= at < r[1]), None)
    if r:
        hits.setdefault(r[2], (r[3], []))[1].append((at, ins.strip()))

for path in sys.argv[3:]:
    rep = json.load(open(path))
    got = sorted(set(rep["reachedRoutines"]) & set(hits))
    print(f"== {path}: {len(rep['reachedRoutines'])} routines, {len(got)} with record loads")
    for g in got:
        name, lst = hits[g]
        print(f"  {name} (0x{g:X}): " + "; ".join(f"0x{a:X} {i}" for a, i in lst[:4]) + (" ..." if len(lst) > 4 else ""))

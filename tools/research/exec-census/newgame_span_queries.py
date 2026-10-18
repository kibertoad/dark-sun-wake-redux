"""Writes the reach queries for the new-game span from the gate routine's +12FF to MAS/50 0x05DC.

Usage: python -I newgame_span_queries.py PATH_TO_DSUN.EXE EFFECT_QUERY PALETTE_QUERY OUT_DIR MODE
           [--fight-cut]

EFFECT_QUERY and PALETTE_QUERY are FND-EXE-592's effect and palette queries (FND-COMBAT-054's first
query and FND-UI-049's second, with the census declarations and newgame_declarations.json added).
MODE picks the opcodes the script dispatch at 172C:01B5 may call:
- `reached`: the 34 opcodes `script_reach.py` finds from MAS/50 offset 0 when it takes both ways at
  every condition (FND-COMBAT-055);
- `executed`: the 29 opcodes MAS/50 and the scripts it calls run when global 22 is 1 and global 82
  and global flag 188 are 0 as MAS/50 starts (FND-UI-052).
Writes four queries to OUT_DIR: `effects` (starts: the gate routine's calls from +12FF up to its
call of 1695:000B) and `palette` (starts: its calls after the DAC write at +13E3), each also with
`-cut37`, which cuts the case of opcode 0x22's function 37. Every query keeps the base query's
declarations, leaves and the 34 relocated rows of segment 2707 as starts, narrows the dispatch,
cuts the cases of overlay 204 +0169 for the opcode 0x22 functions other than 33, 34, 37, 38 and 52,
and cuts the interpreter's error routine, the runtime exit, the stack guard, overlay 193 +12A8,
the opcode hook's call of overlay 199 +0C21 and the region change of opcode 0x5E. `--fight-cut`
also cuts the two branches of 2D40:10AE taken only when the fight state is not 0. Targets: the base
query's, the window hook, record and frame callback call sites, the three trigger-run calls of
172C:000C and the seven stores to the fight state. Checks the installed DSUN.EXE's size and
XXH3-128. Nothing is executed.
"""
import copy
import json
import os
import struct
import sys

import xxhash

SIZE, XXH3 = 634416, "e296af55ba2ecde7e77f555c90f33d0b"
OPCODE_TABLE = 0x4D30A  # file offset of the 129 words at DS:030A (FND-SCRIPT-005)
FUNCTION_TABLE = 0x8BDC0 + 0x6C7  # overlay 204's code at 0x8BDC0, the 53 words at CS:06C7

REACHED = [0x08, 0x09, 0x13, 0x14, 0x15, 0x16, 0x18, 0x19, 0x1B, 0x1C, 0x1D, 0x1F, 0x22, 0x25,
           0x31, 0x33, 0x37, 0x39, 0x3E, 0x3F, 0x40, 0x52, 0x5E, 0x65, 0x66, 0x67, 0x6A, 0x6C,
           0x6D, 0x6E, 0x6F, 0x70, 0x7A, 0x80]
EXECUTED = [0x09, 0x13, 0x14, 0x15, 0x16, 0x18, 0x19, 0x1B, 0x1C, 0x1F, 0x22, 0x25, 0x31, 0x33,
            0x39, 0x3E, 0x3F, 0x52, 0x65, 0x66, 0x67, 0x6A, 0x6C, 0x6D, 0x6E, 0x6F, 0x70, 0x7A,
            0x80]
FUNCTIONS = [33, 34, 37, 38, 52]
HOOKS = ["39D1:07CC", "39D1:083B", "39D1:0B55", "39D1:0786", "39D1:0899", "39D1:0802",
         "3A8E:08B7", "3A8E:0707", "3D72:0590"]
TRIGGERS = ["0x2354A", "0x2465A", "0x2478A"]  # far calls of 172C:000C at 2D40:0F4A, 205A, 218A
FIGHT = ["0x23A37", "173+22B7", "173+409D", "188+04CF", "193+0533", "0x1FFF0", "0x20FDE"]
FROM_12FF = ["182+0365", "182+08C8", "200+007E", "2C5F:018C", "182+1F1D", "31E0:0121",
             "200+0000", "4611:0051", "1BF3:4FEB", "1BF3:4723", "3D72:0D83", "444C:0008",
             "3796:0514", "444C:0092", "2C5F:0139", "2C5F:0C8D", "28C9:2A44", "187+2496",
             "1695:000B"]
AFTER_13E3 = ["444C:0092", "2C5F:0139", "2C5F:0C8D", "3D72:0D83", "28C9:2A44", "187+2496",
              "1695:000B"]

dsun, effect_path, palette_path, out, mode = sys.argv[1:6]
fight_cut = "--fight-cut" in sys.argv[6:]
assert mode in ("reached", "executed"), "MODE is reached or executed"
body = open(dsun, "rb").read()
assert (len(body), xxhash.xxh3_128_hexdigest(body)) == (SIZE, XXH3), "not the installed DSUN.EXE"
handlers = struct.unpack_from("<129H", body, OPCODE_TABLE)
cases = struct.unpack_from("<53H", body, FUNCTION_TABLE)
ops = REACHED if mode == "reached" else EXECUTED
used = {cases[f - 1] for f in FUNCTIONS}
cut_cases = sorted({c for c in cases if c not in used})
os.makedirs(out, exist_ok=True)


def query(base, starts, extra_targets, cut37):
    q = copy.deepcopy(base)
    rows = [s for s in base["starts"] if s.startswith("2707:")]
    q["starts"] = starts + rows
    q["targets"] = base["targets"] + extra_targets
    for d in q["indirectCalls"]:
        if d["site"] == "172C:01B5":
            d["targets"] = sorted({"172C:%04X" % handlers[o] for o in ops})
            d["evidence"] = "FND-EXE-592's declaration narrowed to the handlers of the %s opcodes" % mode
    stops = q.setdefault("stops", [])
    stops.append({"site": "172C:1B16", "reason": "FND-UI-051: no 0x5E on MAS/50's script route has the first operand 32766"})
    for c in cut_cases:
        stops.append({"site": "204+%04X" % c, "reason": "a case of overlay 204 +0169 for an opcode 0x22 function not on MAS/50's script route"})
    stops.append({"routine": "188+1901", "reason": "FND-SCRIPT-006, FND-PARTY-041: the interpreter's error routine"})
    stops.append({"routine": "1000:2E48", "reason": "FND-CONFIG-163: the stack guard, which ends the program"})
    stops.append({"routine": "1000:0388", "reason": "FND-CONFIG-061, FND-CONFIG-163: the runtime exit"})
    stops.append({"routine": "193+12A8", "reason": "FND-COMBAT-054, FND-COMBAT-055: runs only for a slot made from object 147, which the route does not place"})
    stops.append({"site": "2D40:380A", "reason": "FND-PARTY-041: the opcode hook calls overlay 199 +0C21 only for opcode 0x31, which MAS/50 reaches only at 0x05E5"})
    if fight_cut:
        for site in ("2D40:11A7", "2D40:1C46"):
            stops.append({"site": site, "reason": "FND-PARTY-199, FND-PARTY-200: taken only when the fight state is not 0"})
    if cut37:
        stops.append({"site": "204+0657", "reason": "cut: opcode 0x22 function 37's case"})
    if mode == "executed":
        q["controls"] = ["172C:0FAB"]
        q["instructionControls"] = ["172C:01B5", "204+0623", "172C:0FC2"]
    return q


effect = json.load(open(effect_path))
palette = json.load(open(palette_path))
extra = HOOKS + TRIGGERS + FIGHT
for name, base, starts, targets in [
    ("effects", effect, FROM_12FF, ["193+18E3"] + extra),
    ("palette", palette, AFTER_13E3, extra),
]:
    for suffix, cut37 in (("", False), ("-cut37", True)):
        json.dump(query(base, starts, targets, cut37),
                  open(os.path.join(out, name + suffix + "-query.json"), "w"), indent=1)
print("dispatch", " ".join(sorted({"172C:%04X" % handlers[o] for o in ops})))
print("cut cases", " ".join("204+%04X" % c for c in cut_cases))

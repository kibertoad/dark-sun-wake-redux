---
id: FND-PARTY-171
title: The leader's slot word 4C13:0369 is 0 in the load image, ADD's placement leaves it alone, and DROP drops any box but the leader's, so a character added to a box other than slot 0 can be dropped while no leader button has been pressed
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4C13:0369..4C13:036B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3DA6..2D40:3DB9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0226..31E0:022F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A58E..0x0006A6D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CDE7..0x0005CE64
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E72A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058F25..0x00059014
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073C95..0x00073CA1
tool: scientific-method-engine 19.3.0 (reach, Capstone 5.0.9, pypcode 4.0.1) (reach_config.py with q006_leader_writers_reach.json and q006_add_leader_reach.json); Capstone 5.0.9 (bounded listings); tools/research/exec-census/trampoline_target.py
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A far call's segment word in overlay code is eight times a segment table row; row 93
is `4C13`, row 113 `4F49`, row 182 `56BD` and row 25 `2D40`.

**The word.** The leader's slot is the word at `4C13:0369` (FND-COMBAT-023); its two bytes in the
load image (file `0x00041699`) are `00 00`. A scan that decodes every instruction ending in the
bytes `69 03` from two to five bytes before them finds two stores to it: `mov es:[0369], si` at
`2D40:3DB4`, and `mov word es:[0369], 0` at overlay 188 `+0DFA`, both
with ES just loaded with `4C13`.

**The setter.** `2D40:3DB4` stores `si` there when the word equals `di`, at the end of `2D40:3D4C`.
The far calls to `2D40:3D4C` are at `28C9:219C`, `28C9:3185`, overlay 173 `+080C`, overlay 182
`+1D94` and overlay 188 `+186B`. Overlay 182 `+1D3E` (trampoline `56BD:0020`) takes a slot `si` and
a flag. It returns at once when `si` is 4 or more or already the leader's (`+1D49..+1D5D`), and when
byte `+0x14` of the FMT-COMBAT-001 record the word at `4F49:0C34 + 3 * si` selects is above 2
(`+1D60..+1D7F`). Otherwise it calls `2D40:3D4C` with the old leader and `si` (`+1D94`). Its far
callers are overlay 171 `+0DAA`, overlay 173 `+2539` and `+3DFA`, overlay 188 `+14CA`, overlay 189
`+2191`, overlay 190 `+096F` and `+109A`, overlay 208 `+1681`, and overlay 213 `+06E8`, `+0AC2`
and `+0B57`.

**Who changes it on the Create Characters screen.** The walk `q006_leader_writers_reach.json`,
FND-PARTY-045 run 4's query with these 18 sites as targets, reaches 1,222 routines with 66
transfers unresolved. It reaches the store at `2D40:3DB4` through the screen's handler, overlay
190 `+0802` and its call at `+096F` in the branch `+08EB` for the leader buttons `0x2C2D` to
`0x2C30` (FND-PARTY-170), which passes the button's slot. It also reaches `+109A` and overlay 171
`+0DAA`, both in `DROP` (FND-PARTY-170), and overlay 173 `+2539` and `+3DFA` through the
placement routine `31E0:0121`.

**ADD's placement.** The walk `q006_add_leader_reach.json`, FND-PARTY-046 run 1's query with
the same targets, reaches the store through `31E0:0121`'s call at `31E0:0303` to overlay 193 and
on to overlay 173 `+24C7`. `31E0:0121` reaches that call only when bit 2 of its argument at
`[bp + 0x12]` is set (`test byte [bp+12h], 4` at `31E0:0226`; otherwise it jumps to `31E0:03C3`),
and `ADD` pushes 0 there (FND-PARTY-048). Overlay 173 `+24C7` calls the leader setter with the
leader's own slot (`+250D..+2539`), which returns at once. Overlay 173 `+3CDA`, which `+24C7`
calls first, calls it with the first slot whose kind byte at `4F49:0C33 + 3 * si` is 2 and whose
record's `+0x14` is 1, only when the byte at `DS:13FB` ends the slot loop at 0
(`+3DBB..+3DFA`).

**DROP.** Overlay 171 `+0D15` (trampoline `5664:0025`), which `DROP` calls with 0, takes the slot
from `4E71:0B44` and calls overlay 184 `+1446` (trampoline `56DD:004D`) with it, which writes the
character back to `CHARSAVE.GFF` through overlay 187 `+018F` and overlay 186 `+01B6`. When the
slot is the leader's, it calls the leader setter for the first other slot whose kind byte is not
0 (`+0D87..+0DB2`). It then calls `31E0:05BB` with the slot's word at `DS:67BC + 0x25 * slot`, 1,
0 and 0 (`+0DBA..+0DC9`), which removes that placed entry (FND-PARTY-118), stores 8 at
`4F49:0330 + slot` and 0 at `4E71:0A55 + slot`, and keeps only bit 0 of the 0x22 bytes at
`4E71:09CD + 0x22 * slot` (`+0DD1..+0DFF`). It stores nothing to the box's 66-byte record at
`DS:19C5`. The only stores to that record's `+0x0E` a scan of every `les` from `DS:19C5` and the
14 instructions after it finds are 0 at `2D40:2513`, inside the switch at `2D40:248D`, and at
overlay 184 `+1113`, and `si` at overlay 184 `+1D9B`.

## Interpretation

On a new game the leader is slot 0 unless something before the start window changes it, and on
the Create Characters screen only a leader button, `DROP` of the leader's box, or the end of the
slot loop in overlay 173 `+3CDA` moves it; `ADD` does not. `DROP` refuses only the leader's box
when no other box holds a character (FND-PARTY-170). So a player who adds a character to box 1, 2
or 3 without pressing a leader button can drop it, and the placed-object count it raised stays
nonzero (FND-PARTY-022).

## Alternatives

- Code that runs before the start window calls the leader setter: the reading of that path
  (FND-PARTY-031) does not list the setter's callers, so it is not ruled out.
- The walks' unresolved transfers could reach another writer; the store scan covers the direct
  forms only, not a store through a base register or a block copy over `4C13:0369`.
- Whether `DROP` leaves the box empty, through `31E0:05BB`'s removal reaching `2D40:2513`, is not
  read here (Q-PARTY-006).

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, in `tools/research/exec-census/`: list `2D40:3D4C..2D40:3DBD`, `31E0:0121..31E0:0310` from
the load image (file `0x5200` plus 16 times the segment less `1000`, plus the offset), and overlay
182 `+1D3E..+1E81`, overlay 173 `+24C7..+2544` and `+3CDA..+3E0A`, and overlay 171 `+0D15..+0E04`
at the file offsets above. `trampoline_target.py <dsun> 56BD:0020 5664:0025 56DD:004D 56EF:0020
56E9:0020` names the overlay targets. Run `reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
q006_leader_writers_reach.json` and `python -I -m scientific_method_engine reach <config>`, and the
same for `q006_add_leader_reach.json`; the report's targets give the chains.

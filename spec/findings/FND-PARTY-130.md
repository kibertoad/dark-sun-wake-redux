---
id: FND-PARTY-130
title: Of overlay 182 +19F8's mode 2 callees, only 2C5F:0139 reaches routines that load a party record pointer, through the view draw 31BA:000E loading an object whose byte at 4F49:0C33 is 2
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418..39D1:0460
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:250A..28C9:2522
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0139..2C5F:0182
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31BA:0104..31BA:0149
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0C68..31E0:0C93
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E64..2D40:3EBF
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, reach_record_loads.py, immediate_search.py, resident_listing.py, trampoline_target.py, q054_mode2_callees_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. While `DS:0DAB` is 2 or 3, overlay 182 `+19F8` calls its own `+01EF`, `39D1:0430`,
`2C5F:0139`, `28C9:250A`, `39D1:0448` and overlay 190 `+11A1` through `571F:0034` (FND-PARTY-123).

**The short callees.** `39D1:0418`, `39D1:0430` and `39D1:0448` each store their dword argument at
`DS:A0F1`, `DS:A0F5` and `DS:A0F9` after a stack check through `1000:2E48`
(`39D1:0418..39D1:0460`); overlay 190 `+11A1` stores its argument at `DS:61A2` and calls
`39D1:0418` (FND-PARTY-127). `28C9:250A` copies the word at `DS:4596` to `DS:1440` and calls
`2C5F:0C8D` with it when the two differ (`28C9:250A..28C9:2522`). `2C5F:0139` calls `362C:035C`
with 1 and the words at `DS:1408` and `DS:140A`, `31BA:000E` with those words, 1 and 3,
`362C:06B9`, its own `+070E` with 1 and `+06CB` with 1 and 0, and stores 0 in the byte at
`DS:143F` (`2C5F:0139..2C5F:0182`).

**The walks.** `immediate_search.py <dsun> 19C9 19C5 039C 09CD` gives the loads of the combatant
table pointer `DS:19C9`, the details table pointer `DS:19C5`, and the `SPST` and `PSST` tables at
`+0x39C` and `+0x9CD` (FND-PARTY-104). `reach_record_loads.py` places each in its inventory row and
lists the rows a reach walk reached. Walks from one start each, with the exit worker `1000:0388`
as a leaf:

| Start | Routines | Unresolved | Rows with a record load |
|---|---|---|---|
| overlay 182 `+01EF` | 19 | 2 | none |
| `28C9:250A` | 90 | 2 | none |
| `362C:035C` | 50 | 0 | none |
| `362C:06B9` | 1 | 0 | none |
| `2C5F:070E` | 12 | 0 | none |
| `2C5F:06CB` | 108 | 5 | `2C5F:03F1`, `2D40:3E64` |
| `2C5F:0139` | 927 | 34 | 98 |
| `31BA:000E` | 927 | 34 | 98 |

The walk from `2C5F:0139` uses `q054_mode2_callees_reach.json`, whose control, the call at
`2C5F:0146`, it reaches; it reads 76,448 instructions. The others run without a control.

`2D40:3E64` stores only through its two far pointer arguments (`2D40:3E91..2D40:3E94`,
`2D40:3EAC..2D40:3EAF`). `2C5F:03F1` loads both table pointers and passes the records to
`3150:000E` and its own `+03B7` and `+09F3`; its own stores are to its local at `bp-6` and the
byte at `DS:0F3A` (`2C5F:03F1..2C5F:06CB`).

**The view draw.** `31BA:000E` steps through the 8-byte entries at the far pointer `DS:67B7`;
for an entry whose byte at `+5` has bit 4 clear and whose word at `+6` is negative, it calls
`31E0:0EFF` with that word negated and the entry's index, and stores a result that is not negative
at `+6` (`31BA:0104..31BA:0149`). Every route the `31BA:000E` walk reads to overlay 173 `+22C4`,
overlay 179 `+0D2F`, overlay 188 `+1604`, overlay 176 `+0196` and overlay 198 `+0000` passes
`31E0:0EFF`. The shortest goes on through `31E0:0D09` and `31E0:0B4A`, which, when the byte at
`4F49:0C33` plus 3 times its argument is 2, calls overlay 193 `+1DB0` (trampoline `573B:002F`) and
`371D:0002` with the argument, -1, 0 and 1 (`31E0:0C68..31E0:0C93`).

## Interpretation

Of what overlay 182 `+19F8` runs in mode 2 or 3, only the view draw `2C5F:0139` reaches code that
reads a party member's records, and the routes the walk reads all go through the loading of an
object that has come into view and whose state byte at `4F49:0C33` is 2. The damage, hit, award
and psionic routines on those routes write the records. Whether an object can be unloaded and in
that state when a level gain redraws the view decides Q-PARTY-054.

## Alternatives

- The walks' negative results are not usable: the larger walks hold 34 unresolved computed
  transfers, and the small ones run without a control.
- A routine that writes a record through a pointer its caller loaded, without loading `DS:19C9`,
  `DS:19C5` or the `SPST` and `PSST` tables itself, is not listed by `reach_record_loads.py`.
- What the byte at `4F49:0C33` holds, and when an entry's word at `+6` is negative, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `immediate_search.py <dsun> 19C9 19C5 039C 09CD`
into `<immediate>`, `trampoline_target.py <dsun> 573B:002F` and `resident_listing.py` over the
ranges above. In the repository root run `python -I tools/research/exec-census/reach_config.py
<dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q054_mode2_callees_reach.json`, `python -I -m scientific_method_engine
reach <config>` into `<report>`, and `python -I tools/research/exec-census/reach_record_loads.py
<immediate> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <report>`. For the other rows, replace `starts`
with the one start and empty `controls`.

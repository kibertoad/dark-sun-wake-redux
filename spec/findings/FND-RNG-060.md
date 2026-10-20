---
id: FND-RNG-060
title: Of the removal routine's five long-route callees, the unload, 2C5F:0139 and overlay 197 +30ED reach a draw only through an effect row removal that runs overlay 195 +1000, overlay 206 +0A4E also through overlay 193 +0DC7, and 1695:02E7 runs a script
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F969..0x0008F978
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F9AA..0x0008F9BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F7E9..0x0008F7F2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0ABC..31E0:0ACC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000864ED..0x0008652A
tool: tools/research/exec-census/reach_config.py, overlay_listing.py and resident_listing.py at the commit that adds this finding; scientific_method_engine 19.10.0 reach; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-050 finds that `1695:02E7`, overlay 206
`+0A4E`, overlay 197 `+30ED`, the unload `31E0:05BB` and `2C5F:0139`, which the removal routine
overlay 206 `+0E1E` calls, and overlay 201 `+05D5`'s calls other than those of overlay 204
`+0000`, reach the seed setter, the generator, `roll_sum` and `random_between` only through long
routes.

**Cut walks.** Each query copies FND-RNG-050's (its six targets, the leaf `1000:0388`, its 112
computed-transfer declarations, the table jump and the call that does not return), keeps its
control and instruction control for the start, and adds as `stops`, and as targets, the routines
the routes pass: the movement routine `2D40:10AE`, the attack routine overlay 173 `+1CC1`, overlay
193 `+0DC7`, `+166F`, `+1DB0` and the effect key removal `+1873`, the script handler `172C:170B`,
`1695:02E7` and `+0E1E` itself, each omitted from its own walk. Every walk reaches its control
and decodes its instruction control, and none has an unresolved transfer, a gap or a contested
start.

| Start | Routines | Targets of the six reached | Cut routines reached |
|---|---|---|---|
| `31E0:05BB` | 302 | none | `+1DB0`, through `31E0:044C` |
| `2C5F:0139` | 305 | none | `+1DB0`, through `31BA:000E`, `31E0:0EFF`, `31E0:0D09` and `31E0:0B4A` |
| overlay 206 `+0A4E` | 424 | none | `+1DB0`, through `2C5F:0271`, `31E0:1478`, `31BA:000E` and on as above; `+0DC7`, through `2C5F:0271`, `31E0:2C86` and `31E0:2D6E` |
| overlay 197 `+30ED` | 3 | none | `+1873`, at `+3125` |
| `1695:02E7` | 1725 | the setter, the generator, `roll_sum`, `random_between` | all; every chain passes `1695:06FF`, `1695:075D`, `172C:000C` and `172C:00A1` |
| overlay 201 `+05D5` | 1730 | the setter and the generator through overlay 204 `+0000`, `roll_sum` and `random_between` through overlay 206 `+0000` | all |

Three more queries of the same form start at overlay 193 `+1DB0` (control the call of `+1873` at
`+1E25`), `+1873` (control `+1889`) and `+0DC7` (control `+0DD5`), each with overlay 195 `+1000`
and the script dispatcher `172C:018F` added to the cuts. From `+1DB0` the walk reads 3 routines
and reaches no target and only `+1873`; from `+1873` it reads 17 and reaches no target and only
overlay 195 `+1000`, at `+18BA`. From `+0DC7` it reads 1428 and reaches the setter, the generator,
`roll_sum` and `random_between` through the area-effect routine `+12A8` at `+0FBE`. Walks from
overlay 206 `+0000` and `31E0:0121`, without controls, reach the four through `1695:01D1`, the
event script runner FND-COMBAT-053 names, and reach `+0DC7` at `31E0:0303` and `+1DB0`.

**Arguments and entries.** `+0E1E` calls the unload with the placed index from its record, then 1,
0 and 1 at `+1023` (`0x8F969..0x8F978`), and with the index and 1, 1 and 1 at `+1067`
(`0x8F9AA..0x8F9BC`), which it reaches when the object lies outside the view its tests at
`+0FAA..+0FED` check, or, on the path from `+0F69`, when the slot's state byte at `4F49:0C33` is
1. The unload passes its
second argument and the placed entry's object slot to `31E0:044C` at `31E0:0AC9`
(`31E0:0ABC..31E0:0ACC`). FND-PARTY-134 reads the two calls of `+1DB0`: `31E0:0483`, made when the
byte `DS:265B` is not 0, the slot's state byte is 2 and the second argument is not 0, and
`31E0:0C7E`, made when the slot's state byte is 2. `+0E1E` calls overlay 197 `+30ED` with 0x120
and each index `0..3` whose row from `3143:00A8` has the removed slot in its word `+0x24F`
(FND-RNG-050). `+30ED` walks the rows `3143:000D` gives for that index and calls `+1873` with the
index and the row's byte `+0x254` for each row whose word `+0x251` is 0x120
(`0x864ED..0x8652A`). `+1873` calls overlay 195 `+1000` only for a row it finds, and with no rows
nothing (FND-COMBAT-053, FND-COMBAT-054). `+0E1E` calls `1695:02E7` with the slot and 0xF460
(`0x8F7E9..0x8F7F2`).

## Interpretation

On these walks, the unload and `2C5F:0139` can draw only by removing the effect rows of a party
member's object (state 2) through `+1DB0`, `+1873` and overlay 195 `+1000`, and overlay 197
`+30ED` only by removing the rows with word 0x120 of the index it is given, through `+1873` and
the same handler. Overlay 206 `+0A4E` has that route and the area-effect route of `+0DC7`.
`1695:02E7` reaches draws only through the script interpreter, so whether it draws depends on the
script it runs; overlay 201 `+05D5`'s other routes pass overlay 206 `+0000`, which runs event
scripts through `1695:01D1` and reaches `+0DC7` and `+1DB0`.

What overlay 195 `+1000` does for each effect byte, whether `+0DC7` takes the area-effect route
when `+0A4E` reaches it, and which script `1695:02E7` runs, were not read.

## Alternatives

- A cut walk shows that every route passes a cut routine; it does not show that a route runs.
- Whether the object the unload works on from `+0E1E` can have state 2 was not read.
- Interrupt handlers are not read, and the walks keep FND-RNG-050's assumptions.
- No emulated call or run was made; the overlays are outside the resident harness.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: for each start, copy FND-RNG-050's query with that start, its control and instruction
control, the cut routines above as `stops` (each with its reason) and as extra targets; run
`reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python
-I -m scientific_method_engine reach <config>`. `overlay_listing.py <dsun> 206 8E950 9004C`, `197
864ED 8653A` and `193 7E2F0 80ECE`; `resident_listing.py <dsun> 31E0:05BB..31E0:0640
31E0:0A90..31E0:0AE0 31E0:044C..31E0:049A`. Queries, reports and listings are kept under
`GAME_DIR/analysis/rng-routes-20261020/`. Nothing was executed.

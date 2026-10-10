---
id: FND-PARTY-075
title: The calls the generation screen's roll routine makes after the hit points reach no call of the random number generator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D5EE..0x0006D75D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:612C..1BF3:61AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000172DE..0x000172FC
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:6541..1BF3:6552
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000174C3..0x000174CB
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2E36..1000:2E5A
tool: scientific-method-engine 18.1.0 `reach` with Python 3.14.7, Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/reach_config.py with the queries q034_roll_tail_reach.json and q034_control_reach.json; overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `1000:0822` is the random number generator (FND-RNG-001), and `28C9:391D`
(`roll_sum`) calls it.

**The tail of the roll routine.** Overlay 184 `+033E`, the roll routine (FND-PARTY-071), calls
`+015D` for the hit points (FND-PARTY-074) and returns at `+06CC`. From `+055E` to there it makes
these calls and no other, in this order: `2D40:3B30` with two words of `4E68`; overlay 183
`+189B` (trampoline `56CC:003E`); overlay 184 `+06CD` (near); `2D40:3B30`; overlay 183 `+191D`
(`56CC:0043`); overlay 184 `+1748` (near) with the slot at `4E71:0B44`; `2D40:3B30`; overlay 173
`+2C00` (`5671:007A`) with that slot twice and 0; overlay 183 `+19A0` (`56CC:0048`) with its
result; `2D40:3B30`; and overlay 183 `+19E9` (`56CC:004D`). Between them it copies the details
record's word at `+0x08` to `+0x00` and the word at `+0x0C` to `+0x02` of the combatant record
when its flag is set or `DS:112E` is not -1, and stores 10 in the details record's byte at
`+0x21`, or 5 when the origin byte at `+0x12` is 8 (`+0561..+0634`).

**Values of the computed calls.** `1BF3:612C` reads a format string. It stores `1BF3:59C1` in
`1BF3:1154` when its first argument is 0 and `1BF3:5E12` otherwise (`1BF3:6135..615E`); for a
`%` it looks the next character up among the ten at `1BF3:61AE` (`duDUcsHVCP`) and calls
through the word at `1BF3:61B8` plus twice its position (`1BF3:6199`), and for any other
character calls `1BF3:1154` (`1BF3:61A0`). The ten words are `1BF3:624E`, `6227`, `626E`,
`628E`, `61CC`, `61DB`, `61F8`, `6207`, `6216` and `62C2`, as FND-PARTY-039 gives. `1BF3:6541`
calls through the word at `1BF3:6393` plus `DS:106E` times 2 masked with 7 (`1BF3:654E`), so
one of the four words there, `1BF3:659B`, `65DA`, `65BE` and `6600` (FND-PARTY-043).

**The runs.** `reach_config.py` builds each configuration as in FND-PARTY-040 (205 regions,
7,562 far transfers, 157 jump tables), with the target `1000:0822`.

1. `q034_roll_tail_reach.json`. Starts: the nine routines of the tail (`2D40:3B30`, overlay 183
   `+189B`, overlay 184 `+06CD`, overlay 183 `+191D`, overlay 184 `+1748`, overlay 173 `+2C00`,
   overlay 183 `+19A0` and `+19E9`), the 12 formatter values (`1BF3:59C1`, `1BF3:5E12` and the
   ten table words) and the four `1BF3:6393` values. Leaf: the exit worker `1000:0388`, which
   runs only as the program ends (FND-PARTY-039, FND-PARTY-043). Control: the far call at
   `0x6C929` (overlay 183 `+1949`). It reads 123 routines and 11,491 instructions without
   stopping at its limit, reaches the control and the leaf, and does not reach `1000:0822`. It
   reports no gap and no contested instruction, one interrupt (`int 0x21` at `1000:2E53`), and
   nine unresolved calls: `0x172C9` and `0x172D0` in `1BF3:612C`, `lcall [0x1154]` at
   `0x17303`, `0x1731C` and `0x173E6` in the formatter routines `1BF3:61CC`, `61DB` and `624E`,
   `0x1767E` in `1BF3:6541`, `lcall es:[si+0xc]` at `0x16E3E` (`1BF3:5D0E`), and the `APFM`
   callback calls `0x332E7` and `0x334F7` (`3D72:09C7`, `3D72:0BD7`).
2. `q034_control_reach.json`. Start: overlay 183 `+1524`, the routine that rolls one score
   (FND-PARTY-071). No leaves or controls. It reads 5 routines and 181 instructions and reaches
   `1000:0822` in two calls, through the call at `0x6C57D` (overlay 183 `+159D`) into
   `28C9:391D` and its call at `0x217C7`, with nothing unresolved.

The leaf is reached from `1000:03EE` (call at `0x55FB`), which the run reaches from the routine
at `1000:2E48`: it prints the string at `1000:2E36`, `Stack overflow!`, through interrupt `0x21`
function 9 and jumps to `1000:03EE`, which calls the exit worker with 1 and 0.

## Interpretation

None of the nine calls after the hit points draws a random number: the reading reaches
`1000:0822` from none of them, while the same configuration reaches it from the score routine in
two calls. Each call the run leaves unresolved either has its values among the starts (the
formatter's `1BF3:1154` and table calls, the `1BF3:6393` table) or is skipped while the screen
runs: `0x16E3E` from the graphics set-up `39D1:0009` on, which runs before any game screen
(FND-PARTY-043), and the two `APFM` callbacks because their fields are never set
(FND-PARTY-044). The one route to the exit worker is the run-time library's stack overflow abort,
after which nothing more is drawn. So a roll makes its draws for the scores and then for the hit
dice, and no further draw follows them before the routine returns.

## Alternatives

- A computed call has a value outside those listed: the formatter's two pointers and two tables
  are read from the build's bytes, and `1BF3:1154` is written on the same path just before the
  calls, so a stale value would need a path that calls a formatter routine without `1BF3:612C`.
- The routines run with state the reading does not model and call a routine absent from the
  inventory: the run reports no gap, so every instruction it reached decodes inside a row.
- The interrupt at `1000:2E53` returns: it is DOS function 9, which prints a string, on the stack
  overflow path that ends the program.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 184
6D5EE 6D75D`; `trampoline_target.py <dsun> 56CC:003E 56CC:0043 56CC:0048 56CC:004D 5671:007A`;
`resident_listing.py <dsun> 1BF3:612C..1BF3:61AE 1BF3:6541..1BF3:6560 1000:0388..1000:0400
1000:2DC0..1000:2E60`; and `direct_callers.py <dsun> 1000:0388 1000:03EE`. Read the ten bytes at
`1BF3:61AE`, the ten words at `1BF3:61B8` and the four at `1BF3:6393`. Then, in the repository
root, for `q034_roll_tail_reach.json` and `q034_control_reach.json` run `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/<query>` and `python -I -m scientific_method_engine reach <config>`.

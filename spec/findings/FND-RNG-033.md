---
id: FND-RNG-033
title: DONE's finishing routine reaches no generator or reducer call before its Preserver spell picks, except through the score roll it runs only for a character with no first class
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006E0B9..0x0006E170
tool: tools/research/exec-census/reach_config.py and overlay_listing.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.7, xxhash 4.0.1; scientific-method-engine 19.10.0 reach
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 184's code starts at file `0x6D090`.
DONE runs overlay 184 `+1029` with its flag 1 (FND-PARTY-070). Decoding `+1029` from its entry to
the call of `+1980` at `+10DD`, the routine RULE-PARTY-016's picks are in (FND-RNG-029), gives
these calls, in order:

| Site | Callee | Condition |
|---|---|---|
| `+1046` | overlay 186 `+0572` (trampoline `56E9:0052`) | the flag is not 0 |
| `+1067` | overlay 183 `+0A3E` (`56CC:0061`) with 3, 1 and 1 | byte `+0x1B` of the working record at the far pointer `DS:1429`, its first class byte, is 0 |
| `+10C7` | overlay 183 `+0871` (`56CC:00DE`) | none |
| `+10CF` | `+1648` | none |
| `+10D6` | `+1876` | none |

Between them the routine stores to memory only: 0xFF in `DS:1C96`, 0x80 in `DS:42C0` when that
word is 0, and the copies FND-PARTY-070 lists (`+1034..+10C4`).

A `reach` query with the starts overlay 186 `+0572`, overlay 183 `+0871`, overlay 184 `+1648` and
`+1876`, and the targets `1000:0822` (the generator), `2834:061C` (`random_mod`), `2834:0519`
(`chance_in_ten`), `28C9:391D` (`roll_sum`) and `2D40:3A03` (`random_between`), reaches none of the
five. Its control, the call of overlay 210 `+0365` at overlay 184 `+1753` in `+1648`, is reached,
and the instruction control overlay 210 `+0365` decodes. The walk reads 198 routines and 14,855
instructions with no unresolved transfer, gap or contested instruction, and the engine reports
`negativeUsable` true. The query declares, from earlier findings:

- the leaf `1000:0388`, the exit worker (FND-PARTY-039, FND-PARTY-043);
- the near call `1000:0A37` with the targets `1000:0FB5`, `1000:345B` and `1000:3726`
  (FND-EXE-580);
- the far calls `3D72:09C7` and `3D72:0BD7` as never run (FND-EXE-588);
- the far call `1000:0346` with the target overlay 180 `+0848` (FND-EXE-579);
- the jump `1000:252C` through the one word at `DS:398C` (FND-EXE-579);
- the call `1000:02C1` as not returning (FND-EXE-578).

The same query with the one start overlay 183 `+0A3E` and the control `+0C89` reaches `roll_sum`
and the generator, through overlay 184 `+033E` (the roll routine of FND-PARTY-071) and overlay 183
`+1524`, whose `roll_sum` call is at `+159D`.

## Interpretation

For a character whose first class byte is set, DONE's finishing routine makes no draw before
`+1980`, so RULE-PARTY-016's four picks are the first draws `+1029` makes. The one route to a
draw before them is `+0A3E`, the score roll, run only for a character with no first class.

## Alternatives

- The query assumes each call returns and does not read interrupt handlers; a draw from an
  interrupt between two of these calls would not be found.
- Whether DONE can run with the first class byte 0 rests on FND-PARTY-070, which reads it as
  ruled out by DONE's state; that state was not read again here.
- What the button handler does before calling `+1029` (FND-PARTY-070's `+0E75`) was not walked.
- No emulated call or run was made.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 184
6D090 6F197` and read `+1029..+10E0`; write a query with the starts, targets, controls and
declarations above, run `reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
<config> <query>` and `python -I -m scientific_method_engine reach <config>`; then again with the
one start `183+0A3E` and the control `183+0C89`. Queries and reports are kept under
`GAME_DIR/analysis/rng-done-20261020/`. Nothing was executed.

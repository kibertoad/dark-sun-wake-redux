---
id: FND-PARTY-200
title: From the title screen's windows a reach finds three of the seven stores that make the fight state nonzero; two lie behind the fight-state tests of 2D40:10AE, and the third, overlay 193 +0533, only on the USE window's route
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/reach_config.py, disassembly of the load image at segment 1000); scientific-method-engine 19.7.0 reach
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1C36..2D40:1C69
---

## Observation

FND-PARTY-089 lists seven stores of a nonzero value to the fight state at `4C10:0019`:
`2E00:0837`, overlay 173 `+22B7` and `+409D`, overlay 188 `+04CF`, overlay 193 `+0533`, and the
stores of overlay 173 `+059A`'s result at `2A00:0DF0` and `2B00:0DDE`. `2E00:0837` is the
instruction at `2D40:1437` (the same linear address).

`q079_fight_state_reach.json` takes the starts, leaves and instruction control of
FND-PARTY-197's `q073_mark_stores_reach.json` with these seven sites as targets. The engine reads
1,226 routines, leaves 66 transfers unresolved and reports `negativeUsable` false. It reaches
three targets, all from overlay 212 `+0967`:

| Target | Calls | Routines every route passes |
| --- | --- | --- |
| `2D40:1437` | 3 | overlay 212 `+0967`, `2D40:10AE` |
| overlay 173 `+22B7` | 4 | overlay 212 `+0967`, `2D40:10AE`, overlay 173 `+1CC1` |
| overlay 193 `+0533` | 5 | overlay 212 `+0967`, overlay 193 `+003C` |

and not `+409D`, overlay 188 `+04CF`, `2A00:0DF0` or `2B00:0DDE`.

In `2D40:10AE`, `2D40:1437` lies after the test at `2D40:1197` (FND-PARTY-199). A search of the
file for the far call `9A 57 00 71 46` (trampoline `4671:0057`, the attack routine overlay 173
`+1CC1`) finds it at `2D40:13D9`, `2D40:1C61` and at file `0x20F97`, outside segment `2D40`. The
call at `2D40:1C61` follows its own test of the word at `4C10:0019` (`2D40:1C36..2D40:1C43`), which
jumps to `2D40:1D07` when the word is 0.

## Interpretation

On the routes the walk finds, `set_leader` can make the fight state nonzero only through code
that already needs it nonzero, so it does not start a fight on the title screen's windows. The
one store reached on another route is overlay 193 `+0533`, behind the USE window that the key `c`
opens there (FND-PARTY-198). The walk misses routines reached only through registered handlers
(FND-PARTY-197), so the unreached stores are not ruled out.

## Alternatives

- `throughEveryRoute` names routines, not call sites, so that every route to overlay 173 `+22B7`
  passes the gated part of `2D40:10AE` rests on the call search above, which finds only the two
  calls of `4671:0057` in that segment, each after a test of the fight state.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, in `tools/research/exec-census/`: `reach_config.py <dsun>
../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv OUT q079_fight_state_reach.json`, then
`python -I -m scientific_method_engine reach OUT` and read each target's `reached`, `calls` and
`throughEveryRoute`. Search the file for the bytes `9A 57 00 71 46`, and disassemble the load
image (file offset of `S:O` is `(S - 0x1000) * 16 + O + 0x5200`) from `2D40:1C36` to `2D40:1C69`.

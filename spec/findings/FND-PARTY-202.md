---
id: FND-PARTY-202
title: Overlay 179 +0D2F calls the damage routine itself only for a target whose slot kind is 1, and reaches overlay 173 +22C4's other callers in overlay 179 through its near calls of +1373 and +2A05
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
tool: Python 3.14.7 with Capstone 5.0.7 (tools/research/exec-census/overlay_listing.py, reach_config.py); scientific-method-engine 19.7.0 reach
environment: null
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006500F..0x0006508E
---

## Observation

Overlay 179's code starts at file `0x642E0`. Overlay 179 `+0D2F` (trampoline `56A7:006B`) takes
its first argument in `di` and its third in `si`. It returns at once (to `+11D5`) when the word at
`DS:2588` plus twice the byte at `+0x0F` of the combatant record of `di`'s slot is -1
(`+0D45..+0D6E`) or when `si` is `0x134` (`+0D71..+0D77`). When the kind byte at `4F49:0C33 + 3 *
di` is 1 (`+0D7A..+0D8A`) and the word its fifth and sixth arguments point to (the far pointer at `[bp+0xE]`) is
above 0, it calls overlay 173 `+22C4` with `di` and that word (`+0D8C..+0D9C`), and in either case
returns. The only far call to `5671:005C` (`+22C4`) between `+0D2F` and the routine's end at
`+11DC` (the inventory row `0x6500F`) is at `+0D9C`.

`q073_hit_routine_reach.json` starts at `179+0D2F` with the five overlay 179 calls of `+22C4`
that FND-PARTY-197's `direct_callers.py` listed and those of overlays 195 and 204 as targets. The
engine reads 934 routines, leaves 41 transfers unresolved and reports `negativeUsable` false. It
reaches `+0D9C` in `+0D2F` itself, `+1D22`, `+27BF` and `+27D3` through the near call at `+1134`
to `+1373`, and `+2A3A` through the near call at `+1078` to `+2A05`, each in one call with those
routines on every route; it does not reach the calls in overlays 195 and 204.

FND-PARTY-086 reads `+22C4` as acting only on a slot whose state byte, the same kind byte, is 2.

## Interpretation

The damage call at `+0D9C` is for kind-1 slots, which `+22C4` then leaves alone, so it cannot
lower a party member's hit points or mark. On the USE window's route a party member can be
damaged only through `+1373` or `+2A05`.

## Alternatives

- What kind 1 is was not read. A party slot holding a character had kind 2 in FND-PARTY-192's
  run.
- The conditions in `+0D2F` before `+1078` and `+1134`, and in `+1373` and `+2A05` before their
  damage calls, are not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the
installed `DSUN.EXE`, in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 6500F
650B0`; `reach_config.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv OUT
q073_hit_routine_reach.json`, then `python -I -m scientific_method_engine reach OUT` and read each
target's `reached`, `chain` and `throughEveryRoute`.

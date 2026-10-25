---
id: FND-COMBAT-073
title: Each pass of the main loop, 2D40:3EBF gives party slots 0 to 3 one psionic point for every 300 clock units since its last run, up to details word +0x0C, and the same in hit points up to the maximum to a member whose Constitution byte is 20 or more
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3EBF..2D40:400C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:056F..277B:0579
tool: tools/research/exec-census/resident_listing.py, direct_callers.py and record_base_accesses.py at the commit that adds this finding, with coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv as the inventory; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Combatant records are the 49-byte records under `DS:19C9` (FMT-COMBAT-001) and details records
the 66-byte records under `DS:19C5` (FMT-COMBAT-002). `4C13:035B` is the double-word clock that
the timed queues run against (FND-COMBAT-060), and `DS:19B8` the copy of it that a load sets
(FND-SAVE-015).

**`2D40:3EBF`** takes no argument. With `e` the clock less `DS:19B8` as signed 32-bit numbers, it
returns at once when `e` is 300 or less (`2D40:3EC2..+3ED9`). Otherwise, for `cx` from 0 to 3
(`2D40:3EDC..+3FF9`), when the kind byte at `4F49:0C33 + 3 * cx` is 2:

1. it adds `e / 300` (unsigned division, low word) to word `+0x02` of combatant record `cx`, and
   when the sum is above word `+0x0C` of details record `cx` (unsigned) stores that word instead
   (`2D40:3EF3..+3F6C`);
2. when byte `+0x1B` of combatant record `cx` is 20 or more (unsigned) it adds `e / 300` to word
   `+0` and, when the sum is above word `+0x08` of details record `cx`, stores that word instead
   (`2D40:3F6C..+3FF0`).

After the loop it copies the clock to `DS:19B8` (`2D40:3FFC..+4006`). It indexes both tables by
the slot number.

`direct_callers.py <dsun> 2D40:3EBF` finds one call, at `277B:0574` in the loop of `277B:0024`
(FND-IMAGE-012), right after `28C9:2322`. `record_base_accesses.py <dsun> <inventory> --source
19C9 2` lists 21 accesses to combatant word `+0x02`; besides this routine they are in overlays
174, 176, 198, 204 and 213.

## Interpretation

Combatant word `+0x02` is the current psionic strength points and details word `+0x0C` the
greatest, as the `CHAR` load's copy already shows (FMT-PARTY-001 `psionic_points`,
`max_psionic_points`, FND-PARTY-059): this routine raises the one up to the other. Byte `+0x1B`
is Constitution (FND-MAGIC-013), so a party member with Constitution 20 or more also regains hit
points over time, up to `max_hit_points`.

The remainder is lost: the routine acts only once more than 300 units have passed, gives `e /
300` rounded down, and then restarts the count from the full clock, so 599 units give 1 point. It runs on every pass of the main loop, in or out of a fight, and tests
neither `combat_mark` nor `combat_state`; a member at mark 3 with Constitution 20 regains hit
points but keeps mark 3, which only the routines of FND-COMBAT-071 change.

## Alternatives

- The unit of `4C13:035B` is open (FND-MAGIC-008); with a combat round worth 60 (FND-RNG-076), one
  point comes every five rounds of a fight.
- Whether a load or the start of a game sets `DS:19B8` before the first pass, so that the first
  run does not give the whole clock as points, rests on FND-SAVE-015's copy at load and was not
  read for a new game.
- Only party slots 0 to 3 are visited; slot 4 and creatures regain nothing here.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python, `<dsun>` the installed `DSUN.EXE` and
`<inv>` `../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: `resident_listing.py <dsun>
2D40:3EBF..2D40:4016 277B:0560..277B:0580`; `direct_callers.py <dsun> 2D40:3EBF`;
`record_base_accesses.py <dsun> <inv> --source 19C9 2`.

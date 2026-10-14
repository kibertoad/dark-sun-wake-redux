---
id: FND-PARTY-179
title: After overlay 182 +1D3E stores the new leader it sets turn_combatant, and unless every member is shown swaps the two leaders' visibility and positions, and calls 2C5F:0139 and 28C9:2A81 only when its second argument is not 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A5D2..0x0006A6D1
tool: tools/research/exec-census/overlay_listing.py (Capstone 5.0.9)
environment: null
---

## Observation

Overlay 182's code starts at file offset `0x68850`; `+x` is file offset `0x68850 + x`. Segment
words are resolved through the overlay's fixups: `0x02E8` is `4C13`, `0x0388` is `4F49`, `0x00C8`
is `2D40`, `0x00F0` is `31E0`, `0x00B8` is `2C5F` and `0x00B0` is `28C9`.

FND-PARTY-171 reads `+1D3E` up to its guards: with the slot `si` (`[bp + 6]`) below 4, not equal
to `leader`, and the combatant `object_slot_combatants[si]` names having `combat_mark` at most 2,
it goes on at `+1D82`. From there:

1. It keeps the old `leader` in `di`, calls `2D40:3D4C` with the old leader and `si`, which stores
   `si` to `leader` (FND-PARTY-171), and copies `leader` to `turn_combatant` (`DS:426D`)
   (`+1D82..+1DA5`).
2. When the byte at `DS:13FA` is not 0 it goes to `+1E63` (`+1DA8..+1DB1`).
3. Otherwise, with `new` the `object_slots` entry of `si` and `old` that of `di` (`DS:67BB +
   0x25 * slot`): it clears bit 7 of `new`'s byte 0 and sets bit 7 of `old`'s (`+1DD0..+1DDA`);
   calls `31E0:4129` with `si` and `old`'s words `+0x0A` and `+0x0C`, then with `di`, `0x0840`
   and `0x0660` (`+1DDE..+1DFB`); stores `new`'s words `+0x0A` and `+0x0C`, each shifted right by
   4, to the words at `4F49:08A3 + 0x13 * si` and `4F49:08A5 + 0x13 * si`, and `0x000F` to the word
   at `4F49:08A7 + 0x13 * si` (`+1DFE..+1E3E`); and calls `2D40:10AE` with `si` and 1
   (`+1E45..+1E4D`).
4. When the byte argument `[bp + 8]` is not 0, it calls `2C5F:0139` with no arguments and
   `28C9:2A81` with 1 (`+1E50..+1E62`).
5. At `+1E63`, when the byte at `DS:1437` is not 0, it calls `+056F` of the same overlay with 4,
   4, 16 and 16 (`+1E63..+1E7A`). It returns at `+1E80`.

Its callers pass 1 from the keys 1 to 4 (`28C9:0EB7`, FND-PARTY-176) and 0 from overlay 173
`+3DF7` (FND-PARTY-175).

## Interpretation

A leader change also makes the new leader the `turn_combatant`. With the party shown as the leader
alone, the new leader takes the old one's place and is shown, and the old leader is hidden and
moved to `0x0840`, `0x0660`, the position the party loader gives hidden members
(FND-PARTY-013). The second argument decides only whether `2C5F:0139` and `28C9:2A81(1)` run, the
pair key 5 calls after showing the whole party (FND-EXPLORE-005); a key passes 1, START GAME's
party restore 0.

## Alternatives

- `31E0:4129`, `2D40:10AE`, `2C5F:0139`, `28C9:2A81` and `+056F` were not read, so what the
  position call and the two flagged calls change on screen is not shown. Byte `DS:1437` is the
  setting button 16303 toggles (FND-CONFIG-010).

## How to reproduce

From the commit that adds this finding, run
`tools/research/exec-census/overlay_listing.py <dsun> 182 68850 6AD08` on the installed
`DSUN.EXE` and read `+1D82..+1E80`.

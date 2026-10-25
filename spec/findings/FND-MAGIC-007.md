---
id: FND-MAGIC-007
title: The 34 rows of psionic_powers keep the manual's ability, adjustment and initial cost of every power the manual prices with a number, hold 0 for Enhanced Strength, Domination and Mass Domination, whose cost overlay 176 +0196 computes from the target, and 5 for Superior Invisibility, and their third byte is the maintenance per round, 99 for none, which differs from the manual for ten powers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000478F0..0x00047956
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00061703..0x00061794
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000617DA..0x00061814
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:37C1..2D40:37F3
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1; a short read of the 102 bytes at file 0x478F0 and of the names at +0x20 of DATA resources 235 to 268 of RESOURCE.GFF
environment: null
---

## Observation

Segment `51F1` starts at file `0x47110` (FND-PARTY-106, FND-MAGIC-004), so `psionic_powers`,
three bytes per power at `51F1:07E0 + 3 * p` (FND-RNG-046), is file `0x478F0..0x47956`. Byte 0
holds `score_code` in bits 0 to 2 and `adjust` as the signed byte shifted right three; byte 1 is
`cost`; byte 2 is `byte_2` (glossary `psionic_powers`). `score_code` names the byte
`+0x19 + score_code` of the combatant record (FND-RNG-042), where `+0x1B` is constitution,
`+0x1C` intelligence and `+0x1D` wisdom (FND-PARTY-161), so codes 2, 3 and 4 are those three
scores. The name of power `p` is the text at `+0x20` of `DATA` resource `235 + p` of
`RESOURCE.GFF` (FND-EXE-611 `+0AF8`); the 34 names run in the manual's printed order, Detonate
(`DETONATE`) to Thought Shield (`THOUGHT SHIELD`), with these spellings that differ from the
manual's: 7 `ENERGY CONTROL` (Energy Containment), 8 `LIFE DRAIN` (Life Draining), 13 `CELLULAR
ADJUSTMENT` (Cell Adjustment), 29 `PSIONIC MIND BAR` (Mind Bar).

The rows, with the manual's values from `RULE-MAGIC-003.powers.csv`:

| p | Power | Score | Adjust | Byte 1 | Byte 2 | Manual initial / maintenance |
| --- | --- | --- | --- | --- | --- | --- |
| 0 | Detonate | constitution | -3 | 18 | 99 | 18 / NA |
| 1 | Disintegrate | wisdom | -4 | 40 | 99 | 40 / NA |
| 2 | Project Force | constitution | -2 | 10 | 99 | 10 / NA |
| 3 | Ballistic Attack | constitution | -2 | 5 | 99 | 5 / NA |
| 4 | Control Body | constitution | -2 | 8 | 99 | 8 / 8 |
| 5 | Inertial Barrier | constitution | -3 | 7 | 5 | 7 / 5 |
| 6 | Animal Affinity | constitution | -4 | 15 | 4 | 15 / 4 |
| 7 | Energy Containment | constitution | -2 | 10 | 99 | 10 / NA |
| 8 | Life Draining | constitution | -3 | 11 | 99 | 11 / 5 |
| 9 | Absorb Disease | constitution | -3 | 12 | 99 | 12 / NA |
| 10 | Adrenalin Control | constitution | -3 | 8 | 4 | 8 / 4 |
| 11 | Biofeedback | constitution | -2 | 6 | 3 | 6 / 3 |
| 12 | Body Weaponry | constitution | -3 | 9 | 4 | 9 / 4 |
| 13 | Cell Adjustment | constitution | -3 | 5 | 99 | 5 / 0 |
| 14 | Displacement | constitution | -3 | 6 | 3 | 6 / 3 |
| 15 | Enhanced Strength | wisdom | -3 | 0 | 8 | Varies / Varies |
| 16 | Flesh Armor | constitution | -3 | 8 | 4 | 8 / 4 |
| 17 | Graft Weapon | constitution | -5 | 10 | 1 | 10 / 1 |
| 18 | Lend Health | constitution | -1 | 4 | 99 | 4 / NA |
| 19 | Share Strength | constitution | -4 | 6 | 2 | 6 / 2 |
| 20 | Domination | wisdom | -4 | 0 | 99 | Varies / Varies |
| 21 | Mass Domination | wisdom | -6 | 0 | 99 | Varies / Varies |
| 22 | Psychic Crush | wisdom | -4 | 7 | 99 | 7 / NA |
| 23 | Superior Invisibility | intelligence | -5 | 5 | 5 | Varies / 5 |
| 24 | Tower of Iron Will | wisdom | -2 | 6 | 0 | 6 / NA |
| 25 | Ego Whip | wisdom | -3 | 4 | 99 | 4 / NA |
| 26 | Id Insinuation | wisdom | -4 | 5 | 99 | 5 / NA |
| 27 | Intellect Fortress | wisdom | -3 | 4 | 0 | 4 / NA |
| 28 | Mental Barrier | wisdom | -2 | 3 | 0 | 3 / NA |
| 29 | Mind Bar | intelligence | -2 | 6 | 4 | 6 / 4 |
| 30 | Mind Blank | wisdom | -7 | 0 | 0 | 0 / 0 |
| 31 | Psionic Blast | wisdom | -5 | 10 | 99 | 10 / NA |
| 32 | Synaptic Static | intelligence | -4 | 15 | 10 | 15 / 10 |
| 33 | Thought Shield | wisdom | -3 | 1 | 0 | 1 / NA |

Score and adjustment agree with the manual for all 34 powers, and byte 1 with every initial cost
the manual prints as a number. Byte 2 is 99 for every power the manual gives NA except 24, 27, 28
and 33, which hold 0.

**The computed costs.** Overlay 176 `+0196` sets its cost `c` (`+01C3..+0254`): for `p` 15, the
byte `+0x15` of the details record of the target slot `t`'s combatant, subtracted from 24 and
doubled; for `p` 20 or 21, 3, 8, 13 or 18 as `27E5:0154` of `t` is below 6, below 11, below 16 or
more (signed); otherwise byte 1; then 0 when `c` is negative (FND-RNG-046 step 3). The details
byte `+0x15` is the stored strength, the byte the rest copies into the combatant's score byte
`+0x19` (FND-MAGIC-005, FND-PARTY-070). Step 5's refund for a failed check is byte 1 halved
(FND-RNG-046), so it is 0 for powers 15, 20 and 21.

**Byte 2.** A successful use by a party member sets bit 0 of its `psionic_ranks` byte unless
byte 2 is 99 (`+02AC..+02CC`, FND-PARTY-210). `+0134` keeps a power in effect only when byte 2 is
at most the member's points, and `+03CD` takes byte 2 from the points each time it keeps it
(FND-MAGIC-008). `2D40:37C1` takes a slot and clears bit 0 of the slot's byte for each power whose
byte 2 is above 0 (unsigned), leaving the five powers whose byte 2 is 0
(`2D40:37C4..2D40:37F1`).

**Readers and writers.** `immediate_search.py <dsun> 07E0 07E1 07E2` finds memory operands with
these displacements only in overlay 176 (`+00ED`, `+0168`, `+0245`, `+02AC`, `+02FD`, `+049C`,
`+0750`, `+0765`, `+086F`, `+08DF`, `+0900`, `+09F4`, `+0A19`, `+0A3F`, each a read) and at
`30BD:0000`, which is `2D40:37D0` written in another segment; every other hit (`2401:000A`,
`3044:0000`, `3049:0006`, `304B:0002`, `34AF:000E`, `34B1:0006`, overlay 177 file `0x62646`,
overlay 179 files `0x66092` and `0x6612E`, overlay 193 file `0x7FEE1`) starts one byte before an
`and cl, 7` or `and cx, 7` and is not an instruction of the code. None writes. FND-MAGIC-004 found
no store into segment `51F1` near a load of the segment.

## Interpretation

The game keeps the manual's power scores and initial costs. Byte 2 is the maintenance cost per
round, and 99 is the game's NA: such a power is never marked in effect, so it is never kept. The
game prices the four Varies powers this way:

- Enhanced Strength: initial cost twice the points that would bring the target's strength to 24,
  0 when it is 24 or more; maintenance 8 a round, whatever the strength.
- Domination and Mass Domination: initial cost 3 for a target of level 1 to 5, 8 for 6 to 10,
  13 for 11 to 15 and 18 above, by `27E5:0154` (the target's greatest level, or a human's level
  in position 0, FND-PARTY-081); never maintained.
- Superior Invisibility: initial cost 5 and maintenance 5, with no count of creatures.

A failed check of Enhanced Strength, Domination or Mass Domination costs nothing.

The maintenance differs from the manual for ten powers. Control Body (8) and Life Draining (5)
cannot be maintained; Cell Adjustment (0) cannot be maintained; Enhanced Strength costs 8,
Domination and Mass Domination cannot be maintained; and Tower of Iron Will, Intellect Fortress,
Mental Barrier and Thought Shield, NA in the manual, are maintained at 0 like Mind Blank. Those five
are the defence modes RULE-MAGIC-015 scans. `2D40:37C1` ends every maintained power except these
five.

## Alternatives

- That 24 in the Enhanced Strength cost is the strength the power gives rests on the formula's
  shape; what `DATA` 250's effect sets was not read.
- That the details byte `+0x15` is strength rests on FND-PARTY-161's order of the scores at
  combatant `+0x19` and on the rest's copy; a run that reads both for a known character settles
  it.
- The four rows that differ in byte 2 from the manual could be data errors in either source; the
  code treats byte 2 the same for every power, so the game's behaviour follows the bytes.
- The rows could be overwritten at run time through an address formed without these
  displacements; no such store was found and the readings above read the load-time bytes.

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: read the 102 bytes at file `0x478F0` in rows of three; `overlay_listing.py <dsun> 176
61540 62067` and keep `+01C3..+0254` and `+029A..+02D4`; `resident_listing.py <dsun>
2D40:37C1..2D40:37F6`; `immediate_search.py <dsun> 07E0 07E1 07E2` and check each hit's bytes
(file offset of `SSSS:OOOO` is `(SSSS - 0x1000) * 16 + OOOO + 0x5200`). Read the names at `+0x20`
of `DATA` resources 235 to 268 of `RESOURCE.GFF` through its directory as `data_bytes.py` reads
it. Compare with `RULE-MAGIC-003.powers.csv`.

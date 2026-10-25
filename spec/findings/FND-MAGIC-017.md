---
id: FND-MAGIC-017
title: Overlay 193 +1A79 is the call in +1A2D that sums overlay 211 +1FB5 of each matching effect row's source slot and number, so an effect's strength in nine callers follows the source's class level now, a ranger's less 7; overlay 197 +14DA is the call in +111A that ends a hit of number 115 or more on a target with special attack byte 9 when +1FB5 of the slot in DS:426D, not the hit's caster, is below 7
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FD1D..0x0007FD94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097DC5..0x00097E9C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000848AE..0x000848EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085BEC..0x00085C1A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085C40..0x00085C65
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00085C99..0x00085CB8
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x7E2F0 + x` for overlay 193, `0x95E10 + x` for 211 and
`0x83400 + x` for 197. Effect rows are as in FND-MAGIC-015. `direct_callers.py <dsun> 211+1FB5`
lists the far calls at overlay 193 `+1A79` and overlay 197 `+14DA`; neither offset starts a
routine, and `direct_callers.py <dsun> 193+1A79 197+14DA` finds no caller of either.

**`+1A79`** lies in overlay 193 `+1A2D` (`+1A2D..+1AA4`, FND-EXE-606). `+1A2D` takes a slot and a
code byte, starts at `3143:0061` of the two (`+1A32..+1A41`) and, while the row belongs to the slot
and its index is below `DS:2100` (`+1A84..+1A9C`), adds to its sum, for each row whose code byte
is the code, overlay 211 `+1FB5` of the row's word `+2`, the source slot, and its word `+4`, the
number (`+1A47..+1A81`). It returns the sum.

**`+1FB5`** (`+1FB5..+208C`) takes a slot and a number `n`. For `n` below 0xEB (signed) it returns
the greatest, over class codes 0 to 19 that overlay 177 `+0A45` accepts for `n`, of `27E5:00C1`
of the slot and the code, less 7 for codes 13 to 16, and 0 when none is above 0
(`+1FBD..+1FFF`). For `n` below 0x10D it returns `27E5:00C1` for code 12 (`+2002..+2018`); for
0x132 the greatest over codes 1 to 4 (`+201A..+2040`); for any other `n` the greatest level byte
`+0x1E..+0x20` of the slot's details record (`+2042..+2084`). FND-MAGIC-003 reads the same.

`direct_callers.py <dsun> 193+1A2D` finds nine far calls. The code each pushes and what it does
with the sum `S`:

| Call | Code | Use of `S` |
| --- | --- | --- |
| overlay 179 `+1DF1` | its byte `bp-2`, each effect in turn | subtracts `5 * S` from a value built as `50 + 5 * x` (`+1DE5..+1DFD`), the removal threshold of FND-RNG-034 |
| overlay 195 `+0385` | 0x10 | pushes `S` with 0xBC (188) to the next call (`+0382..+038E`), FND-RNG-036 |
| overlay 195 `+14A6` | its argument byte `bp+8`, which is 1 on that branch (`+1497..+149D`) | pushes `S` with 0x63 (`+14AE..+14AF`) |
| overlay 197 `+27F1` | 0x68 | stores `5 - (S - 5) / 3` for `S` above 5, else 5, at `bp-4` (`+27F9..+2814`) |
| overlay 197 `+2845` | 6 | lowers the word at `[di]` to `6 - S / 4` (`+284D..+2863`) |
| overlay 197 `+289E` | 0x28 | lowers the word at `[di]` to `10 - S` (`+28A6..+28B6`) |
| overlay 197 `+2FDF` | 0x20 | multiplies `S` by 10 (`+2FE7`) |
| overlay 204 `+0B36`, `+0B51` | 0x20 | `10 * S + 100` scales a modifier (`+0B3E..+0B44`, `+0B59..+0B5C`), FND-EXE-626 |

**`+14DA`** lies in overlay 197 `+111A`, which ends a hit when it returns 1 (FND-PARTY-112) and
takes no caster argument. At `+14AE..+14E7` it reads byte `+0x0E` of the target's combatant record
(the target `DI`, numbered by the word at `4F49:0C34 + 3 * DI`). When the byte is 9 it jumps to
`+1162`, which returns 1, for a number `SI` below 0x73 (`+14CD..+14D2`); for 0x73 or more it calls
`+1FB5` with the word `DS:426D` and `SI` (`+14D5..+14DA`) and jumps to `+1162` when the result is
below 7 (`+14E2..+14E7`). FND-RNG-055 reads byte `+0x0E` as the special attack byte that overlay
174 `+07ED` passes to overlay 198 `+0149`. `DS:426D` is the slot word the leader change sets
(FND-COMBAT-023) and that combat copies from `4C13:0369` (FND-COMBAT-052).

## Interpretation

`+1A79` makes the strength of an effect a sum, over the target's rows of one code, of each row's
source level as `+1FB5` gives it at the time of the read. The level is not stored in the row: a
row cast by a ranger counts the ranger's level less 7 for a priest spell, a source whose level
changes later counts the new level, and a number of 0x10D or more counts the source's greatest
level byte or, for 0x132, its greatest cleric level. The cast itself carries the full level or 6
(FND-MAGIC-003, FND-MAGIC-016); the other near callers of `+1FB5` in overlay 211 (`+08E5`,
`+1096`, `+1655`, `+1725`, `+18B0`, `+20A7`, `+20BE`) were not all read here.

`+14DA` makes a target with special attack byte 9 immune to every number below 115, the wizard
spells, and to every number from 115 unless the slot in `DS:426D` reaches 7 by `+1FB5`. For a
priest spell a ranger reaches 7 at ranger level 14. The test reads `DS:426D` instead of the hit's
caster: for a hit whose caster is not that slot (a monster's cast, an item, a queued cast) the
level tested is someone else's.

## Alternatives

- What effect codes 0x68, 6, 0x28 and 0x20 are, and what the callers' results feed, was not read
  past the arithmetic in the table.
- That `DS:426D` holds the acting combatant during a fight rests on FND-COMBAT-052's copy from
  `4C13:0369`; whether it can differ from the hit's caster for a party cast was not followed.
- Which combatants have special attack byte 9 was not read (FND-RNG-055 leaves the monster
  records' byte open).

## How to reproduce

From `tools/research/exec-census/`: `direct_callers.py <dsun> 193+1A79 197+14DA 211+1FB5
193+1A2D 197+111A`; `overlay_listing.py <dsun> 193 7FC90 7FDD0`, `... 211 97DC5 97E9C`,
`... 197 84700 84960`; and around each caller of `+1A2D`: `... 179 660B8 660E0`,
`... 195 81AE0 81B04`, `... 195 82C00 82C25`, `... 197 85BE0 85C00`, `... 197 85BF9 85C1A`,
`... 197 85C30 85C54`, `... 197 85C4D 85C65`, `... 197 85C88 85CAD`, `... 197 85CA6 85CB8`,
`... 197 863C8 863EE` and `... 204 8C8E0 8C920`.

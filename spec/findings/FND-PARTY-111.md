---
id: FND-PARTY-111
title: Overlay 182 +0B52 sets a party member's spells to cast per level from overlay 177 +05B7, and the cast window steps only to levels up to overlay 211 +208C, so level 9 wizard spells need class level 17 and 18 and level 7 priest spells class level 13 and 12
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000693A2..0x000693FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000626B7..0x000628AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062A10..0x00062AA4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D670..0x0004D6A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097D1F..0x00097D89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00096BE0..0x00096D99
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00086460..0x000864ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062DB3..0x00062F36
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, immediate_search.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 211 `+10F4` casts a spell of kind 1 only when the byte at
`DS:4469 + 34 * slot + L` is not 0, and of kind 2 when the byte at `DS:4474 + 34 * slot + L` is not
0, `L` being the byte at `DS:43E0 + slot` or `DS:43E4 + slot` (FND-PARTY-109, FND-PARTY-110).

**Writers of the counts.** `immediate_search.py` finds the displacements `0x4469` and `0x4474`
written only by overlay 177 `+0CB3`, which moves one spell of the highest level with a count from
one combatant to another (`+0CB3..+0E36`), and overlay 197 `+3060`, which lowers by 1 the count of
the kind and level of the spell it is given when its tests pass (`+3060..+30EC`); and the
displacements `0x446A` and `0x4475` only in overlay 182 `+0B52`. **Overlay 182 `+0B52`** (trampoline
`56BD:0098`, called from overlays 171, 182, 184, 190, 194 and 204) does nothing for a slot above
4; otherwise, for `L` from 1 to 9, it stores overlay 177 `+05B7` of the slot, 1 and `L` at
`DS:4469 + 34 * slot + L`, and of the slot, 2 and `L` at `DS:4474 + 34 * slot + L`
(`+0B52..+0BAB`).

**Overlay 177 `+05B7`** takes a slot, a kind `k` and a level `L`. For a slot below 4 when the byte
at `DS:13F8` is 1 it returns 19. Otherwise it adds, over the three class bytes at `+0x1B` of the
slot's FMT-COMBAT-002 record that are not 0, skipping the second and third as overlay 177 `+0B00`
does (FND-PARTY-110), those whose class overlay 177 `+0A45` accepts for spell 8 when bit 0 of `k`
is set or for spell 115 when bit 1 is set, the value of overlay 177 `+0910` for the class code,
the pair of the class's level byte at `+0x1E` and `27E5:04A0` of the slot and 4, and `L`
(`+05B7..+07AE`).

**Overlay 177 `+0910`** takes a class code `c`, a pair of bytes and `L`. It reads the byte at
`DS:0670 + c` as up to two four-bit codes, low first, and for each, with `x` the next byte of the
pair and `v` the word at `DS:0684 + 2 * code`, takes `n = x - (x + p) / 2 - (L + s)`, where `p` is
bit 0 of `v` and `s` is bits 4 to 7 of `v` less 1; adds 1 when `n` is 1 and bit 0 of `x` equals
`p`; limits `n` to 0 and to bits 8 to 11 of `v`; and returns the sum (`+0910..+09A3`). The bytes
at `DS:0670` are 0 for the codes 0, 9, 10, 12 and 17, 0x43 for 1 to 8 and 19, 0x01 for 11 and 18,
and 0x02 for 13 to 16; the words at `DS:0684` for the codes 1 to 4 are 0x500, 0x331, 0x900 and
0x360.

**The cast window's level.** Overlay 211 `+081B` changes the level byte of the window's kind by
calling overlay 211 `+1F0F` with the slot, the kind and the byte at `DS:43EC`, and stores the result
in `DS:43EC` and in `DS:43E0 + slot` or `DS:43E4 + slot` (`+0DD0..+0F89`). **Overlay 211 `+1F0F`**
returns the first level from the one after its third argument up to 22 for which overlay 177
`+0400` of the slot, a null pointer, the kind and the level returns nonzero, or 0 when there is
none, and, unless the byte at `DS:13F8` is not 0, no more than overlay 211 `+208C` of the slot and
the kind (`+1F0F..+1F78`).

## Interpretation

Evaluating overlay 177 `+0910` gives a Preserver (code 11) its first level 9 count at class level
17, and a cleric or druid (codes 1 to 8) its first level 7 count at class level 13 while the
second byte of the pair is below 25; that byte adds to level 7 only from 25, and to levels 1 to 3
for 18. Overlay
211 `+208C` gives 9 for kind 1 from a class level of 18 and 7 for kind 2 from 12. With the level
gain stopping at 15 (RULE-PARTY-013), a Preserver's window reaches level 7 and its level 9 count
stays 0, so a party member cannot cast spell 104 from the list while the byte at `DS:13F8` is 0; a
party member with a druid class at a priest level of 13 to 15, or 12 with the second byte at 25
or more, has level 7 spells to cast and can cast spell 225.

## Alternatives

- A class level above 15 from a source other than the level gain, such as a saved or imported
  character or a script, was not looked for; it would give a Preserver level 9 spells.
- What sets the byte at `DS:13F8` was not read.
- When overlay 182 `+0B52` runs (a rest, a level gain or a load) was not read; it is the only
  writer of the counts other than the spend and the move.
- The ability score `27E5:04A0` returns for 4 was not read; it changes the level 7 count only
  from 25 and never the level 9 count.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 4469 4474 446A 4475`;
`overlay_listing.py <dsun> 182 693A2 69400`, `177 626B7 628B0`, `177 62A10 62AC5`,
`177 62DB3 62F60`, `197 86460 86560`, `211 96BE0 96DA0` and `211 97D1F 97DC5`;
`trampoline_target.py <dsun> 5699:002F`; `direct_callers.py <dsun> 182+0B52`; read the 20 bytes at
file `0x0004D670` and the words at file `0x0004D684`; and evaluate `+0910` for the codes 5 and 11,
class levels 1 to 20, a second byte of 18 and `L` from 1 to 9.

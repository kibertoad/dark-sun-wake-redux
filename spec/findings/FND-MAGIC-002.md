---
id: FND-MAGIC-002
title: The game computes each caster's spells per level from two halving formulas in overlay 177 +0910, one on the class level and, for clerics and druids, one on the score at combatant byte +0x1D, sums them over the slot's casting classes into a wizard and a priest pool, and refuses a cast with YOU NEED TO REST when the pool byte of the window's level is 0; most rows differ from the manual's tables
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000626B7..0x000628AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062A10..0x00062AA4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D670..0x0004D684
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D684..0x0004D68E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000693A2..0x000693FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:04A0..27E5:04F1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097309..0x00097368
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008640D..0x000864ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062B45..0x00062C00
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7; a short evaluation of +0910 over the file's bytes at DS:0670 and DS:0684
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x62100 + x` for overlay 177, `0x68850 + x` for 182,
`0x83400 + x` for 197 and `0x95E10 + x` for 211; `DS` bytes are at file `0x4D000` plus the
offset. A slot's details record is the 0x42-byte record at the far pointer `DS:19C5` numbered by
word `+0x04` of its combatant record (49 bytes at `DS:19C9`, numbered by the word at
`4F49:0C34 + 3 * slot`), with the class bytes at `+0x1B..+0x1D` and the level bytes at
`+0x1E..+0x20`. Class codes are FND-PARTY-057's: 1 to 4 Cleric, 5 to 8 Druid, 11 Preserver, 13
to 16 Ranger; 18 and 19 are codes no character of FMT-PARTY-001 has.

**The pools.** `immediate_search.py <dsun> 4469 4474 446A 4475` finds the only full writes in
overlay 182 `+0B52`: for a slot of at most 4 and `i` from 0 to 8 it stores the low byte of overlay
177 `+05B7` (slot, 1, `i + 1`) at `DS:446A + 34 * slot + i` and of `+05B7` (slot, 2, `i + 1`) at
`DS:4475 + 34 * slot + i` (`+0B52..+0BAB`). So `DS:4469 + 34 * slot + L` is the wizard pool and
`DS:4474 + 34 * slot + L` the priest pool for spell level `L` from 1 to 9. The other hits are
overlay 177 `+0CE4..+0E2F` (the move of FND-PARTY-111), overlay 197 `+3033`, `+3054`, `+30BC`
and `+30DD`, and overlay 211's readers.

**The test.** Overlay 211 `+14F9..+1558`, in the cast handler `+10F4`, takes the slot from
`4E71:0B44`; when the window kind byte `DS:43E8 + slot` is 1 it reads `DS:4469 + 34 * slot + L`
with `L` the byte at `DS:43E0 + slot`, when it is 2 it reads `DS:4474 + 34 * slot + L` with `L`
the byte at `DS:43E4 + slot`, and when the byte read is 0 it shows `DS:2CD3`, `YOU NEED TO REST`,
and casts nothing. Kind 3 (psionic powers) is not tested here.

**The spend.** Overlay 197 `+300D` takes a slot and a spell `n` and returns the pool byte for the
kind overlay 177 `+0A1E` gives `n` (1 or 2) at the level overlay 177 `+09C5` gives it, 0 for any
other kind (`+300D..+305F`). Overlay 197 `+3060` takes a slot and `n`. When overlay 193 `+1C95`
of the slot and 0x5C is nonzero, `n` is of kind 1, and the near routine `+37A5` of the slot and
`n` returns nonzero, it changes nothing; otherwise, when `+300D` is nonzero, it subtracts 1 from
that pool byte (`+3060..+30EC`).

**The count.** Overlay 177 `+05B7` takes a slot, a kind `k` and a level `L`. For a slot below 4
with the debug byte `DS:13F8` equal to 1 (FND-PARTY-116) it returns 19 (`+05C2..+05D1`). Otherwise
it walks the class positions 0 to 2 (`+05D4..+07A5`):

- a position whose class byte is 0 is passed over (`+0605`);
- for position 1 or 2 of a slot whose origin byte `+0x12` is 1 (a human), a position whose level
  byte is not below position 0's is passed over (`+060F..+0698`);
- when bit 0 of `k` is set and `+0A45` of spell 8 and the class is nonzero, or bit 1 of `k` is set
  and `+0A45` of spell 115 (0x73) and the class is nonzero, it adds the value of `+0910` for the
  class code, a two-byte pair, and `L` (`+069B..+079C`). The pair's first byte is the position's
  level byte (`+0749..+074D`) and its second is `27E5:04A0` of the slot and 4 (`+0750..+075B`).

`+0A45` (listed `+0A45..+0AD9`, words at `cs:0ADA`) returns 1 for a spell below 115 only for
codes 11 and 18, and for spell 115, whose byte at `51F1:07D3 + 115` is 0x30, for codes 1 to 8, 13
to 16 and 19 (FND-PARTY-110). So kind 1 adds the Preserver positions and kind 2 every Cleric,
Druid and Ranger position. `27E5:04A0` of the slot and 4 returns byte `+0x1D` of the same
combatant record, or 9 when the slot is 9999 or more, the byte at `4F49:0C33 + 3 * slot` is not 2
or the combatant number is 9999 or more (`27E5:04A0..27E5:04F1`).

**The formula.** Overlay 177 `+0910` takes a class code `c`, a pointer to the pair and `L`
(`+0910..+09A3`). It reads `D`, the byte at `DS:0670 + c`, and while `D` is not 0 takes its low
four bits as a code `j`, the word `v` at `DS:0684 + 2 * j` and the next byte `x` of the pair
(signed), and adds the clamped value of

    n = x - (x + p) / 2 - (L + s)     (p = bit 0 of v, s = bits 4 to 7 of v less 1)

where the division truncates toward zero; when `n` is 1 and bit 0 of `x` equals `p` it adds 1;
it then raises `n` to 0 and lowers it to `cap`, bits 8 to 11 of `v`; then `D` shifts right by 4.
It returns the low byte of the sum.

The 20 bytes at `DS:0670` (file `0x4D670`), codes 0 to 19:

| Codes | Byte | First code `j` (pair byte 1, the level) | Second code `j` (pair byte 2, `+0x1D`) |
| --- | --- | --- | --- |
| 0, 9, 10, 12, 17 | 0x00 | none | none |
| 1 to 8, 19 | 0x43 | 3 | 4 |
| 11, 18 | 0x01 | 1 | none |
| 13 to 16 | 0x02 | 2 | none |

The words at `DS:0684` (file `0x4D684`) for `j` from 0 to 4: 0x0000, 0x0500, 0x0331, 0x0900,
0x0360. No byte at `DS:0670` names a code above 4. The words give:

| `j` | Word | `p` | `s` | `cap` | Value for `x`, `L` |
| --- | --- | --- | --- | --- | --- |
| 1 | 0x0500 | 0 | -1 | 5 | `ceil(x / 2) - L + 1`, 2 for a result of 1 with even `x`, within 0 to 5 |
| 2 | 0x0331 | 1 | 2 | 3 | `floor(x / 2) - L - 2`, 2 for a result of 1 with odd `x`, within 0 to 3 |
| 3 | 0x0900 | 0 | -1 | 9 | `ceil(x / 2) - L + 1`, 2 for a result of 1 with even `x`, within 0 to 9 |
| 4 | 0x0360 | 0 | 5 | 3 | `ceil(x / 2) - L - 5`, 2 for a result of 1 with even `x`, within 0 to 3 |

Evaluating `+0910` over these bytes gives the tables below, for spell levels 1 to 9 left to right.

Preserver (codes 11 and 18), by Preserver level:

| Level | Counts |
| --- | --- |
| 1 | 1 0 0 0 0 0 0 0 0 |
| 2 | 2 0 0 0 0 0 0 0 0 |
| 3 | 2 1 0 0 0 0 0 0 0 |
| 4 | 2 2 0 0 0 0 0 0 0 |
| 5 | 3 2 1 0 0 0 0 0 0 |
| 6 | 3 2 2 0 0 0 0 0 0 |
| 7 | 4 3 2 1 0 0 0 0 0 |
| 8 | 4 3 2 2 0 0 0 0 0 |
| 9 | 5 4 3 2 1 0 0 0 0 |
| 10 | 5 4 3 2 2 0 0 0 0 |
| 11 | 5 5 4 3 2 1 0 0 0 |
| 12 | 5 5 4 3 2 2 0 0 0 |
| 13 | 5 5 5 4 3 2 1 0 0 |
| 14 | 5 5 5 4 3 2 2 0 0 |
| 15 | 5 5 5 5 4 3 2 1 0 |
| 16 | 5 5 5 5 4 3 2 2 0 |
| 17 | 5 5 5 5 5 4 3 2 1 |
| 18 | 5 5 5 5 5 4 3 2 2 |
| 19, 20 | 5 5 5 5 5 5 4 3 2 |

Cleric and druid (codes 1 to 8 and 19), the level term, by class level:

| Level | Counts |
| --- | --- |
| 1 | 1 0 0 0 0 0 0 0 0 |
| 2 | 2 0 0 0 0 0 0 0 0 |
| 3 | 2 1 0 0 0 0 0 0 0 |
| 4 | 2 2 0 0 0 0 0 0 0 |
| 5 | 3 2 1 0 0 0 0 0 0 |
| 6 | 3 2 2 0 0 0 0 0 0 |
| 7 | 4 3 2 1 0 0 0 0 0 |
| 8 | 4 3 2 2 0 0 0 0 0 |
| 9 | 5 4 3 2 1 0 0 0 0 |
| 10 | 5 4 3 2 2 0 0 0 0 |
| 11 | 6 5 4 3 2 1 0 0 0 |
| 12 | 6 5 4 3 2 2 0 0 0 |
| 13 | 7 6 5 4 3 2 1 0 0 |
| 14 | 7 6 5 4 3 2 2 0 0 |
| 15 | 8 7 6 5 4 3 2 1 0 |
| 16 | 8 7 6 5 4 3 2 2 0 |
| 17 | 9 8 7 6 5 4 3 2 1 |
| 18 | 9 8 7 6 5 4 3 2 2 |
| 19, 20 | 9 9 8 7 6 5 4 3 2 |

Cleric and druid, the term on byte `+0x1D`, added to the level term; 0 for every value up to 12:

| `+0x1D` | Counts |
| --- | --- |
| 13 | 1 0 0 0 0 0 0 0 0 |
| 14 | 2 0 0 0 0 0 0 0 0 |
| 15 | 2 1 0 0 0 0 0 0 0 |
| 16 | 2 2 0 0 0 0 0 0 0 |
| 17 | 3 2 1 0 0 0 0 0 0 |
| 18 | 3 2 2 0 0 0 0 0 0 |
| 19 | 3 3 2 1 0 0 0 0 0 |
| 20 | 3 3 2 2 0 0 0 0 0 |
| 21 | 3 3 3 2 1 0 0 0 0 |
| 22 | 3 3 3 2 2 0 0 0 0 |
| 23 | 3 3 3 3 2 1 0 0 0 |
| 24 | 3 3 3 3 2 2 0 0 0 |
| 25 | 3 3 3 3 3 2 1 0 0 |

Ranger (codes 13 to 16), by ranger level; 0 at every spell level for levels 1 to 7:

| Level | Counts |
| --- | --- |
| 8 | 1 0 0 0 0 0 0 0 0 |
| 9 | 2 0 0 0 0 0 0 0 0 |
| 10 | 2 1 0 0 0 0 0 0 0 |
| 11 | 2 2 0 0 0 0 0 0 0 |
| 12 | 3 2 1 0 0 0 0 0 0 |
| 13 | 3 2 2 0 0 0 0 0 0 |
| 14 | 3 3 2 1 0 0 0 0 0 |
| 15 | 3 3 2 2 0 0 0 0 0 |
| 16 | 3 3 3 2 1 0 0 0 0 |
| 17 | 3 3 3 2 2 0 0 0 0 |
| 18 | 3 3 3 3 2 1 0 0 0 |
| 19 | 3 3 3 3 2 2 0 0 0 |
| 20 | 3 3 3 3 3 2 1 0 0 |

Against RULE-MAGIC-001's tables from the manual, levels 1 to 15 and spell levels 1 to 7: the
Preserver rows agree at levels 1, 2, 3 and 7 only, the cleric rows at 1, 2 and 3 only, the
Wisdom rows at 13, 15 and 16 only, and the ranger rows at levels 8 to 11 only (the manual's 12 to
15 are 2 2 1, 3 2 1, 3 2 2 and 3 3 2, and it gives no fourth-level spells).

## Interpretation

The game does not use the manual's progression tables. A caster's spells per level are two pools
per party slot, one for wizard spells (kind 1) and one for priest spells (kind 2), each the sum,
over the slot's class positions that count, of `+0910` for the position's class and level. A druid
uses exactly the cleric's formula, Wisdom term included, since codes 5 to 8 carry the same byte
0x43 as codes 1 to 4. Byte `+0x1D` of the combatant record is the score FND-PARTY-081 and
FND-PARTY-165 read as Wisdom; its term depends on the score and the spell level alone, so a
first-level cleric with Wisdom 18 holds counts of 4, 2 and 2 at spell levels 1 to 3. Whether those
counts can be spent is FND-MAGIC-003's cast-window limit, which stops that cleric at spell level 1.

Classes add: a Cleric/Ranger's priest pool is the sum of the cleric term, the Wisdom term once
(the ranger byte 0x02 has no second code) and the ranger term, and a Preserver/Cleric has a
wizard pool from the Preserver term and a separate priest pool from the cleric terms. A slot with
two Cleric or Druid positions would add the Wisdom term twice; whether any character can hold two
such classes was not checked. A dual-classed human's former class counts only once the current
class's level passes it, and then at its own frozen level (FND-PARTY-258).

Overlay 182 `+0B52` overwrites both pools, so a recount refills them; `+3060` takes one from the
pool of the spell's own kind and level when a spell is spent, unless effect 0x5C with `+37A5`
spares a kind 1 spell. A cast from the window needs a pool byte above 0 at the window's level
and kind.

## Alternatives

- That byte `+0x1D` is Wisdom rests on FND-PARTY-081 and FND-PARTY-165; FMT-COMBAT-001 names
  `+0x19..+0x20` only `unk_19`.
- When `+0B52` runs (rest, level gain, load) was not read here (FND-PARTY-111 leaves it open).
- What effect 0x5C is and what overlay 197 `+37A5` tests was not read.
- The cast queue and enemy casts may test other pools; only overlay 211's window test was read.
- The pools are bytes; a sum above 255 cannot arise from these terms (at most 9 + 3 per class
  position, three positions).

## How to reproduce

From `tools/research/exec-census/` with the evidence Python and `<dsun>` the installed
`DSUN.EXE`: `overlay_listing.py <dsun> 177 626B7 62AC5`, `... 177 62B45 62BDA`, `... 182 693A2
69400`, `... 211 97300 97380`, `... 197 863F0 864ED`; `resident_listing.py <dsun>
27E5:04A0..27E5:04F2`; `immediate_search.py <dsun> 4469 4474 446A 4475`; `direct_callers.py <dsun>
177+05B7 177+0910`. Read the 20 bytes at file `0x4D670`, the five words at file `0x4D684`, the 19
words at file `0x62BDA` and the bytes at file `0x478E3 + 115` and `0x478E3 + 225`; evaluate
`+0910` for codes 3, 11 and 13 with `x` from 1 to 20, code 3's second term (`j` = 4) with `x`
from 3 to 25, and `L` from 1 to 9.

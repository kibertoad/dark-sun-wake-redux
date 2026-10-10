---
id: FND-PARTY-110
title: The party's cast list holds a spell below 115 when the slot's SPST bit for it is set, and a spell from 115 to 234 when one of the slot's classes has the spell's sphere bit and the spell's level is within the slot's priest spell level; DATA 104 is a level 9 wizard spell and DATA 225 a level 7 druid spell
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009785F..0x00097902
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062500..0x00062592
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006260A..0x000626B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062AC5..0x00062B1E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062B45..0x00062BDA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00062BDA..0x00062C00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00062C00..0x00062CC8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097E9C..0x00097F00
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004D655..0x0004D66C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047956..0x000479CE
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, immediate_search.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 211 `+10F4` casts entry `i` of the words at `DS:9C20` for `i` below the word at
`DS:9D79` (FND-PARTY-109).

**The list.** `immediate_search.py` finds the words `0x9C20` and `0x9D79` written in overlay 211
and overlay 209 only. Overlay 211 `+1A4F` reads the byte at `DS:43E8 + slot`, the slot being the
word at `4E71:0B44`: for 1 it calls overlay 177 `+0400` with the slot, `DS:9C20`, 1 and the byte at
`DS:43E0 + slot`; for 2 the same with 2 and the byte at `DS:43E4 + slot`; for 3 it calls overlay 211
`+1DBC`; and it stores the count in the words at `DS:9C1E` and `DS:9D79` (`+1A4F..+1AF2`). Overlay
209 stores words at `DS:9C20` in `+03DC`, the spell window's list of FND-PARTY-104, which overlay
211 `+1A4F` refills before a cast.

**Overlay 177 `+0400`** takes a slot, a far pointer, a kind `k` and a level `L`. For `L` = -1 it
takes the spells 0 to 234; otherwise, for odd `k`, the spells from the byte at `DS:0655 + L` up
to, but not including, the byte at `DS:0656 + L`, and for even `k` from the byte at
`DS:0660 + L` to the byte at `DS:0661 + L`. It stores each spell for which `+050A` of the slot
and the spell returns nonzero, at most 21 of them, and returns their count (`+0400..+0491`). The
23 bytes at `DS:0655` are 0, 0, 12, 25, 39, 56, 68, 81, 92, 102, 111, 115, 115, 127, 143, 165, 189,
208, 216, 231, 232, 233 and 235.

**Overlay 177 `+050A`** takes a slot `s` and a spell `n` (`+050A..+05B6`):

- for `s` of 4 or more it returns whether `+0BC8` of the slot and `n` is above 0;
- when the byte at `DS:13F8` is 1 it returns 1;
- for `n` below 115 it returns bit `n & 7` of the byte at `4D62:039C + 15 * s + (n >> 3)`;
- for `n` from 115 to 234 it returns 1 when `+0B00` of `n` and `s` is nonzero and `+09C5` of `n`
  is not above overlay 211 `+208C` of `s` and 2;
- otherwise 0.

**Overlay 177 `+09C5`** returns, for `n` below 115, the greatest `c` from 1 to 10 whose byte at
`DS:0655 + c` is not above `n`; for `n` from 115 to 234 the same with `DS:0660 + c`; 3 for `n` from
269 to 304; and 0 otherwise (`+09C5..+0A1D`).

**Overlay 177 `+0B00`** takes `n` and `s`. For each of the three class bytes at `+0x1B` of the
slot's FMT-COMBAT-002 record it returns 1 when `+0A45` of `n` and the class code is nonzero,
skipping the second and third when byte `+0x12` of the record is 1 and their byte at `+0x1E` is
not below the first one's (`+0B00..+0BC7`). **`+0A45`** takes `n` and a class code `c`. For `n`
below 115 it returns 1 when `c` is 11 or 18. For `n` from 115 to 234 it switches on `c` less 1
through 19 words at `cs:0ADA` to a mask, 0x11, 0x12, 0x14 and 0x18 for codes 1 to 4 and 13 to 16,
0x31, 0x32, 0x34 and 0x38 for codes 5 to 8, 0x3F for code 19 and 0 for the others, and returns 1
when the byte at `51F1:07D3 + n` shares a bit with it (`+0A45..+0AD9`).

**Overlay 211 `+208C`** with a slot and 2 takes `x`, overlay 211 `+1FB5` of the slot and spell 115,
and returns `(x + 1) >> 1` for `x` below 12, 7 for `x` from 12 to 19, `(x + 4) / 3` for `x` from
20 to 27 and 10 otherwise (`+208F..+20EF`). With FND-PARTY-104, `x` is the greatest level among
the slot's classes that `+0A45` accepts for spell 115, less 7 for codes 13 to 16.

The byte at `51F1:07D3 + n` is 0x30 for spell 115 and 0x20 for spell 225.

## Interpretation

Spell 104 lies in the wizard range of level 9 (102 to 110), and enters the cast list at level 9
for any party member whose `SPST` bit 104 is set; nothing in the list builder checks the class or
the level for it. Spell 225 lies in the priest range of level 7 (216 to 230), and its sphere byte
0x20 is accepted for the druid codes 5 to 8 only among the codes a party member can have; it
enters the cast list at level 7 for a party member with a druid class whose greatest level among
its cleric, druid and ranger classes, rangers counting 7 less, is 12 or more, for which overlay
211 `+208C` gives 7 (spell 115's byte 0x30 is accepted for all three).

## Alternatives

- Casting also needs a nonzero count of spells to cast at the level (FND-PARTY-109's tests), and
  the level window must offer level 9 or 7; neither was read here.
- Whether a party member can learn spell 104 other than through the spell window of FND-PARTY-104,
  which offers spell levels up to overlay 211 `+208C` of the slot and 1, was not read.
- The race limits on a druid's level were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 9C20 9D79`;
`overlay_listing.py <dsun> 211 9785F 97910`, `211 97E9C 97F00`, `177 62500 62592`,
`177 6260A 626BA`, `177 62AC5 62C00` and `177 62C00 62CD0`; read the 19 words at file
`0x00062BDA`, the 23 bytes at file `0x0004D655`, and the bytes at file `0x000478E3 + n` for `n`
from 115 to 234.

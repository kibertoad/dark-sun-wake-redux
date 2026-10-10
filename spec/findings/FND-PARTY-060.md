---
id: FND-PARTY-060
title: Overlay 210 turns a class code into a DATA 1000 row through the byte table at 4E4F:009D, which sends codes 1 to 17 to the classes 1 to 8 of the name table at DS:1164, and overlay 209 writes codes of that 17-code numbering into the details record when a class changes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009588D..0x00095980
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095B44..0x00095B7B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095C3E..0x00095C7D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009493C..0x00094A80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004378D..0x000437B1
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, field_stores.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Segment `4E4F` (segment word `0x0360`, descriptor 108) starts at file `0x436F0`.

**Overlay 210.** Each of its six calls of overlay 185 `+0000` with 1000 (FND-PARTY-058), at
`+0977`, `+09C5`, `+0A0E`, `+0C16`, `+0C69` and `+0D18`, takes the class byte at `+0x1B` plus the
class position of the details record of a slot in the table at `DS:19C5`, reads the byte at
`4E4F:009D` plus 2 times that code, and passes that byte less 1 as the row (for example
`+093A..+0973` and `+0BE4..+0C13`). At `+0920..+092A` it ends the loop after position 0 when the
origin byte at `+0x12` is 1, and at `+0943..+0947` it passes over a class byte of 0.

**The table at `4E4F:009D`**, two bytes per code from code 0 (file `0x4378D`):

| Code | Byte 0 | Byte 1 |
| --- | --- | --- |
| 0 | 0 | 0x80 |
| 1, 2, 3, 4 | 1 | 0x40, 0x20, 0x10, 0x80 |
| 5, 6, 7, 8 | 2 | 0x40, 0x20, 0x10, 0 |
| 9 | 3 | 0 |
| 10 | 4 | 0 |
| 11 | 5 | 0 |
| 12 | 6 | 0x80 |
| 13, 14, 15, 16 | 7 | 0x40, 0x20, 0x10, 0 |
| 17 | 8 | 0 |

**Overlay 209 `+17DC`** (trampoline `57B0:003E`, called also from `+1744`) takes a slot and a code.
For the details record of the slot in the table at `DS:19C5` it moves the class bytes at `+0x1B`
and `+0x1C` to `+0x1C` and `+0x1D`, the level bytes at `+0x1E` and `+0x1F` one place up, and the
bytes at `+0x39` and `+0x3A` one place up, from position 2 down (`+17E6..+1860`). It then stores the code at `+0x1B`, 1
at `+0x39` and at `+0x1E`, and 0 in the dwords at `+0x04` and `+0x00` (`+1862..+18B2`). It stores
the result of overlay 210 `+002A` at `+0x08`, calls further overlay 210 routines and stores one
result at `+0x16` of the slot's combatant record in the table at `DS:19C9`. When the code is 12 it
sets bits 0 to 2 of the byte at `4E71:0A55` plus the slot (`+18F7..+191A`), and it then compares
the code with 11 (`+191C`).

**The other readers.** Overlay 186 `+0421` (FND-PARTY-058) and overlay 183 `+13B7..+14D0` pass the
class byte less 1 as the row without this table. `field_stores.py` with `--es` and the
displacements `1B`, `1C` and `1D` finds byte stores to the class bytes in overlay 183 (`+010D` to
`+0117`, `+0A55` to `+0AB5` and `+0B40` to `+0B76`), overlay 184 (`+083B` to `+0845` and `+1748`),
overlay 195 (`+026E` and `+0836`, decrements), overlay 209 (`+180D` and `+1870`) and overlay 179
(`+250C`, a decrement decoded only from inside another instruction); the other hits are word and
dword stores at those displacements in the resident code and in overlays 182 and 193.

## Interpretation

In play the class bytes hold the 17-code numbering FND-PARTY-057 names through `DS:1487`: the
class change of overlay 209 tests the new code against 12 (Psionic) and 11 (Preserver), and the
load copies the stored codes unchanged (FND-PARTY-051). Byte 0 of the table at `4E4F:009D` is the
class's position in the 8-name table at `DS:1164` (Cleric 1 to Thief 8), which is the
`DATA` 1000 row plus 1, so overlay 210 reads the thresholds of the code's class, as the shipped
experience values match (FND-PARTY-058). Overlay 186 reads row code less 1 instead: for codes 1 to
8 another class's row, and for codes 9 to 17 offsets 320 to 640 bytes into a resource of 320.
Byte 1 is 0x40, 0x20 and 0x10 for the first three codes of Cleric, Druid and Ranger, 0x80 for the
fourth Cleric code, for code 12 and for code 0, and 0 elsewhere; which spheres the bits stand for
is not shown here. The class change sets the new class to level 1 and experience to 0 and keeps up
to two earlier classes behind it, which fits a human changing class (RULE-PARTY-004).

## Alternatives

- Generation stores codes of the 8-name numbering, which overlay 186 would read correctly:
  overlay 183 names classes through `DS:1164` and indexes `DATA` 1000 by the code less 1, so it may
  use that numbering while a character is made. What code a finished character keeps was not
  traced; the shipped records and overlay 209 use the 17-code numbering.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 210 0x95880 0x95980`,
`210 0x95B20 0x95C90` and `209 0x9493C 0x94A80`, `direct_callers.py <dsun> 185+0000 209+17DC`, and
`field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 1B 1C 1D`. Read the 36
bytes at file `0x4378D`.

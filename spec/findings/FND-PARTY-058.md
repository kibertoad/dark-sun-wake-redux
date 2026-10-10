---
id: FND-PARTY-058
title: The character sheet prints the details record's dword at 0x00 after the label EXP and, in brackets, the least next-level figure it reads for each class from DATA resource 1000, an 8-row table of words in hundreds indexed by the class byte less 1 and the level
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FB21..0x0006FC5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F640..0x0006F6F9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C3B7..0x0006C4D0
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1858B..0x186CB
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, gff_tag_numbers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose data segment `57E0` starts at file `0x4D000`. The record is the far pointer
argument at `[bp+0x0A]`, which overlay 184 `+0462` passes from `DS:1429` (FND-PARTY-055).

**Overlay 186 `+0421`** (trampoline `56E9:0048`) passes the label at `DS:137D` (`EXP: `) and the
record's dword at `+0x00` to overlay 183 through trampoline `56CC:00F2`, which builds the label
and the value in decimal in the buffer at `4E71:0A71` (`+0438..+0447`). For each of the class
positions 0 to 2 (`+044A..+04C1`) it skips the position when the level byte at `+0x1E` plus the
position is 15 or the class byte at `+0x1B` plus the position is 0, and otherwise calls overlay
185 `+0000` (trampoline `56E6:0020`) with 1000, the class byte less 1 and the level, takes the
result in `DX:AX`, multiplies it by 100 and keeps it when no figure is kept yet or when it is
nonzero and less than the kept one (signed dword compare). When the origin byte at `+0x12` is 1
the loop ends after position 0 (`+04B3..+04BB`). When a figure is kept it appends the string at
`DS:1383`, the figure in decimal and the string at `DS:1386` (`(` and `)`); it always appends the
string at `DS:1381`, and prints the buffer with the format at `DS:1364` (`+04C3..+0558`).

**Overlay 185 `+0000`** requests the resource `DATA` numbered by its first argument through
`38FF:04AB` and returns 0 when that returns -1 (`+0012..+0039`). For 1000 it returns the word at
the resource plus 40 times the second argument plus 2 times the third, zero-extended
(`+007D..+009D`); for 1001 it returns the signed byte at 72 times the second argument plus 9 times
the third plus the fourth (`+0047..+007B`). It frees the resource through `444C:0092` and returns
the dword in `DX:AX` (`+00A1..+00B6`). `direct_callers.py` finds it called from overlay 183
`+0B24`, `+0B99`, `+13F8`, `+149E` and `+14E0`, overlay 186 `+047F`, and six sites in overlay 210.

**Overlay 183 `+13B7..+14D0`**, for each class position, reads `DATA` 1000 at the class byte less
1 and the level less 1, times 100, and stores the greater of it and the record's dword at `+0x00`
back into that dword (unsigned compare). It then reads the entry at the level, times 100, and when
it is not above the dword adds 1 to the level byte, reading the entry for the new level again.

**`DATA` 1000** is the only resource of that number, in `RESOURCE.GFF` at `0x1858B`, 320 bytes:
8 rows of 20 words. `DATA` 1001 is 576 bytes, 8 rows of 72.

| Row | Words 0 to 19 |
| --- | --- |
| 0 | 0, 15, 30, 60, 130, 275, 550, 1100, 2250, 4500, 6750, 9000, 11250, 13500, 15750, 18000, 20250, 22500, 24750, 27000 |
| 1 | 0, 20, 40, 75, 125, 200, 350, 600, 900, 1250, 6750, 9000, 11250, 13500, 15750, 18000, 20250, 22500, 24750, 27000 |
| 2 | 0, 20, 40, 80, 160, 320, 640, 1250, 2500, 5000, 7500, 10000, 12500, 15000, 17500, 20000, 22500, 25000, 27500, 30000 |
| 3 | 0, 22, 45, 90, 180, 360, 750, 1500, 3000, 6000, 9000, 12000, 15000, 18000, 21000, 20000, 22500, 25000, 27500, 30000 |
| 4 | 0, 25, 50, 100, 200, 400, 600, 900, 1350, 2500, 3750, 7500, 11250, 15000, 18750, 22500, 26250, 30000, 33750, 37500 |
| 5 | 0, 22, 44, 88, 165, 300, 550, 1000, 2000, 4000, 6000, 8000, 10000, 12000, 15000, 18000, 21000, 14000, 17000, 30000 |
| 6 | 0, 22, 45, 90, 180, 360, 750, 1500, 3000, 6000, 9000, 12000, 15000, 18000, 21000, 24000, 27000, 30000, 33000, 36000 |
| 7 | 0, 12, 25, 50, 100, 200, 400, 700, 1100, 1600, 2200, 4400, 6600, 8800, 11000, 13200, 15400, 17600, 19800, 22000 |

**The stored records.** The dwords at `0x45` and `0x49` of the 19 `CHAR` resources of
`CHARSAVE.GFF`, which are the type-3 chunk's dwords at `+0x00` and `+0x04` (FND-PARTY-052):

| Resource | `0x45` | `0x49` |
| --- | --- | --- |
| 29 | 64000 | 3000 |
| 30 | 122690 | 122690 |
| 31 | 342833 | 342833 |
| 32 | 1389560 | 1389560 |
| 33 | 365393 | 365393 |
| 34, 35, 36 | 30000 | 3000 |
| 37 | 40000 | 3000 |
| 38 | 36000 | 3000 |
| 39, 43 | 75000 | 3000 |
| 40 | 40000 | 3000 |
| 41 | 55000 | 3000 |
| 42 | 32000 | 3000 |
| 50 | 1400000 | 1411500 |
| 51 | 1400000 | 1417250 |
| 52 | 41855 | 1417250 |
| 53 | 2475000 | 2509500 |

## Interpretation

The details record's dword at `0x00`, `CHAR` offset `0x45`, is the character's experience: the
sheet prints it after `EXP: `, and overlay 183 raises it to the threshold of the current level and
raises the level while it reaches the next threshold. `DATA` 1000 holds the experience thresholds
in hundreds, one row per class and one word per level, the word at a level times 100 being the
experience at which the next level starts. The bracketed figure is the least such threshold over the character's classes,
leaving out a class at level 15 and, for a human, every class after the first.

For records 29, 34 to 43, the experience equals the greatest of the thresholds at which each class's
current level starts, when each class code is placed on the row of its name's position in
the 8-name table at `DS:1164` (Cleric row 0 to Thief row 7) through the names FND-PARTY-057 gives
the codes: record 41's Cleric at level 7 has 55000 (row 0, word 6), record 39's Gladiator at level
7 has 75000 (row 3, word 6). Overlay 186 uses the stored code less 1 as the row instead, so code 2
reads row 1 and codes 9 to 17 read beyond the 320 bytes of the resource. That is a value pattern;
whether the code in memory when the sheet runs is the stored one or one converted to the 8-class
numbering is not shown here.

## Alternatives

- The dword at `0x49` is a second experience figure: it is 3000 in eleven records and equal to or
  above the dword at `0x45` in the others; no routine read here uses it.
- Overlay 186 reads the 17-code value and so draws thresholds from the wrong row or from memory
  past the resource: possible on this reading, but the routines that write the class bytes after
  load were not traced.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 186 0x6FB21 0x6FC5E`,
`185 0x6F640 0x6F6F9` and `183 0x6C3A8 0x6C4D0`, `direct_callers.py <dsun> 185+0000`, and
`gff_tag_numbers.py <install> DATA`. Read the 320 bytes at `0x1858B` of `RESOURCE.GFF` as 8 rows of
20 little-endian words, and the dwords at `0x45` and `0x49` of each `CHAR` resource of
`CHARSAVE.GFF`.

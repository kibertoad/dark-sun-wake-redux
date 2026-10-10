---
id: FND-PARTY-085
title: When a character is stored, changes class or gains a level, four routines set the details record's class flags at 0x10, attack rates at 0x24 and 0x25, five saving throws at 0x31 and a combatant byte at 0x16 of 20 less the best of (level - 1) times a class group's rate over 12
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:000F..27E5:00C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004D788..0x0004D7B0
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000952C5..0x000953A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000954D2..0x0009562D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004ADE0..0x0004ADF0
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004AE4C..0x0004AE88
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Records and slots are as FND-PARTY-081 gives; each routine below takes a slot and
reads the details and combatant records numbered by it. Segment `55BE` starts at file `0x4ADE0`
and the data segment `57E0` at file `0x4D000`. Overlay 210 `+0449` with a flag fills four pairs of
bytes, one per class group from the byte at `55BE:0010` plus the stored class code, whose byte 0
is the greatest level of the group's classes; with the flag 0 it passes over, for an origin byte
of 1, each position after the first whose level is not below the first's (FND-PARTY-074).

**`27E5:000F`** copies the 20 words at `DS:0788` (file `0x4D788`) to its stack: 0, 1, 2, 4, 8,
0x10, 0x10, 0x10, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x200, 0x200, 0x200, 0x400, 0x800, 0x1000
(`+0021..+0032`). It ORs together the word for the class byte of position 0 and of each later
position whose level is above 0 and below a limit, the limit being position 0's level when the
origin byte is 1 and 100 otherwise, and stores the result in the details record's word at `+0x10`
(`+001A..+00BD`).

**Overlay 210 `+0365`** (trampoline `57B9:0039`) calls `+0449` with the flag 0 and starts from 2,
adding 1 when group 1's level is above 0, 1 more when it is above 6 and 1 more when it is above
12 (`+036F..+039E`). It stores that in the details record's byte at `+0x24`; when the origin byte
is 8 it takes 8 instead and stores 2 in the byte at `+0x27`; it stores the value in the byte at
`+0x25` (`+039E..+03E9`).

**Overlay 210 `+03EC`** (trampoline `57B9:003E`) calls `+0449` with the flag 0 and, for each group
`g` from 0 to 3, takes group `g`'s level less 1 times the signed byte at `55BE:0003` plus 4 times
`g`, divided by 12 (signed), keeping the greatest result, starting from 0. It returns 20 less that
(`+03F3..+0446`). Overlay 210 `+0740` stores the result in the byte at `+0x16` of the combatant
record (FND-PARTY-081). The four bytes at `55BE:0003`, `0007`, `000B` and `000F` are 8, 12, 4
and 6.

**Overlay 210 `+0572`** (trampoline `57B9:004D`) calls `+0449` with the flag 1 and stores 99 in
the five bytes at `+0x31..+0x35` of the details record (`+057D..+05A5`). For each group `g` from 0
to 3 whose level `L` is above 0, and each save `s` from 0 to 4, with `b`, `r` and `m` the three
bytes at `55BE:006C` plus `15 * g + 3 * s` (`+05A5..+05B8`):

1. It takes `r * (L - 1) / 100` (signed), and `m` when that is greater (`+05BA..+0624`).
2. When `g`, the group, is 1 or 4, and the origin byte is 2 or 6, it adds twice the combatant
   record's byte at `+0x1B`, its constitution, divided by 7 (`+0624..+066E`).
3. It takes `b` less that, and stores it in the byte at `+0x31` plus `s` when that byte is above
   it (`+066E..+06B7`).

The 60 bytes at `55BE:006C` (file `0x4AE4C`), as `b`, `r`, `m` for saves 0 to 4:

| Group | Save 0 | Save 1 | Save 2 | Save 3 | Save 4 |
| --- | --- | --- | --- | --- | --- |
| 0 | 10, 45, 2 | 14, 45, 6 | 13, 45, 5 | 16, 45, 8 | 15, 45, 7 |
| 1 | 14, 69, 3 | 16, 69, 5 | 15, 69, 4 | 17, 82, 4 | 17, 69, 6 |
| 2 | 14, 30, 8 | 11, 40, 3 | 13, 40, 5 | 15, 40, 7 | 12, 40, 4 |
| 3 | 13, 25, 8 | 14, 50, 4 | 12, 25, 7 | 16, 25, 11 | 15, 50, 5 |

**The callers.** `direct_callers.py` finds `27E5:000F` called from overlay 184 `+10E9`, overlay 209
`+18F1` and overlay 210 `+0824`; `+0365` from overlay 184 `+1753`, overlay 209 `+18CD` and overlay
210 `+082C`; `+03EC` from overlay 184 `+1854`, overlay 197 `+3304`, overlay 209 `+18D4` and overlay
210 `+0832`; and `+0572` from overlay 184 `+186A`, overlay 209 `+18EA` and overlay 210 `+0847`.
Overlay 184 `+1648` stores a character from the generation screen and overlay 209 `+17DC` changes
a class (FND-PARTY-073, FND-PARTY-083).

## Interpretation

The word at `+0x10` is a set of flags for the classes counted, one bit per class code: the four
cleric codes each have their own bit, the four druid codes share one and the four ranger codes
share one; a human's former class counts only while it is below the current one. With the class
groups priest, warrior, wizard and rogue (FND-PARTY-074), the byte at `+0x24` is 2 for a character
without warrior levels and 3, 4 or 5 from warrior levels 1, 7 and 13, and a thri-kreen gets 8 at
`+0x25`. The combatant byte at `+0x16` falls by 2 every 3 levels for a priest, 1 a level for a
warrior, 1 every 3 levels for a wizard and 1 every 2 levels for a rogue, from 20, taking the best
group; RULE-COMBAT-002's THAC0 behaves this way, though no reader of the byte was traced here.

Each saving throw starts at `b` of the best group and falls by `r` hundredths per level above the
first, by at most `m`. For every one of the 20 entries `b` less `r` hundredths times the levels
above the first, at level 19 for a priest, 17 for a warrior and 21 for a wizard or rogue, rounded
down, equals `m`, so `m` reads as the last value of the save while the code uses it as the most the
save can fall. A dwarf or halfling gets its constitution bonus only in the warrior group, on all
five saves, because the test is on the group and not on the save.

## Alternatives

- `m` is meant as a cap on the improvement: then a warrior's saves stop falling between levels 6
  and 10, and the match of `b` less `r` hundredths per level with `m` in all 20 entries would be
  chance.
- The constitution bonus is meant for the warrior group: the test of a group number against 4,
  which no group has, fits a test of the save number, where 1 and 4 are both saves.
- Which reader uses the combatant byte at `+0x16` as THAC0, and the details bytes at `+0x24`,
  `+0x25`, `+0x27` and `+0x31..+0x35`, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `resident_listing.py <dsun> 27E5:000F..27E5:00C1`;
`overlay_listing.py <dsun> 210 952C5 953A9` and `210 954D2 9562D`; and `direct_callers.py <dsun>
27E5:000F 210+0365 210+03EC 210+0572`. Read the 20 words at file `0x4D788`, the 16 bytes at
`0x4ADE0` and the 60 bytes at `0x4AE4C`.

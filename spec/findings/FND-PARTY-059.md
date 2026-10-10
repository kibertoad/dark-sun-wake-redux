---
id: FND-PARTY-059
title: The character sheet prints the combatant record's word at 0x02 over the details record's word at 0x0C as unsigned numbers after the label PSI, as it prints hit points from 0x00 over 0x08, and overlay 184 can copy the details word to the combatant word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C87B..0x0006C980
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006D629..0x0006D6B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000985F1..0x00098716
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004FE09..0x0004FE1E
    kind: file-data
tool: Python 3.14.7 with Capstone 5.0.7 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, trampoline_target.py, direct_callers.py, immediate_search.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`, whose data segment `57E0` starts at file `0x4D000`.

**Overlay 183 `+189B`** (trampoline `56CC:003E`) and **`+191D`** (trampoline `56CC:0043`) each take
a far pointer, a far pointer to a details record, a far pointer to a combatant record and two
coordinates. `+189B` formats the combatant record's word at `+0x00` and the details record's word
at `+0x08` with `%d/%d` (`DS:10E8`); `+191D` formats the combatant record's word at `+0x02` and the
details record's word at `+0x0C` with `%u/%u` (`DS:10EE`). Each centres the text on the first
coordinate, no further left than 1, and prints it with the format at `DS:109F`.

**The callers.** Overlay 212 `+03F1..+0516` prints `%C%C%CHP:` (`DS:2E09`) and calls `+189B`, then
prints `%C%C%CPSI:` (`DS:2E13`) and calls `+191D`, passing in both cases the details record of the
slot in `4E71:0B44` from the table at `DS:19C5` (66 bytes each) and its combatant record from the
table at `DS:19C9` (49 bytes each). An immediate search for `0x2E13` finds only that push and an
unrelated `mov dx` in overlay 195. Overlay 184 `+0599..+061E` calls `+189B` and `+191D` with the
far pointers at `DS:1429` and `DS:142D`. Between the two calls, when its byte argument at `[bp+6]`
is nonzero or the word at `DS:112E` is -1, it copies the word at `+0x0C` of the record at
`DS:1429` to the word at `+0x02` of the record at `DS:142D` (`+05C3..+05DC`). `direct_callers.py`
finds `+191D` called from overlays 184, 202, 207, 209, 211 and 212 (twice) and once from within
overlay 183, and `+189B` from overlays 184, 189, 202, 209, 211 and 212 (twice) and twice from
within overlay 183.

**The stored records.** In the 19 `CHAR` resources of `CHARSAVE.GFF`, the type-1 chunk's word at
`+0x02` is `CHAR` offset `0x0C` and the type-3 chunk's word at `+0x0C` is offset `0x51`
(FND-PARTY-052):

| Resource | `0x0C` | `0x51` | `0x4F` |
| --- | --- | --- | --- |
| 29 | 28 | 28 | 0 |
| 30 | 144 | 144 | 127 |
| 31 | 75 | 75 | 49 |
| 32 | 95 | 95 | 132 |
| 33 | 75 | 75 | 98 |
| 34, 35, 36 | 121 | 121 | 84 |
| 37 | 37 | 37 | 72 |
| 38 | 29 | 29 | 96 |
| 39 | 30 | 30 | 70 |
| 40 | 96 | 96 | 102 |
| 41 | 31 | 31 | 50 |
| 42 | 33 | 33 | 84 |
| 43 | 27 | 27 | 123 |
| 50 | 162 | 202 | 111 |
| 51 | 89 | 89 | 109 |
| 52 | 57 | 57 | 65 |
| 53 | 11 | 91 | 89 |

Records 50 and 53 are also the two whose `hit_points` are below `max_hit_points` (FND-PARTY-053);
record 51 has its hit points below the greatest with its two words equal.

## Interpretation

The combatant record's word at `0x02`, `CHAR` offset `0x0C`, is the character's current psionic
strength points, and the details record's word at `0x0C`, `CHAR` offset `0x51` (the upper half of
`unk_4f`), the greatest, in the layout the hit points use at `0x00` and `0x08`. Overlay 184 refills
the current points to the greatest under the condition it tests. The word at `0x4F` was not read
by these routines.

## Alternatives

- The words are another pair the screen labels PSI: the label is printed just before the call at
  the same row in overlay 212, and the stored current value is never above the stored greatest.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `trampoline_target.py <dsun> 56CC:003E` and
`56CC:0043`, `overlay_listing.py <dsun> 183 0x6C87B 0x6C980`, `184 0x6D626 0x6D6B1` and `212
0x985F1 0x98716`, `direct_callers.py <dsun> 183+189B 183+191D` and `immediate_search.py <dsun>
2E09 2E13`. Read the strings at file `0x4FE09` and the words at `0x0C`, `0x4F` and `0x51` of each
`CHAR` resource of `CHARSAVE.GFF`.

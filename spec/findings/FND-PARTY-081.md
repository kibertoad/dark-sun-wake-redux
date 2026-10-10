---
id: FND-PARTY-081
title: In play overlay 210 +08BC raises each class position's level while the experience reaches the DATA 1000 word at the level, up to 15, after cutting the experience to the word three places on, and each level adds a hit die roll and recomputes the greatest hit points and psionic points
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009581C..0x00095AC6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000956A0..0x0009581C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095074..0x00095261
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00094FDA..0x00094FE2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000437EE..0x00043811
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27E5:00C1..27E5:01F3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000796E6..0x00079794
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A284..0x0007A308
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:00EB..277B:010C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0C4..0x0005B0D7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00074560..0x0007458E
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py, store_values.py), and a Python 3.14.7 reading of RESOURCE.GFF and CHARSAVE.GFF
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The details records are 66 bytes each in the table at `DS:19C5` and the combatant
records 49 bytes each in the table at `DS:19C9`; for party slot `s` the byte at `4F49:0C33` plus
3 times `s` and the word after it hold the slot's state and its combatant number, and a combatant
record's word at `+0x04` holds its details number (FND-PARTY-073, FND-PARTY-074). `DATA` 1000 and
the row of a class code, the byte at `4E4F:009D` plus 2 times the code, less 1, are as FND-PARTY-058
and FND-PARTY-083 give; overlay 185 `+0000` (trampoline `56E6:0020`) with 1000, a row and a column
returns the word there.

**The entry points.** Overlay 210 `+0B44` (trampoline `57B9:0057`) calls overlay 199 `+0C21`
(trampoline `576C:0039`) and then `+08BC` with 0; `+0B55` (trampoline `57B9:006B`) does the same
with 1 (`+0B44..+0B65`).

**Overlay 210 `+08BC`** takes a flag. For each slot 0 to 3 it goes on only when the slot's state
byte is 2, the byte at `+0x14` of the slot's combatant record is at most 2 (unsigned) and the word
at `+0x0E` of the details record numbered by the slot is not 0 (`+08C4..+090F`). For each class
position 0 to 2 of that details record, ending after position 0 when the origin byte at `+0x12`
is 1 and passing over a class byte of 0 (`+0912..+0947`), with `L` the position's level byte at
`+0x1E` and `E` the experience dword at `+0x00`:

1. When `DATA` 1000 at the row and column `L + 3`, times 100, is below `E` (unsigned), it stores
   that value as `E` (`+094A..+09E4`).
2. When the word at column `L`, times 100, is above `E` and the flag is 0, it passes to the next
   position (`+09E4..+0A38`).
3. When `L` is 15 or more (unsigned) it passes to the next position (`+0A38..+0A4F`).
4. It formats `%Fs is %d%s level %Fs` (`DS:2B68`) with the name at `+0x21` of the combatant record
   numbered by the slot, `L + 1`, the suffix `th` for `L + 1` above 3, `rd` for 3 and `nd`
   otherwise, and the class name through the far pointer at `DS:1483` plus 4 times the code, and
   when the text is longer than 29 characters formats `%Fs gains a level` (`DS:2B87`) instead; it
   shows the text through overlay 172 `+05DB` (trampoline `566A:002A`) (`+0A4F..+0B13`).
5. It calls `+0740` with the slot and the class code (`+0B13..+0B1E`).
6. With the flag 0 it takes the same position again; with the flag 1 it goes to the next
   (`+0B1E..+0B31`).

After the positions it calls `+0131` with the slot (`+0B31..+0B37`).

**Overlay 210 `+0740`** (trampoline `57B9:0061`) takes a slot and a class code. It keeps `D`, the
word at `+0x08` of the details record numbered by the combatant record's word at `+0x04`, less the
word at `+0x00` of the combatant record (`+0753..+079A`). In the details record numbered by the
slot it finds the first position whose class byte equals the code (`+079F..+07BA`, `+0871..+0880`)
and there adds 1 to the level byte (`+07CB`) and calls `+0000` with the slot, the code and the new
level (`+07CF..+07F1`). When the byte at `+0x39` of that position is below the new level (signed)
it stores the new level there; otherwise it clears a flag that starts at 1 (`+07F1..+0823`). It
calls `27E5:000F`, `+0365` and `+03EC` with the slot, storing the last result in the byte at
`+0x16` of the combatant record numbered by the slot, and `+0572` (`+0823..+084B`). When the code
is 11 and the flag is set it calls overlay 209 `+0000` (trampoline `57B0:0066`) with the slot, and
when the code is 12 and the flag is set overlay 209 `+0A02` (trampoline `57B0:0061`)
(`+084B..+0871`). It then stores the result of `+0233` for the slot in the details record's word
at `+0x08`, and that word less `D` in the combatant record's word at `+0x00` (`+0880..+08B5`).

**Overlay 210 `+0000`** returns before rolling when the sum of the three bytes at `+0x39` is
above the sum of the three level bytes (signed compare, `+0058..+0082`), as FND-PARTY-074 gives;
when the sums are equal it rolls.

**Overlay 210 `+0233`** (trampoline `57B9:002A`) takes a slot. `+0114` returns the sum of three
signed bytes. With `L` the sum of the three level bytes, `R` the word at `+0x0A`, and `M` the sum
of the three bytes at `+0x39`, it takes the low word of `L * R` (signed), divides it by `M`
(unsigned, high word 0), divides that by the result of `+02BA` (unsigned), adds the result of
`+0301` for the slot (the constitution bonus of FND-PARTY-074), and returns `M` when the total is
below `M` (signed) and the total otherwise (`+023B..+02B6`). `+02BA` returns 1 when the origin
byte is 1, and otherwise the number of nonzero class bytes, 1 when there are none
(`+02BE..+02FE`).

**Overlay 210 `+0131`** (trampoline `57B9:005C`) takes a slot and reads the combatant and details
records numbered by it. Starting from 0 it adds the signed byte at `4E4F:00EF` plus the
constitution at `+0x1B` of the combatant record when that is 15 or more, the byte at `4E4F:00F7`
plus the intelligence at `+0x1C` when that is 15 or more, and, when the wisdom at `+0x1D` is 15 or
more, the byte at `4E4F:00FF` plus the byte at `+0x1D` of the record the far pointer `DS:142D`
points to (`+0168..+01C9`). With `p` the result of `27E5:00C1` for the slot and 12: when `p` is
above 0 it adds `(p - 1) * 10` and, when the wisdom is 15 or more, the byte at `4E4F:0107` plus
the wisdom times `p - 1`; otherwise it adds 4 times the result of `27E5:0154` for the slot
(`+01C9..+0212`). When the details record's word at `+0x0C` is below the total (unsigned) it adds
the difference to the combatant record's word at `+0x02` and stores the total at `+0x0C`
(`+0212..+022F`).

The bytes those reads reach for scores 15 to 25 (file `0x437EE..0x43811`): at `00EF` plus the
score 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2; at `00F7` plus the score 0, 1, 2, 3, 4, 5, 6, 0, 20, 22, 24;
at `00FF` plus the score 20, 22, 24, 26, 28, 30, 32, 34, 0, 1, 2; at `0107` plus the score 0, 1, 2,
3, 4, 5, 6, 7, 8, 8, 10.

**`27E5:00C1`** takes a combatant and a class code, finds its details record through `2D40:3E64`,
and returns the level byte of the first position holding the code, or 0 when none does; for an
origin byte of 1 it returns 0 when the position is not 0 and position 0's level is not above that
level (`+00C4..+0151`). `27E5:0154` returns position 0's level for an origin byte of 1; for other
origins the greatest level byte of the positions with a nonzero class byte (`+0154..+01F3`). Both
return 0 when `2D40:3E64` finds the slot's state byte other than 2 or its numbers out of range.

**The callers.** `direct_callers.py` finds no call of `+08BC` other than those of `+0B44` and
`+0B55`, of `+0740` other than `+0B18`, and of `+0131` other than `+0B33`. It finds `+0B44` called
from overlay 173 `+07AD`, overlay 188 `+16E9` and overlay 190 `+1444`, and `+0B55` from overlay
190 `+144C`.

- Overlay 173 `+07AD` runs when the byte at `DS:13FB` is 0, and is followed by a call of overlay
  188 `+0101` (trampoline `5702:0101`) (`+07A4..+07B7`).
- Overlay 188 `+16E9` ends a routine that stores the dword at `[bp - 8]` at `+0x04` of a record and
  at `+0x00` of the details record numbered by `[bp - 2]` (`+16C0..+16DC`); it runs when the word
  at `4C10:0019` is 0 (`+16DC..+16EE`).
- Overlay 190 `+13A6` looks up a key word among 33 words at `+1F44` of the overlay (file
  `0x7A284`) and jumps through the word 33 words later. Keys `0x1454` and `0x1474` (T and t) go to
  `+13E2`, which goes on only when the byte at `DS:143C` is not 0. For `0x1454` it stores 3,750,000
  in the experience dword of the details record of each slot whose state byte is 2 and calls
  `+0B44`; for `0x1474` it calls `+0B55` (`+13E2..+1454`). `store_values.py` finds one store to
  `DS:143C`, of 1 at `278B:0007`, made when `1000:3787` returns 0 for the text two bytes into the
  string at the near pointer `[di + 2 * si]` and the string `911` at `DS:06BD`
  (`277B:00EB..277B:010C`).

**The stored characters.** In the installed `CHARSAVE.GFF`, the details chunk of `CHAR` 50 holds
classes 11, 12 and 17 at levels 15, 14 and 15 with experience 1,400,000, the bytes at `+0x39`
equal to the levels and the word at `+0x0A` 111.

## Interpretation

In play a character's levels rise only through overlay 210 `+08BC`, which runs after experience is
given (overlays 173 and 188) and from a debug key. For each class, or only the current class of a
human, it raises the level one at a time while the experience reaches the `DATA` 1000 word at the
level times 100, the experience at which the next level starts, and stops at level 15. Before it
tests a class it cuts the experience to the start of the level four above the current one, so one
award raises a class at most four levels where the row rises, and the experience above that point is
lost. Each new level shows a message, adds a hit die roll to the word at `+0x0A` when the total of
the levels is not below the total of the greatest levels reached (`+0x39`), raises the greatest
level reached, and sets the greatest hit points to `max(M, (L * R / M) / classes + constitution
bonus)`, the rolled dice scaled by the levels held against the greatest levels reached; the current
hit points keep the same distance below the greatest. After the classes it recomputes the greatest
psionic strength points from the constitution, intelligence and wisdom bonuses, 10 per psionicist
level above the first plus a wisdom bonus per such level, or, without psionicist levels, 4 times the
greatest level (a human's current level), and raises both the greatest and current points when the
result is higher; it never lowers them.

With row 5 of `DATA` 1000, which code 12 (Psionicist) reads, the word at column 17 (14,000) is
below the word at column 14 (15,000), so at level 14 the cut leaves 1,400,000, below the 1,500,000
level 15 needs. The stored `CHAR` 50 holds a level-14 psionicist at that figure.

The debug key gives every party member 3,750,000 experience and the levels it reaches with T, and
one level in each class with t, when `DS:143C` is set by the string `911`, which looks like a
command-line switch.

## Alternatives

- The wisdom term reads the score from the record at `DS:142D`, which overlay 184 sets to the
  generation combatant record (FND-PARTY-076), while it tests the slot's own wisdom; which record
  `DS:142D` points to when `+0131` runs in play was not read, so whether this term uses another
  character's wisdom is open.
- The routines `+0365`, `+03EC`, `+0572`, `27E5:000F`, overlay 199 `+0C21` and overlay 209 `+0000`
  and `+0A02`, which a level calls, were not read; they may change more of the records.
- The string compared with `911` may come from somewhere other than the command line; the routine
  around `277B:00EB` was not read.
- What overlay 173 `+07AD` and overlay 188 `+16E9` give experience for was not read.
- Stores through an index register only, or block copies into a details record, were not searched
  for; FND-PARTY-082 lists the stores with a displacement.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 210 94F60 95261`, `210
956A0 95AC6`, `185 6F640 6F6B0`, `190 79690 79794`, `173 5B090 5B0E0` and `188 74560 745A0`;
`resident_listing.py <dsun> 27E5:00C1..27E5:01F3 2D40:3E64..2D40:3EBF 277B:00D0..277B:0110
278B:0000..278B:0040`; `direct_callers.py <dsun> 210+0B44 210+0B55 210+08BC 210+0740 210+0131
210+0233`; `trampoline_target.py <dsun> 56E6:0020 576C:0039 57B0:0066 57B0:0061 57B9:0057 57B9:0061
57B9:0066 57B9:005C 57B9:006B 57B9:002A`; and `store_values.py <dsun> 143C`. Read the 33 words at
file `0x7A284` and the 33 after them, the bytes at file `0x436F0` plus `0xEF` to `0x121`, and the
string at file `0x4D000` plus `0x6BD`. Read `DATA` 1000 in the installed `RESOURCE.GFF` at
`0x1858B`, and in `CHARSAVE.GFF` the 66 bytes from the experience dword 1,400,000 at `0x55C`.

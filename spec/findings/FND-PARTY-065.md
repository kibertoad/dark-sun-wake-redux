---
id: FND-PARTY-065
title: When a class is chosen in generation, overlay 183 +1323 sets each class to level 7 for one class or 6 for more, raises experience to the greatest threshold, and then for two classes tests the class positions in reverse order, so the first class is never raised and the empty third one is tested against DATA 1000 row -1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B8BB..0x0006B8FC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BAB3..0x0006BBCC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006BC71..0x0006BC81
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006C303..0x0006C504
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `DS:1429` points at the generation working record `4E4F:0029` while a character is
made (FND-PARTY-063).

**Overlay 183 `+08DB`** (trampoline `56CC:009D`) takes three words and returns 4 if the first is
nonzero, plus 2 if the second is, plus 1 if the third is (`+08E0..+0918`).

**Overlay 183 `+0A3E`**, after changing a class byte (FND-PARTY-063), passes the class bytes at
`+0x1B`, `+0x1C` and `+0x1D` to `+08DB` and jumps through the eight words at `+0C91` (file
`0x6BC71`): masks 1, 2, 3 and 5 move the class bytes forward so that the nonzero ones come first
(`+0B38..+0B76`), and mask 0, 4, 6 and 7 leave them. When its flag argument is nonzero it then calls
`+1323` (`+0BE9`), its only caller found by `direct_callers.py`.

**Overlay 183 `+1323`** (trampoline `56CC:0093`) keeps the mask of `+08DB` for the three class
bytes in the byte at `[bp - 4]`, and sets the byte at `[bp - 3]` to 1 when the mask is 1, 2 or 4
(`+132D..+1362`). It sets the level bytes at `+0x1E`, `+0x1F` and `+0x20` and the dword at
`+0x00` to 0, and zeroes three dword locals and the dwords at `DS:42E8`, `DS:42E4` and `DS:42E0`
(`+1366..+139E`). Then for each position from 0 to 2 whose bit `4 >> position` is set in the
mask (`+13A7..+13B2`):

- it stores 7 in the level byte when `[bp - 3]` is 1 and 6 otherwise (`+13B7..+13D2`);
- it calls overlay 185 `+0000` through trampoline `56E6:0020` with 1000, the class byte less 1
  and the level less 1, multiplies the dword result by 100, keeps it in the position's local, and
  stores the greater of it and the dword at `+0x00` into that dword, compared unsigned
  (`+13D7..+144A`).

When `[bp - 3]` is 0 it goes over the positions from 0 to 2 again, now taking a position when bit
`1 << position` is set in the mask (`+1466..+1479`). For such a position it calls overlay 185
`+0000` with 1000, the class byte less 1 and the level byte, multiplies by 100, and when that is
not above the dword at `+0x00` (unsigned) adds 1 to the level byte and stores in the local the
entry for the new level byte, times 100 (`+147E..+14FB`). It ends by copying the three locals to
`DS:42E8`, `DS:42E4` and `DS:42E0` (`+150A..+151E`).

Overlay 185 `+0000` reads the word at the `DATA` 1000 resource plus 40 times the row plus 2 times
the column with no bound on either (FND-PARTY-058).

## Interpretation

Choosing a class in generation gives the character the experience of the class's level 7 alone,
or with two or three classes level 6 in each and the experience of the greatest of their level-6
thresholds, and then lets each class whose level-7 threshold that experience reaches go up one
level. The class bytes then hold codes 1 to 8, whose rows of `DATA` 1000 these are
(FND-PARTY-063).

The second pass tests the mask with the bits in the reverse order of the first. With three
classes (mask 7) both orders take every position. With two (mask 6, since `+0A3E` puts them first)
it skips position 0 and takes positions 1 and 2. So the first class never gets the extra level,
and position 2, whose class byte is 0, is tested against row -1 of `DATA` 1000 (the 40 bytes before
the resource) at column 0; if the word there, times 100, is not above the experience, the empty
position's level byte becomes 1. One class (mask 4) skips the second pass.

## Alternatives

- The reverse order could be intended, raising the later class rather than the first. It also
  takes the empty position, which has no row, and with three classes it takes all three, so the
  reading of a slip in the bit order fits better; the code alone does not show intent.
- What the words before the `DATA` 1000 resource hold depends on the memory allocator, which was
  not read, so whether the empty position gets level 1 is open.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 183 6B8BB 6B8FC`,
`183 6BA1E 6BC40` and `183 6C303 6C504`; `direct_callers.py <dsun> 183+1323 183+08DB`;
`trampoline_target.py <dsun> 56E6:0020`. Read the eight words at file `0x6BC71`.

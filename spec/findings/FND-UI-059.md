---
id: FND-UI-059
title: Each start-window button blinks three times and then closes the window, Start Game after a fade to black, the money set to 5000 and the game-entry routine, Create Characters and Load Saved Game after saving the screen, and Exit to DOS by clearing the main loop's byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081130..0x00081457
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081634..0x000816C9
tool: Capstone 5.0.9 over overlay code; tools/research/exec-census/overlay_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 194's code starts at file `0x81130`.
FND-UI-035 reads the opener and the callback `+0000`, whose event-2 branch jumps through a table
of four targets for the buttons 19300 to 19303. `50C1:0000` holds the start window's far pointer.

**The helper `+0504`** (`+0504..+0599`) takes a byte argument.

- With 1 (`+0510..+0560`) it calls `3D72:0B84`, `1BF3:4FEB` with 0 and 0, `444C:0008` with 256
  and 3, `3796:0514` with the block returned, 0 and 256, `444C:0092` with that block, and
  `3D72:0942`.
- With 0 (`+0562..+057C`) it calls `3D72:0B84`, `2707:00B4` with 0 and 1, and overlay 190
  `+3169` with 1.
- Both paths then call `2C5F:0182` and overlay 182 `+0128` with the pointer at `50C1:0000` and 0,
  and return far.

**The branches.** Each of the four first runs a loop three times (`si` from 0 to 2): `3EBE:071F`
with the start window, the button number and 4, `28C9:2A81` with 1, `3EBE:071F` with 5 and
`28C9:2A81` with 1. Then:

| Button | Range | Steps after the loop |
|---|---|---|
| 19300 | `+013E..+01F4` | helper with 1; byte `4E71:0004` = `0x31`; overlay 187 `+2424` with `DS:140C`; overlay 187 `+206E` with the double word it returns; overlay 182 `+1167` with 0; overlay 182 `+0B52` with 0, 1, 2 and 3; double word `4C13:0357` = 5000; when word `+6` of the record at the far pointer `DS:19C9` is `0x8300` and `DS:140C` is `0x32`, a jump to `+024A`, otherwise to `+030E` |
| 19301 | `+01F4..+0261` | helper with 0; at `+024A`, overlay 212 `+061C` with the far pointers `DS:19C5` and `DS:19C9` and 0; then `+030E` |
| 19302 | `+0261..+02BE` | helper with 0; overlay 192 `+0000` with no arguments; then `+030E` |
| 19303 | `+02BE..+030E` | a jump to `+0315`, the path event 1 also takes (`+0007..+0010`): helper with 1; byte `DS:1462` = 0; return 0 |

`+030E` calls `2C5F:0182` and returns 0.

## Interpretation

By FND-IMAGE-012 and FND-UI-049, the helper with 1 fades the colours in use to black, writes an
all-black palette from the zero-filled 768-byte block and releases it; by FND-PARTY-195 and
FND-PARTY-027, the helper with 0 copies the shown page onto the hidden one and on to the
full-screen reservation. Overlay 182 `+0128` is the window close (FND-PARTY-045), so both paths
close the start window, and the press loop blinks the button three times (FND-PARTY-077,
FND-PARTY-180).

Start Game shows the loading picture (FND-EXE-596, FND-PARTY-022), enters the game through the
gate routine (FND-UI-049, FND-PARTY-170), sets each member's spells per level (FND-PARTY-111),
and sets the party's money (FND-EXE-607) to 5000; the main loop keeps running with `DS:0D9C` now
1. Create Characters opens `WIND/11500` (SCR-UI-002, FND-PARTY-084). Load Saved Game opens the
load screen, `WIND/18500` in load mode (FND-UI-036). Exit to DOS fades to black and clears
`DS:1462`, the byte the resident main loop repeats on (FND-SAVE-012), after which the program
exits through the cleanup FND-SAVE-012 reads; no window opens in between.

## Alternatives

That `3D72:0B84` and `3D72:0942` hide and show the pointer was not read. When the Start Game
test of `0x8300` and region `0x32` is true, so that the character window opens after a new game,
was not read; `0x8300` is the identifier of `CHARSAVE.GFF#CHAR/33` and `/53` (FND-PARTY-050,
FND-PARTY-053). Which loading picture `DATA/1002` picks for the starting region was not read.

## How to reproduce

`overlay_listing.py <dsun> 194 81130 81457` and `... 194 81634 816C9`.

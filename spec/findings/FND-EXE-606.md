---
id: FND-EXE-606
title: The 8 routines of overlay 193 that no entry placed lay a cast's area out as placed entries of a new effect slot, flash a cast's area points and remove the object, sum caster levels over a slot's effect rows, and gather effect slots in a cone, a strip, a box or a circle
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007EAE1..0x0007ED9D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FD1D..0x0007FD94
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000803B6..0x00080580
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008072B..0x00080CDB
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 8 rows of overlay 193 (header segment `573B`, 44
trampolines, code from file offset `0x7E2F0`, 11,230 bytes) that no spec location covered, 2,733
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x7E2F0` plus it, and `DS` is `57E0`. Every routine has a trampoline in `573B`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A placed entry is one of the 8-byte entries under the far pointer
`DS:67B7`, `DS:264E` of them in use, with its position in words `+0` and `+2` and its object slot
in word `+6` (FND-COMBAT-054). A slot's 37-byte record is at `DS:67BB + 37 * slot`, and its byte
`+0x0F` is 0 or one more than the number of an 18-byte record `R` of the table at `51F1:0000`
(FND-PARTY-106). An effect row is one of the 10-byte rows at `4E71:024D`, `DS:2100` of them,
with its owner in word `+0`, two words at `+2` and `+4` and its code in byte `+6` (FND-COMBAT-031).
A slot list is an array of 4-byte entries in which these routines write only word `+2`, the slot.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+07F1` | `0x7EAE1..0x7ED9D` | `573B:0093` | Takes `x`, `y`, a near buffer of `n` point pairs, `n`, `R` word `0x0E`, the amount (a double word), `R` word `0x10`, the record number `r`, `a` and `g`, as overlay 193 `+0617` passes them (FND-RNG-052). Calls `+29EB` with `r`, the amount plus the absolute values of the two `R` words plus 1, -1, `0xFF`, `a`, `g`, 0 and the double word under the far pointer `DS:9C0C` divided by the signed byte `DS:9C0A`, and returns at once when the slot it returns is negative. When the high four bits of `R` byte 5 are 0 it uses one point instead of `n`. For each point `(x + dx, y + dy)`, with `c` its cell (each coordinate divided by 16), it skips the point when `25AF:00E1` of `c` is nonzero and `25AF:0576` of `c` is not 1, or when `1B0B:00A4` from `(x / 16, y / 16)` to `c` is nonzero, and otherwise places the slot there through `31E0:03CA` (slot, position, 0, 0), skipping the point when that returns -1. For a placed entry it calls `25AF:017F` with the entry's cell when bit 5 of `R` byte 2 is set, `25AF:042A` with `R` word `+0` and the cell, and `+0AAD` with the entry and 0. When `+0AAD` returns nonzero, or a counter has passed `R` word `0x0E`, it calls `31E0:3EF2` with 0 and the slot and `31E0:1478` with `DS:1408`, `DS:140A`, 1, 1 and 1, calls `2C5F:06CB` with 1 and 1 when that returns 0, and resets the counter; otherwise it counts. After the points, when bit 4 of `R` byte 2 is set, it calls `25AF:01AE` with the cell of every placed entry of the slot. Ends with `2C5F:0139` and `28C9:2A81` with 1. Called from `+0617` at `+07E8`. |
| `+1A2D` | `0x7FD1D..0x7FD94` | `573B:0052` | Takes a slot and a code byte. From the first effect row of the slot with that code (`3143:0061`), while rows belong to the slot and the index is below `DS:2100`, adds overlay 211 `+1FB5` of each row's words `+2` and `+4` for the rows whose code is the byte, and returns the sum. Called from overlay 179 `+1DF1`, overlay 195 `+0385` and `+14A6`, overlay 197 `+27F1`, `+2845`, `+289E` and `+2FDF`, and overlay 204 `+0B36` and `+0B51`. |
| `+20C6` | `0x803B6..0x803CA` | `573B:0098` | Calls overlay 176 `+094F`, overlay 179 `+0C76` and `371D:0770`, with no arguments. Called from overlay 180 `+0641`. |
| `+20DA` | `0x803CA..0x80580` | `573B:00E8` | Takes `x`, `y`, a far buffer of `n` point pairs, `n` and the record number `r`, as `+0617` passes them. When `x` is outside `DS:1408` to `DS:1408 + 320` or `y` outside `DS:140A` to `DS:140A + 200`, calls `2C5F:01F3` with `x`, `y` and 1. Places an object of the number in `R` word `+3` at `(x, y)` through `31E0:0121` (with -1, 0, 0 and 3), going to the end when that returns -1. Calls `28C9:2A25`, `362C:01CD` and `2707:00B4` with 1 and `DS:13FE`. It draws the object's image (word `+0x10` of its 37-byte record) at each point through `31E0:328F` with 0, at the record's position words `+3` and `+5` plus the point less `DS:1408` and `DS:140A`; then for `k` from 6 down to 1 draws the first `n * k / 8` points the same way with 1, calls `1BF3:5814` with -1 and `2707:00B4` with `DS:13FE` and 1. Then `1BF3:5814` with -1 and `2707:00B4` with 0 and 1. With an object placed it removes it through `31E0:05BB` (its record word `+1`, 1, 1, 1) and calls `28C9:2A81` with 1 and `28C9:2A44` with 0. Called from `+0617` at `+07B3`. |
| `+243B` | `0x8072B..0x808B0` | `573B:00B1` | Takes a `DATA` number `d`, a position, a range, two directions and a slot list. Lists each placed entry whose slot is not negative, has a nonzero byte `+0x0F` = `b` and is not in the list yet, and passes these tests: bit 6 of byte 2 of record `b` of `51F1:0000` set when bit 1 of byte `+0x1A` of `d`'s record (`2C5F:0003`) is set; bit 7 of that byte set when `d` is `0xE7`; bit 1 of word `+0x1A` of the `DATA` record that record `b`'s word `+0` names set when `d` is `0xB5`; distance from the position (`2D40:382F`) at most the range; and direction from the position (overlay 179 `+09CE`) less the first direction, modulo 256, at most the second less the first. Stops at 64 slots or at `DS:264E` entries and returns the count. Called from overlay 208 `+025A`. |
| `+25C0` | `0x808B0..0x80A2B` | `573B:00B6` | Takes `d`, `x`, `y0`, `x'`, `y1`, a width `w` and a slot list. Swaps `y0` and `y1` when `y0` is the larger and then uses `x'` as the centre; otherwise uses `x`. Lists entries as `+243B` does, with the same tests on `b` and `d`, whose `y` lies from `y0` to `y1` and whose `x` lies within `w / 2` of the centre (the half rounded toward zero). Called from overlay 208 `+017A`. |
| `+273B` | `0x80A2B..0x80B94` | `573B:00BB` | Takes `d`, two corners `(x0, y0)` and `(x1, y1)` and a slot list. When `x0` is above `x1` it stores `x1` in `y0` and `x0` in `x1`, leaving `x0`; then it swaps `y0` and `y1` when `y0` is above `y1`. Lists entries as `+243B` does whose position lies from `x0` to `x1` and `y0` to `y1`, walking 320 entries whatever `DS:264E` holds. No direct call was found. |
| `+28A4` | `0x80B94..0x80CDB` | `573B:00C0` | Takes `d`, a position, a range and a slot list. Lists entries as `+243B` does within the range of the position, except that it uses `b - 1` in place of `b` for every record test and skips slots whose byte `+0x0F` is below 2. Called from overlay 208 `+01C7` and `+02BA`. |

## Interpretation

`+07F1` lays out the area of a cast whose effect row is not of kind 1: it makes one effect slot
for the record and places it once at every reachable point the area sampler gave, applying the
area effect at each (`+0AAD`, which reaches `+12A8`) and redrawing the screen every few points,
which fits a lasting field such as a wall or cloud. `+20DA` handles kind 1 rows: it shows an
object at the caster's point, draws the area's points with the object's image, thins them over six
steps and removes the object, which fits a burst that leaves nothing behind. `+1A2D` gives the
summed caster level of a slot's effects of one code. `+243B`, `+25C0`, `+273B` and `+28A4` are the
cone, strip, box and circle gatherers of overlay 173 (FND-EXE-600) applied to effect slots rather
than combatants, and overlay 208 calls three of them. The first assignment in `+273B` writes the
wrong variable, so a box given with `x0` above `x1` is tested against `y0 = x1`. The names stay
`fn_193_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+273B` in particular may be called only through its trampoline
by a computed far call. `+29EB`, `31E0:03CA`, `31E0:0121`, `31E0:328F`, `31E0:3EF2`, `31E0:1478`,
`25AF:017F`, `25AF:042A`, `25AF:01AE`, `2707:00B4`, `1BF3:5814`, overlay 176 `+094F`, overlay 179
`+0C76` and `371D:0770` are not read here; the reading of them as making a slot, placing it,
placing an object, drawing an image, redrawing, setting cell bits and showing a frame rests on
their arguments and on FND-COMBAT-054. The skip of `b = 1` in `+28A4` may be a deliberate exclusion
of record 0 rather than an off-by-one.

## How to reproduce

Run `overlay_listing.py` for overlay 193 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`193+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Read `+0617..+07F0` for the arguments it passes.

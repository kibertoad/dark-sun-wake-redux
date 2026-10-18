---
id: FND-INPUT-013
title: Pressing the right button over the map frame steps the word at 57E0:1440 from 1 to 4 to 2 and back to 1, through 3 when 57E0:1A34 is not 0xFFFF, once per press
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1261..28C9:152C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2535..28C9:254C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2608..28C9:2839
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0C8D..2C5F:0D30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069FD3..0x0006A0A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C1F0..0x0007C25B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068DEC..0x00068E6C
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, store_values.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

`DS` is `57E0` in the installed `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`). Image numbers are given in decimal; `0x4A9D` is 19101.

**The callback.** Overlay 182 `+1167` gives the full-screen frame `APFM/19200` of
`WIND/19501` the callback `28C9:1261` with the mask `0x01EE` (FND-UI-044), which holds the
event codes 2, 4, 8, 0x20, 0x40, 0x80 and 0x100 of FND-PARTY-154. `28C9:1261` acts only on
events of kind 5 and finds the code in the five words at `28C9:1518`, 8, 0x20, 0x40, 0x80 and
0x100, whose handlers are the next five words, `28C9:1295`, `28C9:1501`, `28C9:130A`,
`28C9:1508` and `28C9:14C1`. The event's words at `+0x0C` and `+0x0E` are the pointer's column
and row.

**Right button down** (code 0x100, `28C9:14C1..28C9:1514`). Nothing happens when `combat_state`
(`4C10:0019`) is not 0 and `turn_combatant` (`DS:426D`) is 4 or more, when the word `DS:0DAB` is
not 1, or when the byte `DS:0DA3` is not 0. Otherwise the handler stores 1 in `DS:0DA3`, copies the
24-byte event to the stack and calls `56BD:00E3`, the trampoline of overlay 182 `+1783`.

**Right button up** (code 0x80, `28C9:1508..28C9:1514`). When `DS:0DAB` is 1 it stores 0 in
`DS:0DA3`. A search for stores to `DS:0DA3` finds 1 only at `28C9:14E5`; the others, at
`28C9:150F` and overlay 182 `+1A00`, store 0.

**The step** (overlay 182 `+1783..+184C`, file `0x69FD3`). It returns at once unless
`combat_state` is 0 or 1 or the word `DS:44E6` is at most 4, and unless `DS:0DAB` is 1. It then
jumps on the word `DS:1440` less 1 through the five words at `+184C`, `0x17C3`, `0x17F4`,
`0x181B`, `0x17DE` and `0x1804`; a value outside 1 to 5 goes to `+181B`.

| `DS:1440` before | Code | `DS:1440` after | Image routine |
|---|---|---|---|
| 1 | `+17C3` | 4 | `si` = `28C9:2608` with the column and row; calls `362C:06B9`; then overlay 182 `+059C` with `si` and 1 |
| 4 | `+17DE` | 2 | `si` = `28C9:2608` with the column and row; then `2C5F:0C8D` with `si` |
| 2 | `+17F4` | 3 when overlay 191 `+0000` returns not 0, else as for 3 | `2C5F:0C8D` with 3 |
| 3, or not 1 to 5 | `+181B` | 1 | calls `362C:06B9`; then `2C5F:0C8D` with 1 |
| 5 | `+1804` | 1 | calls `362C:06B9` and overlay 190 `+2503` with 0; then `2C5F:0C8D` with 1 |

Overlay 191 `+0000` (file `0x7C1F0`, trampoline `572F:0025`) returns 1 when the word `DS:1A34` is
not `0xFFFF` and 0 otherwise. Stores to `DS:1A34` are in overlays 175, 187 and 188, with
`0xFFFF`, 0 and a loop index.

**The image for a mode** (`2C5F:0C8D..2C5F:0D30`). It jumps on its argument less 1 through the
five words at `2C5F:0D26`, `0x0CCE`, `0x0CDC`, `0x0CE1`, `0x0D05` and `0x0CA7`. For 1 the image
is 19101 when `turn_combatant` is below 4 and 19110 otherwise; for 2 it is 19107; for 3 it is
what overlay 191 `+0013` (trampoline `572F:0020`) returns when overlay 191 `+0000` returns not 0,
and otherwise 19101 with 1 stored in `DS:4596` and `DS:1440`; for 4 it is 19103; for 5 it calls
`3D72:12ED` with the far pointer at `4E71:0A5D` when that is not null, stores 1 in the byte
`DS:9BE7` and returns; any other argument gives 19110. When the image differs from the word
`DS:4273` it stores it there and calls `56BD:006B` with the image and a flag, 0 for the image of
overlay 191 `+0013` and 1 otherwise. Overlay 191 `+0013` returns 19103 when `+0000` returns 0;
otherwise it returns the word `+0x0C` of the 23-byte record that the word `5072:0044` selects in
the table at the far pointer `DS:19C1`, after it fills that word from overlay 190 `+2F2F` when
it is 0.

**The image under the pointer** (`28C9:2608..28C9:2839`). With the column and row it starts at
19101 and, by `DS:1440`: for 2 it returns 19108 when `31E0:2DBD` finds no object slot at the
point, and otherwise 19107 or 19108 by the slot's checks (`28C9:263E..+26FC`); for 4 it returns
19104 when the slot is `turn_combatant`'s, and otherwise one of 19103 to 19106 by the slot's
checks or, with no slot, by `2D40:3559`; for 1 it returns
19102 when no slot is at the point and `25AF:00E1` returns not 0, and 19101 otherwise; for any
other value it returns 19101. Overlay 182 `+059C` takes an image number and a flag and, when
they differ from `DS:0FCC` and `DS:0FCF`, loads `ICON` (flag 1) or `BMP` (flag 0) with that
number through `38FF:05B5` and `38FF:04AB`.

**Pointer movement** (code 8, `28C9:1295..28C9:130A`). With the same `combat_state`,
`turn_combatant` and `DS:0DAB` checks as the press, and when `DS:1440` is neither 5 nor 3 and the
column is from 1 to 317 and the row from 1 to 198, it calls `56BD:006B` with the image
`28C9:2608` gives for the column and row and 1.

**The other writers of `DS:1440`.** `28C9:2535` stores its argument in `DS:4596` and `DS:1440`
and calls `2C5F:0C8D` with it. Other direct stores give 1 at `28C9:31BC`, overlay 182 `+13FC` and
`+1BD0`, and overlay 211 `+0925` and `+1692`; 0 at `28C9:3539`, in the pan routine `28C9:34B1`;
5 at overlay 208 `+1643`; and a saved value at `28C9:24FD`, `28C9:2516`, `28C9:3635` and overlay
182 `+1C1C`.

## Interpretation

The word at `DS:1440` is the pointer mode, with 1 Walk, 4 Attack and 2 Look, the order the
manual lists. A right-button press over the map steps it Walk, Attack, Look and back to Walk;
when `DS:1A34` is not `0xFFFF`, Look steps to a fourth mode, 3, whose pointer is an image taken
from an object record, and that mode steps back to Walk. Mode 5, which overlay 208 sets, steps to
Walk. The byte `DS:0DA3` lets one press make one step; the release clears it. The press is
ignored in combat when the turn is not the party's.

After the step from Attack to Look, `2C5F:0C8D` is called with the image number of `28C9:2608`
in place of a mode, which takes its default branch and shows 19110; the next pointer movement
over the frame replaces it with the Look image `28C9:2608` picks.

## Alternatives

- What `DS:1A34` holds, and so when mode 3 is reached and what it is for, is not read; its
  writers are in overlays 175, 187 and 188.
- `362C:06B9`, overlay 190 `+2503`, `3D72:12ED` and `56BD:006B` were not read; that they leave
  `DS:1440` alone is not shown. `DS:44E6`, `DS:4596` and `DS:9BE7` are not identified.
- Which mode the pointer has when play begins depends on which of the writers above runs last;
  that order is not traced.
- That 19110 shows after Attack to Look assumes `56BD:006B` displays the image it is passed.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `resident_listing.py <dsun>` with `28C9:0CE8..28C9:152C`, `28C9:2535..28C9:254C`,
`28C9:2608..28C9:2839` and `2C5F:0C8D..2C5F:0D30`; `overlay_listing.py <dsun> 182` over the
overlay's code range and `191 7C1F0 7C25B`; `store_values.py <dsun> 1440 0da3 1a34`;
`direct_callers.py <dsun> 182+1783 2C5F:0C8D 28C9:2535`; `immediate_search.py` for 19101 to
19110; read the ten words at `28C9:1518` and the five at `2C5F:0D26`. The listings are retained
under `GAME_DIR/analysis/input-pointer-mode-20261019/`. No original function or game process was
run.

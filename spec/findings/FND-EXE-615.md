---
id: FND-EXE-615
title: The 6 routines of overlay 213 that no entry placed open the item view window WIND 15502 and set its two buttons, list the party members who can use an item, read an OJFF record and an item's BMP picture, draw the window's item icon or usable-by list, and draw its frame
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099260..0x000994A3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000994A9..0x00099535
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009A514..0x0009A7E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009A7E5..0x0009A94E
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 6 rows of overlay 213 (header segment `57CE`, 10
trampolines, code from file offset `0x99260`, 6,362 bytes) that no spec location covered, 1,799
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x99260` plus it, and `DS` is `57E0`. Every routine has a trampoline in `57CE`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. An item is a 23-byte record under the far pointer `DS:19C1`; its word
`+0` is its object number, negative or positive, its word `+0x0A` a type `t` that indexes the
15-byte records under the far pointer `DS:19CD` (FND-EXE-600), and its word `+0x10` a `DATA`
number plus 1 or 0. The window is held at the far pointer `55CB:0000`, the item it shows in
`DS:2E62`, and `+02D5` closes it (FND-PARTY-155). `DS:08CC` chooses between the description and
the usable-by list, as the INFO button `0x3BC8` of overlay 178 toggles it (FND-EXE-604).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x99260..0x994A3`, `0x994A9..0x99535` | `57CE:0025` | Takes an item `i`, `x`, `y` and a flag byte. Sets `DS:08CC` to 1, clears the flag when `28C9:2522` does not return 2, and keeps it in `DS:9D8C`. Places `x` with overlay 182 `+07BC` (width `0x12`, `0x6F`) and `y` with `+07DD` (`0x12`, `0x43`), using `0x61` for a `y` of `0x75` or more (FND-EXE-605). Calls `28C9:2ABD` and overlay 182 `+1FDB` with 6, and opens window `0x3C8E` (15502) at `(x, y)` with the handler `+034F` (trampoline `57CE:0048`) through overlay 182 `+016B`, storing it at `55CB:0000`; when that is null it passes `FATAL: Error loading WIND_VIEWITEM` to overlay 180 `+0867`. Gives the window `+14B9` through `3A8E:0D0C` and `+16CE` through `3A8E:0D3C`, stores `i` in `DS:2E62`, and registers `+034F` for control `0x3B60` through `3F96:02AB`. Sets control `0x3BC9` through `3EBE:071F` to a state found by these tests in order: 1 when the flag is 0; 0 when the item's number is 30000 to 31999; 1 when word `+0x10` is 0 or `d`, word `+0x10` less 1, is `0x148` or more; 0 when `t` is `0x270F` or more or `t`'s byte `+5` is 0; 1 when the low two bits of byte 0 of `DATA d` (`2C5F:0003`) are 1, or are 0 while byte `+0x16` of 23-byte record `t` is negative; 1 when `t`'s byte `+5` is 5, `0x0B` or `0x0C`; 0 otherwise. Sets control `0x3BC8` to state 1 when `t` is `0x270F` or more, or when `t`'s byte `+5` and the item's word `+0x10` are both 0. Calls `3A8E:060D` with the window, overlay 182 `+059C` with `0x4A9D` and 1, and `28C9:2A44` with 1. The `jmp` instructions at `0x994A3` and `0x994A6` each follow another `jmp` and nothing reaches them, so the row leaves their six bytes out. Called from overlays 175 `+0732`, 189 `+1273`, 207 `+0530` and 212 `+0B2B`. |
| `+12B4` | `0x9A514..0x9A5E8` | `57CE:0039` | Takes an item and a far window. Sets control `0x3BC9` to state 1, writes `USEABLE BY:` at row `0x0B` and column `0x0D` through `2C5F:09F3` with colours `0xB5` and `0xC2`, and for each `j` from 0 to 3 whose 66-byte record `j` under `DS:19C5` has a nonzero word `+0x0E` and for which overlay 189 `+2B64` (trampoline `5713:002F`) returns nonzero with the item, 0 and that record, writes the name at `+0x21` of the 49-byte record `j` under `DS:19C9` 8 rows lower each time. Writes `NO ONE` when none was written, and returns the count. Called from `+16BC`. |
| `+1388` | `0x9A5E8..0x9A650` | `57CE:004D` | Takes a number `n` and a far pointer `p`. Keeps the current value from `37ED:0000`, selects the double word `DS:1456` with `37ED:0064`, and reads resource `OJFF` `n` through `38FF:04AB` with the address of `p`, setting `p` to 0 when that returns -1; then selects the kept value again. Returns `p`, unchanged when either `37ED` call returns -1. Called from `+1471`. |
| `+13F0` | `0x9A650..0x9A719` | `57CE:0043` | Takes an item. Reads resource `BMP ` numbered by the item's word `+0x0C` through `38FF:04AB` when that word is not 0. When the word is 0 or the read returns -1, it reads `OJFF` of the item's negated word `+0` into a 28-byte stack buffer with `+1388`, returning 0 when that returns 0, then reads `BMP ` numbered by the buffer's word `+0x0C` and stores that number in the item's word `+0x0C`. Returns the picture pointer. Called from `+0C53`. |
| `+14B9` | `0x9A719..0x9A7E3`, `0x9A7E5..0x9A92E` | `57CE:003E` | The window's draw routine; takes the far window and returns 0 at once when it is null. With `DS:08CC` set it calls `+0BFB` with `DS:2E62` and the window, then picks an `ICON` number: when word `+0x10` is not 0, `k` is word `+0x0E` (when `t` is below `0x270F` and has bit 1 of byte `+0x0B` set) or word `+2`, less 1, and the icon is `21000 + k` for `k` below 269, 11103 for `k` from 269 to 318 and `19116 + k - 320` from 319 up; when `+0x10` is 0 the icon is 11103 for an item numbered 30000 to 30999 and none otherwise. It reads the icon through `38FF:04AB`, draws frame 0 of it with `2D40:3BEC` at the window's position (words `+0x96` and `+0x98`) plus `(0x4C, 0x0C)` and frees it with `444C:0092`. With `DS:08CC` clear it sets `DS:08CC` to 1 when `t` is `0x270F` or more or `t`'s byte `+4` has bit `0x10` or `0x20`, and otherwise calls `+12B4` with the item and the window. Ends with overlay 182 `+1FDB` with 4. The `jmp` at `0x9A7E3` follows another `jmp` and nothing reaches it, so the row leaves its two bytes out. No direct call was found; `+0000` passes it to `3A8E:0D0C`. |
| `+16CE` | `0x9A92E..0x9A94E` | `57CE:0034` | Takes a far window and calls overlay 190 `+3456` with `0x0B`, the window's words `+0x96` and `+0x98`, and 0; returns 0. The form of overlay 199 `+0000` with `0x0F` (FND-EXE-603). No direct call was found; `+0000` passes it to `3A8E:0D3C`. |

## Interpretation

`+0000` opens the item view window beside the clicked box, with the INFO button `0x3BC8` and a
second button `0x3BC9`, which it enables only from the screen whose `28C9:2522` mode is 2 and for
items that pass the type tests. `+14B9` draws the window's content: the description from `+0BFB`
and the item's icon, or the `USEABLE BY:` list of party members from `+12B4`, which asks overlay
189 whether each of the four party records can use the item. `+13F0` and `+1388` find an item's
picture, falling back to the picture its object's `OJFF` record names and caching that number in
the item. `+16CE` draws the window's frame as the dialog of overlay 199 does. The names stay
`fn_213_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+14B9` and `+16CE` are reached through the far pointers `+0000`
registers. Overlay 189 `+2B64`, overlay 190 `+3456`, `28C9:2522`, `37ED:0000`, `37ED:0064`,
`3A8E:0D0C`, `3A8E:0D3C`, `3F96:02AB`, `3EBE:071F` and `2C5F:09F3` are not read here; the reading
of them as a can-use test, a frame draw, a mode query, a resource-file switch, handler registration,
a button state setter and a text writer rests on their arguments and on FND-EXE-603 and
FND-EXE-604. The meaning of `0x3BC9` is not settled. In `+0000` the test of byte `+0x16` uses the
type `t` as an index into the 23-byte records, where the item `i` would be expected; this may be a
slip in the original or a deliberate read of another record.

## How to reproduce

Run `overlay_listing.py` for overlay 213 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`213+<offset>`), read the trampolines of `57CE`,
and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

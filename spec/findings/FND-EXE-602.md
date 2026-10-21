---
id: FND-EXE-602
title: The 10 routines of overlay 206 that no entry placed set a slot's frame, move a slot step by step along a line and draw each step, save and restore a 64 by 64 screen area, show a frame of BMP 19001 at one of four places for 40 ticks, place an object, and give one of eight directions between two points
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008EB90..0x0008EC4A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008EC58..0x0008F2D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F39E..0x0008F4D1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F9C3..0x0008FB9F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008FD43..0x0008FE6E
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

After the restage of FND-EXE-601, `npm exec -- standard-coverage` reported 10 rows of overlay 206
(header segment `5799`, 16 trampolines, code from file offset `0x8E950`, 5,884 bytes) that no
spec location covered, 2,920 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x8E950` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `5799`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them, named by the row that holds them. A slot is one of the 320 object
slots whose 3-byte entries are at `4F49:0C33` (FND-PARTY-131), with its 37-byte record at
`DS:67BB + 37 * slot`: the record's word `+1` indexes the 8-byte entries of placed objects under
`DS:67B7` (FND-ACTOR-004), its words `+3` and `+5` and bytes `+0x13` and `+0x14` are a box
(position and size), and its words `+0x0A` and `+0x0C` a position. The view is the 320 by 200
area whose origin is the words `DS:1408` and `DS:140A`. The word `DS:2908` holds a screen buffer
handle, `0xFFFF` when there is none, and the double word `DS:2904` a resource pointer, 0 when
none is loaded.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0240` | `0x8EB90..0x8EC4A` | `5799:0057` | Takes a slot, a far pointer to a word `w` and a flag byte. Returns unless the slot's state byte is 2, and with `w` outside 0 to 7 sets `w` to 0. When the flag is not 1, the 8-byte table at `+02FA` maps `w` 1 and 3 to 2 and 5 and 7 to 6. Calls `31E0:1808` with the slot, takes the word with index byte `DS:1413[w]` plus the flag minus 1 of the 72-byte record under the far pointer `DS:6573` that the slot record's word `+0x16` names, and passes its low byte and the slot to `31E0:271E`. Called twice from `+0000`, from resident `31E0:0121` (at `31E0:03BB`) and from overlay 204 `+259D`. |
| `+0308` | `0x8EC58..0x8F28A` | `5799:002A` | Takes a slot, a target `x` (clamped to 0..`0x7FF`), `y` (clamped to 0..`0x61F`) and a step byte `n`. Sets bit `0x10` of byte `+5` of the slot's placed-object entry, and when the slot's box is outside the view calls `2C5F:018C` with the slot and 1. Builds a list of points from the slot's position divided by `n` to the target divided by `n` with `1B0B:0212` (into a `0x4B0`-byte stack buffer) and `1B0B:017E`, calls `31E0:1808`, gets a direction from overlay 179 `+09CE` and from `+13F3`, and passes word `2 * direction` of the slot's 72-byte record to `31E0:271E`. Calls `28C9:2A81` with 1 and gets two `0x40` by `0x40` buffers from `1BF3:27A8` and `1BF3:282D`, returning when either gives `0xFFFF`. Then for each point until the end mark 999 it sets bit 0 of the entry's byte `+0`, calls `31E0:2837` with the view, sets the position to the point times `n` (`31E0:4408`), takes the union of the old and new boxes clipped to the view (`1BF3:423F`, `4271`, `4258`, `428A`), sets bit `0x20`, copies that area from the screen (`2D40:3B30`), draws the slot into it with `31E0:328F` (with a flag from bit `0x80` of the entry's byte `+5`) and copies it back. When the slot leaves the view it calls `2C5F:01F3` with the target and 0, clamps the position into the view, calls `31E0:4129`, `2C5F:0139` and `28C9:2A81` and builds the point list again. It goes on while the position divided by `n` differs from the target divided by `n`, then calls `2C5F:0139` and `28C9:2A81` and frees both buffers with `2D40:3BD1`. Called from `+093A` and overlay 193 `+003C`. |
| `+093A` | `0x8F28A..0x8F2D6` | `5799:0025` | Takes two slots and `n`. Calls `+0308` with the first slot, the centre of the second slot's box (word `+3` plus half byte `+0x13`, word `+5` plus half byte `+0x14`) and `n`. Called from `+0000`. |
| `+0A4E` | `0x8F39E..0x8F3CA` | `5799:0034` | Takes `n`. Calls `2C5F:0271` `n` times with 1 when `DS:1440` is 1 and 0 otherwise. Called from `+0000` and `+0E1E`. |
| `+0A7A` | `0x8F3CA..0x8F43F` | `5799:005C` | Takes a handle, `x` and `y`. When `DS:2908` is `0xFFFF` it gets a `0x3F` by `0x3F` buffer from `1BF3:27A8` into it, returning on failure. Takes the area from `x`, `y` to `x + 0x40` and `y + 0x40`, limited to `0x13F` and `0xC7`, from the handle with `1BF3:282D`, copies it into `DS:2908` (`2D40:3B30`) and frees it (`2D40:3BD1`). Called from `+0B81` and `+1121`. |
| `+0AEF` | `0x8F43F..0x8F499` | `5799:006B` | Takes a handle, `x` and `y`. When `DS:2908` is not `0xFFFF`, takes the same area as `+0A7A` and copies `DS:2908` back into it, then frees the area. Called twice from `+0B81` and twice from `+1121`. |
| `+0B49` | `0x8F499..0x8F4D1` | `5799:0039` | When `DS:2904` is not 0, passes it to `444C:0092` and stores what that returns there. When `DS:2908` is not `0xFFFF`, frees that buffer and sets the word to `0xFFFF`. Called from overlay 173 `+059A`, overlay 182 `+1525` and `+1856`, and overlay 199 `+02A0`. |
| `+1073` | `0x8F9C3..0x8FA71` | `5799:0043` | Takes a number, `x`, `y` and a flag byte. Returns when the point is not strictly inside the view and the flag is 0. Otherwise it calls `31E0:0121` with the number, -1, `x`, `y`, 0, 0 and 3, and when that returns a slot above -1: calls `2C5F:018C` with it and 1 if the point is outside the view, calls `28C9:2A25`, calls `+0986` with the slot and 2, which returns the slot (FND-PARTY-141), calls `31E0:05BB` with the slot record's word `+1` and three 1s, then `28C9:2A81` with 1 and `28C9:2A44` with 0. Called from overlay 179 `+0D2F` and overlay 204 `+0169`. |
| `+1121` | `0x8FA71..0x8FB9F` | `5799:004D` | Takes `n` and returns when it is above 4. Loads `RESOURCE.GFF#BMP/19001` (`0x4A39`) into `DS:2904` through `38FF:04AB` when nothing is loaded there, returning when the request fails. With `w` and `h` half of what `1BF3:76C2` and `1BF3:771C` return for frame 4 of it: when the double word `5072:003C` is 0, `x` is `0x4E - w - 10` for `n` 0 and 2 and `0x81 - w - 10` for `n` 1 and 3, through the 4-word table at `+124F`, and `y` is `0x36 - h` for `n` 0 and 1 and `0x72 - h` for 2 and 3; for `n` 4 the `ja` skips the table and `x` and `y` are the values its caller left in `SI` and `DI`. When the double word is not 0, `x` is `0x25 - w - 10` and `y` is `0x30 * n + 0x1E - h`. It saves the area at `x`, `y` of handle 1 with `+0A7A`, draws frame 4 there with `2D40:3BEC`, calls `1BF3:5814` with -1, waits until the double word `DS:145E` has grown by `0x28`, restores the area with `+0AEF`, calls `1BF3:5814` again, restores once more and passes `DS:2904` to `444C:0092`, storing what it returns there. Called from overlay 179 `+0D2F` and `+2A05`. |
| `+13F3` | `0x8FD43..0x8FD8F`, `0x8FD92..0x8FDEA`, `0x8FDEC..0x8FE1B`, `0x8FE1E..0x8FE48`, `0x8FE4A..0x8FE6E` | `5799:0061` | Takes two points and returns a direction 0 to 7 from the first to the second, with `dx` and `dy` the differences. With `dx` 0 it returns 0 for `dy` below 0 and 4 otherwise; with `dy` 0, 2 for `dx` above 0 and 6 otherwise. Otherwise, with `r` the absolute value of `dx * 0x2000 / dy`: for `dx` above 0 and `dy` below 0 it returns 0 when `r` is above `0x4000`, 1 when above `0x1000`, else 2; for both above 0, 2 when `r` is below `0x1000`, 3 when below `0x4000`, else 4; for `dx` below 0 and `dy` above 0, 4, 5 or 6 by the first rule; for both below 0, 6, 7 or 0 by the second. Four `jmp` instructions, at `0x8FD8F`, `0x8FDEA`, `0x8FE1B` and `0x8FE48`, each follow another `jmp` and nothing reaches them, so the row leaves them out. Called from `+0308`. |

## Interpretation

`+0308` and `+093A` move a slot to a point or to another slot one step at a time and redraw the
changed area at each step, which fits a thrown or flying object. `+0240` sets the frame of a slot's
image for a direction, and `+13F3` is the direction for such a move. `+0A7A`, `+0AEF` and `+1121`
show a picture over a saved part of the screen for 40 ticks and put the screen back, at one of
four places or in a column, and `+0B49` frees what they keep. `+1073` places an object at a point
and shows it. The names stay `fn_206_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. What `31E0:271E`, `31E0:328F`, `1B0B:0212`, `1B0B:017E` and
overlay 179 `+09CE` compute is not read here. In `+13F3` the ratio `r` is `dx` over `dy`, so in
the four diagonal cases a steep line gives the direction of the horizontal axis and a flat one that
of the vertical axis, the reverse of the cases with `dx` or `dy` 0; the reading takes the code as
it is and does not say whether the directions are meant that way. Whether any caller of `+1121`
passes 4 is not checked.

## How to reproduce

Run `overlay_listing.py` for overlay 206 against the installed `DSUN.EXE` and `direct_callers.py`
for each offset in the table (`206+<offset>`), and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

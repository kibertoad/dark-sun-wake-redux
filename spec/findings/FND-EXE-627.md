---
id: FND-EXE-627
title: The 4 routines of overlay 212 that no entry placed close the character window, handle the three weapon frames' messages by naming the weapon or opening its description, draw the three weapon frames with the pictures of a missile weapon and two other weapons, and clear the frames' item and name words
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098AE3..0x00098B67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098C57..0x00098D40
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098D50..0x00098F5D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00098F5F..0x000990EF
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 4 rows of overlay 212 (header segment `57C9`, 8
trampolines, code from file offset `0x98200`, 3,823 bytes) that no spec location covered, 1,290
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x98200` plus it, and `DS` is `57E0`. Every routine has a trampoline in `57C9`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 212 is the View Character window: `+061C` opens `WIND/11500`
for the slot it stores in `4E71:0B44`, keeps the window at `55CA:0000` and `DS:1431`, and
registers `+0967` as its handler (FND-INPUT-012); for the frame identifiers `0x2BCF` to `0x2BD1`
it registers the trampoline `57C9:002F` through `3F96:02AB` and ORs `0x66` into their event masks
through `3F96:02F8` (FND-CONFIG-074). The three words at `DS:9D86` hold an item record number per
frame and the three at `DS:9D80` that item's name line, `0xFFFF` for none. An item list is the
10-byte list `2D40:2196` returns (FND-PARTY-218) and an item record is the 23-byte record under
the far pointer `DS:19C1`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+08E3` | `0x98AE3..0x98B67` | `57C9:002A` | Closes the window. Calls overlay 213 `+02D5` when the far pointer `55CB:0000` is set, stores the slot `4E71:0B44` in the byte `DS:4264` and 1 in the word `DS:4265 + 2 * slot`, calls `39D1:0430` with a null far pointer and sets `DS:2D14` to `0xF0`. When `55CA:0000` is set it closes that window through overlay 182 `+0128` (FND-EXE-605) with the flag 1, calls overlay 190 `+319E`, and clears `55CA:0000` and `DS:1431`. Called from `+0A15`, the handler `+0967`'s case for message 1, and overlay 213 `+06D2` and `+0B0D`. |
| `+0A57` | `0x98C57..0x98D40` | `57C9:002F` | Takes a word it does not read, a frame identifier and a message. Returns 0 unless the identifier less `0x2BCF` is a frame `f` from 0 to 2, and switches on the message through the 4-word table at `+0B40` (2, 4, `0x20`, `0x80`). For 2 it writes into control `0x2C06` of the window `DS:1431`, through overlay 190 `+06B4`, the line of `TEXT/1000` at the far pointer `DS:56F3 + 4 * DS:9D80[f]` (FND-PARTY-246), or `NO MISSILE WEAPON` (`DS:2E50`) for frame 0 and `NO WEAPON IN HAND` (`DS:2E3E`) for the others when that word is `0xFFFF`. For 4 it writes the empty string at `DS:2D49` there. For `0x20` and `0x80` it reads the frame's rectangle through `3F96:01A6`, calls `28C9:2ABD` and overlay 213 `+02D5` when `55CB:0000` is set, and when `DS:9D86[f]` is not `0xFFFF` calls overlay 213 `+0000` with it, the rectangle's first two words and 1; then `28C9:2A81` with 1. Returns 0. No direct call was found; `+061C` registers its trampoline for the three frames. |
| `+0B50` | `0x98D50..0x98F5D`, `0x98F5F..0x990CF` | `57C9:0039` | Calls `+0ECF` and `28C9:2ABD`. When overlay 190 `+34E7` with 8 returns frames, draws frame 2 of them at each of the three frames' rectangles (`3F96:01A6`) plus the window's origin words `+0x96` and `+0x98` with `2D40:3BEC`, and frees them through `444C:0092`. Fills an item list for `4E71:0B44` and takes three entries through overlay 173 `+2E1B` (FND-EXE-600): the first with -2 and 2, the second with -2 and 1, and the third, when the second was found, with the second's index and 1. For each entry found, with `r` its record number: takes the record's picture number from word `+0x0C`, filling it from overlay 190 `+2F2F` of `r` when it is 0; chooses frame 0 for the first entry and, for the others, frame 2 when record byte `+0x13` is 10 and frame 1 otherwise; stores `r` in `DS:9D86` and record word `+0x14` in `DS:9D80` at that frame; loads `BMP` of the picture number through `38FF:04AB` and draws it in the frame's 16 by 16 cell one pixel in from its corner with `2D40:3BEC`, centred when both sizes from `1BF3:76C2` and `1BF3:771C` are at most 16 and otherwise through a handle from `1BF3:282D` freed with `2D40:3BD1`, as overlay 175 `+0758` does (FND-EXE-598); and frees the picture. Ends with `28C9:2A81` with 0. Called from `+0566` in the sheet draw `+0403`. The `jmp` at `+0D5D` follows a `jmp` and nothing reaches it, so the row leaves its two bytes out. |
| `+0ECF` | `0x990CF..0x990EF` | `57C9:0034` | Stores `0xFFFF` in the three words at `DS:9D86` and the three at `DS:9D80`. Called from `+05CD` in `+0403`, on the path for a character whose 66-byte details word `+0x0E` is 0, and `+0B63` (in `+0B50`). |

## Interpretation

The View Character window shows three weapon frames, `0x2BCF` to `0x2BD1`. `+0B50` fills them
from the character's items: frame 0 with the item overlay 173 `+2E1B` finds with number 2, which
the text of `+0A57` names the missile weapon, and frames 1 and 2 with the next two items it finds
with number 1, by position byte 10 or another. With the pointer on a frame (message 2) the
weapon's name, or a "no weapon" line, goes in the window's text line, message 4 clears it, and a
click (`0x20` or `0x80`) opens the item description window of overlay 213 for the frame's item.
`+0ECF` empties the frames, and `+08E3` closes the window. The names stay `fn_212_<offset>` until a
reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0A57` has no direct call and is reached through the trampoline
`+061C` registers, and may also be reached by other computed far calls. Overlay 213 `+0000` and
`+02D5`, overlay 190 `+06B4`, `+2F2F`, `+319E` and `+34E7`, `39D1:0430`, `3F96:01A6` and
`28C9:2ABD` are not read here; the reading of them as the item description window's open and
close, a control text writer, a picture number lookup, a redraw, a frame image load, an event
helper, a frame rectangle reader and a screen lock rests on how their arguments are used and on
FND-EXE-598 and FND-EXE-620. The message numbers' meanings (pointer over, pointer off, button
press) are taken from the texts the cases write.

## How to reproduce

Run `overlay_listing.py` for overlay 212 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`212+<offset>`), read the table at `+0B40` and
`+061C..+08E2`, and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

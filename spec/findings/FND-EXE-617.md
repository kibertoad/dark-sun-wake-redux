---
id: FND-EXE-617
title: The 3 routines of overlay 181 that no entry placed close the Game Menu window, show the name of the Game Menu control under the pointer, and clear that name once
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000684C2..0x0006873C
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 3 rows of overlay 181 (header segment `56B9`, 5
trampolines, code from file offset `0x67EB0`, 2,188 bytes) that no spec location covered, 634
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x67EB0` plus it, and `DS` is `57E0`. Every routine has a trampoline in `56B9`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 181 is the Game Menu, `WIND/10500`, which `+0480` opens and
whose handler holds the button table at `+0448` (FND-INPUT-012, FND-UI-053). Its window is held
at the far pointer `4E46:0000` and at `DS:1431`. Control numbers are given in hex as the code
pushes them: `0x27D9` is 10201, `0x2843` is 10307 and `0x2C06` is 11270.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0612` | `0x684C2..0x684F8` | `56B9:002A` | When the far pointer `4E46:0000` is not null, calls overlay 182 `+0128` with it and 1 (FND-EXE-605) and sets the double words `DS:1431` and `4E46:0000` to 0. Called from 14 near calls in the Game Menu's handler (`+012C`, `+0139`, `+01D8`, `+01E5`, `+027E`, `+028B`, `+02A8`, `+02C2`, `+0304`, `+033F`, `+039E`, `+03E2`, `+03FB` and `+043F`) and from the resident key routines at `28C9:11AD` (in `28C9:0CFF`) and `28C9:1459` (in `28C9:1261`). |
| `+0648` | `0x684F8..0x686F5` | `56B9:002F` | A frame callback: its second word is a control, its third an event kind, and its seventh and eighth a point. For kind 2 on control `0x27D9` it calls `+0845` with the window and `0x2843` and then `2C5F:0C8D` with 0. For kind 4 it reads each control's rectangle with `3F96:01A6` and tests the point, less the window's position words `+0x96` and `+0x98`, with overlay 190 `+077A`. When the point is in `0x27D9` it takes the first of `0x27DA` to `0x27E3` that holds it, and when none does the last of `0x27E5` to `0x27EB` that holds it. With a control found it writes, through overlay 190 `+06B4` into control `0x2C06`, the string under the far pointer at `DS:0E3A + 4 * (c - 0x27D9)` for the first group (`EXIT TO DOS`, `LOAD/SAVE GAME`, `SET PREFERENCES`, `MOVE CURSOR`, `LOOK CURSOR`, `ATTACK CURSOR`, `COLLAPSE PARTY`, an empty string, `OVERHEAD MAP` and `CENTER ON LEADER` for `0x27DA` to `0x27E3`) or at `DS:2D18 + 4 * (c - 0x27E5)` for the second (`GAME MENU`, `RETURN TO GAME`, `VIEW CHARACTER`, `VIEW INVENTORY`, `CAST SPELL/USE PSIONIC`, `CURRENT SPELL EFFECTS` and `MEMORIZE SPELLS`); with none, or with the point outside `0x27D9`, it calls `+0845` with the window and `0x2843`. Other kinds do nothing. Returns 0. No direct call was found. |
| `+0845` | `0x686F5..0x6873C` | `56B9:0025` | Takes a far window and a control `c`, and returns at once when the byte `DS:4270` is 1. For `c` of `0x2C06` it writes the empty string at `DS:0E71` into it through overlay 190 `+06B4`; for any other it passes `c` and that string to `3EBE:07FF`, then calls `4328:0B4C`. Sets `DS:4270` to 1. Called from `+056D`, `+0683` and `+07F3` and from overlays 189 `+0823`, 190 `+0414` and 211 `+1808`. |

## Interpretation

`+0612` closes the Game Menu before a button's action runs and when a key routine takes over.
`+0648` is the hover handler of the menu's frame `APFM/10201`: moving the pointer over a button
shows that button's name in the label control 11270, and leaving the buttons or clicking clears
the label through `+0845`. `+0845` clears a label once and then does nothing until `DS:4270` is
set back to 0, which this overlay's rows never do. The names stay `fn_181_<offset>` until a
reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0648` is reached only through its trampoline, which the code
that opens the window may register as the frame's callback, or through a computed far call.
Overlay 190 `+077A` and `+06B4`, `3EBE:07FF`, `4328:0B4C` and `2C5F:0C8D` are not read here; the
reading of them as a point-in-rectangle test, a text writer, a label setter and a screen update
rests on their arguments and on FND-EXE-604. Which code sets `DS:4270` to 0 is not read. The second
group's labels name the buttons of the bar beside the view, which `0x27E5` to `0x27EB` may be
frames over; the controls themselves are not read from `WIND/10500` here.

## How to reproduce

Run `overlay_listing.py` for overlay 181 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`181+<offset>`), read the far pointers at
`DS:0E3A` and `DS:2D18` and their strings, and compare the ranges with the rows of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

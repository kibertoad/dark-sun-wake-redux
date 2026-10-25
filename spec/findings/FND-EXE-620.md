---
id: FND-EXE-620
title: The 7 routines of overlay 172 that no entry placed open the three-item choice's window, handle the message window's events, open, redraw and close the last-cast window, end the last-cast window's draw, and show a one-line message window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059450..0x00059557
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059C46..0x00059CA1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059CA3..0x0005A051
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A3FF..0x0005A6FE
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 172 (header segment `566A`, 14
trampolines, code from file offset `0x59430`, 4,814 bytes) that no spec location covered, 2,063
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x59430` plus it, and `DS` is `57E0`. Every routine has a trampoline in `566A`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. The event state is the word `28C9:2522` returns (FND-CONFIG-117).
`3EBE:071F` takes a window, a control number as a double word and a state, and `3EBE:07FF` a
window, a control number and a far text (FND-EXE-603). The last-cast table is the 17-word rows at
`DS:445E`, one per party member, whose first six words hold a spell or power number plus 1, or 0
(FND-PARTY-243). `DS:445C` holds the member the last-cast window shows, and the far pointer
`4C4D:000B` and `DS:1431` hold its window. Overlay 190 `+0000` draws frame `k` of a far image at a
column and a row (FND-PARTY-154); `4E71:002F` holds the frames overlay 190 `+34E7` loads with 8.
For each of the six frames `0x2BCD` to `0x2BD2` of the last-cast window, `3F96:01A6` gives the
frame's position, to which these routines add the window's words `+0x96` and `+0x98`.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0020` | `0x59450..0x59557` | `566A:0057` | Opens the window of the three-item choice and returns its number. Starts from window 14000 (`0x36B0`) with item controls from `0x36B1`. Unless the event state is 2 it stores 1 in the byte `4C4D:0004` and calls overlay 182 `+1856`. When the double word `1000:1959` returns is below `0x840` it goes to the end; when it is below `0x2D20` it uses window 14001 (`0x36B1`) with item controls from `0x36B5`. It loads the window through overlay 182 `+00C4` (FND-UI-044) into the far pointer `4C4D:0000`, and calls overlay 182 `+00F6` with it and the corner `DS:40D6`, `DS:40D4`. When both succeed it sets control `0x2844` to state 0 and, for the three item controls, writes a null text and sets state 1. When the load fails or `+00F6` returns nonzero it passes `Failed to open window` (`DS:0496`) to overlay 180 `+0867`. Called from the choice `+018F` (FND-COMBAT-051, FND-UI-053) at `+01D8`, which then uses `0x36B5` as its first item control when this returns 14001. |
| `+0816` | `0x59C46..0x59CA1`, `0x59CA3..0x59CA7` | `566A:004D` | The handler of the message window. Takes an event kind and, at `[bp+0x12]`, a word. For kind 1 it calls `+0746` and returns -1. For kind 2 it stores 0 in `DS:40CD`, calls `+0746` and returns -1. For kind 6 with the word below 9 it stores 1 in the byte `DS:40CC`; with the word 9 or more it stores 0 there, and then decrements `DS:40CD` when the word with bit `0x800` cleared is below 9, and otherwise stores 0 in `DS:40CD`, calls `+0746` and returns -1. For any other kind it calls `+0746` and returns -1 when `DS:40CD` is 0, and decrements `DS:40CD` when `DS:40CC` is 0. Returns 0 in the other cases. The bytes at `+0871` are a jump no path reaches. Called from `+05DB` at `+0628` (FND-PARTY-139); `+05DB` at `+0696` and `+125E` at `+1267` pass `566A:004D` as the window's handler. |
| `+0877` | `0x59CA7..0x5A051` | `566A:0034` | Opens the last-cast window for member `m`. Stores 0 in the byte `DS:9D76` and `m` in `DS:445C`. When byte `+0x14` of `m`'s 49-byte record at the far pointer `DS:19C9` is not 1, it calls overlay 211 `+02B5`, the spell window (FND-INPUT-012), with `m`'s 66-byte record at `DS:19C5`, the 49-byte record and `m`, and passes `NOT AVAILABLE FOR CASTING` (`DS:04AD`) to `+05DB`; when `m`'s first table word is 0 it does the same with `No Spells in Queue` (`DS:04C7`). Otherwise it calls `28C9:2ABD`, stores 1 in the byte `4C4D:0005` and calls overlay 182 `+1856` unless the event state is 2, opens window 14002 (`0x36B2`) through overlay 182 `+016B` with a zero corner and the handler `566A:0043` (`+0C21`), stores it at `4C4D:000B` and `DS:1431`, and sets its `+0xF9` to `566A:003E` (`+1085`) through `3A8E:0D0C` and its `+0xFD` to `566A:0039` (`+1252`) through `3A8E:0D3C` (FND-EXE-587). For controls `0x2BCD` to `0x2BD3` it registers `566A:0043` through `3F96:02AB` and ORs `0x1C6` into the event mask through `3F96:02F8` (FND-CONFIG-074). It loads `4E71:002F` and, for each of the six frames, draws frame 2 of it there and then, when the table word `w` is not 0, with `n = w - 1`: for `n` below 235 tests overlay 197 `+300D` with `m` and `n` and then overlay 197 `+1A94` with 4, `m` and 0 (RULE-MAGIC-011), counting the spell available when both return nonzero; for `n` from 235 to 268 takes overlay 176 `+005B` with `m` and `n - 235` as the test; for both it requests `ICON` 21000 plus `n` through `38FF:04AB`, and for larger `n` `ICON` 11103 (`0x2B5F`), keeping the previous test. It draws the icon with frame 0 one pixel right of and below the frame's position and frees it through `444C:0092`, and when the test is 0 draws frame 6 of `4E71:002F` over it. It then calls `28C9:2A81` with 1, overlay 182 `+059C` with `0x4A9D` and 1, and `28C9:2A44` with 1, and while `4C4D:000B` is set builds events with `39D1:097D` and passes each to `39D1:071F`. Called from one resident far call at `28C9:00D5`, which passes the word `DS:426D` (FND-CONFIG-072). |
| `+0FCF` | `0x5A3FF..0x5A4B5` | `566A:0048` | Closes the last-cast window. Calls overlay 213 `+02D5` when the far pointer `55CB:0000` is set, and returns when `4C4D:000B` is 0. Frees `4E71:002F` through `444C:0092`, storing what it returns, removes the window through `3A8E:0A26` and clears `4C4D:000B` and `DS:1431`. When the byte `4C4D:0005` is 1 it calls overlay 190 `+319E` if the event state is 2, then `3D72:0D83`, overlay 182 `+19F8` and `28C9:2A81` with 1, and stores 0 in that byte. Called from the window's handler `+0C21` (FND-CONFIG-102) at `+0C39`, `+0DB9`, `+0E27`, `+0F69` and `+0FAD`. |
| `+1085` | `0x5A4B5..0x5A682` | `566A:003E` | Draws the six frames of the last-cast window as `+0877` does: frame 2 of `4E71:002F` at each frame, and for each nonzero table word of member `DS:445C` the same tests, the same `ICON` and frame 6 of `4E71:002F` when the test is 0. Returns 0. No direct call was found; `+0877` stores it at the window's `+0xF9`, which `3A8E:060D` calls at `3A8E:08B7` (FND-EXE-587). |
| `+1252` | `0x5A682..0x5A68E` | `566A:0039` | Calls overlay 190 `+319E` with no arguments and returns 0. No direct call was found; `+0877` stores it at the window's `+0xFD`, which `3A8E:060D` calls at `3A8E:0707` (FND-EXE-587). |
| `+125E` | `0x5A68E..0x5A6FE` | `566A:0020` | Takes a far text. Opens window 10501 (`0x2905`) at (`0x42`, `0x5A`) through overlay 182 `+016B` with the handler `566A:004D` (`+0816`), calls overlay 190 `+3456` with `0x0E`, `0x42`, `0x5A` and 0, writes the empty string at `DS:04AC` and then the text into control `0x2C06` through overlay 190 `+06B4`, calls `28C9:2A81` with 1 and removes the window through `3A8E:0A26`. Called from overlay 192 `+04B7` and `+061A`. |

## Interpretation

`+0020` is the part of the choice of FND-UI-053 that loads its window: the smaller window 14001 is
taken when memory is short, and the choice adjusts its control numbers to match. `+0816` is the
message window's handler that `+05DB` loops on: a key or button with a small code holds the
message, its release or any other event counts the delay word `DS:40CD` down, and a close event
or a larger code ends the message. `+0877`, `+1085` and `+0FCF` open, redraw and close the
last-cast window, a row of six icons for the spells and powers the member cast last, greyed with
frame 6 when the member cannot cast them now; the frame callback `+0C21` casts the one chosen.
`+125E` shows one line of text in the message window and takes the window down again at once,
leaving the drawn text on the screen for overlay 192's save and load paths. The names stay
`fn_172_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+1085` and `+1252` in particular may also be reached through
their trampolines by a computed far call. `+0746`, `1000:1959`, overlay 182 `+00F6`, `+1856`,
`+059C` and `+19F8`, overlay 190 `+3456`, `+319E` and `+06B4`, overlay 213 `+02D5`, `28C9:2ABD`,
`3D72:0D83` and `38FF:04AB` are not read here; the reading of them as closing the message window,
measuring free memory, placing a window, drawing and requesting a resource rests on their
arguments and on the entries cited. The word at `[bp+0x12]` of `+0816` is read as a key or button
code from the kind-6 tests alone. For a table word above 269, `+0877` and `+1085` keep the test of
the previous frame, which may be unintended.

## How to reproduce

Run `overlay_listing.py` for overlay 172 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`172+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Read `+018F..+0220` for the use of `+0020`'s
result, `+05DB..+0696` for the message loop, and `resident_listing.py` over `28C9:00D1..28C9:00DB`
for the resident call of `+0877`.

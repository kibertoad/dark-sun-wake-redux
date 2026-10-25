---
id: FND-EXE-621
title: The 6 routines of overlay 192 that no entry placed read one save file's description into a list row, list the SAVE??.SAV files into the ten rows, end the Save and Load window's draw, save a game, and write a row's text into its button or the name box
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D32A..0x0007D54B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D6FB..0x0007D8ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E058..0x0007E160
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 6 rows of overlay 192 (header segment `5736`, 7
trampolines, code from file offset `0x7D300`, 3,680 bytes) that no spec location covered, 1,307
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x7D300` plus it, and `DS` is `57E0`. Callers are the hits of `direct_callers.py`,
checked against the instructions around them, named by the row that holds them. The overlay is
the Save and Load window of FND-UI-036: `+0000` and `+0015` set the mode byte `50AF:0004` to 1
(Load) or 0 (Save) and call `+024B`, which opens `WIND` 18500 into the far pointer `50AF:0000`,
allocates ten 125-byte records at the far pointer `DS:6278` and fills them. A record holds a save
file's path from byte 0 and its description from byte `+0x50` (FND-SAVE-007). The word
`4C4C:0002` is the record shown in the first row and `4C4C:0000` the selected row (FND-SAVE-004).
Row `i` is button 18304 + `i` (`0x4780 + i`) and the name box is `EBOX` 18400 (`0x47E0`).
`3EBE:071F` takes a window, a control number as a double word and a state, and `3EBE:07FF` a
window, a control number and a far text (FND-EXE-603).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+002A` | `0x7D32A..0x7D3F2` | none | Takes a record number `r`. Opens the path at record `r` through `38FF:0066` with `0x4D4`, 2 and a far handle on the stack; when that succeeds it reads `STXT` 1 into the record's `+0x50` through `38FF:04AB` and closes the handle through `38FF:02B5`. When the open or the read returns -1 it stores 0 in the record's byte 0, and in Load mode 0 in its byte `+0x50`; in Save mode it copies the empty string at `DS:2084` there through `1000:406D` (FND-SAVE-007). Called from `+00F2` at `+0211`. |
| `+00F2` | `0x7D3F2..0x7D52B` | none | Takes a row count `n`. Copies the game's directory at `DS:44F2` to a stack buffer and appends `SAVE??.SAV` (`DS:2085`) through `2D40:3DC2` with at most 80 characters. Empties records 0 to `n - 1` as `+002A` does for a failed open. Calls `44DE:0340` with the pattern, a 44-byte stack buffer and 0, and returns when that is nonzero. For each file found, it cuts the pattern after its last `\` (`1000:381E` with `0x5C`), copies the found name from byte `+0x1E` of the buffer through `1000:37B6`, ends it after six characters, and takes `1000:2E86` of the two characters after `SAVE` less 1 as `r`. When `r` is 0 or more it copies the directory and appends the name into record `r` (at most 80 characters) and calls `+002A` with `r`. It goes on while `44DE:03AC` with the buffer returns 0. There is no test of `r` against `n`. Called from `+024B` at `+035E` with 10. |
| `+022B` | `0x7D52B..0x7D54B` | `5736:003E` | Takes a far window pointer and calls overlay 190 `+3456` with `0x11`, the window's words `+0x96` and `+0x98`, and 0, and returns 0, the form of overlay 175 `+0000` (FND-EXE-598). No direct call was found; `+024B` stores it at the window's `+0xFD` through `3A8E:0D3C` at `+02E7`, which `3A8E:060D` calls at `3A8E:0707` (FND-EXE-587). |
| `+03FB` | `0x7D6FB..0x7D8ED` | none | The Save Game routine of FND-SAVE-004. Takes the selected row `s`. Copies the nine `PREF` bytes and the ten `GREQ` bytes of FND-SAVE-004 into stack buffers, forms `SAVE%.2d.SAV` (`DS:20B6`) with `4C4C:0002 + s + 1` through `3150:000E`, writes the directory and that name into record `4C4C:0002 + s`, shows `SAVING GAME` (`DS:20C3`) through overlay 172 `+125E` (FND-EXE-620) and calls overlay 182 `+059C` with `0x4AA6`. Calls overlay 187 `+0DCD` through `56EF:0084` with the record and its `+0x50` (FND-SAVE-008), and when that returns 0 in `AL` passes `Failed to save the game!` (`DS:20CF`) to overlay 186 `+0572` and returns. Otherwise, when `37ED:0064` returns 0, it removes and writes `PREF` 100 and `GREQ` `4C4C:0002 + s + 1` through `37FC:05C5` and `37FC:00E8`, calls overlay 186 `+05AD`, and passes `GAME SAVED` (`DS:20E8`) to overlay 172 `+05DB`. Called from the window's handler `+0785` (FND-UI-037) at `+0BEF` with `4C4C:0000`. |
| `+0D58` | `0x7E058..0x7E0F4` | `5736:0034` | Takes a row `i`. Calls `28C9:2ABD`, writes the description of record `4C4C:0002 + i` into button 18304 + `i`, and in Load mode sets that button's state to 1 when the record's byte 0 is 0 and to 0 otherwise. Ends with `28C9:2A81` with 0. Called from `+024B` at `+0367` for rows 0 to 9, and from `+0785` at `+0B94` with `4C4C:0000`. |
| `+0DF4` | `0x7E0F4..0x7E160` | `5736:0039` | Takes a row `i`. Calls `28C9:2ABD`, sets button 18304 + `i` to state 2, and writes the description of record `4C4C:0002 + i` into the name box through `4228:002B`. Ends with `28C9:2A81` with 0. Called from `+024B` at `+037D` with `4C4C:0000`, and from `+0785` at `+08D3`, `+0A2E` and `+0ACB`. |

## Interpretation

`+00F2` and `+002A` fill the list of FND-UI-036: each `SAVEnn.SAV` in the game's directory goes to
record `nn - 1` with the description it stores as `STXT` 1, and a record with no file is empty, so
Load mode shows it disabled through `+0D58` and Save mode shows it blank. `+0DF4` marks the
selected row and copies its description into the name box, where the player edits it before a
save. `+03FB` is the save that FND-SAVE-004 and FND-SAVE-008 describe. Since `+00F2` does not
bound `r`, a file named `SAVE11.SAV` or above in the game's directory would be written past the
ten records. The names stay `fn_192_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+022B` in particular may also be reached through its trampoline
by a computed far call. `38FF:0066`, `44DE:0340`, `44DE:03AC`, `4228:002B`, `37ED:0064`, overlay
186 `+0572` and `+05AD` and overlay 190 `+3456` are not read here; the reading of them as opening a
resource file, finding the first and next file of a pattern, writing an edit box, testing the
resource handle and drawing the window rests on their arguments and on the entries cited. The
stack buffer of `+00F2` is read as a DOS find record from the offset of the name alone, and `1000:2E86` as a decimal parse
of `nn` from its use alone.

## How to reproduce

Run `overlay_listing.py` for overlay 192 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`192+<offset>`), and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`. Read `+024B..+03AB` for the arguments `+024B`
passes and `+0B88..+0BF5` for the calls of the window's handler.

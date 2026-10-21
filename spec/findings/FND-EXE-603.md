---
id: FND-EXE-603
title: The 13 routines of overlay 199 that no entry placed keep a text and choice dialog for overlay 188 in three windows at 52A1:0000, 0004 and 0008, take the MAP, BOOK, CLOSE, DEBUG and END lines, scroll the text and the list of up to ten choices, wait for a choice, and load and draw PORT pictures
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087900..0x00087B6C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00087BA0..0x000884B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088675..0x00088CF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00088D52..0x00088F11
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

After the restage of FND-EXE-601, `npm exec -- standard-coverage` reported 13 rows of overlay 199
(header segment `576C`, 15 trampolines, code from file offset `0x87900`, 5,649 bytes) that no
spec location covered, 5,050 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x87900` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `576C`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them, named by the row that holds them; a routine that overlay 199 hands
on as a far pointer (`push 0x638` and a trampoline offset) is named with the setter it goes to.
`39D1:0418` stores a window hook, and `3A8E:0CDC`, `0D0C` and `0D3C` store a window record's
callbacks at `+F5`, `+F9` and `+FD` (FND-EXE-587). The three window pointers are the double words
`52A1:0000`, `52A1:0004` and `52A1:0008`, called W0, W4 and W8 here; overlay 182 `+016B` opens a
window and returns its pointer, and overlay 182 `+0128` closes one. The text buffer is the far
pointer `DS:6554` (0 when none), the choice list the 51-byte entries at `DS:6356` counted by the
byte `DS:6322`, with the byte `DS:6321` as the first one shown, the word `DS:25BD` the current
picture number and the far pointer `DS:6558` the loaded picture. A handler takes a 24-byte event
by value: word `+0` its kind (1, 2 or 6), word `+2` a control number for kind 2, and word `+0x0C`
a key code for kind 6.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x87900..0x87920` | `576C:0061` | Calls overlay 190 `+3456` with `0x0F`, the words at `+0x96` and `+0x98` of the record its far pointer argument names, and 0, and returns 0, the form of overlay 178 `+0000` (FND-EXE-599). Passed to `3A8E:0D3C` for W4, W8 and W0 in `+02A0`. |
| `+0020` | `0x87920..0x87953` | `576C:005C` | Calls overlay 186 `+0000` with 12001, 0, 0 and 0, and when `DS:25BD` is not 0 with `DS:25BD`, 3, 160 and 100; returns 0. Passed to `3A8E:0D3C` for W4 when `+02A0` opens W4 for a MAP or BOOK line. |
| `+0053` | `0x87953..0x87B6C` | `576C:003E` | The handler of W4. Kind 1: calls `+0BC1`, clears `DS:1462` and returns -1. Kind 2 through the 4-key table at `+026C`: `0x822` and `0x300C` call `+0BC1` when W0 is 0; `0x82D` and `0x82E` scroll W4's `EBOX` control (found by `39D1:049C` with the tag `0x584F4245` and number `0x3070`) by 5 lines one way or the other through `409B:17D9`. Kind 6 through the 9-key table at `+027C`: `0x011B`, `0x1C0D` and `0x3920` (Escape, Enter, Space) call `+0BC1` when W0 is 0, `0x4800` and `0x5000` scroll by 1, `0x4900` and `0x5100` by 5, and `0x4700` and `0x4F00` by amounts from `409B:181E`. After a scroll it sets the states of controls `0x82D` and `0x82E` through `3EBE:071F` from the list's position and count and calls `28C9:2A81` with 1. Ends with `2C5F:0182`. Called from `+0D75` twice; passed to `182+016B` and `39D1:0418` in `+02A0`. |
| `+02A0` | `0x87BA0..0x884B7` | `576C:0025` | Takes an operation 0 to 4, a far string and a word; calls `2C5F:0182`, overlay 206 `+0B49` (FND-EXE-602), overlay 204 `+1D2E` with 5 when `DS:14E5` is set, and overlay 182 `+1FDB` with 1, then switches through the 5-word table at `+0BB7`. Operation 0 adds a line: the first after an operation 3 is copied to `DS:6323` (50 bytes, skipped when equal) and the others, cut to 49 characters, to entry `DS:6322` of the choice list, at most 10. Operation 1 draws the picture that the word argument names with `+1567`. Operations 2 and 4 take the string as a command: with operation 2, `MAP` followed by digits sets `DS:25BC` to 1 and `DS:25BD` to the number, `BOOK` sets `DS:25BC` to 2, `CLOSE` calls `+0C21`, a line starting with `DEBUG` is dropped, and any other line except `END` goes to `2D40:3DC2` with the text buffer and `0x400`, after `444C:0008` allocates the buffer (with an out-of-memory message through overlay 180 `+0867` when it fails). `END`, and every operation 4, closes W0 when the choice list is empty, opens W4 when it is not open and the text is not empty (resource 12500 with `+0053`, `+0000` and `+1601` as callbacks, or 12503 with `+0053` and `+0020` when `DS:25BC` is set, after setting `DS:25BD` to the caller's `DI` for 1 and to 0 for 2), puts the text into W4's `EBOX` with `4228:002B`, sets the scroll controls and calls `3A8E:060D` for W4; operation 4 also opens W8 (resource 12502 at 0, 140, with `+0D75`, `+0000` and `+1601`) and clears its control `0x3071`. Operation 3 opens W4 in the same way when needed and then W0 (resource 12501 at 0, 140, with `+102C` as handler and `+0E51` and `+0000` as callbacks), resets `DS:25BA` to `0xFF` and `DS:6321` to 0, sets controls `0x830` and `0x82F` and calls `3A8E:060D` for W0. It sets `DS:143D` and `DS:655C` to 1 when a window opens, and at the end calls `28C9:2A81` with 1 when either is set and overlay 182 `+059C` with `0x4A9D` and 1. Called 15 times from overlay 188 (`+13E0`, `+157D`, `+158D`, `+159B`, `+15B8`, `+15C8`, `+15D8`, `+15E6`, `+16FA`, `+1722`, `+173B`, `+175A`, `+178D`, `+17A2`, `+1937`). |
| `+0D75` | `0x88675..0x886CE`, `0x886D0..0x886ED`, `0x886EF..0x88751` | `576C:004D` | The handler of W8. Kind 6 with Escape, Enter or Space and kind 2 with control `0x3071` set `DS:143E` to 1. Kind 2 with control 0 does nothing; any other kind 2 or kind 6 event calls overlay 182 `+1FDB` with 1, is passed on to `+0053` when W4 is open (except control `0x300C`), and then W8's control `0x3071` is refreshed through `4228:0009` and `3A8E:060D` and `28C9:2A81` called with 1. Returns 0. The `jmp` instructions at `0x886CE` and `0x886ED` each follow another `jmp` and nothing reaches them, so the row leaves them out. Passed to `182+016B`, `3A8E:0CDC` and `39D1:0418` in `+02A0`. |
| `+0E51` | `0x88751..0x8875D` | `576C:0057` | Calls `+0E5D` and returns 0. Passed to `3A8E:0D0C` for W0 in `+02A0`. |
| `+0E5D` | `0x8875D..0x88868` | `576C:0043` | Draws W0: writes `DS:6323` into its control through `401B:0172` with the format `%C%C%C%s` after passing it to `1000:4235`, calls overlay 182 `+1FDB` with 1, and for the ten controls `0x81C` to `0x825` writes choice entry `DS:6321 + i` with `3EBE:07FF` and state 0 with `3EBE:071F`, or an empty string and state 1 past the last entry; then calls `28C9:2A81` with 0. Called from `+0E51` and `+102C`. |
| `+0F68` | `0x88868..0x8892C` | `576C:0034` | Waits for a choice: while `DS:25BA` is `0xFF` it builds an event with `39D1:097D` and passes it to `39D1:071F`, calling `4611:0051` when `DS:14E3` and `DS:14E4` are set. Then sets the chosen control's state to 4 and 5 three times with `28C9:2A81` after each, calls `+0BC1`, clears `DS:6322`, sets `DS:25BA` back to `0xFF` and returns `DS:25BA + DS:6321 + 1` in `AL`. Called from overlay 188 `+15A3`, `+15EE` and `+193F`. |
| `+102C` | `0x8892C..0x88976`, `0x88979..0x88CF4` | `576C:0052` | The handler of W0. Kind 1 clears `DS:1462` and returns -1. Kind 6 through the 13-key table at `+141E`: `1` to `5` choose shown entry 0 to 4 when it exists (state 4 through `3EBE:071F`, then `DS:25BA`), `Y` and `y` choose entry 0 and `N` and `n` entry 1 when `DS:6323` is `answer yes or no` by `1000:4096`; `0x4800` and `0x5000` page the choices by 5 through the word `DS:044C` when there are more than 5, or else scroll W4's text by 1; `0x4900` and `0x5100` scroll the text by 5. Kind 2 through the 21-word table at `+13F4` for controls `0x81C` to `0x830`: `0x81C` to `0x820` choose entry 0 to 4, `0x82D` and `0x82E` scroll the text by 5, `0x82F` and `0x830` page the choices, and the rest do nothing. A page sets the states of `0x830` and `0x82F` and calls `+0E5D`. Ends with `2C5F:0182`, returning 0, or -1 after a key choice. The `jmp` at `0x88976` follows the table `jmp` and is no target of the table, so the row leaves it out. Passed to `182+016B` and `39D1:0418` in `+02A0`. |
| `+1452` | `0x88D52..0x88DA2` | `576C:002F` | While `DS:143D` is 1 it builds an event with `39D1:097D` and passes it to `39D1:071F`, calling `4611:0051` when `DS:13F7`, `DS:14E3` and `DS:14E4` are set. Called from overlay 188 `+0AA1` and `+1762`. |
| `+14A2` | `0x88DA2..0x88E67` | `576C:0066` | Takes `n`. Passes `DS:6558` to `444C:0092` and stores what that returns there. For `n` 0, when W4 is open or the double word `5072:003C` is not 0 it loads `RESOURCE.GFF#BMP/12000` into `DS:6558` through `38FF:04AB`, and otherwise sets `DS:25BD` to 0 and returns; for other `n` it loads `PORT` `n`. When a load returns -1 with a pointer set it releases it again. When `DS:6558` is then set, `DS:25BD` becomes `n`. Called from `+1567`. |
| `+1567` | `0x88E67..0x88F01` | `576C:0020` | Takes `n`. Calls `+14A2` with `n` unless `DS:6558` is set and `DS:25BD` is `n`. When W4 is open or `5072:003C` is not 0, and `DS:6558` is set, calls `28C9:2ABD` and draws `DS:6558` on handle 1 with `2D40:3BEC`: frame 1 at 0, 0 for `n` 0, else frame 0 at 0, 0, or at `0xEE`, 6 when `5072:003C` is not 0; then calls `28C9:2A81` with 0. Called from `+02A0` (three times), `+1601` and overlay 207 `+080D`. |
| `+1601` | `0x88F01..0x88F11` | `576C:0048` | Calls `+1567` with `DS:25BD` and returns 0. Passed to `3A8E:0D0C` for W4 and W8 in `+02A0`. |

## Interpretation

Overlay 199 is the dialog that overlay 188 drives: operation 0 lines and `END` or operation 4
show a scrolling text with an optional portrait (`PORT` `n`, or a map or book picture), operation
3 adds a list of up to ten numbered choices with a question line, `+0F68` waits until one is taken
by key or mouse and returns its number from 1, and `+1452` waits until the text is dismissed.
Operation 4 adds an input field in W8. `+0BC1` and `+0C21`, which other entries place, close the
dialog. The names stay `fn_199_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. What overlay 186 `+0000`, overlay 190 `+3456`, `2D40:3DC2`,
`1000:4235`, `401B:0172` and the states of `3EBE:071F` do is not read here; the text buffer may
be appended to rather than replaced, depending on `2D40:3DC2`. The meaning of `5072:003C`, which
moves the picture and is also tested by overlay 206 `+1121`, is not settled.

## How to reproduce

Run `overlay_listing.py` for overlay 199 against the installed `DSUN.EXE` (`0x87900` to
`0x88CF4` and `0x88D52` to `0x88F11`) and `direct_callers.py` for each offset in the table
(`199+<offset>`), read the key and jump tables at `+026C`, `+027C`, `+0BB7`, `+13F4` and `+141E`,
and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

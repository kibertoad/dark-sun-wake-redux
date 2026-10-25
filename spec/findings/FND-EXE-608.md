---
id: FND-EXE-608
title: The 7 routines of overlay 211 that no entry placed redraw the spell window's four party boxes and current name, close the spell window, handle the events of its 34 list controls, and draw a frame over one of those controls through overlay 190 or 2D40:3BEC
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00095E10..0x000960C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009654D..0x0009662B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0009756E..0x0009772F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097743..0x0009785F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00097D89..0x00097DC5
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 7 rows of overlay 211 (header segment `57C1`, 17
trampolines, code from file offset `0x95E10`, 8,432 bytes) that no spec location covered, 1,708
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x95E10` plus it, and `DS` is `57E0`. Every routine has a trampoline in `57C1`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. Overlay 211 `+02B5` opens the spell window, window 11500, and keeps it
at `55C9:0000` and `DS:1431` (FND-INPUT-012); it loads the frames of overlay 190 `+34E7` with 8
into `4E71:002F` (`+03D4..+03E6`), as overlay 175 does (FND-EXE-598). A party member `m` from 0 to
3 has a 66-byte record at `DS:19C5 + 0x42 * m` (FMT-COMBAT-002), whose word `+0x0E` is 0 for an
empty box (FND-PARTY-046), and a 49-byte record at `DS:19C9 + 0x31 * m` (FMT-COMBAT-001), with
its name at `+0x21` and the computer-control bit 5 in byte `+0x18` (FND-AI-003). `4E71:0B44` is
the current member. A window's position is in its words `+0x96` and `+0x98` (FND-EXE-598).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x95E10..0x960C5` | `57C1:0070` | Takes the window. For each member `m` 0 to 3, with `X` `0x69` for odd `m` and `0x36` otherwise and `Y` `0x5A` for `m` above 1 and `0x1E` otherwise: for an empty box it sets control `0x2C24 + m` to state 1 and then to state 0, and `0x2C2D + m` and `0x2C31 + m` to state 1, through `3EBE:071F`; otherwise it sets `0x2C31 + m` to state 4 when bit 5 of byte `+0x18` is set, sets `DS:2D14` to `0xF0` when `m` is not `4E71:0B44`, and calls overlay 186 `+00D4` with the 49-byte record, `X` and `Y + 10`, overlay 183 `+189B` with `DS:1431`, the two records, `X + 13` and `Y + 0x21` (FND-PARTY-059), overlay 190 `+20B0` with `m`, `X - 13` and `Y + 0x2F`, and with `DS:2D14` at `0x90` overlay 183 `+191D` with `DS:1431`, the two records, `X + 13` and `Y + 0x28`, then sets `DS:2D14` to `0x6C`. After the four it calls overlay 186 `+0000` with 20080, 1, 320 and 11. For a current member whose box is filled it calls `+1A4F` and `+1CBD` and writes the name to control `0xFA3` through `4228:002B` and to control `0x2C36` through overlay 190 `+079F`; for an empty box it writes `INACTIVE CHARACTER` (`DS:2BD5`) to both. Ends with `28C9:2A81` with 0 and `28C9:2A44` with 1, and returns 0. No direct call was found; `+02B5` passes `57C1:0070` to `3A8E:0D0C` for the window at `55C9:0000` (`+03FE..+040F`). |
| `+073D` | `0x9654D..0x9662B` | `57C1:0043` | Takes no argument. Calls overlay 213 `+02D5` when `55CB:0000` is set. Stores the low byte of `4E71:0B44` in `DS:4264` and 3 in the word `DS:4265 + 2 * [4E71:0B44]`, passes a null far pointer to `39D1:0430`, and clears the double words `DS:43ED` and `DS:43F1`. Frees `DS:2B9A` and `4E71:002F` through `444C:0092` when they are set, keeping the results there. Sets `DS:2D14` to `0xF0`. When `55C9:0000` is set it calls overlay 182 `+0128` with it and 1 and clears it and `DS:1431`. Clears the byte `DS:43F7`. Called from `+095F`, `+10BC`, `+16CC`, `+170B` and from `+175E` at `+1868`. |
| `+175E` | `0x9756E..0x9772F` | `57C1:0066` | Takes a word, a control number `id` and a message, and looks the message up among the five words at `+191F` (2, 4, `0x20`, `0x40`, `0x80`), returning 0 for any other. For 2 and 4, unless `55CB:0000` is set, it calls `+1933` with 0, `4E71:002F`, `DS:2B9E`, frame 4 or 5 and 1; then for 2 overlay 190 `+06B4` with `DS:1431`, control `0x2C06` and `TURN UNDEAD` (`DS:2D08`), and for 4 overlay 181 `+0845` with `DS:1431` and `0x2C35`. For `0x20`, when the byte `DS:9D76` is set, it calls `+1933` with 0, `4E71:002F`, `id - 0x2BCD`, 5 and 0, overlay 182 `+059C` with `DS:9D77` and 1, and `2C5F:0182`; overlay 173 `+306B` with `DS:426D` and 0 when the word `4C10:0019` is not 0; `+073D`; stores `DS:9D77` in `DS:445A`; calls overlay 182 `+19F8`, `3D72:0D83` and `2707:00B4` with 0 and 1; and with the current member `m` calls overlay 190 `+2448` with `m`, `0xFFFF` and `0x132`, then overlay 208 `+1612` with `m`, `DS:9D77`, `0x132`, 0 and the result of `+1FB5` with `m` and `0x132`. For `0x80` it calls overlay 213 `+02D5` and `3A8E:060D` with `DS:1431` when `55CB:0000` is set, writes `TURN UNDEAD` to control `0x2C06` as for 2, calls overlay 213 `+16EE` with `DS:9D77 - 0xA9B` and goes on as for `0x40`, which sets the byte `DS:9D76` to 1. Returns 0. No direct call was found; `+02B5` passes it to overlay 182 `+01EF` for each control `0x2BCD` to `0x2BEE` together with `57C1:0061` (`+04A2..+04C8`), and FND-EXE-588 counts `+175E` among that routine's values. |
| `+1933` | `0x97743..0x9779D` | `57C1:006B` | Takes a byte `k`, a far image, a control index `n`, a frame and a byte flag. Reads the rectangle of control `0x2BCD + n` of the window at `55C9:0000` through `+1F79`, and calls `+198D` when the flag is set and `+19EE` otherwise, with `k`, the image, the rectangle's first two words and the frame. Called from `+1405`, `+1451`, `+15CE`, `+17B5`, `+17FA`, `+1833`, `+1B84`, `+1B96`, `+1C99`, `+1D73` and `+1D89`. |
| `+198D` | `0x9779D..0x977FE` | `57C1:0025` | Takes `k`, an image, `x`, `y` and a frame. For `k` 0 calls overlay 190 `+0000` with 1, `x` plus the window `DS:1431`'s word `+0x96`, `y` plus its word `+0x98`, 0, the image and the frame; for `k` 1 the same one pixel further right and down; for any other `k` nothing. Called from `+1933` at `+1970`. |
| `+19EE` | `0x977FE..0x9785F` | `57C1:0020` | The same as `+198D` with `2D40:3BEC` in place of overlay 190 `+0000`. Called from `+1933` at `+1984`. |
| `+1F79` | `0x97D89..0x97DC5` | `57C1:002F` | Takes a far buffer, a window, an index and a base. Reads the rectangle of control `base + index` of the window through `3F96:01A6` into an 8-byte local, copies the 8 bytes to the buffer through `1000:0452`, and returns the buffer. Called from `+1933` at `+1955`. |

## Interpretation

`+0000` is the spell window's redraw callback: it draws the four party boxes with the line of
overlay 186 `+00D4`, hit points, the line of overlay 190 `+20B0` and psionic points, marks a
member under computer control, disables the buttons of an empty box, and names the current member
or shows that the box is inactive. `+073D` closes the spell window and records 3 as the member's
last window, which overlay 171 `+0E04` reads to reopen the spell window (FND-EXE-609). `+175E`
handles the list controls `0x2BCD` to `0x2BEE`: messages 2 and 4 redraw the frame at `DS:2B9E`
and set the hint text, message `0x20` after an earlier `0x40` or `0x80` closes the window and
calls overlay 208 `+1612` for the current member, and `0x80` also calls overlay 213 `+16EE`. The
constant `0x132` (306) goes where a `DATA` number goes, since `+1FB5` compares that argument with
235; with the `TURN UNDEAD` label it may be the turn undead ability, which was not checked against
the `DATA` resource. `+1933`, `+198D`, `+19EE` and `+1F79` draw a frame over a control. The names
stay `fn_211_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list; `+0000` and `+175E` may be reached only through the callback
pointers `+02B5` registers, and `+175E` only for the controls it registers it on. Overlay 186
`+00D4` and `+0000`, overlay 190 `+20B0`, `+0000`, `+06B4`, `+079F` and `+2448`, overlay 181
`+0845`, overlay 208 `+1612`, overlay 213 `+16EE`, `+1A4F`, `+1CBD` and `+1FB5` are not read
here; the reading of them as drawing a box's lines and labels rests on their arguments. What `DS:9D77`, `DS:2B9E` and the message values stand for is inferred from these
uses only; `TURN UNDEAD` may be a default label of control `0x2C06`.

## How to reproduce

Run `overlay_listing.py` for overlay 211 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`211+<offset>`), read the table at `+191F`
and `+02B5`'s registrations at `+03EA..+0427` and `+04A2..+04C8`, and compare the ranges with the
rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

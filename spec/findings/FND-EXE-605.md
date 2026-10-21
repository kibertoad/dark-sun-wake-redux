---
id: FND-EXE-605
title: The 15 routines of overlay 182 that no entry placed test that a file exists, close a window, open a region file, place a box beside or centred on another, find a free cell near a point, snap object positions to cell centres, remap an image's colours, set the interface colours for seven modes, record the sizes of five FLI files, and hide or show the party
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000688A6..0x00068914
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068978..0x000689BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068A7D..0x00068A93
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068AE7..0x00068BB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006900C..0x00069090
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000692CC..0x000693A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000693FC..0x00069450
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000698D6..0x000699B7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A40A..0x0006A472
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A6D1..0x0006ABB1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006ABBF..0x0006AD08
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

After the restage of FND-EXE-601, `npm exec -- standard-coverage` reported 15 rows of overlay 182
(header segment `56BD`, 41 trampolines, code from file offset `0x68850`, 9,400 bytes) that no
spec location covered, 2,741 bytes in all. Each row below is one row of the inventory; "offset"
is the overlay code offset, the file offset is `0x68850` plus it, and `DS` is `57E0`. Every
routine has a trampoline in `56BD`. Callers are the hits of `direct_callers.py`, checked against
the instructions around them, named by the row that holds them. A slot's 37-byte record is at
`DS:67BB + 37 * slot`, with its position in the words `+0x0A` and `+0x0C`, and the byte
`4E71:0033` is the drive number of the CD.

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0056` | `0x688A6..0x68914` | `56BD:0034` | Takes a far path. Returns 1 when `44DE:0340` with the path, a 44-byte stack buffer and 0 does not return -1, and 0 when it does. For a path whose second character is `:` and whose first is `'A'` plus the CD drive number, while `DS:13F7` is set, it returns 0 first when `DS:14E3` or the drive number is 0 or when `28C9:387E` returns nonzero. Called from `+236F` twice, overlay 187 `+25D3`, `+25FE`, `+2643`, `+270C`, `+2C08` and `+2C7D`, and three resident far calls. |
| `+0128` | `0x68978..0x689BB` | `56BD:0043` | Takes a window pointer and a flag byte, and returns 0 for a zero pointer. With the flag set it calls `3A8E:08FB` (FND-UI-045); when that returns 0, or without the flag, it calls the removal routine `3A8E:0A26`. A nonzero result from either calls `+022D`. Returns that result. Called from `+15AC` and 23 far calls in overlays 171, 175, 178 (`+06BD`, which passes 1 as the flag), 181, 184 (three), 189, 192, 194, 199 (four), 201 (two), 202, 203, 209 (two), 211, 212 and 213. |
| `+022D` | `0x68A7D..0x68A93` | `56BD:0052` | Calls `39D1:0460` with the near pointers `DS:4592` and `DS:4542` and returns the word at `DS:4592`. Called from `+0128` twice. |
| `+0297` | `0x68AE7..0x68BB5` | `56BD:005C` | Takes a region number `n`. Releases the resource file under the far pointer `DS:145A` through `38FF:02B5` when it is set, builds the name `RGNXX.GFF` (when `DS:068F` is set) or `RGN0XX.GFF` with the two `X` replaced by `n` in upper-case hexadecimal (`1000:31B9` with radix 16 and `1000:384D`, with a leading `0` for one digit), and opens it through `+0000` with the far pointer `DS:145A` and the words `0x0A` and `0x4D4`. When that fails it passes the name to overlay 180 `+0867`. Called from `+0387`, `+1A59` and overlay 187 `+1D13` (FND-EXE-596). |
| `+07BC` | `0x6900C..0x6902D` | `56BD:00D4` | Takes `x`, a width `w` and a width `d`, and returns `x + w`, or `x - d` when `x + w + d` is above 320: the left edge of a box of width `d` placed right of a box at `x`, or left of it when it would leave the screen. Called from `+1CF0` and overlays 175 (`+008A`, `+06F2`), 178 (`+0DB6`, `+0DF7`), 190 `+0E0C`, 207 `+0508` and 213 `+0030`. |
| `+07DD` | `0x6902D..0x69090` | `56BD:00CF` | Takes `y`, a height `h` and a height `d`, and returns `y + h / 2 - d / 2`, the top of a box of height `d` centred on a box at `y`, or 0 when that is below 0, or `0xC7 - d` when `y + h / 2 + d / 2` is `0xC7` or more. Called from `+1CD6` and overlays 175 (`+00A5`, `+070E`), 178 `+0E12`, 190 `+0E1D`, 207 `+051C` and 213 `+0042`. |
| `+0A7C` | `0x692CC..0x693A2` | `56BD:00A2` | Takes near pointers to `x` and `y`. From the cell `x / 16`, `y / 16` it searches squares of growing half-size `r` from 0 to 19 for a cell with column 0 to `0x7F` and row 0 to `0x61` for which `25AF:00E1` returns 0 (FND-EXPLORE-004), stores that cell's centre (`16 * cell + 8`) back through the pointers and returns 1; returns 0 when none is found. Called from `+049B`, `+067A`, overlay 171 `+0BCE` (FND-PARTY-133) and one resident far call. |
| `+0BAC` | `0x693FC..0x69450` | `56BD:00D9` | For each slot from 5 to `0x13F` whose state byte at `4F49:0C33 + 3 * slot` is 2, calls `31E0:4129` with the slot and its position rounded down to a multiple of 16 plus 8. Called from `+0A75` (FND-PARTY-251). |
| `+1086` | `0x698D6..0x69945` | `56BD:00A7` | Takes a far image pointer and a far table. With `e` the entry the double word at `+6` of the image names (through `1000:060E`), it calls `+10F5` with both arguments when byte `+4` of `e` is `0xFF` and its double word `+5` is not 0, and `1BF3:77A4` with them otherwise. Called from one resident far call. |
| `+10F5` | `0x69945..0x699B7` | `56BD:00DE` | Takes a far image pointer and a far byte table. For each of the image's entries (a count in its word `+4`, each named by a double word from `+6` on), replaces every nonzero byte of the entry's `2^b` bytes from `+0x0A`, with `b` its byte `+9`, by the table's byte at that value. Called from `+10ED`. |
| `+1BBA` | `0x6A40A..0x6A472` | `56BD:00C5` | When `DS:1437` is set, saves `DS:1440`, sets it to 1, and for each slot 0 to `0x2F` whose record byte `+0` has bit `0x20` set and bit `0x80` clear calls overlay 206 `+0986` with the slot and 1 and clears bits `0x20` and 1; then restores `DS:1440`. Called from overlay 173 `+0651` and overlay 204 `+0268`. |
| `+1E81` | `0x6A6D1..0x6A76D` | `56BD:002A` | Shows the hidden party members again, as FND-EXPLORE-005 reads it. Called from overlays 173 `+0642`, 181 `+00B5` and 204 `+01BE` and from the key routine `28C9:0CFF` for key 5 (FND-AI-004). |
| `+1F1D` | `0x6A76D..0x6A82B` | `56BD:0025` | Hides each shown party member other than the leader, as FND-EXPLORE-005 reads it; the file name pushed with the call to `2D40:3388` is `..\src\gamesupp.c` at line `0x7BF`. Called from `+132A`, overlay 181 `+00AC` and three resident far calls, one of them the key 6 call of `28C9:0CFF`. |
| `+1FDB` | `0x6A82B..0x6ABB1` | `56BD:0089` | Takes a mode 0 to 6 and sets interface colours through `39D1:03A6` (index, value), switching through the 7-word table at `+2361`; above 6 it returns. Mode 0 sets index 5 to `0xFE` and nothing else. Modes 1 to 6 set index 7 to 0, 8 to `0xF0`, 3 to `0xF3`, 2 to `0xF7`, 0 to `0xFE` and 1 to `0xFA`, then indexes 5, 4, 6 and 9 and the value passed to `1BF3:5956` with `0x14` as follows: mode 1 `0x64`, `0xB8`, `0xFA`, `0x6C`, `0xFC`; mode 2 `0xFA`, `0xB8`, `0xF3`, `0xFE`, `0xF6`; mode 3 `0x6C`, `0xFC`, `0xFC`, `0xFC`, `0xF3`; mode 4 `0xCA`, `0x6C`, `0xFA`, `0xB8`, `0xAF`; mode 5 `0xFC`, `0xB8`, `0xFA`, `0xB8`, `0xF1`; mode 6 `0xB5`, `0x69`, `0xB5`, `0xB5`, `0xC2`. Called by 50 far calls: overlay 199 (11), 213 (7), 178, 190 and 209 (5 each), 172 (4), 189 (3), 171, 181, 184, 192, 194, 202, 203, 211 and 212 (one each) and one resident call. |
| `+236F` | `0x6ABBF..0x6AD08` | `56BD:00E8` | When `DS:13F7`, `DS:13F6` and `DS:14E3` are all set, for each `i` from 1 to 5 builds `%s%u.FLI` from the string at `DS:44F2` and `i`, and `%c:\CINE\%u.FLI` from the CD drive letter and `i`; for each name that `+0056` finds and `44DE:0086` opens with mode 1 it stores what `44DE:0127` returns for the handle at `4E28:0005 + 4 * i` (first name) or `4E28:001D + 4 * i` (second), closing it with `44DE:003A`, and stores 0 there otherwise. Called from `+117C` (FND-PARTY-193). |

## Interpretation

`+0056` is a file test that refuses the CD drive when no CD is present; `+236F` uses it to note
the sizes of five cinematic files on the hard disk and on the CD. `+0128` takes a window off the
screen and `+022D` updates the list it leaves. `+07BC` and `+07DD` place a pop-up beside and
centred on the box it describes, as overlays 175, 178, 190, 207 and 213 do with their item and
description windows. `+0A7C` and `+0BAC` place objects on cell centres, `+1086` and `+10F5`
recolour an image through a 256-byte table, and `+1FDB` switches the interface colours between
seven schemes. The names stay `fn_182_<offset>` until a reading shows what the fields and tables
are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. `44DE:0340`, `44DE:0086`, `44DE:0127`, `44DE:003A`, `28C9:387E`,
`39D1:03A6`, `39D1:0460`, `1BF3:5956` and `1BF3:77A4` are not read here; the reading of them as
find-first, open, file length, close, a CD check, a colour setter and a list update rests on how
their arguments and results are used. Mode 0 of `+1FDB` may rely on the other indexes keeping the
values an earlier mode set.

## How to reproduce

Run `overlay_listing.py` for overlay 182 against the installed `DSUN.EXE` over each file range in
the table and `direct_callers.py` for each offset (`182+<offset>`), read the table at `+2361`, and
compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

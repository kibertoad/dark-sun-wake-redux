---
id: FND-EXE-618
title: The 4 routines of overlay 186 that no entry placed draw a BMP picture centred in a given width, height or both, draw a record's picture in a 30 by 30 box, report whether all 39 CACT character records of CHARSAVE.GFF are in use, and close CHARSAVE.GFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F700..0x0006F764
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F766..0x0006F7B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F7D4..0x0006F8B6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FAC1..0x0006FB21
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006FCAD..0x0006FCDD
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9; inventory coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv of FND-EXE-601
environment: null
---

## Observation

`npm exec -- standard-coverage` reported 4 rows of overlay 186 (header segment `56E9`, 12
trampolines, code from file offset `0x6F700`, 1,709 bytes) that no spec location covered, 552
bytes in all. Each row below is one row of the inventory; "offset" is the overlay code offset, the
file offset is `0x6F700` plus it, and `DS` is `57E0`. Every routine has a trampoline in `56E9`.
Callers are the hits of `direct_callers.py`, checked against the instructions around them, named
by the row that holds them. A picture is read with `38FF:04AB` (tag, number, address of a far
pointer), its width and height come from `1BF3:76C2` and `1BF3:771C` with frame 0, `2D40:3BEC`
draws a frame of it on a handle (1 for the screen) at a column and row, and `444C:0092` frees it
(FND-EXE-598). The CHARSAVE archive is held at the far pointer `DS:144A` (FND-PARTY-034).

| Offset | File range | Trampoline | What it does |
| --- | --- | --- | --- |
| `+0000` | `0x6F700..0x6F764`, `0x6F766..0x6F7B8` | `56E9:0034` | Takes a picture number `n`, a mode, `x` and `y`. Reads `BMP ` `n` and returns when it is null. Mode 1 replaces `x` by `(x - width) / 2 + 1`, mode 2 replaces `y` by `(y - height) / 2 + 1`, mode 3 does both, and any other mode keeps them; the halving is unsigned. Draws frame 0 on handle 1 at `(x, y)` and frees the picture. The `jmp` at `0x6F764` follows another `jmp` and nothing reaches it, so the row leaves its two bytes out. Called from overlays 184 (`+08F7`, `+0908`, `+091C`, `+0930`, `+0944`), 187 `+213E`, 199 (`+002C`, `+0047`, FND-EXE-603), 202 (`+01E8`, `+02C6`), 211 (`+01E8`, `+0362`) and 212 (`+0607`, `+065B`). |
| `+00D4` | `0x6F7D4..0x6F8B6` | `56E9:002A` | Takes a far record pointer, `x` and `y`. Gets a picture from overlay 190 `+34E7` with the record's word `+0x10` plus `0x23`, and its width `w` and height `h`. The column is `x + (30 - w) / 2 + 1` (halved toward zero) and the row `y + 0x1F - h`, or `y + 1` when `h` is above `0x20`. When `w` or `h` is above 30 it makes a handle with `1BF3:282D` for 1 and the box `(x + 1, y + 1)` to `(x + 30, y + 30)`, draws frame 0 on it and releases it with `2D40:3BD1`; when both fit, or the handle is `0xFFFF`, it draws frame 0 on handle 1. Frees the picture. Called from overlays 183 `+009A`, 184 `+0B4F`, 189 `+01F1`, 202 `+0089`, 209 `+1356`, 211 `+0089` and 212 `+0094`. |
| `+03C1` | `0x6FAC1..0x6FB21` | `56E9:0043` | Opens `CHARSAVE.GFF` through `+0572`, counts the numbers 1 to 39 for which `38FF:04AB` reads a `CACT` resource into a stack word with result 0 and a nonzero word, closes the archive through `+05AD`, and returns 1 when the count is 39 and 0 otherwise. Called from overlay 190 `+0E88`. |
| `+05AD` | `0x6FCAD..0x6FCDD` | `56E9:0057` | When `DS:144A` is not null, releases it through `38FF:02B5`, passes `Error: Unable to close charsave` (`DS:13C1`) to overlay 180 `+0867` when that returns nonzero, and sets `DS:144A` to 0. Called from `+040F` and overlays 171 (`+03BC`, `+0D0E`, `+1041`), 182 `+07A7`, 184 (`+136E`, `+1559`, `+1DC5`) and 192 (`+05D9`, `+0778`). |

## Interpretation

`+0000` draws a picture at a point, or centred across the width and height from the screen's
left and top edges that its caller gives; overlay 199 draws `BMP` 12001 at (0, 0) through it and a map or book picture centred in 160 by
100. `+00D4` draws a portrait or figure in a 30 by 30 frame, standing on the frame's bottom edge
and clipped when it is larger. `+03C1` tells the character list whether a free `CACT` record is
left in `CHARSAVE.GFF` (FND-PARTY-012), and `+05AD` is the close that FND-PARTY-034 names. The
names stay `fn_186_<offset>` until a reading shows what the fields and tables are.

## Alternatives

The calls through trampolines may come from code no listing has reached, so a routine may have
callers the table does not list. Overlay 190 `+34E7`, `1BF3:282D`, `2D40:3BD1`, `38FF:02B5` and
`+0572` are not read here beyond their arguments; the reading of them as a picture loader by
number, a clipped drawing handle and its release, an archive close and an archive open of
`CHARSAVE.GFF` rests on those arguments and on FND-EXE-598 and FND-PARTY-034. Which record type
`+00D4` is given, and so what its word `+0x10` holds, is not read from its callers here. With the
centring of `+0000` unsigned, a picture wider than `x` or taller than `y` lands far to the right
or below.

## How to reproduce

Run `overlay_listing.py` for overlay 186 against the installed `DSUN.EXE` over each file range in
the table and over `+0572..+05AD`, `direct_callers.py` for each offset (`186+<offset>`), read the
string at `DS:13C1`, and compare the ranges with the rows of `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`.

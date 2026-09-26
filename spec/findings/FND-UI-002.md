---
id: FND-UI-002
title: A WIND resource is a 261-byte fixed part and a counted list of 30-byte child records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x20711..0x211C1
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x294EC..0x2A005
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Read byte by byte across all 28 `WIND` resources of `RESOURCE.GFF` (FND-UI-001), past the
12-byte header and the copied bytes `0xC` to `0xA7` (FND-UI-003):

| Offset | Values |
|---|---|
| `0xA8` to `0xBD` | 0 in every record |
| `0xBE`, 16 bits | 91 to 320 |
| `0xC0`, 16 bits | 28 to 200 |
| `0xC2`, 16 bits | 0 in 22 records; a `BMP ` number in 6 |
| `0xC4` to `0xF2` | 0 in every record |
| `0xF3` | 1 to 89, equal in every record to (size - 261) / 30 |
| `0xF4` to `0x104` | 0 in every record |

The 19 distinct (`0xBE`, `0xC0`) pairs include 320 x 200 (`WIND/19500` to `/19503`, `/12503`,
`/13500`), 320 x 189 (`/11500`), 320 x 181 (`/18501`), 210 x 116 (`/10500`, `/16500`), 318 x 72
(`/12500`), 318 x 58 (`/12501`, `/12502`), 110 x 64 (`/19504`, `/19505`) and 92 x 77 (`/3020`).

The six nonzero values at `0xC2` name `BMP ` resources of the same file: `WIND/15503` names
`BMP/15002` (145 x 87, window 145 x 87); `WIND/17500`, `/17501` and `/17502` name `BMP/17000`,
`/17001` and `/17002` (the first 189 x 117, window 189 x 117); `WIND/19504` and `/19505` name
`BMP/20087` (111 x 64, windows 110 x 64).

From `0x105` on, each window holds `0xF3` records of 30 bytes, 402 in all. In every one bytes
`0x0` to `0x3` are 0, bytes `0x4` to `0x7` are `BUTN` (184 records), `APFM` (209) or `EBOX` (9),
the 32-bit value at `0x8` is the number of a resource of that tag that exists, the signed 16-bit
values at `0xC` and `0xE` are 0 to 305 and 0 to 181, and bytes `0x10` to `0x1D` are 0.

`WIND/19500` at `0x20711`, for example, is 381 bytes: 320 x 200, 0 at `0xC2`, 4 at `0xF3`, and
children naming `BUTN` 19300 at (94, 70), 19301 at (50, 87), 19302 at (64, 104) and 19303 at
(92, 120). `WIND/11500` at `0x294EC` is 2,841 bytes with 86 children.

## Interpretation

`0xBE` and `0xC0` are the window's width and height, `0xF3` counts the children, and each child
record places one control by tag and number at an `x` and `y` inside the window. `0xC2` names an
image the size of the window, most likely its background.

## Alternatives

The window record holds no position of its own on the screen. Whether the game draws the image at
`0xC2`, and whether the child positions are relative to the window's top-left corner, are not shown
by the file alone; FND-UI-016 and FND-UI-018 show controls drawn at the window's position plus the
child's. Earlier notes placed a window image at `0x3A`; that value is part of the copied bytes
(FND-UI-003).

## How to reproduce

Read each `WIND` resource of `RESOURCE.GFF` through the directory, tabulate each byte from `0xA8`
to `0x104`, and read the 30-byte records from `0x105`, looking up each tag and number in the
directory.

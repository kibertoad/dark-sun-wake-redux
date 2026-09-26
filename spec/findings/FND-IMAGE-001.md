---
id: FND-IMAGE-001
title: Image resources open with their own size, a frame count and a table of frame offsets
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x3CA726..0x3D8BFC
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x570282..0x570329
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x34E3E9..0x3501F0
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x33354D..0x335463
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x16324C..0x16417F
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x20..0x2F
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Every resource under the tags `BMP `, `CBMP`, `ICON`, `PORT` and `TILE` in the 26 installed `.GFF`
files, 7,731 resources in all, starts with the same three little-endian fields:

- `0x00`: a 32-bit value equal to the resource's size in every one of them.
- `0x04`: a 16-bit frame count, 1 or more.
- `0x06`: that many 32-bit offsets, relative to the start of the resource. The first is always
  `6 + 4 * count`, each is larger than the one before, and the last is smaller than the size.

Each frame runs from its offset to the next frame's offset, or to the end of the resource for the
last one, and starts with a 16-bit width and a 16-bit height. The rest of the frame is one of the
two encodings in FND-IMAGE-002 and FND-IMAGE-003, and in every frame those bytes fill it to its
end.

| File | Tag | Resources | Frames | Widest and tallest frame |
|---|---|---|---|---|
| `RESOURCE.GFF` | `BMP ` | 247 | 421 | 320, 200 |
| `RESOURCE.GFF` | `CBMP` | 6 | 73 | 154, 137 |
| `RESOURCE.GFF` | `ICON` | 400 | 615 | 302, 35 |
| `OBJEX.GFF` | `BMP ` | 3,727 | 6,539 | 64, 64 |
| `OBJEX.GFF` | `CBMP` | 130 | 1,631 | 64, 64 |
| `GPLDATA.GFF` | `PORT` | 178 | 178 | 73, 72 |
| `RGN*.GFF` (20 files) | `TILE` | 3,043 | 3,043 | 16, 16 |

Every `PORT` resource has one frame,
72x72 in 177 of them and 73x72 in one; `GPLDATA.GFF#PORT/18` is 72x72. Every `TILE` has one 16x16
frame. The located examples are `RESOURCE.GFF#BMP/11011` (one 320x200 frame),
`RESOURCE.GFF#ICON/100`, `OBJEX.GFF#BMP/599` (13 frames), `OBJEX.GFF#CBMP/25`,
`GPLDATA.GFF#PORT/18` and `RGN032.GFF#TILE/0`.

## Interpretation

The five tags hold one image layout: a list of frames, each a small bitmap with its own size.
The tag does not decide the frame encoding, since `BMP `, `CBMP` and `ICON` frames use both.

## Alternatives

The size field could be ignored by the game, which would find a resource's size from the GFF
directory instead; it agrees with the directory in every resource, so the files cannot tell.
What sets `CBMP` apart from `BMP `, and `ICON` from both, is not shown by the layout.

## How to reproduce

Walk the directory of each installed `.GFF` file (FMT-GFF-001), and for each resource of the five
tags check the size field against the resource size, the first frame offset against
`6 + 4 * count`, and the offsets for order and range. Decode each frame as FND-IMAGE-002 and
FND-IMAGE-003 describe and check that it ends where the next frame starts.

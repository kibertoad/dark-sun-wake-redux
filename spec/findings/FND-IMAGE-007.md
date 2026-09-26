---
id: FND-IMAGE-007
title: BMP 11011 in RESOURCE.GFF is a full-screen title picture drawn in PAL 11011's colours
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
    offset: 0x4E9455..0x4E9755
tool: hex inspection with Python 3.14.7, and a local rendering of the decoded frame
environment: null
---

## Observation

`RESOURCE.GFF#BMP/11011` is 58,582 bytes and holds one row-encoded frame of 320x200 pixels
(FND-IMAGE-001, FND-IMAGE-002). `RESOURCE.GFF#PAL/11011`, the palette with the same number, is a
768-byte palette (FND-IMAGE-004). The frame decoded and drawn with that palette, and viewed
locally, is the game's title picture with its logo, in colours that look as intended; drawn with
the other palettes of `RESOURCE.GFF` it does not. The rendering was not committed.

## Interpretation

The title picture is `BMP/11011` with `PAL/11011`, which suggests that at least some full-screen
pictures are paired with the palette of the same number.

## Alternatives

"Looks as intended" is a judgement made by eye, not a comparison with a capture of the running
game, so a palette that differs from `PAL/11011` in colours the picture barely uses would look the
same. When and for how long the game shows the picture, and what it draws over it, are not shown
by the files.

## How to reproduce

Decode `RESOURCE.GFF#BMP/11011` as FMT-IMAGE-002 and RULE-IMAGE-001 describe, turn each palette
index into a colour through `RESOURCE.GFF#PAL/11011`, and view the result; repeat with the other
`PAL ` resources of the file for comparison.

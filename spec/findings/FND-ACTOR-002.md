---
id: FND-ACTOR-002
title: Of the 17x35 object frames, only the first frame of BMP 599 unmirrored matches the opening leader
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: DOSBox screenshot compared with Python 3.14.7 and Pillow 12.3.0
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

The screenshot is the first gameplay frame of the opening region described in FND-IMAGE-010,
`dsun_019.png` with XXH3-128 `e8c0086991af1417b8e8a13120d33e82`, kept outside the repository.
Its 367-pixel group of differing pixels spans `(160,91)` to `(176,125)`, 17 by 35 pixels.

Every frame of every `BMP ` resource of `OBJEX.GFF` was decoded as FMT-IMAGE-002. Four frames are
17 by 35 pixels: frame 0 of `OBJEX.GFF#BMP/599`, frames 0 and 1 of `OBJEX.GFF#BMP/2426`, and
frame 1 of `OBJEX.GFF#BMP/2473`. Each
was placed with its top-left at `(160,91)` over the picture FND-IMAGE-010 builds from the region's
files, once as stored and once mirrored left to right, and the 595 pixels of the 17 by 35 box were
compared with the screenshot:

| Frame | Orientation | Pixels that match |
|---|---|---|
| `BMP/599` frame 0 | as stored | 595 |
| `BMP/2426` frame 0 | as stored | 458 |
| `BMP/599` frame 0 | mirrored | 455 |
| `BMP/2426` frame 0 | mirrored | 391 |

The other four candidates match 207 pixels or fewer. `OBJEX.GFF#BMP/599` holds 13 frames, of sizes 17x35,
17x36, 11x33, 18x38, 18x38, 16x38, 16x38, 13x33, 22x32, 15x32, 20x32, 28x30 and 28x27. Of the
4,479 `OJFF` resources of `OBJEX.GFF`, only `OBJEX.GFF#OJFF/305` names it (FND-ACTOR-001).

The view's top-left is at world `(1024,1368)` (FND-IMAGE-010), so the frame's top-left is at world
`(1184,1459)`.

## Interpretation

The party leader in the first gameplay frame is drawn with frame 0 of `OBJEX.GFF#BMP/599`, the
image of `OBJEX.GFF#OJFF/305`, as stored and not mirrored.

## Alternatives

The comparison shows one frame of one run. Another resource with the same pixels, in a file other
than `OBJEX.GFF`, was not searched for, and which of the 13 frames the game shows for other
directions or actions is not shown. The screenshot records colours, so two palette indices with the
same colour cannot be told apart.

## How to reproduce

Build the picture of FND-IMAGE-010 without the leader. Decode every frame of the `BMP ` resources
of `OBJEX.GFF`, keep those 17 pixels wide and 35 high, draw each at `(160,91)` as stored and
mirrored, and count the pixels of the box whose colour matches the screenshot.

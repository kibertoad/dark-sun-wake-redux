---
id: FND-UI-021
title: The inventory screen matches BMP 13001 only in its outlines
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

Four owner captures, each 320x200, kept outside the repository, which the owner confirmed show
the inventory screen of each of the four party members in turn:

| Capture | XXH3-128 |
|---|---|
| `dsun_017.png` | `1c90212c792e05dcc37e40b08a3d5f74` |
| `dsun_018.png` | `491c774ff2eb90ff6247833cc512c406` |
| `dsun_019.png` | `c64bdb28ad84609c8a7805b89a3a479b` |
| `dsun_020.png` | `373b10e01510a078b937409201c5d4b3` |

With the images coloured as in FND-UI-016, `RESOURCE.GFF#BMP/13001`, 320 x 200 and drawn in every
pixel, matches 23,307, 23,206, 23,267 and 23,188 of its 64,000 pixels at (0, 0), its best
placement over x from -6 to 6 and y from -6 to 11. No other 320 x 200 `BMP ` of the file matches
more. The equal pixels trace the outlines of frames and controls; the textured areas differ.
`BMP/11000` matches at most 5,594 pixels at any placement tried.

## Interpretation

The inventory screen's layout follows `BMP/13001`, but the game draws its textured areas with
other pixels, or with other colours, than this image and `PAL/1000` give.

## Alternatives

The screen may use another palette for part of its colours, another image, or a different
composition; comparing under the other palettes of the file did not raise the match. These
`dsun_019.png` and `dsun_020.png` are not the files of the same names cited by FND-IMAGE-010 and
FND-UI-019, which came from an earlier session and have other hashes.

## How to reproduce

As in FND-UI-019, for every 320 x 200 `BMP ` of `RESOURCE.GFF`, and for `BMP/13001` under each
`PAL ` of the file.

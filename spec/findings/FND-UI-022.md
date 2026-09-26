---
id: FND-UI-022
title: BMP 19004 fits hundreds of places in the spell, inventory and character captures
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

`RESOURCE.GFF#BMP/19004` is one 96 x 9 frame whose top two rows are undrawn and whose other 672
pixels all have palette colour (56, 56, 77) under `PAL/1000`: a 96 x 7 rectangle of one colour.
In each of the captures of FND-UI-019 numbered `dsun_013.png` to `dsun_016.png` and of FND-UI-020
it matches all 672 pixels at 319 placements, and in each of FND-UI-021 at 386 placements.

## Interpretation

The captures contain large areas of that colour, so these matches do not show where, or whether,
the game draws `BMP/19004`.

## Alternatives

None known.

## How to reproduce

Colour `BMP/19004` with `PAL/1000` and count the placements at which every drawn pixel equals the
capture's.

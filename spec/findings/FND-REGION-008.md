---
id: FND-REGION-008
title: The first gameplay frame shows the opening region from world position (1024,1368)
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

The screenshot is the first gameplay frame of FND-IMAGE-010, `dsun_019.png` with XXH3-128
`e8c0086991af1417b8e8a13120d33e82`, kept outside the repository.

The whole 2,048 by 1,568 pixel world of the opening region, RGN032.GFF (number 50), was built
from its files as FND-IMAGE-010 describes: the `MAP ` grid's tiles, then the first frame of each
`ETAB` entry's object image in stored order, none mirrored, with colours from
`RGN032.GFF#PAL/50`. 780 pixels of the screenshot were taken as samples, every tenth column from
column 5 and every eighth row from row 4, leaving out those inside the bounds of the eight groups
of differing pixels FND-IMAGE-010 lists. For each of the 2,367,001 positions at which a 320 by
200 view fits inside the world, the samples were compared with the pixels of the built world at
the same place in the view.

At `(1024,1368)` all 780 samples match. No other position matches more than 515: the next best
are `(1024,1352)` with 515 and `(1024,1336)` with 484.

## Interpretation

The first gameplay frame shows the world rectangle whose top-left is `(1024,1368)`, and no other.

## Alternatives

The sample grid leaves out the pixels the eight groups cover, which FND-IMAGE-010 attributes to
the leader, the pointer, a button and marks along the top edge; a view that differs only there
would not be told apart. The earlier analysis recorded for the same screenshot a comparison of
808 samples against every view position, excluding the party, the pointer and the interface
button, in which `(1024,1368)` matched 807 of 808, the one mismatch being a sample on a dynamic
overlay. Its sample positions were not recorded, so that count could not be repeated exactly.

## How to reproduce

Build the world picture as described, take the sample grid from the screenshot, and count the
matching samples at every view position.

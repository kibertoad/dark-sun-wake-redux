---
id: FND-UI-018
title: The first hostile Look panel is WIND 3020 drawn at (67,44) with its three action buttons disabled
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

The owner used Look on the first hostile character in Tyr and captured the result as
`dsun_006.png`, XXH3-128 `826bb65fd1d70f62d41a005db4b719e6`, kept outside the repository. A
panel shows the target's name, `Draxan`, and `LEVEL: 10`.

With the images coloured as in FND-UI-016, frame 2 of each of `RESOURCE.GFF#ICON/15105`,
`ICON/15107` and `ICON/15106` (16 x 15, 135 drawn pixels) matches all 135 pixels at (70, 103),
(90, 103) and (110, 103), and frame 0 of `ICON/15109` (28 x 11, 224 drawn) matches all 224 at
(128, 104). Frame 0 of the first three matches 46, 44 and 44 pixels there. The dark outline of
the panel has its left column at x 67 in row 60.

In `WIND/3020` (FMT-UI-001) those icons belong to `BUTN/15306` at (3, 59), `BUTN/15308` at
(23, 59), `BUTN/15307` at (43, 59) and `BUTN/15309` at (61, 60).

## Interpretation

The Look panel is `WIND/3020` with its origin at (67, 44): each button's icon is drawn at the
origin plus the child's `x` and `y`. The three 16 x 15 buttons show frame 2, which the owner's earlier record
reads as their disabled look, and the fourth button, which closes the panel, shows frame 0. The manual
names the three actions Talk, Pick Up and Use (SRC-MANUAL-1994, page 5), which the icons show on
`BUTN/15306`, `/15308` and `/15307`.

## Alternatives

Earlier notes gave the origin as (68, 45); every icon matches fully only at (67, 44). Which frames
show the enabled and pressed states is not shown by this capture. Where the name and level come
from is not known (FND-UI-015).

## How to reproduce

Decode every frame of the four icons, colour them with `PAL/1000`, and count equal pixels at each
placement near the panel; then subtract each button's child position from the best placement.

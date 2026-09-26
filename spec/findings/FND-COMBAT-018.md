---
id: FND-COMBAT-018
title: In the capture of a party member's combat turn, the panel at (215, 4) shows her name, 50/72, Okay and Move : 15
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

The owner capture `dsun_012.png`, 320 x 200, XXH3-128 `ceeb443ee6901c2abde6e85883abfbb2`, kept
outside the repository, taken in the first fight in Tyr of a game started with START GAME. The
owner labels it as combat during the turn of the third party member, Thy'rokh (FND-PARTY-020).

The frame shows the street with the party's figures grouped in the middle and no conversation
windows. At the top right, `RESOURCE.GFF#BMP/19003` drawn with `RESOURCE.GFF#PAL/1000` at
(215, 4) matches 2,525 of the image's 3,098 opaque pixels; the others lie in (240, 8) to (287, 31),
where four lines of white text with a dark outline are drawn, each centred on the plate:
`Thy'rokh`, `50/72`, `Okay` and `Move : 15`. The pointer is the Walk arrow.

## Interpretation

During a party member's turn the status panel shows that member's name, two numbers separated by
a slash, a condition word and a movement count. FND-COMBAT-022 shows where each line comes from.

## Alternatives

The legacy record read the second and fourth lines as `90/85` and `Moves 15`; enlarged, they read
`50/72` and `Move : 15`, which matches the strings of `DSUN.EXE` (FND-COMBAT-005). It also gave the
match as the same in both panel captures; this capture's longer name leaves 2,525 matching pixels,
where FND-COMBAT-019's has 2,594.

## How to reproduce

Find the capture by its hash. Decode `BMP/19003` with `PAL/1000` (FMT-IMAGE-001), compare it with
the capture at (215, 4) pixel by pixel over its opaque pixels, and enlarge the region of the
differing pixels to read the text.

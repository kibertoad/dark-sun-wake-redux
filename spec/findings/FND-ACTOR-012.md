---
id: FND-ACTOR-012
title: The opening region names 287 objects whose images hold 477 frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0xDC89..0xF7A1
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The 867 records of `RGN032.GFF#ETAB/50`, the entity table of the opening region, RGN032.GFF
(number 50), located above, name 287 distinct `OJFF` resources of `OBJEX.GFF`. Their `image`
values name 246 distinct `BMP ` resources, which decode as FMT-IMAGE-002 into 477 frames, none
wider or taller than 64 pixels. Every one of the 287 records is 16 bytes with its last two bytes
0, and every image decodes.

## Interpretation

Drawing the opening region needs 287 object definitions and 246 images out of the 4,479 and
3,002 that `OBJEX.GFF` holds (FND-ACTOR-001).

## Alternatives

The counts cover what the file names. The game may load other objects into the region at run
time, and the special image numbers of FND-ACTOR-003 are not counted.

## How to reproduce

Read `RGN032.GFF#ETAB/50` as 8-byte records (FMT-REGION-006), collect the object numbers, read
each `OJFF` of `OBJEX.GFF` and its `image`, and decode every frame of each image.

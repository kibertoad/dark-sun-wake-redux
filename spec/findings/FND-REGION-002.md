---
id: FND-REGION-002
title: Every MAP is 12,544 bytes, each naming a TILE of its own file that is one 16x16 frame
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x7A89..0xAB89
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x2F..0xAE
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In each of the 20 region files the `MAP ` resource is 12,544 bytes. Every byte of every one is
the number of a `TILE` resource in the same file; no map names tile 0. All 3,043 `TILE`
resources are image resources (FMT-IMAGE-001) with one frame of 16x16 pixels. 589 tiles are named
by no byte of their file's map.

`TILE/0` is an empty frame in 18 files (FND-IMAGE-002). Of the others, 3,015 draw all 256 of
their pixels and 10 draw 252 or 255; 8 of those 10 are named by their map.

`RGN032.GFF#MAP/50` is located above; its bytes name 85 of the file's 94 tiles.
`RGN032.GFF#TILE/1` is located above as an example tile.

## Interpretation

`MAP ` is a grid of 12,544 cells, each naming the 16x16 tile drawn there. 12,544 is 128 times 98;
FND-REGION-006 shows the executable reads the grid as 98 rows of 128 cells.

## Alternatives

The files alone fit any grid of 12,544 cells.

## How to reproduce

For each installed `RGN*.GFF`, read the `MAP ` resource through the file's directory
(FMT-GFF-001), collect the byte values and check each against the file's `TILE` numbers. Decode
each tile's frame header (FMT-IMAGE-001, FMT-IMAGE-002) and count the pixels its runs cover.

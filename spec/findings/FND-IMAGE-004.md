---
id: FND-IMAGE-004
title: Every PAL resource is 256 colours of three bytes from 0 to 63
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4E8855..0x4E8B55
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4E9455..0x4E9755
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x7789..0x7A89
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed files hold 40 `PAL ` resources: 20 in `RESOURCE.GFF` and one in each of the 20
region files `RGN*.GFF`. `GPLDATA.GFF` and `OBJEX.GFF`, which hold the `PORT` and most of the
`BMP ` and `CBMP` images, have none. Every `PAL ` resource is exactly 768 bytes, and every byte
of every one is between 0 and 63; 63 occurs in each. The located examples are
`RESOURCE.GFF#PAL/1000`, `RESOURCE.GFF#PAL/11011` and `RGN032.GFF#PAL/50`.

## Interpretation

768 bytes are 256 colours of three components each, and the range 0 to 63 is that of the VGA
palette registers, which take six bits per component. The resource holds a palette in the form
the VGA DAC is loaded with.

## Alternatives

The order of the three components within a colour, red, green and blue, follows the VGA DAC's
order and SRC-DSUN-MUSIC-79B6927; the files alone do not show it. Which palette the game loads
for a given image is not in the image or the palette resource.

## How to reproduce

List the `PAL ` resources of every installed `.GFF` file through its directory (FMT-GFF-001) and
check each one's size and largest byte.

---
id: FND-REGION-003
title: Every GMAP is 12,544 bytes whose values are only 0x00, 0x40, 0x80 and 0xC0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0xAB89..0xDC89
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In each of the 20 region files the `GMAP` resource is 12,544 bytes. Across all 250,880 bytes the
only values are `0x00` (137,101 bytes), `0x40` (27,961), `0x80` (653) and `0xC0` (85,165), so
bits 0 to 5 are 0 in every byte. Five files use three of the values and `RGN0FF.GFF` two.

`RGN032.GFF#GMAP/50`, located above, has 8,131 bytes of `0x00`, 2,044 of `0x40`, 38 of `0x80`
and 2,331 of `0xC0`.

## Interpretation

`GMAP` is a second grid of the same size as `MAP ` with a byte of flags per cell, of which the
shipped files use only bits 6 and 7.

## Alternatives

The values could be two-bit codes instead of two independent flags. What either bit means is not
shown by the files.

## How to reproduce

For each installed `RGN*.GFF`, read the `GMAP` resource through the file's directory
(FMT-GFF-001) and count its byte values.

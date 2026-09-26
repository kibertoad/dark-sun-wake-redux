---
id: FND-ACTOR-001
title: Every OJFF resource of OBJEX.GFF is 16 bytes and names a BMP of the same file
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x1C..0x1180C
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`OBJEX.GFF` (6,816,516 bytes, XXH3-128 `e08d3cac153c547f6804ca27b155915d`) holds 4,479 `OJFF`
resources, numbered from 1 to 32,003, all in the range located above. Every one is 16 bytes. Read
byte by byte across all 4,479:

| Offset | Values |
|---|---|
| `0x0` | 4 values: 16 (1,919 records), 2 (1,507), 0 (1,028) and 18 (25) |
| `0x1` | 0 in every record |
| `0x2` | 49 values; read with `0x3` as a 16-bit value, 0 to 64 |
| `0x3` | 0 in every record |
| `0x4` | 65 values; read with `0x5` as a signed 16-bit value, -1 (3 records) to 64 |
| `0x5` | 0, or 255 in the 3 records whose value above is -1 |
| `0x6`, `0x7` | 1,788 distinct 16-bit values |
| `0x8`, `0x9` | 1,361 distinct 16-bit values |
| `0xA` | 7 values: 0 (4,342), 64 (98), 10 (22), 32 (9), 24 (5), 30 (2) and 85 (1) |
| `0xB` | 0, except 5 in `OBJEX.GFF#OJFF/147` |
| `0xC`, `0xD` | 16-bit values from 1 to 3,953 |
| `0xE`, `0xF` | 0 in every record |

Each 16-bit value at `0xC` is the number of a `BMP ` resource of `OBJEX.GFF`; 3,002 distinct
numbers occur, 127 of which also number a `CBMP` resource. The 3,002 images decode as FMT-IMAGE-002
into 5,600 frames, none wider or taller than 64 pixels.

In all 13,559 `ETAB` records of the 20 installed region files (FND-REGION-004), the signed byte at
offset 4 of the record equals byte `0xA` of the `OJFF` record the entry names.

`OBJEX.GFF#OJFF/305`, at offset `0xEEDC`, reads as the 16-bit values 16, 8, 37, 57,345, 41,728,
0, 599 and 0.

## Interpretation

An `OJFF` record is a fixed 16-byte definition of an object that names its image by the `BMP `
number at `0xC`. The last two bytes are unused or reserved. The byte at `0xA` is the vertical
offset a region's entity table repeats for each placed object.

## Alternatives

The byte boundaries of the fields at `0x6` and `0x8` are not shown by the data: `0x6` takes only 10
values and `0x8` only 8, so each may be two bytes. That the value at `0xC` is an image number
follows from every value naming a decodable `BMP `; the files alone do not show that the game reads
it that way.

## How to reproduce

Read each `OJFF` resource of `OBJEX.GFF` through the file's directory (FMT-GFF-001) and tabulate
each byte and each aligned 16-bit value. Look each value at `0xC` up among the `BMP ` and `CBMP`
numbers, decode the `BMP ` resources, and compare byte 4 of every `ETAB` record with byte `0xA` of
its `OJFF`.

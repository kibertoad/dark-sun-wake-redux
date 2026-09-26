---
id: FND-SOUND-001
title: Every VOC file and BVOC resource is one Creative Voice File header, one sound block and a terminator
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND001.VOC
    offset: 0x00..0x20
  - build: BLD-GOG-EN-1.1
    file: SPCH100.VOC
    offset: 0x00..0x20
  - build: BLD-GOG-EN-1.1
    file: CD:INTR/INTR1.VOC
    offset: 0x00..0x20
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x92D89..0x92DA9
tool: hex inspection with Python 3.14.7, reading the GFF directory as FMT-GFF-001 describes and the disc image through its ISO 9660 directory
environment: null
---

## Observation

The installation holds 147 `.VOC` files in its root, 33,989,656 bytes in all: 30 named
`SOUND` and a three-digit number, and 117 named `SPCH` and a number. The disc holds 190 more,
46,936,740 bytes: 117 in `SPEECH`, byte for byte the same as the installed `SPCH` files, and 43
in `INTR`, `INTR1.VOC` to `INTR42.VOC` and `INTR49.VOC`, which are not installed. `RESOURCE.GFF`
holds 177 `BVOC` resources, numbered from 0 to 231, 3,039,343 bytes in all; `RESOURCE.GFF#BVOC/4`
is located above.

Every one of these 514 files and resources:

- starts with the 19 bytes `Creative Voice File`, then `0x1A`, then the words 26, `0x010A` and
  `0x1129`;
- has at offset 26 a block of type 1 whose 24-bit length, at offsets 27 to 29, runs to the byte
  before the last, so its size is 31 plus that length;
- ends with a byte 0.

In each type-1 block the first byte, at offset 30, is 165, 210 or 131, and the second, at offset
31, is 0:

| Set | 165 | 210 | 131 |
|---|---|---|---|
| Installed `SOUND` and `SPCH` files | 30 | 115 | 2 |
| Disc `SPEECH` and `INTR` files | 30 | 157 | 3 |
| `BVOC` resources | 158 | 0 | 19 |

The installed files' block lengths run from 365 to 3,160,452 bytes, the resources' from 365 to
26,754.

The installed `SOUND` numbers are 1, 2, 3, 12, 21, 25, 44, 45, 46, 72, 132, 138, 147, 162 to
165, 167 to 171, 175, 181, 197, 207, 212 to 214 and 230. Only 12, 132, 138 and 147 are also
`BVOC` numbers, and none of those four resources is the same as the file.

## Interpretation

The files are Creative Voice Files of version 1.10 with a single block of 8-bit samples (codec
0) and a terminator. The first byte of the block is the sample rate's time constant, which gives
`1000000 / (256 - constant)` samples a second: 11,111 for 165, 21,739 for 210 and 8,000 for 131.
The game keeps short sound effects as `BVOC` resources and longer or later sounds as loose files.

## Alternatives

The rates follow from the file format's published definition, not from the game's code; the
game could play the samples at another rate. How the game chooses between a `BVOC` resource and
a file is FND-SOUND-007.

## How to reproduce

Read the first 32 bytes and the last byte of every `.VOC` file of the installation and the disc,
and of every `BVOC` resource of `RESOURCE.GFF`, and compare each file's size with the block
length.

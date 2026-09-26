---
id: FND-REGION-001
title: Each of the 20 region files holds one region's name, palette, two maps, entity table and tiles
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0x1C..0x20
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed game has 20 region files, `RGN001.GFF`, `RGN032.GFF` to `RGN034.GFF`,
`RGN036.GFF` to `RGN03F.GFF`, `RGN041.GFF` to `RGN045.GFF` and `RGN0FF.GFF`. Each holds exactly
one resource under each of the tags `RNME`, `PAL `, `MAP `, `GMAP` and `ETAB`, all five with the
same number, and that number written as three hexadecimal digits is the file's name: 1, 50 to
52, 54 to 63, 65 to 69 and 255. Each also holds `TILE` resources numbered from 0 up without a
gap, from 3 (in `RGN0FF.GFF`) to 247 (in `RGN041.GFF`), 3,043 in all.

In every file the `RNME` resource is the first resource, at offset `0x1C`, and is 4 to 16 bytes
of printable ASCII ending with a single NUL. The `PAL ` resource is 768 bytes (FND-IMAGE-004).

`RGN032.GFF`, 64,641 bytes with XXH3-128 `a4b22f8b69bd2a541d67ac4100fea872`, holds region 50, the
opening region, with 94 tiles; its `RNME` is 4 bytes, at `0x1C..0x20`.

## Interpretation

A region file holds one region: its name, palette, terrain map, cell flags, entity table and the
tiles its map draws. Its number is in the file name.

## Alternatives

The files do not show whether the game builds the file name from the region number or finds the
files another way, or whether it reads `RNME` at all; the executable has no `RNME` tag bytes
(FND-REGION-007).

## How to reproduce

List the resources of each installed `RGN*.GFF` through its directory (FMT-GFF-001), compare the
numbers under the five tags with each other and with the file name, and check the `RNME`
resources' bytes.

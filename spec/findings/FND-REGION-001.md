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

The names the `RNME` resources hold, without the NUL:

| File | Region | Name |
|---|---|---|
| `RGN001.GFF` | 1 | `forest` |
| `RGN032.GFF` | 50 | `Tyr` |
| `RGN033.GFF` | 51 | `VA Headquarters` |
| `RGN034.GFF` | 52 | `Pyramid` |
| `RGN036.GFF` | 54 | `Yuan-ti Tunnels` |
| `RGN037.GFF` | 55 | `El's Temple` |
| `RGN038.GFF` | 56 | `Mines1` |
| `RGN039.GFF` | 57 | `Mines2` |
| `RGN03A.GFF` | 58 | `Mines3` |
| `RGN03B.GFF` | 59 | `Jann` |
| `RGN03C.GFF` | 60 | `Mosaic` |
| `RGN03D.GFF` | 61 | `Volcano Level 2` |
| `RGN03E.GFF` | 62 | `Volcano Level 1` |
| `RGN03F.GFF` | 63 | `Volcano Level 3` |
| `RGN041.GFF` | 65 | `Silt Giants` |
| `RGN042.GFF` | 66 | `Cloud` |
| `RGN043.GFF` | 67 | `Crypt` |
| `RGN044.GFF` | 68 | `Cosmos` |
| `RGN045.GFF` | 69 | `UnderTyr` |
| `RGN0FF.GFF` | 255 | `Limbo` |

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

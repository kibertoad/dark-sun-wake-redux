---
id: FND-ACTOR-006
title: Where the OJFF, RDFF and MONR tag bytes occur in DSUN.EXE
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56A2:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 572F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 575A:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5778:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole of `DSUN.EXE` for the four ASCII bytes of each tag finds:

| Tag | Resident load image | `FBOV` pack |
|---|---|---|
| `OJFF` | `31E0:0F37`, `31E0:428B` | `DSUN.EXE+0x0007B2A0` (overlay 190), `DSUN.EXE+0x00086F42` (overlay 197), `DSUN.EXE+0x0009A620` (overlay 213) |
| `RDFF` | `28C9:080D`, `28C9:2890`, `31E0:101C` | `DSUN.EXE+0x00063E2B` (overlay 178), `DSUN.EXE+0x00073AA9` and `DSUN.EXE+0x0007407A` (overlay 188), `DSUN.EXE+0x0007C2AE`, `DSUN.EXE+0x0007C422`, `DSUN.EXE+0x0007CBF2` and `DSUN.EXE+0x0007CE11` (overlay 191), `DSUN.EXE+0x0008A0F8` (overlay 201) |
| `MONR` | none | `DSUN.EXE+0x0008D330` and `DSUN.EXE+0x0008D359` (overlay 204) |

The overlays' resident headers are at `56A2:0000` (178), `5702:0000` (188), `571F:0000` (190),
`572F:0000` (191), `575A:0000` (197), `5778:0000` (201), `5787:0000` (204) and `57CE:0000` (213).
Each resident occurrence is the immediate operand of a `push` of a 32-bit constant in the routines
FND-ACTOR-003, FND-ACTOR-004 and FND-ACTOR-005 read.

## Interpretation

The resident requests for `OJFF` and `RDFF` are the five the other findings read. Overlay code
makes further requests for both, and any code that requests `MONR` by a literal tag is in
overlay 204.

## Alternatives

The overlay occurrences were not read, so some may be data. A tag built at run time or read from
data would not show in the search.

## How to reproduce

Search `DSUN.EXE` for `4F 4A 46 46`, `52 44 46 46` and `4D 4F 4E 52`, and place each match with
`tools/ghidra/ReportFbovOverlayMap.ps1`, or as `0x5200 + (segment - 0x1000) * 16 + offset` for the
resident image.

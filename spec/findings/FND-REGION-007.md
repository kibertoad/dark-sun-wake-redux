---
id: FND-REGION-007
title: The ETAB tag occurs only in overlay code and the RNME tag nowhere in DSUN.EXE
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
    address: 56EF:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5773:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The four ASCII bytes `RNME` do not occur anywhere in `DSUN.EXE`. The bytes `ETAB` do not occur in
the resident load image `1000:0000..6237:0000`. They occur three times in the `FBOV` pack: at
`DSUN.EXE+0x00070635` and `DSUN.EXE+0x0007064B` in the code of overlay 187, whose resident
header is at `56EF:0000`, and at
`DSUN.EXE+0x00089284` in the code of overlay 200, whose header is at `5773:0000`. The bytes `GMAP`
occur twice: in the resident region loader's instruction at `362C:0322` (FND-REGION-005), and at
`DSUN.EXE+0x00071B57` in the code of overlay 187.

## Interpretation

The code that reads a region's entity table, if it names the tag, is overlay code, in overlay 187
or 200. The game may not read `RNME` at all.

## Alternatives

The overlay occurrences were not read, so they may be data that no code passes to a resource
request. A tag can also be built at run time or read from data, so the absence of `RNME` bytes
does not show that the name is unused.

## How to reproduce

Search the whole of `DSUN.EXE` for the bytes `52 4E 4D 45`, `45 54 41 42` and `47 4D 41 50`, and
place each match with `tools/ghidra/ReportFbovOverlayMap.ps1`.

---
id: FND-SAVE-003
title: The GREQ tag bytes occur only in overlay 192 of DSUN.EXE, and the CACT bytes only in overlays 171, 184 and 186
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
    address: 5664:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56DD:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56E9:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole of `DSUN.EXE` for the four ASCII bytes of each tag finds:

| Tag | Resident load image | `FBOV` pack |
|---|---|---|
| `GREQ` | none | `DSUN.EXE+0x0007D8AC`, `DSUN.EXE+0x0007D8C5` and `DSUN.EXE+0x0007DA3F` (overlay 192) |
| `CACT` | none | `DSUN.EXE+0x000584E3`, `DSUN.EXE+0x000591D4`, `DSUN.EXE+0x0005921D` and `DSUN.EXE+0x00059236` (overlay 171); `DSUN.EXE+0x0006E50E`, `DSUN.EXE+0x0006E589`, `DSUN.EXE+0x0006E59F` and `DSUN.EXE+0x0006EDE6` (overlay 184); `DSUN.EXE+0x0006FAF0` (overlay 186) |

The overlays' resident headers are at `5664:0000` (171), `56DD:0000` (184), `56E9:0000` (186)
and `5736:0000` (192). Each occurrence is the immediate operand of a `push` of a 32-bit constant:
the `CACT` ones in the routines FND-PARTY-012 reads, and the `GREQ` ones in the save and load
routines of FND-SAVE-004 and FND-SAVE-005.

An earlier search in Ghidra of the loaded image alone found none of the tags `GREQ`, `CACT`,
`PLYL` and `CSEQ`.

## Interpretation

The game reads and writes `GREQ` resources only when it saves and loads a game, and `CACT`
resources only in the code that stores and lists characters. The earlier search missed both
because it did not cover the overlay pack.

## Alternatives

A tag built at run time or read from data would not show in the search.

## How to reproduce

Search `DSUN.EXE` for `47 52 45 51` and `43 41 43 54`, and place each match with
`tools/ghidra/ReportFbovOverlayMap.ps1`.

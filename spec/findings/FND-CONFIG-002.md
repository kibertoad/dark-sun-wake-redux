---
id: FND-CONFIG-002
title: The PREF tag bytes occur once in the resident image of DSUN.EXE, inside a label, and three times in overlay 192
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
    address: 5000:8C85..5000:8C89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole of `DSUN.EXE` for the ASCII bytes `PREF` finds four occurrences:

- `5000:8C85` in the resident image, the letters `PREF` inside the label `SET PREFERENCES` of the
  executable's text;
- `DSUN.EXE+0x0007D87A`, `DSUN.EXE+0x0007D88D` and `DSUN.EXE+0x0007D96B` in overlay 192, whose
  resident header is at `5736:0000`, each the immediate operand of a `push` of a 32-bit constant.

An earlier search in Ghidra of the loaded image alone found the occurrence at `5000:8C85` and no
reference to it.

## Interpretation

The game names the `PREF` resource only in the save and load routines of overlay 192
(FND-SAVE-004, FND-SAVE-005). The resident occurrence is part of a word of text. The earlier
search did not cover the overlay pack.

## Alternatives

A tag built at run time or read from data would not show in the search.

## How to reproduce

Search `DSUN.EXE` for `50 52 45 46`, read the bytes round each match, and place each with
`tools/ghidra/ReportFbovOverlayMap.ps1`, or as `0x5200 + (segment - 0x1000) * 16 + offset` for the
resident image.

---
id: FND-IMAGE-009
title: The PORT tag does not occur in the resident load image of DSUN.EXE
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
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

A search of every block Ghidra loads from `DSUN.EXE`, which is the resident load image
`1000:0000..6237:0000`, finds no occurrence of the four ASCII bytes `PORT`. The one occurrence in
the file lies in the code of overlay 199 in the `FBOV` pack, at `DSUN.EXE+0x00088E2B`
(FND-EXE-006).

## Interpretation

The resident code does not name the `PORT` tag as a literal, so the code that loads portraits,
if it names the tag at all, is overlay code.

## Alternatives

The resident code could still load portraits through a tag it builds, reads from data or receives
from overlay code. Whether the overlay occurrence is a tag the code passes to a resource lookup has
not been checked.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run `ReportBytePattern`
for `50 4F 52 54` over all loaded blocks. Search the whole file for the same bytes and place the
match with `tools/ghidra/ReportFbovOverlayMap.ps1`.

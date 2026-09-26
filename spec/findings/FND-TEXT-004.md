---
id: FND-TEXT-004
title: The TEXT tag occurs twice in the resident image as unreferenced data and three times in the pack
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2000:7DB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2000:7E14
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the resident load image of `DSUN.EXE` the four ASCII bytes `TEXT` occur at `2000:7DB5` and
`2000:7E14`. Ghidra records no direct reference to either, and neither lies inside a decoded
instruction. The rest of the file holds three more, in the `FBOV` pack (FMT-EXE-001): at
`DSUN.EXE+0x0006FD40` in the code of overlay 186, and at `DSUN.EXE+0x00073103` and
`DSUN.EXE+0x00073173` in the code of overlay 188.

## Interpretation

No resident code names the `TEXT` tag as an operand that Ghidra resolves. The overlay occurrences
are candidates for code that requests `TEXT` resources.

## Alternatives

The resident bytes can still be passed by address through a pointer Ghidra does not resolve. The
overlay occurrences were not read, so they may be data that code never passes to a resource
request.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Run `ReportBytePattern` for `54 45 58 54` over all loaded blocks, then `ReportReferences` and
`ReportInstructionContext` on each match. Search the whole file for the same bytes and place the
other matches with `tools/ghidra/ReportFbovOverlayMap.ps1`.

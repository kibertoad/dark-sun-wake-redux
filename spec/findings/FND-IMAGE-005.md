---
id: FND-IMAGE-005
title: A resident image routine at 2D40:3BEC takes separate paths for PLAN, PLNR and other frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3BEC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31BA:000E
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The routine at `2D40:3BEC` compares a four-byte tag in the image data it is given with `PLAN` and
with `PLNR`, calls a different lower routine for each, and calls a third routine when the tag is
neither. It does not change any coordinate in the structure its caller passes. It has eight
direct callers. One is `362C:00CA`, the helper that requests a region's `TILE` resources.
Another is a helper that fills a 16-byte slot, whose only direct caller is `31BA:000E`;
`31BA:000E` resolves an object record through the `OJFF` lookup at `31E0:0EFF` and hands the
result to that helper.

A byte search of the whole file finds `PLAN` and `PLNR` at `3000:1098` and `3000:110D`, `0xAC` and
`0x121` bytes past the routine's entry, again at `3000:4DFB` and `3000:4EF3`, and once each in the
`FBOV` pack, at `DSUN.EXE+0x0007B67C` and `DSUN.EXE+0x0007B6C9` in the code of overlay 190. The
Ghidra query read only the first pair.

## Interpretation

The game tells the three frame encodings of FND-IMAGE-002 and FND-IMAGE-003 apart by the tag
after the frame's dimensions, and decodes each with its own routine. Both region tiles and object
images reach this routine.

## Alternatives

The query did not read the three lower routines, so it does not show how each decodes its
frame, whether the third path is the row encoding, or whether the routine also checks the `0xFF`
before the tag. The other six callers were not classified.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Starting from the three direct callers of the `OJFF` lookup `31E0:0EFF`, read
`31BA:000E` with `ReportInstructionContext` and one `ReportDecompileWindow`, use
`ReportReferences` to find the only caller of the 16-byte slot helper it calls, and read
`2D40:3BEC` in one decompile window. `ReportReferences` on `2D40:3BEC` lists its eight callers.

---
id: FND-COMBAT-017
title: No decoded function names both the status panel image and the interface font
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

In the overlay-mapped copy of `DSUN.EXE`, no decoded function has all six of 19003, `0x4D42`,
`0x2050` (the two halves of the tag `BMP `), 100, `0x4F46` and `0x544E` (the halves of `FONT`)
among its instruction operands.

## Interpretation

The status panel's image and the font number 100 are not requested together in one function.

## Alternatives

The panel routine draws its text with a font it does not request itself (FND-COMBAT-022), so this
result says nothing about which font that is.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", run
`ReportFunctionScalarIntersection` with the six values.

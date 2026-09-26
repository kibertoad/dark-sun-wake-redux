---
id: FND-IMAGE-008
title: No resident function uses the number 11011 as an operand, with or without the BMP tag words
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

After the full analysis of the resident load image (`1000:0000..6237:0000`), no recovered
function has all three of the scalar operands 11011, `0x4D42` (`BM` as a little-endian word) and
`0x2050` (`P ` as a little-endian word), and no decoded instruction anywhere in the image has the
operand 11011.

## Interpretation

The code that loads the title picture (FND-IMAGE-007) does not name its resource number as an
immediate value next to the tag, so the title picture has no direct static lead from its number.

## Alternatives

The number can be built at run time, read from a table or a resource, passed through resident
state, or used by overlay code, which this search of the resident image does not cover. The
search says nothing about whether or when the game shows the picture.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Run `ReportFunctionScalarIntersection` for the values 11011, `0x4D42` and `0x2050`, then
`ReportScalarConstants` for 11011 alone.

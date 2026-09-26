---
id: FND-TEXT-002
title: The FONT tag occurs twice in DSUN.EXE as data with no recorded reference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3000:9E09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:B0B4
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The four ASCII bytes `FONT` occur twice in `DSUN.EXE`, at `3000:9E09` and `5000:B0B4`, both in the
resident load image; the `FBOV` pack holds none. Ghidra records no direct reference to either
address, and neither lies inside a decoded instruction. No recovered function has all three of
the scalar operands 100 (the number of the one `FONT` resource), `0x4F46` (`FO` as a
little-endian word) and `0x544E` (`NT`).

## Interpretation

The executable does not request the font through a literal tag and number in one function that
Ghidra recovers. How it loads the font is not shown.

## Alternatives

The tag bytes can still be the data a request passes by address, through a pointer Ghidra does
not resolve, and the number can reach the request from a variable or another function. The
absence of a co-located literal says nothing about whether the font is used.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000` and run the full analysis.
Run `ReportBytePattern` for `46 4F 4E 54` over all loaded blocks, then `ReportReferences` and
`ReportInstructionContext` on each match. Run `ReportFunctionScalarIntersection` for `100`,
`0x4f46` and `0x544e`. Search the whole file for the same bytes to check the pack.

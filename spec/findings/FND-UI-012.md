---
id: FND-UI-012
title: No decoded instruction of the resident image has the number of a start-flow, Look-panel or character-view control as an operand
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

After Ghidra's default analysis of the resident load image of `DSUN.EXE`, a scan of every decoded
instruction operand for each of these unsigned decimal values, capped at 256 matches per query,
found no match:

| Values | What they number |
|---|---|
| 19300, 19301, 19302, 19303 | the four buttons of `RESOURCE.GFF#WIND/19500` |
| 19502, 2099 | `WIND/19502` and its one button |
| 19503, 18302, 19304, 2002 to 2009 | `WIND/19503`, two of its buttons and its eight class-name buttons |
| 3020, 15200, 15306, 15307, 15308, 15309 | `WIND/3020` and its frame and four buttons |
| 11308, 10308, 11318, 11319, 11320 | five buttons of `WIND/11500` |
| 16308, 16309 | two buttons of `WIND/16500` |
| 19004 | `BMP/19004` |

## Interpretation

No resident routine names these windows, controls or the image by a constant, so the code that
opens these screens or acts on these controls is not found by searching for their numbers.

## Alternatives

The numbers can be passed as arguments, read from tables or other data, built at run time, or used
by overlay code, none of which this scan sees. A negative result does not show that a control is
unused.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader, run the default analysis, and search the scalar
operands of all instructions for each value in the table.

---
id: FND-COMBAT-004
title: The resident routine at 2C5F:03F1 requests BMP 19003 into a cached pointer and has one caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:03F1..2C5F:06CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0705
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The only decoded instruction of the load image with the operand 19003 is at `2C5F:041E`, in the
routine starting at `2C5F:03F1`. When the far pointer at `57E0:0F3F` is 0, the routine pushes
the address of that pointer, 19003 and the tag `BMP ` and far-calls `38FF:04AB`, and it leaves
when the call returns a value other than 0. The routine does nothing when the far pointer at
`4E28:00D7` is 0. Its later path indexes a table of 49-byte records.

The routine has one caller, the near call at `2C5F:0705` in `2C5F:06CB`, which has eight callers
(FND-COMBAT-007).

## Interpretation

The routine draws `RESOURCE.GFF#BMP/19003`, loading it once and keeping it at `57E0:0F3F`.
FND-COMBAT-022 reads the rest of the routine: it is the combat status panel.

## Alternatives

The legacy record left the routine's purpose open; FND-COMBAT-022 settles it.

## How to reproduce

In Ghidra, run `ReportScalarConstants` for 19003, then `ReportInstructionContext` and
`ReportReferences` on the result and on its function.

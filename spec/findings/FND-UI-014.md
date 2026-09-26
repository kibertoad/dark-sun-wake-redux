---
id: FND-UI-014
title: No function of the overlay-mapped image has both the RDFF tag and the number 19003
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In a copy of `DSUN.EXE` whose header loads the code of the `FBOV` overlays as ordinary segments
(FMT-EXE-001), eleven decoded instruction operands are the tag `RDFF` (`0x46464452`). A search of
every function Ghidra recovers in that copy for one whose decoded operands include both the tag
`RDFF` and the value 19003 found none.

The one decoded operand 19003 lies in the routine that starts in overlay 182, at
`DSUN.EXE+0x00069090`. Overlay 182's code starts at `DSUN.EXE+0x00068850` and its resident
header segment is `56BD`, the location given above.

## Interpretation

No routine requests an `RDFF` record and the image `RESOURCE.GFF#BMP/19003` together, so the
combat status panel that image shows is not filled from an `RDFF` record in the same routine.

## Alternatives

The two can be joined through calls, tables, or values built at run time, which this search does
not see. The copy is a local analysis aid; overlay code is cited by its offset in the shipped file.

## How to reproduce

Build the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", import it,
run the default analysis, and run a function-level intersection of the scalar operands
`0x46464452` and 19003. Convert the mapped address of the routine that holds 19003 with the
overlay map reporter to find its offset in `DSUN.EXE`.

---
id: FND-COMBAT-016
title: No recovered direct write stores 5 in the word at 57E0:1440
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B10:009A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2BD8:00E2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2AE8:0132
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

Of the nine direct writes to `57E0:1440` (FND-COMBAT-013), the immediate ones store 0 or 1, the
setter's caller passes 2 (FND-COMBAT-014) and the other writer's caller 19 (FND-COMBAT-015). The
writer entries at `2B10:009A`, at `DSUN.EXE+0x0006A40A` (overlay 182 offset `0x1BBA`, header
segment `56BD`) and at overlay 182 offset `0x1783` have no recovered direct reference. The
remaining writer, `2BD8:00E2`, has three direct calls, all from the input loop at `2AE8:0132`, and
the context of the call passes `0x270F` (9,999), not 5.

## Interpretation

Where mode 5 comes from is not among the direct writes Ghidra records.

## Alternatives

The value may be written through a computed address, by code in an overlay that the mapped copy
does not decode, or from a table.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", follow each write
reference to `ram:00059240` to its routine, list the references to each routine, and read 16
instructions at the calls of `2BD8:00E2`.

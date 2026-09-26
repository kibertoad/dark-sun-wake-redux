---
id: FND-INPUT-008
title: No decoded function of the overlay-mapped image compares all the manual combat keys
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

In the overlay-mapped copy of `DSUN.EXE` (FMT-EXE-001), no decoded function has all six values
32, 71, 78, 80, 81 and 87 (Space and the upper-case letters `G`, `N`, `P`, `Q` and `W`) among its
operands, nor all five upper-case letter values, nor all five lower-case ones (103, 110, 112, 113,
119). The search covered the resident code and every overlay the copy maps.

## Interpretation

The combat keys of the manual (SRC-MANUAL-1994, page 77) are not compared in one function by
their character codes.

## Alternatives

The keys can be compared by scan code, through a table, or in several functions; this search does
not see those.

## How to reproduce

Build the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", import it, run
the default analysis, and run a function-level intersection of the listed values.

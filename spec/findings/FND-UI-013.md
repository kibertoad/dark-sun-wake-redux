---
id: FND-UI-013
title: No resident function has both numbers of either pair of Preferences buttons 16300 and 16301 or 16304 and 16305
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

A search of the functions Ghidra recovers in the resident load image of `DSUN.EXE` for a function
whose decoded operands include both of two values found none for 16300 and 16301, the two on-off
buttons at the top of `RESOURCE.GFF#WIND/16500`, and none for 16304 and 16305, the first pair of
9 x 8 buttons of the same window.

## Interpretation

No single resident routine tells these paired Preferences buttons apart by their numbers.

## Alternatives

Either number alone may still occur; the search looked only for the pair in one function. The
buttons may be handled through tables, callbacks or overlay code.

## How to reproduce

In Ghidra, run a function-level intersection of scalar operands for each pair of values over the
analysed resident image.

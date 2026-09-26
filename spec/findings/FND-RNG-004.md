---
id: FND-RNG-004
title: The range helper at 2D40:3A03 scales one draw over an inclusive range
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3A03
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far function at `2D40:3A03` takes two signed 16-bit arguments, `lower` and `upper`. When
`lower` is greater than or equal to `upper` it returns `lower` without calling the generator.
Otherwise it calls `1000:0822` once and returns `lower + result * (upper - lower + 1) / 32768`.
Ghidra records no direct reference to the function.

## Interpretation

A draw scaled over `lower..upper` inclusive, with an empty or reversed range costing no draw.

## Alternatives

The widths of the multiplication and of the division were not recorded. The form above implies a
32-bit product, since `result * (upper - lower + 1)` passes 16 bits for any range wider than one.
What calls the function, and so what it is used for, is not known.

## How to reproduce

Disassemble and decompile `2D40:3A03` in the import FND-RNG-001 describes, and query its direct
references.

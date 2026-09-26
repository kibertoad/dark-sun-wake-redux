---
id: FND-RNG-005
title: The repeated-roll helper at 28C9:391D sums one scaled draw per roll
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:391D
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far function at `28C9:391D` takes two signed 16-bit arguments, `count` and `sides`. When
`count` is 0 or less it returns 0 without calling the generator. Otherwise it loops `count` times,
calling `1000:0822` once per pass and adding `result * sides / 32768 + 1` to a total, and returns
the total. Ghidra records no direct reference to the function.

## Interpretation

A dice roll: `count` rolls of 1 to `sides`, each from one draw.

## Alternatives

The width of the total, of the product and of the division were not recorded. What calls the
function is not known, so reading its arguments as a count of dice and their sides is a reading of
the arithmetic, not of any use.

## How to reproduce

Disassemble and decompile `28C9:391D` in the import FND-RNG-001 describes, and query its direct
references.

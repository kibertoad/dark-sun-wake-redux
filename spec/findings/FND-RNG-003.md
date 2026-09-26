---
id: FND-RNG-003
title: The modulo reducer at 2834:061C draws once, and not at all for a divisor of 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:061C
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far function at `2834:061C` takes one argument. When it is 0 the function returns 0 without
calling the generator. Otherwise it calls `1000:0822` once and returns the remainder of the result
divided by the argument. It has eleven direct callers, all in two functions: two calls in
`2834:000C` (FND-RNG-007) and nine in `2834:0519` (FND-RNG-006).

## Interpretation

This is the game's `random(n)`: a value from 0 to `n - 1`, with the bias of a plain remainder, and a
divisor of 0 costs no draw.

## Alternatives

Whether the division is signed or unsigned was not recorded. The generator's results are never
negative, so it matters only for a divisor above 32767.

## How to reproduce

Disassemble `2834:061C` in the import FND-RNG-001 describes, and query its direct references.

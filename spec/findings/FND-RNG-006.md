---
id: FND-RNG-006
title: The helper at 2834:0519 succeeds when a draw modulo 10 is at most its argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:0519
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The function at `2834:0519` takes one argument. It returns false for a value outside 1 to 10, and
true for 10 without calling the modulo reducer. For 1 to 9 it calls `2834:061C` with the literal
10, once, and returns true exactly when the result is less than or equal to the argument. The
function holds nine call sites of `2834:061C`, all with the literal 10, one on each path for the
values 1 to 9.

## Interpretation

A chance test on a scale of ten: an argument of `n` from 1 to 9 succeeds with probability
`(n + 1) / 10`, and 10 always succeeds with no draw.

## Alternatives

Whether the comparison is signed was not recorded; the reducer's result is 0 to 9 either way.

## How to reproduce

Disassemble `2834:0519` in the import FND-RNG-001 describes, and inspect the context of each of the
nine calls to `2834:061C` in it.

---
id: FND-COMBAT-002
title: The operand value -10 occurs in too many functions to point at a hit point test
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

A search of the decoded instruction operands of the load image for `0xFFF6`, -10 as a 16-bit
value, reached its limit of 256 results, spread over many functions, before it covered the
program.

## Interpretation

The manual's death threshold of -10 hit points (SRC-MANUAL-1994, page 24) does not lead to the
code that tests hit points by its value alone.

## Alternatives

The test may compare with -10 in another form (`-9` with a different condition, a byte operand,
a value in a table), or one of the 256 results may be it; none was examined.

## How to reproduce

Run `ReportScalarConstants` for `0xFFF6` over the load image of `DSUN.EXE` with the default limit
of 256 results.

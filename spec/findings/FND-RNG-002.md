---
id: FND-RNG-002
title: The seed setter at 1000:0811 stores a 16-bit seed and clears the high word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0811
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far function at `1000:0811`, just before the generator, stores its one 16-bit argument in
`DS:384A` and stores 0 in `DS:384C`. Ghidra records no direct reference to it. The loaded image
contains neither the direct far-call encoding of it (`9A 11 08 00 10`) nor its address stored as a
far pointer (`11 08 00 10`).

## Interpretation

Seeding sets the generator's state to a 16-bit value with the high word 0, so only 65,536 of its
states can follow a seed.

## Alternatives

The missing references say nothing about whether or when the game seeds the generator: a call
built from a relocated segment, a register or a table would not appear in either search.

## How to reproduce

Disassemble `1000:0811` in the import FND-RNG-001 describes. Query its direct references, and
search the loaded memory for the byte patterns above.

---
id: FND-RNG-001
title: The random number generator is a 32-bit linear congruential generator at 1000:0822
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0822
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The far function at `1000:0822` reads the 32-bit value at `DS:384A` (low word) and `DS:384C` (high
word), multiplies it by `0x015A4E35`, adds 1 with carry, and writes the low 32 bits back to the same
two words. It returns the new high word ANDed with `0x7FFF`, a value from 0 to 32767.

A scan of the whole analysed instruction listing for the displacements `384A` and `384C` finds no
other data access to either word apart from the seed setter at `1000:0811` (FND-RNG-002). The only
other match for `384A` is the destination of a `JMP`, which is not a data access.

The function has three direct static callers: the modulo reducer at `2834:061C` (FND-RNG-003), the
repeated-roll helper at `28C9:391D` (FND-RNG-005) and the range helper at `2D40:3A03`
(FND-RNG-004).

## Interpretation

The game has one generator whose 32-bit state is the global at `DS:384A`, and every draw from it
that the static references reach goes through one of three reducers.

## Alternatives

Code that reaches the state through a computed address, a far pointer or an indirect call would not
show in the scan or in the reference query, so other readers and writers are not ruled out. Whether
some random outcomes use another generator, such as one in a library routine, has not been
checked.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the MZ loader at segment `0x1000`. Search function scalars for
the two halves of the multiplier, `0x015A` and `0x4E35`; the one hit is `1000:0822`. Query the
direct references to it. Scan the instruction listing for operands containing `384a` and `384c`
and keep only data accesses.

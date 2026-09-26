---
id: FND-COMBAT-011
title: The word at 57E0:0DAB is compared with seven values and written directly only with 4
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0DAB..57E0:0DAD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the overlay-mapped copy of `DSUN.EXE`, Ghidra records 22 direct references to the word at
`57E0:0DAB` (mapped `5000:8BAB`) from 14 functions: 21 reads, which compare it with or load 0, 1,
2, 3, 4, 5 or 17, and one write. The write stores 4, at `DSUN.EXE+0x00069E5C` in the overlay 182
routine at offset `0x15D7` (header segment `56BD`), which is the route of FND-RNG-008; that route
calls the panel preload of FND-COMBAT-008 only while the word is 4. The routine of FND-COMBAT-008
works while the word is 2 or 3.

## Interpretation

The word is a state of the game with at least seven values, two or three of which concern the
status panel. Which state each value stands for is not shown, and the writes that set 2 and 3 are
not found.

## Alternatives

The other writes may use a computed address, a register or code the mapped copy does not decode.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", list the
references to `ram:00058BAB` and classify each as a read or a write.

---
id: FND-INPUT-007
title: No resident instruction reads port 60h or sets DX to 60h or 64h before port I/O
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

A search of every loaded block for the encoding `E4 60` (`IN AL,60h`) finds none, and a scan of
every recovered function for `MOV DX,60h` or `MOV DX,64h` followed within 16 instructions by an
`IN` or `OUT` through `DX` finds none.

## Interpretation

The game does not read the keyboard controller directly in those forms; it reads keys through the
BIOS (FND-INPUT-005).

## Alternatives

A port number built at run time or read through another register is not covered by these
searches.

## How to reproduce

Import `DSUN.EXE` in Ghidra, search for the bytes `E4 60`, and scan each function for the two
`MOV DX` forms followed by port I/O.

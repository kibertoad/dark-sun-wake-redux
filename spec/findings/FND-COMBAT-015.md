---
id: FND-COMBAT-015
title: The writer at 2B10:00C5 stores its argument in two words, and its one direct caller passes 19
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B10:00C5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 297F:000B
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The routine at `2B10:00C5` loads its word argument (`[BP+6]`) and stores it into two resident
words, one of them `57E0:1440` (FND-COMBAT-013). Its one recovered call site, `297F:000B`, pushes
`0x13` (19) just before the call. The rest of the 129-line routine uses tables and helpers that
were not identified.

## Interpretation

The word at `57E0:1440` also takes the value 19, so the switch on 1 to 5 of FND-COMBAT-013 does not
cover all its values.

## Alternatives

Other callers may pass other values through pointers or registers.

## How to reproduce

In Ghidra, list the references to `2B10:00C5` and read the instructions at its entry and at
`297F:000B`.

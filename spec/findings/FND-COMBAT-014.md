---
id: FND-COMBAT-014
title: The one direct caller of the setter at 2B10:007A passes 2
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B10:007A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 297F:0023
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:00A4
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The routine at `2B10:007A` writes the word at `57E0:1440` (FND-COMBAT-013). It has one recovered
direct caller, at `297F:0023` in the routine Ghidra starts at `2974:0035`, which pushes 2 just
before the call. That routine tests status values and the BIOS keyboard and calls this setter or
other helpers. Its one recovered entry is an unconditional jump at `2834:00A4`, from a selection
routine whose 16 instructions around the jump initialise and compare resident words and call a
general far helper.

## Interpretation

The one direct write through the setter sets mode 2. What produces mode 5 is not found here.

## Alternatives

The setter may be called through a pointer or a register, with other values.

## How to reproduce

In Ghidra, list the references to `2B10:007A`, decompile the caller, and read 16 instructions at
`297F:0023` and at `2834:00A4`.

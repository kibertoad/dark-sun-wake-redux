---
id: FND-INPUT-006
title: The BIOS shift-flag routine 44B6:0011 has eleven far callers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0011..44B6:0017
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2E3B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0623
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The code of the resident image has four `INT 16h` encodings (`CD 16`): at `44B6:0013` in the
shift-flag routine of FND-INPUT-005, and at `44B6:00EF`, `44B6:0110` and `44B6:011C` in the
keyboard interrupt hook. Relocated far calls to `44B6:0011` come from `31E0:2E3B` and from ten
places between `28C9:0623` and `28C9:0B40`; the hook and `44B6:0063` call it with a near call.
At `31E0:2E3B` the caller has found a 37-byte record whose first byte has `0x10` set, and masks
the result with 3; at `28C9:0623` the caller tests the result against 3.

## Interpretation

Several routines test whether either shift key is held.

## Alternatives

Earlier notes, from Ghidra's references, gave the routine two callers and said no decoded site
reads a key code; the other ten calls lie in code Ghidra did not decode, and the hook reads key
codes (FND-INPUT-005). What holding shift does at each caller is not recovered.

## How to reproduce

Search the code for `CD 16`, and the relocation table for far calls to `44B6:0011`; disassemble
each caller's next instructions.

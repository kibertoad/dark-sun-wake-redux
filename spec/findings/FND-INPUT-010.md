---
id: FND-INPUT-010
title: Eleven INT 33h wrappers in segment 45B9 pass mouse services through to their callers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0000..45B9:013F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:23A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:068D
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Segment `45B9` holds eleven short routines, each loading a service number into `AX`, executing
`INT 33h` and copying the returned registers to storage its caller passes. In order they use the
services 0, 1, 2, 3, 5, 6, 7, 8, 4, 9 and `0Ch`. The resident image has 29 `CD 33`
byte pairs; eleven are these instructions, and the other 18 were not examined as code.

Relocated far calls into them:

| Routine | Service | Far callers |
|---|---|---|
| `45B9:0034` | 3, the pointer position and buttons | `28C9:23A0`, `39D1:068D` |
| `45B9:0059` | 5, button presses | None |
| `45B9:00BB` | 7, the horizontal range | `39D1:01E8` |
| `45B9:00D3` | 8, the vertical range | `39D1:01F8` |
| `45B9:0122` | `0Ch`, the event handler | `44D0:002F` (FND-INPUT-011) |

After the call at `28C9:23A0`, the routine `28C9:2322` goes on only when both returned words are
nonzero, the first is below 318 and the second below 199; otherwise it takes another branch. Its
one direct caller is `277B:056F`, in the routine `277B:0024`, whose one direct caller is the
program's entry code at `1000:0158`.

## Interpretation

The game reaches the mouse driver through one set of wrappers. A routine reached from the
entry code reads the pointer position with service 3 and treats a position on the outer rows and
columns of the 320 x 200 screen differently.

## Alternatives

Earlier notes, from Ghidra's references, listed nine services and no callers of the two range
wrappers; the relocated far calls show eleven services and one caller each. What the loop does on
either branch, and which service's result is the x coordinate, are not recovered.

This replaces FND-INPUT-004, whose location ended at `45B9:013E`, the first byte of the closing
`retf` of the routine it covers, instead of the byte after it. The documentation check found this
when the reconciled `DSUN.EXE` inventory placed a function whose last byte is that return. The end
now gives the byte after the return. Its other observations are unchanged.

## How to reproduce

Disassemble segment `45B9` with the relocations applied, and search the relocation table for far
calls whose target is each routine. Disassemble `28C9:2322` to `28C9:23C2`.

---
id: FND-COMBAT-012
title: The routine that receives the pointer coordinates from the input loop starts with five flag-guarded comparisons in overlay 195
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57A6:0070
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 574E:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the overlay-mapped copy of `DSUN.EXE`, one branch of the input loop, taken when the word at
`57E0:1440` is 5 (FND-COMBAT-013), passes two local coordinate words and the constant 1 to a
routine reached through two unconditional jumps: one at `57A6:0070` and one at
`DSUN.EXE+0x00082A7F`. The routine starts at `DSUN.EXE+0x00082A83`, offset `0x1313` of overlay 195
(code from `DSUN.EXE+0x00081770`, resident header segment `574E`). Its first 96 bytes, and the
entry path before them, are five guards of one form: each tests a bit of a byte on the stack (the
entry path bit `0x08`) and, when it is set, compares a word argument with a constant (`0x23` on
the entry path, then `0x14`, `0x66`, `0x07` and `0x15`), adding to a result that is not 0 when
they differ. Ghidra places these bytes inside a 5,612-line function it starts at
`DSUN.EXE+0x0007A5A7` in overlay 190, whose decompilation overlaps other instructions and is not
readable.

## Interpretation

The coordinate branch leads to a validation step whose flags and constants are not identified.
Nothing here ties it to choosing a target, moving, attacking or a decision of a computer-controlled
actor.

## Alternatives

The routine after the guards has not been read, and the jump at `57A6:0070` is a trampoline entry
of overlay 208's header, which does not match a jump into overlay 195; the mapped copy's layout of
these entries has not been checked against the file.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", run
`ReportReferences`, `ReportDataBytes` (96 bytes) and `ReportInstructionContext` on mapped
`8000:D883`, and convert the addresses with the overlay map reporter.

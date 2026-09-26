---
id: FND-COMBAT-009
title: The call after the panel preload passes 0x92E0, which is no resource number, and its callee is a shared patch routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4758:01D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the overlay-mapped copy of `DSUN.EXE`, Ghidra's decompilation of the overlay 182 routine of
FND-COMBAT-008 shows a call to `4758:01D5` after the panel preload, with the operand `0x92E0`
(37,600) beside a `RESOURCE.GFF` string. `RESOURCE.GFF` has no resource numbered 37,600 under any
tag. `4758:01D5` has two direct callers, this routine and the overlay 182 routine at
`DSUN.EXE+0x00069E27` (overlay offset `0x15D7`, FND-RNG-008), and contains the string `stdpatch`
and general set-up code, with no resource request.

## Interpretation

The call does not load a resource, and the routine it reaches is shared with the random-number
route of FND-RNG-008, so it says nothing about combat.

## Alternatives

In the file, the push of `0x92E0` at `DSUN.EXE+0x0006A2BB` is followed by a far call through the
overlay segment word `0x0640`, the header segment `5773` of overlay 200, entry `0x25`, not by a
call to `4758:01D5`. The two readings have not been reconciled; the mapped copy may resolve the
trampoline differently.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", decompile the
routine at mapped `74BB:0498`, list the references to `4758:01D5`, and list the resource numbers
of every tag in `RESOURCE.GFF` (FMT-GFF-001).

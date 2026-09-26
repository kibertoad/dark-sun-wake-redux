---
id: FND-COMBAT-010
title: The overlay 183 dispatcher that reaches the combat-start routine has no direct caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56CC:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The routine at `DSUN.EXE+0x0006C01D`, offset `0x103D` of overlay 183 (code from
`DSUN.EXE+0x0006AFE0`, resident header segment `56CC`, the location above), has no recovered
direct reference. When its first argument is 2 it takes a second argument from `0x7FA` to `0x7FF`;
for `0x7FC` it makes two calls and a call whose result it compares with 1, 2 and 3, and then calls
the overlay 182 routine at offset `0x19F8` (FND-COMBAT-008). The other five values make other
calls. No decoded function holds the routine's mapped segment and offset together as operands.

## Interpretation

The routine is reached through a pointer, probably as an event handler with the event number in
its second argument, and one of its events starts the work of FND-COMBAT-008.

## Alternatives

Which event `0x7FC` is, and what the neighbouring values are, is not known. The routine may be
registered through a far pointer built in a way a scan for immediate operands does not see.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", run
`ReportReferences` and `ReportDecompileWindow` on mapped `76DA:007D`, and
`ReportFunctionScalarIntersection` for `0x76DA` and `0x7D`; convert the address with the overlay
map reporter.

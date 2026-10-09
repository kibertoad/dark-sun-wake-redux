---
id: FND-INPUT-009
title: The one dispatch-shaped caller above the mapped keyboard routine requests GPLI 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the overlay-mapped copy of `DSUN.EXE` (FMT-EXE-001), five `INT 16h` encodings occur and only
the one in `44B6:0011` decodes as an instruction. Following its direct callers and one layer
above them, the only routine shaped like a dispatcher lies in overlay 187, at
`DSUN.EXE+0x00071659`; overlay 187's code starts at `DSUN.EXE+0x0006FE30` and its resident header
segment is `56EF`, the location above. Its fourth case requests `RESOURCE.GFF#GPLI/1` and updates
selector tables; none of its cases handles a key.

## Interpretation

The overlay route above the keyboard routine found by references is not a key dispatcher.

## Alternatives

Ghidra's decompilation of the callers was unreliable, and it missed the far calls that
FND-INPUT-011 lists.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", search for
`CD 16`, list the references to the decoded wrapper and one layer above, and convert the
dispatcher's mapped address with the overlay map reporter.

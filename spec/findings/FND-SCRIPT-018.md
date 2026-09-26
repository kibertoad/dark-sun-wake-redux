---
id: FND-SCRIPT-018
title: SCMD resources are loaded through a separate 64-slot cache at 31E0:1893 with two callers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1893
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1808
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2670
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The only two occurrences of the tag `SCMD` in `DSUN.EXE`, at file offsets `0x28909` and
`0x2899A`, are in the function `31E0:1893`. It asks for an `SCMD` resource by a number its
caller gives and keeps 64 cache slots: it reuses a slot that holds the number, and otherwise gets
the resource's size, allocates and copies it through shared helpers, and records it in a slot.
Its direct callers are `31E0:1808` and `31E0:2670`. The first takes a signed number from a
37-byte record found by index, asks for the negated number when it is not the sentinel, and
stores the result in the record. Neither caller is one of the calls of FND-SCRIPT-013.

## Interpretation

`SCMD` resources are loaded apart from `GPL ` and `MAS ` scripts, with their own cache, for
37-byte records whose number is stored negated. The 37-byte records are the ones at `57E0:67BB`
that the object code uses. What an `SCMD` resource holds was not read; SRC-OPENDS-5C6CBD7 calls
them animation script command tables.

## Alternatives

Callers through computed addresses would not show as direct references. That the 37-byte
records are the object slots rests on their size.

## How to reproduce

Run `ReportBytePattern` for `SCMD` and `ReportReferences` on `31E0:1893`, and
`ReportDecompileWindow` on `31E0:1893` and `31E0:1808`.

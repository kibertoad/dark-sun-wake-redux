---
id: FND-RNG-007
title: The routine at 2834:000C picks entries of a six-byte table at random
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2834:000C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

`2834:000C` scans a resident table of six-byte entries and sorts candidate indexes into local
storage under guards on the entries. It calls `2834:061C` twice, each time with a local count of
candidates that a guard has checked, and uses the result to select a byte from its local candidate
storage. The first selected index is multiplied by six and indexes the table at `DS:424E`. The
routine passes byte 1 of an entry to `2834:0519` (FND-RNG-006) and, when that succeeds, passes byte
5 of the entry to another routine.

The routine has one direct caller, in `28C9:2322`, which passes it two resident values that the
caller has checked. Ghidra shows the same function as `2AE8:0132` when it is reached from
elsewhere: both forms are linear address `0x2AFB2` of the load image.

## Interpretation

Something the game does picks an entry of a table at random, weights it by a chance on a scale of
ten, and acts on another byte of the entry.

## Alternatives

What the table holds, what the two guarded values are, and what the routine that receives byte 5
does are not known, so which game mechanic this is has not been identified.

## How to reproduce

Query the references to `2834:061C` in the import FND-RNG-001 describes; two are in `2834:000C`.
Inspect the instruction context of each, then query the references to `2834:000C`.

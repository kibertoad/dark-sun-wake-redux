---
id: FND-ACTOR-010
title: No decoded instruction of DSUN.EXE uses the displacements 43 or 76
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

After a full analysis pass of `DSUN.EXE` (634,416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`), the Ghidra script `ReportStructureOffsets` listed every
decoded instruction whose memory operand has a positive constant displacement of 43, 76, 109 or
142: the offset at which the label of FND-ACTOR-007 sits in 23 `RDFF` resources, and that offset
plus one, two and three times 33.

No decoded instruction uses the displacement 43 or 76. The instructions that use 109 or 142 lie
in routines of segments `3EBE`, `409B` and `4842`. None of the four displacements occurs in the
routines at `28C9:2839` and `31E0:0EFF`, which request `RDFF` (FND-ACTOR-003, FND-ACTOR-005).

## Interpretation

No resident routine reads a field at offset 43 of a record through a register and a constant,
which is the plainest way code would read the label from an `RDFF` resource in memory.

## Alternatives

Code can reach offset 43 by adding it at run time, through a pointer already advanced, or through
a string routine given the resource's address plus 43, and none of those shows as a displacement.
Instructions Ghidra did not decode, and all overlay code, are outside the search. This search was
recorded before the migration to the spec and was not run again during it.

## How to reproduce

Import `DSUN.EXE` into Ghidra with the 16-bit real-mode MZ loader, run the full analysis, and run
`ReportStructureOffsets` for the displacements 43, 76, 109 and 142.

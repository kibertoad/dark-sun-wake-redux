---
id: FND-CONFIG-015
title: Overlay 171 writes difficulty 3 only after a record-field threshold test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV ranges; ReportPhysicalBytePattern.ps1 and ReportFbovOverlayMap.ps1
environment: null
---

## Observation

Overlay 171's trampoline at `5664:0048` points to its routine at
`DSUN.EXE+0x00058CB5`. This routine passes the four-byte `CHAR` tag and an
index from a structure at `02F8:0000` to another routine. Later it uses a
word at `0370:0B44` as an index into 49-byte entries at the far pointer
`DS:19C9`.

At `DSUN.EXE+0x00058F0A`, it reads the word at offset 6 of that entry,
performs a sign-dependent XOR and subtraction, and compares the resulting
word as signed against `0x7FEB`. When the result is at or below that
threshold, it skips the write. Only the other branch writes 3 to the
difficulty word `DS:143A` at `DSUN.EXE+0x00058F18`. The routine then calls
another far entry and returns.

A physical search for the exact instruction pattern that writes literal 3
to `DS:143A` finds two locations in the installed executable: this branch
and `DSUN.EXE+0x0008B620`, the Preferences arrow clamp described by
FND-CONFIG-010. This search does not cover values formed in registers or
indirect writes.

## Interpretation

The overlay 171 write noted in FND-CONFIG-009 is guarded by a test on a
selected record field. It is not an unconditional initialization of the
difficulty setting. Its effect in an ordinary new-game path remains unknown
without the routine's caller and input-state reading.

## Alternatives

The record field, the sign-dependent transformation's intended meaning,
and the condition under which this routine runs were not established. The
routine could still run while starting a game if its inputs make the branch
true. No conclusion about the new-game default follows from this write or
from the loaded-image value zero alone.

## How to reproduce

Use `tools/ghidra/ReportFbovOverlayMap.ps1` to locate overlay 171 and read
trampoline `5664:0048`. Disassemble bounded windows at shipped-file offsets
`0x00058CB5..0x00058D40` and `0x00058EA0..0x00058F25`, following the branch
at `0x00058F16`. Search the physical file for `C7 06 3A 14 03 00` and
check both matches in instruction context.

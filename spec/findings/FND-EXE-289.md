---
id: FND-EXE-289
title: Relocated setup-call candidate remains unproved as an instruction-path caller
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00039A1A..0x00039A1F
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The relocation-aware incoming query for FND-EXE-288's setup target at
shipped 0x00039D78 returned one far-call byte candidate at 0x00039A1A.
Its segment operand at 0x00039A1D is a declared MZ relocation resolving
to 44B6:0018. Limit 100 did not truncate the result; no candidate was
reported unresolved. The independent known-call control at 0x00040DA9
resolved to 1000:15A5.

An isolated sixteen-bit decode of 0x00039A1A..0x00039A1F produced one
five-byte far call with raw segment operand 0x34B6 and offset 0x0018.
A separate decode beginning at shipped 0x00039A00 through 0x00039A50
produced no instruction at that initial position. That arbitrary earlier
start is not an established entry and its empty decode neither excludes
the candidate nor supplies a reachable path into it.

## Interpretation

This supplies a bounded lead for the setup's incoming transfer, not a
proved caller. Q-EXE-010 retains independently grounded entry/path
recovery, argument/state provenance and the excluded caller kinds.
The query excludes near calls, computed calls, unrelocated pointers and
instruction-boundary verification. Its positive control covers an
MZ-relocated call, not a separate FBOV-fixup control. No absence claim,
code-range declaration or complete-reading promotion follows.

## Alternatives

Treating a decoded call byte at an arbitrary start as reachable code
assumes the missing entry path. Treating the empty earlier decode as
proof that the region is data ignores the ungrounded starting position.
Treating one candidate as every possible caller ignores the exclusions.

## How to reproduce

At revision 9a77f07, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. Run the
committed incoming reporter with loadSegment 0x1000, target
0x00039D78, limit 100 and controls [0x00040DA9]. With locked Capstone
5.0.7 in sixteen-bit x86 mode decode shipped half-open ranges
0x00039A1A..0x00039A1F at initial IP 0x035A and
0x00039A00..0x00039A50 at initial IP 0x0340. These IPs label the
bounded queries only, not an established native CS binding. Keep the
missing entry/path explicit. No original execution or complete caller
search is claimed; source bytes and reports stay outside Git.

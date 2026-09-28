---
id: FND-CONFIG-074
title: Overlay 172 attempts message callback registration on seven frame identifiers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0034..566A:0043
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:02AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515
tool: Capstone 5.0.7 bounded 16-bit disassembly of overlay and resident windows; FBOV descriptor mapping
environment: null
---

## Observation

Overlay 172's `566A:0034` entry calls overlay 182's `56BD:0048`
entry with resource number `0x36B2` (14,002) at file offset
`0x00059D43`. The callee requests a `WIND` resource with its first
argument and, on its successful path, returns a far pointer; the caller
stores that pointer at `0300:000B` and `DS:1431`.

The caller then loops an identifier from `0x2BCD` through `0x2BD3`
(11,213 through 11,219). For each, it passes the window pointer,
identifier and far address `566A:0043` to resident `3F96:02AB` at
file offset `0x00059DB1`. That resident routine looks up an `APFM`
record by identifier and writes the supplied callback pointer to its
offset `0x62` when lookup succeeds. It returns `-1` on lookup failure;
the overlay loop does not branch on that return. The next call in each
iteration passes the same window pointer and identifier, bits `0x01C6`
and operation one to `3F96:02F8`, which ORs those bits into the frame's
event mask when lookup succeeds (FND-UI-007).

The frame dispatcher `3D72:0515` checks that the record's pointer at
`0x62` is nonzero and that its mask at `0x58` shares an enabled event
bit, then makes a far call through the pointer (FND-UI-006). Its local
event record contains value five. Entry `566A:0043` has an event-five
branch containing four of the internal calls to the shared message
entry inventoried by FND-CONFIG-073.
FND-CONFIG-075 shows that the shipped `WIND/14002` graph contains only
the first six targeted frames, and their masks already include the
bits the loop ORs in.

## Interpretation

The loop attempts seven registrations. The shipped window supplies six
matching frame children for a static registration and dispatch route
to the overlay 172 callback, which has guarded paths to the message
routine. Window acquisition, frame lookup, enabled event bits, and the
callback's own branches all constrain whether any particular call occurs.

## Alternatives

The live event sequence for this window remains unread. Registration
failure is not handled in the bounded loop. Runtime code might alter
the control graph, callback field or event mask later, and a computed
call elsewhere may still add message callers (FND-CONFIG-075).

## How to reproduce

Use FMT-EXE-002 through FMT-EXE-004 to map overlay 172 and overlay 182.
Disassemble `0x00059D20..0x00059DE3` to follow the window call, returned
pointer, seven-identifier loop and two resident calls. Overlay 182's
`56BD:0048` targets file offset `0x000689BB`; inspect its `WIND`
request in `0x00068914..0x00068945` and success path in
`0x000689BB..0x00068A3E`. Map descriptor 46 to resident segment
`3F96`, then read setter `0x00034E0B..0x00034E57`. FND-UI-007 gives
the mask operation, and FND-UI-006 gives the frame dispatch to `0x62`.

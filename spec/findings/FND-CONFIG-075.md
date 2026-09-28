---
id: FND-CONFIG-075
title: The shipped 14002 window contains six of the seven frames targeted by its callback loop
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x2D989
tool: Python 3.14.7 bounded GFF directory and WIND/APFM record inspection
environment: null
---

## Observation

The installed `RESOURCE.GFF` index locates `WIND/14002` at file offset
`0x0002D989`, size 501. Its child count is eight. Children zero through
five are `APFM/11213` through `/11218`, in that order; children six and
seven are `BUTN/11305` and `/10308`. Each of the six frames is 18 by 18
pixels and has the shipped event mask `0x01E6`:

| Frame | Child index | Position in window |
|---|---:|---|
| `APFM/11213` | 0 | (5, 3) |
| `APFM/11214` | 1 | (25, 3) |
| `APFM/11215` | 2 | (45, 3) |
| `APFM/11216` | 3 | (65, 3) |
| `APFM/11217` | 4 | (5, 24) |
| `APFM/11218` | 5 | (25, 24) |

`APFM/11219`, the seventh identifier in the executable's loop
(FND-CONFIG-074), is absent from `WIND/14002`. The same resource ID
occurs as a child of `WIND/11500`, `/13500`, `/17500` and `/17501`.
For the six present frames, the loop's OR mask `0x01C6` is already a
subset of their shipped mask `0x01E6`.

## Interpretation

The shipped `WIND/14002` graph supports callback registration on six
of the seven loop identifiers. On this graph, looking up `/11219`
as a child of this window cannot find it, and ORing `0x01C6` into the
six present frames changes no mask bit. The loop does not test either
setter's return (FND-CONFIG-074).

## Alternatives

This file inspection does not establish whether runtime code inserts
another child or changes a mask before the loop, nor how any resulting
event is produced in a live state. It does not label the missing
seventh child as a player-visible defect.

## How to reproduce

Read `RESOURCE.GFF` through FMT-GFF-001 through FMT-GFF-007, resolving
the indexed `WIND/14002` entry. Validate its 261-byte fixed part,
eight 30-byte children, tag and number using FMT-UI-001 and
FMT-UI-002. Read only the six named `APFM` records' dimensions and
mask fields using FMT-UI-004. Search the 28 indexed `WIND` child lists
for `APFM/11219` to verify its four other placements.

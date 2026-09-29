---
id: FND-CONFIG-093
title: The shipped 13501 window supplies all six item-feedback frames with value 32 enabled
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4BF62
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x27144
tool: DarkSunWakeRedux.Inspect ui-catalog; Python 3.14.7 bounded GFF directory inspection
environment: null
---

## Observation

The installed RESOURCE.GFF directory locates WIND/13501 at file offset
`0x0004BF62`, size 471. Its fixed part declares seven children and
window dimensions 137 by 85. Child zero is APFM/13201 at (0, 0).
The other six children are exactly the identifiers targeted by overlay
175's registration loop (FND-CONFIG-092):

| Frame | Child index | Position in window | Record file offset |
|---|---:|---|---|
| APFM/11213 | 1 | (37, 24) | `0x00027144` |
| APFM/11214 | 2 | (59, 24) | `0x000271B8` |
| APFM/11215 | 3 | (81, 24) | `0x0002722C` |
| APFM/11216 | 4 | (37, 46) | `0x000272A0` |
| APFM/11217 | 5 | (59, 46) | `0x00027314` |
| APFM/11218 | 6 | (81, 46) | `0x00027388` |

Each frame record is 116 bytes, declares dimensions 18 by 18 and holds
mask `0x01E6` at offset `0x58`. The setup's OR mask `0x0066` is
already a subset of that value, and its bit `0x0020` is set.

## Interpretation

The shipped graph supplies all six registration targets. Neither an
absent target nor exclusion of value 32 by the shipped frame mask
explains a failure to enter the feedback handler on this graph.
This differs from the seven-target loop for WIND/14002, which has
only six matching children (FND-CONFIG-075).

## Alternatives

The directory and graph do not prove successful resource acquisition,
resolved runtime child pointers, registration or pointer hit selection.
Runtime changes to the graph, mask or callback may still prevent dispatch.
This finding does not assign a player action to value 32 or establish
visible feedback (Q-CONFIG-008).

## How to reproduce

Read RESOURCE.GFF using FMT-GFF-001 through FMT-GFF-007, resolve
WIND/13501 and the six named APFM entries, and validate their embedded
tags, numbers and sizes. Read the window's child count and ordered
30-byte child records using FMT-UI-001 and FMT-UI-002. Read only
frame dimensions and event masks using FMT-UI-004. The read-only
ui-catalog command gives the same window and six frame metadata records;
filter its output to those identifiers. Compare the six identifiers and
OR mask with FND-CONFIG-092.

---
id: FND-CONFIG-095
title: Item-feedback frame selection is refreshed from the registered-window hit search
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0EB8
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly
environment: null
---

## Observation

The pointer routine at 3D72:0009 copies the packet's coordinate words
at offsets 6 and 8 to DS:A109 and DS:A10B. At file offset
`0x00032992`, it calls local hit-search routine 3D72:0EB8 with that
point and output locations for a control pointer and child index.
The search initially clears its control output and DS:A121, and sets
its index output to `0xFFFF` at `0x000337F2..0x00033808`.
It traverses the registered windows and resolved children under the
region and flag guards already recorded in FND-UI-011.

An intervening APFM-specific path is conditional on a nonzero hit,
tag APFM and nonzero DS:A13F. It branches on loaded frame word
`+0x66`: value one calls a local helper, and value two can call a
nonzero pointer at `+0x70`. A nonzero result from either path goes
to the pointer routine's early exit. The six shipped frames selected
for WIND/13501 have zero in those fields (FMT-UI-004,
FND-CONFIG-093); their live values and the additional state word
are not established here.

After that path, the pointer routine calls one of two further local
routines, based on DS:3330 and a comparison of DS:33B6 with DS:33B8.
It compares the prior selected pointer and index against the hit-search
outputs; if either differs it calls local 3D72:059E with argument zero.
It then stores the hit pointer at DS:A125 and index at DS:A12D
at `0x00032AE8..0x00032AF6`, and stores the selected record's tag
at DS:A129 or zero for a null hit. Thus these fields are refreshed
before the later APFM value-32 dispatch (FND-CONFIG-094).

## Interpretation

The value-32 branch uses a selected control refreshed from a hit search
of the registered window graph. A nonnull frame in the resource file
alone does not supply this live pointer. The routine also has earlier
frame-specific and selection-change calls, so its later bit test alone
is not a complete reachability proof.

## Alternatives

The coordinate helpers, state words, live changes to frame fields and
side effects of the local calls remain unread (Q-CONFIG-008).
The APFM-specific helper path may be inactive for the shipped fields,
or runtime initialization may replace them. A complete reading of those
writers and helper effects would distinguish the code-decided parts.
The hit search's traversal and region gates can prevent a frame from
being selected. This finding does not identify a physical device action
or establish that a feedback message appears.

## How to reproduce

Map resident 3D72 to file base `0x00032920`. Inspect packet coordinate
loads and first hit call at `0x00032945..0x00032998`; inspect the
APFM-specific guards at `0x000329A4..0x00032A03` and conditional
helper calls in bounded blocks through `0x00032A61`. Inspect selection
refresh at `0x00032AB2..0x00032B1C`. Inspect the hit search's output
initialization at `0x000337D8..0x00033808`, and follow its window
and child guards through the windows recorded by FND-UI-011.
Compare the later producer and dispatch in FND-CONFIG-094 and the
six shipped frame records in FND-CONFIG-093 and FMT-UI-004.

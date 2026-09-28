---
id: FND-CONFIG-076
title: Save and cinematic message helpers in overlay 187 have bounded direct caller routes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:0084
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56EF:00ED
tool: Python 3.14.7 FBOV fixup, MZ relocation and bounded near-call target inspection; Capstone 5.0.7 16-bit caller windows
environment: null
---

## Observation

Overlay 187's `56EF:0084` trampoline targets its save-file update and
copy wrapper at file offset `0x00070BFD`. Its single direct far call from
another overlay is at `0x0007D84A`, inside overlay 192's Save Game
routine (FND-SAVE-008). No resident MZ relocation to overlay 187's
segment targets `0084`.

Overlay 187's `56EF:00ED` trampoline targets the size-check helper at
file offset `0x00072632`. No declared fixup in another overlay and no
resident MZ relocation makes a direct far call to `00ED`. Three aligned
near calls inside overlay 187 target that helper:

| Call file offset | Containing path | Gate before the call |
|---|---|---|
| `0x00071E76` | Region-entry cinematic staging routine `56EF:0110` (FND-VIDEO-007). | The installed path open returned `-1`; it passes the local disc-path buffer. |
| `0x0007248B` | Cinematic playback entry `56EF:00FC` (FND-VIDEO-004). | The installed file was not found; it passes the candidate disc path. |
| `0x000724F9` | The same playback entry, after a failed first space check. | A local file-deletion helper returned nonzero; it retries the disc-path check. |

The size-check helper contains the two cinematic message calls at
`0x000726AC` and `0x000726D2` (FND-CONFIG-055). The save wrapper contains
the two disk-space warning calls at `0x00070C5A` and `0x00070C70`.
FND-VIDEO-005 locates the direct external calls to playback from startup
and a script request; FND-VIDEO-007 places the region-entry staging path.

## Interpretation

The four overlay 187 message sites have identified direct incoming
routes: Save Game reaches the wrapper; region staging and cinematic
playback reach the size check. The resource, drive and branch outcomes
at those routes remain conditional, and the later `WIND/10501` gate in
the shared message routine is separate (FND-CONFIG-018).

## Alternatives

A computed, copied or unrelocated pointer could add callers beyond this
direct-call census. The caller scan does not establish the live drive
capacity, whether a file copy succeeds, or whether any message is shown.

## How to reproduce

Map overlay 187's code start `0x0006FE30` and its `0084` and `00ED`
trampolines with FMT-EXE-002 through FMT-EXE-004. Search declared
overlay fixups and resident MZ relocations for far-call operands to
those entries. Scan the declared 11,611-byte overlay code for aligned
`E8` calls whose signed displacement targets code offset `0x2802`,
and disassemble bounded windows `0x00071E45..0x00071E95`,
`0x00072460..0x000724B0` and `0x000724D0..0x00072520` to confirm the
three results. Compare FND-SAVE-008, FND-VIDEO-004, FND-VIDEO-005,
FND-VIDEO-007 and FND-CONFIG-055 for the containing paths.

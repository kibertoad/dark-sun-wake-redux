---
id: FND-COMBAT-170
title: Five native record-base headers describe disjoint extents that fit the four known fixed table requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/native_save.py and getter_metadata.record_allocations at e5bc924d; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

FND-COMBAT-169 admits the getter's record-base pointer bits at one paused
native saved-map boundary but does not measure their block extents.
FND-CONFIG-209/210 and FND-COMBAT-166 identify the paragraph-count
header and marker used by the allocation/release wrappers. This new
observation reads those exact fields for the five captured record bases.

### Observation contract

On 2026-11-04 the fixed verify probe restored the same saved-map source
v181-q063-native-save-input-20261104 into a new owned muted session.
Executable/configuration, stopped instruction/time, map/input state,
native-file digest and frame checks passed. Like FND-COMBAT-169, it made
no gameplay continuation, input or guest-memory write and recorded no
draw. All fixed metadata and header comparisons also passed after the
existing same-process checkpoint roundtrip. The owned session closed.

For each kind one through five, the probe read the far pointer from
the captured DS:614F kind-indexed base table. It required a nonnull
P:0004 pointer, read exactly four bytes at P:0000 and one byte at
P:(16*s-1), where s is the first header word. It required s in 1..4095,
no offset/physical wrap, full reads and marker 77 hex. It did not follow
predecessors, inspect the heap list or read the record payloads.

### Measured extents and known logical requests

The actual pointers below are runtime addresses for this checkpoint.
The described payload excludes the four-byte header and final marker,
so it is 16*s-5 bytes starting at offset four.

| Kind | Captured base | Header paragraphs | Bytes before marker | Known logical request |
|---|---|---:|---:|---:|
| 1 | 73DB:0004 | 576 | 9211 | 9200 |
| 2 | 7662:0004 | 129 | 2059 | 2058 |
| 3 | 761B:0004 | 71 | 1131 | 1122 |
| 4 | 76E3:0004 | 96 | 1531 | Resource-dependent |
| 5 | 77DA:0004 | 30 | 475 | 460 |

All five markers were 77 hex. All five captured predecessor words were
nonzero, but no predecessor header was read. The five physical intervals
[16*P,16*(P+s)) had no pairwise overlap. Consequently their described
payload intervals are also separate at this boundary.

The known requests are not inferred from those gaps. FND-COMBAT-072/083
give the fixed 9200/2058/1122-byte calls, and FND-ACTOR-016 gives 460.
They represent 400 records of 23 bytes, 42 of 49, 17 of 66 and 20 of 23.
Each full logical table fits before its corresponding observed marker,
including the item table's final record. No record-count rounding from
the marker position is needed for those four tables.

Kind four instead obtains its requested size from the ALL/4 size call
in the initializer described by FND-CONFIG-150. A 1531-byte described
payload is not proof of that request's selected length or source. No
kind-four resource length or schema is established by this observation.

## Interpretation

For this specific paused state, pointer nonnullness is now accompanied
by observed compatible headers/markers and pairwise separate described
extents. The four known fixed logical requests fit those extents. This
removes their unmeasured header-size/separation checks at this boundary
from the producer/storage work in Q-COMBAT-047/065/067.

These are header-described extents, not an allocation-history proof or
permission to read arbitrary payload bytes. The observation does not
establish the original allocation's call arguments, the heap's complete
ownership graph, record/base equality with every other alias, continuous
backing through later calls, restore lengths or a populated cleanup root.
It also does not measure separation from every other game allocation or
stack frame. Those remain independent obligations. No entry confidence,
parity or complete-reading promotion follows.

## Alternatives

The four fixed requests exceed neither their observed marker positions
nor one another's described blocks here. A claim that every nonnull
pointer always has sufficient extent remains unsupported. Inferring the
kind-four resource length from the distance to the next captured base
would also be wrong: those bases need not describe adjacent allocations.
Endpoint equality after checkpoint restore does not prove absence of
intermediate writes or lifetime changes while the original executes.

## How to reproduce

Use the pinned project-local Python with native_save.py at e5bc924d:

```text
native_save.py v181-q063-native-save-input-20261104 v181-q067-record-headers-20261104 verify
```

Use a fresh output run name. The accepted report is native-save.json
under GAME_DIR/captures/rust-dos-native-save/v181-q067-record-headers-20261104/.
Its getter_metadata contains the fixed pointer/stride/object snapshots;
record_allocations contains the header fields, extents and overlap result.
The committed record_allocations procedure gives every read and bound.
Compare the fixed requests in FND-COMBAT-072/083 and FND-ACTOR-016 with
16*s-5, retaining their distinct units and the kind-four exception.
Licensed bytes, frame and checkpoint data remain only in GAME_DIR.

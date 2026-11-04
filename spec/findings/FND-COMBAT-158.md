---
id: FND-COMBAT-158
title: A named-cell watch attributes the later restore map change to the reached occupancy setter through its placement wrapper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and map_writer.py at 65f8278e; resident_listing.py; dinorefurb-dosbox-session 1.0.0; Python 3.12.10; Capstone 5.0.7
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed saved-map-writer stage repeated the admitted
native save/restore route through the reader return. It then admitted the
same bounded map binding and zero byte at cell (107,36), offset 4715,
identified by FND-COMBAT-157. A one-byte change watch covered only that
physical cell through the enclosing caller's matched +0FB5 epilogue.
No guest setup writes or additional input were introduced.

Exactly one change stop was observed. It stopped at relocated resident
25AF:05F9, immediately after the occupancy setter's byte OR at +05F5.
Current DS was the admitted resident data segment; ES:BX named exactly
the watched cell. DX and CX were 107 and 36, and the setter's stack x/y
arguments matched them. The byte was now 60 hexadecimal. The current
map pointer still matched the admitted binding at this stop.

The setter's saved far return was relocated 25AF:09B7. A complete bounded
reading of the already-known wrapper 25AF:0969..25AF:09CB confirms that
this is the return after its +09B4 near call, preceded by PUSH CS, to
25AF:05AA. The call takes current wrapper stack y and x, and lies on the
nonzero placement-byte path after the wrapper's successful-result test.
FND-EXPLORE-001 already describes these wrapper and setter contracts.
The probe did not capture this wrapper's slot or original caller frame.

The watch remained installed through the caller epilogue and then was
removed. No further change stop was observed; the final watched byte was
still 60. The map binding matched again there, and its full comparison
had exactly the same one-cell difference as FND-COMBAT-157, with no omitted
cells. The earlier reader-return comparison still matched the map prediction.
Item-table/output, script-chain/trigger and both effect-span comparisons
retained their matches. The run ended at the matched parent epilogue in
170 continuations, with zero recorded draws or reader-return relocations.
The owned muted session closed normally.

## Interpretation

This attributes the one observed post-reader map-cell change to a reached
occupancy setter called through its known placement wrapper. It removes
that writer identification from Q-COMBAT-066's remaining normal-case
child assessment. The setter's complete local body writes one map byte
and has no child call; this observed target is distinct from the sampled
effect-row/count spans. This is not an exhaustive census of transitive
writers or all map cells during the continuation.

The initiating object/slot, wrapper's earlier argument producers, and the
full path from enclosing region setup remain unobserved. Matching map
bindings at admission, the write and epilogue do not establish continuous
pointer or allocation identity. The watch reports changed bytes, not
idempotent stores. No claim about failed/skipped transfers, other cells,
other caller routes, visible load completion or validated parity follows.

## Alternatives

The earlier endpoint-only match to OR 60 was insufficient to name a
writer. This run adds the admitted post-store instruction, exact target,
stack/register coordinates and saved return. It still does not name the
object whose placement reached that wrapper or exclude all transient
changes elsewhere in the map.

## How to reproduce

Use the pinned project interpreter, emulator/package and verified native
saved-map checkpoint:

```text
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-map-writer-20261104 saved-map-writer
```

The accepted report and session/checkpoint metadata are retained under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-writer-20261104/.
Inspect map_writer's cell, change stop, target, coordinates, caller and
final value together with both map endpoints and retained guards. The
fixed watch refuses a changed binding or unexpected writer/arguments.
Use a fresh output name for a repeat. No persistent event or request/reply
journal is claimed; the report retains the selected observation.

For the exact static instruction/caller boundary, resolve licensed DSUN.EXE
through tools/game-dir.mjs and run the approved resident_listing.py with
complete half-open ranges 25AF:05AA..25AF:05FB and
25AF:0969..25AF:09CB. The reader verifies the configured build identity
and relocation mapping. Keep original-derived output only in GAME_DIR.

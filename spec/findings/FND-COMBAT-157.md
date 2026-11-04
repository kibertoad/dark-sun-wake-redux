---
id: FND-COMBAT-157
title: The later native restore caller changes only cell 107 36 from zero to the occupancy mask
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and map_restore.py at 519db8a8; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 a fresh saved-caller-epilogue run repeated the fixed native
save/restore route in FND-COMBAT-156. The bounded map comparison added
cell offsets, coordinates and masks, without guest writes or extra stops.

The reader-return map comparison had zero differences from the captured
GMAP/bitmap prediction. At the enclosing caller's matched +0FB5 epilogue,
exactly one of the 12544 bytes differed. Its zero-based offset was 4715,
column 107 and row 36 under FMT-REGION-003's 128-byte row layout.
The predicted byte was 00 hexadecimal and the observed byte 60;
their XOR was 60. The bounded report omitted zero differences.
The map pointer matched the earlier binding.

Item-table/output, script chain/trigger and effect-span comparisons
retained the matches described in FND-COMBAT-156. Reader and final caller
AL were one. The run completed at the caller epilogue in 169 continuations
with zero recorded draws and zero reader-return relocations. Its owned
muted session closed normally.

## Interpretation

This locates the map change in FND-COMBAT-156 and removes an unknown cell
and mask from Q-COMBAT-066's later-child assessment. FND-EXPLORE-001
already describes 25AF:05AA's occupancy write as OR 60 on an admitted
unblocked cell. The observation is consistent with that setter, but does
not establish that it ran or identify the responsible caller/slot.
Matching a mask does not establish a writer. No continuous lifetime,
intermediate-write, other-input or completed-load claim follows.

## Alternatives

Another writer, or multiple changes that yield the same endpoint, could
produce this byte. The existing report records endpoints, not a persistent
event or request/reply journal. A write watch on this named cell, admitted
only after the matched reader return, can distinguish a reached setter
from an endpoint-only interpretation. A changed map binding would require
separate admission before using that watch to describe the current map.

## How to reproduce

Use the project-local evidence Python with the pinned emulator/package:

```text
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-map-difference-20261104 saved-caller-epilogue
```

The verified source checkpoint is under GAME_DIR/captures/rust-dos-native-save/.
The accepted load-entry.json and session/checkpoint metadata are under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-difference-20261104/.
Inspect map_request.reader_return_comparison and caller_map.comparison
together with the retained frame and target checks. The latter reports
differing_cells and omitted_cells; at most 32 cells are listed, while the
exact total remains available. Use a fresh output name for a repeat.
Original-derived reports remain only in the approved local store.

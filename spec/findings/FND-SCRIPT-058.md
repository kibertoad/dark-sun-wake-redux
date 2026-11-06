---
id: FND-SCRIPT-058
title: One native reverse conversion retains matching prepared SAVE38 and live trigger pointer words at three sampled boundaries
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and script_restore.py at 68d33404; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-06 the existing fixed saved-script-conversion probe repeated
FND-COMBAT-152's native saved-map route. Following FND-SCRIPT-057's static
distinction between pointer publication and the prepared save table, the
probe added one four-byte read of SAVE/38's prepared slot, DS:1612, at each
existing script snapshot. It compared those offset/segment words with the
live trigger pointer at DS:40C0. It did not dereference the added pointer,
add a stop or introduce input or setup writes.

At all three boundaries the prepared slot's two words exactly equalled
the live pointer's two words:

| Sample | Slot equals live pointer | Slot unchanged from first sample |
| --- | --- | --- |
| After GPLI data transfer, before reverse conversion | yes | baseline |
| Before the converter's matched table-release request | yes | yes |
| At the matched numbered-reader return | yes | yes |

The existing source, DS/SS, converter frame, caller and provider controls
passed. The GPLI size query and complete data request selected the same
provider and 7896-byte length; the retained length gave a 1316-row scan
bound at release. The existing live-pointer read captured 2600 trigger
bytes, separately from the new slot read. Sixteen chain nodes supplied
32 eligible pairs, of which 12 changed in the prediction. All 103 eligible
trigger pairs changed; no eligible pair was unmatched. The sampled input
spans had no reported overlap.

Before release and at reader return, the chain fields and all trigger
bytes matched the prediction, with zero differing trigger bytes and
unchanged head/live-pointer bindings. The retained SAVE/24 and SAVE/26
targets and map comparison also passed. The reader returned AL=1 after
145 loop continuations and five dispatches, recording zero RNG draws.
The owned muted session closed without saving a post-load checkpoint.

## Interpretation

This admits equality and preservation at three sampled boundaries in one
normal native route. It does not prove continuous pointer or allocation
lifetime between stops, the earlier SAVE/38 transfer destination, source
file provenance, capacity at other uses, excluded aliases, other inputs
or failures, or the caller's later consumption of the return value.
SAVE/38's own I/O request was not a tracked target transfer in this stage.
No complete reading or general storage-admission claim follows from the
matching samples. Q-SCRIPT-019 retains those obligations.

## Alternatives

A prepared slot differing from the live pointer at any sampled boundary,
or changing between those samples, is ruled out for this run by the
explicit word comparisons. Temporary differences between samples remain
possible. A successful top-level probe flag alone would not establish the
new comparison: its individual slot fields were inspected separately.

## How to reproduce

Use the pinned project interpreter and session package, the verified
saved-map checkpoint under
GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/,
and the scripts at 68d33404:

```powershell
& $env:EVIDENCE_PYTHON -I tools/research/rust-dos-runs/load_entry.py `
  v181-q063-native-save-input-20261104 <fresh-output-name> saved-script-conversion
```

Inspect script_trace.before, before_release and reader_return in
load-entry.json, including save_slot_pointer, save_slot_matches_trigger
and the latter samples' same_save_slot. Check the existing provider,
prediction, target, map and return controls as well as passed. The
accepted output is retained under
GAME_DIR/captures/rust-dos-load-entry/v181-script-save-slot-20261106/.
The report, journals, private image and captures remain local, outside Git.

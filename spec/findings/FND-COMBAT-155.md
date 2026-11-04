---
id: FND-COMBAT-155
title: One native restore clears three nonzero item-cache words and reaches a zero-result enumeration without either traversal helper
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and item_restore.py at 58758918; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed saved-items stage repeated the verified native-save
checkpoint route through copying, selected SAVE transfers, map reconstruction,
script conversion and the matched selector-zero reader return. It added
item-clear and enumeration observations using FND-PARTY-236 and
FND-CONFIG-143/145's static contracts. No guest setup writes or additional
input were introduced. It did not repeat the object-entry census.

At loaded reader +0CC1, before its item clear, the probe matched the original
reader SS/BP and resident DS. The pointer at DS:19C1 denoted a bounded
9200-byte span. Three of the 400 record words at offset +0C were nonzero.
The probe cached the complete table in process memory and predicted a
result changing only those words to zero. That prediction differed from
the captured table at six bytes, so the comparison was not an unchanged-state
control.

At resident 2D40:2196 entry the far caller was reader +0CEA. The input index
was 523, equal to current DS:1A32. The output was relocated 5072:0040.
DS equaled SS at this boundary. The DS:19C1 pointer matched its earlier
snapshot, and all 9200 bytes at the earlier table address matched the clear
prediction with zero differing bytes. These are endpoint observations;
the probe did not watch every intermediate table-pointer read.

The sampled physical spans were the item table (9200 bytes), enumeration
output (792 bytes), enumeration's 296-byte local allocation, effect rows
(1920 bytes) and effect count (two bytes). All ten pairwise comparisons
were disjoint. The local allocation excludes saved-register and argument
storage; this is not an exhaustive alias or heap-allocation census.

The matched enumeration +240D epilogue used its original SS/BP and returned
AX zero. The output's first byte was FF and second byte zero. Neither
monitored resident helper entry, 1AA0:0566 or 1AA0:051C, was reached while
enumeration was active. The full item table still matched the clear
prediction there. The probe cached 792 output bytes only for subsequent
endpoint comparison; it did not assign gameplay meanings to other bytes.

At the matched reader return, the table pointer still matched, all 9200
item bytes still matched the prediction, and all 792 sampled enumeration
output bytes matched their epilogue snapshot. The existing checks also
retained the map prediction/binding, script chain/trigger predictions and
bindings, and both effect target byte spans/source-slot bindings.
Reader AL was one. The run completed in 148 continuations, with zero
recorded RNG draws and zero reader-return relocations. The owned muted
session closed normally and released its run lock.

## Interpretation

This admits one native item-cache clear with observable changes, a bounded
non-overlapping table/output/effect/local allocation case, and one returning
enumeration that did not invoke either traversal helper. Q-COMBAT-066 no
longer needs these normal-case boundary observations.

It does not admit a populated traversal, validate every selector/metadata
index, or establish termination and mutation footprints for helper paths.
The selected slot byte was not captured, so a zero enumeration result is
not used to claim that its input slot was zero. The comparison does not
independently bind the item table to a particular SAVE/7 file span.
No continuous pointer identity, allocation lifetime or absence of transient
mutations follows from matching endpoints. Later enclosing caller work,
failed/skipped transfers and diagnostic paths remain open. Reader AL still
reports its retained transfer flag, not aggregate child success.
No behavioral confidence or parity status is raised by this finding.

## Alternatives

This replaces FND-COMBAT-153, correcting its claim about retained
journals. A direct inspection of the named run directory and session.json
found structured reports and session/checkpoint metadata, with no persistent
event or request/reply journal. The native boundary observations, comparisons
and their limitations are unchanged; no missing journal is used as evidence.

- Treating the clear as inert contradicts the three nonzero input words
  and six changed bytes in the exact captured-table prediction.
- Treating zero output as evidence about populated traversal is unsupported:
  neither monitored helper was reached in this case.
- Matching effect bytes at return does not identify every intermediate
  writer or prove preservation by every child or enclosing caller.
- Disjoint sampled spans do not establish all metadata, slot, stack,
  released-buffer or shared-heap aliases.

## How to reproduce

Use the project-local evidence Python, the pinned installed rust-dos and
session package, and the verified saved-map checkpoint retained under
GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/.
With the tooling commit named above, run:

```text
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-items-20261104 saved-items
```

The accepted structured report and session/checkpoint metadata are retained under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-items-20261104/.
Choose a fresh output name for a repeat; existing runs are refused.
Review item_trace's clear prediction, matched enumeration entry/epilogue,
helper counters and reader-return comparisons together with the retained
map, script, effect, RNG and frame guards. Synthetic controls cover fixed
span bounds, exact clear preservation, wrong frames and arguments.
Original-derived bytes and reports remain in the approved local store.
The session metadata records event_log as null; no persistent debugger
event or request/reply journal is present in this run directory. The report
retains selected stops and comparisons, not every memory response.

---
id: FND-COMBAT-156
title: One native restore caller closes its working archive and retains item script and effect endpoints while one map byte changes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and restore_caller.py at 2b7f0ef6; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed saved-caller-epilogue stage repeated the admitted
native-save route and its map, script, item and effect checks through the
selector-zero reader return, then continued within its enclosing restore
caller. FND-COMBAT-043 supplies the static caller/cleanup contract.
The owned muted run completed at the matched caller +0FB5 epilogue in
169 continuations, with no recorded RNG draws or reader-return relocations.
It did not execute the caller's far return. The session closed normally.

### Post-setup and cleanup result sources

The reader returned AL one. At caller +0FA1, after its later region-setup
call, the probe matched the original parent SS/BP. The saved open-result
byte BP-1 and saved result BP-2 were both one. Archive reuse DS:167A was
one and working handle DS:144E was fourteen.

The reached resident close entry 38FF:02B5 had cleanup-helper return
+1C3A in the same loaded parent segment. Its double-word handle argument
was fourteen, equal to current DS:144E, and the helper's saved parent BP
matched the admitted parent. DS denoted the resident data segment.
The matched close +0427 epilogue returned AX zero. At that stop the reuse
flag remained one; the helper's later flag clear had not run yet.

At parent +0FB5, after its AL load and before LEAVE, the original parent
SS/BP matched again. AL was one, saved open/result bytes were still one,
reuse was zero, and the working-handle field still held fourteen. A retained
numeric field is not evidence that the closed archive is still usable.
The static +1C26/+0FAF contract explains this route's replacement of the
saved result by cleanup AL; because both reader and cleanup yielded one,
this run does not demonstrate masking of a failed reader.

### Endpoint comparisons and changed state

At reader return, the full map still matched the captured GMAP/plane
prediction, and script/item/effect comparisons passed. At caller epilogue:

- All 9200 item-table bytes still matched the exact cache-clear prediction,
  with the same DS:19C1 pointer. All 792 sampled enumeration-output bytes
  matched the earlier enumeration epilogue.
- The sampled script-chain pair fields and all 2600 trigger bytes still
  matched reverse-conversion predictions, with matching sampled bindings.
- Both effect spans, 1920 and two bytes, matched their captured private-file
  references. The current source-table pointers for resources 24 and 26
  still named the same sampled destinations.
- The map pointer matched its reader-return value, but one of the 12544
  map bytes differed from the prediction. The report records its count and
  digest, not the changed offset, value or writer. The first-plane pointer
  remained null; the second-plane field retained its old value, without a
  liveness claim or dereference.

The final archive snapshot had reuse zero, working handle fourteen,
region handle sixteen, game mode one, region fifty and traversal mode two.
The archive counter was seventeen, and current active/head pointers matched
each other with handle field sixteen. This differs from the reader-return
working-archive state. It does not identify every intermediate archive
operation, pathname, allocation reuse or field writer.

## Interpretation

Q-COMBAT-066 no longer needs the normal native enclosing caller's
post-setup, close-result and epilogue endpoints, including the retained
item/script/effect comparisons. The later caller is not an unchanged-state
continuation: the map comparison and archive snapshot changed.
The observation does not attribute that one map-byte difference to an
individual child or establish its gameplay meaning.

This admits neither failed/skipped transfers, populated item traversal,
the other reader caller's selector domain nor complete transitive writer
coverage. Endpoint equality does not prove absence of intermediate changes,
continuous identity or backing capacity. The caller epilogue is not an
observed far return, completed visible loading or validated parity.
No confidence or parity status changes.

## Alternatives

This replaces FND-COMBAT-154, correcting its claim about retained
journals. A direct inspection of the named run directory and session.json
found structured reports and session/checkpoint metadata, with no persistent
event or request/reply journal. The native boundary observations, comparisons
and their limitations are unchanged; no missing journal is used as evidence.

An earlier run, v181-q066-caller-20261104, reached +0FB5 but failed before
final comparisons because the probe deleted an already-consumed one-shot
close breakpoint. Its terminal diagnostic is retained. Commit 2b7f0ef6
fixes that bookkeeping and adds a synthetic rejection control; the accepted
repeat uses a fresh output directory. That diagnostic is not a game failure.

The observed map difference does not contradict the earlier reader-return
prediction: the boundaries differ. Conversely, matching effect endpoints
cannot be extended to every restored structure or every later continuation.

## How to reproduce

Use the project-local evidence Python and pinned emulator/session package,
with the verified saved-map checkpoint under
GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/:

```text
artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-caller-complete-20261104 saved-caller-epilogue
```

The accepted structured report and session/checkpoint metadata are under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-caller-complete-20261104/;
the earlier diagnostic uses the sibling v181-q066-caller-20261104/.
Choose a fresh output name for a repeat. Review caller_trace's separate
reader, saved-local, close and epilogue results alongside both endpoint
sets. Decode little-endian caller_source_slots when comparing with the
reader's formatted target pointers. Retain original-derived reports only
in the approved local store. Both run directories' session metadata records
event_log as null. Neither contains a persistent debugger event or
request/reply journal, and the structured report cannot reconstruct
unreported memory-response bytes. Synthetic controls reject wrong frames/close
arguments, missing preparation and deletion of a consumed breakpoint.

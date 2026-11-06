---
id: FND-SCRIPT-061
title: One native SAVE38 transfer uses matching reader-local prepared and live trigger pointers and copies the selected 2600-byte private-file span
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and request_provider.py at 4a1192aa; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-06 the fixed saved-trigger-transfer stage followed the admitted
native saved-map load route through the existing SAVE/24 and SAVE/26
observations to SAVE/38. FND-SCRIPT-057/059 and FND-COMBAT-035 supply its
static pointer and reader interfaces. FND-SCRIPT-058's later conversion
samples did not observe this earlier transfer. No additional input or
guest-memory write was introduced by the new stage.

### Request and buffer identity

At the matched complete-resource core entry, the observed request was
SAVE/38, start zero and count FFFFFFFF. Its wrapper return matched the
declared 38FF:04D0 frame. The supplied destination-slot address was in
the reader's SS segment. The probe read that four-byte in/out slot,
the prepared SAVE/38 pointer at DS:1612 and live pool pointer DS:40C0.
Both words of all three nonnull pointers matched.

At the core's selection boundary, the same three fields again matched,
and the selected destination equalled them. At the matched core return,
all three fields still matched each other and their request-entry values,
and equalled the selected destination. These are sampled field reads,
not a continuous write trace or proof of unchanged allocation ownership.

### Provider, selected resource and bytes copied

The existing working-archive open/publication guards admitted the selected
provider and its handle fields. The observed provider handle was 14 and
I/O handle 11. The selected plain SAVE directory had 61 rows; row index
37 named resource 38, offset 46252 and length 2600. Its directory bytes
matched the corresponding row in the verified private working file.
Directory position was 48947, length 792, and file-row offset 49429.
These are this run's selected archive values, not universal save offsets.

The selected count was 2600. At its matched return the core's AX was
zero and its reported completed count was 2600. A bounded read of all
2600 destination bytes equalled the selected span of the unchanged private
working file. Both SHA-256 values were
ae6770e5324ae0f24b6468726cbd08cea229a94466f288945fc34b2eb4ea1881.
The payload read followed the existing selected-destination contract,
not arbitrary values from the newly observed pointer fields.

The accepted run observed the fixed target order 24, 26, 38, used 108
loop continuations and five dispatches, and recorded zero RNG draws and
zero setup writes. It stopped at the resource core's matched epilogue
before the numbered reader resumed. The owned muted session closed;
no post-load checkpoint was saved.

## Interpretation

This supplies concrete native evidence for the earlier SAVE/38 transfer
destination and selected bytes under Q-SCRIPT-025. The prepared/live
pointer equality is no longer inferred only from preparation code or
the later FND-SCRIPT-058 conversion samples for this checkpoint route.

This run does not reach reverse conversion, the numbered-reader return
or the later caller. FND-SCRIPT-058 is a separate native observation from
the same verified checkpoint; matching pointer values across reports do
not establish continuous preservation between their boundaries. Other
inputs, skipped/failed transfers, temporary alias changes, capacity at
other uses, heap lifetime and later child effects remain unresolved.
No whole-entry complete reading or queue closure follows.

## Alternatives

A different prepared, local or live pointer at the three observed stops
is ruled out for this run by the explicit field comparisons. A successful
status with different destination bytes is ruled out by the full-span
comparison. A resource of another length would be rejected by this fixed
observation; its rejection is not evidence that the original rejects it.

## How to reproduce

Use the pinned interpreter, session package and verified saved-map state
under GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/:

```powershell
& $env:EVIDENCE_PYTHON -I tools/research/rust-dos-runs/load_entry.py `
  v181-q063-native-save-input-20261104 <fresh-output-name> saved-trigger-transfer
```

The accepted output is under
GAME_DIR/captures/rust-dos-load-entry/v181-trigger-transfer-20261106/.
Inspect load-entry.json's resource_requests in order, particularly the
third request's trigger_entry_bindings, trigger_selected_bindings,
trigger_return_bindings, selected, directory, reported_count and payload.
Require the individual equal/private_row_equal fields, exact selected
count, matched frame and AX zero, as well as the top-level passed flag.
Private images, reports, logs and captures remain local and out of Git.

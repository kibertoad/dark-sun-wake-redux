---
id: FND-COMBAT-146
title: One selector-zero reader returns with both target payload spans retained and matching source-slot bindings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and overlay_returns.py at 8ddfe305; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed native-source probe repeated
FND-COMBAT-140/141/142/143/144/145's verified Save checkpoint, F2/Enter,
successful copy, working-open publication, selected plain rows and target
transfer comparisons. It then continued the complete selector-zero reader
to its declared caller return. No setup writes or further input were used.
FND-COMBAT-035/111 describe the reader's loop and reconstruction, and
FND-EXE-562 describes the overlay return encodings admitted by the controller.

At reader entry, the probe validated selector zero and far return +0F2F
in the same loaded wrapper segment. It watched only that frame's saved
return-segment word. The final stop was the corresponding +0F2F with the
original SS and SP advanced by four, before the caller's POP CX. Its next
instructions matched POP CX followed by storing AL to the wrapper's local
result byte. AL was one. No return-word relocation event was observed in
this run; trap/IRET handling was exercised by synthetic controls, not this
native reader return.

At each earlier matched target transfer, reference bytes were retained
in probe process memory after equality with the selected copied-file span
had passed. At reader return the probe resampled those same bounded
destination spans and the corresponding source-table pointer slots.
Destinations below use the declared modeled mapping.

| Target | Bytes compared at reader return | Destination | Equal to transfer-time reference | Source slot still equals destination |
| --- | ---: | --- | --- | --- |
| SAVE/24 | 1920 | 4E71:024D | Yes | Yes, slot 23 |
| SAVE/26 | 2 | 57E0:2100 | Yes | Yes, slot 25 |

The references were cached at the transfers, not reread from the private
file after reconstruction. The comparison therefore does not require that
later original work left that file unchanged. No payload bytes are committed.

At return, the named archive fields were reuse one, working handle 14,
region handle zero, game mode four, region 50, traversal mode two and
issuance counter 16, all decimal. Active and head pointers equalled the
target-selected working record, with +28 handle 14. The counter was 15 at
the target selection stops and 16 at reader return. This is an observed
endpoint difference, not a trace identifying the intervening operation.

The probe accepted 97 loop continuations and recorded no RNG draw. It
stopped before the caller consumed AL and closed its owned muted session.
No post-load checkpoint or later action cleanup was observed.

## Interpretation

This supplies one reader-return retention case beyond the individual
resource returns. For the admitted spans and source slots, the endpoint
bytes and bindings survived the remainder of the numbered reader, including
whatever reconstruction route this invocation reached. AL one is the
reader's saved transfer-loop flag under FND-COMBAT-111; it is not an
aggregate guarantee that every later child succeeded or every one of the
61 potential resource slots issued a request.

Equal bytes and pointers at return do not exclude intermediate changes,
restore-to-same-value writes, aliasing, allocation/release or replacement
storage at the same address. The archive counter difference also prevents
treating matching active/head endpoint pointers as an unchanged archive
history. The experiment does not independently identify each child's effects,
admit capacity for other lengths, test failed/skipped transfers, validate
effect semantics or establish completed loading.

Q-COMBAT-065 retains actual backing/lifetime, aliases and alternative
destination/failure cases. Q-COMBAT-066 retains reached-child attribution
and later returning continuations; Q-COMBAT-064/054 retains source/record
lifetime and alternative paths. No queue item closes and no rule,
complete-reading or parity status is promoted.

## Alternatives

- A target's successful resource return alone establishes retention through
  the reader: this experiment separately sampled both spans at the matched
  reader return and found them equal in this case.
- Reconstruction must change either target's endpoint bytes or binding:
  both comparisons were equal here; other states and paths remain open.
- Matching active/head pointers proves an unchanged archive history:
  the issuance counter differed between selection and reader return.
- AL one proves completed loading: the probe stopped before the enclosing
  caller continued, and the flag does not combine every reconstruction result.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q065-reader-return-20261104 saved-reader-return`.
Use a fresh output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q065-reader-return-20261104/.
The report's reader_return records AL, target comparisons, source-slot
bindings and archive fields; reader_return_events is empty in this run.
The controller rejects wrong frames, return encodings and caller instructions,
and handles only the declared overlay 187 header/IRET contract. Synthetic
direct/trap/nested-frame controls establish tooling behavior, not additional
native return-path or payload cases.

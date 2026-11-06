---
id: FND-SCRIPT-065
title: One native run connects the verified SAVE38 payload to unchanged conversion input and predicted trigger output at reader return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py, script_restore.py and map_restore.py at 990beddd; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-06 the fixed saved-trigger-conversion stage combined the
SAVE/38 transfer and reverse-converter observations in one admitted
saved-map load. FND-SCRIPT-057/059 supply the static attempt and
FND-SCRIPT-061/058 the earlier separate native observations. The stage
added no guest write, input or breakpoint beyond their existing route.

SAVE/38 was the third fixed observed target after24 and26. Its matched
request/selection/return pointer comparisons agreed for the reader-local
destination, prepared slot and live trigger pointer. Its selected plain
directory row matched the verified private working file. The core returned
AX zero and reported2600 bytes. All2600 destination bytes equalled the
selected private-file span, with SHA-256
ae6770e5324ae0f24b6468726cbd08cea229a94466f288945fc34b2eb4ea1881.

The probe retained that private reference without writing it into the
guest. At the later existing pre-conversion snapshot, after successful
GPLI transfer, the current trigger pointer equalled the SAVE/38 transfer
destination and all2600 input bytes equalled that reference. This is
explicit same-run equality, not a comparison of pointers in separate runs.

GPLI size and data requests selected the same provider and7896-byte
length. The data count was7896 and the retained release-time scan bound
was1316. The captured chain had16 nodes and32 eligible pairs, of which
12 changed in the prediction. All103 eligible trigger pairs changed;
there were zero unmatched eligible pairs. The reported map/input-span
overlap checks had no intersections within their declared scopes.

Before the matched table-release request and at matched numbered-reader
return, the chain fields and entire trigger block matched the prediction.
There were zero differing trigger bytes. Both snapshots retained the head,
live pointer and prepared slot bindings. The predicted and actual output
SHA-256 was
97462ed12b055df0e9227fa74bd025e3e58e6f2dc04077ec34217813e3613573.
Thus the output comparison includes changed records, rather than only
matching unchanged bytes. The SAVE/38 destination itself also matched
that prediction at reader return, with its prepared source slot unchanged.

The existing SAVE/24 and26 payload/binding comparisons and map prediction
also passed at reader return. Reader AL was1. The accepted run used147
loop continuations and five dispatches, recording zero RNG draws and
zero setup writes. It stopped before the reader caller consumed AL and
closed the owned muted session without a post-load checkpoint.

## Interpretation

For this one native route, the same run links the verified SAVE/38 bytes,
the later converter input and its predicted output at two later samples.
Q-SCRIPT-025 no longer needs to compose the separate FND-SCRIPT-061 and
058 captures to admit those sampled normal-route relationships.

This does not trace every intervening write or prove unchanged allocation
ownership continuously between stops. Temporary changes, other aliases,
metadata/heap lifetime, other inputs, failed/skipped/short transfers and
later caller effects remain unresolved. No whole-entry complete reading
or queue closure follows.

## Alternatives

The first connected attempt stopped before conversion because the map
overlap check's fixed-size allowlist omitted2600. Its report has passed
false and is a tooling diagnostic, not an accepted conversion result.
The accepted retry used990beddd's fixed extent controls and fresh output.

In the accepted historical report, reader-return target38's basis field
explicitly says reverse-conversion prediction, while the expected hash
still uses the inherited key private_span_sha256. At that boundary it is
the prediction hash, not the serialized source hash. The transfer-time
payload comparison remains a private-span comparison. Tooling commit
dfdf8fba renames only the former key to prediction_sha256 for future runs;
the accepted report is retained unchanged.

## How to reproduce

Use the pinned project interpreter, package and verified saved-map state
under GAME_DIR/captures/rust-dos-native-save/v181-q063-native-save-input-20261104/:

```powershell
& $env:EVIDENCE_PYTHON -I tools/research/rust-dos-runs/load_entry.py `
  v181-q063-native-save-input-20261104 <fresh-output-name> saved-trigger-conversion
```

Accepted output is under
GAME_DIR/captures/rust-dos-load-entry/v181-trigger-conversion-fixed-20261106/.
The earlier v181-trigger-conversion-20261106 output in the same parent is
the stopped tooling attempt. Inspect resource_requests[2]'s source row,
payload and three binding samples; script_trace.trigger_transfer_input,
prediction, before_release and reader_return; reader_return.targets and
AL; and map_request's final comparison. Require each relevant equality,
including legacy non-gating comparison fields, not only top-level passed.
Reports, captures, private images and source bytes remain local, out of Git.

---
id: FND-COMBAT-148
title: One native reconstruction reads GMAP from the newly opened region archive and matches the saved-bit-plane map prediction at reader return
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and map_restore.py at bec0feb7; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed native-save probe repeated
FND-COMBAT-140/141/142/143/144/145/146's admitted route through the two
target SAVE transfers and matched selector-zero reader return. It added
the region-open, GMAP request and map-comparison observations below, using
FND-COMBAT-147's static reconstruction contract and FMT-REGION-003's grid.
No setup memory writes or additional input were introduced.

### Region publication and actual GMAP request

After the two admitted target transfers, the probe observed resident
38FF:0066 entry for the terminated rooted path C:\RGN032.GFF, thirteen
characters. The wrapper return/call checks matched, with option 10 decimal,
reserve 1236 and output address DS:145A. The matching I/O-open return held
handle 12; the successful archive epilogue published a new active/head
record with +28 archive handle 15 and +4 I/O handle 12. The saved region
handle was 15, working handle remained 14, and issuance counter was 16.

The actual GMAP/50 request entered resident 38FF:07B1 through 04AB's return
+04D0, start zero and complete-length sentinel. Its in/out slot was DS:0538.
The enclosing wrapper return and loaded call instruction matched map-helper
+1D30/+1D2B, admitting this request's reconstruction caller rather than
inferring it from the resource tag alone.

At the core's pre-preparation selection stop, the selected provider and
both handle fields equalled that observed region publication. Its selected
plain GMAP table held one entry, index zero, resource number 50. The row
was bounded within the provider's directory and matched the corresponding
twelve-byte row in the fixed private RGN032.GFF under unchanged whole-file
identity. Numeric directory/resource metadata, all decimal, was:

| Field | Observed value |
| --- | ---: |
| Directory file position | 63393 |
| Directory length | 1248 |
| Selected row file position | 63431 |
| GMAP payload file offset | 43913 |
| Recorded and selected payload length | 12544 |

At the same core frame's successful return, AX was zero and the reported
completed count was 12544. The destination's entire 12544-byte span equalled
the selected span in the unchanged private source file. This is a direct
byte comparison, not an inference from the returned count. The current
map pointer DS:0538 equalled the selected transfer destination.

### Actual spans and predicted map endpoint

While stopped at GMAP's matched core return, the probe read the two named
bitmap planes through DS:55B8 and DS:55B4, 1568 bytes each. It validated
each complete span against segment/real-mode wrapping and read in chunks
of at most 261 bytes. Ten pairwise physical-range comparisons across the
map, both planes and the two admitted effect spans found no overlap. The
second plane began exactly 1568 bytes after the first; their adjacency
was not classified as overlap. This is a sampled range result, not allocator
capacity or lifetime admission.

In probe memory it computed the expected map from the just-transferred
bytes and those two cached planes: bit t = 98*x + y supplies masks 40/80
hexadecimal at map byte 128*y + x, preserving that byte's other bits.
No guest byte was changed by the probe. Original-derived bytes remained
only in the owned process and local debugger evidence.

At the matched full reader return, every one of the 12544 sampled map
bytes equalled that prediction; the comparison reported zero different
bytes. The final map digest differed from the initial GMAP-transfer digest,
so this was not merely an unchanged-map comparison. The map pointer still
equalled the transfer destination. This does not independently trace each
leaf setter or show which later child may have written the same values.

The first-plane pointer field was null at reader return; the second-plane
field retained its earlier value. The probe did not dereference either
plane at that later boundary or infer liveness from the retained value.
Both effect spans and source-slot bindings again matched their transfer-time
references, as in FND-COMBAT-146. Reader AL was one.

At return, region handle was zero and active/head equalled the working
record with handle 14. The issuance counter remained 16. The observed
region publication supplies a concrete new record/handle alongside the
counter change previously left unattributed. No continuous close/reuse or
all-opens trace was collected, and closure's result was not independently
observed by this stage.

The accepted run used 138 loop continuations, recorded no RNG draw and
no reader-return relocation, and stopped before the enclosing caller
consumed AL. The owned muted session closed. No post-load checkpoint was made.

## Interpretation

This removes the unobserved GMAP source/length and predicted map-result
case for this fixed native route. The actual region-open publication,
selected row, full GMAP transfer, sampled span separation and map endpoint
now have direct observations. It also demonstrates a temporary selected
region record distinct from the saved working record; equal working-record
endpoints must not be treated as an unchanged archive history.

The observations do not prove continuous nonaliasing, backing capacity,
record/handle lifetime, complete graph/directory identity, all fallback or
failure paths, individual child attribution or completed loading. Equal
prediction and endpoint do not exclude intervening writes. The retained
second-plane pointer field is not evidence of valid storage after cleanup.
Other map states, null/skipped/failed GMAP paths, later caller work and
other reconstruction children remain under Q-COMBAT-066; general backing
and alias questions remain Q-COMBAT-065, with shared archive history under
Q-COMBAT-054/064. No queue item, parity validation or complete reading closes.

## Alternatives

- The observed reconstruction necessarily reads GMAP from the working
  archive: this request selected the newly published region record instead.
- Matching effect endpoints excludes intervening archive selection:
  the region record was active/head at publication and GMAP selection,
  with the working record observed again at reader return.
- The map endpoint is unrelated to the saved planes: all bytes matched
  the independently computed prediction in this admitted case. Other
  inputs and intermediate mutations remain unobserved.
- A retained interior-plane pointer establishes post-cleanup validity:
  only the field value was sampled; its storage was not read at return.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q066-map-request-20261104 saved-map-request`.
Use a fresh output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q066-map-request-20261104/.
The region_open, map_request and reader_return report records retain named
arguments, publications, row/file identity, transfer and prediction comparisons,
span overlaps and pointer-field endpoints. Raw bytes are not committed.
Synthetic controls cover caller/slot rejection, region publication, bit order,
bounded/short reads and physical aliases; they establish tooling behavior,
not additional native inputs or complete reconstruction parity.

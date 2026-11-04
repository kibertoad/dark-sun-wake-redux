---
id: FND-COMBAT-145
title: One working-file open publishes the record and plain SAVE rows later selected by the two target requests
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: dynamic
locations: []
tool: tools/research/rust-dos-runs/load_entry.py and archive_origin.py at ad047ec8; dinorefurb-dosbox-session 1.0.0; Python 3.12.10
environment: Windows; rust-dos v1.8.1 executable SHA-256 e880ecf0d76dbb2f77e4d1d7568d90cb53f3d1cf798679cd24f025955bf85da0; deterministic 15000 cycles; SB16 base 220 IRQ 5 DMA 1; no Ultrasound; muted host output
---

## Observation

On 2026-11-04 the fixed native-source probe repeated the admitted route
through F2/Enter, successful copying, region preparation and selector-zero
reader entry. It extended FND-COMBAT-143/144's target selection and payload
checks with the working-open publication and bounded plain-row observations
described here. No setup writes or further input were introduced.

After copy return, the probe stopped at resident archive-open entry
38FF:0066 for the terminated rooted path C:\darkrun.GFF, fourteen characters.
Its far return was overlay 182 +0042; the loaded +003D instruction matched
the declared call to that resident opener. Arguments were option two,
reserve 1236 decimal and output address DS:144E. These match
FND-COMBAT-110/127's wrapper and open contract.

At the same opener frame's +0096, immediately after its I/O-open call,
AX held handle 11 decimal. At +02B1, before the success epilogue popped
registers, AX was zero. The constructed record's pointer equalled both
active and head pointers; its +4 I/O handle was 11 and +28 archive handle
was 14, equal to the saved working output. The probe retained the entry
and return stops and original SS/BP checks. This observes the open entry
path, I/O result and record publication, not an independent DOS handle-table
or host-path inspection.

At both target core selection stops, the selected record pointer, archive
handle 14 and I/O handle 11 equalled that observed publication. The probe
read the provider's directory position/length, selected type/row pointers
from the core locals, SAVE type header and selected twelve-byte plain row.
It required the type header and complete plain table to fit within the
provider's contiguous directory, with an aligned selected row index below
the table count. No graph or full-table traversal was performed.

The directory position was 48947 decimal and length 792; the selected
plain SAVE table held 61 entries. The selected metadata was:

| Resource | Zero-based row index | Corresponding file-row offset | Payload offset | Payload length |
| --- | ---: | ---: | ---: | ---: |
| SAVE/24 | 23 | 49261 | 39990 | 1920 |
| SAVE/26 | 25 | 49285 | 42162 | 2 |

For each row, number, offset and length matched the selected core locals.
The row's twelve bytes equalled its corresponding span in the fixed private
working file, whose whole-file identity still matched the native copy at
both sides of that file read. FMT-GFF-001/002/003 describe the file layout;
FND-CONFIG-038 and FND-COMBAT-127 describe lookup and directory placement.
Only these two selected rows were compared, not the full directory.

The experiment also repeated FND-COMBAT-144's matched core returns with
zero AX/error and full reported counts, and byte equality of both destination
spans with the selected private-file payload spans. It ended at SAVE/26's
core return after 94 loop continuations, recording no RNG draw. Its owned
muted session closed; no post-load state was saved.

## Interpretation

This removes the unobserved working-open publication and selected plain-row
binding for this one native-save route. The target record/handle pair and
row metadata are connected by observed endpoints to the admitted working
open and copied file, beyond inferring source identity from equal payload
bytes or the initial active pointer alone.

No continuous close/reopen, alias or record-lifetime trace was collected.
Matching record pointers/handles do not independently exclude intervening
reuse. No direct DOS handle-table inspection, full-directory identity,
complete archive graph or absence of fallback traversal is established.
The fixed path at open entry is not a universal path-resolution contract.
Other saves, missing/indexed/fallback providers, changed directory lengths,
destination lifetimes and later restoration remain open under
Q-COMBAT-064/065/066, with shared graph/handle producers under Q-COMBAT-054.
No complete-reading or parity status changes follow.

## Alternatives

- The two selected records must come from an unobserved archive publication:
  their pointer and both handle fields matched the observed working-open
  publication in this run. Continuous lifetime remains unproved.
- The native selected row may disagree with the copied file even when the
  payload bytes match: both selected row metadata and the corresponding
  twelve-byte private-file rows were compared and equal here.
- Matching two rows establishes the entire archive directory or all sources:
  no other row, indexed path or alternative save was inspected by these guards.

## How to reproduce

Prepare and independently verify FND-COMBAT-140's native candidate. Run
the project-local evidence Python with `-I` and
`tools/research/rust-dos-runs/load_entry.py v181-q063-native-save-input-20261104 v181-q064-origin-20261104 saved-origin`.
Use a fresh output name on repetition. Reports, journals and private files
remain under
GAME_DIR/captures/rust-dos-load-entry/v181-q064-origin-20261104/.
The working_open record contains the admitted entry, I/O result and
publication; each resource_requests item contains selected directory metadata,
row equality, matched transfer result and bounded payload comparison.
Synthetic controls exercise open/result/frame rejection, provider mismatch,
indexed and out-of-directory tables, misaligned rows and unequal file rows;
they establish tooling behavior rather than native parity.

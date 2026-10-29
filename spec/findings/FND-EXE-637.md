---
id: FND-EXE-637
title: Canonical ownership and decode declarations reproduce a bounded repaired overlay view
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065E7A..0x00065F07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066A2C..0x00066B11
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006B749..0x0006B7A1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070607..0x0007060A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C647..0x0008C7CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C834..0x0008C9AF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DDE9..0x0008DFC4
tool: Ghidra 12.1.3; Temurin JDK 21.0.12.1; canonical reconciliation scripts at 55a86ab8 with the declared-entry and declared-flow metadata accompanying this finding; inventory_tails.py at 106df5d0
environment: null
---

## Observation

FND-EXE-636 identifies three adjacent internal blocks and three interior
decode gaps in the full-model control of FND-EXE-635. This reading checks
the two remaining extra starts and tests the resulting canonical declarations.

### The two extra starts are internal conditional blocks

Both 0x00065EB2 and 0x00065EF1 lie inside the frame established at
0x00065653. The first follows a comparison of a frame local; the unsigned
conditional branch at 0x00065EAD enters it. It reads the existing frame's
parameters, performs a call and its argument cleanup, then jumps to the
shared continuation at 0x00066A32. Its exclusive end is 0x00065ECE.

The second follows a call whose result is stored in that frame. The
conditional at 0x00065EEC enters it when the result differs from its sentinel.
It uses existing frame locals and jumps to the same shared continuation.
Its exclusive end is 0x00065F07. Neither block establishes a new frame or
returns as an independent routine. These observations resolve the two
additional starts in the full-model comparison as parent-body ownership,
without claiming that indirect internal entry labels are impossible.

### Independent control and canonical declarations agree

A disposable copy of the frozen full-model snapshot applies five removal
declarations, retaining their bytes under these owners:

| Removed start | Owner | Cleared and redecoded range |
|---|---|---|
| 0x00065EB2 | 0x00065653 | 0x00065EB2..0x00065ECE |
| 0x00065EF1 | 0x00065653 | 0x00065EF1..0x00065F07 |
| 0x00066A2C | 0x00065653 | 0x00066A2C..0x00066B11 |
| 0x0006B749 | 0x0006B581 | 0x0006B749..0x0006B7A1 |
| 0x00070607 | 0x0007054D | 0x00070607..0x0007060A |

The three interior decode declarations are the ranges and owners in
FND-EXE-636, each carrying that finding as evidence. The three tail decode
declarations of FND-EXE-633 remain in place. A second disposable project
uses the actual repository declaration files, rather than the independently
prepared local configuration. Its inventory TSV is byte-identical to the
first control. Each run's repeated inventory, provenance and region exports
also agree byte for byte.

The canonical pass reports zero entry failures, zero flow conflicts and
zero entries without instructions. Both near-transfer passes report no
wrong or outside targets and no new callees. The fill pass adds no bytes;
it retains two held resident-view candidates at 4842:071A and 4842:077F,
both associated with 4420:0008. Those resident candidates are outside this
overlay export, not evidence that the resident join is ready. Clipping
examines 874 overlay functions and removes no bytes.

The bounded final-range census reports 874 rows: 866 terminal returns and
eight unconditional jumps, with no adjacent, unowned, code-end, other-body
fall-through or incomplete final-range decode.

### Every overlay range difference from the committed view is bounded

There are no new starts. Three committed starts disappear into their parents:
0x00066A2C, 0x0006B749 and 0x00070607. Their parents gain respectively
229, 88 and three bytes, preserving the union of those existing bodies.
The two extra starts above existed only in the regenerated control, not in
the committed inventory.

The only other changed rows are the three tails from FND-EXE-633:
0x0008DDE9 gains 36 bytes, 0x0008DE45 gains six and 0x0008DF0E gains 175.
Their exclusive ends are 0x0008DE17, 0x0008DF0E and 0x0008DFC4.
The two interior-gap owners again match their committed ranges exactly.
All other overlay starts and complete body ranges match the committed view.
Thus the candidate changes 877 rows to 874 and adds 217 bytes to the union
of overlay bodies; regrouping internal blocks adds no union bytes.

## Interpretation

The declaration repair is reproducible through the supported static pipeline,
and its complete overlay range delta is explained. The canonical files now
persist both ownership corrections and the evidenced interior decodes.

This completes the overlay-candidate repair, not Q-EXE-032's accepted joined
inventory. The historical resident export is absent. A resident view and
the join/staging checks remain necessary before replacing the committed
inventory and its provenance/regions or updating closure-plan hashes.
No inventory row was edited by hand, no gameplay was implemented, and no
complete-reading or evidence-status promotion follows from these checks.

## Alternatives

- Keeping the two extra starts as separate callable routines conflicts with
  their direct conditional predecessors, existing-frame accesses and shared
  parent continuation. Internal alternate entry labels remain outside this
  bounded ownership conclusion.
- Counting the three adjacent-block merges as newly discovered code would
  double-count already inventoried bytes. Only the three truncated tails
  add bytes to the committed overlay union.
- Accepting the successful overlay export as a successful joined inventory
  omits the resident input and required join validation; that remains open.

## How to reproduce

Use the source identities, mapped-image preparation and frozen full-model
snapshot in FND-EXE-635. The installed source is size 634416, XXH3
e296af55ba2ecde7e77f555c90f33d0b; mapped image XXH3 is
f48148049b7bd26475845bfcbeb2d11e and MD5 is
c1cf710e754178e4fe5fb5aa0a56fafa. No game process is started.

Inspect overlay descriptor 179 with overlay_listing.py for
0x00065E7A..0x00065F07. Follow the direct conditional targets and shared
continuation, rather than interpreting embedded switch data as instructions.

Copy the saved INST-overlay-20261029c static project to an unused project.
With analysis disabled, follow docs/EVIDENCE-TOOLS.md: CorrectNearTransfers
with apply, 55E8:0000, 229, mapping JSON and shipped source; declared entries
in overlay view; declared flows in overlay view; CorrectNearTransfers again;
FillInstructionBodies; clipping to all 49 mapped overlay code regions;
ReportEntriesWithoutInstruction; two ExportResearchBaseline exports with
the same initialized regions and mapped-image identities. Exporter SHA-256
is fd88d69d04118cfbfecf48f03b6768e8a0cc9dbf795a2b3786c5e6b3baa1adba.

The independent control is retained at
GAME_DIR/analysis/inventory-reconcile-20261029/runs/ownership-control-01/,
with exact configuration and arguments. The canonical run is in
runs/ownership-canonical-02/ under that root, with exact argument metadata,
copies and hashes of both declarations, headless log, finished record,
repeated exports and tails.json; its saved project is INST-overlay-20261029g.
The earlier ownership-canonical-01 attempt reused existing export paths,
which the exporter rejected. Its process exit alone was not accepted as
success; its diagnostics remain local.

Run inventory_tails.py SOURCE overlay-a.tsv --view mapped on the successful
canonical export, using the locked Python/decoder identities of FND-EXE-636.
For range comparison, convert each mapped address to a shipped-file offset
using mapping.json (equivalently header 0x5200 plus load-segment displacement
from 0x1000 plus offset), retain every sparse range, and compare starts and
ranges against the overlay rows of coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv.
Do not compare sizes as if every body were contiguous.

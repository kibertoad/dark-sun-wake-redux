---
id: FND-SCRIPT-059
title: The inventoried save-pointer-table operands separate preparation stores from indexed transfer reads
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070BFD
tool: window_operands.py and overlay_listing.py at b5cf13bd; installed function inventory; Ghidra 12.1.3 ReportDecompileMatches.java in INST-overlay-20261029g
environment: null
---

## Observation

This tests the explicit-displacement access mechanism for the prepared
save-pointer table relevant to Q-SCRIPT-019, reading its preparation and
reader together. Source DSUN.EXE is 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The installed inventory's LF-normalized
SHA-256 is 820b317d56cb74114699a8e295b0e93e961900a4b2d7d493fb2240b4daa6afa2.

The 157E..1672 window covers all 61 four-byte source slots. The existing
operand reporter emits 27 operands in inventoried routines and 182 raw
candidates outside their instruction starts. The listed operands divide as:

| Mechanism | Count | Established owner |
| --- | ---: | --- |
| Indexed table reads | 5 | Overlay 187 writer +08E3 |
| Indexed table reads | 6 | Overlay 187 reader +0A6C |
| Direct four-byte table stores | 15 | Overlay 187 preparation +0D1A |
| CS-relative indexed jump-table read | 1 | Overlay 208 +1258 |

FND-SAVE-021's independent preparation stores and FND-COMBAT-034/035's
writer/reader accesses are positive controls. The CS-relative operand's
encoded displacement 1602 is not a DS save-table access. The known table
routines have no string-store instructions in the reporter's counts.
This does not count string stores in unrelated routines as excluded writers.

Preparation fills indices 4..9 and 29..37, as FND-SAVE-021 records. The
last store at +0DC7 writes all four bytes of SAVE/38's slot, DS:1612,
from the live pool pointer, with no child call in preparation. No other
listed explicit-displacement store in this window overlaps that slot.
The raw word write at 4888:0002 to displacement 1613 is inside a listed
instruction, not a second established writer. The raw +0DC8 word-store
variant starts inside the known operand-size-prefixed four-byte store.
The raw byte write at 4B14:0007 to 1611 is also interior and, even as
decoded, ends before the slot. These classifications do not establish
alternative reachable entries. Other raw candidates remain in the report.

### Connected reader use

The current overlay Ghidra view contains both reader +0A6C and preparation
+0D1A, and the bounded decompilation agrees with their instruction sequence.
Its source alias 5B7C:0700 denotes the same physical live-pool field as
57E0:40C0; the instructions establish the latter segment and four-byte width.

The reader calls preparation once before its resource loop. On a reached
iteration it first reads both slot words and compares them with DS:67B7's
pointer. Equality requests the 12000-byte zero fill already identified in
FND-COMBAT-035. After that optional call it reloads the slot for the null
test and then reloads it again to seed its local in/out destination.
Thus the optional call is followed by fresh source-table reads; this is
not one pointer value frozen across the fill. Neither the comparison nor
the request names SAVE/38 specially.

For selector zero and index 37, the computed resource number is 38.
Provided earlier iterations continue, the nonnull slot supplies the
destination to the complete-resource reader. A null slot skips that request
without changing the local success byte. A nonzero returned AX stops later
iterations. There is no local copy-back from the destination in/out slot
to the table or live pool pointer (FND-COMBAT-035). FND-CONFIG-151 supplies
the complete-resource service's retained selection, capacity and I/O limits.

## Interpretation

The explicit-displacement census and connected reading remove an unspecified
second inventoried direct table writer from this mechanism's remaining
questions. Preparation, indexed transfer reads and the unrelated CS access
have concrete owners. This does not prove preservation across calls or
between phases. Computed segment/offset accesses, indexed writes whose
displacements fall outside the window, bulk destinations, raw candidates
without established entries and transfer-buffer aliases remain excluded.
Actual child effects, source-table lifetime and SAVE/38 transfer/live-buffer
identity remain Q-SCRIPT-019, alongside FND-SCRIPT-058's sampled native result.
No complete-reading declaration or queue closure follows.

## Alternatives

Treating every matching displacement as a save-table access would include
the CS jump table incorrectly. Treating a prefix-interior decode as another
writer would duplicate preparation. Treating one preparation copy as a
pointer frozen for the whole loop contradicts the later reloads; actual
changes during those intervals are not established by this observation.

## How to reproduce

Run window_operands.py on the supported executable and
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv with window 157E..1672. Require the
independent store/read controls and retain both listed and raw outputs.
The retained report is trigger-prepared-save-table-window.txt under
GAME_DIR/analysis/handler-census-20261106/.

Read overlay_listing.py <executable> 187 7089C 70BFD; retain
trigger-save-reader-connected.txt in the same local directory. In the
read-only INST-overlay-20261029g project's INST-MAPPED.EXE, use
ReportDecompileMatches.java at 7AC3:0A6C with patterns 157e, 1580, 0d1a,
1612 and at 7AC3:0D1A with 1612, 0700, 40c0. Successful function markers
and matching instructions, not the process exit alone, qualify the view.
The local log is trigger-save-table-connected-decompile.log. No original
run or emulated call was made for this finding.

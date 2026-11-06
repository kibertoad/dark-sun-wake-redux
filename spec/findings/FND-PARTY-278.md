---
id: FND-PARTY-278
title: The nine nearby interior-pointer pairs are unrelated operands or descriptor fields, and a truncated resident inventory body gains its continuation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067732..0x0006773E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A9BA..0x0008A9C7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059BB0..0x00059BBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059A2F..0x00059A3E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005A001..0x0005A00F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00059FDC..0x00059FEB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3F96:070E..3F96:0755
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:00E5..3EBE:0101
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004B218..0x0004B228
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py and window_operands.py, tools/evidence/report.mjs table at d79a7c2a; Ghidra 12.1.3 ReportInstructionWindow.java from scientific-method-engine 19.10.0; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-264/277 and FMT-EXE-002 reuse
environment: null
---

## Observation

Q-PARTY-124 retains the nine nearby-word pairs in FND-PARTY-277's
controlled census of resident interior targets. The unchanged source is
installed DSUN.EXE, 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`. All nine word producers are read
here; none is promoted to a pointer merely because the words match.

### Pair classifications

| Segment-word file site | Matching nearby word | Actual producer relationship |
|---|---|---|
| 6773C | 0133 at -7 | The segment word belongs to overlay 180 +0559's call of 39D1:064A. The matching bytes cross the address/immediate fields of +0552's byte store of one to DS:3330. |
| 8A9BD | 0133 at +8 | The segment word belongs to overlay 202 +051A's call of 39D1:0430. The matching bytes cross the address/immediate fields of +0522's byte store of one to DS:33BA. |
| 59BB3 | 0300 at +6 | Overlay 172 +0780 calls 39D1:0430; +0788 separately loads the segment literal naming 4C4D. |
| 59A32 | 0300 at +10 | Overlay 172 +05FF calls 39D1:097D; +060B separately loads that same segment literal. |
| 5A004 | 0300 at +6 | Overlay 172 +0BD1 calls 39D1:071F; +0BD9 separately loads that segment literal. |
| 59FDF | 0300 at +10 | Overlay 172 +0BAC calls 39D1:097D; +0BB8 separately loads that segment literal. |
| 35297 | 0B0A at +4 | The segment word belongs to 3F96:0734's call of 39D1:049C. The matching word crosses the final immediate byte of ADD SP at 0739 and the following OR opcode at 073C. |
| 33ED0 | 28E9 at +12 | The segment word belongs to 3EBE:00ED's call of 3CFA:0006. The matching bytes are the near-JMP opcode and first displacement byte at 00FC, not that call's offset. |
| 4B220 | 18D9 at -6 | The segment field of descriptor 52 is paired with descriptor 51's end_offset, not an offset field of the same record. |

The four 0300 words are themselves fixup-marked segment operands.
The overlay reader resolves each to descriptor 96, segment 4C4D;
they are not runtime offset 0300 operands. The calls retain their own
different encoded target offsets. No listed code producer assembles
the candidate interior address from the matching words.

The fingerprint-guarded two-record table query confirms descriptor 51
has end_offset 18D9, while descriptor 52 has relative segment 3228,
loaded as 4228. FMT-EXE-002 already identifies their separate fields
and meanings. Arbitrary later reinterpretation or copying of descriptor
bytes is not excluded by that producer classification.

The read-only Ghidra containing-instruction diagnostics agree with the
six overlay code pairs and the 3EBE pair. At the 3F96 pair the saved
listing instead decodes across the actual far call incorrectly. The
source read from inventoried entry 3F96:070E establishes the APFM
argument pushes, far call at 0734, stack adjustment at 0739 and OR
at 073C. Its result takes either the FFFF return branch or the two
stores at DS:A1AD/A1AF, then returns at 0754. The positive aligned
39D1:0088 control is retained; no negative Ghidra proof is used.

### Metadata-only inventory repair

The prior inventory row for 3F96:070E has size 38 and ends at 0734,
immediately before the actual far call. The connected source path owns
070E..0755, size 71. No other committed function range owns the
33-byte continuation 0734..0755. The next inventoried entry is 0755.
Repair this row's range and byte count without adding a function start,
analyzer name, source code or machine bytes to the inventory.

The installed DSUN inventory still has 3230 function rows. Its
LF-normalized SHA-256 changes from
`69555e580c3964e90ec43822629c29e3b909ecc264353b4dec0ccc82bdbf76ea`
to `baac52b88e23a72661cfc2caaf4c19c8c08b1cdce2c23d04e3d2ad402f553477`.
Refresh the two current ownership-map hashes; historical findings remain
tied to their stated revisions. Function ownership assignments and the
6376-function denominator do not change. The current map still leaves
unassigned functions visible and adds no accepted complete reading.

Rerunning the entire FND-PARTY-275 writer operand report with the repaired
inventory leaves its lines and candidate classifications unchanged. The
new continuation's own stores are outside that report's selected window.
Thus this repair does not silently change the 27 remaining inventory
value matches or the 34 classified raw interiors.

The linked region accounting is measured separately with the existing
ExportResearchBaseline script against the original-source resident view
INST-single-20261103b. The narrow 0734..0755 region has 31 decoded
instruction bytes and two undefined bytes in that saved view, with no
defined data. Move those counts from outside to inside function bodies
in the 3F96 region sidecar, preserving the analyzer's classifications
and the region's total size. Summed and unique inventory body bytes
become 484563, both 33 higher; provenance names this source-backed
manual repair rather than pretending a new whole-view export occurred.
The two analyzer-undefined bytes remain visible, not promoted to a
complete reading by assigning an owner.

## Interpretation

Remove the nine inspected pair producers from the possible nearby
relocated/fixed-up interior-pointer constructions. Together with
FND-PARTY-277, the tested direct-call and nearby-pair forms to these
interior targets are accounted for. Retain jumps, unrelocated/computed,
copied or remotely assembled targets and runtime overlay bindings;
this is not a whole-program no-interior-entry result.

The metadata repair maps 33 previously unassigned body bytes without
claiming complete callers, guard behavior, lookup success or runtime
state for the helper. No whole getter site closes: the 83 other direct
sites, 27 remaining writer value matches and repeated-hook preservation
remain. FMT-PARTY-007 stays supported; no queue item closes or complete
reading is declared.

## Alternatives

- Equal nearby words can come from independent operands, instruction
  boundaries or adjacent metadata records rather than a far pointer.
- A source-verified continuation can repair function ownership even
  when the saved analyzer listing is wrong; that repair alone proves
  neither input admission nor a complete-reading contract.
- No producer in these nine sites constructs the candidate pointer;
  unsearched later copies, arithmetic and transfer forms remain open.

## How to reproduce

Reuse FND-PARTY-277's pointer census and controls. Read both words at
each listed pair through the bounded source readers, retaining the
actual instruction boundaries, fixup meanings and separate fields.
Use descriptor 180 at 67732..6773E; descriptor 202 at
8A9BA..8A9C7; descriptor 172 at 59BB0..59BBE,
59A2F..59A3E, 5A001..5A00F and 59FDC..59FEB.
Use resident ranges 3F96:070E..0755 and 3EBE:00E5..0101.
Bounds are hexadecimal and half-open.

Run report.mjs table with the source hash, start 4B218 hex, count two,
stride eight and unsigned word fields at offsets zero, two, four and
six. Inspect the 16 code-word positions with ReportInstructionWindow.java
in the saved read-only project, preserving the conflicting 3F96 decode
instead of accepting it. Check the inventory for any overlapping owner
before extending 070E to its actual return and rerun the retained writer
census with the new inventory hash.

For region accounting, run ExportResearchBaseline.java read-only with
the original-source resident project INST-single-20261103b, program
DSUN.EXE, verified source MD5 90b09ee26f2bc3a8d20063712636241c,
the installed XXH3 above and the single region 3F96:0734..0755.
Retain its BASELINE_COMPLETE marker and 31/0/2 region partition.
That narrow export selects no function entry; it measures the bytes,
not a zero-function claim for the containing routine. Reassess the
current ownership maps after updating the inventory and both sidecars.

Reports under GAME_DIR/analysis/class-table-closure-20261106/ have
nearby-pair-word-boundary, nearby-pair-overlay180,
nearby-pair-overlay202, nearby-pair172, nearby-pair-resident-anchor070E,
nearby-pair-resident-contexts, nearby-pair-descriptor and
wrapped-alias-operands-inventory-fixed stems. No original game ran.

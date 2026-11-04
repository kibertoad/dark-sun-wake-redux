---
id: FND-ITEM-033
title: Same-offset stores target another segment while numbered-save loading requests the effect table independently of insertion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000577F3..0x000577FD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057A6D..0x00057A77
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070964
tool: tools/research/exec-census/operand_range.py and overlay_listing.py at d0865351; locked evidence Python and Capstone
environment: null
---

## Observation

Q-ITEM-013 needs effect-code producers beyond FND-ITEM-032's conditional
insertion path. The retained current-inventory operand report locates
accesses in the effect row's field-offset interval. It is a candidate
locator, not an exhaustive writer census: register-derived destinations,
copies, unmodeled instructions and code outside its inventory ranges remain.

### Two same-offset candidates are another field

At overlay 169 +0278 and +04F2, word stores use ES:0253. Each immediately
preceding segment setup loads ES from a fixup naming descriptor 93,
mapped segment 4C13. Neither addresses the effect-table code field at
4E71:0253. The inspected literal byte store at overlay 193 +175A does
use the latter segment with an indexed row, as FND-ITEM-032 records.
These two word stores therefore cannot be counted as effect-code writers
merely because their displacements match. Physical aliases or unrelated
destinations are not inferred from offset equality.

### The numbered resource request is an independent producer path

FND-COMBAT-034 and FND-SAVE-015 identify pointer-table index 23 as
4E71:024D and index 25 as the separate effect-count word DS:2100.
Their shipped save-source lengths are 1920 and two bytes respectively.
Those lengths describe the writer's table, not a read-time code bound.

Overlay 187 +0A6C selects 61 iterations for selector zero and 17 for
other selectors. Its loop reads the far pointer at DS:157E plus four
times the index. A null entry is skipped. A nonnull entry is copied
into a local far pointer and its address is passed to 38FF:04AB with
tag SAVE and number 61 times selector plus index plus one. Thus on the
selector-zero route, reaching index 23 with the preserved table submits
SAVE/24 and the effect-table destination. Reaching index 25 separately
submits SAVE/26 and the count destination.

The loop tests AX after each request. Nonzero clears its continuation
flag and prevents the next iteration; zero permits continuation. Before
these requests it neither iterates individual effect rows nor checks
their code bytes. It also does not pass the save writer's 1920-byte
length as a destination capacity to this request.

FND-CONFIG-151 establishes that 38FF:04AB requests start zero and the
complete-resource count sentinel through 38FF:07B1. That wrapper's
range check is not a destination-capacity check. Its selected-resource,
buffer, I/O and failure contracts remain the qualifications recorded
there; this finding does not assume a successful transfer or unchanged
table entries. The restore request therefore cannot inherit the
signed-code insertion gate merely because both paths address effect rows.

## Interpretation

Two inspected literal store candidates are removed from effect-code
writer admission by explicit segment provenance. The loader's destination,
resource selection and result gate are locally accounted for. Remaining
Q-ITEM-013 work includes restored resource contents/lengths and backing,
other writers and copies, count/owner/index consistency, and frame aliases.
An intact API-created table can be a source of a later save, but that alone
does not prove all admitted loads came from that state or retain its code
bound. No native defect, successful restore, complete reading or rule
confidence promotion follows.

## Alternatives

- A matching displacement is not a matching segmented destination.
- The save writer's shipped length is not a capacity argument to the reader.
- Separate resources for rows and count require a consistency argument;
  visiting both table entries alone does not supply it.
- A request for a whole resource does not prove transfer success or native
  admission of every possible resource value.

## How to reproduce

Use installed DSUN.EXE through tools/game-dir.mjs with locked evidence
Python; the readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run operand_range.py with the inventory
at d0865351 and hexadecimal bounds 24D 257. Keep its full output, including
reads, unrelated segments and syntactic candidates. FND-ITEM-032's known
store at 7FA4A is the positive literal-code-store control in this output.
No empty-search or universal absence claim is made.

Run overlay_listing.py for descriptor 169 with ranges 577F3 577FD and
57A6D 57A77, then descriptor 187 with range 7089C 70964. Reuse the
source-table and archive-wrapper evidence cited above rather than inferring
pointer identity or read capacity from neighboring fields.

Reports remain only in GAME_DIR/analysis/hit-incoming-20261104/:
effect-row-field-operands.txt, code-field-candidate-169-first.txt,
code-field-candidate-169-second.txt and effect-save-restore-loop.txt.
Retained listings include wider context; claims use the stated paths.
No original listing, bytes or resource content is committed.

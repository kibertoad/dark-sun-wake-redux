---
id: FND-COMBAT-029
title: Status effect collection filters category zero and effect-name metadata supplies ICON resource numbers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FE93..0x0007FF07
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007FFF0..0x000800A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080140..0x00080171
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007A4BC..0x0007A5A7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008AC6F..0x0008AE70
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00045D00..0x00045D0F
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3143:000D..3143:0061
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
tool: tools/research/exec-census/trampoline_target.py, direct_callers.py, segment_references.py, resident_listing.py and overlay_listing.py; tools/evidence/report.mjs table with installed-build fingerprint and relocation checks
environment: null
---

## Observation

Trampolines 573B:0025 and 573B:002A resolve to overlay 193 +1BA3
and +1D00. The status selector behind 571F:0052 reaches overlay 190
+217C, which calls +2190 with object, ordinal zero and flag one.
That caller prepares a bitmap with +1BA3 and passes object, category
zero, bitmap offset and output offset to +1D00. Its flag-one branch
skips a second category-one collection, clamps the count to 35, and
applies FND-COMBAT-022's stable priority sort before selecting ordinal
zero. This identifies the status panel's list source without changing
the earlier observed sort keys or panel layout.

### Bitmap preparation and collection

Both helpers index ten-byte runtime rows at 4E71:024D. A row's first
word is compared with the requested object and its byte at +6 supplies
the effect ID. DS:2100 supplies the row count. The starting-index helper
3143:000D performs a binary search for the object boundary, assuming
an admitted sorted table/count. A zero count or unsigned object at
least 48 returns the count. It does not establish the table's writers,
sorting invariant, row capacity or native lifetime.

Preparation receives an object and a near bitmap offset. Through the
two resident fill helpers, it clears exactly 15 bytes at DS:bitmap,
not every byte of a caller's reserved 16-byte span. For an unsigned
object below 48 it finds the starting row, then ORs bit effect_id & 7
into bitmap byte effect_id / 8 for each matching row. Here the byte
ID is zero-extended before division. It increments the row index and
continues only while the row owner matches and signed index is below
the stored count. The owner read occurs before that count comparison.

Collection receives four caller-cleaned words: object, category, near
bitmap offset and near output offset. It starts at the same boundary
and visits matching rows in ascending table index. Category FF bypasses
the category test; otherwise the row's ID is classified through +1E50
and must equal the requested category byte. The ID must also have its
bit set in the supplied bitmap. Passing IDs are appended as bytes until
the output count is 113; subsequent rows can still be visited but do
not append. It returns the word count and adds no terminator.

The bitmap is a membership filter, not a deduplicator: each passing
row can append its ID, including repeated IDs. There is no numeric
effect-ID sort in either helper. Preparation of a fresh independent
bitmap from unchanged rows makes those rows' membership bits true;
the category filter and row order still decide the collected list.

Collection and classification sign-extend their effect byte before
division by eight, unlike preparation's unsigned index. IDs 0..112
stay in the ordinary reviewed domain. Bytes 80..FF hexadecimal can
address preceding bitmap/classification storage, so their admission
cannot be inferred from preparation's positive byte index. Both loops
read the owner before checking the count; a one-past boundary read is
not evidence of a valid sentinel or complete table admission.

### Category used by the status panel

Classifier +1E50 tests one bit in the bytes at 50B0:0000 and returns
zero or one. Reading the fifteen shipped bytes needed for IDs 0..112
gives category zero for:

```text
0, 1, 7, 10, 12, 16, 19, 20, 21, 22, 23, 27, 30, 33, 35, 37,
44, 50, 56, 59, 60, 61, 62, 65, 66, 67, 68, 69, 70, 71,
82, 83, 86, 88, 91, 100, 105, 106, 107, 108, 110, 111, 112
```

The other documented IDs are category one. In particular 9 and 93
do not enter the status selector's category-zero list under this
unchanged bitmap; 12 and 88 can enter but have empty names. The later
priority sort does not recover an effect excluded during collection.
The list describes shipped classification bytes, not a completed
writer/alias census or observed native effect state.

As a synthetic ordering example with independent storage and IDs
9, 12, 83, 83 in matching rows, category-zero collection yields
12, 83, 83. The existing stable priority sort puts the two 83 entries
before 12. This checks the composition of the read branches; it is
not an original capture, fixture or admitted shipped encounter.

### Trailing effect-name word

The relocation-aware segment-reference query has the known status-panel
name lookup as a control and recovers a separate consumer in overlay
202. Its exported trampoline 577D:0039 reaches +07CF. That function
prepares a bitmap, collects category zero then category one, and indexes
the same 31-byte effect-name records as FMT-COMBAT-003.

At +08AC it tests the word at record offset 1D for zero. Zero removes
the effect from that icon list. At +0911 it zero-extends a nonzero
word to 32 bits, pushes the ICON resource tag and calls 38FF:04AB
with a local far-pointer destination. A zero returned destination
skips later processing. The direct consumer therefore establishes the
word as an ICON resource number, with zero meaning no icon request
in this path. It is not a near text pointer, an effect duration or
another effect number. No complete archive lookup or rendered icon
outcome is claimed.

## Interpretation

Q-COMBAT-008's local list/order, preparation and trailing-word questions
are answered. RULE-COMBAT-009's selection operates on category zero,
not all effect rows. The earlier possible-93 example describes the
generic key function but not this status caller with the shipped
classification bytes. FMT-COMBAT-003's unknown word has a direct
ICON-resource consumer and can be named at supported status.

Complete row/count/bitmap/category storage admission, other callers and
the upper effect-ID domain move to Q-COMBAT-009. Complete icon-request
mapping, archive resolution and the other field consumers move to
Q-COMBAT-010. Neither rule nor format is raised to established.

## Alternatives

- A deduplicated or numerically sorted list does not match the collector.
- The 35-element sort clamp is in the caller. The collector's own append
  limit is 113, and a separate caller can request both categories.
- A reserved 16-byte local span is not evidence of a 16-byte clear.
- Category/table writers, aliases, valid IDs, sentinel storage and DS/SS
  identity remain prerequisites. Do not implement arbitrary effect-byte
  behavior or infer an original presentation from the synthetic example.
- No original run or complete-reading declaration is supplied. The
  delivered resident harness cannot execute the overlay helpers.

## How to reproduce

Use the project evidence Python and approved installed DSUN.EXE. Resolve
573B:0025, 573B:002A and 577D:0039 with trampoline_target.py.
Read the listed ranges with overlay_listing.py and resident_listing.py;
start at the exported/helper entries, following the status selector's
flag branch and the ICON consumer's zero test and request arguments.
Run segment_references.py for 4C87, retaining the status-name lookup
as its independently known control; candidates alone prove no absence.

Use report.mjs table with the executable's recorded XXH3, file start
45D00 hexadecimal, count/limit 15, stride one and a width-one flags
field at offset zero. Count evidence is floor(id / 8) for the documented
0..112 ID domain. Test each bit to reproduce the category set above.
Keep configurations and original-derived output in
GAME_DIR/analysis/combat-effects/. Check the synthetic example directly
from the row traversal, category bitmap and existing stable key rule.

---
id: FND-ACTOR-016
title: Kind-five source records carry a negated resource key and their first word selects free logical slots
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00000000..0x00680304
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073224..0x00073251
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000701CA..0x0007042B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2C87..2D40:2E0A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0002540A..0x00025414
tool: Inspector rdff-key-census at 34fbedad; existing GffArchive reader; resident_listing.py, overlay_listing.py and tools/evidence/report.mjs table; locked evidence Python
environment: null
---

## Observation

FND-SCRIPT-034 identifies kind-five field zero as the first word of a
23-byte record. This reading answers that word's shipped resource relationship
and local copy/free-selection roles, not all fields or live states of the table.

### Controlled source census

The committed census verifies the approved OBJEX.GFF source before parsing.
Its explicit first-kind controls match all 1643 RDFF resources: 1233 kind one,
352 kind two and 58 kind five. All first headers have the supported type and
widths. For each of the 58 selected kind-five records, its signed first data
word equals the negation of its full resource identifier. All 58 are negative;
none is zero or positive. There are zero mismatches and zero low-word-only
aliases. Repeated reports are byte-identical.

Every selected first chunk has 23 data bytes and its next complete header is
the end marker. Only RDFF/500 has trailing bytes: 102, matching FND-ACTOR-015.
Its selected word is -500, before that end header; trailing bytes do not supply
the key. Later chains of other kinds are outside this census's validation.

Changing the expected kind-five count to 59 refuses the census. Replacing
the source hash with zero refuses before archive interpretation. Both return
status two and empty standard output, not a partial successful census.

### The copy and getter refer to the same conditional table

The initializer in overlay 188 supplies double words 1 and 460 to 444C:0008
at +0384..0392. It stores returned DX:AX at DS:19D1 and, only when nonzero,
copies that far pointer to DS:6163 at +03A0..03A4. FND-CONFIG-150 gives
kind five's stride word as 23. DS:6163 is the generic getter's base at
DS:614F plus four times five (FND-CONFIG-140).

The allocation wrapper's product-plus-one and header-derived marker contract
is already FND-CONFIG-209. These arguments form product 460 and an outgoing
request of 461; they do not prove initialized or writable storage. The local
record stride and search bound describe twenty logical records, not an
unconditional live capacity guarantee.

FND-ACTOR-015's copy dispatcher reaches overlay 187 +05B0 for kind five.
That block passes count 23 and its payload pointer to 1000:3F5A, with a
destination formed by word arithmetic as DS:19D1 plus 23 times its record
index SI. The common tail writes SI to the source header's word +4 and the
object handle's paired record-number word, then returns. With valid disjoint
header/payload/destination ranges, these direct tail stores do not replace
the copied first word. FND-CONFIG-182 supplies the copy primitive's forward
copy, register-restoration and overlap limits.

### The first word is also tested for a free record

The complete 2D40:2C87 body gates kind minus one as unsigned at most four
and jumps through five words at 2E0A. Their targets are 2D78, 2CAC, 2D1C,
2DDB and 2DB2. The kind-two and kind-three targets agree with
FND-COMBAT-072 and FND-PARTY-259; the fifth target selects kind five.

SI starts at -1. At 2DB2 the kind-five branch initializes DX to zero, then
scans while signed DX is below 20 and SI is still -1. It compares the first
word of DS:19D1 plus 23 times DX with zero. The first equality stores DX
in SI and ends that scan; the branch itself does not reserve the record by
writing a nonzero key. A full first scan without equality retains SI -1.
The later retry helper at 2DEE and its effects remain outside this local
first-scan result. No arbitrary supplied index or helper preservation is assumed.

FND-PARTY-172 and FND-PARTY-218 already identify the kind-five deletion
branch at 2D40:2551: it clears that indexed record's first word. Reusing
that observation connects the zero test with release without rereading or
declaring the whole deletion continuation complete.

## Interpretation

For the shipped kind-five payloads, field zero is a negated RDFF resource
key when those bytes are copied and preserved through the known binding.
Zero is the local free-slot marker tested by selection and written by the
known deletion branch. This answers the remaining source-key meaning in
Q-SCRIPT-002; it does not identify every kind-five object category.

FND-SCRIPT-033/034 answer the other local context-loop and field bindings.
Actual admitted handles, kinds, metadata/width state, pointer validity and
preservation at trigger uses are separately Q-SCRIPT-022, with dependencies
on Q-CONFIG-008. Q-ACTOR-003 retains other 23-byte record fields, while
Q-ACTOR-007 holds the independent kind-four payload/reference question.
No allocation success, native trigger outcome, complete reading or evidence
status promotion follows. No original game was run.

## Alternatives

- A low-word collision would not prove equality to a full identifier. The
  census distinguishes both and finds 58 exact matches, with no aliases only.
- The bytes after RDFF/500's end header are not its first-chunk key source.
- A 460-byte logical request and twenty scanned slots do not prove available
  capacity, zero initialization, successful I/O or an immutable getter base.
- A source key and free-marker binding do not name the remaining 21 bytes,
  prove all writers or establish that every future/restored record has the
  shipped source value. The retry, heap/header, archive and alias conditions
  remain explicit rather than inferred from the matching data.

## How to reproduce

Use OBJEX.GFF size 6816516, XXH3 e08d3cac153c547f6804ca27b155915d,
SHA-256 8e1e2db24df80dd605bb1d55df709cc550cc8b379dc4d2c2c23becaa5975b9a6.
Build the Inspector at 34fbedad and run `rdff-key-census ARCHIVE CONTROLS`.
Controls name that lowercase sourceSha256, kindOne 1233, kindTwo 352 and
kindFive 58. Repeat and compare report bytes. Inspect every selected exact
match flag and the sole trailing-byte row, rather than only the totals.

Accepted Windows report SHA-256:
c3f5e68680b47e93cd2c9d5aac85e9442938140115b6ac0f40cb3a8a4f5ffd52.
Controls SHA-256:
829180dfb98d24f8defd855cfd1e8eac5339d00e6a8e10a66b97afa982257eca.
Repeat with kindFive 59 and with sourceSha256 all zero; require status two,
the appropriate refusal and empty stdout. Synthetic controls separately cover
exact versus low-word-only matches, malformed spans and vacuous controls.

For DSUN.EXE size 634416 and XXH3 e296af55ba2ecde7e77f555c90f33d0b,
retain entry-based listings: overlay 188 file 0x72EA0..0x7325A;
overlay 187 file 0x701CA..0x7042B; resident 2D40:2C87..2D40:2E0A.
Read its jump table with report.mjs table at file 0x2540A, count five,
stride two, one width-two targetOffset at offset zero; countEvidence names
the unsigned gate at 2C9D and earlier kind-two control. Keep table bytes out
of instruction decoding. Follow only the kind-five first scan for this claim.

Configs, repeated reports, refusals, kind-copy-dispatch.txt,
kind-five-copy.txt, kind-table-setup.txt, record-selector.txt and the
record-selector-table config/report remain under
GAME_DIR/analysis/kind-five-key-census-20261103/. No original payload or
instruction listing is committed.

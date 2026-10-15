---
id: FND-COMBAT-037
title: Relocated references to the category segment identify its classifier, neighboring records and metadata
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F0B7..0x0007F409
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F809..0x0007F95F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080140..0x00080171
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004B430..0x0004B438
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E5DE..0x0004E5E2
    kind: file-data
tool: tools/research/exec-census/segment_references.py, direct_callers.py and overlay_listing.py; tools/evidence/report.mjs table; installed-build fingerprint and relocation/fixup checks
environment: null
---

## Observation

The segment_references.py query for mapped resident segment 50B0
returns nineteen overlay-193 fixup sites and two resident MZ data
relocation sites. The independently known classifier operand at
overlay +1E5D is the overlay-fixup control. The save-source pointer
qualified by FND-COMBAT-034 supplies a resident-data relocation
control. Both kinds are recovered, with no output cap.

The query covers segment words in the MZ relocation and FBOV fixup
lists, excluding words whose preceding pattern is a direct far call
or jump. Nearby automatic decodes are candidates only. This finding
qualifies the returned overlay operands from their exported entries
and classifies both data records. It does not make a complete search
claim over unrelocated constants, computed or aliased segment values,
indirect transfers, unrelated bulk buffers or transitive callee effects.

### All nineteen returned overlay sites

The offsets below locate the segment-word operand, not the first byte
of its containing instruction. Entry offsets name actual function
starts in overlay 193. Direct-call/trampoline inspection gives the
following partition:

| Exported entry | Trampoline | Segment-word sites | Local role |
|---|---|---|---|
| +0DC7 | 573B:005C | +0FD9, +0FFB | Read a neighboring record byte, then restore a marked byte. |
| +1054 | 573B:00DE | +106D, +1081, +10A3, +10BC, +10EE, +10FA | Read owner/marked byte, restore that byte, forward it, and compact neighboring records. |
| +1519 | 573B:00CA | +1541, +1552 | Append owner word and byte. |
| +1565 | 573B:006B | +159C, +15A8 | Pass neighboring-record source/destination pointers to the move helper. |
| +15B9 | 573B:00CF | +15C6, +15DA, +1601 | Owner/byte lookup. |
| +1625 | 573B:00E3 | +1636, +164A, +165D | Read owner and transform matching record bytes. |
| +1E50 | 573B:0034 | +1E5D | Category-bit lookup. |

The first six entries use three-byte records based at offset 000F,
with an owner word at record +0 and byte at +2. Their count is
DS:2102. The classifier instead indexes the prefix at offset zero,
as FND-COMBAT-029 reads. The append and matching-owner transform are
already recorded by FND-COMBAT-034; this partition also accounts for
the remaining returned fixup sites.

### Lookup and marking paths

The +15B9 lookup scans signed indices from zero below DS:2102. It
compares the record owner with its owner argument. For a nonnegative
signed record byte it compares that byte directly with the low byte
argument. For a negative byte it compares the word obtained by
sign-extension, negation and subtraction of two with the zero-extended
byte argument. A match returns the index; exhaustion returns FFFF.
For admitted counts 0..84 and stable state, a successful returned index
is therefore 0..83, independent of which byte comparison matched.

The nonnegative branch reuses AX to reload ES, but its last AX writer
before that reload is the explicit segment assignment at +15D9,
not the earlier owner-word read. The intervening byte comparison does
not change AX. Retaining that segment provenance is necessary when
following the later ES read; the automatic context alone is not enough.

Entry +0DC7 first calls +1625 for its owner, then follows object and
geometry data through external/local helpers. On the listed path after
a successful +15B9 lookup, a negative neighboring byte is replaced by
-old_byte-2 modulo 256 before calling +12A8. A failed lookup instead
calls +12A8 and the append entry. Entry +1054 similarly restores a
matching owner's negative byte, calls +12A8, then removes that record
through a move and count decrement. No full object/geometry or +12A8
effect contract is inferred from these call continuations.

These local direct stores address 0011 + 3*i. With admitted unchanged
indices 0..83 they do not address the prefix 0000..000F. This is
conditional separation for those stores, not proof that every caller
supplies such a count or that the called functions preserve it.

### Compaction includes an extra neighboring record

Both +1054 and +1565 request a downward move of 3*(n-i) bytes, from
50B0:0012 + 3*i to 50B0:000F + 3*i, before decrementing DS:2102.
The subtraction is a word operation, sign-extended before the 32-bit
multiply. For admitted 0 <= i < n <= 84, the source includes record n
and the destination ends at the old active end 000F + 3*n. No local
destination starts below 000F in that domain.

For removal of the last active record this is still a three-byte
request. Thus the compaction requires a complete boundary record,
not merely active rows. The calls use 289B:01DC, previously read in
FND-COMBAT-031/032; this finding records the requests and does not
replace their callee contract with an arbitrary-copy model. Calls,
negative or changing counts, and unsupported indices remain admission
dependencies. In +1054, count is re-read after +12A8 and the decrement
is reached even if its preceding copy test fails.

### The two resident references are metadata

File 4B430 is descriptor 118 of the FMT-EXE-002 segment table:
4B080 + 118*8. The bounded record read gives segment 40B0 relative
to the load image, start 0000, exclusive end 010B and flags zero.
In the 1000 mapping this is the resident span 50B0:0000..010B.
The site's relocation is descriptor data, not a reached instruction
writing the category bitmap.

The other data reference at 4E5E0 is the segment word of the source
pointer 50B0:000F already qualified by FND-COMBAT-034. Its offset
word is at 4E5DE. The source's shipped 252-byte extent ends at 010B,
matching the descriptor end after the fifteen-byte category prefix.
This closes the previously unclassified resident data-site identity.

The declared resident span itself contains exactly that prefix plus
84 three-byte records. The compaction source for count 84 includes
three bytes beginning at 010B, outside that descriptor's declared
span. This establishes a boundary-backing obligation, not that those
bytes are unavailable in the running MZ image or that any native
operation fails. The declaration alone cannot supply a sentinel or
runtime storage contract.

## Interpretation

The returned relocation/fixup sites are all accounted for under the
stated query model. The classifier reads its prefix; the qualified
neighboring-record stores/copy destinations stay after it under their
admitted count/index assumptions. The two resident sites are metadata,
not additional code writers. This answers that bounded candidate
classification part of Q-COMBAT-011.

It does not establish a globally immutable classifier. Q-COMBAT-011
retains admitted counts, bulk/aliased or unrelocated writers, source
restoration and callee effects. Q-COMBAT-013 holds the distinct prepared
membership bitmap's content lifetime. No complete-reading declaration or native run is
claimed, and no additional gameplay behavior is implemented.

## Alternatives

- A relocation into the same segment is not automatically an access
  to its first fifteen bytes. Record offsets and index domains matter.
- A descriptor word is not code merely because a context disassembler
  emits instructions around it.
- The lookup's reused ES assignment must follow AX's actual last
  writer. Neither an earlier owner value nor an assumed preserved
  segment may replace that producer.
- The fixed-count descriptor extent and shipped save length do not
  prove the boundary record required by compaction or every restored
  count. No native defect is inferred from the request arithmetic.

## How to reproduce

Use the locked evidence Python and fingerprinted installed DSUN.EXE.
Run segment_references.py for 50B0 and check the nineteen/two site
partition against both independently known controls. Qualify the seven
entry/trampoline bindings with direct_callers.py. Read overlay_listing.py
descriptor 193 with bounds 7F0B7..7F409, 7F809..7F95F and
80140..80171. Bounds are hexadecimal and half-open. Follow the
segment assignments, including reused AX, and the actual byte and move
destinations; do not infer category writes from the segment operand.

Read descriptor 118 with report.mjs table using the build XXH3,
start 4B430, count/limit one, stride eight and width-two fields at
offsets 0, 2, 4 and 6 for segment, end, flags and start. Compare the
source-pointer and length records with FND-COMBAT-034. The configuration
category-segment-descriptor.json and original-derived reports/listings
remain in GAME_DIR/analysis/combat-effects/. No original code was run.

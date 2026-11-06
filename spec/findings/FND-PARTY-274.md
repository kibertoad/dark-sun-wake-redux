---
id: FND-PARTY-274
title: Byte-derived effect indices exclude shipped DATA word 1001 at every wrapped source position but negative positions alias mutable resident data
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007ED9D..0x0007EDF2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E907..0x0007E98C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047110..0x00047A10
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000567FE..0x00057110
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0000003E..0x000049BA
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py, tools/evidence/report.mjs table at d8b9ae1f; Python 3.12.10, Capstone wheel 5.0.9 and xxhash 4.0.1; FND-PARTY-106/273, FND-RNG-094 and FMT-EXE-001 reuse
environment: null
---

## Observation

Q-PARTY-124 retains effect-row index admission beyond FND-PARTY-273's
112 canonical rows. Installed DSUN.EXE is 634416 bytes, XXH3-128
`e296af55ba2ecde7e77f555c90f33d0b`.

### Local index widths and address calculation

In overlay 193 +0AAD, +0ADA reads the object's byte +0F through
the slot table displacement 67CA. +0ADE sign-extends AL into AX;
+0ADF decrements AX and +0AE0 stores the resulting index locally.
For arbitrary byte contents this producer yields -129 through 126,
not the unsigned-byte range 0..254. Exact -1 exits at +0AE7.
The following row lookup multiplies a word register by 18 and retains
segment 51F1. The later local retention and other direct producers
remain separate; this prefix does not prove all inputs of +12A8 bounded.

The cast-area helper +0617 also sign-extends a resource byte at +065A
and multiplies AX by 18 at +065B. Its source index range is -128..127.
The product is a 16-bit near offset, stored with segment 51F1. In
particular, -38 times 18 wraps to FD54. This does not address physical
bytes before segment 51F1. FND-RNG-094 supersedes FND-RNG-052's
incorrect before-segment wording while retaining the other observations.
The recorded DATA-73 value alone still does not prove a cast reaches
this helper or that the intervening DATA copy preserves its source.

The union -129..127 has 257 indices. It overapproximates these two
byte-derived producers, includes rejected sentinels and does not assert
that every index is reachable in normal play. Its selected DATA word
is at `51F1:((18*i) modulo 65536)`.

### Complete source-word query and relocation check

The fingerprint-guarded existing table reporter reads two disjoint source
queries, each with stride 18 and an unsigned word field at offset zero:

| Indices | Rows | File start | Declared query extent |
|---|---|---|---|
| 0..127 | 128 | 47110 hex | 47110..47A10 hex |
| -129..-1 | 129 | 567FE hex | 567FE..57110 hex |

No selected word in either query is 1001. The field is read only at
each row start; the query extent is not a claim that every byte is a
record. The known canonical controls are row zero's word 4 and row
111's word 302, independently recorded in FND-PARTY-106. The -38
selection is file 56E64, with shipped word zero.

The MZ header gives 4703 relocation entries at file offset 3E hex and
1312 header paragraphs, or 5200 hex bytes. Read the relocation table
in exact disjoint chunks of 4096 and 607 four-byte entries. A relocation
with offset o and segment s writes the word beginning at file
`0x5200 + 16*s + o` in the source mapping. None overlaps either byte
of any of the 257 selected words, including a write starting one byte
before the selected word. The independent startup control is recovered:
relocation segment zero, offset one names the immediate of the entry's
MOV DX at 1000:0000, as FND-PARTY-215 reads independently.

All queried words lie before the MZ image end 57570 hex recorded in
FMT-EXE-001; none substitutes appended overlay-pack bytes for loaded
resident bytes. Therefore the shipped-word exclusion survives the MZ
relocations at these positions. This is not a later-memory invariant.

### Concrete resident-data aliases

At the analysis mapping, 51F1's wrapped negative range is also addressed
through resident segment 57E0: the byte window 51F1:F6EE..10000 maps
to 57E0:97FE..A110. The selected word starts run through A0FE within
that window. The -38 selection 51F1:FD54 is exactly 57E0:9E64.
These are aliases of the same physical memory, not a different file
provider or an out-of-image read. The range overlaps resident runtime
state; no immutability claim follows from initially zero shipped bytes.

## Interpretation

For retained byte-derived indices and unchanged selected source words,
no value outside the canonical 112 rows in these two producer domains
introduces positive 1001. Remove unknown shipped out-of-table words
and relocation changes to those words from the remaining alternatives.

The remaining state question has concrete targets: writes, copies or
aliases to the selected resident words, particularly the negative-index
aliases at data offsets 97FE..A110; index retention after its producer;
and other incoming word-index domains not bounded by these prefixes.
Positive indices beyond row 111 also select neighboring data and require
their actual writers, rather than an assumed table-wide no-write property.
Neither the DATA-73 path nor all +12A8 callers are proved admitted here.

This changes no whole-site count: Q-PARTY-124's 83 other direct sites
remain, as does repeated-hook preservation. FMT-PARTY-007 remains
supported; no queue item closes or complete reading is declared.

## Alternatives

- A negative logical row index does not mean a negative physical address:
  the word multiply wraps before the near lookup.
- No 1001 in source bytes and no overlapping relocation do not exclude
  a later resident-state write of 1001 to a selected word.
- A bounded byte producer is not a universal domain for a word-argument
  consumer reached through other callers or after changed local state.

## How to reproduce

Read descriptor 193 at file `7ED9D..7EDF2` and `7E907..7E98C`
with overlay_listing.py. Preserve the sign extension, decrement, word
multiply and fixed segment. Reuse FND-PARTY-273 for the consuming lookup.

Use report.mjs table with the source fingerprint, the two starts/counts
above, stride 18, limit 256 and field data_word at offset zero, width two.
The retained queries also include the byte at offset two; that extra
field is not needed to exclude DATA word 1001. For the fixed MZ header
read offsets 6, 8 and 24 as unsigned words. Read relocation entries
from 3E hex with stride four, fields offset/segment at zero/two, and
counts 4096 and 607 in separate queries. Compare the resulting write
intervals to each selected word interval and retain the startup control.

Reports and JSON queries remain under
GAME_DIR/analysis/class-table-closure-20261106/ with object-index-source,
object-index-mz-header, object-index-relocations,
object-index-relocation-overlap-summary, object-index-relocation-control,
effect-index-cast-entry and effect-index-object-call stems.
No original game ran; no original-derived table or bytes are committed.

---
id: FND-COMBAT-095
title: RESOURCE.GFF is the sole WIND provider among the 26 fingerprinted top-level archives and its known windows have bounded child tables
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E5F4..0x6A737
tool: tools/research/exec-census/gff_tag_numbers.py and tools/evidence/xxh3.mjs at 70a5bee5
environment: null
---

## Observation

The nonrecursive installed-directory census with tag WIND covers exactly
26 top-level GFF files. Every file's byte length and XXH3-128 matches
its corresponding BLD-GOG-EN-1.1.files.yaml entry. The inspected set is:

- CHARSAVE.GFF, DARKRUN.GFF, DARKSAVE.GFF, GPLDATA.GFF, OBJEX.GFF
  and RESOURCE.GFF;
- the twenty RGN archives with suffixes 001, 032, 033, 034, 036, 037,
  038, 039, 03A, 03B, 03C, 03D, 03E, 03F, 041, 042, 043, 044, 045
  and 0FF.

RESOURCE.GFF is the sole file in that set with a WIND type table.
Its expanded table has exactly the 28 resource numbers recorded in
FND-UI-001, from 3020 through 19505 with the gaps that finding lists.
The other 25 files have no WIND table. This is a directory result,
not a search for tag-shaped payload bytes or a live archive-list census.

The unchanged census procedure and identical file set retain
FND-COMBAT-089's controls for both directory encodings: the plain
GFFI tables have 5, 7 and 20 resources in GPLDATA.GFF, OBJEX.GFF
and RESOURCE.GFF respectively, and the indexed PORT table has
178 resources in GPLDATA.GFF alone. Their independent reference is
FND-GFF-003. The present WIND positive result also agrees with
FND-UI-001's separate resource enumeration. The new query changes
the requested tag, not the directory decoder or source files.

For that sole provider's known WIND payloads, FND-UI-001 verifies
each embedded tag, length and number against its directory entry.
FND-UI-002 reads every payload's child count, table and zero fields.
Together those retained readings establish this source-side extent:

- Each complete payload is 261 plus 30 times its child count bytes,
  with counts from 1 through 89 and lengths from 291 through 2931.
- Byte +F2 is zero. The count byte at +F3 is followed by zero at
  +F4, so the word count read by the resident routines has that same
  value. The child table starts at +105 hexadecimal and ends exactly
  at the payload boundary.
- The 402 child entries have zero initial far-pointer slots and
  zero flag words at +1C. They name 209 APFM, 184 BUTN and 9 EBOX
  resources that exist in the same provider. No source entry in this
  set takes the MENU, ACCL or unrecognized-tag registration branches.

The source-side table extent therefore agrees with FND-COMBAT-093's
registration indexing and FND-COMBAT-092's lookup indexing when the
complete unmodified payload is present, the pointer maps that payload
without offset wrap and the fields are preserved until consumption.
It is not a new native-buffer measurement. The on-disk pointer slots
are still zero before registration replaces them with acquisition results.

## Interpretation

For this installed snapshot, a different file among the other 25
inspected archives cannot supply a WIND table to the window request.
Successful selection and complete transfer from the sole known provider
would supply the bounded source child table already documented in
FND-UI-001 and FND-UI-002. Mere pointer return does not establish
those conditions, as FND-COMBAT-094 shows.

Q-COMBAT-053 can exclude those other 25 unmodified top-level files
as alternative WIND providers and reuse the known provider's source
extent. Its remaining work is live archive/directory admission, heap
and buffer extent/contents, actual external outcomes and preservation.
The source bounds do not settle the returned allocation's address or
capacity, a failed transfer's bytes, or a modified child's later callback.

## Alternatives

This census excludes the disc image, nested paths, numbered saves,
external files, archive contents introduced later and modified working
archives. FND-UI-001 independently compares the disc provider's 28
WIND payloads with the installed ones, but this query does not survey
every disc file or a running archive graph. Dynamic registration or
modification remains possible within the unresolved admission question.

An exact complete copy of the known payload has the source table above.
A partial transfer, different runtime directory state or later memory
write need not. Source fingerprints and table extents alone cannot choose
between those live cases. No successful allocation, native I/O failure,
callback target or complete-reading declaration follows from this census.

## How to reproduce

Resolve the licensed installed directory through tools/game-dir.mjs.
Enumerate only its top-level *.GFF files; require exactly the 26 names
above. Hash every file with tools/evidence/xxh3.mjs and compare both
length and XXH3-128 with BLD-GOG-EN-1.1.files.yaml before reusing any
negative result. RESOURCE.GFF must remain 5724669 bytes with XXH3-128
92fe031616f36062f99484b0f728210e. A changed, extra or missing archive
changes this query's scope and invalidates reuse of its result.

With the locked evidence Python, run gff_tag_numbers.py on that
directory with tag WIND. Require the sole provider and the full 28-number
set in FND-UI-001, not just a positive count. Reuse FND-COMBAT-089's
GFFI/PORT controls only while the source fingerprints and census script
are unchanged; otherwise rerun those controls and compare FND-GFF-003.
The census script's last change is 9249d9e7, also present at 70a5bee5.

Reuse FND-UI-001/FND-UI-002 for payload headers and every child's
stored fields; no new inference about an opaque field comes from a
divisible size. Relate their exact F2/F3/F4 values and table extent to
FND-COMBAT-093 and FND-COMBAT-092 only under the complete-copy and
storage conditions above. No original process or emulated call is used.

---
id: FND-CONFIG-234
title: Message resource lookup searches typed entries across archive records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1..38FF:09D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0045
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:01BC
tool: Retained original Python 3.14.7 / Capstone 5.0.7 lookup survey; failure/output rereading with pinned Python 3.12.10 and Capstone module 5.0.7 (distribution 5.0.9), resident_listing.py and overlay_listing.py at 23fa5697
environment: null
---

## Observation

This replaces FND-CONFIG-038. Its original lookup survey is retained from
d393605eedede878e2882c48b13baee78fb45346, together with the later
FND-CONFIG-039/040 references in its current Alternatives. The old record
is preserved under its original ID. This replacement corrects the broad
claim that every reader failure prevents a resource pointer return;
lookup behavior is retained, not newly reread in every helper.

The resource reader at `38FF:07B1` starts with the archive pointer at
`DS:9D9F` (FND-CONFIG-037). Before searching it calls `39A9:012E`, which
tests the resident four-byte archive signature against `GFFI` and sets a
nonzero error on mismatch. A missing archive pointer or output pointer
also returns an error before search.

For each selected archive record, the reader calls `39A9:0045` with the
requested four-byte type. That helper requires a `GFFI` archive header,
walks the archive's type directory and returns the matching type record
or zero. The reader then tests the type record's high flag bit. Without
that bit it calls `39A9:01BC` with the requested number; this helper
rejects a null or high-bit type record and performs a bounded binary
search over twelve-byte numbered entries, returning the matching entry
or zero. With the high bit set, the reader instead calls an alternate
lookup helper at `38FF:0615` (`DSUN.EXE+0x0002E805`), which has separate
success and failure results. Both missing-type and missing-number paths
advance to another archive through `39A9:0157` when it returns a nonzero
pointer. That traversal helper checks `DS:9D9B` and the active archive
pointer when deciding whether a successor is available.

After an entry is selected, the reader checks its recorded length and
offset, checks a requested range against that length, allocates an output
buffer when the caller supplied none, then checks seek and read results.
An error result does not uniformly prevent a pointer return. In the
zero-output allocation path, 07B1 calls 444C:0008 with the selected count
and first argument one, tests DX:AX and publishes that nonzero allocation
through its caller's far output address at +0939. This precedes archive
preparation at +095A, seek at +0976 and read at +09A2. Preparation failure
jumps to the FFFF return; seek or read result mismatch reaches error 21
and the same FFFF return. These own paths contain no allocation release
or output-pointer clear after publication. Early lookup or allocation
failure, by contrast, need not have published a nonzero allocation.

38FF:04AB forwards the reader's result, but overlay 182 +00C4 ignores
that result for selection: it initializes its output local to zero,
passes that local with the WIND tag and zero-extended number, then
reloads DX:AX from the local without a result-dependent branch. Thus,
if an allocation P is published and preserved through a returned later
failure, +00C4 returns P even though 07B1 returned FFFF. Its OR of AX
has no following conditional branch before that reload. The enclosing
+016B helper tests the pointer and subsequent setup results; it does
not recover and test the discarded reader status.

This is a conditional source-level failure/output counterexample,
not an executed I/O failure. Its frame, output address, allocated span,
callee effects and actual native failure remain admission conditions.
FND-EXE-648 connects it to the slot-action window's published destination;
FND-CONFIG-151 retains the reader's complete local search/transfer graph.

## Interpretation

`WIND/10501` acquisition depends on a reachable archive record and a
matching type and numbered entry, in addition to the later allocation,
seek and read results. Finding that record in the installed file alone
does not establish that the reader reaches it in every message state.
A nonzero returned pointer alone does not establish a completed resource
transfer or a valid fully populated window record.

## Alternatives

FND-CONFIG-039 identifies startup archive registration, and
FND-CONFIG-040 identifies its wraparound traversal setting. A later
archive close or mode change may change reachability. This reading
does not identify archive state in every message call or which callers
reach this reader. It does not establish a failed lookup in a live state.

## How to reproduce

For the retained original lookup survey, use its adding revision above.
Disassemble the approved `DSUN.EXE` at physical offsets
`0x0002E9A1..0x0002EBC6` (reader), `0x0002ECD5..0x0002EDBE` (type
lookup), `0x0002EDE7..0x0002EE4C` (archive traversal), and
`0x0002EE4C..0x0002EF19` (number lookup). Follow the reader's branches
through both type-record flag cases, the zero results and the returned
success path. The alternate helper begins at `0x0002E805`; its full
internal format is not established here.

For the correction, use the pinned readers at 23fa5697. Read
`resident_listing.py <dsun> 38FF:04AB..38FF:04D5
38FF:07B1..38FF:09D6` and `overlay_listing.py <dsun> 182 68914 68946`
and `182 689BB 68A3F`. Verify allocation publication precedes each
later error, no own clear/release occurs on those exits, and the wrapper
reloads the output without testing status. The fingerprint-checked
input is DSUN.EXE, 634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Retain all native/frame/alias conditions.

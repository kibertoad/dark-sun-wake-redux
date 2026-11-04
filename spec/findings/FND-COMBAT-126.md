---
id: FND-COMBAT-126
title: Archive growth requests old-record release before replacement allocation and publishes links after directory reload
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:077C..37FC:08FB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092..444C:00F6
tool: tools/research/exec-census/resident_listing.py at c32f344e; existing growth inventory FND-CONFIG-068
environment: null
---

## Observation

FND-CONFIG-068 identifies this active-record replacement mechanism.
Q-COMBAT-054 retains its shared archive-state effects; Q-COMBAT-064
depends on them for numbered resource providers. This reads the entire
local growth body and connects its copy/release dependencies. Offsets
and sizes below are hexadecimal.

### Snapshot and release precede allocation

077C accepts a far address of a record-pointer variable. After its guard,
it saves the current pointer locally. Record dword +24 mask 4 rejects
growth with DS:9D99 = 0A and AX FFFF. Otherwise it calls 08FB on the
old record and requires AX zero before proceeding. FND-COMBAT-124
supplies that preparation contract, including possible file writes.

It copies 39 bytes from the old record into SS:BP-46 through
1000:0452, then passes the old pointer to 444C:0092. There is no
release-result test or own clearing of global/neighbor references here.
FND-CONFIG-165 describes that wrapper: metadata checks precede an
unconditional runtime request, and normal return forces DX:AX zero.
The ordering does not prove successful release or valid metadata.
Shared allocator admission remains Q-CONFIG-008 and Q-SCRIPT-003.

Using the saved directory length (old record +20), growth computes
length +331 in double-word arithmetic and requests 444C:0008 with
factors one and that computed size. No own overflow gate precedes
this request. A null returned pointer sets global error 0A and returns
FFFF. There is no own recreation of the old record or reference repair
on that branch. Successful release would leave old-reference validity
needing separate admission; no native dangling use is observed here.

### The directory comes back from the file

For a nonnull allocation, it copies the saved 39-byte prefix into the
replacement through 0452. It seeks the saved file handle (+4) to the
saved directory position (+1C), requiring returned DX:AX to equal that
position. Only an exact position reaches the read request.

The read asks 44DE:01AF for the saved directory length (+20), into
replacement segment with its offset increased by 31 in word arithmetic.
It requires the returned double-word count to equal that saved length.
There is no own far normalization of this destination addition or
capacity/segment-wrap check. The directory is requested from the file
after preparation, rather than copied in full from the old allocation.
The prefix copy includes only its first eight directory bytes before
the subsequent read.

After either seek/read outcome, it writes dword 300 to replacement
+2D. A mismatch then sets DS:9D99 = 0E and returns FFFF before the
publication steps below. No own branch releases that replacement,
restores old storage or reverses file/preparation effects. Children,
aliases and external I/O can have additional effects; this records
the outer requests and missing rollback, not an observed native leak.

### Publication is conditional and ordered

After exact seek/read results, it compares DS:9DA3 with the saved old
pointer and replaces it only on equality. It does the same separately
for active pointer DS:9D9F. Copied replacement links then drive
neighbor stores: a nonnull +8 link receives the replacement pointer at
neighbor +0C, and a nonnull +0C link receives it at neighbor +8.
These stores assume valid stable linked records; no own reciprocal-link
or membership validation precedes them.

Last, it writes the replacement pointer through the original supplied
pointer-variable address and returns AX zero. That output may name
DS:9D9F on FND-COMBAT-121's writer route, but the helper also performs
the explicit global comparisons independently. It contains no own
assignment clearing DS:9D99 on this successful continuation.

0452 copies CX bytes forward with CLD, word pairs and an odd tail,
restores DS/SI/DI and removes its two far arguments on return.
These 39-byte calls require valid separate frame/record spans and no
offset wrap; the primitive has no bounds or overlap admission. It is
distinct from FND-COMBAT-125's directional copier.

## Interpretation

The writer's growth retry can replace record storage, reload directory
bytes and update archive graph references. Release is requested before
replacement allocation; failed I/O has no own replacement cleanup or
reference rollback. Success requires the reload checks before its own
publication. This is a conditional local ordering, not native failure evidence.

This removes growth's local ordering and directory-source uncertainty
from unread work in Q-COMBAT-054/064. Actual inputs, allocator and guard
effects, record/link identity, arithmetic bounds, alias preservation and
external results remain open. No question closes or status is promoted.

## Alternatives

- A returning release wrapper does not prove successful memory release.
- A nonnull replacement does not admit its requested capacity.
- A copied prefix does not retain the entire old in-memory directory.
- A failure return does not imply unchanged storage or file contents.
- Conditional global comparisons do not validate neighboring links.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read all listed half-open ranges.
Map SS:BP-46's fields back to the copied record, follow release before
allocation, exact seek/read comparisons and publication comparisons.
Compare FND-CONFIG-068/165 and FND-COMBAT-121/124. Retain the listing
only in GAME_DIR/analysis/hit-incoming-20261104/restore-archive-growth.txt.
No original record, directory or listing is committed.

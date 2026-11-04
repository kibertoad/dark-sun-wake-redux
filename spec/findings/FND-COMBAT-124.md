---
id: FND-COMBAT-124
title: Dirty archive preparation checks header and directory writes then AH 68 before clearing its flag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 37FC:08FB..37FC:09C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:02F8..44DE:0321
tool: tools/research/exec-census/resident_listing.py at d5f1be4d; existing preparation order FND-COMBAT-096
environment: null
---

## Observation

Q-COMBAT-064 retains directory publication and flush effects.
FND-COMBAT-096 already reads 37FC:08FB's complete ordering.
This connects its final handle operation and dirty-flag contract
to the writer and numbered-reader paths, without claiming new
native I/O. Offsets below are hexadecimal.

### Exact own clearing order

08FB checks the supplied record's byte +2C after its stack guard.
Zero skips the file requests, reaches the common byte-clear and
returns AX zero. Nonzero retains record handle +4 in SI and
requests position zero through 38FF:0A72. It requires returned
DX:AX zero, writes 1C bytes from record+10 through 44DE:022C
and requires exactly 1C returned bytes.

It then requests the current directory position at record+1C,
compares the returned position with a reread of that field, writes
the current length at +20 from record+31 and compares the result
with a reread of that length. These are fresh field reads, not a
frozen header/directory snapshot. Handle, record, field and callee
preservation are prerequisites to combining the requests as one
coherent file update.

Only after those checks does it reread record handle +4 and pass
that word to 44DE:02F8. Unlike the preceding requests using SI,
this is a fresh handle-field read. AX FFFF goes to the shared error
path; other AX reaches the dirty-byte clear and AX-zero return.
Each own failed check sets DS:9D99 to 15 and returns FFFF without
its success clear. No own branch restores already requested file bytes.

### The final wrapper reports through AX

02F8 loads BX with its handle argument and issues INT 21h with
AH 68. Carry clear maps to AX zero. Carry set stores returned
AX in DS:33BE and maps the result to FFFF. Its common ending
explicitly clears carry before restoring saved SI, DI, DX, ES and
DS and returning. It does not save/restore BX around that request.
The caller's AX test therefore retains this error distinction;
testing returned carry alone would not.

This helper makes no own seek, zero-length write or explicit file-length
assignment. The outer preparation sequence likewise contains only the
recorded positioning, byte writes and AH-68 wrapper. These observations
identify the actual requests; they do not prove file length, durability,
external success or absence of other file operations elsewhere.

08FB does not independently validate option mask 0002, the record's
capacity or consistency of its fields. Its clean-record AX-zero return
also has no own clear of DS:9D99. An AX-zero result is therefore not
an independent proof of globally clear errors or valid archive contents.

## Interpretation

The final operation and dirty-state publication are now explicit for
Q-COMBAT-064. Clearing the dirty byte requires the named checks on
the dirty path; failures supply no local rollback. Record identity,
same-handle continuity and actual external outcomes remain unadmitted.

FND-COMBAT-121/122/123 mark metadata dirty while constructing or
changing directory entries. FND-CONFIG-151 and FND-COMBAT-120 show
that resource reads call 08FB on their selected record before payload
seek/read; indexed lookup can also call it before reading an index pair.
Consequently a resource-read route can request header/directory writes
when that record is dirty. A failed earlier payload write does not by
itself prove that later acquisition skips preparation or undoes pending
metadata. No particular native combination is asserted here.

The local flush body need not be reread as an unknown operation.
Actual directory bytes, bounds, handle/field preservation, copies and
I/O still belong to Q-COMBAT-064, with downstream storage and child
effects in Q-COMBAT-065/066. No question closes and no rule or parity
status changes.

## Alternatives

- Dirty-flag clearing is later than the directory write request.
- Carry returned by 02F8 does not preserve the interrupt's failure bit.
- A resource acquisition path is not necessarily free of file writes.
- A clean flag and AX zero do not independently validate directory contents.
- Successful preparation does not prove a later close or payload read succeeds.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read both listed ranges from
their inventoried starts; follow SI and the final reread handle separately,
then the interrupt carry, normalized AX and explicit CLC. Compare
FND-COMBAT-096 and FND-CONFIG-151 for the already-read caller order
and transfer dependencies. Retain the listing only under
GAME_DIR/analysis/hit-incoming-20261104/restore-archive-flush.txt.
No original bytes, directory or native file operation is committed.

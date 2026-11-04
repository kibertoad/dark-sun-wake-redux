---
id: FND-COMBAT-127
title: Archive opening constructs and reads a record before list publication but does not reverse earlier accounting on read failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066..38FF:02B5
tool: tools/research/exec-census/resident_listing.py at 6ab418fb; existing opener mapping FND-CONFIG-037/067 and FND-COMBAT-110
environment: null
---

## Observation

Q-COMBAT-054 retains archive-record producers, shared by
Q-COMBAT-064. FND-CONFIG-037 and FND-COMBAT-110 already connect
successful opening to active selection. This follows the opener's
complete own construction, failure and publication order. Offsets,
field sizes and error values below are hexadecimal.

### Header and allocation inputs

After its guard, 0066 requires nonzero AL from 39A9:012E.
FND-CONFIG-208 reads that global signature check and error reset;
it checks DS:9DA7, not bytes from the file being opened.
The opener requests 44DE:0086 with path and mode word 3. AX FFFF
sets DS:9D99 to 3 and returns FFFF; another word becomes its local
file handle in SI. Actual path/file admission remains Q-COMBAT-063.

It reads 1C bytes into SS:BP-28 through 44DE:01AF and requires an
exact double-word returned count. A short/mismatching count requests
44DE:003A close, ignores that result, sets global error 15 and returns
FFFF. The next gate compares header dword +4 unsigned to 30000;
larger values request close, set error 1B and return FFFF. The body's
own path has no comparison of the file header's first four bytes or
directory extent against file length. These are literal local gates,
not proof that arbitrary input reaches a successful native open.

Let K be its double-word argument at BP+0A and L the read header's
directory length at +10. It computes T = K + L + 31 with double-word
wrap and no own overflow rejection. For option word mask 4 clear,
it requests 444C:0008 with factors one and T; null sets error 0A
after an unchecked close request and returns FFFF. The restore route
in FND-COMBAT-110 passes option 2 and K = 04D4, taking this allocation
branch under admitted unchanged arguments.

For mask 4 set, it instead requires unsigned K >= T, sets a local
flag mask 4 and takes its record pointer from the location named by
the output argument. It has no independent null check of that location
or the loaded pointer on this branch. The test is the wrapped comparison
as encoded; it is not replaced here with an intended capacity formula.
Option mask 8 adds local flag mask 8. These are conditional branch
contracts; their actual callers and storage admission remain unresolved.

### Construction and directory transfer precede publication

Before the directory transfer, it adds T to global dword DS:9D95
without an own overflow gate. It writes the record's internal GFFI
signature at +0, file handle at +4, original option word at +6,
and null links at +8 and +0C. It copies the 1C-byte local header
to record+10 through 1000:0452. Its local header +14 was previously
set to zero, then ORed with option-derived masks 4/8, so the copied
record +24 comes from those local stores rather than retaining the
file's original header +14. FND-COMBAT-126 supplies 0452's local
copy contract and its capacity/alias qualifications.

It clears dirty byte +2C and writes dword +2D. On mask-4-clear
allocation, +2D receives K. On mask-4-set supplied storage, it receives
K - (L +31) in double-word arithmetic. It reads directory length +20
and position +1C from the constructed record into locals, seeks SI to
that saved position and requires the exact returned position. Only that
result reaches a read of the saved length into record segment with
record offset +31, a word addition without own far normalization.
It requires an exact double-word read count.

A failed position or count requests file close and 444C:0092 release
of the constructed record, ignores both results and sets error 15 before
returning FFFF. There is no own subtraction of the earlier DS:9D95
increment, no caller-output reset and no restoration of previous record
bytes. This same cleanup is encoded after either storage-source branch.
FND-CONFIG-165 qualifies the release wrapper; the requests do not prove
successful cleanup or a native leak. Children, guards and aliases can
have additional effects beyond the own body described here.

### Successful graph and handle publication

Only after the transfer checks, a nonnull current head DS:9DA3 gets
its +8 link set to the new record. The new record +0C receives that
current head. The new record then becomes both DS:9DA3 and DS:9D9F.
There is no own link-membership, cycle or duplicate-pointer validation.

Next it zero-extends word DS:9D8E into record dword +28. A nonnull
output argument gets that word and a zero high word. It increments
DS:9D8E and count DS:9D9D as words and returns AX zero, without
independent wrap/uniqueness checks or another child call. FND-CONFIG-067
already establishes the numeric handle versus far-record distinction.
These sequential stores require valid distinct output/record/global
storage to compose as an ordinary graph update.

## Interpretation

The opener's own failure paths precede list/active-pointer publication,
but they do not establish an unchanged process state: file requests,
record stores and allocation accounting may precede a later rejection.
An AX-zero return likewise does not independently validate header magic,
directory structure, pointer capacity, graph identity or handle uniqueness.

This removes the local open construction, accounting and failure-order
obligations from unread work under Q-COMBAT-054. Q-COMBAT-064 reuses
that shared producer reading. Actual paths, runtime record/graph state,
allocator/guard preservation, arithmetic bounds, aliases and external
I/O remain unadmitted; no queue item closes or status is promoted.

## Alternatives

- A global signature gate is not validation of the file's header signature.
- Rejection before graph publication need not undo earlier accounting.
- An ignored close/release result does not establish cleanup success.
- Word handle generation does not independently prove unique membership.
- A requested directory length is not a validated directory structure.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. resident_listing.py verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read the full listed half-open
range, map the 1C-byte SS header to record+10 and distinguish the
two allocation/storage branches. Track DS:9D95 before seek/read,
cleanup without an own decrement, and graph/handle publication only
after exact transfer results. Compare FND-CONFIG-037/067/165/208,
FND-COMBAT-110 and FND-COMBAT-126. Retain the listing only under
GAME_DIR/analysis/hit-incoming-20261104/restore-archive-open-contract.txt.
No original header, directory, memory image or listing is committed.

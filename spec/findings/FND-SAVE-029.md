---
id: FND-SAVE-029
title: Explicit Save supplies independent counted-write controls through resource payload updates and the numbered-file copy
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070713..0x0007089C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070BFD..0x00070CEE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44DE:022C..44DE:02A9
tool: connected evidence review at 98e17b26; FND-SAVE-008/015/021, FND-COMBAT-034/096/113/121/122 and FND-SAVE-028; retained controlled interrupt report
environment: null
---

## Observation

This supplies Q-SAVE-007's explicit-Save transfer control from existing
independently recorded paths. No new byte search or tooling is needed.
The cited paths use installed DSUN.EXE,634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. This is a composition of bounded
static interfaces, not a fresh run or a successful native save claim.

### Resource payload control

FND-SAVE-008 connects Save Game's selected record path and description
to overlay187's wrapper at file70BFD. Its opened working-archive path
calls the numbered writer at70713 with selector zero and that description.
FND-SAVE-015 and FND-COMBAT-034 independently identify the writer's
source and length tables, selector-zero61-entry loop and forwarding into
37FC:00E8. For example, retained shipped entry23 supplies1920 bytes
from4E71:024D for SAVE24; the table and source still require use-time
admission, not inference from their shipped values alone.

FND-COMBAT-121 supplies00E8's guard, signature, selected active-record
and option0002 gates. Under those gates it constructs or updates the
directory before its payload seek/write. The joined branch writes the
requested payload through44DE:022C and checks the full returned count.
It marks the current active record's byte+2C dirty even on the recorded
payload-transfer failure continuation. FND-COMBAT-122 retains the
selection/removal failure ordering and record-identity conditions.

Thus explicit Save has a concrete payload-to-counted-writer route and
a dirty-state producer. A payload failure does not establish clean archive
metadata; a later close can still enter FND-SAVE-028's dirty preparation.
Actual record identity, valid buffers and positions, native outcomes and
subsequent field preservation remain required to compose one live update.

### Numbered-file copy control

After its working-archive update/close path, the explicit Save wrapper
passes DARKRUN.GFF as source and the selected SAVEnn.SAV path as
destination to4544:0000 (FND-SAVE-008). FND-COMBAT-113 reads
that copier's open, length, read/write and failure contracts. With admitted
handles and its default128-byte capacity, each positive iteration first
requires the exact read count and then calls44DE:022C for that count.
A short read skips that iteration's write; a short/failed write clears the
success flag and ends the loop without rollback or retry.

For those at-most128-byte write requests,022C bypasses its large-count
loop and reaches its nonzero-count DOS AH40 request at0285. This is
an explicit-Save positive control independent of the shutdown dirty-record
call chain. The large-count site0264 remains separately qualified by
the same complete helper reading, not by assuming a128-byte iteration
exercises both sites.

The matching retained interrupt report lists both0264 and0285 as reached
in its declared walk, as FND-SAVE-028 records. Its candidate sites,84
unresolved transfers, five gaps, four contested sites and false
negativeUsable flag remain unchanged. The new connected control does
not repair that report's missing coverage or prove a global no-write claim.

### What distinguishes the three operations

The explicit resource writer takes the live source tables and submits
numbered SAVE payloads. The explicit copier transfers working-file bytes
to the chosen numbered path. Shutdown close-all instead walks active
archive records and may flush their already-dirty header and directory.
They share a counted-write mechanism but differ in caller inputs and
purpose. Close-time flushing alone does not establish a fresh snapshot,
while an explicit save attempt does not guarantee all payload writes or
the later numbered copy succeed.

## Interpretation

The explicit-Save lower-transfer control required by Q-SAVE-007 is now
bound to the actual022C/0285 mechanism. It need not remain a generic
missing-control obligation. The unresolved work is actual shutdown
record/handle/dirty state and outcomes, plus computed service/callback
paths and any other save-producing calls they admit. No complete reading,
whole queue answer, native write observation or parity promotion follows.

## Alternatives

Treating a dirty flag as proof of successful payload transfer ignores its
failure continuation. Treating a128-byte copy as exercising the large-count
site ignores022C's branch. Treating shared file-write machinery as shared
gameplay-save semantics ignores the distinct source selection and copier
arguments. Treating a passing control as complete census coverage ignores
the retained report's unresolved paths.

## How to reproduce

Review the cited committed findings together: FND-SAVE-008's wrapper
arguments; FND-SAVE-015/FND-COMBAT-034's numbered source/length
forwarding; FND-COMBAT-121/122's payload and dirty-state ordering;
FND-COMBAT-113's complete copier and022C branches; and
FND-COMBAT-096/FND-SAVE-028's close-time preparation. Follow their
recorded source-checked reproduction commands only if retained reports
are unavailable. Check the nonzero default copy chunk against the0285
site, retaining the untouched large-count and incoming-state conditions.
Reuse the matching q007_dsun_census-report.json and after-interrupts.txt
under GAME_DIR/analysis/handler-census-20261106/ with all their limits.
This evidence review runs no original or emulated function and commits
no original-derived bytes or output.

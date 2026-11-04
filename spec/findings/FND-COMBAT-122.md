---
id: FND-COMBAT-122
title: Numbered saves continue the current remove-write pair after selection failure and do not branch on removal results
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
    address: 37FC:05C5..37FC:077C
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 4daf8bd9; existing signature and selection contracts FND-CONFIG-208 and FND-COMBAT-096
environment: null
---

## Observation

Q-COMBAT-064 retains the runtime SAVE directory's producer and preservation.
FND-COMBAT-034 and FND-SAVE-021 identify overlay 187 +08E3's
numbered writer; FND-COMBAT-121 reads its resource-write entry.
This reading resolves the intervening removal and the caller's three
separate result paths. Offsets below are hexadecimal.

### Selection failure does not skip the current pair

For a nonnull source slot, +0976 passes current DS:144E to
37ED:0064. After argument cleanup it tests AX. Nonzero clears
the local success byte BP-1 at +0987, but the next instruction
still computes the numbered resource and calls 37FC:05C5. The
failure store has no branch past that removal or the following write.

After removal the caller executes OR AX,AX at +09A5, then MOV
AX,DI and the resource-number calculation. There is no conditional
branch consuming the removal result or store of it to BP-1. It
pushes the source-table length and pointer and calls 37FC:00E8.
Only the write's nonzero returned AX clears BP-1 at +09DF.

The loop then increments its index and tests its bound and BP-1.
Thus a selection failure stops later iterations, subject to preserved
locals, after the current remove/write calls rather than before them.
The later bitmap release and script-entry restoration still have their
own continuations, as FND-SAVE-021/023 record. The optional STXT
ending likewise performs its removal and write after its selection
test, without branching on the removal result.

FND-COMBAT-096 shows that rejected selection has no own replacement
of the active pointer. Consequently the subsequent calls use current
active state, not a handle value passed directly with each resource.
Whether that state still names a writable record and which record it
names remain admission questions, not an observed wrong-archive write.

### The removal entry acts on a plain type in the active record

37FC:05C5 performs its signature check, requires a nonnull active
pointer and option mask 0002 at record +6, then looks up the supplied
tag in that record. It has no own fallback traversal. Rejections set
error 0C for no active record, 1E for a rejected option, four for
missing type, or 1F for an indexed type. It then looks up the supplied
number in the plain table and sets error six if no entry is found.
Those own early branches return FFFF before directory compaction.

For a found entry it retains its file position at +4 and length at
+8, and passes them with current active record to 0A9F. Nonzero AX
sets error eight and returns FFFF before the removal body's own shift.
That helper's potential preceding mutations are not excluded by this
return or independently read here.
FND-COMBAT-123 subsequently resolves its own rejection-before-mutation
branches; guard, pointer and copy effects still need separate admission.

On a zero 0A9F result, the body derives the span after the twelve-byte
entry and requests an in-place shift through 38DC:0066. It marks
record +2C dirty, subtracts 0C from +20 and +35, adds 0C to +2D,
and decrements the type's double-word count at +4. There is no own
branch deleting the type header when that count becomes zero.
Valid directory bounds, pointer arithmetic, copies and preserved type/
record identity remain prerequisites to interpreting this as one entry
removed. FND-COMBAT-121 retains the shared copy-helper limits.

Record flags +24 with mask 00000008 call 08FB after those stores
and return its AX; otherwise the ordinary continuation returns zero.
The local body supplies no rollback of the count or shifted directory
if later preparation reports an error.

### A prior removal error is not necessarily sticky

The following write entry calls 39A9:012E anew before its gates.
FND-CONFIG-208 reads that helper: on an admitted signature match
it sets DS:9D99 to zero and returns AL one. Therefore a missing
type or number reported by removal does not itself require the next
write to fail. A missing plain type can proceed through the constructor
of FND-COMBAT-121; a missing number can take its insertion path.
Actual record, count, directory space and I/O admission still apply.

Conversely, a failed indexed-type removal does not convert its input:
the following write independently rejects an existing indexed type.
The caller's ignored removal result does not establish that deletion
succeeded or that any earlier backend changes were undone.

## Interpretation

The known numbered-save producer is a sequence of separately checked
and unchecked operations, not a locally atomic select/remove/write
transaction. Selection failure affects the retained loop flag but does
not prevent the current pair; removal's returned word does not select
the continuation; writing has its own signature reset and result gate.

Q-COMBAT-064 need not reread those local result combinations or the
plain-removal publication order. It still needs actual active-record
and saved-handle identity, directory/count bounds, reservation/free-space
and copying/flush effects, and actual external outcomes. A nonzero
outer result is not proved to describe every earlier operation, and
neither a missing resource nor a partial mutation is established in a
native save by these local paths. No queue item closes and no rule or
parity status changes.

## Alternatives

- A failure flag assignment is not an immediate branch out of the iteration.
- OR AX,AX without a consuming branch does not enforce removal success.
- A new signature check can clear an earlier resource-operation error.
- Removing the last plain entry does not locally remove its type header.
- Returning failure after mutation is not restoration of old directory state.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for
descriptor 187, file bounds 70713 7089C, and resident_listing.py
for 37FC:05C5..37FC:077C. Follow BP-1, the selection branch join,
the OR/MOV after removal and the write-result branch separately.
Compare FND-CONFIG-208, FND-COMBAT-096/121 and FND-SAVE-021/023
for already-read signature, selection, construction and continuation
contracts. Keep 0A9F/copy/preparation effects explicit. Retain listings
only under GAME_DIR/analysis/hit-incoming-20261104/:
restore-save-resource-removal.txt and restore-save-remove-write-caller.txt.
No original listing, payload or native save was committed or modified.

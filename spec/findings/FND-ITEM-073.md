---
id: FND-ITEM-073
title: Effect-row removal rescans compacted rows and prepares a bounded deferred unload packet after partial removal
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:050F..371D:0770
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:007A..289B:00F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:01F9..371D:0334
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
tool: tools/research/exec-census/resident_listing.py at b29185b4; Python 3.12.10, Capstone 5.0.7; composition with FND-PARTY-262, FND-COMBAT-159 and FND-ITEM-063/065/066/068
environment: null
---

## Observation

Q-PARTY-116 retains removal/redraw/deferred children after the local
constructor and selected scanner readings. This reads the removal's
compaction and deferred insertion contracts, without treating a paused
table endpoint as a constructor call. Addresses/offsets below are
hexadecimal; row counts and lengths are decimal.

### Matching rows and compaction

FND-PARTY-262 supplies the zero-count unload correction. For an admitted
slot s with object byte +0F nonzero, 371D:050F sets the effect index to
that unsigned byte minus one and scans eight-byte rows below DS:264E.
A row qualifies only when flag byte +5 has bit 20 and slot word +6 equals
s. The scan begins at index zero and DS:67B7; its matching branch calls
the already identified cell children before compaction. Their effects
and use-time row/frame preservation remain conditions.

289B:0094 receives the current index, table pointer, address of persistent
count DS:264E, presentation table DS:67BB and presentation count 320.
FND-COMBAT-159 establishes its early decrement and sentinel-preserving
adjustment. With admitted 0<=index<old_count<=1500 and valid retained storage,
it decrements the persistent count. When rows follow the removed row,
it copies exactly old_count-index-1 eight-byte rows forward from the next
row into the current position, then decrements presentation indices in
the unsigned interval [index, old_count). FFFF presentation indices are
excluded. Removing the final row skips the copy/adjustment branch.

Back in 371D:06B8, the caller rereads persistent count and table base,
then clears only byte +5 of the row at that new count boundary. It does
not clear the whole former tail row. It increments its local removal
count and returns to the loop test without advancing the scan index or
saved row pointer. Therefore the shifted replacement at that position is
examined next. A nonmatch advances both by one row. These statements
require count/base/pointer identity to survive the cell/compaction children;
an alias or another writer is not ruled out by the local loop.

With second argument byte zero the loop stops after ten removals, or
earlier at the current count boundary. A nonzero byte removes that local
cap. This composes with FND-PARTY-262's post-loop gate: zero removals or
nonzero second byte request immediate unload. Nonzero removals with zero
second byte select the redraw/deferred branch instead.

### Redraw arguments and deferred payload

The routine accumulates four signed-comparison words from each matching
row and its object geometry before removal. Minima start at 10000 and
maxima at -1; additions/subtractions keep word width. It passes the
current DS:1408/140A and those accumulated words to 31E0:128C with two
literal-one arguments. That child's drawing/field effects and frame
preservation are not established here, nor are these accumulators an
unconditional valid on-screen rectangle.

After the redraw child returns it writes slot s to local word BP-20 and
byte two at BP-1E. It passes the far pointer SS:BP-20, kind three, queue
one and a signed double-word quotient to 371D:01F9. The quotient divides
the signed double word at the pointer DS:9C10 by the sign-extended byte
DS:9C0B. The caller has no own divisor-zero or quotient-overflow check.
No delay duration or successful scheduling follows solely from these
arguments.

For queue one/kind three, 01F9 requires signed queue count below its
capacity. Its insertion loop advances past records whose signed
double-word time is at most the supplied time, so an admitted ordered
nonnegative-count queue inserts after equal times. An insertion before existing records
requests a shift through 289B:01DC; that child's complete move/lifetime
contract remains separate. The append case needs no such shift.

The queue then stores the supplied time and kind and copies exactly
twelve bytes from the supplied far payload to record +5 through helper
1000:0452, before incrementing queue count. The copy helper clears
direction, copies words and an optional odd byte, restores DS/SI/DI/BP
and removes its eight argument bytes. On this call twelve bytes mean six
word copies, with no odd tail. Valid disjoint source/destination/stack
spans and retained queue identity remain required.

The removal caller explicitly assigns only the first three bytes of
that twelve-byte payload. Its complete body has no own assignments to
the remaining nine payload bytes. They are not documented as zero or
given invented meanings. This copying observation alone establishes
neither a visible error nor how the kind-three consumer uses the tail.

## Interpretation

This removes the local compaction/rescan, count-boundary flag clear and
deferred argument/copy ordering from Q-PARTY-116. It does not prove a
constructed row was counted or survived to the match, cell/redraw child
effects, native clock/queue storage, insertion-shift preservation or the
later event consumer. FND-ITEM-066's conditional spare-row capacity is
distinct from actual count/base/slot lifetime. No native game defect or
party-slot release is inferred.

## Alternatives

Advancing the scan after removal would skip the shifted replacement and
contradicts the matching branch. Calling the boundary flag clear a whole
row reset overstates its one-byte store. Treating the deferred packet as
fully initialized or a guaranteed queued unload omits the copy extent,
queue gates and child/storage conditions. No original was run, complete
reading declared or parity status promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the four location ranges above in order. Retained reports under
GAME_DIR/analysis/item-removal-20261104/ are
effect-row-removal-contract.txt and effect-row-removal-contract-repeat.txt,
both SHA-256
04d42934ccb281dc374b5267abc5239d90c5b99984b431a293c7c47f379e51c5.
Check the helper's old/new count distinction, match/nonmatch successors,
post-child base reread, payload source extent and insertion count ordering
separately. Retain original listings and memory only in approved local stores.

---
id: FND-ITEM-075
title: Due kind-three events snapshot the slot before dequeue and reenter removal with zero independently of the payload mode byte
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:0334..371D:04A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:04B3..371D:04CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:01DC..289B:02EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0002C879..0x0002C883
tool: tools/research/exec-census/resident_listing.py and tools/evidence/report.mjs table at e0425ea2; Python 3.12.10, Capstone 5.0.7, Node.js 24.15.0; composition with FND-ITEM-073/074 and FND-COMBAT-060
environment: null
---

## Observation

Q-PARTY-116 retains the deferred kind-three consumer after FND-ITEM-073's
payload preparation and FND-ITEM-074's insertion shift. FND-COMBAT-060
identifies the two-queue pump. This follows its exact snapshot, dequeue
and selected handler arguments rather than assuming that kind three is
an unconditional immediate unload. Addresses/offsets are hexadecimal;
record lengths and counts are decimal.

### Due-head snapshot precedes movement

371D:0334 calls the rectangle-reset child, then processes queues zero and
one. For each it reads the double word through that queue's clock pointer
and passes the value by argument to 036E. After both calls it requests
the rectangle-redraw child. These children's effects and native clock
outcomes remain separate obligations.

036E tests the current queue count before reading its head timestamp.
Under admitted nonnegative count, zero returns; otherwise a signed
timestamp greater than the supplied cutoff stops this queue. A due head
latches its kind byte and copies exactly twelve payload bytes from row
+5 into SS:BP-12 before any queue move. The complete snapshot occupies
BP-12 through BP-7; it is separate from the saved head time at BP-6
through BP-3 and the kind byte at BP-1. The fixed copy helper and mover
preserve DS/SI/DI/BP under the disjoint-stack conditions already recorded.
Thus movement of queue storage does not itself change this detached
payload or kind, provided there is no alias or intervening external writer.

### Dequeue includes one boundary record

For positive count n it requests 17*n bytes from row one to row zero,
then decrements the count. The source includes inactive row n, not just
the n-1 surviving active successors. Deleting a sole active event still
requests seventeen bytes from row one. Source capacity for the boundary
record is therefore an explicit condition.

With FND-ITEM-074's retained initial pointers and capacities the largest
lengths are 850 bytes for queue one and 1700 for queue zero. Destination
is seventeen bytes below source, selecting forward copying. The mover
normalizes the addresses, copies the even word portion forward, and, for
an odd length, copies the remaining byte with no backward-tail pointer
adjustment. These spans need no second FA00-byte chunk. Under valid
backing this preserves the surviving active records' order. Only after
the move does the persistent count decrement occur.

At full nominal queue-one count, the extra source row is the first row
of the adjacent queue-zero span. It is read, not written by this move;
the destination stays in queue one. At full queue-zero count the extra
source row lies beyond its nominal span. No useful boundary contents or
native lifetime follow from the initial pointer layout. The copied
boundary becomes inactive after decrement.

### Selected kind-three consumer

The caller passes the latched time and kind, the far pointer to the
stack snapshot and a separate literal zero byte to 044D. The dispatch
gate subtracts three and admits unsigned indices zero through four.
A complete five-word table read maps kind three to 046C; kind four
maps to the default return, providing a distinct table control.

046C forwards the separate final argument byte and snapshot pointer to
04B3. That wrapper reads only the snapshot's first word as slot and
calls 371D:050F(slot, final_byte). On this queue-consumer route that byte
is zero. The payload's byte two, initialized to two by FND-ITEM-073's
removal caller, is not used as the final argument. Neither that byte nor
the remaining nine copied payload bytes is read on this selected handler
path before the removal call. Their copied presence is not evidence of
an input to removal.

Consequently a retained event prepared by that caller reenters removal
with the saved slot and zero. FND-PARTY-262 and FND-ITEM-073 then supply its
conditional behavior: up to ten matching rows are removed, with another
deferred request after positive removals, or an immediate unload request
when none are removed. This does not prove those native matches, requests
or their children succeed. No new slot bound is inserted by this wrapper.

The dispatcher sets AL to one after the selected handler returns, but
036E does not test it. It rereads the queue head/count and compares against
the same cutoff argument. A newly queued event can be examined in that
same call if it reaches a due head; there is no fresh clock read inside
this loop and no unconditional same-call completion claim.

## Interpretation

This removes local kind-three snapshot/dequeue/dispatch effects from
Q-PARTY-116 and explains which prepared payload fields the selected
consumer actually uses. The remaining obligations are constructor/use-time
row and slot identity, native clock/queue/boundary storage, cell/redraw
and unload children, other writers and handler preservation. The old
summary of kind three as an object-unload event is refined by this
conditional removal/requeue path, without asserting a native defect.

## Alternatives

Using payload byte two as the removal mode confuses it with a separate
literal-zero argument. Dispatching from the moved queue head would lose
the preceding detached snapshot. A shift of 17*(n-1) omits the actual
boundary read. A due event is not proof of immediate unload or completed
redraw. No original was run, complete reading declared or parity status
promoted.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with the four
code ranges above in order. Under GAME_DIR/analysis/item-removal-20261104/,
effect-queue-consumer-contract.txt and its -repeat pair have SHA-256
8debfbeedc89fbd6f5090e5a96a7c1c714b061f1105e1d689a50ea9709a7bfa3.

Use report.mjs table at file 2C879, five stride-two width-two target rows,
bounded by 371D:045B..0467. Local effect-kind-dispatch-controls.json
records the query; effect-kind-dispatch.json and its -repeat pair have
SHA-256 3c839a01601196209f06a2dd3d5a9e345f0533eb3dd973a3da3635bbf28accc1.
Retain listings, queue records and reports only in approved local stores.

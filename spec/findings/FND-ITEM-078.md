---
id: FND-ITEM-078
title: Unload removes a placement index before a flagged-row residual scan can bypass slot cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:07CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0AD1..31E0:0AFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:06F0..371D:071E
tool: tools/research/exec-census/resident_listing.py at a9b8b3a3; Python 3.12.10, Capstone 5.0.7; composition with FND-PARTY-213/262, FND-COMBAT-159 and FND-ITEM-073/076/077
environment: null
---

## Observation

Q-PARTY-116 retains unload effects after the removal/queue/redraw readings.
FND-PARTY-262 establishes when 371D:050F requests unload, not successful
slot release. This reads the initial unload/index-removal and flagged
residual gate; its later cleanup body and external child effects remain
separate. Addresses/offsets/masks are hexadecimal; counts are decimal.

### Placement index and slot identity differ

31E0:05BB takes its first word as placement index i, rejecting a negative
signed i or unsigned i>=DS:264E before selecting a row. This gate is not
a check on the object slot. For an admitted row it captures slot s from
row word +6 and row flag bit 20 in a local byte. It forms the object
record at DS:67BB+37*s using word arithmetic, without an own slot bound.
Readable row/object storage, valid count and preserved segment identity
are required.

The 371D:050F caller supplies its object's current entry-index word +1,
not its slot number directly. Its following arguments are one, a Boolean
equal to one when the removal's second byte is zero, and one. Thus the
ordinary zero-byte removal request is (entry_index,1,1,1). A stale entry
index can select another slot or fail the entry gate; no own comparison
in this prefix checks that the selected row still names the requesting
slot. Actual index/slot preservation remains Q-PARTY-116.

### Removal precedes residual counting

When the captured row is flagged and the third argument byte is nonzero,
the prefix can request presentation/timing children through the DS:143F
gate, clears that byte after their return, and prepares a rectangle from
the row/object geometry. These child effects and frame preservation are
not assumed. For a nonnegative slot with effect byte +0F nonzero, the
prefix also requests the two conditional cell-mask clears described by
FND-ITEM-076. Its adjacent-link updates use object words +21/+23 and skip
each neighbor store only for the corresponding FFFF sentinel. Neighbor
indices and aliasing require admission.

It then calls 289B:0094 with i, the current placed-table pointer, address
of persistent count DS:264E, presentation table DS:67BB and count 320.
FND-COMBAT-159 and FND-PARTY-213 establish the count decrement/compaction
and presentation-index adjustment. The caller rereads the new count/base,
clears only byte +5 of the row at that count boundary and clamps DS:656F
down to the new count when it was greater. None of this waits for later
object-slot cleanup.

Only when the captured row flag was nonzero does the prefix initialize
a residual counter and scan the current counted prefix. It counts rows
whose flag byte +5 has bit 20 and whose slot word +6 equals saved s.
This scan has no child call. It requires retained readable table/base/count
and disjoint stack storage; it does not search the inactive boundary row.

If the removed row was flagged and this residual count is positive,
07CB jumps to 0AD1, bypassing the later slot-cleanup body at 07CE onward
and its own eventual 044C call. At 0AD1, third byte exactly one requests
31E0:128C with the prepared rectangle, fourth word as page and final
literal one, then returns. Any other third byte returns without that
redraw request. FND-ITEM-077 supplies the redraw wrapper's separate gates.
No earlier child is proved unable to mutate or release a slot merely
because this own later cleanup route is bypassed.

An unflagged removed row bypasses the residual scan and does not receive
this protection. A flagged row with no remaining same-slot flagged row
also proceeds to the later cleanup body. This finding does not read that
body as an unconditional release: FND-PARTY-213 already identifies further
DS:265B and child gates before chunk deletion/slot release.

## Interpretation

This removes the unload entry's placement-index binding, remove-before-
rescan ordering and flagged residual bypass from Q-PARTY-116. It explains
why a request reaching the unload entry is not proof of object-slot
release, and why use-time entry-index identity matters independently of
the saved slot. Remaining work includes the later cleanup branches,
children, native row/slot/table/neighbor state and preservation.

## Alternatives

Treating the first argument as a slot would skip its placed-row lookup.
Applying the residual guard to every removed row ignores the captured
flag test. Counting before compaction would include a row already removed
by the actual order. No native unload, party-slot release, game defect,
complete reading or parity promotion is claimed.

## How to reproduce

Use approved DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py with -I and
the three location ranges above in order. Retained reports under
GAME_DIR/analysis/item-removal-20261104/ are effect-unload-flagged-gate.txt
and effect-unload-flagged-gate-repeat.txt, both SHA-256
4aa17726f0607780e8012110015ca1863864ebdeab8a0387dff6b92192174cd7.
Check the first argument's lookup, captured flag versus later object flag,
post-compaction scan bounds and direct 0AD1 exit separately. Keep listings,
source bytes and native memory only in approved local stores.

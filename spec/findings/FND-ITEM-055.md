---
id: FND-ITEM-055
title: The occupied-slot exchange continues after transfer results and bootstraps its temporary slot from a zero-result graph
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000773B4..0x00077514
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:03C1..2D40:0461
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C3F..2D40:0C92
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:288C..2D40:2B6A
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at c1265663; retained exchange reading of FND-ITEM-054; composition with FND-COMBAT-167 and FND-PARTY-119
environment: null
---

## Observation

Q-ITEM-003 retains occupied-target action effects after FND-ITEM-054's
empty-target wrapper. This reads complete overlay 189 +2A04, file
773B4..77514 hex, including its temporary-slot and result-handling order.
It does not establish an atomic native swap or an observed failure outcome.

### Temporary slot and first transfer

The helper takes logical position L. It calls 2D40:0453 and stores returned
T locally. Exactly 9999 exits immediately, without later projection or release.
Otherwise it projects current character 4E71:0B44 through +1688. For L below
18 it resolves the currently mapped target item's record and calls local
+332B with character and record before continuing. That child's full effects
remain a condition, not a no-operation assumption.

0453 pops the DS:56F1 free-list head through 0C3F. A nonempty pop advances
the head, increments DS:55C8 and clears the popped slot's paired word to
zero. It does not assign a new kind byte. Under FND-PARTY-119's initialized,
uncorrupted ordinary-list contract, T is in 48..318 and retains free kind
zero until another writer changes it. Neither the exchange nor the allocator
establishes arbitrary free-list integrity or preservation through projection.

The exchange enumerates T into an 800-byte SS-local graph G. It then calls
2D40:288C with current character graph as source, current map[L] as source
entry, requested position -1, G as target graph, target entry zero and target
field -1. The final zero/-1 pair comes from a packed FFFF0000 doubleword
push; it is not one unsigned mode value.

### Conditional zero-result bootstrap

If T still has kind zero at that first enumeration, FND-COMBAT-167's invalid-
kind branch writes T to G word +6, returns count zero, and leaves root fields
other than its step/count/identity potentially uninitialized. Thus this is
not assumed to be a positive kind-one graph before the first transfer.

For the first transfer's requested -1, target count zero and admitted storage,
FND-ITEM-044's position helper returns output -1 in either root-kind case:
non-kind-two accepts that negative request, while kind two scans no entries
and returns the ordinary requested word. The unknown root kind does not
supply an indexed descendant in this class. The generic transfer's map
builder forces map[0] to root word +6 even at count zero (FND-COMBAT-167).

Target field -1 bypasses its target-field getter/setter branch. It requires
target entry zero and signed target count at most one, both supplied here,
then loads its destination handle from G word +6, namely T. At 29F9 it
restores the selected source entry's kind byte and record number into T's
three-byte metadata through 286C. With an admitted kind-one source, this
changes T from kind zero to kind one before later field/link requests.

This establishes a conditional metadata producer for the temporary slot;
it does not prove every subsequent splice or setter succeeds. It depends
on T retention, valid selected source, current-object/descriptor state,
returning children, DS/SS near-state binding and disjoint nonwrapping frames,
graphs, maps and copy destinations. Uninitialized G fields are not silently
assigned values; only the count/identity and the branch-independent -1
position result are used in this composition.

### Unchecked exchange sequence

The complete outer helper runs the following sequence after allocation:

1. Make the first direct transfer above. It executes OR AL,AL after return,
   then calls +2897 with L without branching on those flags. This is
   FND-ITEM-054's proposed-item placement wrapper, with its own transfer
   check, source rebuild and later child effects.
2. Rebuild DS:1A32 at 5072:0040 and rebuild T into G. Scan G for the first
   kind-one entry. The scan may stop at its count without finding one;
   there is no own missing-kind-one rejection before the next transfer.
3. Make a second direct transfer: source G and that scanned index,
   requested position -1, target graph 5072:0040, target entry zero and
   target field -1. Again OR AL,AL is followed by graph rebuilding, not
   a branch on transfer success.
4. Rebuild DS:1A32 again, scan for its first kind-one entry, and store the
   stopping index in DS:1A34 even when none was found. Call local +1893
   with L, then release T through 2D40:03C1.

The first target graph construction's returned count is not tested by the
outer helper. Later enumerations and their contents, rather than saved
success flags, determine the scans. The second scan uses near DS access to
G despite its SS-local construction, so segment identity remains material.
The helper's 322-hex-byte frame reserves 800 bytes for G and a separate
word for T; positive enumeration's maximum own 791-byte span fits that
allocation under FND-COMBAT-167's admitted count/storage conditions.

Under a retained ordinary T in 48..318, 03C1 selects DS:56F1, calls 0CC2
and then overlay 188 +1876. FND-CONFIG-147 supplies those local contracts:
0CC2 may clear the kind and reinsert T, and +1876 clears qualifying entries
in the separate nineteen-byte table. This release request is reached after
both direct transfers irrespective of their AL results, provided the
intervening calls return normally. It is not a recursive item-removal request
through 2410 and cannot be assumed to undo earlier record/link mutations.

## Interpretation

This removes the outer exchange order, direct-transfer failure branching
and temporary zero-graph bootstrap from Q-ITEM-003's unknown local behavior.
The code supports a sequence of attempted moves with a temporary handle;
it does not locally enforce all-or-nothing success or rollback.

Actual incoming action/target admission, +332B and other children, free-list
and T lifetime, metadata/link/record ownership, missing-source cases,
DS/SS binding and all generic transfer/copy/store effects remain open.
Q-ITEM-004/013 retain related graph/storage contracts. No native item loss,
release defect, confidence, complete-reading or parity promotion is claimed.

## Alternatives

Branching out after either direct transfer returns zero would change this
outer helper. Treating a freshly popped T as already kind one adds a store
the allocator does not perform. Treating count-zero G as wholly empty or
fully initialized loses its explicitly written identity and retained fields.
Treating final release as rollback substitutes an unobserved ownership effect
for the actual slot free-list and auxiliary-table requests.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Reuse placement-list-transfer.txt under
GAME_DIR/analysis/item-removal-20261104/, regenerated with overlay_listing.py
descriptor 189 over 773B4..77514 hex. Trace every direct transfer return
through the next calls, and split the packed zero/-1 argument words.

Use resident_listing.py for 2D40:03C1..0461 and 2D40:0C3F..0C92;
placement-temporary-slot.txt includes the former plus a diagnostic tail,
while placement-temporary-pop.txt contains the complete pop. Reuse
placement-generic-transfer-full.txt, decoded from 2D40:288C, for the negative-
field branch 29C6..29F9 and metadata writer. Compose the invalid-kind graph
exit and forced root map entry with FND-COMBAT-167. Retain every stated
storage/return/ownership condition. No original code, graph/record fixture,
resource payload or image is committed or executed.

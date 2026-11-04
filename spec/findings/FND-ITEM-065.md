---
id: FND-ITEM-065
title: Effect-row construction prepares the removal flag before insertion but counted publication remains conditional
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:03CA..31E0:044C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0004..289B:0094
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:00F0..289B:01DC
tool: tools/research/exec-census/resident_listing.py at b47921a5; composition with FND-MAGIC-011/019, FND-COMBAT-159 and FND-ITEM-063/064
environment: null
---

## Observation

Q-PARTY-109 retains the production and lifetime of the flagged row required
by 371D:050F. FND-ITEM-063 shows that selection alone does not test that
flag, and FND-ITEM-064 removes the selected final filter as a mutator.
FND-MAGIC-011/019 already identify 31E0:03CA as an effect-placement producer.
This reads its complete count/write ordering and its insertion wrapper,
reusing FND-COMBAT-159's lower insertion and backpointer-adjustment contract.

### Prepared row and pre-child writes

Let n be unsigned DS:264E at entry and s the slot argument. With n at least
1500, 31E0:03CA returns AX=FFFF without persistent writes or child calls.
Otherwise it prepares an eight-byte row in its stack frame: the caller's
two coordinate words, byte argument, flags byte and slot word. The flags
byte is 68 hex OR the low three bits of the final argument. In particular,
bits 08, 20 and 40 are always set in this prepared row.

It then increments persistent DS:264E to N=n+1, sets bit 02 of byte zero
of slot s's 37-byte record, and sets bit 40 in byte five of the table row
selected by that slot's current word +01. This last store does not set
bit 20 in the old selected row. There is no local validation of the slot
or old entry index before these writes; valid nonwrapping spans, pointer
identity and separate frames are required.

The body calls 289B:0154 with position argument N, its prepared far SS row,
the current table pointer, count N, DS:67BB and record count 320. It returns
the child's AX unchanged. It neither tests that result nor rolls back the
already incremented persistent count and flag stores. It has no direct
store of that AX into slot s's entry-index word.

### Insertion uses a local count copy

289B:0154 searches its supplied count N using the prepared row as key.
The complete 289B:00F0 search compares the first coordinate word after
the second, both unsigned; a tie uses signed byte four. With an admitted
N-row span and intact frame it returns a position J in 0..N. Sortedness is
needed for an ordered insertion interpretation, not supplied by the caller.
The flags and slot word are not part of the key comparisons.

The wrapper tests J unsigned against its position argument N. Since J<=N,
the branch that removes/moves an existing entry is not selected on this
constructor's admitted inputs. It instead copies N to a local word and
passes that local word's near address to 289B:0004. Actual DS/SS identity
must make the near address designate that local word.

The leaf increments that local count to N+1, searches N rows again and
copies the prepared eight bytes to the returned position K. Under stable
search inputs K equals J. If K<N it shifts rows [K,N) right by one before
the copy and adjusts presentation-record indices as FND-COMBAT-159 records.
If K=N it performs no shift and copies at index N. The persistent DS:264E
is not passed by address on this route and remains N under separated
storage; the local count increment does not make it N+1.

Consequently the prepared flagged row is within the persistent counted
prefix exactly when K<N. The normal nonnegative returned index alone does
not prove that condition. The search also reads N rows even though the
constructor entered with only n=N-1 counted rows, so the additional row's
readability/content and table order require separate admission. The leaf's
shift/copy needs destination capacity through index N. This is a bounded
memory-footprint requirement, not an observed native failure or a claim
about the actual allocated extent.

### Consequence for a later selected target

This producer supplies a concrete bit-20 row source; it does not by itself
prove that every target selected by 193 +28A4 retains such a row in the
counted prefix. For an admitted constructor call with K<N, preserved
prepared source and separate storage, the copied row has bit 20 and its
slot word is s. A later removal needs that row to remain counted and flagged,
with the same table and slot identity. Other creation/copy/restore paths
and independent mutations remain outside this local contract.

FND-MAGIC-019's effect-slot byte producer and FND-EXE-606's sample-placement
calls identify related creation paths; they do not prove all of this
publication, capacity, ordering or use-time preservation. FND-PARTY-262's
zero-count unload branch therefore cannot be removed solely because a
constructor prepares bit 20. No actual uncounted insertion, lost effect
or erroneous unload was observed.

## Interpretation

The missing producer is no longer an unspecified flag assignment: the
prepared effect row explicitly sets bit 20, while the old indexed row gets
only bit 40. Q-PARTY-109 retains actual constructor/slot/index/count and
spare-row admission, counted insertion position and table order/capacity,
other producers and row lifetime through removal. Q-PARTY-077's broader
DUAL ownership question remains. This does not promote a rule, parity or
complete reading or diagnose a native game defect.

## Alternatives

Equating the prepared stack row with an already published table row would
skip the insertion call. Equating its local count update with DS:264E would
ignore the supplied near pointer. Setting bit 40 in the old row is not
setting bit 20. A nonnegative AX need not lie below the persistent count.
Conversely, the extra-row footprint alone does not show invalid storage,
an uncounted return or a native gameplay error.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run resident_listing.py for
31E0:03CA..31E0:044C, 289B:0154..289B:01DC,
289B:00F0..289B:0154 and 289B:0004..289B:0094.
Retained entry-based listings are placement-effect-row-constructor.txt,
placement-effect-row-insert.txt, placement-effect-row-search.txt and
placement-effect-row-insert-leaf.txt under
GAME_DIR/analysis/item-removal-20261104/.

Bind the nine constructor pushes to the insertion wrapper's far-call
parameters, keeping the persistent N distinct from the local count copy.
Check the search's shrinking lower/upper intervals and end position,
the unsigned J<=N branch, source/destination copy extents and final AX.
Reuse FND-COMBAT-159's adjustment conditions and FND-ITEM-063/064's selected
scanner/filter route. No original code, memory or record fixture is
committed or executed; no native capacity or ordering is assumed.

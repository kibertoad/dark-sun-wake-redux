---
id: FND-ITEM-096
title: Ordinary placement increments count before insertion and directly publishes the returned entry index
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:03CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:444A..31E0:4474
tool: tools/research/exec-census/resident_listing.py at 7630c9ae; Python 3.12.10, Capstone 5.0.7; composition with FND-ACTOR-004, FND-COMBAT-107/149/159 and FND-ITEM-065
environment: null
---

## Observation

Q-PARTY-116 retains other count/key/slot writers before effect use.
FND-ACTOR-004 identifies ordinary placement 31E0:0121 as another count
producer. This records its own prepared key, count/index stores and
coordinate helper, keeping lower loading and later children separate.
Addresses, field offsets and masks are hexadecimal; other counts are decimal.

### Capacity and lower loading

At current unsigned DS:264E at least 1500 it requests 31E0:0D09 with
the low byte of its category argument, zero and one. It rereads count and
returns FFFF if still at least 1500. Otherwise it prepares flags byte
48 OR the category's low three bits, and calls 31E0:0EFF with object
number, the then-current count as entry index, requested slot, those low
category bits and the supplied mode byte. FND-COMBAT-149 records lower
loading's own allocation/eviction and continuation effects.

Exact returned AX FFFF returns FFFF. Every other word is retained as
the slot without a separate local upper bound, selecting the 37-byte
record at DS:67BB+37*slot with word arithmetic. Count is not captured as
an invariant across this lower call or rechecked against 1500 afterward.

### Prepared key and anchor writes

Mode mask 08 selects prepared x as input x plus sign-extended object
byte +7, and prepared y as input y plus object word +8 plus the
sign-extended supplied height byte. These additions keep word width.
Without that mask, slot kind one at 4F49:0C33+3*slot requests
572F:0039 with far addresses of the two input-coordinate words before
capturing them; other kinds capture those words directly. That child's
actual writes and input/frame preservation remain requirements.

Prepared byte +4 is the supplied height's low byte on both routes;
prepared byte +5 is the flags above and word +6 the returned slot.
Thus this producer does not itself force mask 20 in its prepared row,
unlike FND-ITEM-065's effect constructor. Actual row fields can still be
changed by children or later producers.

It increments the current persistent DS:264E at word width, writes
prepared x/y/height to object fields +0A/+0C/+0E, then calls 31E0:444A
with that object's far pointer. This leaf writes object word +3 as
word +0A minus signed byte +7, and word +5 as word +0C minus word +8
minus signed byte +0E, with word-width subtraction. It has no child call
or own pointer/span check and restores BP; ES/scratch are not preserved.
Under intact distinct storage it changes anchor fields, not count or row keys.

### Insertion and direct index publication

It calls 289B:0154 with the object's current entry-index word +1,
the prepared stack row, current placed-table pointer and current persistent
count, plus DS:67BB and 320 records. The position argument is therefore
an object field, not FND-ITEM-065's explicit new-count argument. Its relation
to the searched count depends on lower loading and intervening preservation.
The effect-constructor-specific no-removal conclusion cannot simply be
imported into this producer's call.

Returned AX is immediately stored to object word +1 without a result,
counted-index or slot-identity test. Persistent count and prior object
writes have no own rollback. FND-COMBAT-159 supplies the distinct lower
insertion/removal backpointer adjustment contract; it does not admit every
input or replace this direct store. The routine later returns the retained
slot word, rather than the insertion index, on its normal continuation.

Mode mask 04 and slot kind two select the later service/script path;
FND-COMBAT-107 already records its trigger reentry and preservation limits.
That path can also request recursive placement under its later gates.
Those child effects and recursive input admission remain outside this local
prefix claim. Final SI/DI/BP restores do not prove their intermediate values
or the table/count survived earlier children unchanged.

## Interpretation

This removes ordinary placement's local category/key/anchor/count/direct
index publication from Q-PARTY-116's unspecified producer obligations.
Native lower-load results, coordinate-adjustment effects, object entry-index
and count identity at insertion, later services/recursion and row lifetime
remain. Ordinary placement is not interchangeable with the effect producer
for either flag seeds or returned-index handling.

## Alternatives

Treating the capacity check as a post-load invariant ignores the intervening
child. Treating the returned slot as the insertion index confuses two results.
Treating category flags as forced effect flags adds mask 20. Treating final
register restores as child preservation ignores intermediate use. No native
defect, complete-reading declaration or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the two half-open location ranges in order.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-ordinary-placement-contract.txt and its -repeat pair, both SHA-256
f3f908ddaa39ca08ca8b032d2565982b719c476040ba193172eb422ada6ac0c6.
Reuse the cited lower-loader/insertion/script findings with their limits.
Keep reports local. No original game or isolated function was executed.

---
id: FND-ITEM-015
title: Deletion pool helpers have bounded conditional writes while the generic setter retains a computed-destination alias obligation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C31..2D40:0D59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:012A..1AA0:0209
tool: tools/research/exec-census/resident_listing.py at 1e9ab791; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

Q-ITEM-004 asks whether the callees reached by FND-ITEM-014 preserve the
current-object word and descriptor bytes its later child gates reread.
FND-PARTY-119 already records the pool operations, and FND-PARTY-221 the
setter's width behavior. This reading identifies their complete local write
footprints and distinguishes direct stores from computed destinations.
The source has length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, relocated to load segment 1000.
No original code was executed.

The installed effects reporter provides bounded modeled paths, including
explicit stopped paths and assumptions. This question instead requires the
full instruction-level store expressions on all local branches, including
the unchecked indices and computed far destination. The existing listing
reader supplies those expressions without introducing preservation models
or interpreting a capped path traversal as a complete admission proof.

### Pool callees

The complete 0C31 wrapper passes near head address 56EF to 0C3F.
The latter has no child call. Exact head value 9999 returns without pool
writes. Otherwise it increments DS:55C6 for head address 56EF, or DS:55C8
for 56F1, takes the old head index, writes that object's link into the head,
and clears the object's metadata word +1. It does not clear its kind byte.
Object selection uses the low word of three times the index, at segment
4F49 with base offset 0C33. No local head-index admission check is present.

The complete 0C92 wrapper compares its argument as signed against 320 and
520. Within that interval it calls 0CC2 with head address 56EF. Otherwise
it clears the kind byte at the same index-derived metadata address, with
no independent nonnegative or table-capacity check on that branch.

The complete 0CC2 body has no child call. It rejects signed indices below
five, at least 520, and exactly 319 before changing pool state. Its list
walk reads metadata links until 9999 or a matching index. On a match it
performs no pool write; the walk still reads that matching node's link.
There is no local cycle detection or bound on a nonmatching traversal.
On insertion it decrements the corresponding counter for heads 56EF or
56F1, clears the selected kind byte, saves the old head, writes the selected
index to the head, and writes the saved head into the selected link word.

These bodies do not assign DS. Apart from stack traffic, their writes are
the stated counters, supplied head word and index-derived metadata bytes.
For these deletion call sites the supplied head is 56EF. With incoming DS
57E0, metadata indices within zero through 523, and a valid disjoint stack,
those writes cannot overlap DS:60EB or descriptor bytes at nonwrapping
DS offsets 60ED and above. Wrapped descriptor selectors are not admitted
by this result. This conditional result follows from their actual addresses,
not from treating different segment names as automatically disjoint.
It does not admit arbitrary indices or list chains, or promise termination.

### Generic setter

The complete setter has only two child targets: resolver 2D40:06C4 and
copy helper 1000:3F5A. Reuse FND-CONFIG-140 for the resolver's read-only
local body and FND-CONFIG-137/182 for the copy helper's complete body,
forward byte-counted copy, lack of capacity/overlap validation and restoration
of DS. The setter does not call the getter's temporary-object converter.

Its own non-stack stores are the descriptor type byte at 4C0E:0005,
the result pointer at DS:40BC and status word at DS:40C4. Invalid-object
exits set status -2; a missing field offset sets -1; the returning copy
continuation records the computed destination and sets zero.

For descriptor types seven and eight, used by the reparenting branch,
the source is the address of the setter's doubleword argument on SS.
The byte count is the zero-extended table byte at DS:(040C + type);
the copy destination is the resolved field pointer. FND-PARTY-221 records
the general type-width issue. A type-six or type-at-least-128 branch instead
treats the doubleword argument as a source pointer and supplies its distinct
count; neither branch is a converter call.

The computed destination uses a kind-indexed base pointer, the low word
of signed record-index times stride, and the resolved field offset, with
16-bit offset additions. A rejected negative object is not a proof that
every accepted kind, record index, field selector or destination is valid.
Consequently the absence of a direct store to DS:60EB or DS:60ED does not
prove their preservation: an admitted destination span must be disjoint
from that state and the caller's graph, map and live stack. Descriptor
stability also requires that disjointness throughout each copy, rather
than merely the type byte read before it.

## Interpretation

The pool callees' conditional write set and the setter's complete local
callee set are settled. They remove the need for another generic search
for an undiscovered direct current-object assignment in these bodies.
The remaining preservation proof is input-specific: admitted pool chains
and indices, stack identity, kind/record/field tables and the resulting
copy spans. FND-ITEM-014's unchecked setter status remains relevant.
Other deletion-kind callees and graph/map storage admission remain open.
No complete_reading or confidence promotion follows from this result.

## Alternatives

- Pool allocation does not initialize the selected kind byte; it clears
  only the metadata link word after updating the head.
- The setter does not reach the getter's temporary-object converter.
  Its computed copy still prevents an unconditional preservation claim.
- Signed gates in the release wrapper do not reject all invalid indices;
  the outside-pool kind-byte store remains index-derived.
- A returning local path does not establish a well-formed or terminating
  free-list traversal for every caller.

## How to reproduce

Use the locked evidence Python and verified installed <dsun>:

```text
python -I tools/research/exec-census/resident_listing.py <dsun> 2D40:0C31..2D40:0D59 1AA0:012A..1AA0:0209
```

These bounds cover the complete inventory-declared bodies in
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv. Retain UTF-8 output only at
GAME_DIR/analysis/item-removal-20261104/callee-preservation.txt;
its SHA-256 is
618d211b168deaf75090dff41d1d3c95a7495f68662f24f6d2583d7e9753784e.
Reuse the cited findings for the two already-read children. No instruction
listing, original bytes or extracted records are committed.

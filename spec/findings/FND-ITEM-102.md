---
id: FND-ITEM-102
title: Placement collision adjustment has a finite word-coordinate walk under a stable bounded placed table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007D0F5..0x0007D212
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:01AC..31E0:01DD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0226..31E0:022F
tool: tools/research/exec-census/overlay_listing.py at 81a93f0b; Python 3.12.10, Capstone 5.0.7; existing caller report and FND-EXE-622/FND-ITEM-096 composition
environment: null
---

## Observation

Q-PARTY-121 retains the ordinary placement coordinate child. FND-EXE-622
already identifies overlay 191 +0F05, trampoline 572F:0039, and its
collision-adjustment purpose. This reads its complete bounded body for
search progress, wrapped movement and its actual write footprint, without
repeating the unrelated overlay routines in that earlier finding.
Addresses and field offsets are hexadecimal; row counts are decimal.

### Search and zero-count boundary

Two far arguments point to x and y words. Each outer iteration reads
DS:264E into the upper search index, subtracts one at word width, and
starts the lower index at zero. It chooses the unsigned midpoint and
forms an eight-byte row address from DS:67B7/67B9, with word-width
index multiplication and offset addition. It compares row y, then x,
with the pointed-to words, using unsigned comparisons for ordering.

For a stable count m in 1..1500, intact stack/segments and valid
nonwrapping row spans, both search indices stay in 0..m-1. An interior
midpoint becomes the lower or upper index; when midpoint equals the
lower endpoint the lower-side branch selects the upper endpoint, and
when it equals the upper endpoint the other branch collapses the lower
to that endpoint. Each continuing interval therefore shrinks or reaches
an endpoint exit. Equality exits the search as a collision. A final
explicit x/y equality check determines the collision result. Sortedness
by unsigned y then x makes this an exhaustive lookup; it is not needed
to show index progress or that a reported collision equals a stored key.

There is no zero-count early return. Count zero makes the initial upper
index FFFF, midpoint 7FFF and the multiplied word offset FFF8. This
access is outside the admitted counted-prefix argument, even if backing
memory happens to exist. No native zero-count call is established here.
Larger counts, changing count/table storage and wrapped row spans are
outside the bounded proof, not silently treated as valid placement state.

### Wrapped movement and termination

The two step words start at zero and persist across searches. On a
collision, equality of the steps subtracts four from the x step;
inequality subtracts four from the y step. It adds the x step to x,
then the y step to y, through the respective far pointers, at word
width, and starts another search. There is no independent iteration cap.

For distinct coordinate words that do not overlap table, pointer or
stack storage, let the initial pair be (x0,y0). After 2k adjustments
the pair is (x0-4k(k+1), y0-4k^2); after 2k+1 it is
(x0-4(k+1)^2, y0-4k(k+1)), each coordinate reduced modulo 65536.
These formulas follow by summing the two steps per pair of adjustments.

The first 1501 pairs, including the initial one, are distinct. Within
either parity their x-minus-y differences distinguish k modulo 16384.
Across parities equal differences require comparing adjustment 2k with
2k-1; those pairs differ by four times k in both coordinates, nonzero
for 1<=k<=750. A stable table of m<=1500 rows cannot contain every one
of the first m+1 distinct coordinate pairs. Because every reported
collision equals a table key, the outer loop returns after at most m
adjustments in this domain. Missing a key in an unsorted table can end
it earlier; sortedness is still required to interpret the returned point
as free of all table keys. This is a conditional finite argument, not
a claim that the actual loader preserves its count or storage.

### Caller and state footprint

The helper has no call, interrupt, string instruction or direction-flag
write. Apart from its own frame and saved registers, its only writes are
the two pointed-to coordinate words. It reads the placed rows and global
count/table pointer without directly writing them. It restores SI, DI
and BP; other scratch registers and ES are not generally preserved.
With admitted nonoverlapping backing it preserves the caller's placed
count/table/slot-index state and incoming DF. It needs no DS=SS equality
to reach the coordinates: each access loads its supplied far pointer.

FND-ITEM-096's caller passes separate SS-based addresses of its two
coordinate argument words at 31E0:01BF..01C9, and ignores the return.
The call is reached only with mode mask 08 clear and current slot kind
one. Those words become the prepared row coordinates afterward.
FND-ITEM-098's selected producer supplies mode one, so mask 08 is clear;
actual returned slot kind and its backing still require admission.
The same mode also skips ordinary placement's later mask-04 service/
script branch under unchanged argument storage (31E0:0226..022F).
This does not skip lower loading or the slot producer's later immediate,
cell and queue calls.

## Interpretation

This removes the coordinate child's unread local effects, bounded search
progress and conditional outer-loop termination from Q-PARTY-121. Under
the stated disjoint 1..1500-row domain it is not another unknown child
capable of directly changing the placed count or slot index, and its
far coordinate arguments do not require a near-pointer DS/SS assumption.

Remaining evidence is the actual count and table/row backing at this
call after lower loading, slot-kind admission, pointer/frame alias
exclusion, source/lower-load preservation and the separately queued
immediate/deferred children. The pre-load capacity check does not supply
a post-load count invariant. No whole-entry complete reading, native
termination guarantee, defect or parity promotion follows.

## Alternatives

- The coordinate child needs another transitive helper reading: its full
  bounded body has no child call.
- A fixed coordinate step or an explicit loop cap guarantees termination:
  the steps accumulate and wrap; the finite argument uses distinct pairs
  and a stable bounded table.
- Zero count skips the search: the count is decremented before a guard.
- Same-offset coordinate and stack accesses require DS=SS: the caller
  passes SS explicitly and the helper uses each far pointer.
- Mode one reaches ordinary placement's later mask-04 script branch:
  that bit is clear under unchanged mode storage. Other loader/caller
  children remain reachable.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I.
Run overlay_listing.py for descriptor 191 and file range 7D0F5 7D212.
The two retained reports in GAME_DIR/analysis/item-removal-20261104/ are
effect-coordinate-collision-contract.txt and its -repeat pair, both
SHA-256 10c288eab1910f054e65412d3bf3530dca30f5889508ec92dd7fb94f7f310914.
Reuse FND-ITEM-096's effect-ordinary-placement-contract.txt for the caller
and local mode gates, and FND-ITEM-098 for the selected producer's mode.
Check the step sums and distinctness argument above with word reductions;
they do not depend on an observed native coordinate sample. Keep all
original-derived reports local. No original game or emulated call ran.

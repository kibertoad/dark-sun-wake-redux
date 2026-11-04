---
id: FND-COMBAT-165
title: Admitted deletion from root zero excludes reparenting and preserves cleared temporary kind through its finite recursive release calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:248D..2D40:27AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0C92..2D40:0D59
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0EB4..2D40:0EF1
tool: tools/research/exec-census/resident_listing.py at 05c7cb64; Python 3.12.10; Capstone 5.0.7; composition with FND-ITEM-013/014/015 and FND-COMBAT-164
environment: null
---

## Observation

FND-COMBAT-164 leaves later recursive effects after root kind release
unadmitted. This reading extends its childless deletion case to a finite
rooted graph class. It starts at 248D with graph and map already built;
enumeration and map construction are not included in the preservation result.

### Admitted class

The selected index is zero; graph entry zero has kind two and map[0] is
319. The signed graph count is stable from one through 79. Every other
entry has one parent index from zero through its own index minus one.
Entry kinds are one through five. Map entries are 9999 or valid indices
from zero through 523. At analysis load segment 1000, DS and SS both
denote resident segment 57E0, admitting the near-map storage identity.
Graph, map and stack are valid, separate and stable through record/pool
writes. Stable record bases, field selectors and selected record spans are valid
and nonwrapping; their writes do not overlap the graph, map, live stack,
descriptor state, base-pointer fields, temporary kind byte or presentation index. Pool chains
reached by release are admitted, acyclic and end at 9999. Stack capacity
is sufficient for the bounded recursion. These are conditions, not a
claim about all original-produced graphs.

### Why reparenting is unreachable

FND-ITEM-014 shows that a processed kind-one/two/three/five entry releases
its kind-specific state and object index, then determines its parent p.
Root index zero sets p to -1, skips its parent-field request and stores
9999 into map[0] before either child pass.

For a recursive call on child j, the caller requires that child's parent
byte equal the caller's index and map[j] differ from 9999. The caller has
already stored 9999 into its own map entry. Stable graph/map storage
therefore makes the child's parent lookup see 9999 and select p = -1 too.
This is an induction from the root, not an assumption that every caller
of 248D has a missing parent.

The reparenting pass requires signed p at least zero before it reaches
its parent field type, allocator or setter calls. With p = -1 it skips
those paths for every recursively processed entry. Consequently the
286C metadata copy, 1AA0:012A setter, 0C31 allocator and local +2730
non-returning reparenting branch are not reached in this class. No graph
parent/field rewrite from reparenting occurs.

Kind four returns before map clearing or child passes. Its descendants
are not recursively processed through it. A child whose map is already
9999 is also skipped. The result is not unconditional deletion of every
listed entry.

### Closed release call set and termination

The remaining kind-one branch calls 0EB4, whose complete no-child-call
body writes an item free-link, clears its identifier and changes the item
head, as FND-ITEM-013 records. The other admitted kind operations have no
child call: they conditionally clear their selected record fields.

For a released map index, the remaining object operation is 0C92. Its
direct-clear branch writes only a zero kind byte. Its pool branch calls
only 0CC2, whose complete leaf may clear a kind and update pool head/link
and counter words, as FND-ITEM-015 records. Its search returns on a
matching index or the terminal 9999. With admitted finite acyclic chains,
inserting an absent index at the head or retaining an existing match
preserves that finite chain property. No selected release operation
assigns a nonzero object kind.

Both child scans have stable finite count bounds. A recursive call always
uses j greater than the current index. With a stable single parent per
entry, an entry can be reached at most once from this root, giving at most
79 deletion calls. Valid kind-three record scans are separately bounded
by their constant count. The selected call chain therefore returns under
the stated storage/pool/stack conditions, without the generic setter,
allocation or reparenting failure loop. No selected body calls input,
window, resource or save dispatch.

FND-COMBAT-164's root prefix clears kind 319. Under the stated disjoint
write footprints, these later releases cannot restore it. The deletion
returns with that kind still zero. The presentation entry-index word is
unchanged by the selected deletion's writes, so it may still be populated.

## Interpretation

This removes later recursive kind resurrection, reparenting and synchronous
dispatch from Q-COMBAT-045's admitted rooted-deletion class. It also
removes those branches from Q-ITEM-004's root-zero subcase, without
settling that question's other independent callers.

Actual graph/map/pool admission, enumeration/getter effects before this
entry, standalone deletion with a live parent, aliases and subsequent
buffer release remain open. In particular this does not extend the kind
preservation claim across the RDFF buffer release or imply an empty
presentation record or native save. No confidence, parity or complete-reading
promotion follows.

## Alternatives

A standalone nonroot deletion can find a live parent and reach the setters,
allocator and failure loop of FND-ITEM-014; the induction does not cover it.
An altered map can also invalidate the induction. Kind-four or already-empty
map entries can leave listed subtrees unprocessed while preserving the
claimed absence of nonzero-kind writes in the reached call set.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs and use the project-local
evidence Python with resident_listing.py on the complete half-open ranges
in Locations. The reader verifies size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and declared mappings.
The combined listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-root-delete-tree.txt.
Follow root parent selection and map clearing, the p gate before reparenting,
and the recursive parent's equality/map tests. Include complete release
leaves rather than treating their names as preservation proofs. Reuse
FND-ITEM-013/014/015 and FND-COMBAT-164 for the earlier local readings.
No original ran; original-derived output stays only in GAME_DIR.

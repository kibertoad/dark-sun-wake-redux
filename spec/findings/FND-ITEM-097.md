---
id: FND-ITEM-097
title: Ordinary placement publishes a counted row when its initialized old-count position survives loading
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0E1B..31E0:0EFF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0FE6..31E0:1000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0004..289B:0154
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 289B:0154..289B:01DC
tool: tools/research/exec-census/resident_listing.py at 04b58b2a; Python 3.12.10, Capstone 5.0.7; composition with FND-COMBAT-149/159 and FND-ITEM-065/096
environment: null
---

## Observation

Q-PARTY-116 retains ordinary placement's position/count identity after
FND-ITEM-096. This binds the initializer's entry-index store and resolves
both insertion-wrapper branches for position n and count N=n+1. Addresses,
field offsets and masks are hexadecimal; other counts are decimal.

### Position producer and required preservation

FND-ITEM-096's ordinary placement passes the current count n as the
entry-index argument of 31E0:0EFF. At 0FEF that loader forwards its
entry-index word to 31E0:0E1B together with object-definition and
presentation-record pointers. The initializer takes that word in CX,
selects the corresponding placed row, initializes presentation fields,
and writes CX directly to presentation word +1 at 0EEA. Its own body
has no child call, though storage aliases and valid extents remain required.

FND-COMBAT-149 describes subsequent loading, slot copying/allocation and
children. Assume their admitted continuation retains this initialized
index in the eventually returned slot, and retains count n until ordinary
placement's increment. Also require 0<=n<1500, stable valid table/source/
presentation spans, separate stack locals with the required DS/SS mapping,
and clear direction flag for the wrapper's direct byte-copy branch below.
These are conditions, not a claim about every lower-load flag or native call.

The wrapper then receives position p=n and searched count N=n+1, while
persistent DS:264E is N. Its first search gives J in 0..N under admitted
search storage. Sortedness is needed to interpret ordering, but not for
the search's bounded index result. All possible J fall into two cases.

### J at most n: insertion searches the old prefix

The wrapper's unsigned J<=p branch initializes its local count to p=n,
not to its separately supplied search count N. It calls 289B:0004 through
that local's near address. The leaf increments the local count to n+1,
searches n old rows and returns K in 0..n. It shifts old rows when K<n,
adjusts admitted presentation indices as FND-COMBAT-159 records, and
copies the prepared row at K. Its copy clears direction flag itself.

Thus K<N throughout this branch. The first search inspected N rows,
including the extra row, while this insertion search inspects only n;
their results need not be equated. Persistent N is unchanged by the
local-count increment under the stated separated-storage conditions.

### J greater than n: the boundary-removal copy

Since J<=N=n+1, J>n means J=N. The wrapper initializes its removal-local
count to J=N and requests removal at p=n. The removal leaf decrements
that local to n and immediately skips its shift/adjustment because the
new local count is at most the requested index. No old row is moved by
this call. It does not decrement persistent DS:264E.

The wrapper then copies eight prepared bytes to row index n, decrements
its saved J and returns J-1=n. This is again within persistent count N.
The direct REP MOVSB has no own CLD, and the removal's early return skips
its CLD too. A clear direction flag is therefore an explicit precondition
for this branch's forward eight-byte row-copy interpretation; no native
direction-flag guarantee is inferred from this local path.

### Publication and comparison with effect placement

Both admitted branches place the prepared row at a returned K<N and
FND-ITEM-096's caller writes that K to the returned slot's word +1.
It later returns the slot word to its caller. This establishes conditional
counted publication for the ordinary producer's complete J domain, not
survival through its later children or use-time removal.

FND-ITEM-065's effect constructor instead passes p=N. Its admitted first
search always takes J<=p and the insertion-local count starts at N;
K=N remains possible. Ordinary placement's p=n proof cannot repair or
replace that distinct contract. Ordinary prepared flags also lack the
effect constructor's forced mask 20, as FND-ITEM-096 records.

## Interpretation

This removes the insertion-wrapper outcome for the retained ordinary
old-count position domain from Q-PARTY-116. Remaining evidence is actual
lower-load index/count/slot preservation, direction-flag admission, aliases
and spans, other position/count domains and later row lifetime. A counted
ordinary row is not automatically a flagged effect row or proof that a
native effect constructor used the same mechanism.

## Alternatives

Using searched count N as both wrapper locals erases their distinct
initializers. Equating the two searches adds stability of different input
counts. Treating the boundary removal as deleting a counted old row ignores
its early no-shift case. Treating REP MOVSB as inherently forward omits DF.
No original run, native defect, complete reading or parity promotion follows.

## How to reproduce

Use licensed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and project-local evidence Python -I
with resident_listing.py on the four half-open location ranges in order.
GAME_DIR/analysis/item-removal-20261104/ holds
effect-ordinary-counted-publication.txt and its -repeat pair, both SHA-256
bb8fd9de3b0547d8b5c37e10fe0c04ed8a34e3151e58f127a74a75da7b9a8fbd.
Reuse FND-ITEM-096 for the caller and retain all lower-load conditions.
Keep reports local. No original game or isolated function was executed.

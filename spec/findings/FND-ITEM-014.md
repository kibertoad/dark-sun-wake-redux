---
id: FND-ITEM-014
title: Recursive deletion releases before child passes, conditionally reparents type-eight references, and contains a non-returning failure branch
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
tool: tools/research/exec-census/resident_listing.py at 8aaef9ba; Python 3.12.10, Capstone module 5.0.7 (distribution 5.0.9)
environment: null
---

## Observation

This reads the complete inventory-declared 800-byte resident body at 248D,
including the continuation missing from FND-PARTY-218. Its five-word switch
table is the one that finding already records, immediately after this range.
The source is installed DSUN.EXE, checked by the bounded reader against
length 634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, relocated to 1000.
No original function was executed. As for FND-ITEM-013, the installed reporter
catalog has no instruction-listing command; the existing bounded reader is
used for the complete local control and store ordering.

Use i for the selected word index, G for the supplied far graph and M for
the supplied near map. M accesses use incoming DS. FND-PARTY-214 records the
wrapper's distinct SS-local allocation and leaves that storage identity open.
Graph entries have stride ten. Their parent byte is sign-extended to a word;
the first entry's byte +1 is the signed count reread by the loops.

### Release and parent-field request

The initial switch accepts kinds one through five. Kind four and values
outside that interval return directly, without the continuation below.
Kinds one, two, three and five perform their recorded local kind operation
before reaching 256E. The kind-one operation is FND-ITEM-013's release leaf.

The continuation saves M[i]. When that word is signed less than 524, it
passes the value to 2D40:0C92. This is a signed comparison, not a local
nonnegative or capacity check. FND-PARTY-119 records that callee's distinct
object-release branches.

For i zero it sets local parent p to -1. For nonzero i it takes field f
from G[i].word +6 and p from the sign-extended parent byte. It reads M[p]
without first proving p nonnegative. If that map word is 9999, p becomes -1.
Otherwise it:

1. Calls 2D40:286C with G[p] and the three-byte metadata address for current
   DS:60EB. FND-PARTY-221 records that helper's ordered kind-byte/record-word copy.
2. Calls 1AA0:012A with freshly read DS:60EB, field f and doubleword value 9999.

This is a setter request, not proof that the intended parent field was
successfully changed. Setter status is not tested. The body then stores
9999 into M[i] before either later child pass.

### Conditional reparenting pass

The first scan increments a word index j from i+1 and rereads the signed
count from G[0].byte +1 for each loop test. A child is considered further
only when its sign-extended parent byte equals i. The additional gates are:

- The descriptor type byte at DS:60ED plus G[j].word +6 equals eight.
- M[j] is not 9999.
- Local p is signed nonnegative and M[p] is not 9999.
- The current DS:60EB object's kind byte is nonzero.

The body then reads descriptor type DS:60ED plus local f. For type eight,
it passes G[j].word +4, sign-extended to a doubleword, to 1AA0:012A with
current DS:60EB and f. For type seven it calls 2D40:0C31. A return other
than exact 9999 is used as a new object: its kind byte and record-index word
are copied from G[j], in that order, then the new object number is passed
sign-extended to 1AA0:012A for current DS:60EB and f. It also stores that
new object number in M[j]. FND-PARTY-119 separately records the allocator's
object-pool role; its preservation effects are not established by this caller.

After either returned setter branch, G[j].parent receives the low byte of
p and G[j].word +6 receives f. Both setter calls have unchecked status.
Other gate failures skip the child and increment the scan index.

### Non-returning local branch

A parent-field descriptor other than seven or eight reaches 2730. So does
exact return 9999 from the type-seven allocation. The instruction there
branches to its own address and has no outgoing local edge. This is an
explicit non-returning path in the static body. Earlier kind release,
parent-field request and M[i] clearing have already occurred on routes
reaching it. No rollback follows in this local path.

These conditions are not proof that shipped game states admit such a route.
No observed game stall, runtime bug or complete caller-domain claim is made.

### Recursive child pass

The second scan again starts at i+1 and rereads the signed count. It calls
248D recursively only for an entry whose sign-extended parent byte still
equals i and whose M[j] is not 9999, passing j, the same far G and the same
near M. It increments j after a returned child call. On the ordinary return
path this body restores SI and DI.

Reparenting can therefore remove entries from the later recursive selection:
it has already changed their parent byte. This is not an unconditional
traversal of every later entry, nor a claim that every called child returns.

## Interpretation

The body's own kind release precedes its parent-field request, map clear,
conditional reparenting pass and recursive child calls. This resolves the
previously unread local ordering for Q-ITEM-004. In particular a retained
child is processed after its parent's kind operation; a reparented child
may be skipped by the later recursive pass.

Full deletion effects still depend on the reached callees, current-object
and descriptor preservation, admitted counts/indices/parents and map/graph
storage identity. Reads and writes repeatedly reload graph pointers and
globals; disjoint stable storage is not established by their offsets.
FND-PARTY-214/218 retain the map and graph provenance qualifications.
No complete_reading declaration, status promotion or whole-game termination
claim follows from this local reading.

## Alternatives

- A children-first release order is ruled out locally: the own kind operation
  precedes both child scans.
- Deleting every child regardless of field type and reparenting is ruled out
  by the gates and by the parent-byte rewrite before recursion.
- Treating type-seven allocation failure as an ordinary return is ruled out
  by its edge to the self-branch. Shipped-state reachability remains open.
- Assuming parent-index or table-capacity admission from the map lookup is
  unsupported: those checks are not supplied before the accesses here.

## How to reproduce

Run the committed bounded reader with the locked evidence Python:

```text
python -I tools/research/exec-census/resident_listing.py <dsun> 2D40:248D..2D40:27AD
```

Verify the source fingerprint, inventory range, switch targets from
FND-PARTY-218 and the far return at the end. Retain the output as
GAME_DIR/analysis/item-removal-20261104/recursive-deletion.txt, SHA-256
18f30ca9b077a716a2acecffed410d6749bd024a8dc6d7962baa961e5fc31734.
Original instructions and bytes remain only in that approved local store.

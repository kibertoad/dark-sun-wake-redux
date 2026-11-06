---
id: FND-CONFIG-230
title: Nonterminal release composes predecessor and successor merges with a finite metadata write set and ordinary segment restoration
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:13CA..1000:14C4
tool: resident_listing.py and segment_register_writes.py at 90bdf95a; connected interfaces FND-EXE-273/281/282, FND-CONFIG-165/225/227 and FND-COMBAT-166
environment: null
---

## Observation

This composes the nonterminal coalescing branches that FND-COMBAT-166
excludes, rather than inferring preservation from a released payload alone.
Installed DSUN.EXE has634416 bytes and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Existing interface findings supply
the wrapper, insertion, unlink and shared saved-DS behavior; this reading
checks their connected instruction paths and physical destinations.

### Admission and notation

Let P be the nonzero released segment, different from last root CS:135D;
p is its paragraph count and R its incoming word-two predecessor.
Q=P+p is the following header. Counts and segment sums below are admitted
positive, nonwrapping and backed by valid storage. When R is a distinct
free predecessor, its count r satisfies R+r=P. Headers and links remain
stable during this synchronous call. The header/link writes below and
the wrapper's marker byte are separate from live stack frames, code,
CS:1361 and any caller fields whose preservation is claimed.

These are explicit conditions for a branch contract, not a native heap
observation or a test performed by the release entry. Null, terminal,
marker-failure and changed/asynchronous states retain their own paths.

### Predecessor choice and first writes

149B saves incoming DS in CS:1361 and selects13CA for this nonterminal
segment. 13CA sets DS=P, pushes P, reads R, clears P:0002 and writes
R into P:0008 before choosing a predecessor branch.

If P is the first root or R's word two is nonzero, it inserts P through
1464. Reuse FND-COMBAT-166's exact insertion write set: empty head
writes CS:135F and P:0004/0006; nonempty head H and next N write
H:0006, P:0004, N:0004 and P:0006, in that order. The retained merged
block M is P and its count initially p.

Otherwise it bypasses insertion, changes the saved segment from P to R,
and adds p to R:0000. It forms Q=P+p and writes R into Q:0008
when Q's word two is zero, or Q:0002 otherwise. Thus M is R with
count r+p. It does not unlink R, move R's free-list links, or clear the
old P header/count beyond the earlier P:0002/0008 stores. Those bytes
become interior to the merged extent under the stated physical relation.

Both paths pop M into ES, form DS=M+count(M), and inspect that
following header's word two. Under the admitted predecessor relation
this following segment is Q in either path. Without that relation the
two constructions need not name the same storage.

### Successor choice and unlink

Nonzero Q:0002 returns without another merge. Zero instead reads q
from Q:0000, adds q to M:0000, forms U=Q+q, and writes M to
U:0002 unconditionally. It then falls through into143B, with DS=Q;
there is no additional call frame between the merge and unlink.

If Q:0006 equals Q, unlink clears CS:135F. Otherwise let A=Q:0004
and B=Q:0006. It writes B to A:0006, A to B:0004, publishes A
at CS:135F and restores DS=Q. These exact destinations, together with
the earlier insertion or predecessor updates, bound the metadata write
set. A and B may equal M when a two-node list becomes one; that alias
is not by itself a conflict with the ordinary self-link result.

After successor merging, M's count is p+q or r+p+q. The body has no
independent carry rejection. The U:0002 store does not first distinguish
allocated from free U. Interpreting it as an allocated predecessor link
therefore requires U's allocated state; a valid no-adjacent-free physical
chain supplies that condition when Q is free. Merely knowing Q is free
does not supply it. FND-CONFIG-228's checker tests adjacency on its own
admitted traversal, but is not automatically called by release.

### Segment and frame restoration

Neither predecessor merge, successor merge nor unlink has a child call,
software interrupt or own SS assignment. The insertion branch alone can
temporarily use SS=H. Its flags push precedes that switch, it makes no
implicit stack access while switched, restores original SS from BX and
then pops the saved flags, as FND-EXE-281 records. This does not make
the whole list publication atomic or exclude asynchronous effects.

On the admitted ordinary return,13CA balances its saved segment, including
the predecessor replacement. Falling through143B consumes the original
near-call frame once. 149B then reloads DS from CS:1361 and restores
its saved SI, DI and BP. Under the stated separation and stable-slot
conditions, the incoming DS and SS are restored for all these nonterminal
coalescing choices. Temporary unequal DS/SS inside the helpers is expected;
it does not imply a changed returned SS.

The outer444C:0092 wrapper additionally clears its admitted matching
marker and returns zero DX:AX (FND-CONFIG-165). That returned pointer
does not report which merge happened or prove its admission. The finite
metadata write set can preserve a distinct caller record, but payload-only
disjointness cannot establish separation from linked headers or the stack.

## Interpretation

Q-CONFIG-013 can reuse a composed nonterminal release contract including
both merges, their unlink continuation and ordinary DS/SS restoration.
It no longer needs to treat those merge bodies as an unspecified callee
effect. Remaining obligations are actual predecessor/following/list
membership, positive counts and adjacency, no-wrap/backing and separation,
the shared saved-DS slot, excluded mutations and terminal/native outcomes.
Startup equality and sampled equality do not admit those states universally;
FND-EXE-509/550 and FND-RNG-096 retain their original limits.

No native invocation, complete-reading declaration, parity promotion or
whole queue answer follows from this conditional branch contract.

## Alternatives

Treating predecessor merging as fresh insertion contradicts its bypass.
Treating the saved segment as always P misses the replacement by R.
Treating successor unlink as an independent called routine invents a
frame; it is a fall-through. Treating the final predecessor store as
free-state aware ignores the unconditional U:0002 destination. Treating
every temporary SS change as a persistent changed segment ignores its
explicit restoration and the absence of stack accesses while switched.

## How to reproduce

Reuse the matching current-inventory getter-stack-segment-writers.txt
report in GAME_DIR/analysis/handler-census-20261106/. The refreshed
heap-connected-segment-writes.txt from segment_register_writes.py,
DSUN.EXE and coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv agrees on its
listed SS sites, including1464's two switches. This linear inventory
report excludes unlisted/overlapping streams, generated code and external
effects; it is not a global negative segment-preservation proof. Startup's
explicit SS load and1464's two switches provide independently read
positive controls, not evidence that every listed byte is reachable.

Read resident_listing.py ranges1000:13CA..1464,1000:1464..149B and
1000:149B..14C4, retaining heap-coalescing-release-connected.txt and
heap-stack-switch-connected.txt in that local directory. The former also
contains terminal1367 as context; this finding does not extend its contract
to that branch. Follow each push/pop, effective segment and destination,
using FND-EXE-281/282 and FND-CONFIG-227's instruction-qualified
decompiler limitations. Verify the four predecessor/successor choices,
each unlink case and admitted physical equations separately. Do not adopt
fixed57E0 decompiler aliases for dynamically selected heap headers.
No original or emulated call was run; original-derived outputs stay local.

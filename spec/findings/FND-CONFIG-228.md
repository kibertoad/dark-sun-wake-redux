---
id: FND-CONFIG-228
title: The heap checker tests selected physical and free-list relationships before segment classification or free-payload fill checks
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:19A9..1000:1BCB
tool: resident_listing.py and direct_callers.py at 1837396b; Ghidra 12.1.3 read-only INST-single-20261103b; FND-CONFIG-211/225/226/227 and FND-EXE-548/549 interfaces
environment: null
---

## Observation

This reads the connected heap checker19A9 and its three direct consumers
for Q-CONFIG-013. Installed DSUN.EXE is634416 bytes, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. It establishes what the checker tests,
not that gameplay calls it or that its result certifies backing memory.

### Checker outcomes and initial break request

19A9 saves DS and other registers. A zero CS:135B root returns AX1
without walking headers. Nonzero requests1831 with a zero double-word
increment. It requires returned AX zero and requires the last block's
segment plus its paragraph count, at word width, to equal returned DX.
Otherwise it returns AXFFFF. This admits an aligned returned break
representation; it is not only an unsigned comparison of allocation sizes.

The1831 call is not guaranteed read-only. FND-EXE-548/549 connect it
to bounds tests and177C, whose cache/native branches can publish current,
upper-bound, cache and error state. A zero increment does not remove
those reached branches. No checker result is treated as an observation
that these transitive effects did not occur.

### Physical-block traversal

Starting at CS:135B, the checker advances the next segment by each
current header's word-zero paragraph count. For a nonterminal free
block, identified by zero word two, it rejects a following block whose
word two is also zero. This explicitly tests the no-adjacent-free
condition relevant to FND-CONFIG-227's successor-header obligation.

For each nonterminal transition it rejects an unchanged segment, a
zero current paragraph count, or a next segment unsigned at/below the
first root or above the last root. It then requires the next header's
word two, if nonzero, or word eight otherwise, to equal the current
segment. It sums physical free-block paragraph counts in word CX.

Some header accesses precede these range/count checks. Segment addition
and the free-count sum are word-width without a separate carry rejection.
These are code-decided predicates, not proof that every header was readable,
that roots remained fixed or that arbitrary inputs describe distinct spans.

### Free-list traversal and limits

The second traversal starts at CS:135F. A zero free head skips to the
sum comparison with a zero list total. Otherwise each node must have
word two zero and its segment must be unsigned at least the first root
and strictly below the last root. Its paragraph count is added to word DX.
Traversal follows word six.

A next segment equal to the original free head ends the list traversal
before checking that head's word-four backlink. A different next segment
equal to the current node rejects; other edges require the next node's
word four to name the current segment. Thus the closing backlink is not
checked like an internal edge. There is no separate set-membership test
matching each list node to a visited physical header.

Equality of the two word totals yields AX2; inequality yields AXFFFF.
The sums do not establish set equality or exclude arithmetic wrap on
their own. The body has no separate visited set or iteration budget;
termination and the meaning of the predicates depend on stable admitted
roots, nodes and reached service effects. Ordinary returns restore its
stack-saved DS rather than the shared saved-DS slot.

### Three connected consumers

The direct-call reporter finds near calls after PUSH CS at1AAB,1B16
and1B70; instruction checks qualify their entries and far-return frames.
Each consumer compares the checker result with exactly2 and otherwise
returns that result without the subsequent work. These candidates and
controls do not establish all indirect or unrelocated incoming routes.

At1AA4, the admitted branch walks physical segments toward the segment
word supplied at SS:BP+8. A matching header returns3 for zero word two
or4 otherwise. Reaching the last root without a match returnsFFFE;
its own invalid traversal conditions returnFFFF. The companion low word
at BP+6 is not used as part of this segment match. It does not validate
an arbitrary byte offset within a caller's allocation.

At1B10, the admitted branch fills free-list blocks with the supplied
word pattern. For a stable positive paragraph count H, it writes six
bytes starting at offset0x0A, then sixteen bytes in each remaining paragraph:
16H-10 bytes total. It preserves the ten-byte free metadata footprint
identified in FND-CONFIG-227. It caches the next-list segment before
filling and follows it afterward. A zero next segment returnsFFFF
without undoing earlier fills; returning to the head returns2.

At1B6A, the same traversal compares those word-pattern spans, returning
FFFD on the first mismatch, FFFF on the null-next continuation, or2
after a head-closing traversal. Both routines explicitly clear DF and
do not restore it. Their own paragraph loops decrement a word count
without an initial zero-count bypass; the positive-H footprint above
must not be extended to a zero or unadmitted node merely because the
earlier checker returned2.

The supported decompiler view contains the checker and fill/check entries
and exposes their calls. Dynamic header identities and word arithmetic
use instruction checks, retaining FND-CONFIG-227's decompiler limitations.

## Interpretation

The original includes an explicit consistency predicate and diagnostic
consumers that support the allocated/free header interpretation. They
do not establish that ordinary allocation/release calls are guarded by
this checker. Nor does AX2 alone prove physical membership, every
backlink, unchanged bounds or valid backing memory. In particular, it
would be incorrect to call the checker as a passive native snapshot
without accounting for its break-service effects.

Q-CONFIG-013 retains actual membership/header/root/alias admission and
the callers that reach or bypass these routines, along with backing and
external effects. No native heap-check invocation, successful allocation,
whole-entry complete reading or queue closure is established.

## Alternatives

Treating equal word totals as a complete membership proof exceeds the
encoded tests. Treating every list edge as backlink-checked ignores the
early closing-edge branch. Treating the diagnostics as ordinary gameplay
validation assumes caller evidence not supplied by their existence.

## How to reproduce

Run resident_listing.py on the supported source for1000:19A9..1AA4
and1000:1A98..1BCB. The retained heap-check-entry.txt and
heap-check-connected-consumers.txt are under
GAME_DIR/analysis/handler-census-20261106/; the latter report continues
into a separate walk routine, which is outside this finding's consumer
contract. Run direct_callers.py for1000:19A9 and verify the three
PUSH-CS/near-call controls, retaining heap-check-direct-callers.txt.

Follow the break request, both traversal sums, range/neighbor predicates,
closing-list edge, each consumer's exact AX2 gate and the first/repeated
paragraph pattern counts separately. In read-only INST-single-20261103b,
program DSUN.EXE, run ReportDecompileMatches.java at1000:19A9 with1831,
135f and at1000:1B10 and1B6A with19a9. Require all three function
markers; retain heap-check-connected-decompile.log. Reuse the recorded
1831/177C service effects. No original or emulated call was run, and
all original-derived reports remain local and out of Git.

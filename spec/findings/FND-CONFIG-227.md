---
id: FND-CONFIG-227
title: Allocated and free heap blocks use different metadata footprints and tail splitting requires a valid following header
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:13CA..1000:149B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1582..1000:1622
tool: resident_listing.py at a23328b6; Ghidra 12.1.3 read-only INST-single-20261103b with dynamic-segment interpretation checked against instructions; FND-CONFIG-211/223 and FND-EXE-272/273/281/282/283/296 interfaces
environment: null
---

## Observation

This connects allocation, free-list insertion/unlinking and coalescing
to classify the header state needed by Q-CONFIG-013. Installed DSUN.EXE
is634416 bytes, XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. It does
not establish that every native heap satisfies the ordinary block model.

### Word meanings depend on allocated/free state

The named allocation paths return offset four in the selected segment
(FND-CONFIG-211/223). The paragraph count at segment offset zero
determines block arithmetic. New first acquisition stores its own segment
at offset two; growth and tail allocation store the preceding block's
segment there. Exact-size allocation instead restores offset two from
the selected free block's offset-eight word after unlinking it.

For the nonterminal release route13CA, the initial stores preserve the
old offset-two word at offset eight and set offset two to zero. The body
then tests a preceding or following block's offset-two word for zero
when choosing merge branches (FND-EXE-282). Thus zero is its free-state
discriminator, not a validated ownership certificate.

Under the recorded valid/nonaliased ordinary-header conditions:

| Block-relative word | Allocated interpretation | Free interpretation used by these routines |
| --- | --- | --- |
| 0 | Paragraph extent | Paragraph extent |
| 2 | Preceding physical-block segment; self for the first acquired block | Zero discriminator |
| 4 | Starts caller payload | Previous free-list segment |
| 6 | Caller payload | Next free-list segment |
| 8 | Caller payload | Preserved preceding physical-block segment |

The list interpretation follows both directions:1464 inserts a segment
between the current free head and its old word-six neighbor, updating
word-four/word-six links;143B reconnects those neighbors and restores
the selected DS. The empty-list insertion uses self links. Neither
routine validates the selected segments or a complete circular list.

Consequently free metadata is not confined to the four bytes before
the returned pointer. On a13CA branch that inserts the newly released
block, its words4/6/8 can overwrite the first six former payload bytes.
The predecessor-free branch instead merges without inserting that new
segment, so not every release has the same own payload-write footprint.
The terminal-block route1367 is distinct and is not relabeled as13CA.

### Exact allocation and tail splitting

Exact-size allocation calls143B and then reads the restored selected
block's word eight into word two before returning its segment with offset
four. Restoring that predecessor value does not clear the old free links
at words four and six. Caller initialization, such as the distinct wrapper
fills and script setup writes, must not be inferred from this transition.

For a free block at segment S with count H, the oversized selection
requires unsigned H greater than requested paragraphs p.1582 subtracts p
from S:0 and forms tail segment T=S+(H-p), at word width. It writes p to
T:0 and S to T:2. It then forms U=T+p and writes T to U:2, unconditionally.
The remaining free block's links are not removed or reinitialized.

The ordinary interpretation is a retained free prefix and allocated tail,
with a following header whose predecessor becomes T. To treat the final
store as only a predecessor update requires evidence for that following
header's state and extent. If U:2 were a free-state zero discriminator,
this own store would replace it;1582 does not test that alternative.
Thus a coalesced/nonadjacent-free layout or other valid successor-state
evidence is an actual input obligation, not a consequence of H greater
than p. Segment additions also have no independent wrap rejection.

FND-EXE-282's backward/forward merge and FND-EXE-296's remainder
release supply related predecessor updates and zero tests. Those branch
descriptions do not prove every preceding/following header or free-list
link is valid at every call, nor that rejected bound changes roll back.

### Decompiler limitation

The current resident Ghidra view contains13CA and1582 and exposes their
branch calls. Its displayed C collapses some dynamically selected DS
headers into fixed57E0 data/text aliases or repeated word-two expressions.
It therefore does not independently confirm physical header identity.
The instructions establish DS/ES changes, the temporary SS use in1464,
word widths, exact-size post-call read and unconditional successor store.
No fixed data-segment alias from that C is adopted as a heap field.

## Interpretation

Q-CONFIG-013 now has a state-dependent metadata footprint and explicit
successor/coalescing obligation. A payload-only nonoverlap sample does
not establish valid linked headers or prove that every release preserves
other live allocations. Conversely, writes into a released block's former
payload do not by themselves show another live block is modified.

Remaining evidence includes actual block state, free-list membership,
physical predecessor/successor extents, no-wrap and alias admission,
terminal release/runtime results and other writers. Q-SCRIPT-019/025
retain which allocations and stale copies reach their later uses. No
native released-trigger access, player-visible defect, complete reading
or queue closure is established.

## Alternatives

Treating words4/6/8 as permanent caller-owned bytes ignores the free
state. Treating zero word two as a whole-heap validity check exceeds its
branch use. Treating split allocation as checking its following block
contradicts the unconditional store. Inferring these dynamic segments
from fixed decompiler symbols would conflate distinct physical headers.

## How to reproduce

Use resident_listing.py on the supported source for1000:13CA..149B
and1000:1582..1622. Retain heap-allocated-free-header-transitions.txt
under GAME_DIR/analysis/handler-census-20261106/. Reuse the complete
allocation/list/release readings above, tracking current DS/ES and1464's
temporary SS for every field. Require the exact-size word8-to-word2
copy and the unguarded following-header word-two store at159D.

In read-only INST-single-20261103b, program DSUN.EXE, use
ReportDecompileMatches.java at1000:13CA with143b,1464 and at1000:1582
with0002,0000. Retain heap-header-transition-decompile.log, including
the unreliable fixed-segment aliases; use its function markers for
navigation and the instructions for identity. No original or emulated
run supplied this finding. Original-derived reports remain out of Git.

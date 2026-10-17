---
id: FND-PARTY-229
title: An item-selection path copies an object-slot image word into a rebuilt graph's first kind-1 record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007C3E7..0x0007C519
tool: tools/research/exec-census/overlay_listing.py and direct_callers.py; Python 3.12.10, Capstone wheel 5.0.9 (module version 5.0.7), xxhash 4.0.1
environment: null
---

## Observation

Descriptor 191 entry +01F7, trampoline 572F:0066, begins at file
7C3E7. Its inventory gives multiple body ranges; this finding reads
only its entry prefix through the selection-word assignment at
7C515. Later validation, effects and return paths remain outside
this reading. FND-PARTY-218 describes the ten-byte graph entries
and FMT-ACTOR-004 describes the 37-byte object slots used here.

The prefix retains its argument in SI and may first call 2D40:000A
with an RDFF request derived from the current slot and entry data.
It then calls 2D40:2196 twice: first for current DS:1A32 into the
shared graph at 5072:0040, then for current SI into an SS-local
region. It scans the latter for the first entry whose kind word is
one, using signed loop comparisons against returned AX retained
in CX. The scan's near accesses use DS at offsets derived from
BP; correspondence with the SS-local destination therefore needs
the DS/SS alias condition. No missing-kind rejection precedes the
next transfer request: the scan index, including an exhausted one,
is passed to 2D40:288C with the local and shared graph addresses.

Returned AL zero jumps to the later exit at 7C7C8, skipping the
image capture and cache store described below. Nonzero AL admits
this sequence:

1. Read the word at current DS:67D4 plus 37 times current SI,
   retaining word arithmetic. Save it in a local word. FMT-ACTOR-004
   binds table base 67BB and offset 19 hexadecimal to `image`, so
   this is an object-slot image word. Identifying current SI with
   the original argument requires preservation through preceding
   children, not merely the initial assignment.
2. Call 31E0:05BB with the slot's current entry-index word at
   DS:67BC plus 37 times current SI, and three words one.
   FND-PARTY-118 records that placed-entry removal service and its
   downstream slot handling. This caller does not test its result.
3. Rebuild the shared graph with 2D40:2196 for current DS:1A32,
   again targeting 5072:0040. Retain returned AX in CX and start
   word index DX at zero.
4. Scan ten-byte shared entries with signed DX/CX comparisons.
   The first kind-one entry supplies its record number from +4.
   Select that number times 23 through current DS:19C1 and store
   the captured image word at record +0C. Do not resolve OJFF or
   test the captured word for zero or 9999 here. Stop this scan
   after that first store.
5. Whether a kind-one entry was found or the loop exhausted, store
   current DX at DS:1A34 and continue into later work. An exhausted
   scan performs no own cache store but still assigns its index.
   The subsequent record accesses are not validated by this prefix.

The image capture precedes the placed-entry removal request and
the graph rebuild. Its saved word, rather than a fresh slot read
after removal, is the source of this cache write. With normal local
storage preservation, a zero captured image is copied as zero and
9999 is copied as 9999. Neither value is observed in an admitted
native call by this finding. No claim of a successful transfer,
valid graph, complete object lifetime or native player action is made.

## Interpretation

The attached-record image cache has a concrete producer other than
the OJFF resolver: this admitted path preserves an object slot's
current image word across a removal request and stores it into the
rebuilt graph's first kind-one record. Q-PARTY-087 retains other
producers, invalidation, complete graph/slot admission, aliases and
preservation. This reading does not establish that every graph has
a kind-one entry or that every transfer reaches this path.

## Alternatives

- Treating OJFF resolution as the cache's only producer omits this
  direct capture/store path.
- Reading the slot after removal instead of the saved local word
  changes the store's source and ordering.
- Rejecting zero or 9999 at this store adds a local gate absent here.
- Treating the scan's index as FFFF when no kind-one entry exists
  substitutes a sentinel the signed loop does not assign.
- Treating the local graph's BP-derived near accesses as necessarily
  SS-relative ignores their actual default segment.

## How to reproduce

Use installed DSUN.EXE, size 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b, and locked evidence Python.
Query direct_callers.py for 191+01F7 to verify the exported entry.
Read descriptor 191 with overlay_listing.py, 7C3E7..7C5FE,
hexadecimal and half-open, and follow only the entry-prefix paths
through 7C519 for this finding. Retained report:
GAME_DIR/analysis/party-record-heads/drawing-20261018/item-selection-cache-prefix.txt.

Compare the image source's address difference with FMT-ACTOR-004's
table base and image offset. Track the AL gate, saved local image,
DS-relative local scan, rebuilt graph, first kind-one store and
exhausted scan separately. Read the transfer, graph and removal
findings as child contracts; do not substitute assumed success or
preservation for what the prefix actually checks.

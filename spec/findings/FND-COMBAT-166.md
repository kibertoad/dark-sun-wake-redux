---
id: FND-COMBAT-166
title: An admitted non-top noncoalescing buffer release has finite header writes and preserves the cleared temporary kind and RDFF status
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 444C:0092..444C:00F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:13CA..1000:149B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:149B..1000:14C4
tool: tools/research/exec-census/resident_listing.py at 261557c3; Python 3.12.10; Capstone 5.0.7; composition with FND-CONFIG-165/167 and FND-EXE-281/282
environment: null
---

## Observation

FND-COMBAT-164/165 leave preservation across the subsequent RDFF buffer
release open. Existing heap findings describe the release dispatcher and
link helpers. This reading derives the complete write set for one
noncoalescing branch, including empty and nonempty free-list cases.

### Branch and storage conditions

At analysis load segment 1000 the incoming DS and SS are resident 57E0.
The released buffer is P:0004, P nonzero and unequal to CS:135D.
Its paragraph-count word s at P:0000 is from one through 4095, with
valid byte 77 hex at P:(16*s-1). Header/marker reads and writes are valid
and nonwrapping. The segment addition Q=P+s does not wrap and Q is a
valid following header whose word +2 is nonzero.

Let R be P's incoming header word +2. Either P equals CS:135B, or
R names a valid predecessor header with nonzero word +2. These are
the exact tests selecting the link call rather than predecessor merging.
For a nonzero free-list head H from CS:135F, H and its word-six neighbor
N are valid headers. All reached header/link and marker spans are separate
from the live RDFF stack, temporary kind at 4F49:0FF0, presentation index
at resident DS:95D7, caller pointer fields, code and saved-DS word CS:1361.
The admitted synchronous execution has no intervening mutation of these
inputs. This is a conditional metadata class, not observed native heap state.

### Exact wrapper and runtime writes

444C:0092 first reads P:0000, then its nonnull matching-marker branch
clears byte P:(16*s-1). It passes the original far pointer to 1000:149B.
It does not write the mismatch flag on this selected branch.

149B saves incoming DS at CS:1361. It reads only the pointer segment for
dispatch, selecting 13CA because P is nonzero and differs from CS:135D.
13CA loads DS=P, reads R, clears P:0002 and writes R to P:0008.
The admitted predecessor condition reaches its call of 1464.

1464 has no child call or software interrupt. For an empty head, it writes
P to CS:135F and self-links P:0004 and P:0006. For a nonempty head H it
writes P to H:0006, H to P:0004, P to N:0004 and N to P:0006, in that
order. Its temporary SS switch uses no stack access while SS names H;
the saved flags are pushed before that switch and popped after restoring
the original SS. FND-EXE-281 supplies that complete local ordering and
its lack of whole-publication atomicity. DS remains P in both cases.

13CA pops its original P into ES, forms Q=P+s and reads Q:0002.
The admitted nonzero value returns immediately, excluding successor
merging and the fall-through unlink body at 143B. 149B restores DS
from CS:1361. None of these selected header/link writes touches that
shared saved-DS word under the stated physical separation. 149B also
restores its saved SI and DI; the outer wrapper does not assign them.

The outer wrapper then sets DX and AX to zero and returns. Apart from
ordinary live-frame writes, its selected write set is the marker byte,
P header words +2/+4/+6/+8, the named head/neighbor link words when
present, and CS:135F/1361. No selected call reaches the top-block helper,
the pair setter, DOS interrupt service, game dispatch or an opaque callee.

## Interpretation

For this admitted write set, the protected kind byte and RDFF status
word are preserved, and resident DS/SS are restored on normal return.
The caller's subsequent returned-pointer store can set its distinct
buffer local to zero without changing its status local. This removes
the buffer-release preservation dependency for this metadata branch.

Composed with FND-COMBAT-165's admitted rooted cleanup, kind 319 stays
zero through this release. If the RDFF status was FFFE, its common
kind-two rejection cannot replace that status while kind remains zero;
FND-COMBAT-163's parent therefore takes the corresponding release/fill
failure route. The presentation record is not cleared by this buffer
release itself.

Actual pointer/header provenance, selected branch state and separation
remain unadmitted for a native RDFF instance. Top-block release,
predecessor/successor merging, marker failure, asynchronous changes and
other aliases remain outside this result. The returned zero pointer alone
still does not prove successful native heap ownership/release. No run,
universal heap guarantee, complete-reading or parity promotion follows.

## Alternatives

A matching marker alone does not admit this runtime branch. A nonempty
list is not an unbounded traversal here: only its head and one neighbor
are used, but those links still require valid independent storage. The
temporary SS use does not place a return frame in the head block; its
stack accesses occur before and after restoring SS. No atomicity or
interrupt behavior is inferred from the synchronous preservation result.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. Use the project-local
evidence Python and resident_listing.py on the complete half-open ranges
in Locations. The approved reader verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and applies declared mappings.
The combined listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-buffer-release.txt.
Follow the marker branch, segment dispatch, both link-helper cases,
no-coalescing return and shared saved-DS restoration. Compare the finite
write expressions with the named protected spans before composing the
result with FND-COMBAT-163/165. Original-derived output stays in GAME_DIR.

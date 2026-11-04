---
id: FND-COMBAT-167
title: Admitted enumeration produces bounded earlier-parent prefixes but its zero-result exit leaves root fields consumed by cleanup
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2196..2D40:248D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1AA0:051C..1AA0:063F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F5A..1000:3F7E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:27B7..2D40:286C
tool: tools/research/exec-census/resident_listing.py at 99e39995; Python 3.12.10; Capstone 5.0.7; composition with FND-CONFIG-143/145 and FND-PARTY-214/218
environment: null
---

## Observation

Q-COMBAT-045 and Q-ITEM-004 retain admission of the graph used by cleanup.
Existing findings give the complete enumeration, pending-stack and map
bodies. This reading derives numerical/parent invariants and separates
positive output from the invalid-kind zero-result path.

### Conditions and pending-state bounds

At analysis load segment 1000, DS=SS=57E0 admits the helper's near state
as the caller's BP-relative allocation. State, output, other locals and
live frames are valid, nonwrapping and physically separate from selected
metadata/copy destinations. Getter/copy calls return preserving the
pending state, output, caller locals, loop registers and frames except
for their stated results. All selected table/descriptor/metadata accesses
are admitted. These are conditions, not native caller-state observations.

2196 initializes its pending count to zero at BP-128 hex. 05DC adds one
only while that unsigned count is below 31. At count 31 it performs its
indexed writes and sets DS:0423 to one without incrementing. 051C
decrements a positive count before popping, or returns 9999 without a
decrement when empty. With no other count mutation, the count stays 0..31.

The helper's write-before-check ordering therefore has these maximum
nonwrapping state spans in this initialized class, using half-open offsets:

| Field | Offset span |
|---|---|
| Pending count | 0000..0002 |
| Index words | 0002..0042 |
| Selector words | 0042..0082 |
| Output-ordinal words | 0082..00C2 |
| Optional three-byte snapshots | 00C2..0122 |

State ends before BP-6, within the 128-hex-byte frame allocation, without
overwriting its final three local words. A push at count 31 writes slot
31 but does not make it pending; later pops use slots at most 30. Thus
index/selector/ordinal words actually popped were written by counted
pushes. This is not a completeness guarantee: saturated pushes can lose
candidates. Optional snapshot coherence depends separately on the current
object and metadata state; the numeric bound does not establish it.

### Positive prefix and parent ordering

For each valid-kind output entry at ordinal i, 2196 passes i to 0566.
Every queued candidate receives that ordinal in its pending triple.
After 051C selects an accepted next candidate, the caller increments its
output count and loads the saved ordinal into the next entry's parent
local. Older pending candidates carry still earlier output ordinals.

By induction, every emitted nonroot entry i has a parent from zero through
i-1. The stored byte does not narrow those ordinals incorrectly: the
outer loop emits at most 79 entries. Root's byte +1 is later overwritten
with total count and is not a parent index. The first emitted step code
is one; later step codes are two, three or four. Each emitted kind word
comes from the valid one-through-five test and local copy.

A positive returned prefix therefore has count N from one through 79,
ten-byte entries and a final FF byte at offset 10*N. Its maximum own
output span is 791 bytes, within 2410's 800-byte local buffer. For the
separate map builder, an unchanged such count bounds its own indexed
word stores to 158 bytes within 2452's 160-byte local allocation. Map
value validity and getter effects are independent of those numeric sizes.

The retry path calls 051C without adding pending entries. At most 31
populated pops followed by an empty result can occur between output
increments; 9999 takes the accept/finish continuation. Each 0566 selector
scan also has a signed decrement on every local iteration. With the
admitted returning callees, the retry path cannot keep an unchanged
positive pending count forever. This does not bound nonreturning getters
or establish that the represented object graph is complete or acyclic.

### Zero result is a different consumer contract

If an outer iteration finds kind zero or above five, +21DC writes that
current index to output base word +6, resets output count to zero and
finishes. It does not clear all prior output fields. The final stores
write FF at base byte zero and zero at base byte one. If a root was
previously emitted, its word +6 can therefore be replaced; other fields
can retain earlier values. If none was emitted, those other bytes need
not have been initialized by this invocation.

2410 does not test enumeration's result before invoking 2452 at index
zero. 2452 reads the base word +6 as the map builder's root argument;
27B7 forces map[0] to that argument even for count zero. Deletion
dispatches on the entry's kind word +2, not the FF step byte or a
positive-count check, as FND-ITEM-014 records. A zero result therefore
does not itself establish an empty/no-operation cleanup graph. Actual
reachability and effects depend on the retained fields and admitted state.

## Interpretation

This removes positive-prefix count/parent ordering and initialized
pending-write bounds from the unread producer obligations. It supplies
the structural earlier-parent condition used by FND-COMBAT-165 when a
positive prefix and its additional storage/pool conditions are admitted.
It separately identifies the zero-result root-field reuse that those
conditions must exclude or account for.

Actual near-storage identity, getter/metadata preservation, root kind and
index admission, selected map values, pool ownership, snapshot coherence,
saturation and zero-result fields remain open. Repeated object indices
can occur in an earlier-parent prefix; entry order does not prove unique
ownership or complete traversal. No native graph, confidence/parity or
complete-reading promotion follows.

## Alternatives

The count check after the pending writes is not safe for arbitrary initial
counts; its initialized invariant matters. Earlier-parent output entries
do not prove an acyclic object graph. Conversely, an FF first step byte
with count zero does not prevent a consumer from reading other root fields.

## How to reproduce

Resolve licensed DSUN.EXE through tools/game-dir.mjs. Use the project-local
evidence Python and resident_listing.py on all complete half-open ranges
in Locations. The approved reader verifies length 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b and declared mappings.
The combined listing is retained at
GAME_DIR/analysis/hit-incoming-20261104/temporary-enumeration-prefix.txt.
Follow count initialization, all pending stores/pops, the +22C0 ordinal
argument, +23A3 increment and following ordinal reload, and the +21DC
invalid-kind exit. Compare the unconditional cleanup/map-root use.
No original ran; original-derived output stays only in GAME_DIR.

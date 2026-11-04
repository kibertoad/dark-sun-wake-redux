---
id: FND-COMBAT-119
title: The restore clearing branch uses stored-pointer equality and a fixed 12000-byte far fill before reloading the destination
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007089C..0x00070964
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:3F7E..1000:3FC1
tool: tools/research/exec-census/overlay_listing.py and resident_listing.py at 96f13acf; existing fill contracts FND-CONFIG-144/176
environment: null
---

## Observation

Q-COMBAT-065 retains the placed-buffer clear outside the nominal payload
comparison of FND-COMBAT-118. FND-COMBAT-035 identifies its caller;
FND-CONFIG-144/176 already read the complete fill bodies. This finding
connects those contracts to the restore's pointer reload and register
lifetime, without rediscovering the helper or admitting its input storage.
Offsets and counts below are hexadecimal unless stated otherwise.

At overlay 187 +0ABB the reader loads the current source entry's
segment word into AX and offset word into DX. It compares AX with
DS:67B9 and DX with DS:67B7. Either mismatch skips clearing. Both
matches push double word 2EE00000, then AX and DX, to 1000:3FA2.
The actual far-call frame therefore contains the destination pointer,
fill word zero and count word 2EE0, decimal 12000.

This is equality of the stored segment and offset words, not a
normalization or physical-address comparison. Different far encodings
of the same modeled byte address need not take the same clear branch.
The comparison also supplies no independent nullness or extent check.

### Exact fill effects

3FA2 forwards the pointer, count and low fill byte to 3F7E. The
leaf loads ES:DI, clears direction, writes a leading byte for an odd
offset when the count permits, then word pairs and an odd trailing
byte. It restores saved DI and BP, has no further call or interrupt
and makes no DS or SI assignment. The wrapper restores BP and
returns the supplied pointer in DX:AX. It does not assign SI or DI.
ES remains the destination segment and direction remains clear.

For this nonzero count, the local store footprint is exactly 2EE0
bytes at offsets (O + k) modulo 10000 in the supplied segment,
for k from zero through 2EDF. If O <= D120, the written offsets
fit the ordinary half-open range [O,O+2EE0) without a wrapped
written byte; the cursor can wrap after the last byte at the endpoint.
Larger O wraps the written footprint within that same segment rather
than incrementing the segment. No validity or capacity follows from
the fixed count. A successful earlier allocation of that logical size
is still an independent producer/lifetime requirement.

### Caller reload and retained conditions

Immediately after return and argument cleanup the reader recomputes
the table index from SI and rereads both pointer words for its null
test. If nonnull it again rereads them into the local in/out slot
before the resource request. It does not use the fill's returned
DX:AX as the later destination, test it as an error code, or copy
it into the source table.

Under valid distinct stack/buffer storage, these bodies preserve
current DS, SI and DI, hence the loop's index and selector through
this explicit child call. That is stronger than an unknown-callee
assumption for this helper, but weaker than proving the destination
metadata stable: an aliased supplied buffer can change the table or
supplier fields, and the caller deliberately reloads their contents.
The helper's own stack accesses also require admitted nonaliasing.

The clear precedes the reader's null-resource skip. Equal null words
therefore reach the call in the encoded local route; the later null
test does not guard it. This is branch ordering only. No null fill,
wrapped buffer or actual allocation failure was executed or admitted.

## Interpretation

The explicit clear's callee footprint and DS/index/selector effects
are now resolved for Q-COMBAT-065. Under the valid heap-buffer
conditions of FND-COMBAT-117, this fixed fill also misses the resident
metadata of FND-COMBAT-118. Those conditions, actual pointer identity,
extent, aliases and subsequent child effects remain open.

The read's selected resource length remains Q-COMBAT-064; it is
neither the clear's count nor an enforced capacity derived from it.
The preceding clear is not rolled back by a later read failure.
No global writer census, native load or evidence-status promotion follows.

## Alternatives

- Stored far-pointer equality is not equality of every physical alias.
- The helper's pointer return is not the caller's reloaded destination.
- A fixed fill count does not validate storage or protect metadata aliases.
- A later null test does not guard an earlier call.

## How to reproduce

Resolve DSUN.EXE through tools/game-dir.mjs and use locked evidence
Python. The readers verify size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Read descriptor 187, file bounds
7089C 70964, and resident 1000:3F7E..1000:3FC1. Preserve the
outer frame's fill/count word order, compare both pointer words and
follow the separate pointer reloads after return. Reuse FND-CONFIG-144/176
for the helper's full branch reading and FND-COMBAT-117 for its producer
conditions. Retain reports only under
GAME_DIR/analysis/hit-incoming-20261104/: restore-placed-clear-helper.txt
and restore-placed-clear-caller.txt. No original listing is committed.

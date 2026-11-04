---
id: FND-ITEM-044
title: Placement admission can return positions six and nine and compares a full word before the position byte write
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2B6A..2D40:2C87
tool: tools/research/exec-census/resident_listing.py at a867719d; Python 3.12.10, Capstone module 5.0.7; composition with FND-PARTY-221, FND-CONFIG-144 and FND-ITEM-043
environment: null
---

## Observation

Q-ITEM-006 retains producers of position six and nine, absent from canonical
RDFF source payloads in FND-ITEM-043. This complete reading of placement
helper 2B6A identifies when those requested values survive to the output
used by FND-PARTY-221's transfer continuation. Source DSUN.EXE is length
634416 and XXH3-128 e296af55ba2ecde7e77f555c90f33d0b, at load segment 1000.
No original function was executed.

### Inputs and storage conditions

The helper takes a far target graph, signed requested-position word P
and near output-word pointer. It writes output -1 before inspecting root
kind. Require a valid graph with its signed count N, valid item record
spans for all read indices, stable DS/table/graph identities and disjoint
frames, output and records. To compose with the known transfer prefix,
use N=1..78; this helper itself performs no independent capacity check.

The local 26-byte occupancy buffer is zero-filled through an SS far pointer,
but its increment addresses are formed as near DS addresses from the BP
offset. Require those DS offsets to address the same local bytes; a near
output address must likewise name the caller's intended word. DS/SS mapping
is a prerequisite, not implied by LEA or a valid SS allocation.

### Root kind and collision scan

For root kind other than two, signed P <= -1 returns AL one while leaving
output -1; larger P returns AL zero. This branch does not fill or scan the
occupancy buffer and does not return the requested negative value itself.

Kind two clears the occupancy buffer and scans graph indices one through
N-1. Each kind-one entry selects its attached record through DS:19C1 and
the low word of 23 times its record index. An attached position byte FF
skips the collision comparison. Other position bytes are sign-extended
and compared with the full requested word P. Equality sets the local
success byte to zero; scanning still continues through the remaining entries.
There is no parent/ancestor filter in this scan.

For every kind-one entry, a signed position from zero through 25 increments
the corresponding occupancy byte. Other values are not counted there.
With the admitted N bound these byte counts cannot wrap. The helper's
only child call is the initial fill; FND-CONFIG-144 records its contract.

### Ordinary output and automatic selection

If any collision cleared success, output stays -1 and AL returns zero.
Otherwise every P except exact -2 is copied unchanged to output and AL
returns one. There is no additional equipment-position range check.

Exact -2 instead initializes output to -2 and searches the occupancy bytes
for the first zero from 14 through 25, inclusive. A found index becomes
the output with AL one. No free index leaves output -2 and returns AL zero.
A record position -2 is not the FF exception: it collides with requested
-2 before this search, preventing automatic selection altogether.

Under the admitted root-kind-two conditions, requested six or nine thus
returns that same word when no scanned attached item has that position.
If every scanned position still has FND-ITEM-043's canonical -1/zero
source values, neither request collides. This identifies a conditional
production route for both source-absent positions, not a proven UI action
or native graph invocation.

### Word comparison versus final byte width

FND-PARTY-221's transfer continuation sign-extends this output word and
requests field ten through the generic setter. With canonical kind-one
descriptors, stable current-object binding and retained setup width one,
that request copies the low byte into attached position +13 hex.
Reaching that continuation, successful resolution and disjoint copy storage
remain prerequisites; the transfer does not check setter status.

The collision test has not narrowed P to a byte. For example, P=262 cannot
equal any sign-extended position byte and can pass the scan even when an
item already has position six. A later successful width-one setter writes
low byte six. This is a conditional arithmetic/store-width distinction,
not evidence that a native caller supplies 262 or that a duplicate position
occurs. FND-PARTY-221 separately retains the initial width-two alternative.

## Interpretation

This removes the placement helper's local range/collision/output contract
from Q-ITEM-006's unknown producers. Six and nine need no distinct source
payload encoding when an admitted continuing transfer supplies those
requested words. Remaining work is actual request/caller admission, graph
and record selection, near storage identity and setter lifetime/width,
plus independent writers and other inputs. No complete-reading, confidence,
parity or native behavior promotion follows.

## Alternatives

A presumed fixed equipment-slot range would reject other requested words;
the ordinary branch simply returns P after collision checks. A byte-first
collision comparison would see 262 as six; the actual comparison is against
the full word. A negative output is not by itself failure: a non-kind-two
root can return AL one with output -1. These distinctions do not admit
malformed graphs, changed tables or unsupported caller states.

## How to reproduce

Run resident_listing.py with the verified DSUN.EXE on the complete
2D40:2B6A..2D40:2C87 span. The retained listing is
GAME_DIR/analysis/item-removal-20261104/position-admission-body.txt.
Follow output initialization, root-kind branches, every position read,
full-word comparison, occupancy addressing, continued scan after a match,
ordinary output and automatic search endpoints. Check requested six/nine,
exact -2, the FF position exception and the distinct word 262. Reuse
FND-PARTY-221 for the later field-ten request instead of assuming that
AL one proves a successful byte mutation. No original listing or fixture
is committed.

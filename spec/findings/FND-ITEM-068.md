---
id: FND-ITEM-068
title: The sampled effect-placement caller builds wrapped coordinate keys and accepts every constructor result except FFFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007EAE1..0x0007ED9D
tool: tools/research/exec-census/overlay_listing.py at 5a64665b; composition with FND-EXE-606, FND-MAGIC-019 and FND-ITEM-065/066/067
environment: null
---

## Observation

FND-ITEM-065/067 leave the effect constructor's actual key and publication
open. FND-EXE-606 already describes sampled-area placement 193 +07F1.
This verifies the complete caller's coordinate widths, byte producers and
returned-index consumers rather than treating a nonnegative constructor
result as proof of counted publication. It removes these local binding
alternatives from Q-PARTY-109, not the native argument/lifetime obligations.

### Local key production and point admission

The caller obtains one slot from +29EB with the effect-row number and
the lifetime/level/kind arguments recorded by FND-MAGIC-019. It stores the
returned AX and skips the point loop when that saved word is negative.
There is no independent upper slot bound in this caller. The effect-row
read selecting point count occurs before that negative-slot test.

A zero high nibble of effect-row byte five forces its point-count word
to one; otherwise it retains the incoming count. The loop starts at zero
and compares its index signed with that count. A forced one therefore
requires a readable first point even if the incoming count was zero;
source buffer capacity and actual sampler output are separate prerequisites.

For each visited four-byte point, x is the low word of base x plus its
first word, and y the low word of base y plus its second word. These
are wrapping word additions, with no own overflow test. The two cell
coordinates used for admission divide those wrapped words signed by sixteen,
truncating toward zero. They are not unsigned shifts of the unwrapped sum.

The point proceeds if 25AF:00E1(cell) returns AL zero, or, when it is
nonzero, 25AF:0576(cell) returns AL one. Other results skip that point.
It then requests 1B0B:00A4 from the base position's signed-divided cell to
the candidate cell, with the two extra words zero, and requires AX zero.
FND-SCRIPT-037 supplies that geometry reader's separate contract. Actual
grid/coordinate admission, segment identity and child effects remain open.

On admission it calls 31E0:03CA with the saved slot, wrapped x, wrapped y,
the low byte of a local word initialized to zero and final byte zero.
The local word is initialized at +0861 and has no other own writer in
the full body. It is distinct from the redraw counter at BP-2. Thus the
prepared search key's byte four is zero under intact separated frames
and child preservation; the final low-three-bit flags input is also zero.
FND-ITEM-065's constructor consequently prepares flags 68 hex on this route.

### Returned index is consumed without a count check

After 31E0:03CA the caller copies AX to SI and skips the rest of the point
only for exact SI=FFFF. It does not compare SI with DS:264E, verify a slot
word or reread the prepared flags before the following requests.

With effect-row byte-two bit 20 set, it selects table row SI, shifts its
two unsigned coordinate words right four and calls 25AF:017F on that cell.
It then independently reselects row SI and requests 25AF:042A with the
effect row's DATA number and the selected cell. Finally it calls +0AAD
with SI and zero. Its subsequent return-byte and redraw-counter handling
is already FND-EXE-606; none of those own gates adds a counted-index test.
Each child can change the table or other shared state, so these rereads
need their own storage-preservation conditions.

In particular a hypothetical returned K equal to persistent count N,
allowed by the local constructor/search contract, passes this caller's
own FFFF test. FND-ITEM-066 supplies conditional storage capacity at that
index; it does not make it a counted row. The immediate cell/effect
requests therefore cannot be used as evidence that K<N was checked.
No native K=N return or effect outcome was observed here.

### Final counted pass differs from the immediate requests

After all points, effect-row byte-two bit 10 selects a pass over indices
below current unsigned DS:264E. For each row whose slot word matches the
saved effect slot, it calls 25AF:01AE on that row's shifted cell. This
pass uses the counted prefix, rather than the most recent returned K.
An uncounted row would not be included merely because an earlier immediate
request used its index. Actual count/table changes by intervening children
remain separate; no lost update or cleanup defect is established.

## Interpretation

The selected caller's byte-four key and flags input are retained zeros;
the coordinate key is a wrapped point translation subject to the named
grid/geometry gates. Its result acceptance is exact-not-FFFF, and immediate
row consumers do not establish counted publication. Q-PARTY-109 retains
actual slot/sample/base/key inputs, child/frame/table preservation, returned
K and use-time row lifetime. FND-ITEM-067's paused endpoint supplies no
constructor call or key. No rule, parity or complete-reading promotion follows.

## Alternatives

Using the redraw counter as byte four would confuse two separate locals.
Using unsigned cell division would change negative wrapped coordinates.
Treating any accepted return as a counted row would add an absent bound
check. Conversely, accepting a hypothetical index at N does not prove
that any native call returns it or that the downstream effect fails.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 193,
7EAE1..7ED9D hex. placement-effect-samples.txt under
GAME_DIR/analysis/item-removal-20261104/ retains the complete entry-based
body. Trace the two separate zero locals, signed point-count loop,
wrapping additions and signed divisions, child gates, all five constructor
pushes, exact FFFF comparison, selected-row rereads and final counted pass.
Reuse FND-ITEM-065's constructor and FND-ITEM-066's capacity contract.
No original code, coordinate buffer, memory or fixture is committed or executed.

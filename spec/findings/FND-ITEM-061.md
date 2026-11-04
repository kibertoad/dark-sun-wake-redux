---
id: FND-ITEM-061
title: Equipment dispatch forces the direct cast route with level six even for canonical psionic selectors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x00080617
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x00000000..0x00680304
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x000120E8..0x00017C28
tool: tools/research/exec-census/overlay_listing.py at 9be91390; controlled joint item and DATA reports of FND-ITEM-057/060; composition with FND-PARTY-254 and FND-MAGIC-016
environment: null
---

## Observation

Q-PARTY-109 retains the equipment hook's dispatched effects. FND-MAGIC-016
already describes dispatcher 193 +2290's branches. This verifies the full
local body and binds the equipment caller's arguments, removing the choice
between its class-level, psionic-use and direct-cast child routes for this
caller. Downstream cast effects remain separate obligations.

The dispatcher compares selector D signed with 328 and returns low byte
zero when D is at least that bound. Below it, a nonzero byte at BP+1A
chooses its direct call to 193 +0000 before the selector-category branches.
Otherwise D at least 269 also chooses that call; the remaining alternatives
are 177 +0492 for D below 235 and 176 +0196 for D=235..268, passing D-235
to the latter. These comparisons are signed, not an unsigned admission
check; negative selectors are outside the canonical composition below.

FND-PARTY-254 and FND-ITEM-059 bind the equipment after-hook's call to
owner, owner, captured D=W-1, word six, four zero words, a far SS pointer
to its initially zero local byte, word one and word zero. The one supplies
BP+1A. Thus every reached call with retained canonical D=0..318 takes
193 +0000 directly. It passes D unchanged and low byte six as the level;
the final byte argument is zero. The output pointer remains the caller's
far SS pointer. The dispatcher does not substitute a computed class level,
subtract 235 or enter the psionic-use wrapper on this route.

Before that child call the dispatcher performs only register and stack
operations. It has no own persistent-data stores or other child call on
the selected prefix. With admitted disjoint frames and retained DS identity,
the equipment caller's published DS:6174 value one therefore reaches the
entry of 193 +0000. This does not establish that the flag stays one until a
later consumer in the cast/hit graph. On normal child return the dispatcher
passes back its low-byte result; FND-ITEM-059's outer hook ignores that
result and clears the flag.

### Canonical selectors on the conditional path

Join the same 1931 item rows to DATA[W-1] as FND-ITEM-060, selecting W=1..319
and property one. Among the 93 conditional candidates, 78 have D below 235
and fifteen have D=235..268. None has D=269..318. The fifteen psionic-range
payloads have these selectors and frequencies:

| D | Items |
| --- | ---: |
| 240 | 3 |
| 241 | 1 |
| 242 | 1 |
| 246 | 3 |
| 251 | 1 |
| 252 | 2 |
| 253 | 1 |
| 254 | 1 |
| 262 | 1 |
| 263 | 1 |

They still take the direct cast route because of the explicit one byte.
The ordinary dispatcher path's power-eligibility, check and point-charge
wrapper described by FND-RNG-046 is not called by this dispatcher invocation.
This is a local route exclusion, not proof that no downstream routine can
check or modify any related state. Source candidates also require the
status, DATA-content, reset and preservation conditions of FND-ITEM-056/057/060.

## Interpretation

The equipment path carries level six and a forced direct-cast selector,
including fifteen canonical psionic-range candidates. Q-PARTY-109 no longer
needs to choose between the dispatcher's three child families for this call.
Actual 193 +0000/+003C inputs and child effects, flag preservation to each
consumer, reentrancy, record/frame and source-value admission remain open.
FND-MAGIC-018 and FND-PARTY-167 record later duration consumers; publishing
one at the outer call alone does not prove their observed use-time value.
No native effect, parity or complete-reading promotion is claimed.

## Alternatives

Selecting the psionic-use branch solely because D is in 235..268 would
ignore the earlier nonzero-byte gate. Replacing level six with a class
level would follow the unselected party-spell wrapper. Treating the signed
328 comparison as an unsigned or negative-selector guard would broaden
the verified input contract. Treating the whole cast graph as flag-preserving
would extend a proof of a store-free local prefix across unexamined children.

## How to reproduce

Use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. Run overlay_listing.py for descriptor 193,
file range 80580..80617 hex. The complete entry-based listing is retained
as placement-effect-dispatch.txt under GAME_DIR/analysis/item-removal-20261104/.
Check signed branches, BP+1A's priority, the direct child's pushes and the
low-byte return. Reuse FND-PARTY-254 and FND-ITEM-059's equipment-hook pushes.

Use FND-ITEM-060's hash-controlled item-hook-inputs.json and
FND-ITEM-057's placement-data-properties.json. Select nonzero W below 320
with DATA[W-1].property & 3 equal to one. Count D=W-1 below 235, from 235
through 268, and from 269 through 318; reconcile their sum with 93. Group
the middle set by D and check the table above. This procedure uses committed
bounded readers and retained private reports; no original game is run and
no original code or payload is committed.

---
id: FND-EXE-419
title: Record release publishes unused state before tail reclamation and returns its result
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1146..1425:1198
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:1769..1425:17AE
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

The 82-byte body at 1146 decodes as 34 instructions. It saves
BP and SI and copies incoming word SS:BP+6 into SI. It
passes a far pointer to current DS:3EEC indexed by SI times
000E to 09AD, removes four outgoing bytes and tests full
returned AX through DX. Nonzero returns that AX immediately;
there is no additional rollback of the release helper's effects.
FND-EXE-411 describes that helper's ordered failure prefixes.

### Publication and later result

Zero sets the current indexed DS:3EF8 dirty word to one,
then sets its DS:3EEC head word to one. It reads indexed
DS:3EF4 and compares that word unsigned against current
DS:00CE. If the identity is smaller, it lowers DS:00CE
to that word. Equal or larger identities leave the scalar unchanged.
The head and dirty writes occur before this comparison.

It then calls 0DE9 without arguments and returns that helper's
AX without a local result test or reversal of its earlier stores.
Thus release success is not itself the final return result: a later
tail-reclamation failure can become the returned error after unused-state
publication. Conversely, FND-EXE-417's count-one early exit can
return zero without flushing the newly dirty record. That is a
conditional local sequence, not proof of admitted native state or an
observed original bug. The body restores saved SI and BP on return.

There is no local selector bound or DS save/restore. SI is saved
for final return but not around either helper; current DS and SI
are re-used after release. The head/identity/dirty storage and callback
preservation remain unadmitted by the local call alone.

### One public wrapper

At 1769, a 69-byte/30-instruction body saves BP and allocates
two local bytes. Current DS:00C4 zero returns AX three.
Otherwise it calls 0FA3 with incoming word SS:BP+6 and
far local output pointer SS:BP-2, removes six outgoing bytes
and tests full returned AX through DX. Nonzero returns that AX.
FND-EXE-414 describes the selector's preceding effects and output.

On zero it reads current DS:3EEE indexed by local BP-2
times 000E and compares that word with incoming SS:BP+8.
A mismatch returns AX eleven, without undoing selector effects.
A match pushes the local selector and calls 1146. A word
pop into CX removes its two outgoing argument bytes, leaving
returned AX intact; the wrapper then restores BP and returns.

This wrapper does not separately test DS:3EEC against unused
head value one before invoking 1146. That differs from the
quantity-adjustment wrapper in FND-EXE-413. Neither a matching
scalar nor the selector's zero result independently admits the head
storage, link structure, current DS, input slots or native effects.

## Interpretation

This adds the adjacent release caller requested by FND-EXE-418,
another DS:00CE writer and one wrapper's scalar admission test.
Q-EXE-007 retains complete callers and input/selector admission,
head/identity/dirty/scalar/count producers, stable links, alias/extents,
segment/register preservation and native contracts. No complete reading,
atomic release/reclamation or observed original bug is declared.

## Alternatives

Returning only the first release result discards the later helper's AX.
Treating the later failure as leaving the record unchanged erases the
dirty/head publications. Treating DS:00CE as unconditionally replaced
ignores its unsigned comparison. Adding the quantity wrapper's head-one
test to the public release wrapper invents an absent check. Treating
the wrapper's scalar mismatch as side-effect free ignores prior selection.

## How to reproduce

At revision 724f4d96 require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus each Locations span separately in sixteen-bit mode.
Require 82/69 bytes and 34/30 instructions. Follow selector inputs,
head far-pointer binding, release result, dirty/head/scalar ordering,
later helper's AX, wrapper gate/selection/scalar comparison and its
word-pop cleanup. Use FND-EXE-411/417/414/413 for dependencies
and the contrasting head test. Preserve caller, alias, writer and
native-effect gaps. Licensed bytes remain outside Git; no game,
DOSBox or emulated call runs.

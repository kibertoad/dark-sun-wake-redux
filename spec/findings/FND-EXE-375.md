---
id: FND-EXE-375
title: Sound utility smaller-buffer helper retains old segment after unchecked terminal calls
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1CD9..1000:1D3D
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-374 calls near 1CD9 when old segment BX has word-zero
count CX greater unsigned than encoded requested units AX. This helper
compares BX with current CS:198E. It performs no independent check of
that caller comparison or of the segments and field extents.

For unequal segments, it forms DI=BX+AX at word width and loads ES
from DI. It computes SI=CX-AX at word width, stores that remaining
quantity to ES:0000 and old BX to ES:0002. It holds this ES segment
and requested AX on the stack, reloads ES from old BX and writes AX
to old word zero. It then forms DX=BX+CX at word width and loads ES
from DX. A nonzero word two there is replaced with DI; otherwise word
eight is replaced with DI. These stores precede the subsequent call.

It sets SI to old BX and makes a far call to 1ACC using the two held
words as segment and offset arguments. Thus the segment argument is
the newly formed DI segment, while the offset argument is the encoded
requested quantity. FND-EXE-367 reads this callee's segment-only
dispatch and local SI preservation; its incoming offset is ignored there.
The caller removes four argument bytes, discards the returned pair,
copies current SI into DX, sets AX=0004 and returns near. Under the
callee's ordinary SI-preserving continuation this is old-segment:0004.

For BX equal to CS:198E, it holds old BX on the stack, loads ES
from BX and stores requested AX to old word zero. It adds AX to BX
at word width and calls near 1E34 with that computed segment and offset
zero. After four-byte argument cleanup it pops the held old segment
into DX, sets AX=0004 and returns near without testing the result.
FND-EXE-369 reads the terminal helper's gates and state changes.

Both branches update the old count before their terminal calls and have
no local rollback. The unequal branch also writes the remaining segment's
two fields and a later segment's selected link field before its call.
All computed segment sums wrap at word width. The helper itself saves
neither DS nor ES; the outer root's shared DS restoration and SI/DI
restoration are recorded in FND-EXE-374. No interrupt occurs in this
local body. Calls may reach the external paths described by the cited
findings; those results and preservation require separate admission.

## Interpretation

This resolves 1CD9's local branch order and held arguments. The
comparison with CS:198E selects different terminal inputs and different
field updates, while both return the retained old segment on ordinary
continuation. The returned pair does not certify a terminal outcome or
undo earlier field changes. Requested units, remaining units and later
segment are distinct values; their arithmetic establishes no extent.

Q-EXE-007 retains the outer entry's callers, old/count/link producers,
CS:198E writers and lifetime, actual segments and extents, aliases,
native preservation/results in downstream paths and shared-state re-entry.
No successful shrinking, capacity contract or complete reading is claimed.

## Alternatives

Treating the terminal result as the helper's result ignores its discarded
AX and explicit old-segment return. Treating field changes as conditional
on terminal success reverses their order. Treating the held requested word
as a meaningful offset ignores the segment-only callee's local reads.
Treating either computed segment as an admitted extent assumes bounds
and alias guarantees not present in this helper.

## How to reproduce

At revision 1ea64df require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x000030D9..0x0000313D at IP 1CD9,
modeled CS 1000 and MZ header size 1400, with locked Capstone 5.0.7
in sixteen-bit mode. Bind FND-EXE-374's AX/BX/CX inputs, trace both
segment-comparison branches, ordered field writes, held words and SI,
near/far call frames, argument cleanup and explicit returned pair.
Keep downstream native outcomes, aliases and extents open. Original
bytes remain outside Git; no original process, DOSBox or emulated call
is executed.

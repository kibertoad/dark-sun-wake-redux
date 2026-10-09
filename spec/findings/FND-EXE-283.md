---
id: FND-EXE-283
title: Selected-segment cleanup publishes head state before its bounded pair-setter call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1367..1000:13CA
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The selected near helper compares incoming DX to CS-relative word
0x135B. Equality clears CS-relative words 0x135B, 0x135D and 0x135F,
in that order. It reloads DS from CS-relative word 0x1361, pushes DX
and a zero low word, then calls 1000:17F2. After that call it removes
four argument bytes and near-returns without rewriting AX.

Inequality loads DS from DX, then loads DS from that segment's word
two. If the newly selected DS-relative word two is nonzero it publishes
this DS to CS-relative word 0x135D and reaches the same shared-DS reload
and DX/zero argument preparation. DX has not locally changed on this arm.

If that word two is zero it compares the new DS segment to CS-relative
word 0x135B. Equality reloads DX from the shared word and enters the
three-word clearing path. Otherwise it reads current DS-relative word
eight into AX and publishes it to CS-relative word 0x135D. It pushes
current DS and a zero word, then calls 1000:143B. Those two words are
arguments prepared for the later pair-setter, not arguments consumed by
the unlink helper's ordinary near return. After unlinking it reloads DS
from CS-relative word 0x1361 and calls 1000:17F2 using the retained pair.
It removes four argument bytes and near-returns without rewriting AX.

The DS load through word two reads through the segment selected by
incoming DX; the subsequent word-two and word-eight reads use the
newly loaded DS. The unlink path's saved high argument is the segment
before that call, independently of any later DS mutation. All shared
publications precede the pair-setter call. There is no local restoration
of those publications when the pair setter rejects.

## Interpretation

FND-CONFIG-167 already records the far wrapper's selection of this helper
and the pair setter's result handling. This finding supplies the precise
branch publications and outgoing-pair provenance beyond that summary.
FND-EXE-273's unlink suffix consumes only its near return frame, so the
two prepared words remain for the later call; its ordinary DS restoration
does not replace the saved stack argument.

The pair setter can reject under FND-CONFIG-167's local bounds or under
FND-EXE-278's updater result. This helper still retains its preceding
head-state changes and forwards returned AX. Q-EXE-001 and Q-EXE-010
retain incoming segment provenance, header/state writers, shared saved-DS
lifetime, aliases, interrupt effects and all callers. No initialized
extent, rollback guarantee or complete reading is established.

## Alternatives

Treating every pair-setter request as the incoming DX ignores the arm
that saves the linked DS segment instead. Treating BP or a private stack
save as the restored data segment ignores the shared CS-relative slot.
Treating the unlink call as consuming the prepared argument pair conflicts
with its ordinary near-return contract. Pair-setter rejection does not
locally undo the earlier head publications.

## How to reproduce

At revision 4ee0507, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open range 0x00006567..0x000065CA
at initial IP 0x1367. The MZ header is 0x5200 and model load segment
0x1000. Follow each equality/inequality gate, qualifying accesses by
their current CS or DS. Track the two outgoing words through the unlink
call and common pair-setter suffix. Cross-check FND-EXE-273's return
and FND-CONFIG-167's wrapper and pair-setter contracts. No original
execution or complete writer/caller search is made; source bytes and
analysis output stay outside Git.

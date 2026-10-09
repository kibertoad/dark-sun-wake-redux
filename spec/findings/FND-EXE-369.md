---
id: FND-EXE-369
title: Sound utility terminal buffer helper separates pair bounds from sentinel-selected state updates
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1E34..1000:1E73
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:06C2..1000:06E3
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1DBE..1000:1E34
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:25FD..1000:2619
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-367's matching path calls near 1E34 with offset zero and
one of its selected segment words. The helper loads the pair at current
DS:0087/0089 into BX/CX and the incoming pair into AX/DX, then
calls near 06C2. Carry set selects AX=FFFF. Otherwise it repeats with
the pair at DS:008F/0091; unsigned above selects the same failure.
The encoded lower and upper comparisons therefore include equality.

The 06C2 helper converts each offset into its low nibble and adds its
unsigned offset divided by sixteen to its segment at word width. It first
compares the resulting DX/CX segment words, then compares the AX/BX
nibbles only when those words match. It near-returns with those comparison
flags. Segment addition wraps; the helper does not establish hardware
address equivalence, readable extents or an admitted ordering of the bounds.

An incoming pair inside those encoded bounds is passed to near 1DBE.
That callee removes four incoming bytes on return. Returned AX zero
selects 1E34's FFFF result; any nonzero AX selects zero. The root restores
BP and near-returns without incoming argument cleanup. FND-EXE-367's
caller subsequently removes its four bytes and does not test this result.

The 1DBE helper saves SI and forms a word-width quantity from incoming
segment plus one, minus current DS:007B, plus 003F, then shifts it right
by six. Equality with DS:DF06 skips its native wrapper. That path writes
the incoming segment to DS:008D and incoming offset to DS:008B and
returns AX=0001 after restoring SI/BP and removing four argument bytes.

On inequality it shifts the quantity left by six, adds DS:007B at word
width and compares the sum unsigned with DS:0091. If above, it replaces
SI with DS:0091 minus DS:007B at word width. It passes DS:007B and
current SI to 25FD, then removes four argument bytes and copies returned
AX into DX. Returned FFFF writes current SI divided by 64 to DS:DF06,
then publishes the incoming pair and returns one as above. Any other
returned word updates DS:0091 to DS:007B plus that word at word width,
clears DS:008F and returns AX=0000. Thus the root's failure result can
follow a bound-pair update rather than unchanged state.

The 25FD wrapper forms BP, sets AH=4A without separately initializing
AL, loads BX from its second argument and ES from its first, then executes
interrupt 21. Carry clear returns AX=FFFF. Carry set pushes post-interrupt
BX, passes post-interrupt AX to 04CE, then pops the saved BX word into
AX. FND-EXE-362 reads that mapper and its two-byte argument cleanup.
The wrapper restores BP and far-returns without incoming cleanup. It does
not locally save DS, SI, DI or ES.

Consequently the carry-set wrapper result is the saved post-interrupt BX
word, not the mapper's FFFF AX. A carry-set BX equal to FFFF would
also select 1DBE's FFFF branch; this reading does not establish whether
that native value is admitted. Current SI and DS used after the call also
require native preservation/state admission before they can be identified
with the earlier local values.

## Interpretation

This resolves the local terminal helper left open by FND-EXE-367.
The pair-bound gate, rounded word arithmetic, native carry result, returned
word sentinel and later pair publication are separate decisions. The root
normalizes the local one/zero result into zero/FFFF, while its caller ignores it.

The arithmetic alone establishes no allocation unit, valid capacity, physical
address ordering or successful native operation. In particular the sentinel
test is not itself a preserved carry-status test, and the root failure path
may retain changes to the stored bound pair. No external service meaning
is assigned to the observed selector here.

Q-EXE-007 retains bounds/base/quantity writers, actual DS, native
arguments/results and preservation, segment/extent admission, aliases,
lifetime and shared-state re-entry. No complete-reading promotion or
launch exclusion follows.

## Alternatives

Treating the comparison as nonwrapping physical-pointer ordering ignores
the segment-word addition. Treating every root failure as pre-mutation
rejection ignores the changed bound pair after a non-FFFF wrapper result.
Treating carry-set AX as the mapped error ignores the saved-BX pop.
Treating FFFF as necessarily native carry clear assumes an admitted BX
range not established by this local caller.

## How to reproduce

At revision bcf685e require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003234..0x00003273 at IP 1E34,
0x00001AC2..0x00001AE3 at IP 06C2,
0x000031BE..0x00003234 at IP 1DBE, and
0x000039FD..0x00003A19 at IP 25FD, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-367's distinct pairs, track comparison
flags, word-width rounding and both post-wrapper state paths. Follow saved
post-interrupt BX through error mapping and its final AX pop, rather than
assigning the mapper's AX to the wrapper return. Keep native preservation
and stored-state admission separate from the local arithmetic.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.

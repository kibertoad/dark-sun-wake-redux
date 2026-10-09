---
id: FND-EXE-367
title: Sound utility bit-four buffer helper dispatches by segment and retains arguments across link adjustment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1ACC..1000:1AF5
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1998..1000:19FB
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1A6C..1000:1A95
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-357 and FND-EXE-358 pass record words eight/ten to 1ACC
when record word two has bit four set, and do not test its result. The
helper saves BP/SI/DI and writes incoming DS to CS:1992. It loads the
incoming segment word at SS:BP+08 into DX; it never reads the incoming
offset at SS:BP+06. Segment zero skips both callees. A nonzero segment
equal to CS:198E calls near 1998; another nonzero segment calls near
19FB. The suffix reloads DS from CS:1992, restores DI/SI/BP and
far-returns without incoming cleanup or a normalized AX result.

The matching-segment 1998 path compares DX with CS:198C. Equality
clears CS:198C, CS:198E and CS:1990, then restores DS from CS:1992
and calls near 1E34 with offset zero and segment DX. It removes four
argument bytes and near-returns. These global clears precede the unread
terminal callee and are not locally conditional on its result.

Otherwise it loads DS from DX, then replaces DS with the word at
that segment's offset two. It tests word two in this newly loaded segment.
A nonzero word publishes that current segment to CS:198E, then restores
DS from CS:1992 and calls 1E34 with zero offset and original DX segment.
There is no local validation of either loaded segment or readable extent.

For a zero word two in the newly loaded segment, it compares that segment
with CS:198C. Equality sets DX to CS:198C and selects the three-global
clear path above. Inequality publishes the word at current DS:0008 to
CS:198E, pushes current DS followed by word zero, then near-calls 1A6C.
Those two words remain on the stack across that call. It reloads DS from
CS:1992 and calls 1E34 with the retained zero/current-segment pair,
then removes four bytes and returns. This pair differs from the original
DX pair used by the other local suffix.

The 1A6C helper copies current DS to BX and compares it with DS:0006.
Equality clears CS:1990 and near-returns. Otherwise it loads ES from
current DS:0006 and then DS from the old segment's word four. It writes
ES to the newly loaded DS:0006, writes current DS to ES:0004 and
publishes current DS to CS:1990. It restores DS from BX and near-returns.
It makes no further calls and consumes no incoming argument words; it
therefore leaves 1998's retained pair available for the later 1E34 call.
This body touches no SI or DI but changes BX/ES and flags.

The 1998 body has no result test after 1E34. Its caller 1ACC likewise
tests neither callee's AX. Segment zero does not assign AX locally, so it
is not a defined zero-success return. The CS-relative saved-DS slot is
shared storage rather than a stack-local save; its final reload requires
writer and re-entry admission before it proves restoration of the original DS.

## Interpretation

This resolves 1ACC's local segment dispatch and the matching-segment
branch reached by the bit-four record paths. A supplied offset is not locally
validated or forwarded as such. The matching branch may clear global state
or adjust segment-based links before its terminal call, and the two terminal
argument sources must be kept distinct.

The held pair across 1A6C is established by its local stack behavior, not
inferred from a later cleanup amount alone. No allocation/release unit,
valid link topology, ownership outcome or unchanged-state return follows
without the remaining contracts and admitted segment fields.

Q-EXE-007 retains alternate 19FB, terminal 1E34, CS-relative state
writers and re-entry, segment/link admission, extents, aliases and lifetime.
No complete-reading promotion, native release result or launch exclusion follows.

## Alternatives

Treating the wrapper as a full-pointer validator ignores its unread offset.
Treating the zero-segment branch as AX-zero success assumes a missing
return assignment. Treating every terminal call as using original DX ignores
the pair held from the newly loaded segment. Treating CS:1992 as a local
DS save ignores its shared location and remaining writers/re-entry obligations.

## How to reproduce

At revision 8a1f24b require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00002ECC..0x00002EF5 at IP 1ACC,
0x00002D98..0x00002DFB at IP 1998, and
0x00002E6C..0x00002E95 at IP 1A6C, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-357/358's buffer pair, follow
each DS load before interpreting its subsequent fields, distinguish all terminal
argument pairs, and track the two held words through 1A6C's near return.
Keep the alternate branch, terminal helper and shared-state admission open.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.

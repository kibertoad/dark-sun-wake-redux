---
id: FND-EXE-282
title: Segment-return candidate calls the list writer and falls through to unlink after a merge
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:13CA..1000:143B
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The selected entry loads DS from incoming DX and pushes that DS. It loads
ES from DS-relative word two, clears that word and writes ES to DS-relative
word eight. It compares DX to CS-relative word 0x135B. Equality calls
1000:1464. Inequality tests ES-relative word two; nonzero also calls
1000:1464. That call therefore receives DS equal to incoming DX on these
ordinary paths, apart from aliases or asynchronous effects.

If both comparisons bypass the call, it reads DS-relative word zero into
AX, pops the original selected segment into BX, pushes the predecessor ES
instead, and adds AX to ES-relative word zero at word width. It saves
that ES in CX, adds AX to DX and loads ES from the resulting segment.
If this new ES-relative word two is zero it writes CX to word eight;
otherwise it writes CX to word two. Neither branch calls the list writer.

All paths then pop ES: the call paths restore the original selected
segment, while the merge path restores the predecessor. They add that
ES-relative word zero to ES in AX, load DS from the resulting segment
and test DS-relative word two. Nonzero near-returns immediately.

Zero reads DS-relative word zero into AX and adds it to ES-relative
word zero. It saves the retained ES segment in AX, adds DS-relative
word zero to the current DS segment in BX, loads ES from that result
and writes saved AX to ES-relative word two. It then falls through at
1000:143B into FND-EXE-273's unlink body, with DS still identifying the
segment whose word-two zero selected this final merge. No fresh call
or return frame is introduced at this boundary.

These local additions are word-width and have no independent overflow
rejection. The body writes no DS before the list-writer call other than
its initial incoming-DX load. Header validity and physical independence
are not checked locally. Each stack path has one matched segment push
and pop, with the merge path replacing the saved segment before the join.

## Interpretation

This establishes one direct incoming call and its DS producer for
FND-EXE-281, beyond matching link-field layouts. It also shows why the
shared unlink continuation cannot be read only as the allocation helper's
explicit near call: this caller enters it by fall-through after merging
header counts. FND-EXE-273's unlink paths remain relevant to that retained
DS and the original caller's return frame.

Q-EXE-001 and Q-EXE-010 retain incoming DX provenance, admitted headers,
all other callers and writers, arithmetic bounds, aliases and lifetime.
The cleared word two, copied word eight and count merges do not alone
establish semantic ownership, safe extent or complete list consistency.
No complete reading or original execution is claimed.

## Alternatives

Treating all paths as calling the link writer ignores the predecessor-merge
bypass. Treating the joined ES as always the original segment ignores its
replacement on that bypass. Ending the final merge as a near return
misses the fall-through into the unlink body. Treating the newly added
counts as validated contiguous storage assumes admission not shown here.

## How to reproduce

At revision c0ab5aa, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open range 0x000065CA..0x0000663B
at initial IP 0x13CA. The MZ header is 0x5200 and model load segment
0x1000. Follow both writer-call gates, the predecessor-merge bypass,
each saved-segment stack path and the final immediate-return versus
fall-through paths. Continue the latter with FND-EXE-273, and qualify
every header access by the current DS or ES. Cross-check FND-EXE-281's
incoming DS contract. No complete caller or writer search is made;
source bytes and analysis output stay outside Git.

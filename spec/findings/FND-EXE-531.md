---
id: FND-EXE-531
title: Game resident request wrapper traverses segment links and restores DS from a shared code word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:15A5..1000:1622
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:15A5..1000:1622
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-529's far callee 15A5 saves BP and reads high and
low incoming words from SS:BP+8 and SS:BP+6 into DX and
AX. It ORs a copy of AX with DX into CX, saves SI and
DI and stores current DS at CS:1361. The pushes and store
preserve the OR flags. A zero incoming pair goes directly to the common
return with AX/DX still zero locally.

Nonzero adds 0013 to AX at word width, propagates carry into
DX and rejects carry from that high-word addition. It also rejects any
set bit in DX masked by FFF0. Rejection clears AX and uses
opcode 99, word CWD, to make DX zero before the common return.
Otherwise it shifts AX right logically by four, shifts DX left by
four, and ORs DL into AH. This yields the low word of the
admitted sum shifted right by four; the accepted high word is at most
000F before shifting. There is no locally admitted allocation unit or
backing extent from this numerical transformation alone.

It loads DX from CS:135B. Zero calls near 14C4 then
takes the common return. Nonzero loads DX from CS:135F. Zero
there calls near 1528. Otherwise it captures initial DX in BX,
loads DS from DX and compares DS word zero unsigned with AX.
Smaller reloads DX from DS word six. DX equal to held BX
calls 1528; otherwise it loops at the DS assignment and comparison.
BX is not reloaded on loop entry. The initial-segment sentinel therefore
stops a cycle returning to its head, but not a cycle excluding that
head. There is no local subsequent-null test, extent check or iteration bound.

A word greater than AX calls near 1582 and returns through the
common path. Equality calls near 143B, then reloads BX from current
DS word eight, stores BX into current DS word two, sets AX to
four and continues. DS and DX after 143B are not locally reset
before these operations; the unread helper's return contract is needed to
admit the selected record and returned pair. No result test occurs after
any of the four near calls.

The common path reloads DS from current CS:1361, pops DI,
SI and BP and returns far without incoming cleanup. This is a
shared-word restoration, not a private saved DS stack slot. Nested calls,
aliases and other writers can affect that word or the saved frame; their
preservation is not established by the wrapper. Every near-call result pair
is passed on except that the equality branch replaces AX with four.
The wrapper itself contains no interrupt and does not locally change SS.
Both editions have identical instructions throughout the bounded body.

FND-EXE-529 supplies the unsigned product of its held quantity and
0010 as this incoming pair. Its three cleanup pops account for these
four argument bytes plus its additional held word if the callee stack
contract is intact. It tests returned AX/DX jointly for zero; nonzero
then increments DX and uses it in the next call's arguments. Thus a
word return cannot be interpreted independently of the high word or the
caller arithmetic. This reading does not establish that the pair denotes
valid writable storage.

## Interpretation

This resolves the resident wrapper's local request transformation, segment-link
traversal and result consumption in the startup caller. Q-EXE-007 retains
14C4, 1528, 1582 and 143B, shared code-word and record/link
producers, actual segments, extents, aliases and lifetime, other callers and
remaining startup/native dependencies. No complete allocation, initialization
or launch contract is claimed.

## Alternatives

Calling DS restored from a private save ignores the shared CS word.
Treating every cycle as terminating ignores the held initial sentinel.
Calling the equality result a fixed pair ignores DX's callee dependency.
Treating numerical rounding as valid allocation ignores the unread storage
producers and returned segment contract. Checking AX alone contradicts the
startup caller's joint AX/DX test.

## How to reproduce

At revision 4c65597 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
67A5..6822 in sixteen-bit mode. Track the argument words, OR flags
across saves, both addition carries, high-word mask and shift/merge. Follow
the two shared-word tests, held BX sentinel, every call branch, equality
stores and shared DS reload. Trace the pair through FND-EXE-529's
cleanup and argument formation. Keep near callees and storage admission
explicit. Licensed bytes remain outside Git; no original process, DOSBox
or emulated call runs.

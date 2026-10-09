---
id: FND-EXE-527
title: Game grow helper copies words before cleanup and returns zero on allocation failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:2254..1000:2289
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 1000:2254..1000:2289
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-526's near grow branch at 2254 replaces BP with current
SP without saving BP itself. It pushes incoming BX, AX and CX,
then pushes AX again and CS before calling far-returning 2172 through
a near call. With an intact frame, the three saved words occupy
SS:BP-2, SS:BP-4 and SS:BP-6; the first is incoming BX.
BP-relative reads and writes here use SS, whereas SI-based reads use DS.

It removes the outgoing quantity into BX, replaces BX with returned AX
and tests AX for zero. Zero skips the copy and cleanup, removes six
saved bytes and returns near with BX zero. The outer 22CB replaces
AX from BX, therefore returning zero on this local failure path rather
than returning the original offset. This does not undo any preliminary
request effects in the allocator (FND-EXE-513/515/516).

Nonzero sets ES from DS, clears the direction flag and sets DI to
returned AX. It reads old BX from SS:BP-2 into SI, then reads
word DS:SI into CX. It advances SI by four at word width and
pushes that source offset. It subtracts five from CX at word width,
shifts CX right once logically, and performs repeated forward word copies
from DS:SI into ES:DI. There is no odd-byte tail copy, local underflow
check, overlap check or source/destination extent validation. The numerical
count is bounded by 32767 words, or 65534 bytes, independently of the
input word. SI and DI updates remain word-width; this bound alone does
not establish valid storage or a safe copy.

After the copy it writes current AX into SS:BP-2, pushes CS and
calls far-returning 20A3 using the previously pushed source offset. It
removes that argument into BX, then reloads BX from SS:BP-2,
removes the three saved words and returns near. It does not test cleanup's
AX. Under admitted intact, disjoint frame and callee preservation, the
saved slot now supplies the allocated offset to the outer caller's AX
replacement. Memory aliases can affect the source argument, saved slot or
return frame; their exclusion is not established here.

The outgoing quantity to 2172 is incoming AX, not transformed DX.
In FND-EXE-526's caller this is the original quantity. That allocator
has its own guarded transformation, unlike the outer caller's unguarded
word addition. FND-EXE-513 and FND-EXE-518 record the two wrappers'
DS reads at stack-derived offsets: binding these to the pushed words needs
actual DS/SS equality and intact storage. No unconditional argument or
copy-extent contract follows from the push sequence alone.

Both editions have identical instructions throughout this body. It contains
no local interrupt and does not locally change DS or SS. It does not
restore ES or the direction flag. The outer caller restores its saved
BP, SI and DI after this helper, subject to frame preservation; this
helper's BP replacement is not itself a saved-register restoration.

## Interpretation

This resolves the previously unread immediate grow branch of 22CB.
The local order is allocation, conditional word copy, then old-offset
cleanup. A zero allocation result yields zero through the caller; successful
copy does not make cleanup failure the outer return. Q-EXE-007 retains
outer callers/input producers, actual segments, block/header units and
extents, aliases and lifetime, shared-state writers and earlier startup/launch
coverage. No complete allocation, size-adjustment or launch contract is claimed.

## Alternatives

Returning the old offset on allocation failure contradicts BX's replacement
from zero AX. Copying the incoming requested quantity contradicts the count's
DS metadata source. Copying an odd trailing byte contradicts the word-only
copy. Calling the BP-relative saved slot a DS access ignores the default
SS segment. Treating the count bound as storage admission ignores word
offset wrap, aliases and the unresolved producer contract.

## How to reproduce

At revision 035a875 require both DSUN.EXE identities from FND-EXE-350.
With header size 5200 and modeled load segment 1000, decode shipped
7454..7489 in sixteen-bit mode. Track BP from helper entry, all six
saved bytes, the extra quantity and both manufactured far returns. Resolve
default segments by addressing register. Follow zero AX into BX and the
outer AX replacement using FND-EXE-526. Track the copy count's word
subtraction and logical shift, the retained source argument across the copy,
the saved-slot write before cleanup and the final BX reload. Use
FND-EXE-513/515/516 and FND-EXE-518/519 for immediate callee effects;
keep storage and segment admission explicit. Licensed bytes remain outside
Git; no original process, DOSBox or emulated call runs.

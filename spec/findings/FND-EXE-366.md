---
id: FND-EXE-366
title: Sound utility preliminary cleanup iterator counts selected calls rather than successful results
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:29F8..1000:2A3A
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-363's all-zero record-pair path calls this helper without
arguments, then ignores its result and returns zero. The helper reserves
four local bytes, saves SI/DI, clears DI and snapshots current DS:DD38
into SI. It stores current DS as a local pointer segment and DBA8 as
its offset. It does not independently admit the segment, limit or table extent.

At the loop guard it copies current SI to AX, decrements SI at word
width, and tests the copied AX for zero. Nonzero enters the body; zero
selects the return suffix. Thus an initial zero limit reads no record despite
the local decrement, and every nonzero word is treated as a countdown
quantity rather than a signed positive count.

The body loads ES:BX from the local pointer and tests bits one and two
in record word two together. If neither bit is set it skips the call and DI
increment. Otherwise it passes the local pointer segment and current offset
to 292B, removes four argument bytes and increments DI without testing
returned AX. Both paths then add twenty to the local offset at word width
and return to the countdown guard. No segment carry, pointer-bound check
or repeated DS:DD38 read occurs locally.

The suffix copies current DI to AX, restores saved DI/SI, restores the
frame and far-returns without incoming argument cleanup. It does not
normalize DX. The result therefore counts selected calls on ordinary
continuation, not zero results, completed releases or records cleared.

For an initial word N, with noninterfering local state, returning callees
and preserved countdown/count registers, the body runs N times and makes
at most N calls. N can range from zero through 65535 locally. At iteration
k the encoded pointer offset is (DBA8 + 20*k) modulo 65536, with the
captured segment unchanged. This arithmetic bound does not establish a
readable table of N records or prevent repeat/wrapped offsets.

The records are tested as reached; the iterator does not snapshot their
flags before the first call. A cleanup call may change state that later
record tests observe. FND-EXE-380's shipped limit twenty and first-five
byte initializers do not establish the actual limit, segment or flags at
this iterator's entry.

## Interpretation

This resolves the local iteration body reached by FND-EXE-363's zero-pair
case. That caller discards even the selected-call count. Neither its zero
return nor the iterator's count establishes successful native cleanup.
The count snapshot, per-record flag reads and offset progression are
separate admission obligations rather than a validated table traversal.

The nested 292B call can itself reach the all-zero-pair path. This reading
does not prove that path impossible for every admitted iterator pointer or
establish recursion depth. Actual segment/limit admission and wrapped-pointer
behavior remain required before excluding re-entry.

Q-EXE-007 retains callers, segment/count/flag writers, table extents,
callee preservation, aliases, lifetime and re-entry admission. No complete-reading
promotion, successful-release count or launch exclusion follows.

## Alternatives

Treating AX as successful-cleanup count ignores the absent callee-result test.
Treating the limit as rechecked every iteration ignores the single snapshot.
Treating a negative-looking word as a rejected count ignores the zero-only
guard. Treating the local offset advance as a validated far-pointer step
ignores its unchanged segment and absent extent check.

## How to reproduce

At revision f4d486d require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00003DF8..0x00003E3A at IP 29F8, modeled CS 1000,
MZ header size 1400. Follow the one-time count/segment capture, test of
the pre-decrement word, per-record bit test, ignored AX result, DI increment
and low-word pointer step. Bind FND-EXE-363's zero-pair call and ignored
iterator result separately. Keep frame aliases, preserved registers and
pointer/re-entry admission open rather than assuming them from a local loop.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.

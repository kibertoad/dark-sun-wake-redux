---
id: FND-EXE-365
title: Sound utility byte processing checks its batching threshold after newline expansion
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:3B6D..1000:3C54
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-364 reaches this branch when indexed state has bit 4000 set.
It clears bit 0200 in the indexed word, copies the incoming source pair
into SS:BP-0C/0A and the incoming count into SS:BP-06, then initializes
the output cursor at SS:BP-8E using the pair at SS:BP-04/02.
The outer entry reserved 008E local bytes. Source and output offset
increments are word-width operations without segment adjustment.

Each iteration decrements the remaining count, loads the current source
byte and increments the source offset. It retains the byte at SS:BP-07.
Byte 0A first causes a 0D store at the output cursor and an offset
increment. Every input byte then causes its own retained-byte store and
another cursor increment. No terminator is appended by this branch.

Only after those stores does it subtract the local base offset from the
cursor offset, extending unsigned borrow into a high word initially zero.
A negative high word skips submission. Otherwise the low difference is
compared unsigned with 0080; below that threshold also skips submission.
Thus a wrapped cursor below the base does not follow the ordinary positive
threshold path. No prior output-capacity or source-extent check occurs here.

Under nonwrapping, noninterfering input/frame state and ordinary helper
preservation, a pending prefix below 128 bytes can receive two bytes for
one input before this check. It can therefore reach 129 bytes before
submission, with the last byte at SS:BP-0E. A threshold of 128 is not a
128-byte maximum production bound. These conditions are not admitted by
this finding; source/frame aliases or native changes can invalidate them.

The submission path loads SI with the low-word pending difference, pushes
that count, SS and the local base offset, then current DI, and calls 3C54.
It removes eight bytes and copies returned AX to DX. Equality with current
SI resets the output cursor to the local base and continues. On inequality,
returned FFFF selects AX=FFFF. Any other unequal result selects the
word-width expression incoming count minus current remaining count plus
returned word minus current SI, then returns through the common suffix.

After remaining count reaches zero, it computes the low-word pending
difference again. A zero difference returns the incoming count without
another call. A nonzero difference makes a final 3C54 call with the same
argument structure. Equality with current SI returns the incoming count.
On inequality, FFFF selects failure; any other result returns incoming
count plus returned word minus current SI, all at word width. Equality is
tested before the FFFF sentinel on both submission paths.

The SI compared after each call is a register value, not a separately
reloaded copy of the outgoing count. FND-EXE-364's 3C54 does not save
SI around its interrupt, so native SI preservation remains required before
equating that comparison value with the submitted count. Current DI also
supplies each next request. The suffix restores the outer saved DI/SI,
restores its frame and far-returns without incoming argument cleanup.

## Interpretation

This resolves the local byte-processing branch left open by FND-EXE-364.
Under admitted ordinary state, each source byte produces one or two output
bytes, whereas the all-equal completion returns the input count. The unequal
result paths combine an input-progress count with an output-chunk result;
they are not simply a total emitted-byte count.

The threshold is tested after expansion, and its borrow handling differs
from a wrap-safe capacity calculation. A conditional 129-byte chunk bound
does not establish a writable extent, noninterference or successful native
submission. No claim of a buffer corruption or native result follows solely
from this local reading.

Q-EXE-007 retains native results and SI/DI/DS preservation, input/frame
admission, source and output extents, indexed-state writers, aliases and
lifetime. FND-EXE-363's outer comparison remains a separate decision,
including its bit-0200 mismatch exception. No complete-reading promotion
or launch exclusion follows.

## Alternatives

Treating 128 as the maximum produced chunk ignores the extra byte stored
before the threshold check. Treating successful completion as emitted-byte
count ignores the incoming-count return after expansion. Treating the partial
result expressions as pure input progress ignores the returned output word
and current-SI subtraction. Treating post-call SI as necessarily the submitted
count assumes a native preservation contract the wrapper does not establish.

## How to reproduce

At revision 97c2973 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
With locked Capstone 5.0.7 in sixteen-bit mode decode shipped half-open
0x00004F6D..0x00005054 at IP 3B6D, modeled CS 1000,
MZ header size 1400. Bind FND-EXE-364's frame and arguments, follow
both stores before the threshold, and distinguish borrow from the ordinary
positive difference. Derive the 127-byte prefix plus two-byte case without
assuming its destination is admitted. Track both calls' current-SI comparisons,
sentinel ordering, remaining-count reads and word-width return arithmetic.
Original bytes and reports remain outside Git. No original process,
DOSBox, interrupt thunk or emulated call is executed.

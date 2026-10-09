---
id: FND-EXE-280
title: Initial allocation helper retains zero-size and alignment call effects before its final test
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:14C4..1000:1528
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near helper saves incoming AX, loads DS from CS-relative word
0x1361 and calls 1000:1831 with two zero argument words. It removes
four outgoing argument bytes and masks returned AX to its low nibble.
Zero bypasses alignment. Nonzero forms sixteen minus the nibble in DX,
reloads DS from the same shared slot and calls 1000:1831 with a zero
high word and that low word. It removes four argument bytes without
testing or retaining the returned pair. The first result is not tested
against 0xFFFF before its nibble controls this second call.

Both paths restore the original incoming count into AX and save it again.
They form a two-word request equal to that unsigned count multiplied by
sixteen, reload DS from CS-relative word 0x1361, push the high then low
words and call 1000:1831. After removing four argument bytes they pop
the original count into BX and test returned AX against 0xFFFF.

Equality zeros AX and sign-extends its word value into DX, then near-returns.
No local release, restoration of earlier state or shared-word publication
occurs on this rejection suffix. The earlier calls' effects are retained
unless their own contracts undo them; this body performs no such undo.

Otherwise it publishes returned DX to CS-relative words 0x135B and
0x135D, in that order, loads DS from DX, writes the original count to
DS-relative word zero and returned DX to word two, sets AX to four
and near-returns. It does not locally write CS-relative word 0x135F.
Every request reloads DS from the shared slot at the time of that request;
these are not independent private saves of the helper's incoming DS.

## Interpretation

FND-EXE-272 selects this helper when CS-relative word 0x135B is zero.
This body supplies a conditional writer of that word and of word 0x135D.
Its successful local result is offset four with the final call's segment
word, while DS remains loaded from that segment until the caller's shared
restoration. No initialized allocation extent follows from those stores.

FND-EXE-276 through FND-EXE-279 show that an ordinary request rejection
can follow state publication by lower callees. The first all-ones result
has nibble fifteen and therefore selects an alignment request of one;
an alignment rejection does not bypass the final full-size request here.
Neither ignored result proves successful acquisition or unchanged state.
Q-EXE-001 and Q-EXE-010 retain admitted state, remaining writers,
interrupt effects, shared-slot lifetime and physical aliases. This is
a local callee-body reading, not complete allocator or loader closure.

## Alternatives

Treating the three calls as one checked allocation loses the zero-size and
alignment continuations. Treating the first all-ones result as an immediate
exit contradicts the nibble test. Treating a final rejection as rollback
ignores the earlier calls and absent local undo. Treating shared DS reloads
as restoring a private incoming value ignores the shared slot's writers.

## How to reproduce

At revision db4a57f, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open range 0x000066C4..0x00006728
at initial IP 0x14C4. The MZ header is 0x5200 and model load segment
0x1000. Follow both nibble paths, each call's argument cleanup, the final
failure test and ordered state/header stores. Retain the effective word
sign extension controlled in FND-EXE-258. Cross-check FND-EXE-272's
gate and FND-EXE-276 through FND-EXE-279's request continuations.
No original execution or complete writer/caller search is made; source
bytes and analysis output stay outside Git.

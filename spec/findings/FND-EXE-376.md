---
id: FND-EXE-376
title: Sound utility preliminary configuration adjusts quantity before unchecked caller continuation
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2CC8..1000:2D48
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2C46..1000:2CC8
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:05CC..1000:05F5
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-357 calls far-returning 2CC8 with a record pair, zero quantity
and mode word one, removes ten argument bytes and ignores the result.
This helper saves SI, loads the mode from SS:BP+0E and calls 292B
with the record pair. After four-byte argument removal it tests full AX.
Any nonzero AX returns FFFF through SI/BP restoration without reaching
the following stores. FND-EXE-363 records the first callee's own state
changes; this early result does not imply unchanged incoming state.

On AX zero, current SI equal to one and record word zero signed greater
than zero select near 2C46 with the record pair. That callee removes its
four incoming argument bytes. Returned AX is sign-extended into DX and
subtracted with borrow from incoming quantity words BP+0A/+0C, which
are modified in the caller's argument area. Other mode/count combinations
skip this adjustment. There is no result-range test before sign extension.

The 2C46 helper forms a four-byte local frame and saves SI. For a
negative signed record word zero W it computes CX/SI=(record word six
+ W + 1) modulo 65536. For a nonnegative W it selects CX/SI=W
through a local sign-extension/absolute-value sequence. Record word two
bit 0040 skips scanning and returns current SI. Otherwise it snapshots
the record pair at words twelve/fourteen into its frame.

It reloads record word zero to select scan direction. A negative value
decrements the local offset before each byte read; a nonnegative value
reads the current offset and then increments the local offset. Neither
adjusts the segment for offset wrapping. Both loops use a captured CX
word count: each iteration tests its old value and decrements it, stopping
when the old value was zero. Each byte equal to 0A increments SI at
word width. The helper returns SI in AX, restores saved SI/BP and removes
four incoming bytes. It checks no source extent or count/return overflow.
The signed direction reload and initial count can differ if state interferes.

Back in 2CC8 it reloads the record pair, clears word-two bits 0020,
0080 and 0100 with mask FE5F, clears word zero and copies the pair
at words eight/ten into words twelve/fourteen. It pushes current mode,
the adjusted high/low quantity and the sign-extended record byte four,
then calls 05CC. It removes eight incoming bytes. Only returned pair
FFFF:FFFF selects AX=FFFF; every other pair selects AX=0000.
It restores SI/BP and returns far without incoming argument cleanup.
These result tests do not undo preceding record or argument-area stores.

The 05CC wrapper loads its incoming first word, doubles it at word
width and clears bit 0200 in the current DS indexed word at displacement
DD3A. There is no local index-range check. It then sets AH=42, takes
AL from the incoming mode's low byte, BX from the first word, CX from
the high quantity and DX from the low quantity, and executes interrupt
21. Carry clear preserves the returned AX/DX pair. Carry set passes AX
to 04CE and sign-extends its returned FFFF into DX; FND-EXE-362
reads that mapper's two-byte incoming cleanup. The wrapper restores BP
and returns far without incoming cleanup. It does not locally save DS,
SI, DI or ES, or test a carry-clear pair for a sentinel collision.

## Interpretation

This resolves the remaining preliminary call in FND-EXE-357's configuration
path. Local early failure and later pair failure have different preceding
changes; the configuring caller ignores either result. The scanned byte
count and newline-adjusted return are distinct quantities. A wrapped AX
with its sign bit set becomes a negative double-word adjustment, even
though local scanning increments a word. This describes the arithmetic,
not whether such an input is admitted by actual producers.

Q-EXE-007 retains record/count and mode producers, scanned extents and
aliases, incoming argument/frame admission, actual DS/index state, native
preservation/results, caller continuation and shared-state lifetime/re-entry.
No successful native operation, capacity contract or complete reading follows.

## Alternatives

Treating early failure as no mutation overlooks 292B's prior changes.
Treating an adjusted count as unsigned ignores the caller's sign extension.
Treating the later sentinel as a pre-store guard reverses record-write order.
Treating a carry-clear native pair as necessarily accepted ignores the
explicit FFFF:FFFF comparison. Treating mode as a complete word consumed
by the native selector ignores the wrapper's low-byte load.

## How to reproduce

At revision 7813558 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x000040C8..0x00004148 at IP 2CC8,
0x00004046..0x000040C8 at IP 2C46 and
0x000019CC..0x000019F5 at IP 05CC, modeled CS 1000 and
MZ header size 1400, with locked Capstone 5.0.7 in sixteen-bit mode.
Bind FND-EXE-357's argument order, trace the first full-word result,
count/direction reloads, scan offset wrapping, local and incoming frames,
sign extensions, ordered stores, wrapper register inputs and full-pair test.
Extensions at 2C68, 2CFA and 05F2 are sixteen-bit DX extensions;
2D2A is a sixteen-bit AL extension despite wider decoder mnemonics.
Original bytes stay outside Git; no original process, DOSBox, interrupt
thunk or emulated call is executed.

---
id: FND-EXE-471
title: Sound utility refill resets current pointer before storing and classifying returned count
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2EF1..1000:2F69
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2EB3..1000:2EF1
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-379 calls near 2EF1 with a record pair when record word six
is nonzero. It enters the cached-byte path only for returned AX zero.
This helper forms BP, loads the record and tests word-two bit 0200.
A set bit calls near 2EB3 without pushed arguments or a result test.
It reloads the incoming record pair after that optional call.

It holds record word six as the count argument, copies the pair at words
eight/ten into words twelve/fourteen and pushes that same segment/offset
as the destination. It sign-extends record byte four and calls far-returning
3720 with these arguments, then removes eight incoming bytes. This pointer
reset precedes the lower read's result. The lower helper's successful-data
and preservation contract is not established by this finding.

After reloading the record pair it writes full returned AX into word zero.
Signed AX greater than zero clears word-two bit 0020 and returns AX zero.
For a nonpositive AX it reloads the record again and tests current word
zero. Zero clears bits 0080 and 0100, sets bit 0020 and returns FFFF.
Nonzero clears word zero, sets bit 0010 and returns FFFF. Thus a negative
returned word is first published as the count and then locally replaced
with zero on ordinary noninterfering continuation. No prior-pointer rollback
or independent capacity check occurs.

The helper restores BP and returns near while removing its four incoming
argument bytes. It does not save SI, DI, DS or ES. Its local zero result
requires a positive signed lower count, rather than merely any nonzero
word. FND-EXE-379 subsequently reloads, decrements the current count and
reads the current pair; this contract does not exclude intervening aliases
or re-entry, or establish that the returned count fits the destination.

The optional 2EB3 pass forms a four-byte local frame, saves SI, initializes
SI to 0014 and snapshots current DS with offset DBA8. Each iteration
loads a record at the current frame pair and selects it only when word-two
bits 0100 and 0200 are both set. Selected records are passed to 292B;
the caller removes four argument bytes and ignores full returned AX.
FND-EXE-363 reads that callee's local mutations and SI restoration.
Every iteration advances the frame offset by 0014 at word width without
segment carry. It tests the old SI value and decrements SI; old zero ends
the pass. It restores saved SI, SP and BP and returns near.

Under admitted noninterfering frame state and ordinary callee continuation,
the pass examines twenty records separated by twenty bytes from its saved
segment:DBA8. It does not load the mutable limit at DS:DD38 used by
other helpers, validate this table extent or produce a tested success count.
All reached record results are discarded. No direct interrupt occurs in
either local body; reached lower paths remain separate dependencies.

## Interpretation

This resolves the buffered refill branch and its optional preliminary pass
left open by FND-EXE-379. Pointer publication, count publication, signed
count classification and later cached-byte consumption are distinct steps.
Local failure retains the pointer reset and may follow other record changes.
The fixed pass count is separate from other helpers' mutable index bounds.

Q-EXE-007 retains 3720 and its lower read contract, source and destination
extents, record/count/flag producers, actual segments, fixed-table admission,
native preservation/results, aliases, frame state and re-entry. The separate
uncached auxiliary helper 27FC and later pathname continuation remain open.
No successful refill, capacity contract or complete reading is claimed.

## Alternatives

Treating refill failure as pre-mutation rejection ignores the pointer reset
and count store. Treating any nonzero lower count as accepted overlooks
the signed positive test. Treating the preliminary pass as bounded by the
mutable global count overlooks its immediate 0014. Treating selected-call
completion as success overlooks every discarded 292B result. Treating the
fixed iteration count as proof of readable table extent assumes admission.

## How to reproduce

At revision 93bbaba require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x000042F1..0x00004369 at IP 2EF1
and 0x000042B3..0x000042F1 at IP 2EB3, modeled CS 1000,
MZ header size 1400, with locked Capstone 5.0.7 in sixteen-bit mode.
Follow the optional call, record-pair reloads, destination/count arguments,
pre-call pointer reset, returned-count publication and signed branches,
then the pass's captured pair, immediate loop count and discarded results.
The extension at 2F1F is sixteen-bit AL sign extension despite a wider
decoder mnemonic. Original bytes remain outside Git; no original process,
DOSBox, interrupt thunk or emulated call is executed.

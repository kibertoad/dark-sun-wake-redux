---
id: FND-EXE-379
title: Sound utility record reader distinguishes unsigned cached bytes from full-word failure
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:2F81..1000:306F
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-377's matcher and pathname continuation call far-returning 2F81
with a record pair. It saves SI and rejects an all-zero incoming pair with
AX=FFFF. A nonzero pair loads ES:BX and tests record word zero signed.
A positive word directly selects a cached-byte path, before any flag tests.
That path reloads the record, decrements word zero, holds the segment and
offset from words fourteen/twelve, increments word twelve at word width
and reads the byte at the held segment:offset. It sets AH zero before
restoring SI/BP and returning far without incoming argument cleanup.
The count and pointer stores precede the byte read; offset wrap does not
adjust the segment. No independent record or source extent check occurs.

For the initial nonpositive count it reloads the record and rejects a
currently negative word zero, any word-two bit in 0110, or a clear bit
0001. Rejection sets word-two bit 0010 and returns FFFF. Otherwise it
sets bit 0080 and tests word six. A nonzero word six calls near 2EF1
with the record pair. Returned AX zero enters the cached path without
retesting its count or flags; nonzero returns FFFF. The refill contract
and its four-byte incoming cleanup remain dependencies of this local
root reading rather than established results here.

Word six zero selects the other route. It reloads the record and, if
word-two bit 0200 is set, calls near 2EB3 without pushed arguments
and without a local result test. It then calls 3720 with sign-extended
record byte four, current DS:EDFA and count one. It removes eight
incoming bytes and tests full AX only for zero. Any nonzero result
selects the current shared byte at DS:EDFA; no FFFF test or comparison
with requested count one occurs locally. The called helper's output and
preservation contract remain open.

On zero AX, it reloads record byte four, sign-extends it and calls 27FC,
then removes two bytes. Returned AX equal to one clears record bits
0080 and 0100, sets bit 0020 and returns FFFF. Every other value sets
bit 0010 and returns FFFF. These local flag changes follow the reached
callees; the auxiliary result meaning is not assigned by this reading.

On nonzero 3720 AX, shared byte 0D with record word-two bit 0040
clear repeats from the word-six-zero route. It may therefore repeat the
conditional 2EB3 call and the one-byte request. There is no independent
iteration cap or local progress/termination check. Other bytes, or 0D
with bit 0040 set, clear record bit 0020 and return the current shared
byte with AH zero. Shared DS and byte lifetime across these calls require
preservation and alias admission; the byte is not held across the flag test.

Thus both cached and continuing shared-byte paths return unsigned values
0000..00FF. Cached byte FF returns 00FF, distinct from full FFFF.
FND-EXE-377's matcher tests full FFFF, whereas its later pathname loop
stores AL and treats byte FF as a delimiter. A positive cached count
also bypasses the nonpositive path's flag rejection and does not clear
bit 0020 locally. The helper has no direct interrupt; it reaches the
unread callees listed above and does not save DS, DI or ES.

## Interpretation

This resolves the local reader root behind FND-EXE-377 and separates
word-valued failure from byte-valued data. Counts, pointer advances and
flag stores can precede a later caller decision. The cached path's
positive-count admission and refill-zero entry are distinct paths; neither
proves an initialized readable extent. The uncached delimiter handling is
a loop inside the helper, not recursion.

Q-EXE-007 retains 2EF1/2EB3/3720/27FC, record/count/flag and shared
byte producers, incoming segment and pointer admission, callee cleanup and
preservation, extents, aliases and re-entry. Later pathname continuation
also remains open. No complete reading or execution exclusion is claimed.

## Alternatives

Treating FF data as full-word failure ignores AH zeroing. Treating flag
rejection as universal ignores the earlier positive-count path. Treating
refill AX zero as proof of a positive count adds a test not performed here.
Treating every nonzero one-byte-request result as successful data assumes
the unread lower contract. Treating repeated 0D removal as bounded relies
on progress and source termination not admitted by this local loop.

## How to reproduce

At revision 2ac91d0 require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00004381..0x0000446F at IP 2F81,
modeled CS 1000 and MZ header size 1400, using locked Capstone 5.0.7
in sixteen-bit mode. Track the signed count gates, pre-read stores,
held source pair, refill result, word-six-zero loop, every pushed argument,
zero-only lower result test, auxiliary result and byte/full-word suffixes.
Extensions at 300C and 3021 are sixteen-bit AL extensions despite wider
decoder mnemonics. Keep all four called contracts open. Original bytes
remain outside Git; no original process, DOSBox or emulated call runs.

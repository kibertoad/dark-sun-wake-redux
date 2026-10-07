---
id: FND-EXE-123
title: Optional second-mode scan bounds positions to sixteen and adds a count-dependent group gate
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2230..0x004F230A
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x004F2335..0x004F233C
tool: Ghidra 12.1.3 PUBLIC bounded optional second-mode scan reading
environment: null
---

## Observation

FND-EXE-104 and FND-EXE-122 record the prefix of `0x004F2110`,
its admission gates, retained zero-extended word scan count K and initial
position zero. Byte `0x01D292A0` nonzero takes the second mode at
`0x004F2256`, before the first mode's K-zero exit. This mode can
therefore examine candidates when K is zero.

At each position J it compares retained K with J, unsigned, then loads
full N from `0x0070DCC0` plus four times J. The load preserves the
comparison flags. K greater than J bypasses the preliminary group-byte
gate. Otherwise it derives G from bit three of N, making G zero or one,
and tests byte `0x01D2927C` plus twenty times G. Zero skips this
candidate; nonzero continues. The candidate load precedes that group
gate, and the gate is required when J equals K as well as when J exceeds K.

Candidate continuation compares byte `0x01D291F0` plus eight times
N with zero, sets the retained slot base without changing those flags,
and skips if the tested byte was nonzero. It then skips if byte
`0x01D291F1` plus eight times N is zero. Only after those slot reads
it compares N with seven, unsigned. N at most seven passes; a larger N
passes only when byte `0x01D29200` is zero. No local comparison
bounds N to fifteen. These are slot-byte gates rather than local tests of
bit N in the initially admitted nonzero full mask.

Every skip increments J, compares it with fifteen, unsigned, and exits
when greater. Otherwise it returns to the K comparison. With the recorded
initial zero and ordinary local skip edges, positions zero through fifteen
are examined at most once each. K does not terminate this mode's loop:
zero subjects every position to the preliminary group gate, while K above
fifteen bypasses that gate for every locally admitted position. This bound
is on table position, not on loaded N or the extent of slot storage.

For an admitted candidate it first clears byte `0x01D291F1` plus
eight times N, then forms full `0xFFFFFFFE` rotated left by N's low
byte, with the thirty-two-bit rotation count reduced to its low five bits.
It reads full `0x0075B200` and full slot field `0x01D291F4`
plus eight times N before ANDing full `0x01D271C8` and then full
`0x01D271C4` with that clearing mask. It calls `0x00411A30`
at `0x004F22D9`, supplying the retained slot field, full zero and
retained `0x0075B200` value in its three outgoing slots. All clearing
stores precede the call. Unlike the first mode in FND-EXE-122, this mode
reads the full slot field before the two mask stores.

After an ordinary return it drops the callee's EAX rather than testing it,
derives G from retained N's bit three again, and reads current byte
`0x01D2927D` plus twenty times G. Zero publishes full N at
`0x01D271C0`, then byte one at `0x01D291F2` plus eight times
N, and returns full EAX one. A nonzero first group byte instead reads
current byte `0x01D2927E` plus twenty times G. Second byte zero
returns full EAX one without either selection-publication store; both
nonzero take the shared pointer `0x0072BC60` call to `0x0058F890`
at `0x004F233C`. The reading stops at that call, without asserting
its continuation or return effects. No local post-call group gate restores
the pre-call clearing stores. Exhaustion and other early exits do not
share one normalized return-value assignment.

## Interpretation

The second mode has a fixed sixteen-position local scan and an additional
count-dependent group gate, rather than the first mode's count-terminated
scan. Its selected input retains the slot value read before mask publication;
post-call group decisions are fresh. Q-EXE-009 in FMT-EXE-006 remains open
for table/slot producers, selected and shared-transfer callee effects,
initialization, lifetime and caller/native admission. The fixed position
bound does not establish candidate safety or actual PATH behavior, and this
is not a complete reading of the consumer and all its dependencies.

## Alternatives

- K zero does not reject this mode; it enables the preliminary group gate
  at every position. The mode branch precedes the first mode's zero exit.
- K is a gate threshold here, not the local iteration limit. The skip
  back-edge uses fifteen, and equality with K requires the group-byte gate.
- Sixteen positions do not imply sixteen-valued N: candidate slot reads
  precede its comparison with seven and there is no fifteen bound on N.
- Both modes clear before calling, but their slot-field load orders differ.
  Treating those loads as interchangeable requires unresolved alias/lifetime
  and external-effect contracts.
- A one return need not publish current selection, and unread callees may
  change state beyond the local writes described here.

## How to reproduce

Verify FND-EXE-011's executable identity and FND-EXE-099's six physical
controls. FND-EXE-104 independently supplies the direct consumer target;
FND-EXE-122 supplies the prefix and first-mode contrast. Use the saved
Ghidra program with -noanalysis and ReportInstructionWindow.java at
`0x004F2110` limit 160. Follow the nonzero mode edge to
`0x004F2256`, restricting observations to the declared second-mode
and shared exit/call ranges and excluding the following function. Track the
K comparison flags across the candidate load, group offsets and bit-three
index, skip increment/bound, slot reads before candidate comparison, clearing
order and the slot-field load before mask stores. Check the last writer of
each outgoing slot, then both fresh post-call group decisions and their
selection stores or omission. Stop at the shared transfer call; a following
instruction gap does not prove its return contract. No original-program run
or emulated call is part of this observation.

---
id: FND-EXE-373
title: Sound utility quantity helper returns saved prior pair after terminal publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:1E73..1000:1EFE
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:03C5..1000:03E6
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:046E..1000:049D
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:04B0..1000:04CE
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-372's growth paths call near 1E73 with a double-word quantity.
It forms a BP frame with eight local bytes. It passes current DS:008D,
high word zero and shift count four to 03C5, then adds DS:008B with
carry and the incoming low/high quantity with carry. The resulting high
word is compared signed with 000F. Signed less proceeds; signed greater
returns FFFF:FFFF. Equality compares the low word unsigned with FFFF,
whose inclusive test proceeds for every word value. No separate rejection
of a negative signed high word occurs at this gate. Arithmetic wraps at
double-word width; carry beyond the final high word is not tested.

For the selected count four, 03C5 shifts the AX/DX double word left four,
combining the old low word's high nibble into DX. The count-below-sixteen
branch changes CL to sixteen minus the count and uses BX as temporary
storage. Its alternate branch subtracts sixteen from CL, transfers original
AX to DX, clears AX and shifts DX by the remaining count. The root's
selected count uses only the first branch. This helper converts a near
return frame into a far frame by inserting CS and returns far; no incoming
argument bytes are removed.

After the first gate, 1E73 reloads the current pair from DS:008B/008D
into AX/DX and the incoming quantity into BX/CX, and calls 046E.
That helper also inserts CS into the near return frame, using ES to hold
the incoming return offset. A nonnegative signed CX selects addition.
It adds the low quantity to the offset; a low-word carry adds 1000 to
the segment. It adds the low nibble of the quantity's high word times
4096, then adds the resulting offset divided by sixteen to the segment.
It retains only the offset's low nibble. Segment arithmetic wraps at word
width. Bits above the high word's low nibble do not contribute to this
encoded segment arithmetic.

A negative signed CX first negates the complete BX/CX pair with carry
and enters the subtraction suffix at 04B0. That suffix subtracts the
magnitude's low word from the offset; borrow subtracts 1000 from the
segment. It subtracts the magnitude high word's low nibble times 4096,
adds the remaining offset divided by sixteen to the segment and retains
the offset's low nibble. It returns far without argument cleanup. The
original near return offset remains in ES; neither selected helper restores
the caller's prior ES. No interrupt occurs in these arithmetic helpers.

The root saves the candidate pair into SS:BP-04/-02, compares it with
DS:0087/0089 using 06C2 and rejects carry set. It reloads the candidate
before comparison with DS:008F/0091 and rejects unsigned above. Both
encoded bounds include equality; FND-EXE-369 reads the comparison helper.
It then saves the current pair from DS:008B/008D into SS:BP-08/-06,
pushes the candidate segment and offset, and calls 1DBE. That helper
removes the four argument bytes, as FND-EXE-369 records.

Returned AX zero selects FFFF:FFFF. Any nonzero AX reloads the saved
prior pair from the frame and returns it, rather than the candidate or a
pair reread from current DS. The root restores SP from BP, restores BP
and returns near without incoming argument cleanup. It does not locally
save SI, DI or ES. FND-EXE-369's terminal reading identifies publication
on its nonzero route and bound changes on its zero route; caller tests do
not undo those changes here.

## Interpretation

This resolves the locally called quantity helper behind FND-EXE-372's
growth requests. Under admitted noninterfering frame/state conditions its
non-failure pair is the saved prior position, while the candidate is the
input to the terminal helper. A failure pair can follow terminal state
changes. The first arithmetic gate admits signed-negative high words;
the later encoded comparisons are separate gates, not proof of a physical
extent or an unsigned arithmetic limit.

Q-EXE-007 retains incoming quantity admission, actual DS, stored pair and
bound writers, frame aliases, native preservation/results in the terminal
callee, shared-state lifetime and re-entry, and usable returned extents.
No native allocation, writable-capacity or complete-reading claim follows.

## Alternatives

Treating the high-word comparison as unsigned overlooks its signed branches.
Treating the returned pair as the candidate overlooks the saved prior pair.
Treating failure as unchanged state ignores the terminal helper's bound update.
Treating encoded normalization as physical address validation assumes an
admitted segment and memory model not supplied by this local arithmetic.

## How to reproduce

At revision 7964b4a require FND-EXE-350's SOUND_DS.EXE identity:
length 204593, XXH3-128 236c2dc23c071eca421eb5b427caee57.
Decode shipped half-open offsets 0x00003273..0x000032FE at IP 1E73,
0x000017C5..0x000017E6 at IP 03C5,
0x0000186E..0x0000189D at IP 046E and
0x000018B0..0x000018CE at IP 04B0, modeled CS 1000 and
MZ header size 1400, using locked Capstone 5.0.7 in sixteen-bit mode.
Track the signed first gate, selected shift count, pair normalization,
near-to-far return frames and ES mutation, local candidate/prior slots,
terminal argument cleanup and full-word AX test. Keep the unselected
adjacent entry at 049D outside this claim. Original bytes remain outside
Git; no original process, DOSBox or emulated call is executed.

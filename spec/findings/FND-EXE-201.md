---
id: FND-EXE-201
title: Negative-mode direct clearing can invalidate duplicate-word overlap exclusions before link publication
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x006008F0..0x00600924
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600936..0x00600948
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600860..0x0060088E
  - build: BLD-GOG-EN-1.1
    file: DOSBOX/DOSBox.exe
    address: 0x00600AF2..0x00600B25
tool: Ghidra 12.1.3 PUBLIC
environment: null
---

## Observation

FND-EXE-199's duplicate-word exclusion applies to setup's nonnull,
no-call zero-mode route. A negative initial full mode instead calls
FND-EXE-200's helper at `0x00600936`. On normal return setup reloads
the shared pointer, reads its full offset-48 word, and branches directly
to the two link publications when that word is zero. It does not repeat
the initial signed-mode test or reread the duplicate callback words.

Consider the same valid stack and equal DS/SS model as FND-EXE-198,
with callback frame C, supplied record R = C -108, shared pointer P,
and duplicate word V at C +32 and C +36. Additionally require the
shared pointer to remain P, the shared gate at `0x0242C910` to be zero
when the helper reads it, valid accesses, and no external intervening
writer. Keep that gate and the shared-pointer cell separate from the
frame writes and both publication destinations. These are admission
conditions, not established shipped inputs.

On the helper's zero-gate branch there is no external call. It writes
four zero bytes at P +48 and restores its saved registers and frame.
For this callback, setup's conventional frame is C -208 and its saved
registers and local reservation leave ESP at C -232 before the helper
call. The call and helper's saved-frame push put the helper frame at
C -240; its two saved-register slots are below that frame. These direct
stack writes are below the duplicate incoming words. The pointer-based
mode clear, however, is not separated from them by this geometry.

Let b0 through b3 denote V's bytes from least to most significant.
FND-EXE-199 gives b0 as the equality result, zero or one. The mode read
for four of its overlap candidates has the following sign byte:

| Shared base | Mode-read start | Most significant mode byte |
| --- | --- | --- |
| C -15 | C +33 | b0 |
| C -14 | C +34 | b1 |
| C -13 | C +35 | b2 |
| C -12 | C +36 | b3 |

Consequently P = C -15 cannot take the initial negative-mode branch
from these unchanged duplicate words: b0 is below 128. For each of the
other three candidates, the branch is taken when its named sign byte
is at least 128. The zero-gate helper then clears that four-byte mode
window, including bytes of the duplicate incoming words. With P
unchanged, setup's fresh mode read is zero and reaches publication.
The subsequent four-byte store at P +40 overlaps the sixth slot at
C +28 for these three candidates. At P = C -12 it replaces that whole
slot with R; the other two candidates overlap only part of the slot.
The mode clear and link store are eight bytes apart and do not undo
one another. R's first-word store remains distinct under the admitted
frame model.

## Interpretation

A nonzero duplicate word excludes those candidates on the no-call
route, but does not exclude the negative-mode direct-clear route.
The helper is itself a last writer of some duplicate argument bytes
under these conditions. Q-EXE-009 retains actual P and gate producers,
V's admitted bytes, segment identity, valid storage, other helper
branches, aliases of the shared cells and external or exceptional
writers. No shipped overlap, native corruption, pointer preservation
or complete reading is established.

## Alternatives

Carrying FND-EXE-199's unchanged-duplicate premise through the negative
helper would miss its four-byte mode store. Conversely, treating every
candidate as negative would ignore the sign byte: C -15 has b0 zero
or one. A direct clear proves neither that the shared gate is zero in
the shipped path nor that the supplied P comes from a caller stack.

## How to reproduce

Use shipped interpreter XXH3-128 `09861838aa3018346f9f15c9a4f5925c`.
In the saved project, read-only with analysis disabled, run
ReportInstructionWindow at `0x006008F0`, count 27; at `0x00600936`,
count five; at `0x00600860`, count 19; and at `0x00600AF2`, count 27.
Restrict claims to the cited spans. Check ReportCitationBoundaries for
`00600860..0060088E:return`, `006008F0..00600924:return` and
`00600936..00600948`. Endpoints do not prove interior completeness.

Use FND-EXE-198's stack trace, then account for the helper call's
return-address push, saved frame and two saved registers. For each
candidate add 48, derive all four mode bytes from the duplicate V
words, and identify the most significant byte before applying the
signed test. Follow the zero-gate clear, normal restoration, shared
pointer reload and fresh zero test in execution order. Keep rich
reports local and execute no original program.

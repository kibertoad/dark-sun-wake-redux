---
id: FND-EXE-275
title: Allocation fallback makes a separate alignment request and retains the first segment
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1528..1000:1582
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near helper saves incoming AX on the stack. It forms a two-word value
equal to that unsigned word multiplied by sixteen: the low word is AX
shifted left four, and the high word is the former high byte shifted right
four in a cleared BX. It loads DS from CS-relative word 0x1361, pushes the
high then low request words and calls 1000:1831. It removes four argument
bytes, pops the saved incoming count into BX and tests returned AX as a
word against 0xFFFF. Equality reaches the zero-result suffix.

Otherwise it masks AX to its low nibble. Zero reads CS-relative word
0x135D into CX, publishes returned DX to that shared word, loads DS from
DX, writes the saved count to DS-relative word zero and the former shared
word to DS-relative word two. It sets AX to four and near-returns.

A nonzero nibble saves the count and first returned DX on the stack. It
forms the word value sixteen minus that nibble, pushes a zero high word
and that low word, and calls 1000:1831 again. After removing those four
argument bytes it restores the first DX and the count from the stack.
The second returned AX is tested as a word against 0xFFFF. Equality
reaches the zero-result suffix; otherwise it increments the restored DX
at word width and enters the same shared-word and header publications.
The second returned DX is discarded by the stack restoration.

The zero-result suffix zeros AX and sign-extends the zero word to DX,
then near-returns. There is no local release call or shared-word/header
publication on either rejection path. Effects performed inside either
1000:1831 call are unread, not shown to be absent or undone. Both calls
use the same locally restored DS unless the first callee changes it;
this helper does not reload DS before the second call.

## Interpretation

The first request represents the incoming paragraph count in bytes. The
second request is only the alignment deficit, between one and fifteen,
and is a distinct operation whose success test precedes segment increment.
This body does not join two returned extents or verify that the second
allocation is adjacent to the first. Its publication uses the first segment
plus one on the unaligned success path, not the second returned segment.

FND-EXE-272 reaches this helper from an absent list head or a completed
list traversal. The callee's input contract, returned offset/segment
meaning, effects on DS, failure state and lifetime remain obligations under
Q-EXE-001 and Q-EXE-010. Shared-word writers, header admission, physical
aliases and segment-increment wrap remain unresolved. No allocation extent,
rollback guarantee or complete reading follows from these local paths.

## Alternatives

Treating the alignment request as a retry of the full original size ignores
its one-to-fifteen low word and zero high word. Using the second DX as the
published segment contradicts its replacement by the saved first DX.
Treating either failure as undoing an earlier call ignores the absence of
a local release and the unread callee effects. Treating success as a
verified contiguous extent assumes a callee contract this body does not show.

## How to reproduce

At revision a5c82bb, statically read installed DSUN.EXE and require the
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176.
With the locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in
sixteen-bit x86 mode, decode shipped half-open range
0x00006728..0x00006782 at initial IP 0x1528. The MZ header is 0x5200
and the model load segment is 0x1000. Follow each failure test, the zero
and nonzero low-nibble paths, every push/pop and each header store.
For the sign-extension instruction retain the sixteen-bit effective width
controlled in FND-EXE-258 rather than its printed cdq mnemonic. Read
FND-EXE-272's two incoming paths separately. No original execution,
caller-completeness search or callee-preservation assumption is made.
Source bytes and analysis output stay outside Git.

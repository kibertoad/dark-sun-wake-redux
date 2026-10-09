---
id: FND-EXE-291
title: Second setup callee gates an interrupt wrapper and hides its returned AX
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0006..44D0:0040
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0122..45B9:013F
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-290's second setup call reaches 44D0:0006. Its first far call
has a relocated segment operand at shipped 0x00039F09 resolving to
45B9:0006, whose body FND-EXE-287 reads. A zero returned AX writes
0804 to word DS:33BE and returns AX=FFFF. No cleanup call or clearing
of FND-EXE-290's preceding publications occurs on this path. The procedure
does not establish or save DS locally before either call.

For nonzero AX it pushes AX and BP, forms a temporary BP frame and
overwrites the pushed AX word with 007F, then restores BP. It pushes
CS, pushes AX again and repeats the temporary-frame sequence to replace
that second pushed AX word with 0053. The resulting outgoing words are
offset 0053, segment CS, and mask 007F. No earlier AX result supplies
either overwritten argument word. The next call's relocated segment operand
at shipped 0x00039F32 resolves to 45B9:0122.

That wrapper forms an SS-relative BP frame and saves ES, AX, BX, CX
and DX. It sets AX=000C, reads the mask word from SS:BP+0A into CX
and masks it with 007F, and loads the far pointer from SS:BP+06 into
ES:DX. It executes INT 33, restores DX, CX, BX, AX, ES and BP and
far-returns without argument cleanup. In particular its outgoing AX is
the saved incoming AX, not the interrupt's returned AX. It does not save
DS, SI or DI, and this local reading establishes no interrupt preservation
contract for those registers or for the stack.

The outer procedure removes six argument bytes, writes one to byte
DS:33D8, replaces AX with zero and far-returns. It performs no returned
status test for this second call. The local byte publication follows the
interrupt continuation; it is not evidence that a real driver accepted the
pointer or mask.

## Interpretation

This resolves the local call order and outgoing argument writers left open
in FND-EXE-290. The early zero-result path can return failure after that
caller has already published its buffer and active word. The nonzero path
always returns local success if the interrupt wrapper returns normally;
it does not propagate an interrupt result. Q-EXE-010 retains interrupt
effects, post-interrupt DS admission, state writers, repeat-entry lifetime
and the incoming caller's path and arguments. No complete-reading promotion
or claim about an installed hook follows.

## Alternatives

Treating the wrapper's returned AX as an interrupt status ignores the saved
AX restoration. Treating DS:33D8 as proof of successful external registration
ignores the unchecked call continuation. Treating failure as an unmodified
initial state ignores the preceding publications in FND-EXE-290. Treating
the argument pushes as unmodified AX values ignores their final word writers.

## How to reproduce

At revision 70474aa require FND-EXE-176's installed source identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With locked Capstone
5.0.7 in sixteen-bit x86 mode decode shipped half-open ranges
0x00039F06..0x00039F40 at IP 0006, modeled CS 44D0, and
0x0003AEB2..0x0003AECF at IP 0122, modeled CS 45B9. With the
committed operand reporter and loadSegment 1000 resolve sites
0x00039F09/targetOffset 0006 and 0x00039F32/0122. Track both
temporary BP frames through each replacement and the caller's six-byte
cleanup. Reports and source bytes remain outside Git. No original execution
or interrupt emulation is involved.

---
id: FND-EXE-404
title: Registration input helper ignores the argument high byte and admits only zero or uppercase selectors
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:13D2..1425:13FE
tool: Capstone 5.0.7, Python struct and xxhash 4.0.1
environment: null
---

## Observation

FND-EXE-403's first callee saves BP and reads only the byte at
SS:BP+6 into AL. The following instruction sign-extends AL into AX
at sixteen-bit width, replacing AH; the pushed argument's upper byte is
not consumed. The unprefixed sign-extension instruction at 13D8 is one byte
long. Its sixteen-bit operation is CBW despite the decoder's CWDE label,
the same width distinction noted in FND-EXE-398.

It copies AX into DX and tests the whole word. Zero is pushed
unchanged for a far call to 15F3:01C1. Nonzero must pass unsigned
comparisons at least 0041 and at most 005A. Values outside that
interval return AX zero without the call. Values inside it subtract 0040
and push the resulting selector, one through 26, for the same call.
Lowercase and sign-extended high-bit bytes are rejected, not normalized.

The helper removes the outgoing word by popping CX, restores BP and
returns far without incoming cleanup. It forwards the native helper's AX
without testing it. It does not save DS or SI locally or assign either
register; this is not their preservation contract across the native path.

FND-EXE-398 reads 15F3:01C1's local return mapping and arithmetic.
Under admitted continuation and frame accesses, its returned word is at most
1000 hexadecimal. Therefore FND-EXE-403's subsequent unsigned quotient and
subtraction computes r-floor(r/10), at most 0E67 hexadecimal, for that
locally bounded return. This does not establish native units or a usable
quantity. Rejected byte inputs instead feed zero to the same caller arithmetic.

The complete local body covers 44 bytes in 22 instructions, ending
at 13FD. Input cases 00, 40, 41, 5A, 5B, 61,
80 and FF respectively select native argument zero, local rejection,
native arguments one and 26, then four local rejections. No native request
or successful result is inferred for a rejected case.

## Interpretation

This supplies one previously open argument-width and local result dependency
of FND-EXE-403. Entry DS/SI provenance, actual callers, memory aliases,
native request effects and preservation, and the two subsequent pointer-call
contracts remain Q-EXE-007. No complete-reading declaration is made.

## Alternatives

Interpreting the word push as a consumed word input would retain an upper
byte that CBW replaces. Accepting lowercase would invent normalization absent
from the comparisons. Treating invalid bytes as native selector zero would
erase the local return path. A local bound on returned arithmetic does
not admit native meaning or register preservation.

## How to reproduce

At revision 9df7334d require installed DSUN.EXE length 634416 and
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode file base
0x9450 plus 0x13D2 through exclusive 0x13FE in sixteen-bit mode.
Require 22 instructions and full 44-byte coverage. Inspect the one-byte
unprefixed instruction at 13D8 independently of its displayed mnemonic.
Track incoming byte consumption, signed extension, unsigned comparisons,
both argument-producing paths, rejected returns and outgoing-word cleanup.
Use FND-EXE-398's resident native helper and FND-EXE-403's caller;
check the listed byte cases statically. Licensed bytes remain outside Git.
No original game, DOSBox or emulated call runs.

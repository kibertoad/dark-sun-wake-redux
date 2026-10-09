---
id: FND-EXE-277
title: Fallback arithmetic helpers normalize segment pairs with word-width wrapping
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0562..1000:0583
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:060B..1000:063A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:064D..1000:066B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:07F0..1000:0811
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

At 1000:0562 the helper converts a near return to a far return by popping
the return offset into BX and pushing current CS and that offset. For CL
below sixteen it saves AX in BX, shifts AX and DX left by CL, changes CL
to sixteen minus its former value, shifts BX right by that count and ORs
BX into DX. For CL at least sixteen it subtracts sixteen from CL, exchanges
DX and AX, clears AX and shifts DX left by CL. Both paths far-return.
FND-EXE-276 supplies CL four and DX zero, so that specific call produces
the incoming AX multiplied by sixteen in DX:AX. Counts outside that
caller case have no admitted CPU shift-count model here.

At 1000:060B the helper uses ES to pop the near return offset, then pushes
CS and that offset for its far return. It tests CX's sign. A nonnegative
CX adds BX to AX, adding 0x1000 to DX on low-word carry. It adds the low
byte of CX shifted left four to DH, then adds AX shifted right four to
DX. It retains only the original low nibble of the post-addition AX.

A negative CX first negates CX:BX as a two-word value, then reaches
1000:064D. That suffix subtracts BX from AX and subtracts 0x1000 from
DX on low-word borrow. It subtracts from DX the word formed by the low
byte of the negated CX shifted left four with a zero low byte. It then
adds the remaining AX shifted right four to DX and retains only AX's
low nibble. Both adjustment paths use word-width arithmetic with no
overflow rejection. Both far-return with ES holding the near return
offset, not its incoming value. Neither path writes DS or memory apart
from stack operations.

At 1000:07F0 the helper saves CX. It adds unsigned AX divided by sixteen
to DX and unsigned BX divided by sixteen to the restored CX, at word
width, and retains each original offset's low nibble in AX and BX.
It compares DX to CX; if unequal it returns those comparison flags.
If equal it compares AX to BX and returns those flags instead. It
near-returns, without changing DS or ES. AX, BX, CX and DX are not
preserved as their incoming values.

## Interpretation

FND-EXE-276's candidate calculation uses an offset/segment adjustment,
not an unbounded linear-address sum. Its first comparison rejects a
normalized candidate below the first bound; its second rejects a
normalized candidate above the second bound. Equality is admitted by
both local flag consumers. These are unsigned comparisons of wrapped
segment words followed, on equality, by low-nibble offsets.

The helpers preserve DS on their ordinary paths, resolving that local
preservation obligation before the final state-update call. Their
normalization does not validate physical segment identity, aliases or
initialized extent. Q-EXE-001 and Q-EXE-010 retain state-word writers,
admitted argument ranges, final updater effects, wrap admission and every
caller. No complete reading or safe allocation bound is established.

## Alternatives

Treating normalized comparison as an unlimited physical-address comparison
ignores word-width segment additions. Treating the request adjustment as
unsigned ignores its signed CX gate and negative suffix. Assuming ES is
preserved ignores the popped return offset. Treating equality with either
bound as failure contradicts the caller's strict below/above branches.

## How to reproduce

At revision 5dd3f18, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open ranges 0x00005762..0x00005783,
0x0000580B..0x0000583A, 0x0000584D..0x0000586B and
0x000059F0..0x00005A11 at initial IPs 0x0562, 0x060B, 0x064D
and 0x07F0 respectively. The MZ header is 0x5200 and model load segment
0x1000. Follow every branch and register-width operation, keeping the
shared negative suffix separate from its adjacent entry. Cross-check
FND-EXE-276's inputs and flag consumers. No original execution or complete
caller search is claimed; source bytes and reports stay outside Git.

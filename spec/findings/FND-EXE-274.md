---
id: FND-EXE-274
title: Larger-count allocation helper splits segment storage and publishes neighbor headers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1582..1000:15A5
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near helper copies incoming DX to BX and subtracts incoming AX from
incoming DS-relative word zero. It adds the resulting word to DX, at word
width, then loads DS from that adjusted DX. Through this new DS it writes
incoming AX to word zero and the saved incoming DX to word two.

It then copies adjusted DX to BX, adds current DS-relative word zero to
BX at word width, and loads DS from BX. Through this third selected segment
it writes adjusted DX to word two. It sets AX to four and near-returns.
No carry rejection, calls, interrupts or explicit ES writes occur in this
bounded body. DS is not restored locally, and DX retains the first adjusted
segment value. The word-zero reload is an actual memory read after stores,
not an independently protected copy of incoming AX.

## Interpretation

FND-EXE-272 selects this helper when the current DS-relative available count
is unsigned-greater than the converted request. That comparison alone does
not establish that incoming DX equals the current DS segment, that segment
addition avoids wrap, or that every header is initialized and writable.
On an ordinary return the caller receives AX four and the adjusted DX,
while DS identifies the later neighbor. Its shared-slot restoration must
still be read with every writer and physical alias accounted for.

The sequence exposes three distinct header accesses: subtraction through
incoming DS, count and predecessor publication through adjusted DX, and
the later word-two publication through adjusted DX plus a reloaded count.
Physical aliases can make these header publications overlap other state. Q-EXE-001 and Q-EXE-010 retain incoming segment identity, header
writers, arithmetic bounds, aliases and lifetime. No complete reading or
initialized allocation extent is established.

## Alternatives

Treating this helper as preserving DS contradicts its last segment load.
Treating the returned offset four as a byte allocation length ignores its
literal publication. Treating the final segment sum as checked arithmetic
ignores the absence of a carry branch. The word-zero reload remains an explicit access; asynchronous changes are
outside this ordinary-path reading.

## How to reproduce

At revision 3f15f06, statically read installed DSUN.EXE and require the
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176.
Using the locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in
sixteen-bit x86 mode, decode shipped half-open range
0x00006782..0x000067A5 at initial IP 0x1582. The MZ header is 0x5200
and the model load segment is 0x1000. Qualify each memory access by the
current DS, follow both word-width additions and the subtraction, and
check the near return at the last byte. Read FND-EXE-272's larger-count
call and restoration separately. No original execution, caller-completeness
search or writer-absence claim is made; source bytes stay outside Git.

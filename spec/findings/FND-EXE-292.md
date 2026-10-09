---
id: FND-EXE-292
title: Setup caller passes supplied storage and continues through discarded status returns
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:00F1..39D1:0185
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44B6:0161..44B6:0183
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:009E..44D0:00C0
tool: executable-reader 2.5.0, Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

FND-CONFIG-207 names the resident entry 39D1:00F1 and its nonzero
resource-return continuation through 39D1:0143. Decoding from that
entry reaches FND-EXE-290's incoming call-byte candidate at 39D1:0173
on this continuation. This supplies an instruction path, not complete native
entry or caller admission.

At 39D1:0146 the caller pushes a four-byte immediate 00000400 and
calls 444C:00FA; it removes four argument bytes. It stores returned DX
to current DS:9E76 and AX to DS:9E74, then compares the whole stored
double word with zero. Zero writes 001B to DS:9DB0 and branches to
39D1:0123, whose local path calls 39D1:0609, sets AX=FFFF and jumps
to 39D1:0314. The helper and final suffix are not read here.

Nonzero pushes a two-byte immediate 0400, then the four-byte value
read from current DS:9E74. These distinct operand widths supply
FND-EXE-290's far pointer at SS:BP+06 and size word at SS:BP+0A.
The segment operand at shipped 0x0002F086 resolves to 4464:014E.
The caller removes six argument bytes, calls 44B6:0161, then calls
44D0:009E, with no intervening test or retention of any returned AX.
The respective segment operands at 0x0002F08E and 0x0002F093
resolve to those two targets. The request callee's operand at
0x0002F05F resolves to 444C:00FA.

44B6:0161 saves incoming DS and loads DS=57E0 from its relocated
immediate at shipped 0x00039EC3. If byte DS:33D4 equals one, it
writes one to byte DS:33D5 and returns AX=0000. Otherwise it writes
0802 to word DS:33BE and returns AX=FFFF. Both paths restore the
saved DS before the far return. Neither path calls another procedure.

44D0:009E likewise saves incoming DS and loads DS=57E0, using the
relocated immediate at 0x00039FA0. If byte DS:33D8 equals one,
it writes one to byte DS:33D9 and returns AX=0000. Otherwise it
writes 0802 to word DS:33BE and returns AX=FFFF. Both paths
restore DS and far-return without another call. An earlier failure can
therefore be followed by these separate gate tests and publications;
their results do not control this caller's next step.

## Interpretation

This grounds the local incoming path and its final argument writers.
Under unchanged DS and stored-pointer state, the caller supplies a nonzero
pointer and size 1,024, choosing FND-EXE-290's supplied-storage arm.
That arm's unchanged allocation flag and its failure publications remain
relevant; this caller neither checks setup's error nor checks either later
gate setter's result. It does not thereby prove usable buffer storage or
successful native setup.

Q-EXE-010 retains 444C:00FA's storage and preservation contract, the
entry's upstream callers, current DS identity, state writers and lifetime,
aliases and interrupt effects. FND-CONFIG-207's earlier resource call is
read-only dependency evidence, not a complete reading of this outer entry.
No complete-reading promotion, code-range change or caller-absence claim
follows.

## Alternatives

Treating both 0400 pushes as the same-width argument ignores their
four-byte versus two-byte operands and distinct cleanup. Treating the
stored nonzero pair as validated memory ignores its unread producer.
Treating gate setters as conditional on successful setup ignores the absent
AX test. Treating all failures as one returned error ignores subsequent
callee overwrites and independently executed state publications.

## How to reproduce

At revision cb5c0a0 require FND-EXE-176's installed DSUN.EXE identity,
XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. Decode shipped
half-open 0x0002F001..0x0002F095 with Capstone 5.0.7 in sixteen-bit
x86 mode at IP 00F1, modeled CS 39D1; enable operand details to
check push widths. Decode 0x00039EC1..0x00039EE3 at IP 0161,
CS 44B6, and 0x00039F9E..0x00039FC0 at IP 009E, CS 44D0.
With the committed operand reporter and loadSegment 1000 resolve
site/targetOffset pairs 0x0002F05F/00FA, 0x0002F086/014E,
0x0002F08E/0161, 0x0002F093/009E, 0x00039EC3/0000 and
0x00039FA0/0000. Keep source bytes and reports outside Git. No
original execution or complete incoming search is claimed.

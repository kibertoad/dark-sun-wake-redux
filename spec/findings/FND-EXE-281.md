---
id: FND-EXE-281
title: List-link writer uses temporary SS to insert a segment beside the current head
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1464..1000:149B
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The selected body reads CS-relative word 0x135F into AX and tests it
for zero. Zero publishes incoming DS to that shared word, then writes
DS to its own DS-relative words four and six, in that order, and
near-returns. It does not load a new DS or ES on this path.

Nonzero saves incoming SS into BX, pushes flags and disables maskable
interrupts. It loads SS from AX and reads SS-relative word six into ES.
It writes incoming DS to SS-relative word six, then writes the temporary
SS segment word to DS-relative word four. It restores SS from BX and
pops flags. After that restoration it writes incoming DS to ES-relative
word four and ES to DS-relative word six, then near-returns.

The stack push occurs before changing SS and the stack pop after
restoring it. No stack operation occurs locally while SS holds the head
segment. The incoming DS remains the segment used for the new entry's
words four and six throughout the nonzero path. ES is the head's
word-six value loaded before its replacement. The shared head word is
not locally rewritten on this path.

There are no calls or software interrupts in this bounded body. Its
explicit flag protection ends before the last two link writes, so it
does not establish atomic publication of all four links. Non-maskable
events, faults and physical aliases remain outside the ordinary-path
reading. No local segment or header-extent validation occurs.

## Interpretation

FND-EXE-272 traverses word-six links and FND-EXE-273 reconnects word-four
and word-six neighbors. This candidate supplies a matching local writer:
self-link initialization when the shared head is zero, or insertion of
incoming DS between the head and the head's former word-six neighbor.
That description is conditional on admitted distinct headers; aliasing
could make the stated stores overlap other fields or stack storage.

Q-EXE-001 and Q-EXE-010 retain incoming callers and DS producers,
all other link/head writers, lifetime, physical aliases and interrupt
admission. This is not a complete writer search or proof that every
allocator-reachable list is circular, initialized or valid. No complete
reading or original execution is claimed.

## Alternatives

Reading the head accesses through DS misses the explicit SS override
and temporary SS load. Treating all link writes as interrupt-protected
ignores the flag restoration before the final pair. Treating a nonzero
head as proof of a valid header ignores the absence of validation.
Treating the matching layout as a proven caller relationship would require
the incoming transfers and state producers that this reading does not cover.

## How to reproduce

At revision 55c687e, require installed DSUN.EXE's XXH3-128
e296af55ba2ecde7e77f555c90f33d0b recorded in FND-EXE-176. With the
locked Python interpreter, xxhash 4.0.1 and Capstone 5.0.7 in sixteen-bit
x86 mode, decode shipped half-open range 0x00006664..0x0000669B
at initial IP 0x1464. The MZ header is 0x5200 and model load segment
0x1000. Follow both head-value branches, qualifying every memory access
by CS, DS, SS or ES as encoded. Track SS and the flags push/pop separately
from link publication. Compare FND-EXE-272/273's link consumers without
assuming a caller relation. No caller-completeness or writer-absence
claim is made; source bytes and analysis output stay outside Git.

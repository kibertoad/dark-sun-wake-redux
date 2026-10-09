---
id: FND-EXE-273
title: Exact-size allocation helper unlinks a segment and conditionally preserves DS
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:143B..1000:1464
tool: Capstone 5.0.7 and xxhash 4.0.1
environment: null
---

## Observation

The near helper saves incoming DS in BX and compares that segment word with
incoming DS-relative word six. Equality stores zero into CS-relative word
0x135F and near-returns, without changing DS or ES or writing the selected
segment's link fields.

Inequality loads ES from incoming DS-relative word six, then loads DS from
incoming DS-relative word four. It writes ES into the new DS-relative word
six, then writes the new DS segment word into ES-relative word four. It
publishes that new DS into CS-relative word 0x135F, restores DS from BX and
near-returns. The second segment load reads through the original DS: the
first load changes ES only. The two neighbor-link stores precede the shared
publication and restoration.

There are no calls, interrupts, stack writes or explicit AX/DX writes in this
bounded body. Both ordinary returns preserve AX and DX. BX holds incoming DS;
ES is unchanged on equality and holds the word-six segment on inequality.
The inequality restoration uses a register, unlike the calling allocator's
shared saved-DS slot. Memory access faults and asynchronously changed state
are outside this ordinary-path reading.

## Interpretation

On FND-EXE-272's exact-size continuation, DS is restored to the selected
segment on both ordinary helper paths. The caller consequently reads that
segment's word eight into BX, copies it to its word two and sets AX to four.
This helper supplies no DX result: any segment half of the returned DX:AX
pair must come from the incoming path and its earlier writers. Neither
unlinking nor AX four proves an initialized allocation extent.

Equality clears the shared list-head word without rewriting the selected
segment's links. Inequality reconnects the word-four and word-six neighbors
in the stated order and publishes the former neighbor. Physical segment
aliases could make these stores overlap the selected header or code-segment
state; no distinct-storage assumption is admitted.

Q-EXE-001 and Q-EXE-010 retain live link/header writers, segment validity,
physical aliases, list lifetime, incoming DX and the other allocation helper
contracts. This closes one local callee-body obligation, not a complete
reading of the allocator or overlay loader.

## Alternatives

Treating this helper as returning a fresh segment in DX is contradicted by
its absence of DX writes. Treating both paths as rewriting neighbor links
ignores the equality bypass. Assuming unlinking clears the selected header
or initializes its allocation ignores the absence of those writes. Ordinary
DS restoration does not establish absence of overlapping memory effects.

## How to reproduce

At revision 80a21ed, read the installed DSUN.EXE statically and require
FND-EXE-176's XXH3-128 e296af55ba2ecde7e77f555c90f33d0b. With the locked
Python interpreter, Capstone 5.0.7 in sixteen-bit x86 mode and xxhash 4.0.1,
decode shipped half-open range 0x0000663B..0x00006664 at initial IP 0x143B.
The MZ header is 0x5200 and the model load segment is 0x1000. Follow both
comparison outcomes, qualifying every access by its current segment and
checking the two near returns. Check FND-EXE-272's exact-size call and its
post-call word-eight, word-two and AX publications separately. No execution
of the original, caller-completeness search or writer-absence claim is made.
Source bytes and analysis output remain outside Git in GAME_DIR.

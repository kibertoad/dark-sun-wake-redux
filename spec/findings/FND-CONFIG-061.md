---
id: FND-CONFIG-061
title: Startup registers overlay 180's archive cleanup as a runtime exit callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0020..56B2:0025
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:030B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0388
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV and resident windows; FBOV trampoline inspection
environment: null
---

## Observation

Overlay 180's resident trampoline `56B2:0020` targets its routine at
physical file offset `0x00067321`. The resident main routine calls that
trampoline at `0x0001CC2F`, before the resource archive open at
`0x0006761D` in the target routine (FND-CONFIG-039). At the start of
`0x00067321`, the code pushes the encoded overlay segment `0x05A0` and
offset `0x0025`, then calls resident `1000:030B` at `0x0006732F`. The
`56B2:0025` trampoline targets overlay 180's close-all archive routine
(FND-CONFIG-059).

The resident routine at `1000:030B` checks the callback count at
`DS:3572` against 32. If there is room, it places the supplied far
pointer into a four-byte table slot indexed by that count, increments
the count and returns zero; at the limit it returns one without storing
the pointer. The overlay 180 caller removes the arguments without
checking this return value.

The executable's startup entry calls the resident main routine and,
after it returns, calls `1000:03DF` with its result (FND-SAVE-012).
That wrapper reaches `1000:0388`, whose zero-mode branch decrements
the callback count and far-calls each stored pointer until the count
is zero, then proceeds through further cleanup and a DOS termination
request. A registered `56B2:0025` pointer therefore enters the
close-all routine through this callback loop, without a literal far
call to that entry (FND-CONFIG-060).

## Interpretation

The ordinary main-loop return path has a static route to archive
close-all cleanup through the exit-callback table. This explains the
separate pushed words that the contiguous-pointer search of
FND-CONFIG-060 could not identify. Registration is conditional on a
free table slot, and the code examined here does not prove the
callback's execution in every live run.

## Alternatives

Other indirect routes to the cleanup trampoline remain possible. The
table's complete writer inventory, callback count in every startup
state, effects of earlier cleanup helpers and operating-system outcome
were not established by these windows. This finding does not show
archive closure during ordinary message calls.

## How to reproduce

Resolve overlay 180's `56B2:0020` and `56B2:0025` trampolines from its
resident header at physical file offset `0x0004BD20`. Disassemble
`0x0001CC1F..0x0001CC3A`, `0x00067321..0x00067345`,
`0x0000550B..0x00005538`, `0x00005588..0x000055DF`, and
`0x000055DF..0x000055EE`. The segment-zero resident calls use the
documented `0x1000` load segment. Compare the main-loop return route
with FND-SAVE-012 and the callback target with FND-CONFIG-059.

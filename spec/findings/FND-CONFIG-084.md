---
id: FND-CONFIG-084
title: A shared-message path restores its prior global callback after temporary registration
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0025..566A:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0034..571F:0039
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0430
tool: Python 3.14.7 FBOV fixup and trampoline inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

The declared FBOV fixups contain 22 direct far calls to overlay 190's
global-callback setter `571F:0034` from 14 overlays. Overlay 171's list
registration at `0x000583C0` is one of them (FND-CONFIG-083). Three calls
are in overlay 172.

On an overlay 172 path beginning at file offset `0x000597D6`, a call to
`571F:0039` returns the pointer kept at `DS:61A2`. The caller saves that
pointer in two stack words, registers its own callback through
`571F:0034` at `0x000597E7`, and later passes the saved stack pointer
back to the same setter at `0x000598A9`. A separate cleanup path at
`0x00059BA5` passes the far pointer stored at `0300:000F` to the setter;
the writer of that stored pointer was not followed here.

Overlay 171's local list cleanup routine at `0x00059014` has no direct
call to `571F:0034`. It passes zero to resident `39D1:0430`, clearing
the distinct global pointer `DS:A0F5`. The cleanup routine then releases
its stored window pointer and buffer and calls overlay 190's `571F:007A`.
The latter's bounded
routine at `0x0007B4DE..0x0007B4F7` does not call the global setter.

## Interpretation

The overlay 172 path can temporarily replace and then restore the list's
global callback when that callback was the prior value. The list cleanup
routine does not directly clear `DS:A0F1`; this observation alone does not
establish how long the list registration remains live, because other
callers of the setter and indirect cleanup effects remain.

## Alternatives

FND-CONFIG-085 identifies six overlay 182 setter calls that install the
resident key callback or zero; their order relative to the list remains
unread. Other direct setter calls may also replace the list callback before
an event, and the cleanup routine's other callees may change it indirectly.
This finding does not establish every exit from the shared-message path or
that its saved pointer is always the list callback.

## How to reproduce

Select declared overlay fixups with target descriptor 190 and offset
`0x0034`; count their direct `0x9A` far calls by source descriptor.
Disassemble overlay 172 at `0x000597D6..0x000597EF`,
`0x000598A5..0x000598B5` and `0x00059B9A..0x00059BB8`, resolving the
`571F:0039` getter to `0x00079501`. Disassemble overlay 171's cleanup at
`0x00059014..0x00059067`, resident `39D1:0430` at `0x0002F340`, and
overlay 190's `571F:007A` at `0x0007B4DE`.

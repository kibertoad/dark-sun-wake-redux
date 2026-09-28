---
id: FND-CONFIG-059
title: Overlay 180's close-all archive call lies in a separate exported cleanup routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0025
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:02B5
tool: Python 3.14.7 bounded FBOV header inspection; Capstone 5.0.7 16-bit disassembly of a bounded overlay window
environment: null
---

## Observation

Overlay 180's resident header has a trampoline at `56B2:0025` whose
target offset is `0x0762` in the overlay code beginning at physical file
offset `0x000671E0`. The target is the routine at `0x00067942`, separate
from the startup resource-archive open at `0x000675FD`
(FND-CONFIG-039).

The `0x00067942` routine conditionally calls several helpers and checks
local data at `DS:13F7`, `DS:13FE`, `DS:13FC` and in segments `0338` and
`0370`. Its path at `0x00067A06` passes `0xFFFFFFFF` to resident
`38FF:02B5` at `0x00067A09`, the close-all archive entry
(FND-CONFIG-041). If that entry returns, the routine calls `01F8:0623`
with `0x0338` and 2, clears `DS:1462`, and returns. There is no local
branch around the close-all call after entry into this routine, though
an earlier helper might not return.

## Interpretation

Entry into this exported routine can remove the startup resource archive
from the open list. The call's placement in a separate routine does not
establish when that routine executes relative to any message call.

## Alternatives

FND-CONFIG-061 identifies its registration in the startup exit-callback
table and the runtime exit route that invokes that table. Other possible
callers and the effects of preceding helpers remain unread. This finding does not
establish that `WIND/10501` becomes unavailable during ordinary play or
that the message-delay wait is skipped in a live state.

## How to reproduce

Read overlay 180's resident header at physical file offset
`0x0004BD20`; its second five-byte trampoline starts at header offset
`0x0025` and carries target code offset `0x0762`. Add that offset to the
overlay code start `0x000671E0`, then disassemble
`0x00067942..0x00067A27`. Resolve the `0x0128` FBOV fixup at
`0x00067A09` to resident `38FF:02B5` and compare its all-ones argument
with FND-CONFIG-041.

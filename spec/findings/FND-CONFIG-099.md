---
id: FND-CONFIG-099
title: The pre-dispatch position and region helpers write separate fixed data ranges
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:009E..4328:00BC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4328:005A..4328:007C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4237:0076..4237:00A6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0452..1000:046E
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

For a nonnull accepted window, resident 3A8E:0D9A passes its words
at `+0x98` and `+0x96` to raw far target 3328:009E, and its pointer
at `+0x0C` to raw target 3328:005A (FND-CONFIG-096). With the
approved import's load segment 1000, those targets are resident
4328:009E and 4328:005A.

The first helper at file offset `0x0003851E` stores its first word
argument at DS:A348 and second at DS:A346, then returns zero.
These calls therefore copy the window's position words into two
separate data words.

The second helper at `0x000384DA` passes its pointer argument and
fixed destination DS:A2B4 to resident 4237:0076. That routine at
`0x000375E6` returns zero if either pointer is null; otherwise it
passes source and destination with byte count `0x008A` (138) to
resident 1000:0452. The bounded copy primitive at `0x00005652`
performs a forward copy of that many bytes and restores its saved
segment and index registers. Thus the ordinary nonnull call writes
the half-open range DS:A2B4 through DS:A33E.

The two coordinate words and that destination range do not overlap
DS:A11D, DS:A121, DS:A125, DS:A129 or DS:A12D, the window and
selection fields read in FND-CONFIG-095 and FND-CONFIG-096. Neither
helper has a control-tag dispatcher or a window/frame callback call.
Their runtime stack-guard failure path is outside this observation.

## Interpretation

These two direct callees have bounded position/region write targets;
they do not directly replace the selected-control or containing-window
pointers by writing those fields. This removes one previously unread
part of the intervening window routine while preserving its separate
old-window and handled-child gates.

## Alternatives

Source-pointer validity, the region's later consumers and other runtime
state changes remain unread (Q-CONFIG-008). These fixed writes may
matter to a later pointer or rendering operation without overlapping
the selected-control fields. No conclusion about clipping, visible
feedback or message-window success follows from the copy alone.

## How to reproduce

Inspect the two argument sequences at `0x000308D2..0x000308F9`
from FND-CONFIG-096. Map resident 4328 to file base `0x00038480`
and inspect its two bounded helpers at `0x0003851E..0x0003853C`
and `0x000384DA..0x000384FC`. Map resident 4237 to base
`0x00037570`, inspect its null checks and byte-count argument at
`0x000375E6..0x00037616`, then inspect the bounded copy primitive
at `0x00005652..0x0000566E`. Compare the fixed destination range
and coordinate-word addresses with the selection fields; do not infer
the region's later meaning from the copy operation.

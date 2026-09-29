---
id: FND-CONFIG-051
title: Overlay 171 reports allocation and character-list failures through the message entry
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV windows; FBOV fixup inspection
environment: null
---

## Observation

Three direct calls in overlay 171 target overlay 172's `566A:002A`
message entry (FND-CONFIG-035):

| Call file offset | Local condition and text argument |
|---|---|
| `0x00058369` | The preceding allocation call at `01D0:0008` returns a zero far pointer in `DX:AX`. The entry passes `DS:044E`, a memory-allocation failure message, then jumps to its exit path. |
| `0x00058465` | After a ten-iteration local loop, a refresh call and other UI/resource calls, the word at the state segment's `+0` is `0xFFFF`. The entry passes `DS:0458`, a no-available-characters message, then calls another local routine. |
| `0x000589A1` | A separate dispatch branch checks the same state word at `+0` for `0xFFFF`. It passes the same `DS:0458` text and jumps past the branch's other resource calls. |

The state segment appears as fixup word `0x02F8` in these instruction
windows and resolves to resident segment `4C4C` (FND-SAVE-004). All three
sites pass a far text pointer and clean up four argument bytes.

## Interpretation

These conditional failure paths enter the shared overlay 172 message
routine. FND-CONFIG-124 traces the two containing entries to the
stored-character list choice and window callback. They do not show
whether its later `WIND/10501` acquisition and
setup succeed, so they do not establish a wait or visible window
(FND-CONFIG-018).

## Alternatives

The character-list producer, the full meaning of the state word, and all
effects of the local refresh and resource calls were not read here.
FND-CONFIG-080 identifies two event-record discriminators for the callback's
message site, but does not establish the player action producing either.
The same message text can arise from two separate branches, and neither call
site proves how often that state occurs in live use.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x00058336..0x00058374`, `0x00058400..0x00058475`, and
`0x00058990..0x000589CF`. Resolve the three `0x0560` FBOV fixups to
overlay 172's `566A:002A` entry. Read only the short strings at
`DS:044E` and `DS:0458`.

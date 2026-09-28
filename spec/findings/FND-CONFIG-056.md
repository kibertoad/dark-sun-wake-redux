---
id: FND-CONFIG-056
title: Overlay 188 sends a diagnostic exit message when its debug gate is set
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5702:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:002A
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of a bounded FBOV window; FBOV fixup inspection
environment: null
---

## Observation

The routine at file offset `0x000747A1` in overlay 188 first calls
`0638:0039`, then checks `DS:143C`. If that byte is nonzero, it passes
`DS:19A0`, a diagnostic text about a GPL exit, to overlay 172's
`566A:002A` entry at `0x000747B4` (FND-CONFIG-035). If the byte is zero,
it skips the message call. Both paths then call `0030:00EE` and clear the
byte at `0388:000A`. The message call passes one far text pointer and
removes four argument bytes.

## Interpretation

This direct message call is gated by the same byte that other diagnostic
paths check (FND-CONFIG-044). Its later entry into the message-delay wait
still depends on overlay 172's `WIND/10501` acquisition and setup
(FND-CONFIG-018).

## Alternatives

The effects of `0638:0039` and `0030:00EE`, the route into this routine,
and live success through the message window remain unread. The local
condition does not establish that a player sees this diagnostic text.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical file offsets
`0x000747A1..0x000747CE`. Resolve the `0x0560` FBOV fixup at
`0x000747B4` to overlay 172's `566A:002A` entry. Read only the short
string at `DS:19A0`.

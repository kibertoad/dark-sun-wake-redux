---
id: FND-CONFIG-090
title: The item-feedback callback limits mouse event bits before its shared message path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 44D0:0053
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D..39D1:0BD2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:071F..39D1:0752
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0048
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident and FBOV disassembly
environment: null
---

## Observation

The mouse driver handler at `44D0:0053` queues seven words: packet kind 2,
byte length 14, zero, the returned `CX`, the returned `DX`, a segment word,
and the driver event bits from `AX` (FND-INPUT-005). The event builder at
`39D1:097D` copies those 14 bytes to its 24-byte output record beginning
at offset 6. Thus the seventh packet word becomes output event word at
offset 18. On the kind-two path at `39D1:0BA7`, the builder calls
`3D72:0009` to get a matched control number and writes that to output
offset 2; it also writes a separate word at offset 4, leaving the copied
word at offset 18 intact (FND-CONFIG-081).

The resident dispatcher copies the 24-byte event record to a window
callback's arguments (FND-CONFIG-079). Its first event word appears at
`[BP+6]` in overlay 213's `57CE:0048` callback, making `[BP+0x18]`
the record word at offset 18. On the event-two, control-`0x3BC9` path,
the comparison at file offset `0x000998A6` takes the route toward the
shared message call only when that word is below 8. Values 8 or greater
branch to `0x00099DEC`, past the call (FND-CONFIG-088). The comparison is
unsigned, so the admitted word values are `0..7`.

## Interpretation

The callback's previously unnamed stack-word gate limits the mouse
driver's queued event-bit word. A pointer hit on `BUTN/15305` can supply
the control identifier (FND-CONFIG-089), but the matching event reaches
the later message-selection code only with event bits numerically below
8 and with the remaining record and window gates passed.

## Alternatives

The meanings of the individual event-bit values and the physical device
actions that produce them were not established by this reading. Other
event producers could construct a record with the same control number and
offset-18 word. This finding does not establish that the message appears
or waits during ordinary play.

## How to reproduce

Read the queued mouse packet in FND-INPUT-005. Disassemble the resident
event builder at physical file offsets `0x0002F8AB..0x0002F8EE` and
`0x0002FAAF..0x0002FADC` to follow the 14-byte copy and kind-two writes.
Follow the 24-byte callback copy in FND-CONFIG-079, then disassemble
overlay 213 at `0x000998A4..0x000998B3` for the unsigned threshold and
branch target.

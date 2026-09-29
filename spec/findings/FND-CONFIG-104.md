---
id: FND-CONFIG-104
title: The resident APFM input branch maps queued mouse bit 2 to overlay 172 frame value 64
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0298..3D72:0364
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 566A:0043
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit disassembly; FBOV trampoline and MZ mapping
environment: null
---

## Observation

The resident pointer routine 3D72:0009 reads queued mouse event bits
at packet offset `0x0C` into DI (FND-CONFIG-094). An earlier input
branch at file offset `0x00032BB8` continues when DI has bit two
or eight set and the selected control pointer DS:A125 is nonzero.
It retains the containing window and selected record, and dispatches
its explicit two-tag table: APFM goes to `0x00032C3F`, BUTN to
`0x00032C22`.

The APFM branch independently tests bit eight and bit two. Bit eight
passes value 256 to frame dispatcher 3D72:0515 at `0x00032C5C`.
Bit two passes value 64 at `0x00032C7E`. When both bits are present,
the value-256 call precedes value 64, subject to its effects. The
calls pass the selected frame, its tag, containing window and packet.

The frame dispatcher requires a nonzero callback and a positive signed
intersection of the frame mask with the supplied value; it constructs
callback words 5, the frame identifier, and that value
(FND-UI-006, FND-CONFIG-094). The six shipped frames in WIND/14002
have mask `0x01E6`, which admits value 64. Overlay 172's registration
loop attempts to install 566A:0043 on those frames
(FND-CONFIG-074, FND-CONFIG-075). Successful registration and
selection therefore supply the event-five, value-64 branch that
contains the guarded selector call in FND-CONFIG-102.

## Interpretation

Queued mouse bit two provides a concrete resident producer for this
frame branch. It differs from the bit-four/value-32 path traced for
overlay 175 in FND-CONFIG-094. The callback's selected code and
remaining guards still decide whether its selector call reaches the
overlay 176 feedback entry.

## Alternatives

The physical action represented by mouse bit two is not established
here. Prior callback effects, pointer-hit state, registration and later
mask changes remain open (Q-CONFIG-008). A directly constructed frame
event may provide another route. This producer does not establish a
visible message or a successful message-window setup.

## How to reproduce

Inspect the pointer routine's input gate at
`0x00032BB8..0x00032BD6`, retained record and tag dispatch in bounded
blocks through `0x00032C3F`, and APFM bit tests and calls at
`0x00032C3F..0x00032C84`. Decode exactly two tags and two targets
at `0x00032E29..0x00032E35`, with resident file base
`0x00032920`. Compare the frame dispatch record in FND-UI-006 and
FND-CONFIG-094, shipped graph in FND-CONFIG-075, and callback
value table in FND-CONFIG-102.

---
id: FND-CONFIG-094
title: The resident pointer APFM branch maps input bit 4 to the item-feedback handler's value 32
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0476..3D72:04B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0515..3D72:059E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5689:005C
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

The resident event builder's kind-two path calls `3D72:0009` with its
local input packet at physical file offset `0x0002FABB`
(FND-CONFIG-081). The pointer routine reads a word at packet offset
`0x0C` into DI at `0x00032951`. That is the seventh word of the
14-byte mouse packet, carrying driver event bits (FND-INPUT-005).

The pointer routine searches registered windows and children, retaining
its current selected control pointer at DS:A125 and containing window
at DS:A121 (FND-CONFIG-081, FND-UI-011). Its later branch at
`0x00032C84` continues only if DI has bit `0x04` or `0x10` set.
After an intervening resident call it requires DS:A125 to be nonzero,
reads the selected record's tag and selects a four-tag dispatch table.
The APFM target is `0x00032D96`.

That APFM branch has two independent tests:

| Input bit | Frame-dispatch value | Call file offset |
|---|---:|---|
| `0x10` | `0x0080` | `0x00032DB3` |
| `0x04` | `0x0020` | `0x00032DD5` |

When both bits are present the value-128 call precedes the value-32
call, subject to the first call's effects. Each passes the input packet,
containing window, selected control and its tag to `3D72:0515`.

The frame dispatcher requires tag APFM, a nonzero control pointer,
a nonzero callback at control offset `0x62`, and a positive signed
intersection of its event mask at `0x58` with the supplied value
(FND-UI-006). For value 32 and shipped mask `0x01E6`, the intersection
is 32, which passes. It constructs a 24-byte callback record beginning
with three words: 5, the frame's resource number at offset 8, and the
supplied value. At `0x00032EB0` it invokes the callback at `0x62`
with that record as arguments.

For a successfully registered frame of WIND/13501, this supplies
handler `5689:005C` with one of numbers 11213 through 11218 as its
second word argument and 32 as its third. The handler's third-word
value-32 branch enters the conditional feedback helper
(FND-CONFIG-092, FND-CONFIG-093). This dispatch occurs synchronously
inside the pointer routine, before that routine returns to the event
builder. It does not require the later ordinary event-two window
callback route to enter this particular frame handler.

## Interpretation

A concrete producer of the handler's value 32 is the resident pointer
APFM branch with mouse-packet bit `0x04`. The static chain now reaches
from the queued packet through pointer selection and frame dispatch to
the registered feedback handler, with explicit pointer, tag, mask and
callback gates. This is a conditional route, not an observed message.

## Alternatives

The physical action that generates driver bit `0x04` is not established
here. Window traversal, selected-control state, the intervening call,
registration and subsequent helper results may still block the route.
Another producer may call the dispatcher or handler directly. Later
callback or mask changes remain open (Q-CONFIG-008). The locally
conditional no-effect and money-message sites still require their own
guards and successful message-window setup for a delay wait.

## How to reproduce

Map resident segment 3D72 to file base `0x00032920`. Inspect the
packet-word load at `0x00032929..0x00032954`, input-bit gate
and tag dispatch at `0x00032C84..0x00032CDE`, and APFM branch at
`0x00032D96..0x00032DDB`. Decode the four tag words and four
branch-target words at `0x00032E11..0x00032E29` to verify APFM's
branch. Inspect the frame dispatcher at
`0x00032E35..0x00032EBE` to follow its mask test, first three output
words and callback call. Inspect the event-builder argument at
`0x0002FAAF..0x0002FAC1`. Compare the mouse packet in FND-INPUT-005
and the handler and shipped graph in FND-CONFIG-092 and FND-CONFIG-093.

---
id: FND-CONFIG-089
title: A button in one overlay 213 window can supply the guarded message event identifier
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x32445..0x32880
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x31F66..0x32046
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:0061..57CE:010B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57CE:1720..57CE:18AF
tool: Python 3.14.7 bounded RESOURCE.GFF directory and control inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 213 passes window number `0x3C8E` (15502) and callback
`57CE:0048` to its window-creation call at file offset `0x000992CE`.
Another call at `0x0009A98F` passes window number `0x3C8F` (15503) with
the same callback (FND-CONFIG-088). Their shipped `RESOURCE.GFF` child
graphs differ:

| Window | Ordered children |
|---|---|
| `WIND/15502` | `BUTN/15305` at (75, 11), `APFM/15200` at (0, 0), `BUTN/15304` at (73, 50) |
| `WIND/15503` | `APFM/15200` at (0, 0), `EBOX/15400` at (9, 15) |

The `BUTN/15305` record is 18 by 18 and has event-mask word zero.
`BUTN/15304` is 27 by 11 and has mask `0x0050`. As FND-CONFIG-215 reads
from resident code, the pointer-hit path can return a matched button's
resource number as event word at offset 2 after a two-phase selection,
provided its mask bit 2 is clear and a local control check passes. The
resident dispatcher then calls the current window's callback at offset
`0xF5` when present (FND-CONFIG-079). The callback's event-two branch for
`0x3BC9` (15305) is the route toward the shared message call described in
FND-CONFIG-088.

## Interpretation

While `WIND/15502` is current and its button passes the resident input
checks, a pointer interaction on `BUTN/15305` can supply the event-two
identifier that enters overlay 213's guarded message branch. The same
direct button producer is absent from `WIND/15503`'s child graph. Later
callback guards and the message-window acquisition gate still determine
whether any text is shown or delayed.

## Alternatives

FND-CONFIG-217 traces the copied driver event bits to a later callback
threshold. The physical device action mapped to those bits, the local
control check, which window is current in a live state, and the selected
record fields remain open. Keyboard, synthetic or indirect event producers
could also supply `0x3BC9`; this finding only establishes the shipped
button's conditional pointer path.

## How to reproduce

Read `WIND/15502` at `RESOURCE.GFF+0x00032445` and `WIND/15503` at
`+0x0003273F` through the bounded GFF directory. Decode each child from
window offset `0x105`, then inspect the `BUTN/15305` and `/15304` records
at `+0x00031FD8` and `+0x00031F66` for size and mask. Disassemble overlay
213 around `0x000992C1..0x000992E4` and `0x0009A980..0x0009A9A5`
for the window numbers and callback pointer. Follow the generic event path
as in FND-CONFIG-215 and FND-CONFIG-079.

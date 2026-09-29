---
id: FND-CONFIG-083
title: The stored-character list callback is also installed as the global event fallback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0020
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0034
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0418
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:0752
tool: Python 3.14.7 FBOV trampoline and fixup inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

After passing `5664:002F` to the `WIND/18501` setup helper, overlay 171's
list-opening entry passes the same far address to overlay 190's
`571F:0034` at file offset `0x000583C0`. That trampoline reaches file
offset `0x000794E1`. Its routine stores the address at `DS:61A2` and
passes it to resident `39D1:0418`, which copies it to global far pointer
`DS:A0F1`. This is the second address-taking use noted in FND-CONFIG-124.

For an event whose first word is neither 1 nor 3, resident `39D1:0752`
tests the registered-window count. With a nonzero count, it first tries the
current window's `0xF5` callback. If the current pointer or callback is
absent, it clears the current pointer and calls `DS:A0F1` when nonzero,
passing the unchanged 24-byte event record. The event builder's generic
kind-one fallback clears the current-window pointer and changes the event
first word to 6 (FND-CONFIG-082). Overlay 171's callback has an event-six
branch for word `0x1C0D` at event offset 12 (FND-CONFIG-080).

## Interpretation

The list callback has two indirect resident entry routes: the loaded
window's `0xF5` field and the global `DS:A0F1` fallback. While the list
registration remains in `DS:A0F1` and a window is registered, a queued
keyboard word `0x1C0D` that takes the generic event-six fallback can reach
the callback's message branch even though that fallback clears `DS:A0FD`.
The branch still requires the list state word to be `0xFFFF` and successful
message-window setup for a delay wait.

## Alternatives

FND-CONFIG-084 shows one temporary replacement and restoration path, but
other setter calls, the list window's live registered state, and the
producer of its failure-state word are not fully read. The code establishes
a conditional route. Whether the failure message appears during ordinary
play remains open. The BIOS key word is not assigned a
physical-key label here.

## How to reproduce

Disassemble overlay 171 at `0x00058395..0x000583C8` and resolve the
`0x05F0` fixup of the far call with offset `0x0034` to descriptor 190.
Map its `571F:0034` trampoline to `0x000794E1` and follow the call to
resident `39D1:0418` at file offset `0x0002F328`. Read the fallback path
`0x0002F6E2..0x0002F75F`, and compare the event builder and callback table
in FND-CONFIG-082 and FND-CONFIG-080.

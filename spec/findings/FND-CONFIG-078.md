---
id: FND-CONFIG-078
title: The stored-character list opens with a message-capable window callback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0020..5664:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
tool: Python 3.14.7 FBOV fixup, MZ relocation and trampoline inspection; Capstone 5.0.7 bounded 16-bit disassembly
environment: null
---

## Observation

Overlay 171's `5664:0020` trampoline targets file offset
`0x00058336`. It contains the allocation-failure message at
`0x00058369` and the first no-available-characters message at
`0x00058465` (FND-CONFIG-051). Its only direct far call from another
overlay is at `0x00079214` in overlay 190. That caller reaches the
branch after a choice call to overlay 172's `566A:0025` returns one,
then passes the word at `0370:0B44` to `5664:0020`. The choice
arguments include a maximum-characters heading, a delete-characters
action and cancellation; the intervening indirect callback and helper
effects have not been read through.

Within `5664:0020`, the code passes resource number `0x4845`
(18,501) and far address `5664:002F` to overlay 182's window helper
`56BD:0048` at `0x000583A4`. That helper requests a `WIND` resource
and stores the supplied callback in the loaded window's offset
`0xF5` through resident `3A8E:0CDC` when acquisition succeeds
(FND-CONFIG-030). The caller stores the returned far window pointer
at `4C4C:0004`. `RESOURCE.GFF#WIND/18501` is the stored-character list
graph (FND-UI-027, SCR-UI-003).

The `5664:002F` trampoline targets file offset `0x000585D5` and
contains the second no-available-characters message at
`0x000589A1` (FND-CONFIG-051). Declared overlay fixups and resident
MZ relocations contain no direct far call to `5664:002F`; its address
is pushed twice in the list-opening routine: once for the window helper
and once for overlay 190's global callback registration (FND-CONFIG-083).
An aligned near-call scan of overlay 171 found no near
call to the `002F` entry.

## Interpretation

The list-opening entry has a direct route from overlay 190's
choice branch and has two local message gates. It installs a callback
with a third message gate on the stored-character list window and in the
resident global fallback (FND-CONFIG-083).
Entering any of the messages still depends on the local branches and
the shared routine's later window-acquisition gate (FND-CONFIG-018).

## Alternatives

FND-CONFIG-079 identifies the resident event dispatcher that can invoke
`5664:002F`, and FND-CONFIG-080 identifies the callback's two message-branch
discriminators. The event's player-visible source, the full origin of the
overlay 190 choice, and computed or unrelocated caller pointers remain
unread. A stored callback pointer is not evidence that a particular
event occurred or that the message-delay wait ran.

## How to reproduce

Map overlay 171's header at `0x0004B840`, code start `0x00058210`,
and trampolines `0020` and `002F` to code offsets `0x0126` and
`0x03C5`. Search declared overlay fixups and resident MZ relocations
for those two entries. Disassemble `0x000791D0..0x0007921C`,
`0x00058336..0x00058435` and the message windows of FND-CONFIG-051.
Map overlay 182's `0048` helper to `0x000689BB` and compare its callback
storage call with FND-CONFIG-030. FND-UI-027 gives the shipped window graph.

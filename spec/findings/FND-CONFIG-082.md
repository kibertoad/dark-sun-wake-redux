---
id: FND-CONFIG-082
title: The keyboard packet supplies the event-six word to the global fallback
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4464:0382
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D..39D1:0BD2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3BA6:13A4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3EBE:0BFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4066:0003
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

The keyboard interrupt path queues a packet of five words: kind 1, length
10, zero, the BIOS key word, and shift flags (FND-INPUT-005). The reader at
`4464:0382` removes that packet from the shared queue. The event builder
`39D1:097D` copies its payload into the 24-byte output record beginning at
offset 6, putting the BIOS key word at output offset 12. Thus a queued key
word `0x1C0D` can supply the word used by overlay 171's event-six table
(FND-CONFIG-080).

For a kind-one packet, the builder first checks focused-control state. Its
window path can call a MENU child handler at `3BA6:13A4`. If that does not
handle the packet, window flag `0x80` permits an ACCL child search at
`4066:0003`; flag `0x40` permits a BUTN mnemonic search at `3EBE:0BFD`.
Those handlers can store a window pointer at `DS:A0FD` and a control number
at output offset 2. The fallback at `39D1:0B09` instead clears `DS:A0FD`
and changes the output's first word to 6. The subsequent resident
dispatcher can call global `DS:A0F1` when the window pointer is zero
(FND-CONFIG-079); FND-CONFIG-083 traces the list callback's registration
there.

The shipped `WIND/18501` graph has no MENU or ACCL child (FND-UI-027),
and the shipped BUTN records have zero at their mnemonic byte `0x6C`
(FND-UI-004). The native handlers inspect those tags or byte before
claiming a key.

## Interpretation

The queued BIOS key word can match the list callback's `0x1C0D` table
entry. The generic event-six fallback clears the window target but can
invoke the same callback through its global registration (FND-CONFIG-083).
The specialized handlers provide no match from the shipped list controls
as recorded in the source file. The keyboard route remains conditional on
the global registration and the list state.

## Alternatives

Runtime changes to window flags, children or button mnemonic fields, and
other writers of event records, remain unread. The focused-control path
may consume a key through another route. This finding does not label
`0x1C0D` as a physical key or prove that the failure message appears in
ordinary play.

## How to reproduce

Read the packet layout in FND-INPUT-005. Map `4464:0382` to file offset
`0x00039BC2` and disassemble through `0x00039C5E`. Map `39D1:097D` to
`0x0002F88D`; follow the 14-byte copy, kind-one branch
`0x0002F91C..0x0002FA2A`, and the callback target test in
`0x0002F6E2..0x0002F712`. Disassemble the three specialized handlers at
`0x00031F18`, `0x00035863` and `0x000349DD`, then compare their tag and
mnemonic-byte tests with FND-UI-004 and FND-UI-027.

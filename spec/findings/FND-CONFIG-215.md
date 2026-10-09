---
id: FND-CONFIG-215
title: The resident pointer path returns a matched button number as an event-two identifier
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D..39D1:0BD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0009..3D72:04F1
tool: Python 3.14.7 and Capstone 5.0.7 bounded 16-bit resident disassembly; MZ segment mapping
environment: null
---

## Observation

The resident event builder at `39D1:097D` initializes the 24-byte output
record's first word to 2. When its input decoder returns class 2, it calls
`3D72:0009` with the input record. A nonzero return is copied to the output
record's word at offset 2; the next resident loop does not discard that
record for having both offset-2 and offset-4 words zero (FND-CONFIG-079).

`3D72:0009` searches the registered windows and children under the pointer,
recording a selected child and its containing window. One input-bit branch
remembers the selected child pointer and child index. A later input-bit
branch compares both with the current hit. For a `BUTN` child it requires
the child's event-mask bit 2 to be clear, then passes a local control check
and returns the word at button offset `0x5A`. Before returning it copies
the containing window pointer to `DS:A0FD`, which the resident event
dispatcher later uses for the window's `0xF5` callback (FND-CONFIG-079).

Every shipped `BUTN` record stores its own resource number at `0x5A`
(FND-UI-004), and `BUTN/18301` in `WIND/18501` has mask zero
(FND-UI-027). Overlay 171's list callback routes event first word 2 with
offset-2 word 18301 to its guarded failure message (FND-CONFIG-080).

## Interpretation

A two-phase pointer interaction that selects and remains on `BUTN/18301`,
passes the local control check, and occurs while the list window is current
can produce the event-two identifier used by the list callback's message
branch. This supplies a concrete generic-input route from the shipped
button record to that branch; the branch still requires the list state word
to be `0xFFFF` and successful message-window setup for a delay wait.

## Alternatives

The device or physical action that the input library maps to the two input
bit groups was not read through here. The local control check and the
list-state producer have further conditions. This reading does not prove
that an ordinary player action reaches the failure state or visibly shows
the message.

This replaces FND-CONFIG-081, whose locations ended at `39D1:0BD2` and `3D72:04F0`, the first bytes
of the closing `retf` instructions of the routines they cover, instead of the byte after them, and
so did the same ranges where the text repeats them. The documentation check found this when the
reconciled `DSUN.EXE` inventory placed functions whose last bytes are those returns. The ends now
give the byte after each return. Its other observations are unchanged.

## How to reproduce

Map resident segments `39D1` and `3D72` to file bases `0x0002EF10` and
`0x00032920`. Disassemble `0x0002F88D..0x0002F911`,
`0x0002FAAF..0x0002FADE`, `0x00032929..0x00032E11`, and the event loop
`0x0002F7D8..0x0002F818` in bounded windows. Decode the four tag words
and target offsets at `0x00032E11` to identify the `BUTN` branch at
`0x00032D24`; follow its match tests through the return of the word at
`0x5A`. Compare FND-UI-004, FND-UI-027 and FND-CONFIG-080.

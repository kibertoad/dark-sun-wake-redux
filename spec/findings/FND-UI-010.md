---
id: FND-UI-010
title: Three EBOX lookup wrappers have no direct callers, and the child dispatcher is reached from one input path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 409B:189C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4228:002B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4228:00A9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3D72:0009
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:097D
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The routines at `409B:189C`, `4228:002B` and `4228:00A9` each pass a number they are given and the
tag `EBOX` to the tag-aware resource lookup that the window code uses, and test what it returns.
Ghidra finds no direct caller of any of the three.

The child dispatcher `3D72:0EB8` tells `APFM`, `BUTN` and `EBOX` children apart (FND-UI-006,
FND-UI-011). Its only two direct callers are inside `3D72:0009`, whose one direct caller is at
`39D1:097D`, reached for one class of decoded input event. `3D72:0009` passes the current pointer
state to the child dispatcher twice, keeps the child and event it returns, and enters tables of
far handlers under tests of event bits. Neither routine was found to draw an image, fill,
outline, clip or set a palette.

## Interpretation

Edit boxes are looked up by tag and number like the other controls, and the generic input path
finds the control under the pointer and calls a handler for it. How an edit box draws its frame
or text is not shown by this code.

## Alternatives

The wrappers may be called through far pointers or from overlay code. The handler tables and which
screens fill them are not known.

## How to reproduce

Search for the tag bytes `EBOX` (`0x584F4245`) used as a pushed operand to find the three
wrappers, and list the references to each, to `3D72:0EB8` and to `3D72:0009`.

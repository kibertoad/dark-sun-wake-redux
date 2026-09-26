---
id: FND-UI-009
title: A window's two handler pointers at 0xF9 and 0xFD are read only by the activation routine
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:05D5..3A8E:05E3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:06FA..3A8E:08BB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0D0C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:0D3C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 3A8E:1048..3A8E:107E
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The registration routine `3A8E:02B3` clears the far pointers at `0xF9` and `0xFD` of the window
record at `3A8E:05D5` and `3A8E:05DF`. The activation routine `3A8E:060D` tests the pointer at
`0xFD` at `3A8E:06FA` and calls it at `3A8E:0707` when it is set, dispatches the resolved
children, then tests the pointer at `0xF9` at `3A8E:08AA` and calls it at `3A8E:08B7`.

A query over the whole program for accesses at structure offsets `0xF9` and `0xFD` found only
these reads, the two clears, and two setters at `3A8E:0D0C` and `3A8E:0D3C`. The only direct
call to `3A8E:060D` is at `3A8E:107A`, in the loop at `3A8E:1048` that scans the registered
windows. Ghidra finds no direct caller of `3A8E:02B3`, `3A8E:0D0C` or `3A8E:0D3C`.

## Interpretation

A window can carry two handlers, one called before its children are dispatched and one after, set
while the game runs through the two setters. The code that registers a window and installs its
handlers is reached indirectly, so what a screen does when a control is used is not found by
following direct calls.

## Alternatives

The routines without direct callers may be called through far pointers, from overlay code, or not
at all.

## How to reproduce

Read `3A8E:02B3` and `3A8E:060D` for accesses at `0xF9` and `0xFD`, search the program for
instructions with those displacements, and list the references to `3A8E:02B3`, `3A8E:060D`,
`3A8E:0D0C` and `3A8E:0D3C`.

---
id: FND-SCRIPT-012
title: The script instructions that print, show a portrait and play sound pass their parameters to far routines
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:16BD..172C:170B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1742..172C:1763
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The handlers, from the table of FND-SCRIPT-005, with parameters read as FND-SCRIPT-010 describes
(p0 and p1 the values at `4C13:00B4` and `4C13:00B8`, and `4C13:00D8` the address p1 left):

| Opcode | Handler | What it does |
|---|---|---|
| `0x4F` | `16BD` | reads 2 parameters and far-calls `5702:004D` with the far pointer at `4C13:00D8` and the low byte of p0 |
| `0x50` | `16E1` | reads 2 parameters and far-calls `5702:0052` with p1 as 32 bits and the low byte of p0 |
| `0x51` | `1705` | far-calls `5702:0057` |
| `0x54` | `1742` | reads a number and far-calls `5702:009D` with its low word |
| `0x5D` | `174D` | reads a number and far-calls `2D40:0B00` with its low word |
| `0x5F` | `1758` | reads a number and far-calls `2D40:0B0F` with its low word |

`5702:004D`, `0052`, `0057` and `009D` are entries of the resident header of overlay 188.

## Interpretation

`0x4F` prints the text of its second parameter, a string (FND-SCRIPT-010), and `0x50` prints a
number; the first parameter of both is passed on as a byte whose use was not read. `0x51` starts
a new line. `0x54` shows a portrait: GPL resource 135 uses it with 18 before the conversation
FND-UI-016 captured, whose portrait is `GPLDATA.GFF#PORT/18`, and the `PORT` tag occurs only in
overlay 199 (FND-EXE-006). `0x5D` and `0x5F` pass a number to two routines of the resident
segment `2D40`, which by the names SRC-OPENDS-5C6CBD7 gives them play a sound and music.

## Alternatives

None of the far routines was read, so where the text goes, what the byte parameter selects, and
what `2D40:0B00` and `2D40:0B0F` do are open.

## How to reproduce

Disassemble the handlers at the addresses above.

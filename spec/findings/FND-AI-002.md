---
id: FND-AI-002
title: The four COMPUTER CONTROL strings are resident data at 57E0:1D17 to 57E0:1D89, pushed only by overlay 190
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1D17..57E0:1DA0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1; Python 3.14.7 byte search
environment: null
---

## Observation

`DSUN.EXE` holds the upper-case text `COMPUTER CONTROL` four times, at file offsets 322,839,
322,902, 322,929 and 322,953 (`0x4ED17`, `0x4ED56`, `0x4ED71`, `0x4ED89`). These lie in the
resident data segment, at `57E0:1D17`, `57E0:1D56`, `57E0:1D71` and `57E0:1D89`, the strings
`COMPUTER CONTROL IS `, `COMPUTER CONTROL IS LOCKED`, `COMPUTER CONTROL IS OFF` and
`COMPUTER CONTROL IS ON`. In the overlay-mapped copy (`5000:9B17` and so on) Ghidra records no
reference to any of them. A search for `push` of each offset finds one each, all in overlay 190
(code at file offset `0x78340`, header segment `571F`), at offsets `0x0526`, `0x0A0A`, `0x0A50`
and `0x0A80` (FND-COMBAT-024).

## Interpretation

The strings are the messages and the hover text of the computer-control buttons, which overlay
190 handles (FND-AI-003).

## Alternatives

The legacy record placed the four occurrences in the overlay pack and read the absence of Ghidra
references as the strings being unused. The file offsets are below the overlay pack, which starts
at `0x57570` (FND-EXE-001), and the overlay code pushes the offsets as plain numbers, which Ghidra does not
record as references to the data segment.

## How to reproduce

Search `DSUN.EXE` for `COMPUTER CONTROL`, convert each file offset `f` to `57E0:(f - 0x5200 +
0x10000 - 0x57E00)`, and search the file for `68` followed by each offset.

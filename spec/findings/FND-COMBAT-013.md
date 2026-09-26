---
id: FND-COMBAT-013
title: The word at 57E0:1440 selects the coordinate branch at 5 and a leader-change refusal path at 2
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:1440..57E0:1442
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

In the overlay-mapped copy of `DSUN.EXE`, Ghidra records 27 direct references to the word at
`57E0:1440` (mapped `5000:9240`) from 13 functions, nine of them writes. The reads compare it with
0, 1, 4 and 5; the writes store 0, 1 or a register. The routine at `DSUN.EXE+0x00069FD3`, offset
`0x1783` of overlay 182 (header segment `56BD`), has one guarded path that switches on the word
for 1 to 5: cases 1 to 4 do different things, case 2 leads to the message `CAN'T CHANGE LEADER IN
COMBAT`, and 5 falls through to shared code. The input loop's coordinate branch (FND-COMBAT-012)
is taken when the word is 5. The routine has no recovered direct caller.

## Interpretation

The word is a mode of the input handling. In mode 2 the routine can refuse a change of leader
during combat, and in mode 5 the input loop hands the pointer's coordinates on. What each value
stands for is not known.

## Alternatives

A byte search finds the one push of the message's address in overlay 190, at offset `0x0905`
(FND-COMBAT-024), not in overlay 182, so the overlay 182 case reaches it through a call; that was
not followed. FND-COMBAT-015 shows the word also takes 19.

## How to reproduce

In the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", list the
references to `ram:00059240`, decompile mapped `74BB:0223`, and convert with the overlay map
reporter.

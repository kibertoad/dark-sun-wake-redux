---
id: FND-TALK-001
title: Script instruction 0x48 lists up to 25 menu entries whose condition is 1 and pushes a frame at the chosen entry's target
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2CF8..172C:2DB9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:00FE..172C:015E
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The dispatch table entry for opcode `0x48` (FND-SCRIPT-005) is `172C:16B9`, which calls
`172C:2CF8` and returns. Addresses below are in segment `172C` unless given; the helpers are those
of FND-SCRIPT-006 and FND-SCRIPT-010.

`2CF8` keeps a byte count, starting at 0, and:

1. calls `00FE`;
2. calls `3669`, which reads a number and returns the address the reader left, and far-calls
   `5702:0048` with the word 0 and that address;
3. then, while the byte `281B` returns is not `0x4A` and the count is 24 or less:
   1. calls `00FE`;
   2. calls `3669` and keeps the address;
   3. reads a number with `3278` and stores its low word at `4C13:027D + 2 * count`;
   4. reads a number with `3278` and stores its low byte at `4C13:02F9 + count`;
   5. when that byte is 1, far-calls `5702:0048` with count + 1 and the kept address, and adds 1
      to the count;
   6. calls `00FE`;
4. reads one byte with `2805`, whatever it is;
5. far-calls `5702:0025`, takes the returned byte minus 1, and far-calls it again until the result
   is 0 or more and less than the count;
6. calls `01C1`, which pushes a frame, with the word at `4C13:027D` for that result.

`00FE`, while the byte `281B` returns is `0x23`, `0x28`, `0x2E` or `0x4B`, fetches it with `20F5`
and runs the same routine the dispatch table gives for that opcode: `016E`, `2C05`, `2BF6` and
`017D` (reached for `0x4B` through `0D9C`). Its table of the four opcodes and their routines is at
`172C:015E`.

`4C13:0295` is the first frame offset of FND-SCRIPT-006, 12 words after `4C13:027D`, and
`4C13:0313`, the buffer size, is 26 bytes after `4C13:02F9`.

## Interpretation

Instruction `0x48` offers a menu. Its first parameter is a title, passed to `5702:0048` as row 0.
Each entry that follows has three parameters: a label, a target offset and a condition. An entry
is offered when the low byte of its condition is 1; offered entries are numbered from 1 in the
order they appear. Entries end at byte `0x4A`, which the instruction consumes, or after 25
offered entries, when the instruction consumes the next byte whatever it is. `5702:0025` returns
the chosen row number from 1; the instruction then runs the entry's target as a local
subroutine of the script (RULE-SCRIPT-002), so a local return at the target's end resumes the
script after the menu. Trace instructions before and between entries run as they would anywhere.

The targets array holds 12 words before the frame offsets begin, so the targets of the 13th and
later offered entries (count 12 to 24) are written over `script_frame_offsets[0]` to `[12]`.

## Alternatives

What `5702:0048` and `5702:0025` do has not been read: they are entries of the resident header of
overlay 188. That they draw a row and wait for the player's choice rests on the capture of
FND-TALK-004 and the manual.

## How to reproduce

Read word `0x48` of the table at `57E0:030A` (FND-SCRIPT-005), disassemble `172C:16B9`,
`172C:2CF8` to `172C:2DB9` and `172C:00FE` to `172C:015E`, and read the eight words at
`172C:015E`.

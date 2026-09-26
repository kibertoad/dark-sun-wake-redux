---
id: FND-SCRIPT-006
title: The interpreter reads code at a per-frame offset into the loaded script and keeps up to 50 frames
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:01C1..172C:0299
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:20B3..172C:210B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:27B0..172C:284D
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Segment `4C13` holds, among others:

| Address | Type | Use in the routines below |
|---|---|---|
| `4C13:0192` | `INT8` | frame depth |
| `4C13:0193` | `UINT16` | offset of the current script in the script buffer |
| `4C13:0255` | `FARPTR<BYTE>` | the script buffer |
| `4C13:0295` | `UINT16[50]` | code offset of each frame |
| `4C13:013F` | `UINT8[50]` | saved `4C0E:0003` of each frame |
| `4C13:010D` | `UINT8[50]` | saved `4C0E:0004` of each frame |
| `4C13:0313` | `UINT32` | size of the script buffer, written only by overlay 169 |
| `4C13:032A` | `UINT8` | last opcode fetched |

- `172C:281B` returns the byte at the script buffer plus `4C13:0193` plus the code offset of the
  current frame, `4C13:0295 + 2 * depth`. `172C:27B0` does the same with a byte argument added to
  the offset.
- `172C:20B3` adds 1 to the current frame's code offset and far-calls `5702:00B1` when the new
  offset, as an unsigned value, is not below `4C13:0313`.
- `172C:2805` reads a byte with `281B` and advances with `20B3`. `172C:20F5` does the same and
  stores the byte at `4C13:032A`.
- `172C:27EF` reads two bytes with `2805` and returns the first times 256 plus the second.
- `172C:01C1(offset)` adds 1 to the frame depth. When it reaches 50 it sets the depth to 0 and
  far-calls `5702:00B1`. It then stores the byte at `4C0E:0003` at `4C13:013F + depth`, the byte at
  `4C0E:0004` at `4C13:010D + depth`, and the offset in the new frame's code offset.
- `172C:0241` copies `4C13:013F + depth` back to `4C0E:0003` and `4C13:010D + depth` to
  `4C0E:0004`, subtracts 1 from the depth, and far-calls `5702:00B1` when the depth is then below
  0.

`5702:00B1` is an entry of the resident header of overlay 188. It is called from every failed
check of the interpreter.

## Interpretation

A frame is one level of local subroutine call inside a script: it holds where to read next and
the depths of the `if` and `compare` nesting (FND-SCRIPT-009) at the time of the call, which a
return restores. Words in the code are stored high byte first. `5702:00B1` reports an error of the
interpreter.

## Alternatives

What `5702:00B1` does, and whether it returns, was not read. The check against `4C13:0313`
compares a code offset inside one script with the size of the whole buffer, so it catches only a
runaway offset.

## How to reproduce

Disassemble the three ranges, and search overlay code for writes to `4C13:0313`
(`66 26 A3 13 03`).

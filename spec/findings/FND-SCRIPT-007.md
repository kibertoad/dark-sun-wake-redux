---
id: FND-SCRIPT-007
title: 172C:000C runs a script from a start offset until its frames unwind or it stops, and 172C:0299 and 172C:031F enter and leave nested scripts
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:000C..172C:00EE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0299..172C:0388
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`172C:000C` is a far routine with three word arguments: a script number, a start offset and a
selector. When the word at `4C0E:000B` is not 2 it returns at once. Otherwise it:

1. calls `172C:31ED`;
2. stores 0 in the stop byte `4C13:0326`, 1 in the word `4C0E:0009`, and 0 in the bytes
   `4C13:032B`, `4C13:0325`, `4C13:0327` (script depth), `4C13:0192` (frame depth), `4C0E:0003`
   and `4C0E:0004`;
3. calls `172C:00A1` with the three arguments and the frame depth, now 0;
4. stores 0 in the stop byte again, and returns.

`172C:00A1(number, start, selector, depth)` does nothing when the number is 0 or the stop byte is
1. Otherwise it calls `172C:0299(number, start, selector)`, then repeats, while the frame depth is
not below `depth` and the stop byte is 0: fetch a byte with `172C:20F5` and pass it to the
dispatcher `172C:018F` (FND-SCRIPT-005).

`172C:0299(number, start, selector)` does nothing when the stop byte is 1. Otherwise it stores 1
in `4C0E:0009`, adds 1 to the script depth, stores the number at `4C13:025D + 2 * depth` and the
selector at `4C13:0199 + 2 * depth`, and calls the loader `172C:0388(number, selector)`
(FND-SCRIPT-019). When the loader returns 0 it far-calls `5702:00B1`. Then, when the stop byte is
0, it calls `172C:01C1(start)` to push a frame (FND-SCRIPT-006). Nothing checks the script depth
against a limit.

`172C:031F` subtracts 1 from the script depth. When the depth is still above 0, it calls the
loader with the number and selector stored for that depth, and far-calls `5702:00B1` when the
loader returns 0. When the depth is 0, it calls `172C:00EE`, which stores 1 in the stop byte.
Then, when the stop byte is 0, it calls `172C:0241` to pop a frame.

## Interpretation

`172C:000C` is the entry the rest of the game uses to run a script, and it runs only in the state
where `4C0E:000B` is 2. One run starts from empty frame and script stacks and ends when something
sets the stop byte, or when the frame depth falls below 0; the stop byte is cleared when the run
ends. `0299` is how one script calls another: the callee is loaded in place of the
caller, and `031F` reloads the caller when the callee returns. A return from the first script of a
run stops the run instead of popping its frame.

## Alternatives

What `172C:31ED` does, what the word `4C0E:000B` means, and what the bytes `4C13:032B` and
`4C13:0325` and the word `4C0E:0009` are used for elsewhere were not read. Frames and script
depths are 8-bit, and `4C13:025D + 2 * depth` meets the frame offsets at `4C13:0295` when the
script depth reaches 28; what happens then was not traced.

## How to reproduce

Disassemble `172C:000C` to `172C:00EE` and `172C:0299` to `172C:0388`.

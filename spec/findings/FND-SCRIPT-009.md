---
id: FND-SCRIPT-009
title: The handlers of the control-flow, accumulator and assignment instructions
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0AEA..172C:0AFE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0C5F..172C:0C8B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0D93..172C:0EE9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1127..172C:112C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1587..172C:1610
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1B42..172C:1B88
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:24A0..172C:25D6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2C1A..172C:2CF8
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Addresses in segments `4C13` and `4C0E`. The accumulator is the double word at `4C13:031B`. "Read
a number" is a call of `172C:3278`, and "read n parameters" a call of `172C:367B(n)`, which leave
the values at `4C13:00B4 + 4 * i` (FND-SCRIPT-010). "Jump to x" is `172C:2CD7(x)`, which stores x
as the current frame's code offset.

| Opcode | Handler | What it does |
|---|---|---|
| `0x06` | `0AEA` | reads 1 parameter and adds 1 to the 16-bit word at its address (`4C13:00D4`) |
| `0x0E` | `0C5F` | sets the accumulator to 1 when it is 0, and to 0 otherwise |
| `0x12` | `0D93` | reads a number and jumps to its low word |
| `0x13` | `0DA0` | reads a number and pushes a frame at its low word (`172C:01C1`) |
| `0x14` | `0DA9` | reads 2 parameters and calls `172C:0299` with script number p1, start p0 and selector 1 |
| `0x15` | `0DC9` | pops a frame (`172C:0241`) |
| `0x16` | `0DCD` | reads a number into the accumulator, then calls `172C:2C1A` |
| `0x17` | `0DD4` | reads a number into the accumulator; adds 1 to the byte `4C0E:0004`, far-calls `5702:00B1` when it reaches 8; stores 0 at `4C13:0104 + depth` and the accumulator in the double word at the far pointer `4C13:0249` plus `4 * depth` |
| `0x18` | `0ED3` | reads a number into the accumulator |
| `0x19` | `0EE5` | calls `172C:031F` (FND-SCRIPT-007) |
| `0x27` | `0E31` | reads 2 parameters; when the double word stored by `0x17` for the current depth differs from p0, jumps to p1, and otherwise stores 1 at `4C13:0104 + depth` |
| `0x29` | `0E89` | reads a number; when the byte at `4C13:0104 + depth` is 1, jumps to it |
| `0x31` | `1127` | calls `172C:00EE`, which sets the stop byte |
| `0x3E` | `1587` | reads 1 parameter; adds 1 to the byte `4C0E:0003`, far-calls `5702:00B1` when it reaches 32; stores the accumulator's low byte at `4C13:0171 + depth`; when the accumulator is 0, jumps to p0 |
| `0x3F` | `15E2` | reads 1 parameter; when the byte at `4C13:0171 + depth` is not 0, jumps to p0 |
| `0x61` | `0EB9` | subtracts 1 from `4C0E:0004`, and far-calls `5702:00B1` when it is below 0 |
| `0x63` | `1B42` | reads 1 parameter; when the accumulator is 0, jumps to p0 |
| `0x64` | `1B65` | reads a number and jumps to it |
| `0x67` | `1B6E` | subtracts 1 from `4C0E:0003`, and far-calls `5702:00B1` when it is below 0 |

"depth" in a row is the byte at `4C0E:0004` for `0x17`, `0x27`, `0x29` and at `4C0E:0003` for
`0x3E` and `0x3F`, read after any change the row makes. Parameters are 32-bit, and the jumps use
their low 16 bits.

`172C:2C1A` reads a byte k and keeps its low 7 bits. When bit 6 of k is set, k loses it and the
variable number is two bytes, high byte first; otherwise it is one byte. When k is below `0x10`,
it reads the number and calls `172C:24A0(k, number, accumulator)`; otherwise it calls
`172C:2914(accumulator)`, which was not read. `172C:24A0` writes the accumulator to:

| k | Destination |
|---|---|
| 2 | the `INT16` at the far pointer `4C13:0337` plus `2 * number` |
| 5 | the `INT32` at the far pointer `4C13:0333` plus `4 * number` |
| 7 | the `INT16` at the far pointer `4C13:0347` plus `2 * number` |
| `0x0A` | the `INT32` at the far pointer `4C13:0343` plus `4 * number` |
| `0x0D` | bit `number % 8` of the byte at the far pointer `4C13:034B` plus `number / 8`: set when the accumulator is not 0, cleared otherwise |
| `0x0E` | the same in the bits at the far pointer `4C13:033B` |
| 3, 4, 6, 8, 9, `0x0B`, `0x0C` | nothing |

The masks come from the tables at `57E0:02FA` (`01 02 04 08 10 20 40 80`) and `57E0:0302`
(`FE FD FB F7 EF DF BF 7F`). After a write for k = 2, 5 or `0x0E`, it stores 1 in the byte
`4C0E:0000`.

## Interpretation

`0x12` is a jump, `0x13` and `0x15` a call and return inside a script, `0x14` and `0x19` a call
of another script and the return from it, and `0x31` stops the interpreter. `0x3E`, `0x3F` and
`0x67` are `if`, `else` and `endif`: `if` records whether its test was true so that `else` can skip
its part, with 32 levels. `0x63` and `0x64` are the head and the back jump of a `while` loop.
`0x17`, `0x27`, `0x29` and `0x61` form a `compare` block with 8 levels: `0x17` records a value,
each `0x27` jumps past its case unless the value matches, and `0x29` leaves the block once a case
has matched. `0x16` assigns a number to a variable, and `0x06` adds 1 to one. The variable kinds
are those FND-SCRIPT-010 reads, and the byte `4C0E:0000` records that a local variable changed.

## Alternatives

The meaning of the kinds as local or global variables follows SRC-LIBGFF-839B11D; nothing here
shows how long each kind lives. `0x06` changes a word wherever its parameter's address points,
which for a flag or a string kind is not a number.

## How to reproduce

Read the handler addresses from the table of FND-SCRIPT-005 and disassemble each handler, then
`172C:2C1A` to `172C:2CF8` and `172C:24A0` to `172C:25D6` with its jump table at `172C:25BC`.

---
id: FND-SCRIPT-010
title: 172C:3278 reads an expression of literals, variables and operators left to right with 8 levels of parentheses
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3278..172C:36CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:210B..172C:24A0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:25D6..172C:2805
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Addresses in segment `4C13` unless given. `172C:3278` keeps 8 double-word values and 8 pending
operators, all 0 at the start, and a level `si` starting at 0. It sets the far pointer at `024D`
to the accumulator `031B`. Then it repeats: read a byte b and act on it.

| b | Action |
|---|---|
| `0x00` to `0x7F` | the value is `b * 256` plus the next byte, sign-extended from 16 bits |
| `0x80`, `0xC0` | the value is the accumulator |
| `0x81` to `0x8A`, `0x8D`, `0x8E`, and the same plus `0x40` | a variable (below) |
| `0x8B`, `0xCB` | the value is two words, the first the signed high half |
| `0x8C`, `0xCC` | saves 64 bytes from `00B4` in a stack of 2 at `0034` (far-calls `5702:00B1` when full), reads and dispatches one instruction (FND-SCRIPT-005), restores them, and takes the accumulator |
| `0x8F`, `0xCF` | the value is the next byte, sign-extended |
| `0x90` | the value is the next word, sign-extended |
| `0x91` | the value is the next word, negated and sign-extended |
| `0x92` | a string (below); the value is 0 |
| `0xB1` | the value `172C:284D` returns, which was not read |
| `0xD1` to `0xDF` | an operator: stored as the pending operator of level `si`; reading goes on |
| `0xE2` | `si` goes up by 1 (far-call `5702:00B1` at 8); the value is 0 |
| `0xE1` | the value is value `si`; `si` goes down by 1 (far-call `5702:00B1` below 0) |
| any other | far-calls `5702:00B1` |

Every b other than an operator combines its value v with value `si` and the pending operator p of
level `si`, stores the result as value `si`, and clears p. With a as value `si`, the result is:

| p | Result | p | Result |
|---|---|---|---|
| none | v | `0xD8` | 1 if a != v, else 0 |
| `0xD1` | a + v | `0xD9` | 1 if a > v, else 0 |
| `0xD2` | a - v | `0xDA` | 1 if a < v, else 0 |
| `0xD3` | a * v | `0xDB` | 1 if a >= v, else 0 |
| `0xD4` | a / v, signed | `0xDC` | 1 if a <= v, else 0 |
| `0xD5` | 1 if a and v are both not 0, else 0 | `0xDD` | a & v |
| `0xD6` | 1 if a or v is not 0, else 0 | `0xDE` | a \| v |
| `0xD7` | 1 if a == v, else 0 | `0xDF` | a & ~v |

The arithmetic is 32-bit. After an operator or `0xE2`, reading goes on. Otherwise it goes on when
the next byte is from `0xD1` to `0xDF`, or when `si` is above 0 and the next byte is `0xE1`, and
stops otherwise, returning value 0.

A variable byte keeps bit 6 as an extended flag and its low 6 bits as the kind k; `172C:210B`
reads the variable number as one byte, or as two bytes high byte first when the flag is set. For
k:

| k | Value | Address left at `024D` |
|---|---|---|
| 1 | unchanged | the far pointer `032F` plus `42 * number` |
| 2 | the `INT16` at the far pointer `0337` plus `2 * number` | that `INT16` |
| 5 | the `INT32` at the far pointer `0333` plus `4 * number` | that `INT32` |
| 6 | unchanged | the far pointer `033F` plus `42 * number` |
| 7 | the `INT16` at the far pointer `0347` plus `2 * number` | that `INT16` |
| 9 | for a number from `0x20` to `0x2E`, the value at the far pointer stored at `4 * (number - 0x20)`, 32 bits for `0x29` and `0x2A` and a sign-extended `INT16` otherwise; for other numbers 9,999, and it far-calls `5702:00B1` | that far pointer |
| `0x0A` | the `INT32` at the far pointer `0343` plus `4 * number` | that `INT32` |
| `0x0D` | 1 when bit `number % 8` of the byte at the far pointer `034B` plus `number / 8` is set, else 0 | that byte |
| `0x0E` | the same for the far pointer `033B` | that byte |

For k = 1 and 6 the value is left as the kind code; kinds 3, 4 and 8 set neither.

A string (`172C:25D6`) is written into the buffer at the far pointer `0251` according to its first
byte:

- `0x01`: that byte only; the string copied from the far pointer `172C:31B7` returns;
- `0x02`: that byte, then bytes, each passed through `172C:325F`, up to a byte whose result is 3
  or 299 bytes;
- `0x05`: that byte, then 7-bit characters packed high bit first: a shift s starts at 1; while s
  is above 0, a 16-bit buffer is shifted left 8, masked to `0xFF00` and given the next byte; the
  character is `(buffer >> s) & 0x7F`; s goes up by 1 and wraps from 8 to 0. The string ends at
  character 3, or after 299 characters, and a character below `0x20` or above `0x7E` is stored as
  a space;
- any other byte: nothing is read and the buffer keeps what it held.

The string ends with a NUL, and its address is left at `0259`.

`172C:367B(n)` reads n numbers and stores number i at `00B4 + 4 * i` and the address the reader
left at `024D` at `00D4 + 4 * i`. `172C:3669` reads a number and returns the address at `024D`.

## Interpretation

Every parameter of an instruction is an expression, evaluated left to right with no precedence
and with parentheses `0xE2` and `0xE1`. Literals below `0x80` are 15-bit numbers; larger ones
have their own prefixes. Numbers of the name kind are negative (`0x91`). The address a parameter
leaves lets an instruction write to the variable it names, or find the text of a string
parameter. `0x8C` lets an expression use the result of an instruction.

## Alternatives

The kind names (1 local string, 2 local number, 5 local big number, 6 global string, 7 global
number, 9 global name, `0x0A` global big number, `0x0D` global flag, `0x0E` local flag) follow
SRC-LIBGFF-839B11D; the code shows only where each kind lives. `172C:284D`, `172C:31B7` and
`172C:325F` were not read, so `0xB1`, the `0x01` string and the translation of `0x02` strings are
open. What the stack at `0034` saves across `0x8C` beyond the parameters was not traced.

## How to reproduce

Disassemble `172C:3278` with its jump tables at `172C:35A3` (99 words for bytes `0x80` to `0xE2`)
and `172C:3585` (15 words for `0xD1` to `0xDF`), `172C:210B` with its key table at `172C:246A`,
`172C:25D6` to `172C:2805`, and `172C:367B`.

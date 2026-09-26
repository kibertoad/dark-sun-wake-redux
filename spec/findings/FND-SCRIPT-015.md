---
id: FND-SCRIPT-015
title: Script instructions 0x65 and 0x68 add trigger records from the free list to sorted lists
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1B88..172C:1BC2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:1C9D..172C:1CCA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2A8F..172C:2B89
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:2EA1..172C:3026
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:3106..172C:3196
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Records are the 13-byte records at the far pointer `57E0:40C0` (FND-SCRIPT-014); "node n" is
the record at the pointer plus `13 * n`, and its link is the word at offset 11. Parameters are
read with `172C:367B` (FND-SCRIPT-010), p0 to p4 being the low words of the values.

- `172C:314F` returns `0xFFFF` when the word `4C13:032D` is `0xFFFF`; otherwise it takes the
  node n that word names, stores node n's link in `4C13:032D`, and returns n.
- `172C:3106(p)`, for a far pointer p to a list head or a link: when the word at p is `0xFFFF`,
  stores the result of `314F` there; otherwise takes n from `314F` and, when n is not `0xFFFF`,
  stores the old word at p in node n's link and n at p.
- `172C:2EA1(p, record)` is recursive. While the word at p is not `0xFFFF` and word 4 of its
  node is below the record's word 4 (signed), it moves p to that node's link. Then, unless the
  node at p has the same word 4, it calls `3106(p)`. When the word at p is then not `0xFFFF`, it
  copies 7 bytes of the record into that node.
- `172C:2F46(p, record)` does the same with two keys: it moves on while the node's word 4 is
  below the record's word 4 or, failing that, while its word 6 is below the record's word 6
  (both unsigned); it calls `3106(p)` unless the node has the same word 4 and the same word 6;
  and it copies 9 bytes.
- Opcode `0x65` (`172C:1C9D`) reads 3 parameters, and `172C:2B29` stores p0, p1 and p2 at
  `4C13:00FD`, `00FF` and `0101` and 0 at `4C13:0103`, or 1 when the words `4C0E:0012` and
  `4C0E:0014` are 99 and 2. It then calls `2EA1` with the list head `4C10:0011` and those 7
  bytes.
- Opcode `0x68` (`172C:1B88`) reads 5 parameters, builds with `172C:2A8F` the 9 bytes p2, p3,
  p0, p1 (words) and the low byte of p4, and calls `2F46` with the list head `4C10:0005`.

The handlers of opcodes `0x66`, `0x69` to `0x6F` and `0x70` were not read.

## Interpretation

`0x65` and `0x68` register script triggers. A trigger's words 0 and 2 are the entry point
(offset and `GPL ` number) that the walkers of FND-SCRIPT-014 run. A new trigger takes a record
from the free list; a trigger with the same key replaces the one on the list in place. The byte
at offset 6 of a `0x65` trigger is 1 when `MAS ` resource 99, the script run at start
(FND-SCRIPT-013), registered it, and the region change of FND-SCRIPT-013 removes the triggers of
that list whose byte is 0. `0x68` keys its triggers by two words, plausibly a tile position;
`0x65` by one, plausibly the object attacked. The two-key walk moves past a record whose word 4
is above the new one when its word 6 is below, so that list is not kept in the order of the pair.

## Alternatives

By the names in SRC-OPENDS-5C6CBD7, `0x65` is an attack trigger and `0x68` a move-tile trigger;
nothing here shows when the game tests them. The loaded script number and selector at
`4C0E:0012` and `4C0E:0014` are those of FND-SCRIPT-008.

## How to reproduce

Disassemble the ranges above; `1000:0452` is a far copy of `CX` bytes.

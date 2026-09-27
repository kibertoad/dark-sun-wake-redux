---
id: FND-CONFIG-020
title: Sound initialization reads four SOUND.CFG fields and rewrites one conditionally
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:00B0
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident-image ranges
environment: null
---

## Observation

Overlay 180 passes the far pointer returned by the `SOUND.CFG` loader to
`4734:00B0` at `DSUN.EXE+0x0006729C` (FND-CONFIG-019). The receiving
routine at `0x0003C5F0` stores a nonzero pointer at `DS:3411` and calls a
local initialization routine at `0x0003C673`. The bounded opening of that
routine accesses these offsets from the stored pointer:

| Offset | Direct operation |
|---|---|
| `0x14` | Tests bit `0x08`; when set, writes zero to `DS:3444`. Later tests bit `0x02`; when it and the far pointer at `DS:3486` are nonzero, calls that pointer and takes an error branch if it returns zero. |
| `0x32` | When `DS:3417` equals 1, replaces a word value of 1 or 2 with 4 in the loaded buffer. |
| `0x08` | Compares the word to `0x77` and `0x79`. On either match, executes the port writes below. |
| `0x0A` | For those two `0x08` values, uses this word plus 4 and plus 5 as I/O port numbers, writing `0x83` and `0x0B` respectively. |

The routine reads the fields directly from the loaded buffer; these are not
copies parsed by the whole-file loader. The bounded reading stops at
`DSUN.EXE+0x0003C710` and does not describe the rest of initialization.

## Interpretation

The second ten-byte block's first word can supply an I/O base address when
the first block's ID is `0x77` or `0x79`. The word at `0x32` can be changed
in memory during initialization; the installed file value alone does not
describe every later consumer's input.

## Alternatives

The identities of IDs `0x77` and `0x79`, the reason for the port writes,
the roles of `DS:3417`, `DS:3444` and the callback at `DS:3486`, and the
remaining field consumers have not been read. No claim about actual
sound output or a live hardware state follows from this static path.

## How to reproduce

In the approved `DSUN.EXE`, disassemble `0x00067218..0x000672A8`,
`0x0003C5F0..0x0003C64F` and `0x0003C673..0x0003C710` in 16-bit mode.
Follow the pointer store at `0x0003C618` and each branch around the four
field accesses. Keep the later initialization outside this bounded claim.

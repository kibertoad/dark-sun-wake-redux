---
id: FND-PARTY-023
title: The resource-to-slot loader 2D40:000A returns 0xFFFF when either resource lookup it makes fails
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:03B9
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The party loader of FND-PARTY-013 calls `2D40:000A` (canonical target `0x0002260A`, the call at
`DSUN.EXE+0x00068F02`) with the slot, the character number as a 32-bit value, the tag `CHAR` and
7. In the routine those are `[bp+6]`, `[bp+8]`, `[bp+0Ch]` and `[bp+10h]`. It keeps a result word
at `[bp-10h]`, starting at 0, and returns that word in `ax` at `2D40:03B2`.

For a number from 1 to 8,999 or above 13,998, which covers 40 to 43:

1. a slot of 9,999 or more calls `56B2:0034` with the string at `DS:19D7`, which reads
   `Bad iCtrl in gplshell.c`;
2. `38FF:05B5` is called with the tag, the number and the address of a local double word; when
   it returns nonzero, the result is set to `0xFFFF` at `2D40:00B5` and the routine goes to its
   exit code at `2D40:030F`;
3. `444C:00FA` is called with the double word; a null result calls `56B2:0034` with the string at
   `DS:19F1`, `gpldisk out of memory`;
4. `38FF:04AB` is called with the tag, the number and the address of another local double word;
   when it returns nonzero, the result is set to `0xFFFF` and the routine goes to `2D40:030F`.

A number of 0 or less sets the result to `0xFFFE` at `2D40:0035`. The result is also set to
`0xFFFE` at `2D40:02C4`, after the record walk, and to `0xFFFF` at `2D40:035C`, when the slot's
entry in the table at `DS:19C9` has a word at `0x4` of 9,999 or more. The exit code from
`2D40:030F` releases the double word at `[bp-8]` when it is not 0, updates slot tables in segment
`4F49`, calls `56EF:00B6` with the slot when the result is still 0 and the slot is below 9,999,
and stores 9,999 at `DS:157C`.

What `38FF:05B5` and `38FF:04AB` return for a missing archive or a missing record, and which
archive they search, was not read.

## Interpretation

When the resource system cannot find the character record, the loader returns `0xFFFF`, and the
party loader then skips that slot's `PSIN`, `PSST` and `SPST` load and goes on to place the slot
(FND-PARTY-013). Whether a missing `CHARSAVE.GFF` also comes back as a nonzero lookup result, or
ends the program in `56B2:0034` or elsewhere, depends on the two resource-system routines.

## Alternatives

- A failed lookup ends the program inside `2D40:000A`: ruled out for the two lookup results; both
  set `0xFFFF` and return. The fatal-looking calls are on other conditions (a slot of 9,999 or
  more, a null buffer).

## How to reproduce

Disassemble `DSUN.EXE` from `2D40:000A` (file offset `0x0002260A`) to the `retf` at `2D40:03B8`
as 16-bit code with relocations applied, and read the strings at `DS:19D7` and `DS:19F1` (DS is
`57E0`, FND-CONFIG-005).

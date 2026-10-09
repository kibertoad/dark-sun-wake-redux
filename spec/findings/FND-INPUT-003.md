---
id: FND-INPUT-003
title: One routine holds the numbers of ICON 19101 to 19108, and an overlay routine calls it under a byte guard
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B10:0198..2B10:03C9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

A search of every decoded operand for the numbers 19101 to 19110 finds 19101 to 19108 in one
resident routine, which starts after the return at `2B10:0197` and ends at `2B10:03C8`, each
loaded into `SI` (for example 19101 at `2B10:01A3` and 19108 at `2B10:028C`). The routine returns
the chosen number. 19109 and 19110 do not occur in it.

The routine has no direct far caller in the resident image. In the overlay-mapped copy of
`DSUN.EXE` (FMT-EXE-001) it has one caller, in overlay 182 at `DSUN.EXE+0x00068978`; overlay
182's code starts at `DSUN.EXE+0x00068850` and its resident header segment is `56BD`, the second
location above. That caller takes a far pointer to four bytes, returns if it is null, and then
tests the next byte: when that byte is not 0 it calls the routine, and when it is 0 it calls
another shared routine instead, which has nine direct callers in six functions. Neither a function
that holds both halves of the caller's own address nor the encoded far pointer to it was found,
so the caller's own caller is not known.

## Interpretation

The game picks the pointer image for the Walk, Attack and Look modes in one routine, from values
its overlay caller passes. The caller calls it only when a flag byte is set.

## Alternatives

The branches that pick each number test values whose meaning is not recovered, so which condition
selects the valid or invalid image, or the hand-to-hand or ranged one, is not known. The images
`ICON/19109` and `ICON/19110` may be chosen elsewhere or from a computed number.

## How to reproduce

Search the resident image for the immediate operands `0x4A9D` to `0x4AA6`; list the far calls to
the routine; in the overlay-mapped copy described in `docs/GHIDRA.md`, "FBOV mapped image", list
its callers and convert the caller's mapped address with the overlay map reporter.

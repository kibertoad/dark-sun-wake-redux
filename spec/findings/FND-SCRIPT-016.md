---
id: FND-SCRIPT-016
title: Segment 2D40 runs GPL scripts from word pairs of linked 19-byte records whose list head is 57E0:5AF5
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0EF1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0F4A..2D40:0F4F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:205A..2D40:205F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:218A..2D40:218F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:5AF5..57E0:5AF7
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

The three calls of `172C:000C` in segment `2D40` (FND-SCRIPT-013) index records as `index * 19`.
Two pass the words at offsets 8 and 12 of a record, and the third the words at offsets 10 and 14,
with selector 1. The routine at `2D40:0EF1` starts at the list head `57E0:5AF5` (Ghidra's
`5B7C:2135`), follows the signed byte at offset 18, and makes its call only when its own tests
pass.

Two routines keep the list: one puts an unlinked record in front of the head; the other joins
the record's neighbours through the signed bytes at offsets 17 and 18, moves the head when the
record was first, clears the record's 19 bytes and sets both bytes to `-1`. A third routine looks
through indexes 0 to 47 and, under its own tests, replaces the word at offset 0; its two direct
callers are in segment `28C9` and pass it resident words.

## Interpretation

The 19-byte records form a second list with previous and next links, of up to 48 records, each
holding two script entry points: offset 8 or 10 in the `GPL ` resource named by offset 12 or 14.
Overlay 187 converts these pairs to and from `GPLI` entry numbers (FND-SCRIPT-017). What the
records stand for in the game was not found.

## Alternatives

Which of the two calls uses which pair, which tests guard them, and what offset 0 holds were not
recorded. The 48-record bound comes from the updater's loop and may not be the size of the table.

## How to reproduce

In a Ghidra project of `DSUN.EXE`, run `ReportReferences` on `172C:000C` and
`ReportInstructionContext` on the three calls in `2D40`, and `ReportDecompileWindow` on
`2D40:0EF1` and the two list routines; query the references to `5B7C:2135`.

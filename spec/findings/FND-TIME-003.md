---
id: FND-TIME-003
title: The only read of the VGA status port is a word copy that waits for a blank before each word
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0ECC..1000:0F44
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1052
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:267F
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportScalarConstants, ReportInstructionContext, ReportReferences); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

Decoded operands hold `0x3DA` twice. At `1000:0F01` it is loaded into `DX` inside the routine at
`1000:0ECC`; at `2707:03D5` it is a displacement of an indexed far jump, not a port.

The routine at `1000:0ECC` copies a count of words from one far address to another, backwards
when the areas overlap that way. When the byte at `DS:38C4` is 0 it copies them with one
`rep movsw`. Otherwise, for each word, with interrupts off, it reads port `0x3DA` until bit 0 is
0 and then until bit 0 is 1, and moves the word; when the two segments are the same it does this
once before reading the word and again before writing it. It turns interrupts back on after each
word.

It has two near callers, at `1000:1052` and `1000:267F`, in the routines Ghidra starts at
`1000:0FB5` and `1000:265C`; the second chooses this routine or another copy by a word it tests.
The first also calls the BIOS video wrapper at `1000:1136`.

## Interpretation

Bit 0 of port `0x3DA` is set while the display is blanked, so the copy moves one word per blank.
The routine is a screen copy that avoids interference on older adapters, the kind a C runtime's
text-console library uses; with `DS:38C4` at 0 it does not wait at all. It is not a frame clock
and gives the game no duration.

## Alternatives

Whether `DS:38C4` is ever set in this game, and which screen, if any, the two callers draw, were
not read. That the routine belongs to the runtime library of Borland C++, whose copyright text is
at `57E0:0004`, is a reading of its shape and was not confirmed against a library listing.

## How to reproduce

Search decoded operands for `0x3DA`, or the bytes `BA DA 03`. Disassemble `1000:0ECC` and search
the segment `1000` for near calls to it.

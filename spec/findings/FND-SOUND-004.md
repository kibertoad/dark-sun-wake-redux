---
id: FND-SOUND-004
title: SOUND_DS.EXE writes to ports at six sites, three of them timer-chip code the game shares
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:18F0
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:C61E
  - build: BLD-GOG-EN-1.1
    file: SOUND_DS.EXE
    address: 1000:64EA..1000:64F8
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1, after the default analyzer pass (ReportImmediatePortIo, ReportInstructionText, ReportReferences); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7
environment: null
---

## Observation

Addresses are in the load image of `SOUND_DS.EXE` placed at segment `0x1000`.

A search for `MOV DX, value` followed within 16 instructions by `IN` or `OUT DX` finds no such
sequence for the values 513, 544, 556, 816 and 904 (`0x201`, `0x220`, `0x22C`, `0x330`,
`0x388`). The decoded output instructions are six, in three routines:

- At `1000:18F0` a routine writes 0 to port `0x43`, reads port `0x40` twice, low byte then high
  byte, and complements the word.
- At `1000:C61E` a routine writes `0x36` to port `0x43` and then the low and high byte of its word
  argument to port `0x40`. It has three direct callers.
- At `1000:64EA` to `1000:64F8` a routine adds 4 and 5 to a word it keeps at `[bp-2]` and writes
  `0x83` to the first port and `0x0B` to the second. It has one direct caller.

The bytes from the write of `0x36` to the second write to port `0x40` are the same as those of the
game's routine at `DSUN.EXE` `4868:033C` (FND-TIME-007).

## Interpretation

The first two routines read and program channel 0 of the timer chip, as the game does
(FND-TIME-004, FND-TIME-007), and come from code the two programs share, likely the sound
library. The third writes to two ports of a card whose base address is a variable. None of the
six names a file, a sample rate or a duration.

## Alternatives

The callers and the source of the base word were read only as far as their counts, so which card
the third routine sets up, and why the setup program reprograms the timer, are not known. The
five port values cover only immediate forms; ports reached through a variable base are not
excluded.

## How to reproduce

Search the load image of `SOUND_DS.EXE` for `E6 43`, `E6 40` and `EE`, and disassemble the three
routines above.

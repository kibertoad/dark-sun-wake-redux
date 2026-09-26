---
id: FND-TIME-005
title: A resident routine reprograms timer channel 0 to the shortest of up to 17 periods given in microseconds
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4868:033C..4868:0365
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4868:0365..4868:038F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4868:03DA..4868:0464
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4868:05EA..4868:0630
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4868:0C53..4868:0C8A
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportInstructionContext, ReportReferences); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

A search of the load image for the port instructions `E4 40`, `E6 40`, `E4 43` and `E6 43`
finds two coherent `IN AL, 40h`, two `OUT 40h, AL` and two `OUT 43h, AL`; the other byte matches
lie inside unrelated instructions. Two of them are in the counter read of FND-TIME-004. The
others are in the routine at `4868:033C`:

- `4868:033C`, with interrupts off, writes `0x36` to port `0x43`, keeps its word argument at
  `CS:0124`, and writes the argument's low and then high byte to port `0x40`. It has three near
  callers, at `4868:0384`, `4868:061D` and `4868:0C75`.
- `4868:0365` takes a word `period`. When `period` is below 54,925 (`0xD68D`) it passes
  `period * 10000 / 8380`, computed as an unsigned 32-bit product divided by 8,380, to
  `4868:033C`; otherwise it passes 0.
- `4868:03DA`, with interrupts off, visits 17 slots (0 to 16). For each slot whose word at
  `0x6C + 2 * slot` is not 0 it reads a 32-bit period at `CS:00D2 + 4 * slot`, and keeps the
  smallest. When the smallest differs from the 32-bit value at `CS:0116`, it stores it there and
  passes its low word to `4868:0365` (near call at `4868:0451`).
- `4868:061D` passes 0. It is in the routine at `4868:05EA`, which takes a slot number and, when
  the slot's word at `0x6C` is not 0, clears it and counts the word at `CS:0002` down; the call
  is made when that count reaches 0.
- `4868:0C75` passes the value `4868:07AF` returns, when bit 3 of the word at `CS:0E2A` is set and
  the words at `CS:0002` and `CS:0E7A` are 0, and then sets `CS:0E7A` to 1.

A search of the load image for a far call to `4868:0365` or `4868:033C` finds none.

The same bytes as `4868:033C`, from the write of `0x36` to the second write to port `0x40`, are
in `SOUND_DS.EXE` at `1000:C61E` (FND-SOUND-004).

## Interpretation

Writing `0x36` to port `0x43` puts channel 0 of the timer chip, whose output raises the timer
interrupt, into mode 3 with a 16-bit reload value that follows. 8,380 divided by 10,000 is
0.838 µs, the length of one input cycle at 1,193,182 Hz, so `4868:0365` turns a period in
microseconds into a reload value, and a period of 54,925 µs or more, which does not fit, gives 0,
which the chip reads as 65,536: the BIOS rate of 18.2 Hz. `4868:03DA` lets up to 17 clients each
ask for a period and runs the interrupt at the shortest. The same code in the sound setup program
suggests that it belongs to the sound library the two programs share.

## Alternatives

Which clients register periods, what their handlers do and which periods they ask for were not
read, so the interrupt rate while the game runs is not known. That the slot word at `0x6C` marks
a slot in use is read from how `4868:03DA` and `4868:05EA` use it.

## How to reproduce

Search the load image for `E6 43`. Disassemble `4868:033C` to `4868:0464`, `4868:05EA` and
`4868:0C53`, and search the segment `4868` for near calls (`0E E8`) to `0x033C` and `0x0365`.
Ghidra showed `4868:033C` as `4842:059C` and `4868:0C75` as `4926:0095`; they are the same bytes.

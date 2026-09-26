---
id: FND-TIME-004
title: A calibrated millisecond wait reads the timer chip, and five overlays call it with fixed and computed durations
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:12BF..1000:12D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:12D9..1000:12FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:12FA..1000:135B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:39A7..57E0:39AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:3904..57E0:3908
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportInstructionContext, ReportReferences, ReportImmediatePortIo); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

`DS` is `57E0`.

- `1000:12BF`, with interrupts off, writes 0 to port `0x43`, reads port `0x40` twice for the low
  and the high byte, and returns the complement of the 16-bit value.
- `1000:12D9` calls `1000:12BF` up to 100 times. At the first value whose bit 0 is 0 it sets the
  32-bit value at `DS:3904` to 1,193 and stops. `DS:3904` holds 2,386 in the file. A six-byte
  record at `DS:39A7` (`01 10 D9 12 00 00`) holds the far pointer `1000:12D9`.
- `1000:12FA` takes a word `ms`. It reads `1000:12BF` into `last`, and computes a 32-bit target as
  `ms` times the value at `DS:3904`, plus `last`. It then reads `1000:12BF` again and again. It
  returns when the reading, as a 32-bit value, is not below the target. When a reading is below
  `last`, it returns if the target is below `0x10000`, and otherwise takes `0x10000` off the
  target. After each reading it keeps it as `last`.

A search of the load image finds no near or far call to `1000:12BF` outside these two routines,
and none to `1000:12D9` or `1000:12FA`. In the overlay pack, 13 far calls whose fixup word is 0
(descriptor 0, segment `1000`) target offset `0x12FA`. Each pushes one word first:

| Overlay | Call at | Duration passed |
|---|---|---|
| 172 | `DSUN.EXE+0x00059B69` | the word at `DS:26B7` times 100 (the word holds 50 in the file) |
| 187 | `DSUN.EXE+0x000727AE` | 10, inside a loop |
| 187 | `DSUN.EXE+0x0007295C` | 16,000 |
| 201 | `DSUN.EXE+0x00089ED7`, `0x00089FDB`, `0x0008A053`, `0x0008A2E5` | 400 each |
| 204 | `DSUN.EXE+0x0008BEFE` | 50 |
| 204 | `DSUN.EXE+0x0008DA0B`, `0x0008DB4F` | 90 less 10 times a signed byte |
| 206 | `DSUN.EXE+0x0008FC30`, `0x0008FC79`, `0x0008FD23` | a byte parameter of the calling routine |

The overlays' resident headers are at `566A`, `56EF`, `5778`, `5787` and `5799`.

## Interpretation

`1000:12BF` reads how far channel 0 of the timer chip has counted down since its last reload. The
chip's input runs at 1,193,182 Hz. In mode 3, the mode the BIOS sets, channel 0 counts down by 2
per input cycle and every value is even, so the complement is always odd and the multiplier stays
2,386 counts per millisecond; in a mode that counts by 1, an even complement appears and the
multiplier becomes 1,193. `1000:12D9` is run once at startup from the record at `DS:39A7`, and
`1000:12FA` waits `ms` milliseconds. Its shape matches the `delay` function of the Borland C++
runtime.

The game uses the wait for fixed pauses: 400 ms four times in overlay 201, 50 ms in overlay 204,
16 seconds once in overlay 187, and a pause of a hundred times the word at `DS:26B7` in
overlay 172.

## Alternatives

What each calling routine does was not read, so which screen or event each pause belongs to is
open. The word at `DS:26B7` could be the message delay the Preferences screen sets (its
description text is in `DSUN.EXE`), but nothing here shows that. That the record at `DS:39A7` is
one of the runtime's startup records is read from its form and was not traced to the startup
code.

## How to reproduce

Disassemble `1000:12BF` to `1000:135B`. Search the whole file for `9A FA 12 00 00`; each match in
the overlay pack is a far call, and the instruction before it pushes the duration. Search for
`D9 12 00 00` to find the record at `DS:39A7`. The data segment `57E0` starts at file offset
`0x4D000`, so `DS:x` is at file offset `0x4D000 + x`: the record at `0x509A7`, the multiplier at
`0x50904` and the word at `DS:26B7` at `0x4F6B7`.

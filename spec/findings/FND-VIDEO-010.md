---
id: FND-VIDEO-010
title: The cinematic routine runs at startup with cinematic 1 unless a test switch picks another, and from script opcode 0x22 with request 6
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 277B:0024..277B:02CE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:0F84..172C:0FC1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5787:0066..5787:006B
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE for far calls to the cinematic entries
environment: null
---

## Observation

`DS` is the data segment `57E0`. Far calls to `56EF:00FC`, the cinematic routine
(FND-VIDEO-004), are found at `277B:02B8` in the load image and at offset `0x302` of overlay 204,
`DSUN.EXE+0x0008C0C2`. The only far call to `56EF:011A`, the entry of its fallback
(FND-VIDEO-006), is at `277B:02C3`.

The far routine at `277B:0024` takes the argument count and vector (FND-SOUND-010). It sets a
local word to 1 before reading the switches. Three more switches do the following only when the
byte at `DS:143C` is not 0, which `-K 911` sets:

| Switch | Effect |
|---|---|
| `-C` | converts the rest of the argument to a number and keeps it in the local word; a number below 1 or above 5 gives 1 |
| `-O` | converts the rest of the argument to a number and stores it in the word at `DS:13F2` |
| `-Z` | converts the rest of the argument to a number and stores it in the word at `DS:13F4` |

When `DS:143C` is 0, `-O` and `-Z` do nothing and `-C` sets the byte at `3796:0386` to 1.

After the switches, when the bytes at `DS:14E3`, `DS:13F7` and `DS:13F6` are all not 0 it calls
`56EF:00FC` with the local word's low byte, and otherwise `56EF:011A` with it; then it sets the
bytes at `DS:0690` and `DS:13FB` to 0.

The script instruction table (FND-SCRIPT-005) sends opcode `0x22` to `172C:0F84`. That routine
calls `172C:367B` with 4, which reads four numbers into `4C13:00B4` to `4C13:00C3`
(FND-SCRIPT-010), then calls `5787:0066` with the first two as words and the last two as double
words, and stores the result, widened to a double word, at `4C13:031B`, the accumulator
(FND-SCRIPT-009). `5787:0066` leads to offset `0x169` of overlay 204 (header segment `5787`, code
from file offset `0x8BDC0`), which switches on its first argument from 1 to 53 through the table
at offset `0x6C7`. For 6 it calls `56BD:00B6`, then `56EF:00FC` with the low byte of the third
argument, then `56BD:00BB`, then offset `0x1D2E` with 4, and returns 0.

## Interpretation

The game plays cinematic 1, the opening, at startup when digital sound, sound and cinematics are
on, and the fallback slideshow for 1 otherwise. The `-C`, `-O` and `-Z` switches are for testing:
they choose the opening cinematic, the wait before an FLI's first frame, and the time between
frames. Scripts start the other cinematics through opcode `0x22` with 6 as its first parameter and
the cinematic's number as its third.

## Alternatives

`56BD:00B6`, `56BD:00BB` and offset `0x1D2E` of overlay 204 were not read, nor were the other 52
requests of opcode `0x22`. Which scripts use request 6, with which numbers, was not searched, so
when cinematics 2 to 5 play is not shown. Only direct far calls were searched; a call through a
pointer would not be found.

This replaces FND-VIDEO-005, whose location for the trampoline at `5787:0066` ended at
`5787:006A`, covering four of its five bytes (FMT-EXE-004). The documentation check found this
when the `DSUN.EXE` inventory, rebuilt from a single import that sees calls from overlay code,
placed a function whose body includes the whole trampoline. The location now ends at `5787:006B`.
Its other observations are unchanged.

## How to reproduce

Disassemble `277B:0024` to `277B:02CE`, reading the switch table of 26 words at `277B:05A1`;
disassemble `172C:0F84` and overlay 204 from file offset `0x8BF29` to `0x8C487`, resolving far
calls through the segment table at `0x4B080`; search the load image for far calls to `56EF:00FC`
and `56EF:011A` and the overlays for `9A FC 00 D8 05` and `9A 1A 01 D8 05`.

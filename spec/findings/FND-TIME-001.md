---
id: FND-TIME-001
title: DSUN.EXE reads the BIOS time of day at startup and in one word mixer, and nowhere else
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0120..1000:012C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:010C..15F3:012E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1425:111B..1425:1134
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportInstructionContext, ReportReferences); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

A search of the load image for `CD 1A`, the instruction `INT 1Ah`, finds three matches.

- At `1000:0120` the startup code sets `AH` to 0, calls `INT 1Ah` at `1000:0122`, and stores the
  returned `DX` and `CX` at `DS:0096` and `DS:0098`.
- The far routine at `15F3:010C` takes a far pointer to a word. It rotates the word left through
  carry three times, sets `AH` to 0, calls `INT 1Ah` at `15F3:0121`, exclusive-ors the word with
  the returned `DX` and stores it back. Its only far caller, at `1425:111F`, passes `DS:00D2` and
  then copies the word at `DS:00D2` into the word at offset 2 of a 14-byte record of the table at
  `DS:3EEC`.
- The third match, at `5676:0004`, lies in the data after the overlay segment table (FMT-EXE-002),
  inside bytes that do not decode as coherent instructions.

`DS` is `57E0`. A byte search of the whole file finds no `CD 1A` in the overlay pack.

## Interpretation

The game takes the BIOS tick count of the time of day at startup, and uses its low word to mix a
value kept at `DS:00D2`. Neither use schedules anything: no routine here waits for a tick or
counts ticks between frames or moves.

## Alternatives

Other clocks remain possible and are recorded in FND-TIME-003, FND-TIME-004 and FND-TIME-007. What the value at
`DS:00D2` and the 14-byte records are for was not read; the mixer could seed a generator, but
RULE-RNG-001's generator was not found to read `DS:00D2`.

## How to reproduce

Search the load image for `CD 1A`. Disassemble `1000:0120`, `15F3:010C` and the far call at
`1425:111F`, whose target `15F3:010C` has no other far caller.

---
id: FND-TIME-002
title: The four INT 15h calls in DSUN.EXE ask for extended-memory services, not the BIOS wait
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-TIME-006]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:0070..15F3:0075
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 15F3:03A8..15F3:03AD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:0FBC..4AE5:0FC0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4AE5:1246..4AE5:124A
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1 (ReportBytePattern, ReportInstructionContext); re-read with Capstone 5.0.7 16-bit disassembly with Python 3.14.7
environment: null
---

## Observation

A search of the load image for `CD 15`, the instruction `INT 15h`, finds four matches, at
`15F3:0073`, `15F3:03AB`, `4AE5:0FBE` and `4AE5:1248`. The instruction before each sets `AH`:
`AX = 0x8700` before the first, `AX = 0x8800` before the second, `AH = 0x88` before the third and
`AH = 0x87` before the fourth. None sets `AH` to `0x86`.

## Interpretation

`AH = 0x87` copies extended memory and `AH = 0x88` asks its size. The game does not use the BIOS
wait service (`AH = 0x86`) through a literal `INT 15h`.

## Alternatives

A wait through another interrupt, a port or a busy loop is not excluded by this search; see
FND-TIME-003 to FND-TIME-005. Overlay code was not searched in Ghidra. A byte search of the whole
file finds two more `CD 15` pairs in the overlay pack, at `DSUN.EXE+0x0006AE3A` and
`DSUN.EXE+0x0007BFAA`, each after the end of an overlay's code (overlays 182 and 190) among
words that do not decode as coherent instructions.

## How to reproduce

Search the load image for `CD 15` and disassemble the instruction before each match.

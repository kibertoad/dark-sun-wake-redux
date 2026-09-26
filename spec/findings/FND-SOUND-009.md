---
id: FND-SOUND-009
title: The script's sound opcode calls the sound-effect routine and its music opcode calls a routine that does nothing
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0B00..2D40:0B14
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The far routine at `2D40:0B00` pushes the low byte of its word argument, far-calls `2C5F:0A22`
(FND-SOUND-007) and returns. The far routine at `2D40:0B0F` is `PUSH BP`, `MOV BP, SP`, `POP BP`,
`RETF`: it returns at once and ignores its argument.

A search of the load image finds one far call to each: to `2D40:0B00` at load-image address
`0x18A11` and to `2D40:0B0F` at `0x18A1C`, the two handlers of the script opcodes `0x5D` and `0x5F`
that FND-SCRIPT-012 lists.

## Interpretation

A script's sound request plays the effect of that number, cut to its low 8 bits, through the
sound-effect routine. A script's music request has no effect in this build: the routine it calls
was left empty. The music the player hears is chosen elsewhere (FND-SOUND-012).

## Alternatives

None known. The routine at `2D40:0B0F` has no code that could reach another routine.

## How to reproduce

Disassemble `2D40:0B00` to `2D40:0B14` and search the load image for far calls to both entries.

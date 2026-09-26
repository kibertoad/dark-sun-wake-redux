---
id: FND-SCRIPT-011
title: Script instruction 0x52 draws from the game's generator and scales the draw to 0 to n
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 172C:170B..172C:1742
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The handler of opcode `0x52` (FND-SCRIPT-005) at `172C:170B`:

1. far-calls `1000:0822` and sign-extends the 16-bit result to 32 bits;
2. reads a number with `172C:3278` (FND-SCRIPT-010) and adds 1 to it;
3. multiplies the two as signed 32-bit values, divides the product by `0x8000` with a signed
   32-bit division, and sign-extends the low 16 bits of the quotient into the accumulator
   `4C13:031B`.

The call to `1000:0822` comes before the operand is read, and there is no test of the operand.

## Interpretation

`1000:0822` is the generator of RULE-RNG-001 (FND-RNG-001), which gives 0 to 32767. The
instruction sets the accumulator to a value from 0 to n for an operand n of 0 or more, and every
execution makes one draw. This is a direct caller of the generator besides the three FND-RNG-001
names.

## Alternatives

None known.

## How to reproduce

Disassemble `172C:170B`, and search the resident image for far calls to `1000:0822` (`9A 22 08`
followed by the relocated segment).

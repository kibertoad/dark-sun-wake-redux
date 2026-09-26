---
id: FND-PARTY-013
title: A routine in overlay 182 loads CHAR 40 to 43 into party slots 0 to 3
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0000
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine is in FBOV overlay 182 of `DSUN.EXE` (code at `DSUN.EXE+0x00068850`, 9,400 bytes),
which has no `segment:offset` address; the location is the overlay's resident header, `56BD`.

The routine at `DSUN.EXE+0x00068E8D` takes no arguments. It sets a register to 40 (`0x28`) and
runs a loop over a slot index from 0 to 3. In each pass it:

1. works out a pair of position words, from a routine of the same overlay or, in one branch, the
   constants `0x840` and `0x660`;
2. calls a far routine with the slot, the number `40 + slot`, the tag `CHAR` (pushed at
   `DSUN.EXE+0x00068EE7`) and 7, the call that overlay 171 makes to load a stored character
   (FND-PARTY-012);
3. when that call does not return `0xFFFF`, calls a far routine with the slot and `40 + slot`;
4. calls a far routine with the slot, the position words and a value computed from offset `0x10`
   of the slot's 49-byte record in the table the far pointer at `DS:19C9` points to, plus 300;
5. copies byte `0x14` of that record into a per-slot byte table, clears a word in a table of
   19-byte records and sets a byte in a table of 28-byte records to `0xFF`;
6. clears bit 7 of the first byte of the slot's 37-byte record at `DS:67BB + slot * 0x25` when
   the byte at `DS:13FA` is nonzero or the slot equals a word the code compares it with, and sets
   the bit otherwise.

`FND-PARTY-007` found no run 40, 41, 42, 43 in the file: this code computes each number from 40
and the slot.

## Interpretation

This routine puts characters 40, 41, 42 and 43 of the character archive into the four party
slots in that order, with each character's other records (the second call, which has the
arguments of the routine that reads `SPST`, `PSST` and `PSIN`, FND-PARTY-012), a position, and
a flag set on every slot but one, which fits the manual's party that shows only its leader
outside combat. Characters 40 to 43 are the first set on the
disc (FND-PARTY-005), and the four party members of the owner's captures match records 40, 41 or
53, 42, and 33 or 43, in the same order (FND-PARTY-020). This is most likely the party START GAME
supplies.

## Alternatives

What calls the routine has not been traced, so that START GAME is the path that runs it rests on
the match with the captures and the manual (SRC-MANUAL-1994, page 2). Where the second far call
lands has not been resolved; that it is the reader at `DSUN.EXE+0x0006F982` rests on its
arguments. Which archive the loader reads is not shown by this code. Nothing here uses characters
50 to 53, the disc's second set.

## How to reproduce

Map the overlays with `tools/ghidra/ReportFbovOverlayMap.ps1 -SourcePath <DSUN.EXE>`, find the
`66 68 43 48 41 52` push at `DSUN.EXE+0x00068EE7`, and disassemble from the routine's start at
`DSUN.EXE+0x00068E8D` to its `retf` at `DSUN.EXE+0x0006900B`.

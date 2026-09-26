---
id: FND-UI-023
title: Eight far pointers at 5000:AB20 list the character-screen labels, with no direct reference
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5000:AB20..5000:AC20
tool: Ghidra 12.1.3, 16-bit real-mode MZ loader, JDK 21.0.12.1
environment: null
---

## Observation

A byte search of the memory Ghidra loads from `DSUN.EXE` for `VIEW CHARACTER` followed by a NUL
finds it once, at `5000:AB59`. The eight far pointers at `5000:AB20` point, in order, at eight
NUL-terminated labels: `VIEW CHARACTER`, `VIEW INVENTORY`, `CAST SPELL/USE PSIONIC`,
`CURRENT SPELL EFFECTS`, `MEMORIZE SPELLS`, `HIT POINTS: CURRENT/MAX`,
`PSIONIC POINTS: CURRENT/MAX` and `CURRENT STATUS`. The two strings stored just before them are
`GAME MENU` and `RETURN TO GAME`. In the file, `VIEW CHARACTER` and its NUL occur once, at offset
`0x4FD59`.

Ghidra records no direct reference to `5000:AB20` or `5000:AB59`, and no decoded instruction has
`0xAB20` as an operand.

## Interpretation

The executable holds a table of the labels of the screens reached from the Game Menu. No routine
names the table by its address, so which code draws these labels, if any, is not known.

## Alternatives

The screens draw their titles as images (FND-UI-019, FND-UI-020), so these labels may serve
another purpose, such as a description line. The table can be reached through a pointer stored
elsewhere, a segment and offset built at run time, or overlay code.

## How to reproduce

Search the loaded memory for `VIEW CHARACTER` as ASCII with a trailing NUL, read the 256
bytes from `5000:AB20`, and list the references to `5000:AB20` and `5000:AB59` and the instructions
with the operand `0xAB20`.

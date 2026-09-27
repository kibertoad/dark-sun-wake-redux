---
id: FND-UI-034
title: Preferences hover text names message delay above About, and button 16302 opens About
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5782:0000
tool: Ghidra 12.1.3 mapped-overlay MZ import with ReportDecompileWindow and ReportInstructionContext; Capstone 5.0.7 16-bit disassembly of bounded shipped-file ranges
environment: null
---

## Observation

Overlay 203's input dispatcher at `DSUN.EXE+0x0008B32E` compares a clicked
control number against the 13 numbers of `WIND/16500` and jumps through a
parallel table. Its entry for `BUTN/16302`, at window-relative (67, 78), calls
the routine at `DSUN.EXE+0x0008BABD`. That routine draws the nine lines reached
through the About-line pointers in FND-TEXT-005. The entry for `BUTN/16303`,
at (49, 78), instead reaches the animation-state change described by
FND-CONFIG-010.

The same overlay's hover routine at `DSUN.EXE+0x0008B946` subtracts the
window position from the pointer coordinates and chooses a description string.
In the top control band, x below 65 selects `MUSIC TOGGLE ON/OFF` and larger x
selects `MESSAGE DELAY`. In the second band, x below 65 selects `SOUND EFFECTS
ON/OFF` and larger x selects `SOUND EFFECT VOLUME`. In the bottom band, the
successive x bands select `ANIMATIONS ON/OFF`, `ABOUT`, `SPEECH EFFECTS ON/OFF`,
`GAME MENU`, and `RETURN TO GAME`. The difficulty band selects `GAME
DIFFICULTY`. These are short original labels, not a copy of the game's prose.

## Interpretation

`BUTN/16302` is the About control despite its uniformly coloured extracted
icon. The first pair of arrows belongs to Message Delay by the executable's
hover text; the second pair belongs to sound-effect volume. The earlier
assignment of the first pair to music volume came from the manual rather than
the shipped screen code. The hover routine identifies each control's label,
while FND-CONFIG-010 records what clicking it changes.

## Alternatives

This static reading does not establish the exact position, font or native
chrome with which a hover description is displayed. It does not determine
whether a music-volume control exists outside this Preferences window.

## How to reproduce

In the approved `DSUN.EXE` mapped-overlay import, inspect the bounded
decompilation of `9616:05E6` and instruction context at `9616:001E` and
`9616:002E`. Read the 13 control numbers and 13 branch offsets beginning at
shipped file offset `0x0008B735`, then inspect only branches `0x048A` and
`0x0491` relative to overlay 203's code start `0x0008B240`. Inspect the
bounded routine at `968A:001D` and compare its line pointers with
FND-TEXT-005. Keep analysis output outside Git.

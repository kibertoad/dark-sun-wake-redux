---
id: FND-SAVE-010
title: Overlay 190 sends F1 and F2 to Save and Load and F3 to an exit choice
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 571F:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0020..5736:0025
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV overlay 190 key branches, call sites and resident strings; FBOV descriptor inspection
environment: null
---

## Observation

Overlay 190's key table maps BIOS words `0x3B00`, `0x3C00` and `0x3D00`
to its handler offsets `0x17BD`, `0x1807` and `0x1953`, respectively
(FND-COMBAT-025). These target file offsets `0x00079AFD`,
`0x00079B47` and `0x00079C93`.

The `0x3B00` branch tests the combat-state word at `4C10:0019`. If
nonzero, it selects the message `CAN'T SAVE DURING COMBAT` and leaves
this branch (FND-COMBAT-025). Otherwise it reaches a far call to overlay
192's `5736:0025` trampoline at `0x00079D6A`, which targets the Save
Game entry at `0x0007D315` (FND-UI-036). The `0x3C00` branch reaches
`5736:0020` at `0x00079B74`, targeting the Load Game entry at
`0x0007D300`. Both paths can call a far callback held in a stack
parameter before opening the screen.

The `0x3D00` branch reaches a choice call at `0x00079D47`. Its
arguments include the short strings `CANCEL`, `QUIT`, and, when the
combat-state word is zero, `SAVE`. The heading selected before the
call is `EXIT GAME?` when combat-state is nonzero and `EXIT: SAVE GAME?`
otherwise. Return value 1 branches to the Save Game call at
`0x00079D6A`; return value 2 clears `DS:1462`. Other returns do not
directly call Save or Load or clear that byte in this bounded branch.
The Start Game window's Exit to DOS branch also clears `DS:1462`
(FND-UI-035).

## Interpretation

In this key dispatcher, `F1` opens Save Game unless combat blocks it,
`F2` opens Load Game, and `F3` offers an exit choice. Outside combat the
choice offers Save before leaving; the Quit branch clears the same byte
as the Start Game window's Exit to DOS choice. The handler itself does
not immediately write a numbered saved-game file for `F1` or `F3`.

## Alternatives

This is one keyboard consumer; it does not establish identical behavior
in every game state. The indirect callback, choice routine's complete
return convention, and indirect cleanup before the DOS interrupt
(FND-SAVE-012) were not fully read. The Save choice's behavior if somehow returned while combat
is active is not established by the omitted label alone.

## How to reproduce

Resolve overlay 190 through its `571F` header and the key table in
FND-COMBAT-025. Disassemble the three target branches at
`0x00079AFD..0x00079B79` and `0x00079C93..0x00079D84`; read the short
strings at `DS:1EBF`, `DS:1DC2`, `DS:1F61`, `DS:1F5C`, `DS:1F40` and
`DS:1F4B`. Resolve raw far calls `0600:0020` and `0600:0025` through
descriptor 192 to the two Save/Load entries in FND-UI-036.

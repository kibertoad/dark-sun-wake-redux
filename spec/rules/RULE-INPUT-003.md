---
id: RULE-INPUT-003
title: The keys that open the character option screens and the Game Menu
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-006, SCR-UI-008, SCR-UI-009, SCR-UI-010]
---

## Summary

The game is played with the mouse, but some keys work too. `C` or `U` opens the Cast Spells and
Use Psionics screen, `E` the Current Spell Effects screen, `I` the inventory, `V` the View
Character screen, and `Tab` the Game Menu. The other keys of the manual's list act on the game
itself and belong to the rules of the areas they act on.

## When it runs

`screen_for_key` runs when the player presses a key while the map is shown [SRC-MANUAL-1994].

## Parameters

`key`, the character printed on the key the player pressed, as an ASCII code, with letters in
upper case.

## Inputs

None beyond the parameters.

## Procedure

```text
define screen_for_key(key):
    if key == 0x43 or key == 0x55:
        show SCR-UI-009
    else if key == 0x45:
        show SCR-UI-010
    else if key == 0x49:
        show SCR-UI-008
    else if key == 0x56:
        show SCR-UI-002
    else if key == 0x09:
        show SCR-UI-006
```

## Outputs

The screen the key opens, if any.

## Edge cases

A key not in the procedure opens no screen. The manual lists these further keys: `A`, `F4`, `F5` and
`F6` turn animations, music and sound effects on or off (RULE-CONFIG-001); `F1`, `F2` and `F3` save,
load and quit (RULE-SAVE-001); `H` centres the view on the leader; `O` shows the overhead map; `1`
to `4` make that character the leader; `5` shows all characters while moving and `6` the leader
alone; `Y` and `N` answer yes and no questions; `Alt+X` quits; `Esc` leaves every menu, or the game
when no menu is shown; in a conversation `1` to `5` choose a response (SCR-UI-012, RULE-TALK-001); and `G`, `N`,
`P`, `Q`, `W` and `Space` give combat commands (SRC-MANUAL-1994, page 77), of which `G`, `W` and
`Q` are handled as RULE-COMBAT-004 describes.

## What the sources say

The manual's hotkey list gives these keys (SRC-MANUAL-1994, page 77), and says the arrow keys of
the numeric keypad also move the party (page 4). It prints letters in upper case and does not say
whether the lower-case letter or a shift key matters.

## Differences between builds

None known.

## Open questions

- No code that maps a key to a screen is known. Keys reach the game as BIOS key words with the
  shift flags (FND-INPUT-005), several routines test the shift keys (FND-INPUT-006), the game
  does not read the keyboard port directly (FND-INPUT-007), the combat keys are not compared
  together in one function (FND-INPUT-008), and the overlay route above the keyboard routine is
  not a key dispatcher (FND-INPUT-009, Q-INPUT-002).
- A key routine in overlay 190 compares BIOS key words with a table that holds `1` to `4`, `Tab`,
  `Esc`, `=`, `?`, `A`, `G`, `H`, `M`, `Q`, `S`, `T`, `W`, Space, `Alt+X` and `F1` to `F6`, and
  sends other letters, among them `C`, `E`, `I`, `O`, `P`, `U` and `V`, to a second table
  (FND-COMBAT-025). Only its combat keys have been read; what it does with the others, and
  whether it is the routine this rule describes, is open (Q-INPUT-002).
- Whether the keys work on the character option screens themselves, and whether case or shift
  matters (Q-INPUT-001).

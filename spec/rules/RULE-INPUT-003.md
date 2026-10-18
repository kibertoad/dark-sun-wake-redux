---
id: RULE-INPUT-003
title: The keys that open the character option screens and the Game Menu
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-INPUT-012, FND-COMBAT-025, FND-AI-004, FND-UI-046, FND-PARTY-045, FND-PARTY-217, FND-COMBAT-040, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-006, SCR-UI-008, SCR-UI-009, SCR-UI-010, RULE-COMBAT-004]
---

## Summary

The game is played with the mouse, but some keys work too. `C` or `U` opens the Cast Spells and
Use Psionics screen, `E` the Current Spell Effects screen, `I` the inventory, `V` the View
Character screen, and `Tab` the Game Menu. The letters work in either case, and do nothing while
a combatant outside the party has the turn. The other keys of the manual's list act on the game
itself and belong to the rules of the areas they act on.

## When it runs

`screen_for_key` runs when the player presses a key that the map's own key routine does not
take, while the map is shown and in its usual mode, and when the handler of the Game Menu or of
one of the four character option screens passes a key on [FND-INPUT-012, FND-AI-004].

## Parameters

`key_word`, the BIOS keyboard word of the key: the scan code in the high byte and the character
in the low byte, so `0x2E63` for `c`, `0x2E43` for `C` and `0x0F09` for `Tab`.

## Inputs

`turn_combatant`.

## Procedure

```text
define screen_for_key(key_word):
    if key_word == 0x0F09:
        show SCR-UI-006
        return
    let letter = (key_word & 0xFF) | 0x20
    if letter != 0x63 and letter != 0x65 and letter != 0x69 and letter != 0x75 and letter != 0x76:
        return
    if turn_combatant >= 4:
        return
    if letter == 0x63 or letter == 0x75:
        show SCR-UI-009
    else if letter == 0x65:
        show SCR-UI-010
    else if letter == 0x69:
        show SCR-UI-008
    else:
        show SCR-UI-002
```

## Outputs

The screen the key opens, if any.

## Edge cases

A key not in the procedure opens no screen. `Ctrl` and `Alt` with a letter give a key word whose
low byte is not that letter, so they open nothing here. When one of the four character option
screens is shown, the key closes it and opens the new screen for the character it showed, and
marks the navigation button of the new screen; pressing the key of the screen shown opens it
again [FND-INPUT-012]. Tab opens the Game Menu whoever has the turn.

The manual lists these further keys: `A`, `F4`, `F5` and `F6` turn animations, music and sound
effects on or off (RULE-CONFIG-001); `F1`, `F2` and `F3` save, load and quit (RULE-SAVE-001); `H`
centres the view on the leader; `O` shows the overhead map (FND-UI-046); `1` to `4` make that
character the leader; `5` shows all characters while moving and `6` the leader alone; `Y` and `N`
answer yes and no questions; `Alt+X` quits; `Esc` leaves every menu, or the game when no menu is
shown; in a conversation `1` to `5` choose a response (SCR-UI-012, RULE-TALK-001); and `G`, `N`,
`P`, `Q`, `W` and `Space` give combat commands (SRC-MANUAL-1994, page 77), of which `G`, `W` and
`Q` are handled as RULE-COMBAT-004 describes.

## What the sources say

The manual's hotkey list gives these keys (SRC-MANUAL-1994, page 77), and says the arrow keys of
the numeric keypad also move the party (page 4). It prints letters in upper case and does not say
whether the lower-case letter or a shift key matters; the code takes both cases (FND-INPUT-012).

## Differences between builds

None known.

## Open questions

- That `C` and `U` open the spell screen and `E` the effect screen rests on the navigation
  button each key marks and on the Game Menu's button routes, whose meanings come from the
  manual (FND-INPUT-012); a capture of each key would confirm it (Q-INPUT-001).
- What the word `DS:0DAB`, which must be 1 for the map to pass the key on, holds in each game
  mode, and so in which modes the keys work (Q-COMBAT-003).

---
id: RULE-SAVE-001
title: The keys that save, load and quit
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-013, SCR-UI-014]
---

## Summary

`F1` saves the game, `F2` loads a saved game and `F3` quits the game.

## When it runs

When the player presses a key while the map is shown (SRC-MANUAL-1994, page 77).

## Parameters

`scan_code`, the scan code of the key the player pressed, as the PC keyboard reports it: `0x3B`
for `F1`, `0x3C` for `F2` and `0x3D` for `F3`.

## Inputs

None beyond the parameters.

## Procedure

```text
define save_load_key(scan_code):
    if scan_code == 0x3B:
        show SCR-UI-014
    else if scan_code == 0x3C:
        show SCR-UI-013
    else if scan_code == 0x3D:
        quit_requested = true
```

## Outputs

The Save Game or Load Game screen, or a request to leave the game.

## Edge cases

The manual says only that `F1` saves the game and `F2` loads one; the procedure opens the screens
the Game Menu's LOAD and SAVE choices open, which is a guess. Whether `F3` offers to save first, as
Exit to DOS on the Game Menu does (SCR-UI-006), is not known.

## What the sources say

SRC-MANUAL-1994, page 77, hotkeys: `F1` saves game, `F2` loads game, `F3` quits game. It also
lists `Alt+X`, which quits the game, and `Esc`, which leaves every menu, or the game when no menu
is shown.

## Differences between builds

None known.

## Open questions

- Whether `F1` and `F2` open the screens or save and load at once, whether `F3` asks first, and
  where the game keeps `quit_requested` (Q-SAVE-001).

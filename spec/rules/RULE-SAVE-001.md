---
id: RULE-SAVE-001
title: The F1 and F2 screens and F3 exit choice
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-SAVE-010, FND-SAVE-012, FND-UI-035, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-013, SCR-UI-014]
---

## Summary

In overlay 190's key dispatcher, `F1` opens Save Game when combat is
inactive and reports that saving is refused during combat. `F2` opens
Load Game. `F3` opens an exit choice; when combat is inactive, that
choice offers Save, Quit and Cancel (FND-SAVE-010).

## When it runs

When overlay 190 handles a key word (FND-COMBAT-025).

## Parameters

`key_word`, the BIOS keyboard word: `0x3B00` for `F1`, `0x3C00` for
`F2` and `0x3D00` for `F3` (FND-COMBAT-025).

## Inputs

The combat-state word at `4C10:0019` and the exit-control byte at
`DS:1462`.

## Procedure

```text
define save_load_key(key_word):
    if key_word == 0x3B00:
        if combat_state != 0:
            show "CAN'T SAVE DURING COMBAT"
        else:
            show SCR-UI-014
    else if key_word == 0x3C00:
        show SCR-UI-013
    else if key_word == 0x3D00:
        show exit choice, with "SAVE" offered when combat_state == 0
        if choice == 1:
            show SCR-UI-014
        else if choice == 2:
            exit_control_byte = 0
```

## Outputs

The Save Game or Load Game screen, a combat refusal message, or an exit
choice whose Quit branch clears `DS:1462`. That zero value makes the
resident main loop return at its next continuation check; the startup
path then submits a DOS terminate request (FND-SAVE-012).

## Edge cases

The key consumer can invoke an indirect callback before opening a
Save/Load screen; its additional effects are not fully read. The exit
choice omits its `SAVE` label when the combat-state word is nonzero
(FND-SAVE-010).

## What the sources say

SRC-MANUAL-1994, page 77, hotkeys: `F1` saves game, `F2` loads game, `F3` quits game. It also
lists `Alt+X`, which quits the game, and `Esc`, which leaves every menu, or the game when no menu
is shown.

## Differences between builds

None known.

## Open questions

- Whether other key consumers act differently, the full effects of the
  indirect callback and cleanup effects before the DOS interrupt
  (FND-SAVE-010, FND-SAVE-012, Q-SAVE-001).

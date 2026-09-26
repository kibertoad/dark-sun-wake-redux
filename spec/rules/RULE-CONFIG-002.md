---
id: RULE-CONFIG-002
title: The difficulty setting
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-GAMEFAQS-81038, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

The difficulty sets how hard combat is. It has four settings, in order Easy, Balanced, Hard and
Hideous, which the Preferences screen steps through with a button at each end. A new game begins
on Balanced.

## When it runs

`easier_difficulty` and `harder_difficulty` run when the player clicks the difficulty buttons of
the Preferences screen (SCR-UI-007, SRC-MANUAL-1994, page 15).

## Parameters

None.

## Inputs

`difficulty`.

## Procedure

```text
define easier_difficulty():
    difficulty = max(difficulty - 1, 0)

define harder_difficulty():
    difficulty = min(difficulty + 1, 3)
```

## Outputs

The new `difficulty`, which the screen shows by the label of that index in the executable's list
(FMT-TEXT-004).

## Edge cases

Whether the setting stops at Easy and Hideous, as the procedure has it, or wraps round, is not
known.

## What the sources say

SRC-MANUAL-1994, page 15: difficulty controls the level of difficulty in combat; the settings are
Easy, Balanced, Hard and Hideous; the default is Average. No setting of that name exists, and the
executable's list holds only the four (FMT-TEXT-004). SRC-GAMEFAQS-81038, section 2.8, says a new
game begins on Balanced, and reports that Easy gives hostile creatures about half the hit points
of Balanced and Hideous about twice, applied when a creature appears; that report is kept in
RULE-COMBAT-007.

## Differences between builds

None known.

## Open questions

- The game saves its difficulty-label index at `PREF/100` offset `0x00`
  (FMT-CONFIG-003, FND-CONFIG-009). Whether the buttons stop or wrap, and what
  value a new game starts with, remain open (Q-CONFIG-001, Q-CONFIG-002).
- What the difficulty changes in combat (RULE-COMBAT-007, Q-COMBAT-007).

---
id: RULE-CONFIG-002
title: The difficulty setting
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-CONFIG-009, FND-CONFIG-010, FND-TEXT-005, SRC-GAMEFAQS-81038, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

The Preferences screen holds a difficulty index from 0 to 3, labelled Easy, Balanced, Hard and
Hideous in that order. Its two arrow buttons move the index one step and stop at the ends.

## When it runs

`easier_difficulty` and `harder_difficulty` run when the player clicks buttons `16309` and
`16308`, respectively, on the Preferences screen (SCR-UI-007, FND-CONFIG-010).

## Parameters

None.

## Inputs

`difficulty_index`, a signed 16-bit word saved at `PREF/100` offset `0x00`
(FMT-CONFIG-003).

## Procedure

```text
define easier_difficulty():
    difficulty_index = (difficulty_index - 1) modulo 65536
    if difficulty_index interpreted as signed is above 3: difficulty_index = 3
    if difficulty_index interpreted as signed is below 0: difficulty_index = 0

define harder_difficulty():
    difficulty_index = (difficulty_index + 1) modulo 65536
    if difficulty_index interpreted as signed is above 3: difficulty_index = 3
    if difficulty_index interpreted as signed is below 0: difficulty_index = 0
```

## Outputs

The new `difficulty_index`, which the screen uses to choose one of the four labels
(FMT-TEXT-004, FND-CONFIG-009).

## Edge cases

For ordinary values 0 through 3, the arrows stop at Easy and Hideous. The signed comparisons
also clamp out-of-range loaded words after each click (FND-CONFIG-010).

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

- What value a new game starts with: the manual calls it Average while the FAQ
  says Balanced. The loaded-image value zero and the guarded overlay 171
  write of 3 do not establish the executable's new-game initialization.
  FND-CONFIG-016 finds no second instruction-embedded `PREF` tag beyond
  save/load, but its resident data occurrence and indirect paths are not
  traced (FND-CONFIG-009, FND-CONFIG-015, Q-CONFIG-001, Q-CONFIG-002).
- What the difficulty changes in combat (RULE-COMBAT-007, Q-COMBAT-007).

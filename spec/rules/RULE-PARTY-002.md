---
id: RULE-PARTY-002
title: What a new character may be
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, SRC-README-1.1]
conflicting: []
split_with: []
related: [RULE-PARTY-007, RULE-PARTY-009, SCR-UI-004]
---

## Summary

A new character has an origin, a gender, an alignment that is good or neutral, six ability
scores from 9 to 24, and one to three classes. A human has one class; other origins may combine
up to three in the combinations the generation screen offers (RULE-PARTY-009). Each class needs minimum ability scores, and each
origin allows only some classes. Muls are always male and thri-kreen always female.

## When it runs

`character_allowed` runs when the player clicks DONE on the character generation screen
(SCR-UI-004), and `class_allowed` for each class the player considers there (SRC-MANUAL-1994,
pages 7 to 9).

## Parameters

`origin`, the character's `origin` code, 0 to 7. `gender`, 0 for male and 1 for female, in the
order of the executable's labels (FND-PARTY-017). `alignment`, the character's `alignment` code,
0 to 8. `classes`, a list of one or more `character_class` codes. `character_class`, one such
code. `scores`, the six ability scores in the order strength, dexterity, constitution,
intelligence, wisdom, charisma, as FMT-PARTY-001 stores them.

## Inputs

None beyond the parameters.

## Procedure

```text
define class_allowed(origin, character_class, scores):
    # Minimum scores, six per class in the order of scores
    let minimum: UINT8[48] = [0, 0, 0, 0, 9, 0, 0, 0, 0, 0, 12, 15, 9, 0, 0, 0, 0, 0, 13, 12, 15, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 0, 11, 12, 15, 0, 13, 13, 14, 0, 14, 0, 0, 9, 0, 0, 0, 0]
    for ability in 0..6:
        if scores[ability] < minimum[character_class * 6 + ability]:
            return false
    return class_level_limit(origin, character_class) != 0

define character_allowed(origin, gender, alignment, classes, scores):
    for ability in 0..6:
        if scores[ability] < 9 or scores[ability] > 24:
            return false
    if origin == 6 and gender != 0:
        return false
    if origin == 7 and gender != 1:
        return false
    if alignment == 2 or alignment == 5 or alignment == 8:
        return false
    let n = count(classes)
    if n < 1 or n > 3:
        return false
    if origin == 0 and n != 1:
        return false
    for i in 0..n:
        if not class_allowed(origin, classes[i], scores):
            return false
        # classes_offered never offers a class already in its list
        if (classes_offered(origin, classes[0..i]) >> (7 - classes[i])) & 1 == 0:
            return false
    return true
```

## Outputs

Each function returns true or false and changes no state.

## Edge cases

The class codes are cleric 0, druid 1, fighter 2, gladiator 3, preserver 4, psionicist 5, ranger 6
and thief 7; the origin codes human 0, dwarf 1, elf 2, half-elf 3, half-giant 4, halfling 5, mul 6
and thri-kreen 7; the alignment codes lawful good 0, lawful neutral 1, lawful evil 2, neutral good
3, true neutral 4, neutral evil 5, chaotic good 6, chaotic neutral 7 and chaotic evil 8. They are
the positions of the executable's labels (FND-PARTY-016, FND-PARTY-017), which is not shown to be
how the game stores them; the generation screen numbers the classes 1 to 8 in the same order
(FND-PARTY-063). Which classes may go together, and in which order, comes from RULE-PARTY-009's
`classes_offered`, whose table forbids cleric with druid as the manual does and many other
combinations besides; `classes` is in the order the screen keeps them. `class_level_limit`
(RULE-PARTY-007) is nonzero exactly for the classes `classes_offered` gives an origin with no
class.

Scores are checked after origin modifiers (RULE-PARTY-005) only if the game applies them before
DONE, which is not known.

## What the sources say

SRC-MANUAL-1994:

- Page 7: muls are male only and thri-kreen female only.
- Page 8: a new character starts as a fighter; humans take one class, other origins up to three,
  and cleric and druid cannot be combined.
- Page 16: ability scores run from 9 to 24.
- Pages 19 to 22: the minimum scores of each class, as the procedure's list gives them: cleric
  wisdom 9; druid wisdom 12 and charisma 15; fighter strength 9; gladiator strength 13, dexterity
  12 and constitution 15; preserver intelligence 9; psionicist constitution 11, intelligence 12
  and wisdom 15; ranger strength 13, dexterity 13, constitution 14 and wisdom 14; thief dexterity
  9.
- Page 22: a player character's alignment must be good or neutral.

SRC-README-1.1, table 3, gives which classes each origin may take (RULE-PARTY-007).

## Differences between builds

None known.

## Open questions

- Whether the generation screen enforces the scores, genders, alignment and class minimums by
  refusing DONE, by greying out choices, or not at all (it greys out the classes RULE-PARTY-009
  does not offer, FND-PARTY-068), and whether the minimums apply to scores before or after origin modifiers
  (Q-PARTY-002).
- How the six scores are rolled and what editing them allows (SRC-MANUAL-1994, page 9;
  Q-PARTY-002).

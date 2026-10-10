---
id: RULE-PARTY-002
title: What a new character may be
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, FND-PARTY-073, FND-PARTY-066, FND-PARTY-077, FND-PARTY-068, FND-PARTY-070, FND-PARTY-071, FND-PARTY-072]
conflicting: []
split_with: []
related: [RULE-PARTY-003, RULE-PARTY-005, RULE-PARTY-007, RULE-PARTY-009, RULE-PARTY-010, RULE-PARTY-011, RULE-PARTY-012, SCR-UI-004]
---

## Summary

A new character takes its origin and gender from one of 14 portraits: male and female for the
first six origins, a male mul and a female thri-kreen. It has one to three classes in a
combination the generation screen offers, an alignment all its classes allow, and six scores
within the bounds its classes and origin set. DONE can be pressed only when the character has a
class and a discipline, a sphere if it is a cleric or druid, and all three disciplines if it is a
psionicist.

## When it runs

`character_allowed` describes what DONE on the character generation screen can store
(SCR-UI-004), and `done_enabled` runs whenever a class, sphere or discipline changes
(FND-PARTY-070).

## Parameters

`portrait`, the portrait number, 0 to 13. `origin`, the character's `origin` code, 0 to 7.
`gender`, 0 for male and 1 for female, in the order of the executable's labels (FND-PARTY-017).
`alignment`, the character's `alignment` code, 0 to 8. `classes`, a list of zero to three
`character_class` codes in the order the screen keeps them. `scores`, the six ability scores in
the order strength, dexterity, constitution, intelligence, wisdom, charisma, as FMT-PARTY-001
stores them. `disciplines`, the number of psionic disciplines chosen, 0 to 3, and `sphere_chosen`,
true when an elemental sphere is chosen (RULE-PARTY-003).

## Inputs

None beyond the parameters.

## Procedure

```text
define portrait_origin(portrait):
    if portrait == 13:
        return 7
    return portrait / 2

define portrait_gender(portrait):
    if portrait == 12:
        return 0
    if portrait == 13:
        return 1
    return portrait % 2

define done_enabled(classes, disciplines, sphere_chosen):
    if count(classes) == 0 or disciplines == 0:
        return false
    for each c in classes:
        if (c == 0 or c == 1) and not sphere_chosen:
            return false
        if c == 5 and disciplines != 3:
            return false
    return true

define character_allowed(origin, gender, alignment, classes, scores, disciplines, sphere_chosen):
    let has_portrait = false
    for portrait in 0..14:
        if portrait_origin(portrait) == origin and portrait_gender(portrait) == gender:
            has_portrait = true
    if not has_portrait:
        return false
    if not done_enabled(classes, disciplines, sphere_chosen):
        return false
    let n = count(classes)
    for i in 0..n:
        # classes_offered never offers a class already in its list
        if (classes_offered(origin, classes[0..i]) >> (7 - classes[i])) & 1 == 0:
            return false
        if not alignment_allowed(classes[i], alignment):
            return false
    for ability in 0..6:
        if scores[ability] < score_minimum(classes, ability):
            return false
        if scores[ability] > score_maximum(origin, ability):
            return false
    return true
```

## Outputs

`portrait_origin` and `portrait_gender` return the codes a portrait sets. `done_enabled` returns
whether DONE can be pressed, and `character_allowed` whether the screen can store the character.
None changes state.

## Edge cases

The class codes are cleric 0, druid 1, fighter 2, gladiator 3, preserver 4, psionicist 5, ranger 6
and thief 7; the origin codes human 0, dwarf 1, elf 2, half-elf 3, half-giant 4, halfling 5, mul 6
and thri-kreen 7; the alignment codes lawful good 0, lawful neutral 1, lawful evil 2, neutral good
3, true neutral 4, neutral evil 5, chaotic good 6, chaotic neutral 7 and chaotic evil 8. They are
the positions of the executable's labels (FND-PARTY-016, FND-PARTY-017), which is not shown to be
how the game stores them; the generation screen numbers the classes 1 to 8 in the same order
(FND-PARTY-073). The screen stores origin, gender and alignment counting from 1 (FND-PARTY-070).

DONE checks only the classes, the disciplines and the sphere; the screen keeps the rest within
these limits as the player chooses (RULE-PARTY-009, RULE-PARTY-010, RULE-PARTY-011), so it never
refuses a character for them. Choosing a portrait remakes the character as a true neutral fighter
with rolled scores. A human has one class, a gladiator no other class, and no character cleric
with druid, all through `classes_offered`. `class_level_limit` (RULE-PARTY-007) is nonzero exactly
for the classes `classes_offered` gives an origin with no class. The scores are bounded after the
origin modifiers (RULE-PARTY-005), and every bound lies within 9 to 24.

The screen holds the hit points between the bounds RULE-PARTY-012 gives, which DONE does not
check either.

Holding Ctrl while stepping the alignment lets a character reach DONE with an alignment its
classes forbid (RULE-PARTY-011), which `character_allowed` does not cover. Rangers and druids
choose a sphere too, but DONE does not require it for a ranger.

## What the sources say

SRC-MANUAL-1994:

- Page 7: muls are male only and thri-kreen female only.
- Page 8: a new character starts as a fighter; humans take one class, other origins up to three,
  and cleric and druid cannot be combined.
- Page 16: ability scores run from 9 to 24.
- Pages 19 to 22: the minimum scores of each class: cleric wisdom 9; druid wisdom 12 and charisma
  15; fighter strength 9; gladiator strength 13, dexterity 12 and constitution 15; preserver
  intelligence 9; psionicist constitution 11, intelligence 12 and wisdom 15; ranger strength 13,
  dexterity 13, constitution 14 and wisdom 14; thief dexterity 9. The game's minimums are higher
  (RULE-PARTY-010).
- Page 22: a player character's alignment must be good or neutral.

SRC-README-1.1, table 3, gives which classes each origin may take (RULE-PARTY-007).

## Differences between builds

None known.

## Open questions

None.

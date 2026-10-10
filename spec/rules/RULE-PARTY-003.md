---
id: RULE-PARTY-003
title: Which psionic disciplines and elemental sphere a new character chooses
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, SRC-README-1.1]
conflicting: []
split_with: []
related: [SCR-UI-004, SCR-UI-005]
---

## Summary

A psionicist has all three psionic disciplines, psychokinesis, psychometabolism and telepathy;
every other character chooses one, and psychokinesis is chosen at first. A cleric chooses one
elemental sphere, air, earth, fire or water, and air is chosen at first. Druids and rangers have
an elemental sphere too.

## When it runs

On the character generation screen, when the class list changes and when the player opens the
discipline or sphere list (SCR-UI-004, SCR-UI-005).

## Parameters

`classes`, the list of the character's `character_class` codes (RULE-PARTY-002).

## Inputs

None beyond the parameters.

## Procedure

```text
define disciplines_to_choose(classes):
    for each c in classes:
        if c == 5:
            return 3
    return 1

define chooses_sphere(classes):
    for each c in classes:
        if c == 0 or c == 1 or c == 6:
            return true
    return false
```

## Outputs

`disciplines_to_choose` returns 3 when the psionicist has all three disciplines and 1 when the
player picks one. `chooses_sphere` returns true when the character has a sphere. Neither changes
state.

## Edge cases

The discipline codes follow the manual's order, psychokinesis 0, psychometabolism 1 and telepathy
2, and the sphere codes air 0, earth 1, fire 2 and water 3; the first choice is code 0 in both. A
multi-class psionicist has all three disciplines.

## What the sources say

SRC-MANUAL-1994, pages 8 and 9, says psionicists have all three disciplines and every other
character picks one, psychokinesis by default, and that clerics pick one of the four elemental
spheres, air by default. It names only clerics there, but its class descriptions (pages 20 and
21) give druids a sphere of their element and rangers an elemental sphere chosen at creation.
SRC-README-1.1 says the View Character screen shows a druid's or cleric's sphere as the colour of
its level, red for fire.

## Differences between builds

None known.

## Open questions

- The sphere part is read in the code: the Cleric, Druid and Ranger buttons of the generation
  screen open a window of four radio buttons, air, earth, fire and water, with air chosen, and the
  stored class code holds the choice (FND-PARTY-063, FND-PARTY-066). The discipline part, and what
  the screen shows for a character with neither, are not read (Q-PARTY-002).
- Whether the choice of discipline is stored as a bit of the character's `PSIN` byte
  (FMT-PARTY-003, Q-PARTY-004).

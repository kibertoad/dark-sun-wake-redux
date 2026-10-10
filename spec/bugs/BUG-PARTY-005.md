---
id: BUG-PARTY-005
title: A dwarf or halfling gets its constitution bonus on all five saves of a warrior class and on none of its other classes' saves
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-085, FND-PARTY-099]
conflicting: []
split_with: []
related: [RULE-PARTY-014]
---

## Symptom

For a dwarf or halfling with a fighter, gladiator or ranger class, every save the warrior group
gives is lowered by twice the constitution over 7. A dwarf or halfling cleric, druid, preserver,
psionicist or thief gets no constitution bonus on any save.

## Trigger conditions

A dwarf or halfling whose saves are set: when it is stored from the generation screen, when it
gains a level in play, or at a class change (FND-PARTY-085). The bonus changes a save only where
the warrior group gives the best value for it, and it matters when the character is the target of
an effect that allows a saving throw, which compares a d20 roll with the save (FND-PARTY-099).

## Mechanism

Overlay 210 `+0572` loops over the four class groups and, inside, over the five saves. It adds the
bonus when the group number is 1 or 4 and the origin is dwarf or halfling (`+0624..+066E`,
FND-PARTY-085). No group has the number 4, while 1 and 4 are both save numbers, so the test reads
as one meant for the save number that took the group number.

## Frequency

Every time the saves of a dwarf or halfling are set.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the test is a slip for the save number: the group number 4, which cannot occur, favours
  one, but the code cannot show intent and no source discusses the saves (No item: the code cannot
  show intent).

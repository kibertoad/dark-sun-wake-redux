---
id: BUG-PARTY-009
title: A dual-classed human keeps a held-back former class's power level, thief skills, resting actions, hit escape and CHAOS warrior test
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-169, FND-PARTY-164, FND-PARTY-258]
conflicting: []
split_with: []
related: [RULE-PARTY-004]
---

## Symptom

A human who changed class still gets some of the former class's effects at the former class's
level, although the game withholds its spells, its class flag and its level in the class level
query until the new class's level is higher.

## Trigger conditions

A party member whose origin is human, with a former class at a position other than 0 whose level
is not below the current class's level, where the former class is one of these
(FND-PARTY-169, FND-PARTY-164):

- psionicist: the level of each power it learned, which stays on its power list;
- thief: the thief skills;
- cleric, druid or preserver: being the party's best of that class when the party rests;
- cleric of code 3 at level 5 or more: the escape from a hit whose mask has bit 2 set;
- fighter, gladiator or ranger: counting as a warrior against `CHAOS`.

## Mechanism

These uses ask overlay 176 `+076C` for the class's level, and `+076C` returns the level of any
position holding the class with no test of the origin (FND-PARTY-169). The class flags,
`27E5:00C1` and overlay 177's spell routines leave such a former class out for a human
(FND-PARTY-258). The game applies the hold-back in some routines and not in others, so whether
these uses were meant to escape it is unclear; the psionicist level that `+076C` returns as 1 when
no class holds it fits a design where every character has some psionic use. The power list
tests only a power's rank and the thief skill checks no class, so the former class's uses are
reached in play; the psionic points in play hold the class back (FND-PARTY-169).

## Frequency

Every use listed above by such a human, until the new class passes the former level. The change
needs a level of 2 or more (RULE-PARTY-004).

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None.

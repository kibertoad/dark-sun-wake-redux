---
id: RULE-MAGIC-004
title: Camping restores spells and psionic strength points
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-MAGIC-001]
---

## Summary

The party rests by clicking the Look pointer on a fire ring. Resting restores every character's
psionic strength points and every spell the party's casters can cast, and while the party
rests, characters with cure spells cast them on wounded characters.

## When it runs

When the player clicks the Look pointer on a fire ring during exploration.

## Parameters

`members`, the party's characters. For each, `psionic_strength_points` and
`maximum_psionic_strength_points` are the character's current and full PSPs, and
`spells_left[spell_level]` is the number of spells of that level the character can still cast
before resting, for spell levels 1 to 7. `spell_slots[spell_level]` is the number of spells
of that level the character's classes and levels give (RULE-MAGIC-001).

## Inputs

None beyond the parameters.

## Procedure

```text
define camp(members):
    for each member in members:
        member.psionic_strength_points = member.maximum_psionic_strength_points
        for spell_level in [1, 2, 3, 4, 5, 6, 7]:
            member.spells_left[spell_level] = member.spell_slots[spell_level]
```

## Outputs

Every character's PSPs and spells are full. The procedure leaves out the cure spells cast during
the rest and any hit points they restore.

## Edge cases

A character who casts no spells has 0 spells of every level after the rest. How the spells of a
character who casts in more than one class are counted is open (RULE-MAGIC-001).

## What the sources say

SRC-MANUAL-1994, page 6, says camping lets the party rest, which is needed to recover from
battles and to regain spells and psionic points; that safe places to rest are marked by a fire
ring, on which the player clicks the Look pointer; that as the party rests, characters with cure
spells cast them on wounded characters; and that PSPs are fully restored, as are all the spells
the spellcasters can cast. Page 13 says the number of spells a character can cast before resting
depends on level (RULE-MAGIC-001).

## Differences between builds

None known.

## Open questions

- Whether resting takes game time, restores hit points on its own, can be interrupted, or is
  refused while enemies are near, and in which order the cure spells are cast (Q-MAGIC-005).
- How a preserver chooses which spells to cast again, if the game keeps memorized spells at all
  (Q-MAGIC-005).

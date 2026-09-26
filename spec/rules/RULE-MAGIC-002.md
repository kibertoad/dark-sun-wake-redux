---
id: RULE-MAGIC-002
title: Which spheres of cleric spells a priest may cast
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-003]
---

## Summary

Every cleric spell belongs to the Sphere of the Cosmos or to one or more of the four elemental
spheres.
A cleric may cast every spell of the sphere of his or her element, and Cosmos spells of level 3
or lower. A druid may cast every spell of the Cosmos and of his or her element. A ranger of
eighth level or higher may cast the spells of his or her element, up to level 3.

## When it runs

Whenever the game decides which cleric spells a character may learn or cast, for example to fill
the Cast Spells / Use PSI screen (SCR-UI-009).

## Parameters

`character_class`, the casting class's `character_class` code (RULE-PARTY-002). `level`, the
character's level in that class. `sphere`, the character's elemental sphere code, 0 air, 1 earth,
2 fire and 3 water (RULE-PARTY-003). `spell_sphere`, the sphere of the spell: 0 to 3 as for
`sphere`, 4 the Sphere of the Cosmos. `spell_level`, the spell's level, 1 to 7.

## Inputs

None beyond the parameters.

## Procedure

```text
define may_cast_cleric_spell(character_class, level, sphere, spell_sphere, spell_level):
    if character_class == 0:
        if spell_sphere == sphere:
            return true
        return spell_sphere == 4 and spell_level <= 3
    if character_class == 1:
        return spell_sphere == sphere or spell_sphere == 4
    if character_class == 6:
        return level >= 8 and spell_sphere == sphere and spell_level <= 3
    return false
```

## Outputs

`may_cast_cleric_spell` returns true when the character may cast the spell, and false for every
class but cleric, druid and ranger. It does not say whether a spell of that level is left to cast
(RULE-MAGIC-001). It changes no state.

## Edge cases

A cleric may cast no Cosmos spell above level 3, so a cleric's spells of levels 4 to 7 all come from
the cleric's own element. The ranger's limit of level 3 follows from the ranger progression
table, which has no column above spell level 3 (RULE-MAGIC-001).

Eleven spells of the manual's summary belong to two or more elemental spheres, such as
Bramblestaff (earth and water) and Conjure Elemental (all four). The procedure takes one
`spell_sphere`; such a spell is open to a character when the procedure allows it for any of its
spheres.

## What the sources say

SRC-MANUAL-1994, page 20, says clerics have major access to the sphere of their element and minor
access to the Sphere of the Cosmos, may cast any spell within their own sphere and Cosmos spells of
third level or less, and cannot cast spells from other spheres. Page 21 gives druids major
access to the Sphere of the Cosmos and to the sphere of their chosen element. Page 20 says a ranger
chooses an elemental sphere at creation and gains the ability to cast cleric spells from it at
eighth level. The cleric spell descriptions (pages 53 to 68) give each spell's sphere as Cosmos or
as Elemental with the element named. The Cleric Spell Summary on page 52 lists the 107 cleric
spells by level, each marked with its spheres and with whether it can be cast only in combat. The
value file `RULE-MAGIC-002.cleric_spells.csv` gives that table, one row per spell in the printed
order, with the columns `spell` (the name as printed), `level`, `spheres` (`air`, `earth`,
`fire`, `water` or `cosmos`, several separated by spaces) and `combat_only` (1 for a spell the
summary marks as combat-only, 0 otherwise). 61 spells are of the Cosmos, 35 of one element and 11
of two or more.

## Differences between builds

None known.

## Open questions

- Whether the game treats the druid as the manual does, whether a ranger's spells come from the
  cleric spell list of the ranger's sphere or from a list of its own, and whether a spell of
  several spheres is open to a caster of any of them (Q-MAGIC-003).
- Where the game keeps a spell's sphere and level, and a character's `sphere` (Q-MAGIC-003,
  Q-PARTY-003).

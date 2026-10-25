---
id: RULE-MAGIC-002
title: Which spheres of cleric spells a priest may cast
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-MAGIC-004, FND-MAGIC-012, FND-MAGIC-003, FND-PARTY-110, FND-PARTY-073, FND-PARTY-161, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-003, RULE-MAGIC-001]
---

## Summary

The game gives each priest spell, numbers 115 to 234, a level from its number and a sphere byte:
bits 0x01, 0x02, 0x04 and 0x08 for air, earth, fire and water, 0x20 for the Sphere of the Cosmos
and 0x10 for the Cosmos spells a cleric may cast, which are those of levels 1 to 3. A character's
sphere is the code of its class within that class's four codes. A cleric or a ranger may take a
spell that has the bit of its element or 0x10; a druid one that has the bit of its element, 0x10 or
0x20. A spell of several spheres is open to a caster of any of them. Which spell levels a caster
reaches is the cast window's limit (RULE-MAGIC-001).

## When it runs

Whenever the game builds a party member's list of priest spells for the Cast Spells / Use PSI
screen (SCR-UI-009) and counts the member's priest spells (RULE-MAGIC-001).

## Parameters

`positions`, the member's class codes (FMT-PARTY-001 `classes`: 1 to 4 Cleric, 5 to 8 Druid, 13 to
16 Ranger, each group in the order air, earth, fire, water), with the positions the list builder
passes over removed (FND-PARTY-110, FND-PARTY-258). `spell`, a priest spell number from 115 to 234.

## Inputs

None beyond the parameters.

## Procedure

```text
# One row per spell 115 to 234: number, name, level, sphere bits.
table priest_spells: (spell, name, level, sphere_bits)[120] from "RULE-MAGIC-002.priest_spells.csv"

define sphere_mask(code):
    if code >= 1 and code <= 4:
        return 0x10 | (1 << (code - 1))
    if code >= 5 and code <= 8:
        return 0x30 | (1 << (code - 5))
    if code >= 13 and code <= 16:
        return 0x10 | (1 << (code - 13))
    if code == 19:
        return 0x3F
    return 0

define may_cast_cleric_spell(positions, spell):
    let bits = priest_spells[spell - 115].sphere_bits
    for each code in positions:
        if (sphere_mask(code) & bits) != 0:
            return true
    return false
```

## Outputs

`may_cast_cleric_spell` returns true when the spell is in the member's priest list at its level.
The Use list holds it when also its `level` is not above `cast_level_limit(positions, 2)` of
RULE-MAGIC-001. It changes no state.

## Edge cases

- A cleric's limit of Cosmos spells to levels 1 to 3 is in the data: only those Cosmos spells carry
  0x10, and a cleric's mask has 0x10 and not 0x20.
- A ranger has the cleric's mask, so it is offered the Cosmos spells of levels 1 to 3 as well as its
  element's, which the manual does not give it. With RULE-MAGIC-001's window it reaches them from
  ranger level 8 and its element's fourth-level spells from 14.
- Spells of several spheres: 0x03 (Dustdevil, Dust Cloud), 0x05 (Create Smoke, Sunstroke), 0x0A
  (Bramblestaff, Thorns of Binding, Crumble, Quicksand, Heartseeker), 0x21 (Protection from Weather
  and Intensification of Weather: air, and the Cosmos for druids) and 0x25 (Sunray: air, fire, and
  the Cosmos for druids). A water cleric or ranger is not offered the two Weather spells, which the
  manual makes air and water spells.
- The manual's Conjure Elemental and Conjure Greater Elemental are four one-element spells each,
  and its Symbol is three Cosmos spells. Charm Mammal is a Cosmos spell (manual: air), Sunstroke air
  and fire (manual: Cosmos), and Dustdevil is in the game only.
- Spells 231 to 234 (levels 8 to 10) are never offered: the window reaches level 8 only from a
  class level of 20.
- A character with classes of two spheres is offered the spells of both.
- Code 19, which no character has, would be offered every priest spell.
- The list holds at most 21 spells per level; no class or pair of sphere classes reaches more than
  21 at one level.

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
of two or more. The game's list differs from it in the ways Edge cases gives
(FND-MAGIC-004).

## Differences between builds

None known.

## Open questions

- Whether the running game offers a level 8 ranger Cosmos spells such as Bless (FND-MAGIC-004,
  Q-MAGIC-020).
- Whether a script field store or a `CHAR` resource the game writes can give a party slot's
  combatant word `+0x04` a value other than the slot, so that overlay 177 `+0B00` reads the class
  bytes and the origin and level bytes from two characters; every writer read sets it to the slot
  (FND-MAGIC-004, FND-MAGIC-012, Q-MAGIC-027).

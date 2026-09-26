---
id: RULE-MAGIC-001
title: How many spells of each level a caster can cast before resting
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

A preserver's, cleric's or ranger's level gives the number of spells of each spell level the
character can cast before resting. A cleric with Wisdom 13 or higher can cast extra spells.
Rangers cast nothing before eighth level and never above spell level 3. Fighters, gladiators,
thieves and psionicists cast no spells.

## When it runs

Whenever the game decides whether a caster has a spell of a level left to cast, and when resting
restores the spells a caster can cast (RULE-MAGIC-004).

## Parameters

`level`, the character's level in the casting class, 1 to 15. `spell_level`, 1 to 7. `wisdom`,
the character's Wisdom score.

## Inputs

None beyond the parameters.

## Procedure

```text
# Seven values per level, levels 1 to 15, one for each spell level from 1 to 7.
table preserver_slots: UINT8[105] from "RULE-MAGIC-001.preserver_slots.csv"
table cleric_slots: UINT8[105] from "RULE-MAGIC-001.cleric_slots.csv"
# Seven values per Wisdom score, 13 to 25, one for each spell level from 1 to 7.
table wisdom_bonus_slots: UINT8[91] from "RULE-MAGIC-001.wisdom_bonus_slots.csv"

define preserver_spell_slots(level, spell_level):
    if level < 1 or spell_level < 1 or spell_level > 7:
        return 0
    return preserver_slots[(min(level, 15) - 1) * 7 + spell_level - 1]

define wisdom_bonus_spells(wisdom, spell_level):
    if wisdom < 13 or spell_level < 1 or spell_level > 7:
        return 0
    return wisdom_bonus_slots[(min(wisdom, 25) - 13) * 7 + spell_level - 1]

define cleric_spell_slots(level, spell_level, wisdom):
    if level < 1 or spell_level < 1 or spell_level > 7:
        return 0
    let slots = cleric_slots[(min(level, 15) - 1) * 7 + spell_level - 1]
    return slots + wisdom_bonus_spells(wisdom, spell_level)

define ranger_spell_slots(level, spell_level):
    if level < 8 or spell_level < 1 or spell_level > 3:
        return 0
    # Three values per ranger level, levels 8 to 15, one for each spell level from 1 to 3.
    let slots: UINT8[24] = [1, 0, 0, 2, 0, 0, 2, 1, 0, 2, 2, 0, 2, 2, 1, 3, 2, 1, 3, 2, 2, 3, 3, 2]
    return slots[(min(level, 15) - 8) * 3 + spell_level - 1]

define ranger_casting_level(level):
    if level < 8:
        return 0
    return min(level, 15) - 7
```

## Outputs

Each `_slots` function returns how many spells of `spell_level` the character can cast before
resting, 0 when it can cast none. `wisdom_bonus_spells` returns the extra spells a cleric's
Wisdom gives at that spell level. `ranger_casting_level` returns the level a ranger casts at, 1 at
ranger level 8 to 8 at level 15, and 0 below level 8. None of them changes state.

## Edge cases

The tables stop at level 15, the highest level any character reaches (RULE-PARTY-007), and at
Wisdom 25; the procedure reads the last row for anything higher. A cleric with Wisdom 13 or 14
gets one extra first-level spell. From Wisdom 19 the table gives more extra fourth-level spells
than third-level ones; the procedure keeps the numbers as the manual prints them.

## What the sources say

SRC-MANUAL-1994, page 13, says the maximum number of spells a character can cast before resting
depends on level, and refers to page 76 for the numbers. Page 76 prints the preserver, cleric and
ranger spell progression tables and the cleric Wisdom spell bonus table, which are the values of
the procedure; the ranger table also gives each ranger level a casting level. Page 20 says a
ranger gains cleric spells of his or her elemental sphere at eighth level, page 21 says clerics
and druids with Wisdom 13 or higher gain extra spells from the Wisdom table, and page 24 says
preservers, clerics, druids and high-level rangers gain more spells as they rise in level. Pages
19 and 20 say fighters and gladiators cannot cast spells, and page 17 says dwarves never cast
spells unless they are clerics. The manual prints no druid spell progression table.

## Differences between builds

None known.

## Open questions

- How many spells a druid can cast at each level: the manual gives druids the Wisdom bonus but
  no progression table of their own (Q-MAGIC-002).
- Whether the Wisdom bonus gives a cleric spells of a level the cleric's own level does not yet
  reach, and whether the game uses the bonus numbers as printed from Wisdom 19 up (Q-MAGIC-002).
- How a multi-class or dual-class character's spells are counted, for example a cleric/preserver
  or a human who has left a casting class (Q-MAGIC-002).
- What the ranger casting level changes: the duration, damage or range of the ranger's spells, or
  nothing (Q-MAGIC-002).

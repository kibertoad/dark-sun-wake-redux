---
id: RULE-MAGIC-001
title: How many spells of each level a caster can cast before resting
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-MAGIC-002, FND-MAGIC-003, FND-MAGIC-006, FND-MAGIC-011, FND-PARTY-110, FND-PARTY-111, FND-PARTY-258, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Each party member has two pools of spells per spell level, one for wizard spells and one for
priest spells. A pool is the sum, over the member's casting classes, of a count that halves the
class level; a cleric or druid adds a second count on Wisdom that does not depend on the class
level. Rangers count from eighth level. A cast from the window needs a spell left in the pool at
the window's level, and the window offers only spell levels up to a limit set by the greatest
qualifying class level, a ranger's taken seven lower. The counts differ from the manual's tables
in most rows.

## When it runs

When the pools are filled (overlay 182 `+0B52`: at a rest, at Start Game and in the new-game
party loader, when ADD or character generation places a character, and on a debug key; a level
gain does not refill them, and a load reads them back as saved, FND-MAGIC-006,
and then reaches the party loader only when the saved game held no placed object, FND-MAGIC-011), when the cast window chooses the spell levels it
offers and tests the pool before a cast, and when a cast spends a spell.

## Parameters

`positions`, the member's class positions 0 to 2 as pairs of class code (FMT-PARTY-001 `classes`,
1 to 4 Cleric, 5 to 8 Druid, 11 Preserver, 13 to 16 Ranger) and level byte, with a held-back former class of a human already removed (FND-PARTY-258). `wisdom`, byte
`+0x1D` of the member's combatant record. `kind`, 1 for wizard spells, 2 for priest spells.
`spell_level`, 1 to 9.

## Inputs

None beyond the parameters.

## Procedure

```text
define spell_term(x, spell_level, p, s, cap):
    # / truncates toward zero
    let n = x - (x + p) / 2 - (spell_level + s)
    if n == 1 and (x & 1) == p:
        n = 2
    if n < 0:
        n = 0
    if n > cap:
        n = cap
    return n

define class_spell_count(code, level, wisdom, kind, spell_level):
    # Cleric 1 to 4, Druid 5 to 8, code 19: the level term and the Wisdom term
    if kind == 2 and ((code >= 1 and code <= 8) or code == 19):
        return spell_term(level, spell_level, 0, -1, 9) + spell_term(wisdom, spell_level, 0, 5, 3)
    # Preserver 11, code 18
    if kind == 1 and (code == 11 or code == 18):
        return spell_term(level, spell_level, 0, -1, 5)
    # Ranger 13 to 16
    if kind == 2 and code >= 13 and code <= 16:
        return spell_term(level, spell_level, 1, 2, 3)
    return 0

define spell_pool(positions, wisdom, kind, spell_level):
    let total = 0
    for each (code, level) in positions:
        if code != 0:
            total = total + class_spell_count(code, level, wisdom, kind, spell_level)
    return total & 0xFF

define cast_level_limit(positions, kind):
    let x = 0
    for each (code, level) in positions:
        let counted = 0
        if kind == 1 and (code == 11 or code == 18):
            counted = level
        if kind == 2 and ((code >= 1 and code <= 8) or code == 19):
            counted = level
        if kind == 2 and code >= 13 and code <= 16:
            counted = level - 7
        if counted > x:
            x = counted
    if kind == 1:
        if x < 10:
            return (x + 1) >> 1
        return x >> 1
    if x < 12:
        return (x + 1) >> 1
    if x < 20:
        return 7
    if x < 28:
        return (x + 4) / 3
    return 10
```

## Outputs

`spell_pool` returns the number of spells of `spell_level` and `kind` the member can cast before
resting; overlay 182 `+0B52` stores it, as a byte, for spell levels 1 to 9. `cast_level_limit`
returns the highest spell level the cast window offers. A cast from the window is refused with the
message `YOU NEED TO REST` when the pool byte at the window's level is 0, and a cast spends one
from the pool of the spell's own kind and level. None of the functions changes state.

## Edge cases

The Wisdom term counts only at 13 and above, and fills spell levels the window may not reach: a
first-level cleric with Wisdom 18 holds 4, 2 and 2 at spell levels 1 to 3 but the window stops at
1. A Preserver at level 11, 13 or 15 holds one spell a level above the window's limit, and a cleric
or druid at 15 one spell of level 8 above the window's 7; those spells cannot be cast. A cleric or
druid at level 12 has a window of 7 and a level 7 count of 0 unless `wisdom` is 25. A ranger casts
fourth-level spells at levels 14 and 15. With the debug byte `DS:13F8` at 1 every count of a slot
below 4 is 19 and the window is not limited.

## What the sources say

SRC-MANUAL-1994, page 13, says the maximum number of spells a character can cast before resting
depends on level, and refers to page 76 for the numbers. Page 76 prints the preserver, cleric and
ranger spell progression tables and the cleric Wisdom spell bonus table, whose rows the game's
counts match only in places (FND-MAGIC-002); the ranger table also gives each ranger level a casting level. Page 20 says a
ranger gains cleric spells of his or her elemental sphere at eighth level, page 21 says clerics
and druids with Wisdom 13 or higher gain extra spells from the Wisdom table, and page 24 says
preservers, clerics, druids and high-level rangers gain more spells as they rise in level. Pages
19 and 20 say fighters and gladiators cannot cast spells, and page 17 says dwarves never cast
spells unless they are clerics. The manual prints no druid spell progression table.

## Differences between builds

None known.

## Open questions

- Which ability byte `+0x1D` of the combatant record is: it is read as Wisdom from FND-PARTY-081 and
  FND-PARTY-165 (Q-MAGIC-018).
- Whether play can reach a save with no placed object, or a failed load from the title screen can
  pass overlay 182 `+1167`'s gate with the placed-object count at 0, and whether a resident routine
  on the load path empties the placed-object table; any of these would let a load reach the party
  loader and refill the pools of slots 0 to 3 (FND-MAGIC-006, FND-MAGIC-011, FND-PARTY-021,
  Q-MAGIC-024).
- What effect 0x5C and overlay 197 `+37A5` do, which let a wizard spell be cast without spending
  from the pool (FND-MAGIC-002, Q-MAGIC-019).
- Whether overlay 208's byte at `DS:9BD3` can make a party spell carry the ranger level less 7,
  and what overlay 193 `+1A79` and overlay 197 `+14DA` do with that level (FND-MAGIC-003,
  Q-MAGIC-019).
- What the caster level a cast carries changes: the cast passes the full class level, a ranger's
  included (FND-MAGIC-003, Q-MAGIC-006).

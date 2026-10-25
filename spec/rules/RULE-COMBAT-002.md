---
id: RULE-COMBAT-002
title: An attack hits on a 20, misses on a 1, and otherwise hits when a d20 is at least the attacker's THAC0, lowered by its terms, less the target's Armor Class
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-091, FND-PARTY-145, FND-COMBAT-065, FND-COMBAT-066, FND-COMBAT-067, FND-COMBAT-068, SRC-MANUAL-1994, SRC-GAMEFAQS-81038]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-COMBAT-003, RULE-COMBAT-006, RULE-COMBAT-007, RULE-COMBAT-010]
---

## Summary

Each swing of an attack draws a d20. A 20 always hits and a 1 always misses; any other roll hits
when it is at least the needed number. The needed number starts from the attacker's THAC0,
combatant byte `+0x16`, and drops by 2 for an attack from the rear, by 2 more for a thief's
backstab, by the attacker's effect, Strength or Dexterity term, by the weapon's magic bonus and
material term, by a two-weapon term, and for a combatant outside the party by the difficulty less
1. The target's Armor Class is then subtracted: the base from details byte `+0x21`, less worn
armour and its magic, less a gladiator's level over 5, plus a Dexterity entry, with effect caps
and terms (FND-PARTY-091, FND-COMBAT-065 to FND-COMBAT-068).

## When it runs

Once for each swing overlay 173 `+2560` makes in an attack of overlay 173 `+1CC1`
(RULE-COMBAT-006, FND-COMBAT-067).

## Parameters

- `thac0: INT8`: the attacker's combatant byte `+0x16`.
- `rear: BOOL`: the attack comes from the direction opposite the one the round's first attacker
  of the target came from (FND-COMBAT-065).
- `backstab: BOOL`: `rear`, and the attacker's `class_flags` has the thief bit 0x400, the distance
  is 1 and the weapon's type byte `+2` is 0x28 or less.
- `attacker_term: INT16`: overlay 197 `+259E`'s value, the sum of `attacker_effect_term` and the
  Strength to-hit entry for a melee attack or the Dexterity missile entry for a ranged one
  (FND-COMBAT-066).
- `weapon_bonus: INT16`: the weapon item's signed byte `+0x16`, or 0 when the target holds
  effect 4; 0 for a natural attack.
- `type_flags: UINT8` and `item_bonus: INT8`: the weapon type's byte `+4` and the item's byte
  `+0x16`; `natural: BOOL` for an attack without a weapon.
- `melee_weapons: INT16`: the number of melee weapons the attacker holds at location 0x11.
- `ranger: BOOL`: bit 0x200 of the attacker's `class_flags`.
- `dexterity_missile: INT8`: the attacker's `DS:084C` entry (FND-COMBAT-066).
- `distance: INT16`: the distance `2D40:35EF` gives (FND-COMBAT-065).
- `enemy: BOOL`: the attacker's slot is 4 or more.
- `difficulty`: 0 to 3.
- `armour_class: INT16`: `armour_class` below for the target.

## Inputs

The signed tables of FND-COMBAT-066, indexed by the score 0 to 25: Strength to hit `DS:0832`
(0, -5, -3, -3, -2, -2, -1, -1, 0 from 8 to 16, 1, 2, 3, 3, 4, 4, 5, 6, 7), Dexterity missile
`DS:084C` (0, -6, -4, -3, -2, -1, 0 from 6 to 15, 1, 2, 2, 3, 3, 4, 4, 4, 5, 5) and Dexterity
Armor Class `DS:0866` (0, 5, 5, 4, 3, 2, 1, 0 from 7 to 14, -1, -2, -3, -4, -4, -4, -5, -5, -5,
-6, -6). `effect_block` of the attacker and the target.

## Procedure

```text
define material_to_hit(type_flags, item_bonus, natural):
    let terms: INT8[4] = [-3, -1, -2, -2]
    if natural or (type_flags & 0x80) != 0 or item_bonus != 0:
        return 0
    if (type_flags & 0x0F) > 3:
        return 0
    return terms[type_flags & 0x0F]

define two_weapon_term(ranger, melee_weapons, dexterity_missile):
    if ranger or melee_weapons <= 1:
        return 0
    if -dexterity_missile > 0:
        return -dexterity_missile
    return 0

define needed_roll(thac0, rear, backstab, attacker_term, weapon_bonus, type_flags, item_bonus, natural, melee_weapons, ranger, dexterity_missile, distance, enemy, difficulty, armour_class):
    let base: INT16 = thac0
    if rear:
        base = base - 2
    if backstab:
        base = base - 2
    base = base - attacker_term
    let bonus: INT16 = weapon_bonus + material_to_hit(type_flags, item_bonus, natural)
    if distance <= 1 and not natural:
        bonus = bonus + two_weapon_term(ranger, melee_weapons, dexterity_missile)
    if enemy:
        bonus = bonus + difficulty - 1
    return base - bonus - armour_class

define attack_hits(needed):
    # one draw of RULE-RNG-001's generator, scaled to 1..20
    let roll = (INT32(draw()) * 20) / 32768 + 1
    if roll == 20:
        return true
    if roll == 1:
        return false
    return needed <= roll

define armour_class(base, worn, gladiator_level, dexterity_ac, rear, effect_terms):
    # base: details byte +0x21; worn: the sum over worn armour pieces (type byte +0x0B bit 0x80,
    # not at location 4) of the type's byte +0x0E plus the item's byte +0x16, a shield (type
    # byte +0 bit 4) left out when rear, the item byte left out when the target holds effect 4
    let a: INT16 = base - worn
    if gladiator_level > 0:
        a = a - gladiator_level / 5
    let t: INT16 = 0
    if not rear:
        t = dexterity_ac
    return a + t + effect_terms
```

`effect_terms` is overlay 197 `+27A7`'s caps and terms on the sum (FND-COMBAT-068): caps of the
Armor Class at 4, 6, `6 - effect_level_sum(t, 6) / 4` or `10 - effect_level_sum(t, 0x28)` under
block bits, and terms of -2, -7, +4, +4, -2, -4, -1, +4, -4 and +8 under other bits.

## Outputs

`attack_hits` gives whether the swing hits.

## Edge cases

A roll equal to the needed number hits, as the manual says; the FAQ's "greater than" does not
hold for this build. A needed number of 1 or less still misses on a 1, and one above 20 still hits
on a 20. A non-ranger with two melee weapons gets no penalty: the two-weapon term is 0 at
Dexterity 6 or more and adds 1 to 6 to the bonus below 6 (FND-COMBAT-066). On Easy enemies get -1
to hit, on Balanced 0, on Hard +1 and on Hideous +2. A shield and the Dexterity entry do not count
against a rear attack.

## What the sources say

SRC-MANUAL-1994, page 24: Armor Class is how hard a character is to hit, lower being harder, and a
high Dexterity improves it. THAC0 is the number needed To Hit Armor Class 0; the computer rolls a
number from 1 to 20 and the attack hits when it is equal to or greater than the THAC0 less the
target's Armor Class. The chance is modified by range, attacks from the rear, magic weapons and
spells. Among the spells, bless improves the THAC0 of friendly characters by 1 and curse worsens
that of enemies by 1; prayer improves friends' THAC0 and saving throws by 1 and worsens enemies'
by 1; a target's invisibility makes melee attacks against it 4 worse and ranged attacks
impossible. Page 21: a thief's backstab, an attack from the direction exactly opposite to the
first attack, has a better chance to hit and does more damage.

SRC-GAMEFAQS-81038, section 2.1, gives the same subtraction but has the roll strictly greater
than THAC0 less Armor Class, and is unsure whether a 1 or a 20 is automatic.

## Differences between builds

None known.

## Open questions

- Which effects the `effect_block` bits of the attacker term and the Armor Class terms stand for,
  and what location 0x11 and the type bytes `+2`, `+6` and `+0x0B` are by name (FND-COMBAT-066,
  FND-COMBAT-068, Q-COMBAT-038).
- Whether a non-ranger with two melee weapons and Dexterity below 6 hits more often in play, and
  whether overlay 197 `+27A7`'s comparison at `+287D`, which reads two locals only one branch
  writes, ever replaces the Armor Class (FND-COMBAT-066, FND-COMBAT-068, Q-COMBAT-041).

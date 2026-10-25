---
id: RULE-COMBAT-003
title: A blow that leaves a combatant at 0 hit points or fewer knocks it out at 0, and one at least 10 more than its hit points kills it
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-086, FND-PARTY-100, FND-RNG-043, FND-COMBAT-067, FND-COMBAT-069, FND-MAGIC-018, FND-COMBAT-071, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-COMBAT-002, RULE-RNG-001]
---

## Summary

A swing that hits rolls the weapon's dice plus its bonuses, at least 1, adds Strength's damage
entry in melee, and multiplies a thief's backstab on the round's first or second swing by 2 to 5.
The hit routine doubles the damage against a combatant already at 0 hit points and scales it by
the target's percentage for the hit word. The damage then sets the hit points: a blow at least the
hit points plus 10 sets them to 0 and kills the combatant (`combat_mark` 7, or 8 from 10,000), a
blow at least the hit points sets them to 0 and knocks it out (`combat_mark` 3), and a smaller one
is subtracted. Hit points never go below 0 (FND-COMBAT-067, FND-COMBAT-069, FND-PARTY-086). A combatant at
`combat_mark` 3 takes no turn, the computer never chooses it as a target, and one outside the party
is removed from the map; it stands up at mark 1 when it is healed, at the end of the fight (party
members, with at least 1 hit point) or at a rest, and dies from one blow of 10 or more
(FND-COMBAT-071).

## When it runs

`swing_damage` for each swing that hits (RULE-COMBAT-002); `take_damage` when the hit routine
deals the damage through overlay 179 `+2A05` and overlay 173 `+22C4`, for weapon hits and for
spells (FND-MAGIC-018).

## Parameters

- `dice`, `sides` and `bonus`: the weapon type's bytes `+9`, `+8` and signed `+0x0A`, or a
  natural attack's values (FND-PARTY-100). `bonus` already holds the item's byte `+0x16` unless
  the attacker or the target holds effect 4, the material damage term, and the ammunition's
  byte `+0x16`.
- `melee: BOOL`: the distance is 1 or less.
- `strength_damage: INT8`: the attacker's `DS:0818` entry (FND-COMBAT-066).
- `backstab: BOOL` and `rear: BOOL`: as RULE-COMBAT-002 gives them.
- `swings_before: UINT8`: the swings the attacker has made this round before this one.
- `thief_level: UINT8`: the attacker's level in class code 0x11.
- `out_cold: BOOL`: the target's `combat_mark` is 3, or its `effect_block` byte 8 has bit 0x20.
- `percentage: INT32`: overlay 197 `+1C52`'s percentage for the target and the hit word
  (FND-COMBAT-069).
- `hit_points: INT16` and `combat_mark: UINT8`: the target's combatant word `+0` and byte `+0x14`.

## Inputs

The Strength damage table `DS:0818` for scores 0 to 25: 0, -4, -2, -1, -1, -1, 0 from 6 to 15,
1, 1, 4, 7, 8, 9, 10, 11, 12, 14 (FND-COMBAT-066). The material damage term of an unenchanted
item of type nibble 0, 1, 2 or 3: -2, -1, -1 or -1.

## Procedure

```text
define swing_damage(dice, sides, bonus, melee, strength_damage, backstab, rear, swings_before, thief_level):
    # roll_sum draws once per die (RULE-RNG-001)
    let d: INT16 = INT16(roll_sum(dice, sides)) + bonus
    if d < 1:
        d = 1
    if melee:
        d = d + strength_damage
    if backstab and rear and swings_before <= 1:
        let m = (thief_level - 1) / 4 + 2
        if m > 5:
            m = 5
        d = d * m
    return d

define weapon_hit_damage(d, out_cold, percentage):
    let total: INT32 = d
    if out_cold:
        total = 2 * d
    return (total * percentage) / 100

define take_damage(hit_points, combat_mark, d):
    if d <= 0:
        return [hit_points, combat_mark]
    if hit_points + 10 <= d:
        if d >= 10000:
            return [0, 8]
        return [0, 7]
    if d >= hit_points:
        return [0, 3]
    return [hit_points - d, combat_mark]
```

`weapon_hit_damage` leaves out overlay 197 `+0000`'s other steps, which FND-MAGIC-018 lists for
spells and which apply to weapon hits through effects.

## Outputs

`swing_damage` gives the damage of the swing; `take_damage` the new hit points and `combat_mark`.

## Edge cases

A combatant at 0 hit points stays at 0 and `combat_mark` 3 under blows below 10 and dies only from
one blow of 10 or more; a weapon hit on it deals double damage. The manual's "dead at -10" holds
only as one blow that would take the combatant to -10. A ranged swing with attack number 1 or 2
deals no damage (FND-COMBAT-067). The doubled damage against a target at mark 3 reaches a party
member through an attack whose target was set before it fell (the walker sweep, the direction
key) or, as the maximum, through an area spell. A heal stands the combatant up even when it adds
no hit points (FND-COMBAT-071). The attacker's returned total keeps the damage before the
backstab multiplier; only the hit routine gets the product.

## What the sources say

SRC-MANUAL-1994, page 24, Hit Points: damage is subtracted from the hit points, a character at 0
is unconscious, and one at -10 or less is dead.

## Differences between builds

None known.

## Open questions

- How the player orders an attack on a chosen combatant, and whether that path tests the target's
  `combat_mark` (FND-COMBAT-071, Q-COMBAT-040).
- Which effects the bits behind `out_cold` and the percentage stand for (FND-COMBAT-069,
  Q-COMBAT-038).

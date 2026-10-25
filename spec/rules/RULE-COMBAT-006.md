---
id: RULE-COMBAT-006
title: An attack at distance 1 or less uses the first melee weapon held, or a natural attack without one, and beyond 1 needs a held missile weapon whose range reaches the target
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-065, FND-COMBAT-066, FND-COMBAT-067, FND-PARTY-100, SRC-MANUAL-1994, FND-COMBAT-021]
conflicting: []
split_with: []
related: [RULE-COMBAT-002, RULE-COMBAT-003, RULE-INPUT-001]
---

## Summary

In combat, clicking an enemy with the Walk or Attack pointer makes the character whose turn it is
walk to it and attack (FND-COMBAT-021). The attack routine overlay 173 `+1CC1` takes the distance
to the target. At distance 1 or less it attacks with the first melee weapon held at location
0x11, a weapon type with range byte `+6` 1, or with a natural attack when there is none. Beyond 1
it needs a held weapon whose range byte is above 1 and at least the distance, and refuses the
attack otherwise. Attack numbers 0 to 2 step through the held weapons, so a second melee weapon is
attack number 1. Each weapon swings up to its rate in half attacks per round (FND-COMBAT-065,
FND-COMBAT-067).

## When it runs

When a combatant attacks: a party member through the click, and the computer's combatants through
the resident calls of FND-COMBAT-065.

## Parameters

- `ranges`: for each entry of the attacker's item list in order, the range byte `+6` of its type
  when the entry is of kind 1 at location 0x11, or 0 otherwise.
- `distance: INT16`: the distance `2D40:35EF` gives, or 1 from the callers that pass 1.
- `rate: UINT8`: the weapon type's byte `+7` at range, the attacker's details byte `+0x24` in
  melee, or a natural attack's rate (FND-PARTY-100).
- `rate_effect: UINT8`: overlay 197 `+2499`'s value for the attacker, 0 to 6 (FND-COMBAT-067).
- `round_parity: UINT8`: bit 0 of the clock `4C13:035B` divided by 60.
- `ammunition: INT16`: overlay 173 `+2A9A`'s count, 999 for a weapon that needs none.

## Inputs

None beyond the parameters.

## Procedure

```text
define pick_weapon(ranges, distance, after):
    for i in after + 1..len(ranges):
        if ranges[i] != 0:
            if (distance <= 1 and ranges[i] == 1) or (distance > 1 and ranges[i] > 1):
                return i
    return -1

define attack_kind(ranges, distance):
    let w = pick_weapon(ranges, distance, -1)
    if distance <= 1:
        if w == -1:
            return [0, -1]
        return [1, w]
    if w == -1 or ranges[w] < distance:
        return [-1, -1]
    return [2, w]

define swings_per_round(rate, rate_effect, round_parity, ammunition):
    let r: INT16 = rate
    if rate_effect == 4:
        r = r * 2
    if rate_effect == 1:
        r = r / 2
    r = (r + round_parity) / 2
    if r > ammunition:
        r = ammunition
    return r
```

## Outputs

`attack_kind` gives 0 for a natural attack, 1 for a melee attack and 2 for a ranged attack, with
the weapon entry, or -1 when the attack is refused. `swings_per_round` gives how many swings the
weapon can make this round.

## Edge cases

A character next to an enemy with only a missile weapon makes a natural attack. A rate of 3
alternates one and two swings from round to round, since each round step adds 60 to the clock
(FND-COMBAT-060). An ammunition count of 0 shows "Out of ammo" and gives no swing. The routine
stops at the first swing that hits and calls the swing routine again while the target stands, so
one attack spends all the round's swings of a weapon. When neither attack is allowed, the pointer
shows the Invalid icon (SRC-MANUAL-1994, page 5).

## What the sources say

SRC-MANUAL-1994, pages 4 and 5: once combat has begun with an attack, clicking a target with the
Walk pointer walks to it and attacks automatically. A hand-to-hand attack needs the enemy
adjacent and a weapon readied; a character may ready two one-handed weapons. Rangers and
characters with a high Dexterity fight with two weapons without penalty, and others use the
second at a disadvantage. A ranged attack needs the enemy at a distance, within range, and a
missile weapon or ammunition readied; a target out of range shows the Invalid icon. The computer
decides which kind of attack applies. The owner saw a click on an enemy make the active character
walk up and strike it, with no step of choosing or confirming the target (FND-COMBAT-021).

## Differences between builds

None known.

## Open questions

- How the click on an enemy reaches the attack routine, and what the pointer shows before it
  (Q-COMBAT-001).
- What location 0x11, the range byte `+6` and the rate effects of overlay 197 `+2499` are by name
  (FND-COMBAT-065, FND-COMBAT-067, Q-COMBAT-038).

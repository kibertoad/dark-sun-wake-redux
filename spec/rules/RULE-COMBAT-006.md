---
id: RULE-COMBAT-006
title: A click on an enemy attacks it, in melee when adjacent with a readied weapon or at range with a missile weapon or ammunition
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, FND-COMBAT-021]
conflicting: []
split_with: []
related: [RULE-COMBAT-002, RULE-INPUT-001]
---

## Summary

In combat, clicking an enemy with the Walk or Attack pointer makes the character whose turn it is
walk to it and attack. The game picks a hand-to-hand attack when the enemy is adjacent and the
character has a weapon readied, and a ranged attack when the enemy is at a distance and the
character has a missile weapon or ammunition readied.

## When it runs

When the player left-clicks an enemy during a party member's turn in combat (SRC-MANUAL-1994,
pages 4 and 5).

## Parameters

`adjacent`, true when the target stands next to the attacker. `in_range`, true when the target is
within the range of the attacker's missile weapon. `weapon_readied`, true when the attacker has a
hand-to-hand weapon readied. `missile_readied`, true when the attacker has a missile weapon or
ammunition readied. `main_one_handed` and `off_one_handed`, true when the weapon in each hand is
one-handed.

## Inputs

None beyond the parameters.

## Procedure

```text
define melee_allowed(adjacent, weapon_readied):
    return adjacent and weapon_readied

define ranged_allowed(adjacent, in_range, missile_readied):
    return not adjacent and in_range and missile_readied

define two_weapons_allowed(main_one_handed, off_one_handed):
    return main_one_handed and off_one_handed
```

## Outputs

`melee_allowed` and `ranged_allowed` give whether each kind of attack can be made;
`two_weapons_allowed` whether the character fights with a weapon in each hand.

## Edge cases

When neither attack is allowed, the pointer shows the Invalid icon (SRC-MANUAL-1994, page 5). The
manual does not say what happens when a character next to an enemy has only a missile weapon
readied.

## What the sources say

SRC-MANUAL-1994, pages 4 and 5: once combat has begun with an attack, clicking a target with the
Walk pointer walks to it and attacks automatically. A hand-to-hand attack needs the enemy
adjacent and a weapon readied; a character may ready two one-handed weapons. Rangers and
characters with a high Dexterity fight with two weapons without penalty, and others use the
second at a disadvantage. A ranged attack needs the enemy at a distance, within range, and a
missile weapon or ammunition readied; a target out of range shows the Invalid icon. The computer
decides which kind of attack applies. Non-player characters may flee, fight back or call for
help. The owner saw a click on an enemy make the active character walk up and strike it, with no
step of choosing or confirming the target (FND-COMBAT-021).

## Differences between builds

None known.

## Open questions

- Where the game decides the attack, what counts as adjacent and as in range, which Dexterity
  counts as high, and what the penalty for the second weapon is; no code that handles a click on
  an enemy has been found (Q-COMBAT-001, Q-COMBAT-005).

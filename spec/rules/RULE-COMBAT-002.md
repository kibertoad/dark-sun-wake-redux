---
id: RULE-COMBAT-002
title: An attack hits when a roll of 1 to 20 is at least the attacker's THAC0 less the target's Armor Class
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, SRC-GAMEFAQS-81038]
conflicting: []
split_with: []
related: []
---

## Summary

To hit, an attacker rolls a number from 1 to 20. The attack hits when the roll is at least the
attacker's THAC0 minus the target's Armor Class, so a lower Armor Class is harder to hit.

## When it runs

`attack_hits` runs for each attack a combatant makes (SRC-MANUAL-1994, page 24).

## Parameters

`roll`, the number rolled, 1 to 20. `thac0`, the attacker's THAC0 with its modifiers applied.
`armour_class`, the target's Armor Class.

## Inputs

None beyond the parameters.

## Procedure

```text
define attack_hits(roll, thac0, armour_class):
    return roll >= thac0 - armour_class
```

## Outputs

True when the attack hits.

## Edge cases

The manual's examples: with a THAC0 of 5, a target of Armor Class 3 is hit on 2 or more, and one
of Armor Class -2 on 7 or more. When `thac0 - armour_class` is 1 or less every roll hits, and when
it is more than 20 none does; the manual does not say whether a 20 always hits or a 1 always
misses.

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

- Whether a roll equal to `thac0 - armour_class` hits, as the manual says, or misses, as the FAQ
  says; whether 1 and 20 are automatic; which generator draws the roll and in what order; and
  what the modifiers are and in which order they apply. No code that compares a roll with THAC0
  and Armor Class has been found (Q-COMBAT-005).

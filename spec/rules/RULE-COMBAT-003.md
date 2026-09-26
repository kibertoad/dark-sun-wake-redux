---
id: RULE-COMBAT-003
title: Damage lowers hit points; at 0 a character is unconscious and at -10 dead
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-COMBAT-002]
---

## Summary

Damage is taken from a character's hit points. A character left with 0 hit points or fewer falls
unconscious, and one left with -10 or fewer dies.

## When it runs

`damage_outcome` runs each time a combatant takes damage.

## Parameters

`hit_points`, the combatant's hit points before the damage. `damage`, the damage done, 0 or more.

## Inputs

None beyond the parameters.

## Procedure

```text
define damage_outcome(hit_points, damage):
    let left = hit_points - damage
    if left <= -10:
        return 2
    if left <= 0:
        return 1
    return 0
```

## Outputs

0 when the combatant stays conscious, 1 when it is unconscious, and 2 when it is dead. The
combatant's hit points become `hit_points - damage`.

## Edge cases

A combatant already at 0 to -9 who takes more damage stays unconscious until the total reaches
-10. The manual sets no lower limit on hit points.

## What the sources say

SRC-MANUAL-1994, page 24, Hit Points: damage is subtracted from the hit points, a character at 0
is unconscious, and one at -10 or less is dead.

## Differences between builds

None known.

## Open questions

- Whether the game keeps hit points below -10, how an unconscious character recovers, and what
  each state stops a character doing (Q-COMBAT-005).
- The executable holds a list of nine names, New, Okay, Stunned, Out Cold, Dying, Animated,
  Petrified, Dead and Gone, which suggests states between conscious and dead that the manual does
  not describe; no code that reads the list was found (FND-COMBAT-028, circumstantial). A search
  for the constant -10 found too many uses to follow (FND-COMBAT-002, circumstantial)
  (Q-COMBAT-005).

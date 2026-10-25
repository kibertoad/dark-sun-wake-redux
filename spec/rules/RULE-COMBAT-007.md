---
id: RULE-COMBAT-007
title: What the difficulty setting changes in combat
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-COMBAT-065]
conflicting: []
split_with: []
related: [RULE-CONFIG-002, RULE-COMBAT-002]
---

## Summary

The difficulty setting (RULE-CONFIG-002) gives every combatant outside the party, slot 4 or more,
`difficulty - 1` to hit: -1 on Easy, 0 on Balanced, +1 on Hard and +2 on Hideous
(FND-COMBAT-065). The game reads the setting in other places too, which are not described here.

## When it runs

`enemy_to_hit` runs in each attack of overlay 173 `+1CC1` (RULE-COMBAT-002).

## Parameters

- `slot: INT16`: the attacker's slot.
- `difficulty`: 0 to 3.

## Inputs

None beyond the parameters.

## Procedure

```text
define enemy_to_hit(slot, difficulty):
    if slot >= 4:
        return difficulty - 1
    return 0
```

## Outputs

The amount added to the attacker's to-hit bonus in RULE-COMBAT-002.

## Edge cases

Party members are not changed by the setting here.

## What the sources say

SRC-MANUAL-1994, page 15: difficulty controls the level of difficulty in combat. SRC-GAMEFAQS-81038,
section 2.8: a new game begins on Balanced; Easy seems to give hostile creatures about half the hit
points they have on Balanced and Hideous about twice as many, and the change applies when a
creature appears, not to creatures already present. It says nothing of Hard, of rounding, or of the
party.

## Differences between builds

None known.

## Open questions

- What the other readers of the difficulty word do, at overlay 187 file `0x703B1`, overlay 192
  `0x7D705`, overlay 203 `0x8B633` to `0x8B916` and overlay 204 `0x8C226`, and whether one scales
  the hit points of hostile creatures when they appear as the FAQ reports (FND-COMBAT-065,
  Q-COMBAT-007).

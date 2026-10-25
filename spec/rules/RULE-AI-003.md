---
id: RULE-AI-003
title: Whether a combatant uses a spell from its list depends on its kind byte, its hit points, a one-in-four roll or a game state byte
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-RNG-038, FND-COMBAT-052, FND-COMBAT-022, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-AI-002, RULE-RNG-001]
---

## Summary

Before a combatant whose kind byte names a spell list casts from it, `spell_use_test` decides
whether it does. It always rolls a d4 first. By the kind byte, the answer is then: yes below half
hit points (kinds 1 to 4, 6 and 17), the d4's 4 (kinds 5, 7, 8, 9, 14, 16, 18, 19 and 20), yes
when the byte `g_50C2_0000` is 4 (kind 10), yes when that byte is 0 and otherwise the d4's 4
(kinds 11 and 12), never (kind 13), always (kind 15), and never for a kind of 0 or above 20.

## When it runs

The fight routine overlay 173 calls it at `+15F0` for a combatant after overlay 198 `+0149` of the
combatant's kind byte and 0 has returned other than -1, and goes on to the spell only when it
returns other than 0 (FND-RNG-035).

## Parameters

- `combatant: INT16`: the combatant's slot.

## Inputs

`combatants`: byte `+0x0E` of the combatant's record, the kind byte (`special_attack` of FMT-COMBAT-001),
its `hit_points` and its `details_index`; `combatant_details`: the `max_hit_points` of that details
record; and `g_50C2_0000`. The record is the combatant's number times 49, with no slot map.

## Procedure

```text
define spell_use_test(combatant):
    let c = combatants[combatant]
    let kind: UINT16 = c.special_attack
    # roll_sum draws once (RULE-RNG-001)
    let lucky = roll_sum(1, 4) == 4
    let low = UINT16(c.hit_points) < UINT16(combatant_details[c.details_index].max_hit_points) >> 1
    if kind in [1, 2, 3, 4, 6, 17]:
        return low
    if kind in [5, 7, 8, 9, 14, 16, 18, 19, 20]:
        return lucky
    if kind == 10:
        return g_50C2_0000 == 4
    if kind == 11 or kind == 12:
        if g_50C2_0000 == 0:
            return true
        return lucky
    if kind == 15:
        return true
    return false
```

## Outputs

True when the combatant uses a spell.

## Edge cases

Every call draws once, also for the kinds whose answer does not use the d4. The hit point test
compares unsigned words, so hit points read as a negative signed word count as high.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the kind byte's values stand for, which combatants have one, what overlay 198 `+0149`
  picks, and what `g_50C2_0000` holds (FND-RNG-035, FND-RNG-038, Q-AI-004).

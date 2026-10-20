---
id: RULE-MAGIC-018
title: DATA 89 applies effect 0x45 for 60 times 4d4, 2d4 or 1d4 by the target's hit points, kept as a signed byte, and DATA 23 and 245 apply effects 98 and 2 carrying a d6
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`effect_dice` is the part of the hit routine's effect switch that rolls for an effect's duration
or its last word. `DATA` 89 rolls 4d4 for a target with fewer than 31 hit points, 2d4 below 61 and
1d4 below 91, multiplies the total by 60 and keeps the low byte as a signed duration for effect
0x45; a target with 91 or more gets the effect with duration 0 and no roll. `DATA` 23 and 245
apply effect 98 and effect 2 for the hit's duration, with a d6 as the effect's last word.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with `DATA` 89, 23 or 245 (FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `c: UINT16`: the second slot.
- `n: UINT16`: the `DATA` number.
- `flags: UINT8[]`: the hit's flags byte.
- `u: INT16`: the hit routine's duration word.

## Inputs

Through `object_slot_combatants`, `hit_points` of `t`'s `combatants` record.

## Procedure

```text
define effect_dice(t, c, n, flags, u):
    let hp: INT16 = combatants[object_slot_combatants[t]].hit_points
    if n == 89:
        let d: INT8 = 0
        # roll_sum draws 4, 2 or 1 times, or not at all (RULE-RNG-001)
        if hp < 31:
            d = INT8(UINT8(roll_sum(4, 4) * 60))
        else if hp < 61:
            d = INT8(UINT8(roll_sum(2, 4) * 60))
        else if hp < 91:
            d = INT8(UINT8(roll_sum(1, 4) * 60))
        emit EffectApplied(t, c, n, 0x45, INT32(d), 0)
        return
    # n is 23 or 245; roll_sum draws once (RULE-RNG-001)
    let code = 2 if n == 245 else 98
    emit EffectApplied(t, c, n, code, INT32(u), roll_sum(1, 6))
    flags[0] = flags[0] | 0x02
```

After each path `+1373` goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The effect, and for 23 and 245 bit 0x02 of the hit's flags byte.

## Edge cases

The low byte of `60 * total` is read as signed: totals 1 to 16 give 60, 120, -76, -16, 44, 104,
-92, -32, 28, 88, -108, -48, 12, 72, -124 and -64, so 4d4 gives a negative duration for totals 4,
7, 8, 11, 12, 15 and 16, 2d4 for 3, 4, 7 and 8, and 1d4 for 3 and 4. A duration of 0 is passed for a target with 91 hit points or more.
The hit-point comparisons are signed, so negative hit points count as fewer than 31.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What effects 0x45, 98 and 2 are, what a negative or zero duration does in overlay 193 `+166F`,
  and which spells the `DATA` numbers are (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- What `EffectApplied` does in overlay 193 with its arguments (FND-RNG-047, Q-RNG-021).

---
id: RULE-MAGIC-021
title: DATA 203 raises a dead party member on a d100 below 5 times its byte 0x1B less 9, plus 70, at the cost of a point of that byte, and otherwise leaves it permanently dead
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-AI-004, FND-PARTY-081, FND-COMBAT-023, FND-ACTOR-003, FND-PARTY-013, FND-RNG-041]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-022]
---

## Summary

`raise_roll` is the case of the hit routine's effect switch for `DATA` 203. A target that is a
party slot whose `combat_mark` is 7 and whose details byte `+0x12` is not 3 rolls a d100; below
`5 * (b - 9) + 70`, `b` being its combatant byte `+0x1B`, it comes back with 1 hit point and
`combat_mark` 1, loses 1 from that byte and from its details byte `+0x17`, and takes effects 0x42,
0x44, 0x41 and 0x43 with no end. At or above it, the game shows `Blew Con roll, permanantly dead`
and sets `combat_mark` to 8. Any other target shows `Cannot raise` and makes no roll.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with `DATA` 203 (FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `c: UINT16`: the second slot.
- `flags: UINT8[]`: the hit's flags byte.

## Inputs

`combatant_found`, `fn_2D40_3E64` and `combatant_details_of` of `t`; through
`object_slot_combatants`, `t`'s `combatants` record `R`: `combat_mark`, `hit_points` and byte
`+0x1B` (`unk_19[2]`); its `combatant_details` record `D`: bytes `+0x12` (`unk_10[2]`) and
`+0x17` (`unk_10[7]`); `entry_index` of `t`'s record of `object_slots` and `g_57E0_67B7`.

## Procedure

```text
define may_raise(t) -> UINT8:
    if not combatant_found(t) or t >= 4:
        return 0
    if combatants[fn_2D40_3E64(t)].combat_mark != 7:
        return 0
    if combatant_details[combatant_details_of(t)].unk_10[2] == 3:
        return 0
    return 1

define raise_roll(t, c, flags):
    if may_raise(t) == 0:
        emit MessageShown("Cannot raise")
        return
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    let need: INT16 = 5 * (INT16(R.unk_19[2]) - 9) + 70
    # roll_sum draws once (RULE-RNG-001); signed word compare
    if INT16(roll_sum(1, 100)) >= need:
        emit MessageShown("Blew Con roll, permanantly dead")
        R.combat_mark = 8
        return
    D.unk_10[7] = UINT8(D.unk_10[7] - 1)
    R.unk_19[2] = UINT8(R.unk_19[2] - 1)
    R.hit_points = 1
    R.combat_mark = 1
    for code in [0x42, 0x44, 0x41, 0x43]:
        emit EffectApplied(t, c, 203, code, -9999, 0)
    flags[0] = flags[0] | 0x02
    let e = object_slots[t].entry_index
    g_57E0_67B7[e].byte_5 = g_57E0_67B7[e].byte_5 & 0xEF
```

`+1373` then goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The changes to `R` and `D`, the four effects, bit 0x02 of the hit's flags byte and the cleared
bit, or a message and, after a failed roll, `combat_mark` 8.

## Edge cases

A byte `+0x1B` of 16 or more always succeeds, 15 fails only on 100, and even 0 succeeds on 1 to
24. The two bytes are lowered as bytes, so 0 becomes 255. A `combat_mark` of 8 fails the gate,
so a failed raising cannot be retried with this case.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What effects 0x42, 0x44, 0x41 and 0x43 are, what details bytes `+0x12` 3 and `+0x17` hold, what
  bit 0x10 of the cleared byte marks, and whether
  byte `+0x1B` is the Constitution score the message names (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- What `EffectApplied` does in overlay 193 with its arguments (FND-RNG-047, Q-RNG-021).
- Whether `fn_2D40_3E64`, which this spec has not read, returns the index of `t`'s record of
  `combatants` here, and what else reads byte `+5` of `g_57E0_67B7`'s entries (FND-RNG-047,
  Q-RNG-021).

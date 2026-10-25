---
id: RULE-MAGIC-020
title: DATA 60 and 165 deal 10,000 damage to a target of details byte 0x12 14 when a d20 reaches 11 less a plus the target's details byte 0x1E
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-PARTY-086, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-023, FND-RNG-051]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`destroy_roll` is the case of the hit routine's effect switch for `DATA` 60 and 165. It rolls a
d20, and when the roll is at least `11 - a` plus the target's details byte `+0x1E` and the
target's details byte `+0x12` is 14, it passes 10,000 damage to overlay 173 `+22C4`, the damage
routine, directly.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with `DATA` 60 or 165 (FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `flags: UINT8[]`: the hit's flags byte.
- `a: UINT8`: the hit routine's byte argument at its `bp+0x1C`, whose value at each caller
  FND-RNG-051 lists.

## Inputs

Through `object_slot_combatants`, `t`'s `combatant_details` record: bytes `+0x12` (`origin`)
and `+0x1E` (`levels[0]`).

## Procedure

```text
define destroy_roll(t, flags, a):
    let D = combatant_details[combatants[object_slot_combatants[t]].details_index]
    let need: INT16 = 11 - a + D.levels[0]
    # roll_sum draws once (RULE-RNG-001); signed word compare
    if need > INT16(roll_sum(1, 20)):
        return
    if D.origin != 14:
        return
    fn_5671_005C(t, 10000)
    flags[0] = flags[0] | 0x01
```

`+1373` then goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The 10,000 damage and bit 0x01 of the hit's flags byte.

## Edge cases

The roll is made before byte `+0x12` is tested, so a target of any other kind costs a draw. The
damage goes to the damage routine without overlay 179 `+2A05`, so none of that routine's other
calls (the cry or dissolve of RULE-SOUND-004 and RULE-VIDEO-006) run for it here. A need of 1 or
less always passes and one above 20 never does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What details bytes `+0x12` 14 and `+0x1E` stand for, what `fn_5671_005C` does with 10,000,
  and which spells 60 and 165 are (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- What the values of `a` and `g` that callers pass on from their own arguments and locals are, and
  what the actor's byte `+0x15` and `a` stand for (FND-RNG-051, Q-MAGIC-016).

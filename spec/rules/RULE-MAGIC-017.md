---
id: RULE-MAGIC-017
title: DATA 227, 295 and 298 deal 8d6 to a target of details byte 0x12 9, 2d4 or 2d10, and 3d6 on a saved throw, and DATA 38, 243, 127 and 312 pass (a/2)d6, 1d6, 1d8 and 1d6 to unread routines
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-PARTY-086, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-RNG-041, FND-EXPLORE-005, FND-ACTOR-003, FND-PARTY-013, FND-COMBAT-023, FND-RNG-051]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`damage_dice` is the part of the hit routine's effect switch that rolls dice of damage or passes
dice to another routine. 227 deals 8d6 to a target whose details byte `+0x12` is 9 and, on a
saved throw, applies effect 0x0A for half the duration. 295 deals 2d4 on a saved throw and 2d10
otherwise. 298 deals 3d6 on a saved throw only. 227 and 298 then run the caster's tail steps.
38 and 243 pass `(a / 2)`d6 and 1d6 to `fn_575A_004D` when there is a second slot, 127 passes 1d8
to `fn_575A_0048`, and 312 passes 1d6 to `fn_5699_005C` when the second slot is not the target,
and otherwise applies effect 0x5C to the target.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with one of these `DATA` numbers
(FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `c: UINT16`: the second slot.
- `n: UINT16`: the `DATA` number.
- `s: UINT8`: the saving throw byte, 1 for a success.
- `flags: UINT8[]`: the hit's flags byte.
- `a: UINT8`: the hit routine's byte argument at its `bp+0x1C`, whose value at each caller
  FND-RNG-051 lists.
- `u: INT16`: the hit routine's duration word.

## Inputs

Through `object_slot_combatants`, `t`'s `combatant_details` record `D`: byte `+0x12`
(`origin`); `c`'s `combatants` record, when `c` is below 9999, and `t`'s record of `object_slots`.

## Procedure

```text
define caster_tail(t, c):
    fn_28C9_2C9E(c, object_slots[t])
    fn_5671_00F2(c)
    # through a pointer that is 0:0 when c is 9999 or more
    combatants[object_slot_combatants[c]].special_attack = 0

define damage_dice(t, c, n, s, flags, a, u):
    let D = combatant_details[combatants[object_slot_combatants[t]].details_index]
    if n == 227:
        if D.origin == 9:
            # roll_sum draws eight times (RULE-RNG-001)
            emit DamageDealt(t, roll_sum(8, 6))
            flags[0] = flags[0] | 0x01
        if s != 0:
            emit EffectApplied(t, c, n, 0x0A, INT32(u / 2), 0)
            flags[0] = flags[0] | 0x02
        caster_tail(t, c)
    else if n == 295:
        # roll_sum draws twice (RULE-RNG-001)
        emit DamageDealt(t, roll_sum(2, 4) if s != 0 else roll_sum(2, 10))
    else if n == 298:
        if s != 0:
            # roll_sum draws three times (RULE-RNG-001)
            emit DamageDealt(t, roll_sum(3, 6))
        caster_tail(t, c)
    else if n == 38 or n == 243:
        if c >= 9999:
            return
        # roll_sum draws a / 2 times for 38 and once for 243 (RULE-RNG-001)
        let total = roll_sum(a / 2, 6) if n == 38 else roll_sum(1, 6)
        fn_575A_004D(t, c, total, n)
        flags[0] = flags[0] | 0x03
    else if n == 127:
        # roll_sum draws once (RULE-RNG-001)
        fn_575A_0048(t, c, roll_sum(1, 8), n)
        flags[0] = flags[0] | 0x02
    else if n == 312:
        if t == c:
            emit EffectApplied(t, c, 312, 0x5C, -9999, 0)
        else:
            # roll_sum draws once (RULE-RNG-001)
            fn_5699_005C(t, c, roll_sum(1, 6))
        flags[0] = flags[0] | 0x02
```

`DamageDealt` here is overlay 179 `+2A05` with `n` and the codes 0x86 for 227, 0x20 for 295 and 8
for 298. After each path `+1373` goes on to the steps after its switch (FND-PARTY-166), with its
local bits set as FND-RNG-047 lists.

## Outputs

The damage, the calls with the totals, the effects, and the bits of the hit's flags byte above.

## Edge cases

38 with `a` below 2 rolls no die and passes 0. 227's effect applies on a saved throw whatever byte
`+0x12` is, and its duration is `u` halved toward zero. 227's and 298's tail writes the byte
`+0x0E` through the second slot's record pointer, which is 0:0 when `c` is 9999 or more.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_575A_004D`, `fn_575A_0048`, `fn_5699_005C`, `fn_5671_00F2` and `fn_28C9_2C9E` do with
  their arguments, what `fn_28C9_2C9E` reads from the object slot, what effects 0x0A and 0x5C and byte `+0x12` 9
  stand for, and which spells the `DATA` numbers are (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- What the values of `a` and `g` that callers pass on from their own arguments and locals are, and
  what the actor's byte `+0x15` and `a` stand for (FND-RNG-051, Q-MAGIC-016).
- What `EffectApplied` does in overlay 193 with its arguments (FND-RNG-047, Q-RNG-021).

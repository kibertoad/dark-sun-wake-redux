---
id: RULE-MAGIC-016
title: DATA 117, 166, 170, 194 and 210 pass 1d8, 2d6, 2d8+1, 3d8+3 or the target's maximum less its hit points less 1d4 to overlay 197 +2D05, and DATA 211 damages a target down to 1d4
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-PARTY-086, FND-COMBAT-018, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-RNG-041, FND-COMBAT-023, FND-RNG-051]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`hit_point_dice` is the part of the hit routine's effect switch that rolls for the `DATA` numbers
whose totals go to a target's hit points. 117 passes 1d8, 170 2d8 + 1 and 194 3d8 + 3 to
`fn_575A_0043` with the target, as 166 passes 2d6 when the target's byte `+0x15` matches the hit's
byte `g`; 210 passes the target's maximum hit points less its hit points less 1d4, after removing
nineteen effects. 194 and 210 call `fn_575A_008E` first. 211 applies effect 0x1B and then deals
the target its hit points less 1d4 in damage. The readings as healing and as leaving 1d4 hit
points rest on the amounts, since `fn_575A_0043` was not read.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with one of these `DATA` numbers
(FND-PARTY-096); `+1373` is the effect switch.

## Parameters

- `t: INT16`: the target's object slot.
- `c: UINT16`: the second slot.
- `n: UINT16`: the `DATA` number.
- `s: UINT8`: the saving throw byte, 1 for a success.
- `flags: UINT8[]`: the hit's flags byte.
- `g: UINT8`: the hit routine's byte argument at its `bp+0x1E`: the acting combatant's byte
  `+0x15`, or a value its caller passes, as FND-RNG-051 lists.
- `u: INT16`: the hit routine's duration word.

## Inputs

Through `object_slot_combatants`, `t`'s `combatants` record `R`: `hit_points` and `unk_15[0]`
(byte `+0x15`); its `combatant_details` record `D`: `max_hit_points`.

## Procedure

```text
define hit_point_dice(t, c, n, s, flags, g, u):
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    let amount: UINT16 = 0
    if n == 117:
        # roll_sum draws once (RULE-RNG-001)
        amount = roll_sum(1, 8)
    else if n == 170:
        # roll_sum draws twice (RULE-RNG-001)
        amount = roll_sum(2, 8) + 1
    else if n == 194:
        fn_575A_008E(213, t)
        # roll_sum draws three times (RULE-RNG-001)
        amount = roll_sum(3, 8) + 3
    else if n == 166:
        if R.unk_15[0] != g:
            if s == 0:
                emit EffectApplied(t, c, n, 0x45, INT32(u), 0)
            flags[0] = flags[0] | 0x01
            return
        # roll_sum draws twice (RULE-RNG-001)
        amount = roll_sum(2, 6)
    else if n == 210:
        fn_575A_008E(213, t)
        for code in [108, 105, 101, 100, 82, 44, 37, 35, 33, 27, 21, 20, 10, 65, 66, 68, 67, 69, 70]:
            emit EffectRemoved(t, code)
        # roll_sum draws once (RULE-RNG-001)
        let r = roll_sum(1, 4)
        amount = UINT16(D.max_hit_points - R.hit_points - r)
    else if n == 211:
        emit EffectApplied(t, c, n, 0x1B, -9999, -1)
        # roll_sum draws once (RULE-RNG-001); hit_points is read after the effect
        let r = roll_sum(1, 4)
        emit DamageDealt(t, UINT16(R.hit_points - r))
        flags[0] = flags[0] | 0x03
        return
    fn_575A_0043(t, amount)
    flags[0] = flags[0] | 0x02
```

After each path `+1373` goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The call of `fn_575A_0043` with the amount, or for 211 the damage through overlay 179 `+2A05` with
code -1, and the bits of the hit's flags byte above.

## Edge cases

Each path draws as its comment says, whatever the target's state. 210's amount is a word
difference, so a target above its maximum less the roll gives a wrapped amount. 211's damage is
the hit points the target has after effect 0x1B less the roll; a target with fewer hit points than
the roll gets a wrapped amount. 166 with the bytes unequal draws nothing, and applies its effect
only on a failed saving throw.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_575A_0043` does with the amount and what `fn_575A_008E` does with 213, what effects
  0x45 and 0x1B and the nineteen removed codes are, and which spells the `DATA` numbers are
  (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- What the values of `a` and `g` that callers pass on from their own arguments and locals are, and
  what the actor's byte `+0x15` and `a` stand for (FND-RNG-051, Q-MAGIC-016).
- What `EffectApplied` and `EffectRemoved` do in overlay 193 with their arguments (FND-RNG-047,
  Q-RNG-021).

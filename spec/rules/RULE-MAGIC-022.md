---
id: RULE-MAGIC-022
title: DATA 77 gives a dead party member a d8 origin byte, rerolled while 1 unless it was 1, a d2 gender byte and full hit points, and places it again
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-AI-004, FND-PARTY-081, FND-COMBAT-023, FND-PARTY-133, FND-RNG-041, FND-RNG-040, FND-ACTOR-003, FND-PARTY-013]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-MAGIC-021]
---

## Summary

`reincarnate_roll` is the case of the hit routine's effect switch for `DATA` 77. A target that is
a party slot whose `combat_mark` is 7 gets a new details byte `+0x12` from a d8, rolled again
while it comes up 1 unless the old byte was 1, and a new byte `+0x13` from a d2; origin 8 forces
byte `+0x13` to 2 and origin 7 to 1. Its combatant word `+0x10` becomes 13, 12 or twice the
origin plus byte `+0x13` less 3, its hit points its maximum and its `combat_mark` 1, and it is
placed again as a new object. Any other target shows `CANNOT REINCARNATE`.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with `DATA` 77 (FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `flags: UINT8[]`: the hit's flags byte.

## Inputs

`combatant_found` and `fn_2D40_3E64` of `t`; through `object_slot_combatants`, `t`'s `combatants`
record `R` and `combatant_details` record `D`: bytes `+0x12` and `+0x13` (`unk_10[2]`,
`unk_10[3]`) and `max_hit_points`; `combatants` record `t`'s word `+0x10`; `t`'s record of
`object_slots` and `g_57E0_67B7`.

## Procedure

```text
define may_reincarnate(t) -> UINT8:
    if not combatant_found(t) or t >= 4:
        return 0
    if combatants[fn_2D40_3E64(t)].combat_mark != 7:
        return 0
    return 1

define reincarnate_roll(t, flags):
    if may_reincarnate(t) == 0:
        emit MessageShown("CANNOT REINCARNATE")
        return
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    let old: UINT16 = D.unk_10[2]
    loop:
        # roll_sum draws once per pass (RULE-RNG-001)
        D.unk_10[2] = UINT8(roll_sum(1, 8))
        if old == 1 or D.unk_10[2] != 1:
            break
    # roll_sum draws once (RULE-RNG-001)
    D.unk_10[3] = UINT8(roll_sum(1, 2))
    if D.unk_10[2] == 8:
        D.unk_10[3] = 2
        R.unk_08_word_8 = 13
    else if D.unk_10[2] == 7:
        D.unk_10[3] = 1
        R.unk_08_word_8 = 12
    else:
        R.unk_08_word_8 = UINT16(2 * D.unk_10[2] + D.unk_10[3] - 3)
    R.hit_points = D.max_hit_points
    R.combat_mark = 1
    flags[0] = flags[0] | 0x02
    let s = object_slots[t]
    g_57E0_67B7[s.entry_index].byte_5 = g_57E0_67B7[s.entry_index].byte_5 & 0xEF
    let x = s.position_x
    let y = s.position_y
    # all 37 bytes of the slot become 0xFF
    for i in 0..37:
        s[i] = 0xFF
    fn_31E0_0121(combatants[t].unk_08_word_8 + 300, t, x, y, 0, 7, 0)
```

`R.unk_08_word_8` is the word at `+0x10` of the record, bytes 8 and 9 of `unk_08`. The last call
reads the word from the combatant record numbered `t`, not through `object_slot_combatants`, and
its result is not used. `+1373` then goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The new bytes and words, bit 0x02 of the hit's flags byte, the cleared bit, the object slot
filled with 0xFF and the new object, or the message.

## Edge cases

A d8 of 1 is rolled again until another value comes up, each roll a draw, unless the old byte was
1, in which case 1 is kept. Origins 7 and 8 still draw the d2 before forcing byte `+0x13`.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether bytes `+0x12` and `+0x13` are the origin and gender (FND-PARTY-070) and word `+0x10` an
  image or object number, what the filled slot and the bit of `g_57E0_67B7` change, and
  what `fn_31E0_0121` places with these arguments (FND-RNG-044, FND-RNG-047, Q-RNG-021).
- Whether `fn_2D40_3E64`, which this spec has not read, returns the index of `t`'s record of
  `combatants` here (FND-RNG-047,
  Q-RNG-021).

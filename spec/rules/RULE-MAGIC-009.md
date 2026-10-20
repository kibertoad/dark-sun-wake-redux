---
id: RULE-MAGIC-009
title: A target with bit 0x80 of its effect block byte 9 that saves against a caster of details kind 9 hands a 2d6 total to overlay 179 +1304 and queues DATA 308
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-112, FND-PARTY-099, FND-PARTY-107, FND-COMBAT-022, FND-EXPLORE-001, FND-EXPLORE-002, FND-PARTY-081, FND-RNG-044, FND-RNG-039, FND-COMBAT-023, FND-COMBAT-052]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`kind_9_save_roll` is a step of overlay 197 `+1760`. When the caster is found, overlay
`2D40:35EF` of the caster and the target is 1 or less, the target's effect block has bit 0x80 of
byte 9, the hit's mask has bit 0x100, the caster's details byte `+0x12` is 9 and the target passes
the saving throw overlay 179 `+2A46` with 38 and 6, it calls overlay 193 `+1AA4` with the target
and 0x4F, rolls 2d6, passes the total to overlay 179 `+1304` and queues `DATA` 308 from the target
at the caster. `+1760` then returns 0 after its last step, ending the hit.

## When it runs

In `+1760`, after the steps FND-PARTY-112 lists, for every hit the hit routine overlay 179 `+0D2F`
passes it (FND-RNG-040). A caster that `2D40:3E64` does not find, or for which `2D40:35EF` returns
more than 1, makes `+1760` return before this step.

## Parameters

- `target: INT16`: the target's object slot.
- `caster: INT16`: the caster's object slot.
- `mask: UINT16`: `+1760`'s word argument at `bp+0x14`, the hit's mask.

## Inputs

The target's `effect_block`; `combatant_details`: byte `+0x12` (`unk_10[2]` of FMT-COMBAT-002) of
the caster's details record; `combatants`: byte `+0x15` (`unk_15[0]` of FMT-COMBAT-001) of the
target's record through `object_slot_combatants`.

## Procedure

```text
define kind_9_save_roll(target, caster, mask: UINT16):
    if effect_block(target)[9] & 0x80 == 0 or mask & 0x100 == 0:
        return false  # +1760 goes on to its last step
    if combatant_details[combatant_details_of(caster)].unk_10[2] != 9:
        return false
    if UINT8(fn_56A7_0075(target, caster, 38, 6)) == 0:
        return false
    fn_573B_0057(target, 0x4F)
    # roll_sum draws twice (RULE-RNG-001); the total is passed as a word
    let total: UINT16 = roll_sum(2, 6)
    fn_56A7_0089(caster, target, -1, 0x200, total, 1, 6, combatants[object_slot_combatants[target]].unk_15[0])
    emit CastQueued(target, caster, 308, 6)
    return true  # +1760 returns 0 after its last step
```

## Outputs

True when `+1760` will return 0, which ends the hit (FND-PARTY-112); the call of overlay 179
`+1304` with the total; the `CastQueued` record.

## Edge cases

The total is 2 to 12. A target that fails the saving throw makes no roll.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What overlay 179 `+1304` (`fn_56A7_0089`) does with the total, and what `DATA` 308 does when the
  queue runs it, given that FND-PARTY-107 has the queue run overlay 193 `+003C` with a `DATA`
  number of -1 for such a record (FND-RNG-040, Q-MAGIC-009).
- What bit 0x80 of block byte 9, bit 0x100 of the mask, details byte `+0x12` 9 and
  `fn_2D40_35EF` stand for (FND-RNG-040, Q-MAGIC-008).
- What `fn_56A7_0075` returns for these arguments, what `fn_573B_0057` does with code 0x4F, and
  what the bytes of `effect_block` stand for (FND-PARTY-099, FND-RNG-039, Q-MAGIC-008).

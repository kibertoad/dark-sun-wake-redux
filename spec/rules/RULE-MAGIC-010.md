---
id: RULE-MAGIC-010
title: When the hit routine's byte flag is set, a target of details kind 8 and level 7 or more is spared on a d20 above 8, and one with bit 0x80 of effect block byte 5 queues DATA 307 instead
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-112, FND-PARTY-107, FND-PARTY-081, FND-COMBAT-022, FND-EXPLORE-002, FND-RNG-089, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`level_spared` is a step of overlay 197 `+0E8F`. With the hit routine's byte flag set, a target
whose effect block has bit 0x80 of byte 5 queues `DATA` 307 with itself as both slots and the hit
ends with no roll. Otherwise `kind_8_spared` (overlay 197 `+314E`) decides: for a target found
whose details byte `+0x12` is 8 and whose highest level is 7 or more it rolls a d20, and a total
above 8 ends the hit. With the flag clear, or when `kind_8_spared` returns false, `+0E8F` goes on.

## When it runs

In `+0E8F`, the first of the hit routine's gates, after its earlier tests (FND-PARTY-112). The
flag is `+0E8F`'s byte argument at `bp+0x10`, which the hit routine overlay 179 `+0D2F` passes
from its own byte at `bp+0x14` (FND-RNG-040).

## Parameters

- `target: INT16`: the target's object slot.
- `flag: UINT8`: the hit routine's byte at `bp+0x14`.

## Inputs

The target's `effect_block`; `combatant_details`: byte `+0x12` (`origin` of FMT-COMBAT-002) of
the target's details record.

## Procedure

```text
define level_spared(target, flag: UINT8):
    if flag == 0:
        return false  # +0E8F goes on
    if effect_block(target)[5] & 0x80 != 0:
        emit CastQueued(target, target, 307, 6)
        return true  # the hit ends
    return kind_8_spared(target)

define kind_8_spared(slot):
    if not combatant_found(slot):
        return false
    if combatant_details[combatant_details_of(slot)].origin != 8:
        return false
    if INT16(fn_27E5_0154(slot)) < 7:
        return false
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    return INT16(roll_sum(1, 20)) > 8
```

## Outputs

True when `+0E8F` returns 1, which ends the hit (FND-PARTY-112); false when it goes on.

## Edge cases

`kind_8_spared` spares 12 times in 20. `fn_27E5_0154` is position 0's level for an origin byte of
1 and otherwise the greatest level of the positions with a class (FND-PARTY-081).

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the hit routine's byte at `bp+0x14`, bit 0x80 of block byte 5, details byte `+0x12` 8 and
  `DATA` 307 stand for (FND-RNG-040, Q-MAGIC-008).
- Whether `+314E` is reached other than from `+0E8F`: its trampoline has no direct caller, and no
  computed call was searched for (FND-RNG-040, Q-MAGIC-008).
- What `fn_27E5_0154`'s level counts beyond FND-PARTY-081's reading, and what the bytes of
  `effect_block` stand for (FND-PARTY-081, FND-RNG-089, Q-MAGIC-008).

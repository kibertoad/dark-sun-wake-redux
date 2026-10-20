---
id: RULE-MAGIC-008
title: A target with bit 0x40 of its effect block byte 5 blocks a DATA record whose overlay 208 +0885 value is 8 or less, other than 235 to 268, on a d100 of 75 or less and queues DATA 307 on itself
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-112, FND-PARTY-107, FND-RNG-034, FND-RNG-039]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`blocked_roll` is a step of overlay 197 `+1760`. For a `DATA` number other than -1 whose value
from overlay 208 `+0885` is 8 or less and that is not 235 to 268, against a target whose effect
block has bit 0x40 of byte 5, it rolls a d100. On 75 or less it calls overlay 193 `+1AA4` with the
target and 0x2E, queues `DATA` 307 with the target as both slots, and `+1760` returns 0 at once,
ending the hit. Otherwise `+1760` goes on.

## When it runs

In `+1760`, after the steps FND-PARTY-112 lists, for every hit the hit routine overlay 179 `+0D2F`
passes it (FND-RNG-040).

## Parameters

- `target: INT16`: the target's object slot.
- `data_number: INT16`: the `DATA` number of the hit.

## Inputs

The target's `effect_block`.

## Procedure

```text
define blocked_roll(target, data_number: INT16):
    if data_number == -1:
        return false
    if INT16(fn_57A6_0020(data_number, 1)) > 8:
        return false
    if data_number >= 235 and data_number <= 268:
        return false
    if effect_block(target)[5] & 0x40 == 0:
        return false
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    if INT16(roll_sum(1, 100)) > 75:
        return false
    fn_573B_0057(target, 0x2E)
    emit CastQueued(target, target, 307, 6)
    return true  # +1760 returns 0
```

## Outputs

True when `+1760` returns 0 here, which ends the hit (FND-PARTY-112); false when it goes on to its
next step.

## Edge cases

The roll blocks 75 times in 100. `DATA` numbers 235 to 268 never take this step, whatever their
value from overlay 208 `+0885`.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What bit 0x40 of block byte 5, the value overlay 208 `+0885` gives (`fn_57A6_0020`), code 0x2E
  of overlay 193 `+1AA4` (`fn_573B_0057`) and `DATA` 307 stand for (FND-RNG-040, Q-MAGIC-008).
- What the bytes of `effect_block` stand for (FND-PARTY-112, FND-RNG-039, Q-MAGIC-008).

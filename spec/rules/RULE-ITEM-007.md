---
id: RULE-ITEM-007
title: A corroding hit destroys a picked held item when a d20 falls below 8 less the item's byte 0x16
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-ITEM-009, FND-COMBAT-023, FND-CONFIG-143, FND-PARTY-051]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-009]
---

## Summary

`weapon_corrodes` has overlay 173 `+2E1B` pick an entry of the target's held objects. With none
it returns 0 and makes no roll. Otherwise it rolls a d20; below 8 less the item's signed byte
`+0x16`, it shows that a weapon is corroded, removes the entry and returns 1, and otherwise it
returns 0.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 103, followed by
`armour_corrodes` when it returns 0, and for `DATA` 284 (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and the signed byte `+0x16` of the item's
record in `item_records`.

## Procedure

```text
define weapon_corrodes(slot):
    let list = fn_2D40_2196(slot)
    let e: INT16 = fn_5671_0084(-2, list, 1)
    if e == -1:
        return 0
    let b: INT16 = INT8(item_records[list[e].item][0x16])
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    if INT16(roll_sum(1, 20)) >= 8 - b:
        return 0
    emit MessageShown("Weapon is corroded")
    fn_2D40_2452(list, e)
    return 1
```

## Outputs

1 when the entry was removed, 0 otherwise. The switch then sets bit 0x02 of the hit's flags byte
on a 1 (FND-ITEM-010).

## Edge cases

Each call that finds an entry draws once. A byte `+0x16` of 0 removes the entry on 1 to 7, 7 chances
in 20; 7 or more never removes it, and -13 or less always does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_5671_0084` picks with its arguments -2 and 1, what `fn_2D40_2452` does to the removed
  entry's item, and what byte `+0x16` of an item record holds (FND-RNG-035, Q-ITEM-004).
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).

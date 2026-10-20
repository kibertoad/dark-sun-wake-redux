---
id: RULE-ITEM-008
title: DATA 235 hands the target's first held item of the lowest byte 0x12 below 6 to overlay 173 +2B88 when a d20 falls below 15 less the item's byte 0x16
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-CONFIG-143, FND-PARTY-051]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-007]
---

## Summary

`lowest_item_test` goes through the target's held-object entries of kind 1 and keeps the first
whose item has the lowest byte `+0x12` below 6. With none it returns 1 and makes no roll.
Otherwise it rolls a d20 and, below 15 less the item's signed byte `+0x16`, calls overlay 173
`+2B88` with the list, the item, the entry and 1. It returns 1 on every path.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 235, after queuing `DATA` 311
for the target (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and bytes `+0x12` (unsigned) and `+0x16`
(signed) of each listed item's record in `item_records`.

## Procedure

```text
define lowest_item_test(slot):
    let list = fn_2D40_2196(slot)
    let best: INT16 = -1
    let lowest: INT16 = 6
    for e in 0..count(list):
        if list[e].kind == 1 and INT16(item_records[list[e].item][0x12]) < lowest:
            best = e
            lowest = item_records[list[e].item][0x12]
    if best == -1:
        return 1
    # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
    if INT16(roll_sum(1, 20)) < 15 - INT16(INT8(item_records[list[best].item][0x16])):
        fn_5671_0075(list, list[best].item, best, 1)
    return 1
```

## Outputs

Always 1, so the switch always sets bit 0x02 of the hit's flags byte (FND-ITEM-010); and the call
of `fn_5671_0075` when the roll succeeds.

## Edge cases

A later entry with the same byte `+0x12` does not replace the first. Each call that finds an entry
draws once. A byte `+0x16` of 0 succeeds on 1 to 14; 14 or more never does, and -6 or less always
does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_5671_0075` does with the item, and what bytes `+0x12` and `+0x16` of an item record
  hold (FND-RNG-035, Q-ITEM-004).
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).

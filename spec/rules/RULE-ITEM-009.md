---
id: RULE-ITEM-009
title: A corroding hit destroys the first held item of type 0, 6 or 9 at once when its word 0x10 is 0, and otherwise on a d20 below 10 less that word
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-ITEM-009, FND-COMBAT-023, FND-CONFIG-143, FND-PARTY-051]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-ITEM-007]
---

## Summary

`armour_corrodes` goes through the target's held-object entries of kind 1 in order. For each whose
item has signed byte `+0x13` of 0, 6 or 9 it takes the item's word `+0x10`: 0 removes the entry
with no roll; any other value rolls a d20, which removes it when below 10 less that word. A removal
shows that armour is corroded and returns 1; a roll that does not remove goes on to the next entry.
After the last entry it returns 0.

## When it runs

The hit routine's effect switch runs it on the target for `DATA` 275 and 283, and for `DATA` 103
when `weapon_corrodes` returned 0 (FND-ITEM-010).

## Parameters

- `slot: INT16`: the target's object slot.

## Inputs

The held-object list `fn_2D40_2196` gives for the slot, and byte `+0x13` and word `+0x10`, both
signed, of each listed item's record in `item_records`.

## Procedure

```text
define armour_corrodes(slot):
    let list = fn_2D40_2196(slot)
    for e in 0..count(list):
        if list[e].kind != 1:
            continue
        let r = item_records[list[e].item]
        let t: INT16 = INT8(r[0x13])
        if t != 0 and t != 6 and t != 9:
            continue
        let w: INT16 = INT16(r.word(0x10))
        if w != 0:
            # roll_sum draws once (RULE-RNG-001); the total is compared as a signed word
            if INT16(roll_sum(1, 20)) >= 10 - w:
                continue
        emit MessageShown("Armor is corroded")
        fn_2D40_2452(list, e)
        return 1
    return 0
```

## Outputs

1 when an entry was removed, 0 otherwise. The switch then sets bit 0x02 of the hit's flags byte
on a 1 (FND-ITEM-010).

## Edge cases

A call draws once for each entry it reaches whose type matches and whose word is not 0, so it can
draw several times and still return 0. An item whose word `+0x10` is 0 is removed at once, before
any later entry. A word of 9 or more is never removed by the roll, and -11 or less always is.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_2D40_2452` does to the removed entry's item, which items byte `+0x13`'s values 0, 6
  and 9 are and what word `+0x10` holds (FND-RNG-035, Q-ITEM-004).
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).

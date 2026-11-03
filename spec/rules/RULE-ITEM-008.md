---
id: RULE-ITEM-008
title: DATA 235 hands the target's first held item of the lowest byte 0x12 below 6 to overlay 173 +2B88 when a d20 falls below 15 less the item's byte 0x16
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-ITEM-010, FND-CONFIG-143, FND-PARTY-051, FND-EXE-600, FND-PARTY-248]
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

The signed +0x16 byte is the item bonus used by the name formatter and
recorded combat consumers (FND-PARTY-248). Byte +0x12 remains the numeric
ranking input here; its complete writer and semantic contract is still open.

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

The recorded helper tests shared type byte +0x0B mask 0x02. With it set and
attached-item quantity word +2 nonzero, it subtracts its requested count
(one here) and retains the entry while the resulting quantity is above zero.
Otherwise it removes the entry through `fn_2D40_2452` and returns zero
(FND-EXE-600). This is the local branch description; the removal dispatch,
all input/writer domains and complete arithmetic contract remain open.

## Edge cases

A later entry with the same byte `+0x12` does not replace the first. Each call that finds an entry
draws once. A byte `+0x16` of 0 succeeds on 1 to 14; 14 or more never does, and -6 or less always
does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- `fn_5671_0075`'s removal effects, complete arithmetic/admission contract, and
  independent writers and uses of the +0x12 rank and +0x16 signed bonus
  (Q-ITEM-004). Its local quantity/flag branch is recorded above.
- Which entries `fn_2D40_2196` lists for a slot beyond their kind and item words, and what the
  other fields of `item_records` hold (FND-CONFIG-143, Q-ITEM-004).

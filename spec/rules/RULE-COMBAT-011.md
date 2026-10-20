---
id: RULE-COMBAT-011
title: After a swing with an item does damage, an eligible item breaks on one draw in eight followed by one in twenty
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-041, FND-PARTY-091]
conflicting: []
split_with: []
related: [RULE-COMBAT-002, RULE-RNG-001]
---

## Summary

Each time an attack with an item does damage, the game tests whether the item breaks. An item
can break only when its type's flags byte has bit 0x80 clear and its low four bits 0 to 3, and the
item's byte `+0x16` and word `+0x10` are 0; such an item breaks when a draw scaled to 0..7 gives 0
and a second draw scaled to 0..19 gives 0.
Every test makes one draw, and an eligible item makes a second on the first draw's 0.

## When it runs

`item_breaks` runs once after the attack routine's swing returns a value above 0, when the
attacker's entry holds an item (its number 0 or more), with that item and its type (FND-RNG-041,
FND-PARTY-091).

## Parameters

`type_flags`, byte `+4` of the item type's 15-byte record at `DS:19CD`. `item_bonus`, byte `+0x16`
of the item's 23-byte record at `DS:19C1`. `item_word`, word `+0x10` of that record.

## Inputs

None beyond the parameters.

## Procedure

```text
define item_breaks(type_flags, item_bonus, item_word):
    let eligible = 0
    if (type_flags & 0x80) == 0 and item_bonus == 0 and item_word == 0 and (type_flags & 0x0F) <= 3:
        eligible = 1
    # one draw of RULE-RNG-001's generator, scaled to 0..7
    if (INT32(draw()) * 8) / 32768 >= eligible:
        return false
    # a second draw, scaled to 0..19
    if (INT32(draw()) * 20) / 32768 >= eligible:
        return false
    return true
```

## Outputs

True when the item breaks. The game then shows a message naming the item and calls overlay 173
`+3E6B` with it (FND-RNG-041).

## Edge cases

An ineligible item never breaks and makes exactly one draw. An eligible item makes a second draw
on 4096 of the 32768 values of the first, and breaks on 1639 of the 32768 values of the second, a
chance of 6,713,344 in 1,073,741,824, about one in 160.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the type flags byte, the low four bits of it, the item's byte `+0x16` and word `+0x10`
  stand for, and what overlay 173 `+3E6B` does to the broken item (Q-COMBAT-031).

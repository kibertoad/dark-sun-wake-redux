---
id: RULE-COMBAT-012
title: A combatant flagged in the turn word rolls a d10 that lets it act, holds it, or gives it a random side byte
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-089, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-052, FND-COMBAT-022, FND-PARTY-112]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-COMBAT-013]
---

## Summary

At the start of its turn, a combatant whose turn word has bit 0x100 set has that bit cleared and
gets a result from `confusion_roll`. Bit 0x08 of byte 4 of its effect block gives 1. Otherwise,
with bit 0x80 of byte 0 the combatant gets a random side byte at once; with bit 0x20 of byte 2 it
rolls a d10, where 1 gives 1, 2 to 6 give 0, 7 to 9 give the random side byte and 10 gives 2; and
with neither bit it gets 2. The random side byte is a d2 stored in byte `+0x15` of its record,
with -1 stored in its second turn word, and gives the result 7. FND-RNG-089 reads the d10 as a
confusion table.

## When it runs

The turn routine overlay 173 `+1238` calls it for the combatant whose turn it is, when
`2D40:3E64` finds it and bit 0x100 of its word in `g_4C4F_01D8` is set, and stores the result as
a word in `DS:420C` (FND-RNG-089).

## Parameters

- `slot: INT16`: the combatant's object slot.

## Inputs

`object_slot_combatants`, `g_4C4F_01D8` and the slot's `effect_block`.

## Procedure

```text
define confusion_roll(slot):
    let c = object_slot_combatants[slot]
    g_4C4F_01D8[c] = g_4C4F_01D8[c] & 0xFEFF
    let block = effect_block(slot)
    if block[4] & 0x08 != 0:
        return 1
    if block[0] & 0x80 == 0:
        if block[2] & 0x20 == 0:
            return 2
        # roll_sum draws once (RULE-RNG-001); the low byte is compared, unsigned
        let r = UINT8(roll_sum(1, 10))
        if r == 1:
            return 1
        if r >= 2 and r <= 6:
            return 0
        if r > 9:
            return 2
    # roll_sum draws once (RULE-RNG-001)
    combatants[c].combatant_kind = UINT8(roll_sum(1, 2))
    g_4E71_0AC6[c] = -1
    return 7
```

## Outputs

The result, 0, 1, 2 or 7; byte `+0x15` of the combatant's record and its word in `g_4E71_0AC6`
on a result of 7; and bit 0x100 of its word in `g_4C4F_01D8`, always cleared.

## Edge cases

A combatant with bit 0x80 of block byte 0 makes one draw, for the side byte; one with only bit
0x20 of block byte 2 makes one draw, or two on a 7, 8 or 9. Bit 0x08 of block byte 4 takes
precedence over both and makes no draw. The bit is cleared before the block is read, also when
the result is 1 or 2.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What the results 0, 1, 2 and 7 make the turn routine do, what the bits of `effect_block` and
  the words of `g_4C4F_01D8` and `g_4E71_0AC6` stand for, which code sets bit 0x100, and what
  byte `+0x15` of the record holds (FND-RNG-089, Q-COMBAT-026).

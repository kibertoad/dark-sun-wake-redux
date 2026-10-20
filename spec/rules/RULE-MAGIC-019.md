---
id: RULE-MAGIC-019
title: DATA 26 and 150 remove each removable effect of the target on a d100 at most 50 plus 5 times a less 5 times the effect's level
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-044, FND-RNG-047, FND-PARTY-096, FND-COMBAT-022, FND-COMBAT-052, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`dispel_dice` is the case of the hit routine's effect switch for `DATA` 26 and 150. It first
applies effect 0x41 for 240 to a target whose details byte `+0x12` is 11 and whose combatant byte
`+0x0E` is 10, when there is a second slot. Then for each effect code from 0 to 112 set in the
target's effect bits, other than ten codes it never removes, it rolls a d100 and removes the
effect when the roll is at most `50 + 5 * a - 5 * fn_573B_0052(t, code)`.

## When it runs

When the hit routine overlay 179 `+0D2F` calls `+1373` with `DATA` 26 or 150 (FND-PARTY-096).

## Parameters

- `t: INT16`: the target's object slot.
- `c: UINT16`: the second slot.
- `n: UINT16`: the `DATA` number.
- `bits: UINT8[]`: the far pointer `+1373` takes at its `bp+0xA`, the target's effect bits.
- `flags: UINT8[]`: the hit's flags byte.
- `a: UINT8`: the hit routine's byte argument at its `bp+0x1C`.

## Inputs

Through `object_slot_combatants`, `t`'s `combatants` record: byte `+0x0E` (`unk_08[6]`); its
`combatant_details` record: byte `+0x12` (`unk_10[2]`).

## Procedure

```text
define dispel_dice(t, c, n, bits, flags, a):
    let R = combatants[object_slot_combatants[t]]
    let D = combatant_details[R.details_index]
    if D.unk_10[2] == 11 and R.unk_08[6] == 10 and c < 9999:
        emit EffectApplied(t, c, n, 0x41, 240, 0)
    for code in 0..113:
        if bits[code >> 3] & (1 << (code & 7)) == 0:
            continue
        if code in [4, 8, 27, 37, 58, 64, 66, 68, 71, 106]:
            continue
        let chance: INT16 = 50 + 5 * a - 5 * INT16(fn_573B_0052(t, code))
        # roll_sum draws once (RULE-RNG-001); signed word compare
        if chance >= INT16(roll_sum(1, 100)):
            emit EffectRemoved(t, code)
            flags[0] = flags[0] | 0x02
```

`+1373` then goes on to the steps after its switch (FND-PARTY-166).

## Outputs

The effect 0x41, the removed effects and bit 0x02 of the hit's flags byte when one is removed.

## Edge cases

One draw for each code tested, whether or not it is removed. The bits are read again for each
code, after any removal before it. A chance of 100 or more always removes and one below 1 never
does.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_573B_0052` returns for an effect, read here as its level, what `a`, effect 0x41 and
  the ten kept codes are, what the bit set holds, and which spells 26 and 150 are (FND-RNG-044,
  FND-RNG-047, Q-RNG-021).
- What `EffectApplied` and `EffectRemoved` do in overlay 193 with their arguments (FND-RNG-047,
  Q-RNG-021).

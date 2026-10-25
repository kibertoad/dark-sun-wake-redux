---
id: RULE-MAGIC-011
title: Check 4 of overlay 197 +1A94 passes a slot at once, or on a d100 below its preserver level when bit 0x40 of its effect block byte 7 is set
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-081, FND-PARTY-083, FND-PARTY-057, FND-COMBAT-023, FND-COMBAT-052, FND-COMBAT-022, FND-PARTY-112, FND-RNG-089]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`class_11_check` is case 4 of overlay 197 `+1A94`, the only case its direct callers use. It
returns 0 for a slot whose effect block has bit 0x20 of byte 8, whose details record numbered by
the slot has word `+0x0E` of 0, whose combatant record numbered by the slot has `combat_mark` other
than 1, whose byte in `g_4C4F_00B2` is not 0 while `combat_state` is not 0, or whose effect block
has any of bits 0x10 of byte 8, 0x10 or 0x80 of byte 0, or 0x08 or 0x20 of byte 4. Otherwise it
returns 1 when bit 0x40 of block byte 7 is clear, and when it is set rolls a d100 and returns 1
when the total is below the slot's level in class code 11, the preserver.

## When it runs

Overlay 172 `+0A73`, `+0D44` and `+1140` and overlay 211 `+159B` and `+1C46` call `+1A94` with
case 4, the slot and 0 (FND-RNG-040).

## Parameters

- `slot: INT16`: the object slot, used as a record number as well.

## Inputs

The slot's `effect_block`; `combatant_details`: word `+0x0E` (`unk_0E` of FMT-COMBAT-002) of record
`slot`; `combatants`: `combat_mark` of record `slot`; `g_4C4F_00B2`; `combat_state`.

## Procedure

```text
define class_11_check(slot):
    let block = effect_block(slot)
    if block[8] & 0x20 != 0:
        return 0
    if combatant_details[slot].unk_0E == 0:
        return 0
    if combatants[slot].combat_mark != 1:
        return 0
    if INT8(g_4C4F_00B2[slot]) != 0 and combat_state != 0:
        return 0
    if block[8] & 0x10 != 0 or block[0] & 0x10 != 0 or block[4] & 0x08 != 0 or block[4] & 0x20 != 0 or block[0] & 0x80 != 0:
        return 0
    if block[7] & 0x40 == 0:
        return 1
    # roll_sum draws once (RULE-RNG-001); signed word compare
    let roll: INT16 = INT16(roll_sum(1, 100))
    if roll < INT16(fn_27E5_00C1(slot, 11)):
        return 1
    return 0
```

## Outputs

1 or 0, as a byte.

## Edge cases

The records are numbered by the slot itself, without `object_slot_combatants` or the details
number in the combatant record, while `fn_27E5_00C1` finds the slot's records through them
(FND-PARTY-081); the two agree for a slot whose map entry and details number equal the slot. A
slot with bit 0x40 set and no preserver level never passes, and one with a preserver level of
`L` passes on `L - 1` rolls in 100.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What overlay 172 and overlay 211 do with the result, what the block bits, word `+0x0E` and
  `g_4C4F_00B2` stand for, and which slots the callers pass (FND-RNG-040, Q-MAGIC-010).
- What `fn_27E5_00C1` returns beyond FND-PARTY-081's reading, and what the bytes of
  `effect_block` stand for (FND-PARTY-081, FND-RNG-089, Q-MAGIC-010).

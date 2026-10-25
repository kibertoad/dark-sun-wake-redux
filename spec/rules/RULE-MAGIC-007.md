---
id: RULE-MAGIC-007
title: A target whose record byte 0x0E is 13 ends the hit of a DATA record with byte 0x17 of -1, and on a d6 of 6 the record is queued from the combatant whose turn it is at the target
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-040, FND-PARTY-112, FND-PARTY-122, FND-PARTY-107, FND-RNG-035, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-EXPLORE-001, FND-EXPLORE-002, FND-COMBAT-052]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`turned_back_roll` is a step of overlay 197 `+111A`. For a target whose combatant record byte
`+0x0E` is 13 and a `DATA` record whose byte `+0x17` is -1 and whose word `+1` is above -2, the
hit ends. A `DATA` number passed negated ends it with no roll; otherwise the step rolls a d6 and,
on a 6, queues the record with the combatant whose turn it is and the target before ending the
hit. Any other target or record lets `+111A` go on to its later tests.

## When it runs

In `+111A` after the tests FND-PARTY-112 lists have let it go on. The hit routine overlay 179
`+0D2F` calls `+111A` after `+0E8F` and `+1760`, and overlay 174 `+04D2` calls it too
(FND-RNG-040, FND-PARTY-122).

## Parameters

- `target: INT16`: the target's object slot.
- `data_number: INT16`: the `DATA` number as `+111A` is given it, negated when below -1.
- `level: UINT8`: `+111A`'s byte argument at `bp+0x14`, which goes into the queued record.

## Inputs

`combatants`: byte `+0x0E` (`special_attack` of FMT-COMBAT-001) of the target's record through
`object_slot_combatants`; bytes `+0x17` and word `+1` of `data_record`; `turn_combatant`.

## Procedure

```text
define turned_back_roll(target, data_number: INT16, level: UINT8):
    let n: INT16 = data_number
    let negated = false
    if n < -1:
        negated = true
        n = -n
    if combatants[object_slot_combatants[target]].special_attack != 13 or n == -1:
        return false  # +111A goes on to its later tests
    let d = data_record(n)
    if d[0x17] != 0xFF or INT16(d.word(1)) <= -2:
        return false
    if negated:
        return true  # the hit ends with no roll
    # roll_sum draws once (RULE-RNG-001); the total is compared as a word
    if roll_sum(1, 6) == 6:
        emit CastQueued(turn_combatant, target, n, level)
    return true  # the hit ends
```

## Outputs

True when `+111A` returns 1, which ends the hit (FND-PARTY-112); false when it goes on. On a 6 the
`CastQueued` record.

## Edge cases

The target takes nothing from the record on any roll; the roll decides only whether the record
is queued. A record numbered -1 never reaches the step.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What record byte `+0x0E` 13 and the `DATA` bytes the step tests stand for, and whether the queued
  record casts the spell back at its caster (FND-RNG-040, Q-MAGIC-008).

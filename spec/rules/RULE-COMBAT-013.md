---
id: RULE-COMBAT-013
title: A combatant passes a d100 check unless it rolls below a chance its record byte 0x19 sets, from 0 at 7 or less to 99 at 25
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-089, FND-COMBAT-052, FND-COMBAT-022]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-COMBAT-012]
---

## Summary

`byte_19_check` sets a chance from byte `+0x19` of a combatant's record, rolls a d100, and returns
0 when the roll is below the chance and 1 otherwise. The chance is 0 for a byte of 7 or less, 1
for 8 to 11, 2 for 12 and 13, 7 for 14 and 15, 10 for 16, 13 for 17, 25 for 18, 10 times the byte
less 14 for 19 to 23 and for 26 or more, 95 for 24 and 99 for 25.

## When it runs

The per-round effect routine overlay 195 `+00D1` calls it in three of its steps (FND-RNG-089).

## Parameters

- `combatant: INT16`: the combatant's record number; the routine indexes `combatants` with it
  directly.

## Inputs

`combatants`: byte `+0x19` of the record, `strength` of FMT-COMBAT-001.

## Procedure

```text
define byte_19_check(combatant):
    let b: UINT8 = combatants[combatant].strength
    let chance: INT16 = 0
    if b == 25:
        chance = 99
    else if b == 24:
        chance = 95
    else if b > 18:
        chance = 10 * (b - 14)
    else if b == 18:
        chance = 25
    else if b == 17:
        chance = 13
    else if b == 16:
        chance = 10
    else if b > 13:
        chance = 7
    else if b > 11:
        chance = 2
    else if b > 7:
        chance = 1
    # roll_sum draws once (RULE-RNG-001)
    if INT16(roll_sum(1, 100)) < chance:
        return 0
    return 1
```

## Outputs

0 or 1.

## Edge cases

Every call draws once. A byte of 7 or less always returns 1; 25 returns 0 on 99 rolls in 100 and
1 only on a 100. A byte of 26 or more gives a chance of 120 or more and always returns 0, unlike
24 and 25.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What byte `+0x19` holds, and what each of the three steps that call the check does with 0 and
  1 (FND-RNG-089, Q-COMBAT-027).

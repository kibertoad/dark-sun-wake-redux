---
id: RULE-MAGIC-006
title: Turn undead, DATA 306, destroys an undead target on a d20 at least three times its adjusted level less the adjusted caster level, plus 10
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-035, FND-RNG-044, FND-PARTY-086, FND-COMBAT-022, FND-PARTY-081, FND-EXPLORE-002, FND-COMBAT-023, FND-MAGIC-022, FND-MAGIC-023, FND-MAGIC-017]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

`DATA` 306 is the cleric classes' turn undead (FND-MAGIC-023). When the hit routine applies it to a
target whose details record has byte `+0x12` 9, it
rolls a d20 against a target number from the target's details byte `+0x1E` and a level byte the
hit routine passes. On a total at least the target number, the target takes 1,000 damage and the
hit's flags get bit 0x01; otherwise nothing happens. Byte `+0x12` 9 is the undead creature kind
and byte `+0x1E` the level of the first class position (FND-MAGIC-022). Any other target makes no
roll and nothing happens.

## When it runs

The spell window shows a `TURN UNDEAD` button, control 0x2BEF, for a member whose `class_flags`
has one of the four cleric bits, except while `DS:140C` is 0x44. Using it casts 306 from the member
at the greatest of its levels in class codes 1 to 4; the cast does not enter the recent-cast list
and spends nothing, and in a fight it then ends the turn of the slot at `DS:426D`. The hit routine's
gate overlay 208 `+0F07` ends the hit before the effect switch for a target whose details byte
`+0x12` is not 9. The case of `DATA` 306 in the effect switch calls this rule with the target, the
caster, the hit's flags and the hit routine's byte argument `a` as the level (FND-RNG-044,
FND-RNG-035, FND-MAGIC-017, FND-MAGIC-023).

## Parameters

- `target: INT16`: the target's slot.
- `caster: INT16`: the caster's slot.
- `flags`: the hit's flags, which the routine returns with bit 0x01 set on a success.
- `level: UINT8`: the level byte, the hit routine's `a`.

## Inputs

`combatant_details`: bytes `+0x12` and `+0x1E` of the target's details record, `unk_10[2]` and
`unk_10[14]` of FMT-COMBAT-002.

## Procedure

```text
define data_306_test(target, caster, flags, level: UINT8):
    let found = combatant_found(target)
    let d = combatant_details[combatant_details_of(target)]
    if not found or d.unk_10[2] != 9:
        return flags
    let h: INT16 = d.unk_10[14]
    if h > 4:
        h = h + 1
        if h > 13:
            h = 13
    let l: INT16 = level
    if l >= 14:
        l = 14
    if l > 10:
        l = l - 1
        if l > 12:
            l = l - 1
    let need: INT16 = 3 * (h - l) + 10
    if need == 22:
        need = 20
    # roll_sum draws once (RULE-RNG-001)
    if INT16(roll_sum(1, 20)) >= need:
        emit DamageDealt(target, 1000)
        return flags | 0x01
    return flags
```

## Outputs

The 1,000 damage, dealt through overlay 179 `+2A05` with `DATA` number -1 and code -1, and bit
0x01 of the hit's flags (FND-RNG-044). On a failed roll the routine calls `2D40:06E1`, which
returns at once (FND-MAGIC-022).

## Edge cases

`h` takes the values 0 to 4 unchanged, 5 to 12 as 6 to 13, and 13 or more as 13; `level` takes 0
to 10 unchanged, 11 as 10, 12 as 11, 13 as 12 and 14 or more as 12. A target number of 1 or less
always succeeds and one above 20 never does, though the roll is still made. The number 22, from
`h - l` of 4, is lowered to 20; 25 and 28, from `h - l` of 5 and 6, are not, and never succeed.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether a script casts `DATA` 306 by a computed number, and what region `DS:140C` 0x44 is, where
  the button is not shown (FND-MAGIC-023, Q-MAGIC-029).

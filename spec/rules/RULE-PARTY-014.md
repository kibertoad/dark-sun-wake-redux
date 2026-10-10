---
id: RULE-PARTY-014
title: Class flags, attack rate, THAC0 and saving throws from a character's classes and levels
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-074, FND-PARTY-081, FND-PARTY-083, FND-PARTY-085, FND-PARTY-091]
conflicting: []
split_with: []
related: [RULE-PARTY-013, RULE-COMBAT-002]
---

## Summary

From a character's classes and levels the game sets a flag word naming its classes, an attack
rate, a THAC0 and five saving throws. Each comes from the greatest level in each class group:
priest, warrior, wizard and rogue. A human's former classes count only while they are below the
current class, except for the saving throws.

## When it runs

Each function when a character from the generation screen is stored, when a human changes class,
and for each level gained in play, after the level's hit die (FND-PARTY-081, FND-PARTY-085).

## Parameters

For one character: `origin`, the `origin` code; `codes`, the stored class codes that are not 0, in
position order; `levels`, the level of each; `constitution`, the constitution score.

## Inputs

None beyond the parameters.

## Procedure

```text
define group_levels(origin, codes, levels, all_classes):
    let group: UINT8[8] = [0, 0, 1, 1, 2, 3, 1, 3]
    let top: UINT8[4] = [0, 0, 0, 0]
    for i in 0..count(codes):
        if all_classes or origin != 0 or i == 0 or levels[i] < levels[0]:
            let g = group[class_of(codes[i])]
            if levels[i] > top[g]:
                top[g] = levels[i]
    return top

define class_flags(origin, codes, levels):
    let flag: UINT16[18] = [0, 1, 2, 4, 8, 16, 16, 16, 16, 32, 64, 128, 256, 512, 512, 512, 512, 1024]
    let limit = 100
    if origin == 0:
        limit = levels[0]
    let flags = 0
    for i in 0..count(codes):
        if i == 0 or (levels[i] > 0 and levels[i] < limit):
            flags = flags | flag[codes[i]]
    return flags

define class_attack_rate(origin, codes, levels):
    let top = group_levels(origin, codes, levels, false)
    let rate = 2
    if top[1] > 0:
        rate = rate + 1
    if top[1] > 6:
        rate = rate + 1
    if top[1] > 12:
        rate = rate + 1
    return rate

define attack_rate(origin, codes, levels):
    if origin == 7:
        return 8
    return class_attack_rate(origin, codes, levels)

define thac0(origin, codes, levels):
    let rate: INT8[4] = [8, 12, 4, 6]
    let top = group_levels(origin, codes, levels, false)
    let best = 0
    for g in 0..4:
        best = max(best, (top[g] - 1) * rate[g] / 12)
    return 20 - best

define saving_throw(origin, codes, levels, constitution, s):
    let save_table: UINT8[4][5][3] = [
        [[10, 45, 2], [14, 45, 6], [13, 45, 5], [16, 45, 8], [15, 45, 7]],
        [[14, 69, 3], [16, 69, 5], [15, 69, 4], [17, 82, 4], [17, 69, 6]],
        [[14, 30, 8], [11, 40, 3], [13, 40, 5], [15, 40, 7], [12, 40, 4]],
        [[13, 25, 8], [14, 50, 4], [12, 25, 7], [16, 25, 11], [15, 50, 5]]]
    let top = group_levels(origin, codes, levels, true)
    let save = 99
    for g in 0..4:
        if top[g] > 0:
            let gain = save_table[g][s][1] * (top[g] - 1) / 100
            if gain > save_table[g][s][2]:
                gain = save_table[g][s][2]
            if g == 1 and (origin == 1 or origin == 5):
                gain = gain + constitution * 2 / 7
            let value = save_table[g][s][0] - gain
            if save > value:
                save = value
    return save
```

## Outputs

The original stores `class_flags` in the details record's word at `0x10`, `class_attack_rate` and
`attack_rate` in its bytes at `0x24` and `0x25`, `saving_throw` for saves 0 to 4 in its bytes at
`0x31` to `0x35`, and `thac0` in the combatant record's byte at `0x16`; for a thri-kreen it also
stores 2 in the details record's byte at `0x27`. No draws.

## Edge cases

The divisions round toward zero, so a group at level 0 gives a THAC0 term of 0. A save falls by
at most the third number of its entry: a warrior's saves stop falling between levels 6 and 10
(BUG-PARTY-004). A dwarf or halfling with a warrior class adds its constitution bonus to all five
saves of the warrior group and to none of the other groups' (BUG-PARTY-005). The best group
decides each save separately.

## What the sources say

RULE-COMBAT-002 gives the manual's account of THAC0. No source read for this entry gives the
saving throws or the attack rate.

## Differences between builds

None known.

## Open questions

- Which code reads the other values, and as what: the bytes at `0x24`, `0x25` and `0x27` as
  attacks and the five bytes at `0x31` as saving throws rest on the shape of the formulas; the
  combatant byte at `0x16` is read as the attacker's THAC0 (FND-PARTY-091, Q-PARTY-044).

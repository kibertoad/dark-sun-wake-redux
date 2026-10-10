---
id: RULE-PARTY-012
title: Hit points of a new character
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-065, FND-PARTY-070, FND-PARTY-071, FND-PARTY-073, FND-PARTY-074]
conflicting: []
split_with: []
related: [RULE-PARTY-002, RULE-PARTY-010, RULE-RNG-001, SCR-UI-004]
---

## Summary

On the character generation screen the player sets a new character's greatest hit points between
two bounds: one per level, and the class's hit die per level, each summed over the classes,
divided by their number, doubled for a half-giant and raised by a constitution bonus. Choosing a
class puts the hit points at the low bound if they were below it. The screen also rolls each
level's hit die into a separate total.

## When it runs

`hit_point_low` and `hit_point_high` whenever a class is chosen or removed, the constitution score
changes, or the screen rolls the scores; `step_hit_points` when the player presses the hit point
button; `rolled_hit_points` when a class is chosen or removed and on each roll with its flag set
(FND-PARTY-070, FND-PARTY-071, FND-PARTY-074).

## Parameters

`origin`, the character's `origin` code. `classes`, the list of the character's `character_class`
codes in the order the screen keeps them, and `levels`, the level of each, from RULE-PARTY-007's
starting levels as the screen sets them (FND-PARTY-065). `constitution`, the character's
constitution score. `floor_constitution`, the constitution held in the character's details
record, which the roll's floor reads (FND-PARTY-074). `hit_points`, the greatest hit points.
`step`, 1 or -1.

## Inputs

None beyond the parameters.

## Procedure

```text
define constitution_hit_bonus(classes, levels, constitution):
    let bonus_table: INT8[26] = [-3, -3, -2, -2, -1, -1, -1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 5, 6, 6, 6, 7, 7]
    # Per class code: 0 priest, 1 warrior, 2 preserver, 3 psionicist and thief
    let group: UINT8[8] = [0, 0, 1, 1, 2, 3, 1, 3]
    let last_level: UINT8[4] = [9, 9, 10, 9]
    let top = 0
    let warrior = 0
    for i in 0..count(classes):
        let level = min(levels[i], last_level[group[classes[i]]])
        if level > top:
            top = level
        if group[classes[i]] == 1 and level > warrior:
            warrior = level
    let bonus = bonus_table[constitution]
    return bonus * warrior + min(bonus, 2) * (top - warrior)

define hit_point_low(origin, classes, levels, constitution):
    let n = 0
    let total = 0
    for i in 0..count(classes):
        if levels[i] > 0:
            n = n + 1
            total = total + levels[i]
    if n == 0:
        return 0
    if origin == 4:
        total = total * 2
    return total / n + constitution_hit_bonus(classes, levels, constitution)

define hit_point_high(origin, classes, levels, constitution):
    let hit_die: UINT8[8] = [8, 8, 10, 10, 4, 6, 10, 6]
    let n = 0
    let total = 0
    for i in 0..count(classes):
        if levels[i] > 0:
            n = n + 1
            total = total + levels[i] * hit_die[classes[i]]
    if n == 0:
        return 0
    if origin == 4:
        total = total * 2
    return total / n + constitution_hit_bonus(classes, levels, constitution)

define clamp_hit_points(origin, classes, levels, constitution, hit_points):
    let low = hit_point_low(origin, classes, levels, constitution)
    let high = hit_point_high(origin, classes, levels, constitution)
    if hit_points < low:
        return low
    if hit_points > high:
        return high
    return hit_points

define step_hit_points(origin, classes, levels, constitution, hit_points, step):
    let low = hit_point_low(origin, classes, levels, constitution)
    let high = hit_point_high(origin, classes, levels, constitution)
    let value = hit_points + step
    if value > high:
        value = low
    if value < low:
        value = high
    return value

define rolled_hit_points(origin, classes, levels, floor_constitution):
    let hit_die: UINT8[8] = [8, 8, 10, 10, 4, 6, 10, 6]
    let after_last: UINT8[8] = [2, 2, 3, 3, 1, 2, 3, 2]
    let floor_table: UINT8[26] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 3, 4, 4, 4]
    let group: UINT8[8] = [0, 0, 1, 1, 2, 3, 1, 3]
    let last_level: UINT8[4] = [9, 9, 10, 9]
    let total = 0
    for i in 0..count(classes):
        for k in 1..levels[i] + 1:
            let gain = after_last[classes[i]]
            if k <= last_level[group[classes[i]]]:
                gain = roll_sum(1, hit_die[classes[i]])
                if floor_table[floor_constitution] > gain:
                    gain = floor_table[floor_constitution]
            if origin == 4:
                gain = gain * 2
            total = total + gain
    return total
```

## Outputs

`hit_point_low` and `hit_point_high` return the bounds; `clamp_hit_points` and `step_hit_points`
return the new greatest hit points, which become the current hit points too. `rolled_hit_points`
returns the total the screen keeps in the word at `0x0A` of the details record, making one draw
per level through `roll_sum`, class by class. None changes other state.

## Edge cases

The divisions round toward zero. A character with no class has both bounds 0. A new character's
hit points start at 0 when its portrait is chosen, so choosing a class sets them to the low bound.
At generation the levels are 6 or 7 (FND-PARTY-065), below every `last_level`, so `after_last`
never applies there. The original skips a human's level when its second or third class has a level
of at least that number, and skips the whole roll when the levels last rolled add up to more than
the current ones; neither happens at generation, where a human has one class and the screen
records the levels just before it rolls.

## What the sources say

No source read for this entry describes the generation screen's hit points or the classes' hit dice.

## Differences between builds

None known.

## Open questions

- Which constitution the roll's floor reads while a character is made: the details record's score
  is filled from the scores only at DONE (Q-PARTY-033).

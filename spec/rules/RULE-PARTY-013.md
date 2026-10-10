---
id: RULE-PARTY-013
title: How a character gains levels in play
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-057, FND-PARTY-058, FND-PARTY-074, FND-PARTY-081, FND-PARTY-083, FND-PARTY-084, FND-PARTY-085, FND-PARTY-086, FND-PARTY-089, FND-PARTY-088, FND-PARTY-090]
conflicting: []
split_with: []
related: [RULE-PARTY-012, RULE-PARTY-014, RULE-PARTY-015, RULE-RNG-001, FMT-PARTY-001]
---

## Summary

After experience is given, each party member gains levels in each class, or only in a human's
current class, while the experience reaches the class's next threshold, up to level 15. The
experience is first cut to the start of the level four above the class's level. Each level shows
a message, may add a hit die roll to the character's rolled total, and sets the greatest hit
points from that total; the current hit points keep their distance below the greatest. After its
classes the character's greatest psionic strength points are recomputed and raised if the result
is higher.

## When it runs

`gain_levels` with `one_each` false after each experience award of RULE-PARTY-015 unless the gain
is deferred, as it is while a fight goes on, and when a fight ends with a party member still
standing, after those members are brought back to at least 1 hit point (FND-PARTY-081,
FND-PARTY-086, FND-PARTY-089). With the debug flag set by the string `911`,
the key T gives each party member 3,750,000 experience and then runs `gain_levels` with
`one_each` false, and the key t runs it with `one_each` true (FND-PARTY-081). It runs for each
of the four party slots in order whose state byte at `4F49:0C33` plus 3 times the slot is 2,
whose `combat_mark` is at most 2 and whose details record's word at `0x0E` is not 0.

## Parameters

For one party member: `origin`, the `origin` code; `codes`, the stored class codes of
FMT-PARTY-001 `classes` that are not 0, in position order; `levels`, the level of each;
`greatest_levels`, FMT-PARTY-001 `greatest_levels` for each; `experience`; `hit_die_total`,
FMT-PARTY-001 `hit_die_total`; `constitution`, `intelligence` and `wisdom`, the character's
scores, 0 to 25; `screen_wisdom`, the wisdom in the combatant record the far pointer `DS:142D`
points to when the psionic points are recomputed: the character whose new Psionicist level came
last, in this member's gain or an earlier one, and otherwise the slot the game last made current,
a record another screen was given, or the generation screen's working record (FND-PARTY-076,
FND-PARTY-084, FND-PARTY-088, FND-PARTY-090, Q-PARTY-043); `hit_points` and `max_hit_points`; `psionic_points` and
`max_psionic_points`; `class_flags`, `class_attack_rate`, `attack_rate`, `natural_attack_rates`, `thac0` and
`saving_throws`, the FMT-PARTY-001 fields RULE-PARTY-014 sets; `name`, the character's name; `one_each`, true when each class gains one
level without the experience test.

## Inputs

None beyond the parameters.

## Procedure

```text
define class_of(code):
    let class: UINT8[18] = [0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 3, 4, 5, 6, 6, 6, 6, 7]
    return class[code]

define level_total(levels):
    let total = 0
    for i in 0..count(levels):
        total = total + levels[i]
    return total

define level_start(class, column):
    let table: UINT16[8][20] = [
        [0, 15, 30, 60, 130, 275, 550, 1100, 2250, 4500, 6750, 9000, 11250, 13500, 15750, 18000, 20250, 22500, 24750, 27000],
        [0, 20, 40, 75, 125, 200, 350, 600, 900, 1250, 6750, 9000, 11250, 13500, 15750, 18000, 20250, 22500, 24750, 27000],
        [0, 20, 40, 80, 160, 320, 640, 1250, 2500, 5000, 7500, 10000, 12500, 15000, 17500, 20000, 22500, 25000, 27500, 30000],
        [0, 22, 45, 90, 180, 360, 750, 1500, 3000, 6000, 9000, 12000, 15000, 18000, 21000, 20000, 22500, 25000, 27500, 30000],
        [0, 25, 50, 100, 200, 400, 600, 900, 1350, 2500, 3750, 7500, 11250, 15000, 18750, 22500, 26250, 30000, 33750, 37500],
        [0, 22, 44, 88, 165, 300, 550, 1000, 2000, 4000, 6000, 8000, 10000, 12000, 15000, 18000, 21000, 14000, 17000, 30000],
        [0, 22, 45, 90, 180, 360, 750, 1500, 3000, 6000, 9000, 12000, 15000, 18000, 21000, 24000, 27000, 30000, 33000, 36000],
        [0, 12, 25, 50, 100, 200, 400, 700, 1100, 1600, 2200, 4400, 6600, 8800, 11000, 13200, 15400, 17600, 19800, 22000]]
    return table[class][column] * 100

define level_hit_die(origin, codes, levels, greatest_levels, i, constitution):
    let hit_die: UINT8[8] = [8, 8, 10, 10, 4, 6, 10, 6]
    let after_last: UINT8[8] = [2, 2, 3, 3, 1, 2, 3, 2]
    let floor_table: UINT8[26] = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 3, 4, 4, 4]
    let group: UINT8[8] = [0, 0, 1, 1, 2, 3, 1, 3]
    let last_level: UINT8[4] = [9, 9, 10, 9]
    let k = levels[i]
    if origin == 0:
        for j in 1..count(levels):
            if levels[j] >= k:
                return 0
    if level_total(greatest_levels) > level_total(levels):
        return 0
    let class = class_of(codes[i])
    let gain = after_last[class]
    if k <= last_level[group[class]]:
        gain = roll_sum(1, hit_die[class])
        if floor_table[constitution] > gain:
            gain = floor_table[constitution]
    if origin == 4:
        gain = gain * 2
    return gain

define greatest_hit_points(origin, codes, levels, greatest_levels, hit_die_total, constitution):
    let held = level_total(levels)
    let reached = level_total(greatest_levels)
    let n = count(codes)
    if origin == 0:
        n = 1
    let classes = [class_of(c) for c in codes]
    let value = (held * hit_die_total) / reached / n + constitution_hit_bonus(classes, levels, constitution)
    if value < reached:
        value = reached
    return value

define psionicist_level(origin, codes, levels):
    for i in 0..count(codes):
        if class_of(codes[i]) == 5:
            if origin == 0 and i > 0 and levels[0] <= levels[i]:
                return 0
            return levels[i]
    return 0

define greatest_psionic_points(origin, codes, levels, constitution, intelligence, wisdom, screen_wisdom):
    let con_table: INT8[26] = [0, 0, 0, 0, 0, 1, 2, 3, 7, 8, 9, 10, 11, 12, 14, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2]
    let int_table: INT8[26] = [7, 8, 9, 10, 11, 12, 14, 0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 0, 20, 22, 24]
    let wis_table: INT8[26] = [1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3, 4, 5, 6, 0, 20, 22, 24, 26, 28, 30, 32, 34, 0, 1, 2]
    let wis_level_table: INT8[26] = [1, 2, 3, 4, 5, 6, 0, 20, 22, 24, 26, 28, 30, 32, 34, 0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 10]
    let total = 0
    if constitution >= 15:
        total = total + con_table[constitution]
    if intelligence >= 15:
        total = total + int_table[intelligence]
    if wisdom >= 15:
        total = total + wis_table[screen_wisdom]
    let p = psionicist_level(origin, codes, levels)
    if p > 0:
        total = total + (p - 1) * 10
        if wisdom >= 15:
            total = total + wis_level_table[wisdom] * (p - 1)
    else if origin == 0:
        total = total + levels[0] * 4
    else:
        let top = 0
        for i in 0..count(levels):
            top = max(top, levels[i])
        total = total + top * 4
    return total

define gain_one_level(member, i):
    emit LevelGainShown(member.name, member.levels[i] + 1, member.codes[i])
    # visible: the message
    let distance = member.max_hit_points - member.hit_points
    member.levels[i] = member.levels[i] + 1
    member.hit_die_total = member.hit_die_total + level_hit_die(member.origin, member.codes, member.levels, member.greatest_levels, i, member.constitution)
    if member.greatest_levels[i] < member.levels[i]:
        member.greatest_levels[i] = member.levels[i]
    member.class_flags = class_flags(member.origin, member.codes, member.levels)
    member.class_attack_rate = class_attack_rate(member.origin, member.codes, member.levels)
    member.attack_rate = attack_rate(member.origin, member.codes, member.levels)
    if member.origin == 7:
        member.natural_attack_rates[1] = 2
    member.thac0 = thac0(member.origin, member.codes, member.levels)
    for s in 0..5:
        member.saving_throws[s] = saving_throw(member.origin, member.codes, member.levels, member.constitution, s)
    member.max_hit_points = greatest_hit_points(member.origin, member.codes, member.levels, member.greatest_levels, member.hit_die_total, member.constitution)
    member.hit_points = member.max_hit_points - distance
    # visible: the level and hit points

define gain_levels(member, one_each):
    let positions = count(member.codes)
    if member.origin == 0:
        positions = min(positions, 1)
    let i = 0
    while i < positions:
        let class = class_of(member.codes[i])
        let cap = level_start(class, member.levels[i] + 3)
        if cap < member.experience:
            member.experience = cap
        if (one_each or level_start(class, member.levels[i]) <= member.experience) and member.levels[i] < 15:
            gain_one_level(member, i)
            if not one_each:
                continue
        i = i + 1
    let points = greatest_psionic_points(member.origin, member.codes, member.levels, member.constitution, member.intelligence, member.wisdom, member.screen_wisdom)
    if member.max_psionic_points < points:
        member.psionic_points = member.psionic_points + points - member.max_psionic_points
        member.max_psionic_points = points
    # visible: the psionic points
```

## Outputs

The member's `experience`, `levels`, `greatest_levels`, `hit_die_total`, `max_hit_points`,
`hit_points`, `max_psionic_points`, `psionic_points` and the values of RULE-PARTY-014. One `LevelGainShown` per level gained, and
one draw through `roll_sum` for each level whose hit die is rolled, in the order the levels are
gained.

## Edge cases

The experience is cut even when no level is gained. Where a row rises, one run gains a class at
most four levels and loses the experience above the start of the fourth; for a level-14
psionicist the cut is below the start of level 15, so it never gains level 15 this way
(BUG-PARTY-003). A human gains levels only in its current class. Several classes share one
experience, so the cut for one class lowers what the later classes are tested against.

A level regained after the level drain of overlay 210 `+0B66` rolls another hit die when the
level totals meet, since the drain leaves `greatest_levels` and `hit_die_total` as they were
(FND-PARTY-082). The divisions in `greatest_hit_points` are unsigned and round down; the product
is taken to 16 bits, which the levels and dice of a character do not exceed. When the greatest hit
points fall, the current hit points fall by the same amount. The psionic points are never
lowered. A character whose wisdom is 15 or more takes the wisdom bonus from `screen_wisdom`, so a
level in another class gains less, or nothing, when the record last made current has a wisdom
below 15 (FND-PARTY-084).

## What the sources say

No source read for this entry describes how levels are gained in play.

## Differences between builds

None known.

## Open questions

- Which character `screen_wisdom` belongs to after a fight: the original reads the wisdom bonus's
  index from the record at `DS:142D`, which is the levelling character's only after a new
  Psionicist level or when that character was made current last; the end of a fight was not found
  to change it, and the fight routine changes it only inside the gain (FND-PARTY-088,
  FND-PARTY-090); whether the code that runs between its calls during a fight moves it was not
  read (Q-PARTY-043).
- What else a level changes: for a new Preserver or Psionicist level the original calls overlay
  209 routines, and before the gain it calls overlay 199 `+0C21`, none of which this entry covers
  (Q-PARTY-039).
- That the messages and draws happen in this order in play: a capture of a party gaining levels
  after a fight would confirm it (Q-PARTY-037).

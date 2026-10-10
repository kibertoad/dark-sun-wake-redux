---
id: RULE-PARTY-010
title: How the generation screen rolls and bounds ability scores
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-065, FND-PARTY-070, FND-PARTY-071]
conflicting: []
split_with: []
related: [RULE-PARTY-002, RULE-PARTY-005, RULE-RNG-001, SCR-UI-004]
---

## Summary

On the character generation screen each ability score lies between a minimum its classes set and
20 plus its origin modifier. Each class asks 17 of its prime ability and its own figure of every
other score, and a character with several classes meets the highest demand on each. A roll gives
4 plus the origin modifier plus the best of four rolls of four four-sided dice, raised to the
minimum. The score buttons step a score by one, wrapping between the bounds.

## When it runs

`roll_scores` when the player chooses a portrait, presses the reroll button (six times, keeping
the last), or presses DONE with no class (FND-PARTY-070); `clamp_scores` when the player chooses
or removes a class; `step_score` when the player presses a score's button with a class chosen
(SCR-UI-004, FND-PARTY-071).

## Parameters

`origin`, the character's `origin` code. `classes`, the list of the character's `character_class`
codes (RULE-PARTY-002), in the order the screen keeps them. `ability`, a score's position in the
order strength, dexterity, constitution, intelligence, wisdom, charisma, 0 to 5. `scores`, the six
scores in that order. `step`, 1 or -1.

## Inputs

None beyond the parameters.

## Procedure

```text
define score_minimum(classes, ability):
    # Per class code: the prime ability, and the minimum of every other score
    let prime: UINT8[8] = [4, 4, 0, 0, 3, 4, 4, 1]
    let other_minimum: UINT8[8] = [9, 12, 9, 13, 9, 12, 14, 9]
    let lowest = 0
    for each c in classes:
        let demand = other_minimum[c]
        if prime[c] == ability:
            demand = 17
        if demand > lowest:
            lowest = demand
    return lowest

define score_maximum(origin, ability):
    return 20 + origin_modifier(origin, ability)

define roll_scores(origin, classes, scores):
    if count(classes) == 0:
        return scores
    let result = scores
    for ability in 0..6:
        let best = 0
        for attempt in 0..4:
            let roll = roll_sum(4, 4) + 4 + origin_modifier(origin, ability)
            if roll > best:
                best = roll
        let lowest = score_minimum(classes, ability)
        if best < lowest:
            best = lowest
        result[ability] = best
    return result

define clamp_scores(origin, classes, scores):
    if count(classes) == 0:
        return scores
    let result = scores
    for ability in 0..6:
        if scores[ability] < score_minimum(classes, ability):
            result[ability] = score_minimum(classes, ability)
        else if scores[ability] > score_maximum(origin, ability):
            result[ability] = score_maximum(origin, ability)
    return result

define step_score(origin, classes, scores, ability, step):
    if count(classes) == 0:
        return scores
    let result = scores
    let value = scores[ability] + step
    if value > score_maximum(origin, ability):
        value = score_minimum(classes, ability)
    if value < score_minimum(classes, ability):
        value = score_maximum(origin, ability)
    result[ability] = value
    return result
```

## Outputs

`score_minimum` and `score_maximum` return the bounds. `roll_scores`, `clamp_scores` and
`step_score` return the new six scores, which the screen stores in the character's combatant
record. None changes other state; `roll_scores` makes 16 draws per score through `roll_sum`, in
the order of the abilities.

## Edge cases

The class codes in `prime` and `other_minimum` are the `character_class` codes, one less than the
screen's numbering (FND-PARTY-073). The prime ability is wisdom for cleric, druid, psionicist and
ranger, strength for fighter and gladiator, intelligence for preserver and dexterity for thief.

A roll never passes the maximum, since four four-sided dice give at most 16. No set of classes the
screen offers (RULE-PARTY-009) has a minimum above a maximum (FND-PARTY-071). A character with no
class keeps its scores, and DONE cannot be pressed then (RULE-PARTY-002).

## What the sources say

SRC-MANUAL-1994, pages 19 to 22, gives lower minimums for each class (RULE-PARTY-002 lists them),
and page 16 gives scores from 9 to 24; it does not describe the roll.

## Differences between builds

None known.

## Open questions

- Whether the routines a roll calls after the hit points draw random numbers, which would follow
  the scores' and the hit dice's draws (Q-PARTY-034).
- Which input makes `step` 1 and which -1: the screen takes 1 when a word of the event is below 8
  (Q-PARTY-031).

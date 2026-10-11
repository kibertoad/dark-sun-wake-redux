---
id: RULE-PARTY-004
title: A human can change class twice, from second level
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-161, FND-PARTY-081, FND-PARTY-160, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002, SCR-UI-015, RULE-PARTY-010]
---

## Summary

Only a human can become dual-classed: at second level or higher in the current class, with 15
or more in the current class's prime ability and 17 or more in the new class's, the human takes
up a new class at first level and stops advancing in the old one. The new class must come from a
class group the character has never held, a ranger must be good and a druid true neutral. A human
can do this twice, for three classes in all. The manual adds that the old class's abilities come
back once the new class's level is higher than the old one's.

## When it runs

`can_change_class` runs for each class when the player picks DUAL for a character on the View
Character screen (SCR-UI-002), and the DUAL window offers the classes for which it is true
(SCR-UI-015). `former_class_usable` runs whenever the game decides whether a dual-classed human
may use a former class's abilities (SRC-MANUAL-1994, pages 9 and 19).

## Parameters

`origin`, the character's `origin` code (RULE-PARTY-002). `careers`, the list of the character's
`character_class` codes, the current one first and then the former ones from the latest.
`current_level`, the character's level in its current class. `scores`, the six current ability
scores in the order strength, dexterity, constitution, intelligence, wisdom, charisma.
`alignment`, the character's `alignment` code, 0 to 8. `new_class`, the `character_class` code
the player would take up. `former_level`, the level the character reached in a former class.

## Inputs

None beyond the parameters.

## Procedure

```text
define can_change_class(origin, careers, current_level, scores, alignment, new_class):
    # Per class code: the prime ability's position in scores, and the class group
    # (FND-PARTY-161).
    let prime: UINT8[8] = [4, 4, 0, 0, 3, 4, 4, 1]
    let group: UINT8[8] = [1, 1, 2, 2, 4, 8, 2, 16]
    if origin != 0:
        return false
    if scores[prime[careers[0]]] < 15:
        return false
    if scores[prime[new_class]] < 17:
        return false
    if count(careers) >= 3:
        return false
    if current_level < 2:
        return false
    let held = 0
    for each c in careers:
        held = held | group[c]
    if held & group[new_class] != 0:
        return false
    if new_class == 6 and alignment != 0 and alignment != 3 and alignment != 6:
        return false
    if new_class == 1 and alignment != 4:
        return false
    return true

define former_class_usable(former_level, current_level):
    return current_level > former_level
```

## Outputs

Each function returns true or false and changes no state. Taking up the new class puts it first
in `careers` at level 1 and keeps the former classes' levels; only the current class gains levels
afterwards (FND-PARTY-081, FND-PARTY-160). A new cleric, druid or ranger also gets an elemental
sphere, chosen from a menu that gives air when closed without a choice (FND-PARTY-161).

## Edge cases

Every class is in its own group, so neither the current class nor a former one is ever offered,
and a cleric can never add druid or the reverse; a fighter, gladiator or ranger can add none of
the other two. The other minimum scores of RULE-PARTY-010 are not tested: a human with 17
strength and 15 wisdom as a cleric can become a fighter whatever its other scores. When no class
passes, the game shows `NO OTHER CLASSES AVAILABLE` instead of the window.

## What the sources say

SRC-MANUAL-1994, page 9: DUAL is offered only for a human, who must be at least third level in
the current class. Page 19: a human may become dual-classed; the new class starts at first
level, the old class no longer advances, and the old class's benefits return once the new level
exceeds the old one; a character may do this once more, for at most three classes. The manual
names no score, group or alignment condition. The game allows the change from second level
(FND-PARTY-161).

## Differences between builds

None known.

## Open questions

- Whether the game holds back a former class's abilities until the current class's level exceeds
  it (Q-PARTY-067).
- How a dual-classed character's levels, experience and former classes are stored
  (FMT-PARTY-001, Q-PARTY-003).

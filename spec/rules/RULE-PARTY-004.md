---
id: RULE-PARTY-004
title: A human can change class twice, from third level
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [SCR-UI-002]
---

## Summary

Only a human can become dual-classed: at third level or higher in the current class, the human
takes up a new class at first level and stops advancing in the old one. The old class's
abilities come back once the new class's level is higher than the old one's. A human can do this
twice, for three classes in all.

## When it runs

`can_change_class` runs when the player picks DUAL for a character on the View Character screen
(SCR-UI-002), and `former_class_usable` whenever the game decides whether a dual-classed human
may use a former class's abilities (SRC-MANUAL-1994, pages 9 and 19).

## Parameters

`origin`, the character's `origin` code (RULE-PARTY-002). `careers`, the list of the character's
`character_class` codes in the order the character took them, the current one last.
`current_level`, the character's level in its current class. `former_level`, the level the
character reached in a former class.

## Inputs

None beyond the parameters.

## Procedure

```text
define can_change_class(origin, careers, current_level):
    if origin != 0:
        return false
    if count(careers) >= 3:
        return false
    return current_level >= 3

define former_class_usable(former_level, current_level):
    return current_level > former_level
```

## Outputs

Each function returns true or false and changes no state. Taking up the new class starts it at
level 1 and leaves the former class's level as it was.

## Edge cases

A human whose new class level equals the former level still cannot use the former class's
abilities. A human with three classes cannot change again.

## What the sources say

SRC-MANUAL-1994, page 9: DUAL is offered only for a human, who must be at least third level in
the current class. Page 19: a human may become dual-classed; the new class starts at first
level, the old class no longer advances, and the old class's benefits return once the new level
exceeds the old one; a character may do this once more, for at most three classes.

## Differences between builds

None known.

## Open questions

- Which classes the DUAL list offers: whether a new class must meet its minimum scores of
  RULE-PARTY-002, and whether the current class or a former one may be picked (Q-PARTY-007).
- How a dual-classed character's levels, experience and former classes are stored
  (FMT-PARTY-001, Q-PARTY-003).

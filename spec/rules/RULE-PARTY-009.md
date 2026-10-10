---
id: RULE-PARTY-009
title: Which classes the generation screen offers
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-058, FND-PARTY-063, FND-PARTY-065, FND-PARTY-068]
conflicting: []
split_with: []
related: [RULE-PARTY-002, RULE-PARTY-007, SCR-UI-004]
---

## Summary

On the character generation screen a character with no class may take any class its origin
allows; a character with one or two classes may add only the classes the class combination table
lists for its origin and the classes it has, in the order it took them; a character with three
may add none. A human takes one class. A class already taken can always be removed, and the
classes after it move forward.

## When it runs

Each time the screen sets a class or changes the origin, before it draws the eight class buttons
(SCR-UI-004, FND-PARTY-063).

## Parameters

`origin`, the character's `origin` code, 0 to 7. `classes`, the list of the character's
`character_class` codes (RULE-PARTY-002), zero to three, in the order the screen keeps them.

## Inputs

`class_combinations`, `DATA` 1001 of `RESOURCE.GFF` (FMT-PARTY-007).

## Procedure

```text
define classes_offered(origin, classes):
    # One bit per class, 0x80 cleric to 0x01 thief, in the order of the origin codes
    let origin_classes: UINT8[8] = [0xFF, 0xB5, 0xBF, 0xFF, 0xB6, 0xF7, 0xF5, 0xF6]
    let n = count(classes)
    if n == 0:
        return origin_classes[origin]
    if n == 1:
        return class_combinations[origin][classes[0]][0]
    if n == 2:
        return class_combinations[origin][classes[0]][classes[1] + 1]
    return 0

define class_offered(origin, classes, character_class):
    if character_class in classes:
        return true
    return (classes_offered(origin, classes) >> (7 - character_class)) & 1 == 1

define remove_class(classes, character_class):
    # The classes after the removed one move forward
    return [c for c in classes if c != character_class]
```

## Outputs

`classes_offered` returns the set of classes that may be added, one bit per class.
`class_offered` returns true when the class's button can be pressed, either to add the class or
to remove it. `remove_class` returns the list after removing a class. None changes state.

## Edge cases

The screen numbers the classes 1 to 8 (FND-PARTY-063); the procedure uses the `character_class`
codes 0 to 7 in the same order, and `class_combinations` is indexed as FMT-PARTY-007 gives it, by the first
class's code and then 0 or the second class's code plus 1.

Every human row of `class_combinations` is 0, so a human never has a second class. No row offers a Gladiator,
and the Gladiator rows are 0, so a Gladiator never has another class; no row offers Druid with
Cleric, and none Fighter with Ranger. A half-giant has at most two classes. Removing a class never
leaves an order the procedure would not have offered: every order reachable by adding offered
classes and removing any passes `classes_offered` for each class given the ones before it
(FND-PARTY-068).

`origin_classes` gives no class to an origin where README table 3 gives no level limit, and every
class to one where it gives a limit (RULE-PARTY-007).

## What the sources say

SRC-MANUAL-1994, page 8, says humans take one class and other origins up to three, and that
cleric and druid cannot be combined; it lists no other combinations. SRC-README-1.1, table 3,
gives which classes each origin may take (RULE-PARTY-007).

## Differences between builds

None known.

## Open questions

- Whether the words at `4E68:0000` are written after load: a search for stores to them would
  settle that the screen uses the shipped values (Q-PARTY-029).
- Whether DONE or the class minimum scores refuse a set of classes the buttons offered
  (Q-PARTY-002).

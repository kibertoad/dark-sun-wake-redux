---
id: RULE-PARTY-005
title: Origin ability modifiers
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Every origin but human raises some ability scores and lowers others. A half-giant, for example,
gains 4 strength and 2 constitution and loses 2 intelligence, wisdom and charisma.

## When it runs

When a character of that origin is generated (SRC-MANUAL-1994, page 77). When during generation,
and whether again after editing, is not known.

## Parameters

`origin`, the character's `origin` code (RULE-PARTY-002). `ability`, an ability's position in the
order strength, dexterity, constitution, intelligence, wisdom, charisma, 0 to 5.

## Inputs

None beyond the parameters.

## Procedure

```text
define origin_modifier(origin, ability):
    # Six modifiers per origin, in the order of the ability positions
    let modifier: INT8[48] = [0, 0, 0, 0, 0, 0, 1, -1, 2, 0, 0, -2, 0, 2, -2, 1, -1, 0, 0, 1, -1, 0, 0, 0, 4, 0, 2, -2, -2, -2, -2, 2, -1, 0, 2, -1, 2, 0, 1, -1, 0, -2, 0, 2, 0, -1, 1, -2]
    return modifier[origin * 6 + ability]
```

## Outputs

The modifier, from -2 to 4, added to the rolled score. No state changes.

## Edge cases

Human modifiers are all 0. Whether a modified score is held within 9 to 24 (RULE-PARTY-002) is
not known.

## What the sources say

SRC-MANUAL-1994, page 77, the racial ability adjustments table: dwarf strength +1, dexterity -1,
constitution +2, charisma -2; elf dexterity +2, constitution -2, intelligence +1, wisdom -1;
half-elf dexterity +1, constitution -1; half-giant strength +4, constitution +2, intelligence -2,
wisdom -2, charisma -2; halfling strength -2, dexterity +2, constitution -1, wisdom +2, charisma
-1; mul strength +2, constitution +1, intelligence -1, charisma -2; thri-kreen dexterity +2,
intelligence -1, wisdom +1, charisma -2. It gives no row for humans.

## Differences between builds

None known.

## Open questions

- When the game applies the modifiers, whether a player's edits are made before or after them,
  and whether the result is held within 9 to 24 (Q-PARTY-002).

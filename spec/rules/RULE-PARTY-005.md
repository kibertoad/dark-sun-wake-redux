---
id: RULE-PARTY-005
title: Origin ability modifiers
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, FND-PARTY-071]
conflicting: []
split_with: []
related: [RULE-PARTY-010]
---

## Summary

Every origin but human raises some ability scores and lowers others. A half-giant, for example,
gains 4 strength and 2 constitution and loses 5 dexterity and intelligence and 3 wisdom and
charisma. The generation screen adds the modifier to each rolled score and to the highest a score
may be (RULE-PARTY-010).

## When it runs

Whenever the generation screen rolls, bounds or steps a score (RULE-PARTY-010, FND-PARTY-071).

## Parameters

`origin`, the character's `origin` code (RULE-PARTY-002). `ability`, an ability's position in the
order strength, dexterity, constitution, intelligence, wisdom, charisma, 0 to 5.

## Inputs

None beyond the parameters.

## Procedure

```text
define origin_modifier(origin, ability):
    # Six modifiers per origin, in the order of the ability positions
    let modifier: INT8[48] = [0, 0, 0, 0, 0, 0, 1, -1, 2, 0, 0, -2, 0, 2, -2, 1, -1, 0, 0, 1, -1, 0, 0, 0, 4, -5, 2, -5, -3, -3, -2, 2, -1, 0, 2, -1, 2, 0, 1, -1, 0, -2, 0, 2, 0, -1, 1, -2]
    return modifier[origin * 6 + ability]
```

## Outputs

The modifier, from -5 to 4. No state changes.

## Edge cases

Human modifiers are all 0. The game keeps the table as six signed bytes per origin at `4E4F:0120`
plus 6 times the origin byte, which counts from 1 (FND-PARTY-071).

## What the sources say

SRC-MANUAL-1994, page 77, the racial ability adjustments table: dwarf strength +1, dexterity -1,
constitution +2, charisma -2; elf dexterity +2, constitution -2, intelligence +1, wisdom -1;
half-elf dexterity +1, constitution -1; half-giant strength +4, constitution +2, intelligence -2,
wisdom -2, charisma -2; halfling strength -2, dexterity +2, constitution -1, wisdom +2, charisma
-1; mul strength +2, constitution +1, intelligence -1, charisma -2; thri-kreen dexterity +2,
intelligence -1, wisdom +1, charisma -2. It gives no row for humans. Every row matches the
executable's but the half-giant's, where the executable has dexterity -5 (the manual gives none),
intelligence -5, wisdom -3 and charisma -3 (FND-PARTY-071).

## Differences between builds

None known.

## Open questions

None.

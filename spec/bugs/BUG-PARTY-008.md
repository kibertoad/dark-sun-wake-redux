---
id: BUG-PARTY-008
title: A target with no fighter, gladiator or ranger level saves against CHAOS on every roll but a 1, because the 100 the modifier subtracts wraps in the saving throw's byte arithmetic
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-164, FND-PARTY-099, FND-PARTY-162]
conflicting: []
split_with: []
related: [RULE-PARTY-014]
---

## Symptom

`CHAOS` affects a fighter, gladiator or ranger on a failed saving throw as usual, while every
other target saves against it unless it rolls a 1.

## Trigger conditions

A saving throw against `DATA` 56, `CHAOS`, by a target for which overlay 176 `+076C` returns no
level above 0 for the class codes 9, 10 and 13 to 16 (FND-PARTY-164).

## Mechanism

The saving throw adds the low byte of overlay 197 `+0206`'s modifier to a roll byte from 2 to 19
or 4 to 38, and fails only when the sum is below the save as an unsigned byte (FND-PARTY-099,
FND-PARTY-164). For `CHAOS`, `+0206` subtracts 100 from the modifier when the target has no
warrior level. The sum then lies near 0x9C to 0xC2, above every save, so the save succeeds.

The subtraction reads as meant to deny the save to non-warriors, which would give the opposite
result; nothing read for this entry says so, hence `intent: unclear`. The warrior levels come
from `+076C`, which counts a dual-classed human's held-back former class (FND-PARTY-162).

## Frequency

Every saving throw against `CHAOS` by a target with none of those classes.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether a save that succeeds spares the target from `CHAOS`, and whether the terms of
  `27E5:02F2`, `27E5:0324` and `27E5:030B` can bring the sum back below a save: neither was read
  (Q-PARTY-068).

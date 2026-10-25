---
id: BUG-PARTY-008
title: A target with no fighter, gladiator or ranger level saves against CHAOS on every roll but a 1, because the 100 the modifier subtracts wraps in the saving throw's byte arithmetic
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-164, FND-PARTY-166, FND-PARTY-167, FND-PARTY-168, FND-PARTY-099, FND-PARTY-258]
conflicting: []
split_with: []
related: [RULE-PARTY-014]
---

## Symptom

`CHAOS` affects a fighter, gladiator or ranger on a failed saving throw as usual, while every
other target saves against it, and so escapes its effect, unless it rolls a 1.

## Trigger conditions

A saving throw against `DATA` 56, `CHAOS`, by a target for which overlay 176 `+076C` returns no
level above 0 for the class codes 9, 10 and 13 to 16 (FND-PARTY-164).

## Mechanism

The saving throw adds the low byte of overlay 197 `+0206`'s modifier to a roll byte from 2 to 19
or 4 to 38, and fails only when the sum is below the save as an unsigned byte (FND-PARTY-099,
FND-PARTY-164). For `CHAOS`, `+0206` subtracts 100 from the modifier when the target has no
warrior level. The other terms move the sum by at most 39 upward (FND-PARTY-166), so the byte
lies between about 0x8D and 0xD6, above every save, and the save succeeds. The hit routine adds
`CHAOS`'s effect, type 21, only on a failed save (FND-PARTY-166). The one term that lowers the
modifier by more than a few points, twice the count of a level drain on the target, takes off
at most 30, since every source gives that count 15 or less (FND-PARTY-167, FND-PARTY-168).

The subtraction reads as meant to deny the save to non-warriors, which would give the opposite
result; nothing read for this entry says so, hence `intent: unclear`. The warrior levels come
from `+076C`, which counts a dual-classed human's held-back former class (FND-PARTY-258).

## Frequency

Every saving throw against `CHAOS` by a target with none of those classes.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None.

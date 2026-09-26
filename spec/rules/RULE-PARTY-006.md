---
id: RULE-PARTY-006
title: START GAME supplies characters 40 to 43 as the party
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-013, FND-PARTY-020, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [FMT-PARTY-001, SCR-UI-001]
---

## Summary

START GAME begins with a party the game supplies: the four characters stored on the disc under
the numbers 40, 41, 42 and 43, in that order.

## When it runs

When a game begins from START GAME on the start window (SCR-UI-001) [SRC-MANUAL-1994,
FND-PARTY-020].

## Parameters

`slot`, a party slot, 0 to 3.

## Inputs

None beyond the parameters.

## Procedure

```text
define supplied_character(slot):
    return 40 + slot
```

For each slot from 0 to 3, the game loads the FMT-PARTY-001 character record numbered
`supplied_character(slot)` into that slot, with the character's `PSIN`, `PSST` and `SPST`
records of the same number [FND-PARTY-013].

## Outputs

The number of the character record for the slot. No state changes.

## Edge cases

The numbers are computed from 40 and the slot, so the executable holds no list of them
(FND-PARTY-007). The disc's archive holds a second set, 50 to 53, which this path does not use
(FND-PARTY-005).

## What the sources say

SRC-MANUAL-1994, page 2, says START GAME begins play with a party that has already been made,
and does not name its members.

## Differences between builds

None known.

## Open questions

- Which code runs the loading routine of FND-PARTY-013, and whether START GAME reaches it. The
  owner's captures of a game started with START GAME show the four characters in this order,
  but they do not tell 41 from 53 or 43 from 33, so they agree with the routine without proving
  that it is the one START GAME runs (FND-PARTY-020, Q-PARTY-010, Q-PARTY-001).
- What START GAME does when `CHARSAVE.GFF` or one of the four records is missing
  (FND-PARTY-008, Q-PARTY-010).

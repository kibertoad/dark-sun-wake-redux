---
id: RULE-PARTY-006
title: START GAME supplies characters 40 to 43 as the party
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-PARTY-013, FND-PARTY-021, FND-PARTY-020, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [FMT-PARTY-001, SCR-UI-001]
---

## Summary

START GAME begins with a party the game supplies: the four characters stored on the disc under
the numbers 40, 41, 42 and 43, in that order.

## When it runs

When a game begins from START GAME on the start window (SCR-UI-001) [SRC-MANUAL-1994,
FND-PARTY-020]. The Start Game branch calls the overlay 182 routine that runs the loader only
while the placed-object count (`DS:264E`) is 0 [FND-PARTY-021].

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

- Whether the gate routine reaches its gate with the placed-object count at 0, so that the
  loader runs. FND-PARTY-021 shows START GAME reaching the loader only through that gate, after
  overlay 187 entries and three early returns. For the reading that it does: the count and the
  word at `DS:0DAB` start at 0 (FND-PARTY-022, FND-PARTY-024), and no direct call made between
  program start and the gate reaches a routine that changes the count or makes that word nonzero
  (FND-PARTY-026). Still open: 56 indirect calls on those paths whose targets are unresolved
  (FND-PARTY-026, FND-PARTY-027, Q-PARTY-011), and whether either of the gate routine's two
  video-memory reservations fails, which depends on the reservations held when it runs
  (FND-PARTY-025, FND-PARTY-027, FND-PARTY-028, Q-PARTY-013). The owner's captures of a game started with START GAME show the four characters
  in this order, but they do not tell 41 from 53 or 43 from 33 (FND-PARTY-020); the
  shipped-party live session would confirm it (Q-PARTY-001).
- What START GAME does when `CHARSAVE.GFF` or one of the four records is missing. The loader
  skips the second load for a slot whose first load returns `0xFFFF` but still places the slot
  (FND-PARTY-013), and the far load routine returns `0xFFFF` when its resource lookup fails
  (FND-PARTY-023); what the lookup does for a missing archive or record is unread (FND-PARTY-008,
  Q-PARTY-012).

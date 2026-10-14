---
id: RULE-PARTY-001
title: A party has one to four characters
status: disputed
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: [FND-PARTY-170]
split_with: []
related: [SCR-UI-002, SCR-UI-003, SCR-UI-004]
---

## Summary

A party holds up to four characters, and a created party can set out with any number from one to
four. The manual recommends four.

## When it runs

`party_can_start` runs when the player leaves the View Character screen of a new party to begin
play, and `party_has_room` when the player picks NEW or ADD on an empty character box
(SRC-MANUAL-1994, pages 7 and 10).

## Parameters

`member_count`, the number of characters in the party, 0 to 4.

## Inputs

None beyond the parameters.

## Procedure

```text
define party_can_start(member_count):
    return member_count >= 1 and member_count <= 4

define party_has_room(member_count):
    return member_count < 4
```

## Outputs

Each function returns true or false and changes no state.

## Edge cases

An empty party cannot set out, and a party of four offers no empty box for a fifth character.
The game keeps four slots for the party's characters and their `PSIN`, `PSST` and `SPST`
records (FND-PARTY-012), and loads four characters for START GAME (RULE-PARTY-006).

## What the sources say

SRC-MANUAL-1994 says on page 2 that START GAME begins with a party the game supplies, and on page
7 that a party has one to four characters and that four is best. Pages 9 and 10 describe the
View Character screen's four character boxes, where right-clicking an empty box offers NEW, ADD
and CANCEL.

## Differences between builds

None known.

## Open questions

- Disputed: FND-PARTY-170 contradicts When it runs and the first edge case. Leaving the View
  Character screen of a new party, by its return button or Esc, brings back the start window while
  `DS:0DAB` is 0 and begins no play, and START GAME with nothing placed loads characters 40 to 43
  (RULE-PARTY-006), so no code found refuses an empty party; the player who adds no character gets
  the supplied party. The reading rests on `DS:0DAB` staying 0 during party creation, which
  FND-PARTY-046's runs do not rule out, and does not cover a party emptied with `DROP` after `ADD`,
  which leaves the placed-object count nonzero: whether that reaches START GAME with no character
  in a box is Q-PARTY-006.
- Whether a party of fewer than four plays differently (Q-PARTY-070).

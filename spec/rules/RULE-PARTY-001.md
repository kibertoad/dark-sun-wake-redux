---
id: RULE-PARTY-001
title: A party has one to four characters
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
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

- What the game does when the player tries to begin with no characters, and whether a party of
  fewer than four behaves differently in play. No code for the check has been located
  (Q-PARTY-006).

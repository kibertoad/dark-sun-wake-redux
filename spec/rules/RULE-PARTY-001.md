---
id: RULE-PARTY-001
title: A party has one to four characters
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [EXP-PARTY-001, EXP-PARTY-002, FND-PARTY-170, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-PARTY-006, SCR-UI-001, SCR-UI-002, SCR-UI-003, SCR-UI-004]
---

## Summary

A party holds up to four characters. START GAME begins play with the characters placed in the
party boxes, any number from one to four, each in its own box, or with the party the game supplies
when no box holds a character. The manual recommends four.

## When it runs

`party_has_room` when the player picks NEW or ADD on an empty character box (SRC-MANUAL-1994,
pages 7 and 10), and `starting_party` when the player picks START GAME on the start window
(EXP-PARTY-001, EXP-PARTY-002). Leaving the View Character screen begins no play: it brings back the start window
(FND-PARTY-170).

## Parameters

`member_count`, the number of characters in the party, 0 to 4. `placed`, the four party boxes in
order, each holding a stored character or nothing.

## Inputs

None beyond the parameters.

## Procedure

```text
define party_has_room(member_count):
    return member_count < 4

define starting_party(placed):
    if no box of placed holds a character:
        return [supplied_character(slot) for slot in 0 to 3]    # RULE-PARTY-006
    return placed                                               # each character stays in its box
```

## Outputs

`party_has_room` returns true or false. `starting_party` returns the four boxes play begins with.
Neither changes state.

## Edge cases

A party of four offers no empty box for a fifth character. A party emptied with `DROP` after `ADD`
counts as empty: START GAME then begins play with the supplied party, as it does when nothing was
added (EXP-PARTY-001). A party of one or two plays with each character in the box it was added to,
which need not be box 0 (EXP-PARTY-001, EXP-PARTY-002).
A party of fewer than four plays under the same rules, except that a kill's experience is divided
among the filled slots (RULE-PARTY-015); no other code found counts the filled slots
(FND-PARTY-173).
The game keeps four slots for the party's characters and their `PSIN`, `PSST` and `SPST`
records (FND-PARTY-012).

## What the sources say

SRC-MANUAL-1994 says on page 2 that START GAME begins with a party the game supplies, and on page
7 that a party has one to four characters and that four is best. Pages 9 and 10 describe the
View Character screen's four character boxes, where right-clicking an empty box offers NEW, ADD
and CANCEL. The manual does not say what START GAME does with a created party.

## Differences between builds

None known.

## Open questions

- What makes `leader` name a filled box once play begins: it was 0 until START GAME, then 2 for a
  party in box 2 alone (EXP-PARTY-001) and 1 for a party in boxes 1 and 3 (EXP-PARTY-002), which
  fits the lowest filled box (Q-PARTY-071).
- Whether a created party of three or four, or one made with NEW, begins play the same way; the
  experiments added one and two stored characters (No item: the same mechanism as EXP-PARTY-002,
  with no reading that predicts a difference).

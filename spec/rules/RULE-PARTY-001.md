---
id: RULE-PARTY-001
title: A party has one to four characters
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [EXP-PARTY-001, EXP-PARTY-002, FND-PARTY-170, FND-PARTY-174, FND-PARTY-183, FND-PARTY-184, FND-PARTY-185, SRC-MANUAL-1994]
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
which need not be box 0 (EXP-PARTY-001, EXP-PARTY-002). `leader` stays 0 on the Create
Characters screen; once play begins it names the lowest filled box, set by the routine that
restores the party's combatants, which picks the lowest filled slot whose `combat_mark` was 1, 2
or 3, provided one slot marked 1 or 2 has hit points (FND-PARTY-174, FND-PARTY-185). An added character arrives with the `combat_mark` its record
holds, 0 becoming 1 (FND-PARTY-183, FND-PARTY-184), and every shipped stored character holds 0
or 1.
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

- What START GAME's party restore does when no filled slot with `combat_mark` 1 or 2 has hit
  points: it then sets no leader and calls overlay 201 `+0410` (FND-PARTY-185), which opens window
  19502 with a picture (`BMP` number `DS:4458`, or 10004), takes one event and closes it
  (FND-PARTY-194); whether it waits for a click, and what the game does after, are not read
  (Q-PARTY-078).
- Whether a party can reach START GAME in that state. Added characters arrive marked 1 unless
  their record holds 2 or more (FND-PARTY-183); a character made with NEW is stored with mark 0,
  and leaving View Character with Esc and START GAME's gate routine both turn a 0 into 1
  (FND-PARTY-192, FND-PARTY-193), so a party of one made with NEW begins play with that slot as
  leader (FND-PARTY-192). Left: whether any route before START GAME gives a filled slot a mark of
  3 or more or hit points below 1. The damage routine overlay 173 `+22C4` is the only store
  through the combatant table pointer that writes such a mark, and a static walk reaches it from
  the title screen's windows (FND-PARTY-197). The key `c` opens View Character's overlay 211 USE
  window on Create Characters (FND-PARTY-198), and `set_leader` reaches the attack routine only
  during a fight, which the fight state there rules out unless something sets it (FND-PARTY-199);
  whether casting from that window can run the damage routine there, what sets the fight state,
  and the other writes, are open (Q-PARTY-073, Q-PARTY-079,
  Q-PARTY-080).
- Whether a created party of three or four begins play the same way; the experiments added one
  and two stored characters and FND-PARTY-192 made one with NEW (No item: the same mechanism as
  EXP-PARTY-002, with no reading that predicts a difference).

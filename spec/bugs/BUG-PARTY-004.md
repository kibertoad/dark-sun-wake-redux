---
id: BUG-PARTY-004
title: Saving throws stop improving after a few levels, because the last value of each save is used as the most it can fall
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-085]
conflicting: []
split_with: []
related: [RULE-PARTY-014]
---

## Symptom

Some saving throws improve for a few levels and then stay where they are. Within the 15 levels a
class can reach, a warrior's five saves stop at 11, 11, 11, 13 and 11 from levels 6, 9, 7, 6 and
10; a priest's saves 0, 1 and 2 stop at 8 from levels 6, 15 and 13; a wizard's saves 1, 2 and 4 at
8 from levels 9, 14 and 11; and a rogue's saves 1 and 4 at 10 from levels 9 and 11.

## Trigger conditions

Any character whose level in a class group is high enough for the save's fall to reach the third
number of its entry. The saves are set when a character is stored from the generation screen,
when a human changes class and at each level gained in play (FND-PARTY-085).

## Mechanism

Each entry of the save table holds a starting value, a fall in hundredths per level above the
first, and a third number. For all 20 entries the starting value less the fall at the group's top
level (19 for a priest, 17 for a warrior, 21 for a wizard or rogue) equals the third number, so it
reads as the save's last value. The code instead caps the fall at the third number, taking the
smaller of the fall and that number before subtracting it from the starting value (overlay 210
`+05BA..+0624`, FND-PARTY-085).

## Frequency

Every character whose best group for a save is at or above the level where that save stops, as
listed under Symptom; the other saves keep falling up to level 15.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the cap is a slip: the match of all 20 third numbers with the last values favours one,
  but the code cannot show intent and no source discusses the saves (No item: the code cannot
  show intent).
- Which code reads the five saves, and so whether they act as saving throws in play: the searches
  of FND-PARTY-092 and FND-PARTY-093 found no reader, so the bug may have no effect in play (Q-PARTY-044).

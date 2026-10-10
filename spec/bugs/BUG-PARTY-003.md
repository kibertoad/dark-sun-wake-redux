---
id: BUG-PARTY-003
title: A psionicist at level 14 never reaches level 15 in play, and every award cuts its experience back to 1,400,000
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-081, FND-PARTY-058, FND-PARTY-083]
conflicting: []
split_with: []
related: [FMT-PARTY-001]
---

## Symptom

A character whose psionicist class reaches level 14 stays at level 14 however much experience it
earns. Each time levels are checked after an award, its experience drops to 1,400,000, below the
1,500,000 that level 15 needs.

## Trigger conditions

A party member with the Psionicist class (code 12) at level 14 when the level gain of overlay 210
`+0B44` runs, after experience is given. For a human the class must be the current one, since
the gain looks only at a human's first position. The debug key that gives one level per class
(t, FND-PARTY-081) raises the level without the experience test.

## Mechanism

Before testing a class, the level gain cuts the experience to the `DATA` 1000 word three columns
past the level, times 100, when the experience is above it; it then raises the level while the
experience reaches the word at the level, times 100 (FND-PARTY-081). The Psionicist reads row 5
(FND-PARTY-083), whose column 17 holds 14,000 and column 14 holds 15,000, while the columns before
and after rise (FND-PARTY-058). At level 14 the cut leaves 1,400,000 and the test needs 1,500,000,
so the level never rises and the experience above 1,400,000 is lost on every check.

## Frequency

Every level check for a level-14 psionicist, in a single-class or multi-class character alike.
With several classes the cut also lowers the experience the other classes are tested against.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether 14,000 is a slip: columns 17 and 18 of row 5 (14,000 and 17,000) are below column 16
  (21,000), and the only other row out of order is row 3 at column 15 (No item: the data cannot
  show intent and no source discusses it).
- What a level-14 psionicist shows after an award: a capture of the sheet's experience and
  level before and after a fight would confirm it (Q-PARTY-037).

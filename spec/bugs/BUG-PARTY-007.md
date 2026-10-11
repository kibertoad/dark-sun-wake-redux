---
id: BUG-PARTY-007
title: A dual-classed human keeps a former druid class's saving throw bonus against fire and lightning while the former class is held back
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-163, FND-PARTY-162, FND-PARTY-099]
conflicting: []
split_with: []
related: [RULE-PARTY-004]
---

## Symptom

A human who was a druid and changed class gets 2 added to its saving throws against fire and
lightning effects at once, although the game withholds the druid's other abilities until the new
class's level is higher than the druid level.

## Trigger conditions

A party member whose origin is human, with a druid class (codes 5 to 8) at a position other than
0 whose level is not below the current class's level, making a saving throw against an effect
whose `DATA` word at `+0x1A` has a bit of 0x82, such as `FIREBALL` or `LIGHTNING BOLT`
(FND-PARTY-163).

## Mechanism

The saving throw adds the modifier of overlay 197 `+0206` to its roll (FND-PARTY-099). That
routine gives 2 when overlay 176 `+076C` returns a druid level above 0, and `+076C` returns the
level of any position holding the class, with no test of the origin (FND-PARTY-163). The class
flags, `27E5:00C1` and the spell routines leave such a former class out for a human
(FND-PARTY-162).

## Frequency

Every saving throw against those effects by a human who has dual-classed out of druid, until the
new class passes the druid level. The change needs druid level 2 or more (RULE-PARTY-004), so the
bonus lasts at least until the new class reaches level 3.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether other uses of overlay 176 `+076C` let a held-back former class act in the same way
  (Q-PARTY-067).

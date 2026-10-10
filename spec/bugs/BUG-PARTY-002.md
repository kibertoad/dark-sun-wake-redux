---
id: BUG-PARTY-002
title: A character made with two classes never gets the extra starting level in its first class, and its empty third class can get level 1
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-PARTY-073, FND-PARTY-065]
conflicting: []
split_with: []
related: [FMT-PARTY-001]
---

## Symptom

A character made in generation with two classes starts at level 6 in each, with the experience of
the greater of the two level-6 thresholds. The second class goes up to level 7 when that
experience reaches its level-7 threshold, and the first class never does, even when the
experience reaches its own level-7 threshold. The level byte of the unused third class can also
become 1.

## Trigger conditions

Choosing a second class on the generation screen, or any later change that leaves two classes,
since every class change reruns the starting levels (FND-PARTY-065). The missed level shows only
when the first class's level-7 threshold is not above the second class's level-6 threshold: with
`DATA` 1000, that holds for a Druid first (level 7 at 35,000) with a Gladiator or Ranger (level 6
at 36,000) or a Preserver (level 6 at 40,000) second, and for a Thief first (level 7 at 40,000)
with a Preserver second. Which of these pairs the generation screen allows is not shown
here (RULE-PARTY-007).

## Mechanism

Overlay 183 `+1323` keeps the classes present as a mask with position 0 in bit 2 and position 2 in
bit 0, the order overlay 183 `+08DB` builds it in, and the first pass over the positions tests the
mask that way. The pass that adds the extra level tests bit `1 << position`, the reverse order.
With two classes, which `+0A3E` puts in positions 0 and 1 (mask 6), it skips position 0 and takes
position 2, whose class byte is 0, so it reads row -1 of `DATA` 1000 (the 40 bytes before the
resource) at column 0, and adds 1 to that position's level byte if the word there, times 100, is
not above the experience (FND-PARTY-065). Storing the character keeps the level byte and turns
the class byte 0 into 0 (FND-PARTY-073).

## Frequency

Every time the starting levels are set for a character with two classes. The missed level shows
for the four pairs above; whether the empty position gets level 1 depends on what precedes the
resource in memory.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the reverse order is a slip or meant to favour the later class: three classes take every
  position in both orders, and the empty position has no row, which favours a slip (No item: the
  code cannot show intent and no source discusses it).
- What a made character shows: a capture of the generation screen's levels for a Druid with a
  Preserver second, and of the stored record's level bytes, would confirm the missed level and the
  third level byte (Q-PARTY-027).

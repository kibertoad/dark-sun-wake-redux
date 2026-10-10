---
id: BUG-PARTY-006
title: The level drain ends the game with Math Err for a character whose classes all get their level back
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: crash
intent: unintended
player_reliance: unknown
evidence: [FND-PARTY-082, FND-PARTY-105, FND-PARTY-096, FND-PARTY-108, FND-PARTY-109, FND-PARTY-110]
conflicting: []
split_with: []
related: [RULE-PARTY-013, FMT-PARTY-001]
---

## Symptom

When a party member is struck by a level drain, the game can return to DOS at once, printing
`Math Err`. Nothing since the last save is kept.

## Trigger conditions

The drain of overlay 210 `+0B66` runs on a party member in slot 0 to 3 that receives the effect
code 59 (FND-PARTY-096, FND-PARTY-108). It ends the game whenever its second pass raises every
class of the member back to its old level, so that it rolls no hit die: always for a character
with one class above level 1, and for a character with several classes when the new experience
is at least the start of each class's old level (FND-PARTY-082).

## Mechanism

The drain sets the experience from the class thresholds, lowers each level by 1, and then raises
each level whose start the experience reaches, counting a hit die roll for each level it leaves
lowered. It then divides the total of those rolls by their count with a signed 16-bit `idiv`
(FND-PARTY-082). With no roll the count is 0 and the processor raises a divide error. The game's
handler for that error, installed at start-up, restores the screen, prints `Math Err` and exits
with code 1 (FND-PARTY-105).

## Frequency

Every drain of a single-class character above level 1. The sources of effect code 59 are a strike
by the item of `DATA` 313 on an attack roll of 20, the spells `DATA` 104 and 225 on a target that
fails its saving throw, and scripts through function 23 of opcode `0x22` (FND-PARTY-096,
FND-PARTY-108). Both spells are cast at a single target picked with the map cursor, which takes a
companion of the caster while a Shift key is held (FND-PARTY-109). The party's cast list holds
104 for a member who knows it and 225 for a druid of priest level 12 or more (FND-PARTY-110), so
such a member can drain another; whether one can also have spells of that level to cast, and
which of the sources can strike the party in play otherwise, is open.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether any enemy in play casts the spell `DATA` 104 or 225 at a party member, carries the item
  of `DATA` 313, or runs a script that sends code 59 to one, so that the crash can happen in a
  normal game: the casters are Q-PARTY-052, and the scripts are Q-PARTY-051.
- Whether a party member can cast the spell `DATA` 104 or 225 at a companion, which the target
  cursor allows while a Shift key is held (FND-PARTY-109) and the cast list allows for a member
  who knows 104 or a druid who can cast level 7 spells (FND-PARTY-110): it needs a nonzero count
  of spells to cast at that level (Q-PARTY-057).
- Whether the hit routine's calls before its saving throw can stop the drain for a party member
  (Q-PARTY-056).
- What a player sees on the screen as the game ends: a capture of a drain on a single-class
  character would confirm it (No item: no live session can be arranged until an enemy that
  drains is known; Q-PARTY-052 comes first).

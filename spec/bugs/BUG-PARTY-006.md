---
id: BUG-PARTY-006
title: The level drain ends the game with Math Err for a character whose classes all get their level back
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
impact: crash
intent: unintended
player_reliance: unknown
evidence: [FND-PARTY-082, FND-PARTY-105, FND-PARTY-096, FND-PARTY-108, FND-PARTY-109, FND-PARTY-110, FND-PARTY-111, FND-PARTY-112, FND-PARTY-113, FND-PARTY-114, FND-PARTY-115, FND-PARTY-116, FND-PARTY-119, FND-PARTY-120, FND-PARTY-121, FND-PARTY-122, FND-PARTY-237, FND-PARTY-241, FND-PARTY-242, FND-PARTY-243]
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
FND-PARTY-108), which no shipped script calls with code 59 (FND-PARTY-237). Both spells are cast at a single target picked with the map cursor, which takes a
companion of the caster while a Shift key is held (FND-PARTY-109). A party member with a druid
class at priest level 13 to 15 has spell 225 in its cast list and level 7 spells to cast, so it
can drain another party member; spell 104 needs level 9 wizard spells, which a Preserver capped at
level 15 never gets (FND-PARTY-110, FND-PARTY-111). The levels stay at 15 or below in the game's
stores and the shipped party list, no path found lets an `OBJEX` actor with a higher Preserver
level take a party slot, and the byte `DS:13F8` that lifts the limit is set only behind the
`-k911` debug switch (FND-PARTY-116, FND-PARTY-119, FND-PARTY-120). No enemy casts either
spell: the enemy spell choice keeps a spell only when its weight byte gives a score above 0, and
that byte is 0 for both; the enemies' other path and the item powers pass other numbers; and only
`RDFF` 3 has spell 104 in its list (FND-PARTY-121). Of the hit routine's other computed spell
numbers, only function 46 of script opcode `0x22` can give 104 or 225, from a script argument
(FND-PARTY-122). No shipped call of it passes either with the second argument 9999, apart from
`GPL/255` `0x0327`, which casts only when global script number 129 holds -9999 (FND-PARTY-237).
That number starts at 0 and changes only when its whole block is cleared or loaded from a saved
game, so the call does not cast in a game the program saved itself (FND-PARTY-241). No enemy strikes with the item of `DATA` 313:
the fight's swing passes no item, the enemies' spell, psionic and other choices pass other
numbers, no shipped script or item power passes 313, and the area rows hold none
(FND-PARTY-242). The party's own casts pass numbers from the wizard, priest and psionic cast
lists, all below 269, or numbers repeated from the last-cast table, which holds only those; and
its item powers pass no shipped item's number 313 (FND-PARTY-243). The one number left open is
the one the spell window's cast handler passes when the casting member's spell mode is 0, since
it then reads no list and leaves the number it had (FND-PARTY-243).

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the spell window's cast handler can cast while the member's spell mode is 0 and a list
  count is left above 0, the only party route not shown to pass a number other than `DATA` 313
  to the hit routine; no enemy route and no other party route passes it (FND-PARTY-242,
  FND-PARTY-243, Q-PARTY-105).
- Whether any party member has type bytes that make the calls before the hit routine's saving
  throw stop the spells for it, which they do for the types 2, 3, 7, 11, 13 and 16 (FND-PARTY-112);
  the shipped and transferred characters and the characters generation stores hold 0, and no
  other block copy found writes a whole record into the table (FND-PARTY-113 to FND-PARTY-115);
  saved-game loads and the flows those searches do not follow are Q-PARTY-062.
- What a player sees on the screen as the game ends: a capture of a party druid of priest level
  13 or more casting the spell `DATA` 225 at a single-class companion above level 1 while holding
  Shift would confirm it (Q-PARTY-059).

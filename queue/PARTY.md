# PARTY

Next ID: Q-PARTY-030

## Static

- Q-PARTY-028. FMT-PARTY-001: How does a character's level follow experience in play: which
  routine raises the level bytes once experience passes a threshold, how many levels it raises at
  once, and does level 15 end it, as the character sheet's skip of a level-15 position suggests
  (FND-PARTY-058, FND-PARTY-062)? Settles it: a reading of the overlay 210 routines that read
  `DATA` 1000 and of every store to the level bytes at `+0x1E..+0x20` of a details record.
  Blocks: slice 2.
- Q-PARTY-026. FMT-PARTY-001: On the DUAL path, does anything the routine at overlay 209 `+123E`
  calls before `+14FF` store another slot in the word at `4E71:0B44`, so that overlay 184 `+1DCE`
  takes the class bytes of a different character from the one whose records it copies
  (FND-PARTY-063, FND-PARTY-064)? Settles it: a reach query from overlay 209 `+123E` to `+14FF`
  over the calls it makes, checking which of the 11 stores to the word it reaches and with what
  value. Tried: FND-PARTY-064 reads the other three callers of `+1DCE` and the edit path, where the
  slots are the same; the DUAL path's calls were not followed. Blocks: slice 2.
- Q-PARTY-002. RULE-PARTY-002, RULE-PARTY-003, RULE-PARTY-005, RULE-PARTY-009: What does the character
  generation screen check beyond the class buttons: how it rolls the six scores and when it
  applies the origin modifiers, whether DONE refuses a character for its scores, gender,
  alignment or class minimums, and whether a psionicist can turn a discipline off? Settles it:
  the code behind SCR-UI-004 and SCR-UI-005 for DONE, the score buttons and the discipline
  window's `+0CE3`, `+0D2C` and `+0D95`. Tried: FND-PARTY-063 and FND-PARTY-066 read the class
  buttons and the sphere window, FND-PARTY-067 the discipline window, and FND-PARTY-068 which
  classes the buttons offer. Blocks: slice 2.
- Q-PARTY-029. RULE-PARTY-009: Is any of the eight words at `4E68:0000` written after load?
  Settles it: a search for stores through a segment register loaded with `4E68` (segment word
  `0x0368`) to offsets `0x00` to `0x0F`, with a positive control, and of every far pointer formed
  to that segment (FND-PARTY-068). Blocks: slice 2.
- Q-PARTY-003. FMT-PARTY-001, RULE-PARTY-004, RULE-MAGIC-002: What do the record's remaining `unk_`
  fields hold, and are the scores stored before or after origin modifiers? Settles it: the other
  routines that print the character sheet from the record at `DS:1429` (FND-PARTY-055), and the
  generation code that sets the scores. Tried: a search of the first 79 bytes of records 40 and 42
  for the label positions of their gender, origin, alignment and class (FND-PARTY-061);
  FND-PARTY-063 and FND-PARTY-066 show the four codes of Cleric, Druid and Ranger are the spheres; FND-PARTY-049, FND-PARTY-051 and FND-PARTY-055 to
  FND-PARTY-057 place hit points, the object offset, the combatant identifier, the control flags,
  greatest hit points, origin, gender, alignment, class codes with their names, and levels, and
  FND-PARTY-058 and FND-PARTY-059 place experience at `0x45` and the current and greatest
  psionic points at `0x0C` and `0x51`. Blocks: slice 2.
- Q-PARTY-020. FMT-PARTY-006: What does a 23-byte record of a type-2 or type-4 chunk hold, and
  what do the character's combatant words at `0x08`, `0x0A` and `0x0C`, and a record's word at
  `0x08`, which type-2 chunks fill with handles, lead to? Settles it: the code that reads the
  table at `DS:19C1` and those words, such as the View Character screen's possessions
  (SCR-UI-002). Blocks: slice 2.
- Q-PARTY-005. FMT-PARTY-004, FMT-PARTY-005: What do the `PSST` and `SPST` bytes hold, and what
  does the game do when it reads a 9-byte `SPST` into its 15-byte table entry? Settles it: the
  code that reads the four-slot tables overlay 186 fills (FND-PARTY-012). Blocks: slice 3.
- Q-PARTY-006. RULE-PARTY-001: What does the game do when the player tries to begin with an
  empty party, and does a party of fewer than four play differently? Settles it: the code of the
  View Character screen's exit that begins play (SCR-UI-002). Blocks: slice 2.
- Q-PARTY-007. RULE-PARTY-004, SCR-UI-015: Which classes does the DUAL list offer: must the new class meet
  its minimum scores, and may the current or a former class be picked? Settles it: the code
  behind the DUAL choice of SCR-UI-002. Blocks: slice 5.
- Q-PARTY-008. RULE-PARTY-007: Does the game apply the level limits of README table 3 and its
  prime requisite bonus, and how does it count the prime requisite of a class with more than
  one? Settles it: the code that raises a character's level. Blocks: slice 5.
- Q-PARTY-009. RULE-PARTY-008, RULE-COMBAT-001: What does a key from 1 to 4 do, in particular when its slot is
  empty, and is the word the party loader compares each slot with (FND-PARTY-013) the leader's
  slot? Settles it: the handler the keys 1 to 4 reach in overlay 190, which posts an event for
  the character boxes `0x2C24` to `0x2C27` (FND-COMBAT-025), and the writers of `leader`. Tried:
  the leader buttons, which store the slot at `4C13:0369` (FND-COMBAT-023). Blocks: slice 3.
- Q-PARTY-017. RULE-PARTY-006: When the player opens Create Characters (start window button
  `0x4B65`, overlay 212 `+061C`, window `0x2CEC`) before START GAME, which actions on that
  screen other than `ADD` on an empty box change the placed-object count, `DS:0DAB`, the
  pointer image or a window callback before the start loop reaches START GAME, and does any
  routine reached only through the unresolved transfers of FND-PARTY-046's two runs (48 from
  the ADD window, 66 from the screen's handler) store 0 to the count or make `DS:0DAB`
  nonzero? Settles it: the code of `NEW` (overlay 184 `+07D8`, and whether it reaches
  `+1029`'s placement call at `+12E5`), of overlay 190 `+139B` and `+0802` read for the events
  that reach the `DS:13FB` setter overlay 173 `+3CDA` and the pointer-image sites, each
  unresolved transfer of the two runs resolved or shown unreachable, and `56E9:0025`, overlay
  171 `+0938` and `56BD:00A2` read for stores to the party record between `ADD`'s load and its
  placement (FND-PARTY-049). Tried: FND-PARTY-045 run 4
  found the routes; FND-PARTY-046 reads `ADD`, which increments the count and leaves no store
  that clears it on the way back to the start window. Blocks: slice 2.
- Q-PARTY-018. RULE-PARTY-006: After a confirmed load from the start window's Load Saved Game
  window (overlay 192 `+0785`, confirm branch `+0B21`), can START GAME still be chosen, and with
  what placed-object count and `DS:0DAB`? Settles it: the load routine overlay 192 `+05ED`,
  overlay 182 `+19F8`, `28C9:2522`, the gate routine called with 0, and whether window 19500 or
  its handler survives those calls (FND-PARTY-045). Blocks: slice 2.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-PARTY-027. BUG-PARTY-002: Does a character made with a Druid first and a Preserver second
  start at level 6 Druid and level 7 Preserver with 40,000 experience, where the Druid's own table
  gives level 7 from 35,000, and what level does its stored record hold for the unused third class?
  Settles it: an owner capture of the generation screen's class line and experience after choosing
  the two classes, and of the View Character screen once the character is stored (FND-PARTY-065).
  Blocks: slice 2.
- Q-PARTY-025. BUG-PARTY-001: Does the character sheet print, in brackets after the experience,
  the threshold of the row numbered by the class code less 1, so that a supplied Cleric (code 2) at
  level 7 shows 60000 where its class's table gives 110000, and what does it show for a Fighter,
  Gladiator or Thief (codes 9, 10 and 17)? Settles it: an owner capture of the View Character
  screen for each supplied character before any play, compared with records 40 to 43 and
  FND-PARTY-058's table. Tried: the static reading FND-PARTY-058 and FND-PARTY-062. Blocks:
  slice 2.
- Q-PARTY-001. RULE-PARTY-006: Does START GAME put characters 40, 41, 42 and 43 into the four
  party slots, and not 53 or 33? Settles it: the shipped-party live session, with captures taken
  before any play so that experience and hit points can be compared with records 41 and 53, and
  43 and 33. Tried: the static reading FND-PARTY-013 finds a routine that loads characters 40
  to 43, FND-PARTY-021 traces START GAME to it through a gate on the placed-object count
  (Q-PARTY-017), and the earlier captures FND-PARTY-020, taken after
  play, match 40, 41 or 53, 42, and 33 or 43. Blocks: slice 2.

## Source

None.

## Blocked

None.

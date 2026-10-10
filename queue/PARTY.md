# PARTY

Next ID: Q-PARTY-044

## Static

- Q-PARTY-026. FMT-PARTY-001: On the DUAL path, does anything the routine at overlay 209 `+123E`
  calls before `+14FF` store another slot in the word at `4E71:0B44`, so that overlay 184 `+1DCE`
  takes the class bytes of a different character from the one whose records it copies
  (FND-PARTY-073, FND-PARTY-080)? Settles it: the routes from overlay 172 `+05DB` to overlay 188
  `+1604` read for the state they need while overlay 190 offers `DUAL` (`DS:0DAB` among it), and
  the argument overlay 210 `+0740` passes to overlay 209 `+0000`, `+0218` and `+0A84`, read for
  whether it can differ from the DUAL slot and whether the word is set back before `+14FF`.
  Tried: FND-PARTY-080 reads the other three callers of `+1DCE`, where the slots are the same, and
  a reach run from the DUAL routine's 14 calls reaches three overlay 209 stores to the word, only
  through overlay 172 `+05DB` and overlays 173, 188 and 210; the routes were not read for the state
  they need; FND-PARTY-081 shows overlay 210 `+0740` passes overlay 209 `+0000` the slot whose
  Preserver level rose, so the question is whether that can be another slot on this path.
  Blocks: slice 2.
- Q-PARTY-043. RULE-PARTY-013: When the level gain runs after a fight, which slot do `4E71:0B44`
  and the far pointer `DS:142D` hold, and so whose wisdom does the wisdom bonus of the greatest
  psionic points use for a level that is not a new Psionicist level (FND-PARTY-084)? Settles it:
  the paths from the end of a fight through overlays 173 and 188 to overlay 210 `+0B44`, read for
  calls that store either. Blocks: slice 2.
- Q-PARTY-039. RULE-PARTY-013: What else does a level change: overlay 210 `+0365`, `+03EC` (whose
  result goes to the combatant record's byte at `0x16`) and `+0572`, `27E5:000F`, overlay 199
  `+0C21`, and overlay 209 `+0000` and `+0A02` for a new Preserver or Psionicist level
  (FND-PARTY-081)? Settles it: a reading of those routines. Blocks: slice 2.
- Q-PARTY-040. RULE-PARTY-013: Which awards run the level gain: what the routine around overlay
  173 `+07AD` and the overlay 188 routine that ends at `+16E9` give experience for, and how much
  (FND-PARTY-081)? Settles it: a reading of those routines and their callers. Blocks: slice 2.
- Q-PARTY-041. FMT-PARTY-001: When does the level drain of overlay 210 `+0B66` run: what overlay
  195 `+0BC9` handles, with which slot, and from where (FND-PARTY-082)? Settles it: a reading of
  overlay 195 around `+0BC9` and of the dispatch that reaches it. Blocks: nothing yet.
- Q-PARTY-042. FMT-PARTY-001: What does the program do on a divide by 0, which the level drain
  reaches for a character with one class above level 1 (FND-PARTY-082)? Settles it: the startup
  code's setting of the interrupt 0 vector and the handler it installs, read to its end. Blocks: nothing yet.
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

- Q-PARTY-037. RULE-PARTY-013, BUG-PARTY-003: Does a party gain levels after a fight as
  RULE-PARTY-013 gives, and does a level-14 psionicist stay at level 14 with 1,400,000
  experience? Settles it: an owner capture of each member's experience, levels and hit points on
  the character sheet before and after a fight that gives experience, with the messages shown,
  for a party that includes a level-14 psionicist (FND-PARTY-081). Tried: the static reading
  FND-PARTY-081. Blocks: slice 2.
- Q-PARTY-035. RULE-PARTY-010, RULE-PARTY-011, RULE-PARTY-012: Does the left mouse button step
  the generation screen's buttons forward and the right one back? Settles it: an owner capture of
  a score, the hit points and the alignment after one left press and after one right press on each
  button (FND-PARTY-078). Tried: the static reading FND-PARTY-078, which rests on the mouse driver's
  meaning of the event bits. Blocks: slice 2.
- Q-PARTY-032. RULE-PARTY-011: Does pressing the alignment button with Ctrl held skip the
  classes' check, so that a fighter can be stepped to lawful evil and stored that way? Settles it:
  an owner capture of the generation screen's alignment after each press with Ctrl held, starting
  from true neutral, then of the View Character screen after DONE (FND-PARTY-072). Tried: the
  static reading FND-PARTY-072, which rests on the BIOS meaning of bit 4 of the keyboard flags.
  Blocks: slice 2.
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
  FND-PARTY-058's table. Tried: the static reading FND-PARTY-058 and FND-PARTY-083. Blocks:
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

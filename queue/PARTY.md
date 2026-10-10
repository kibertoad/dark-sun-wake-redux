# PARTY

Next ID: Q-PARTY-020

## Static

- Q-PARTY-002. RULE-PARTY-002, RULE-PARTY-003, RULE-PARTY-005, RULE-PARTY-007: What does the
  character generation screen check and offer: which classes it offers each origin where the
  manual's two lists and README table 3 disagree (half-giant ranger or thief, mul druid,
  thri-kreen druid or thief), which class combinations it allows, which classes get the sphere
  list, how it rolls the six scores and when it applies the origin modifiers, and whether it
  refuses DONE or greys out choices? Settles it: the code behind SCR-UI-004 and SCR-UI-005, then
  an owner capture of the generation screen for each disputed pair. Blocks: slice 2.
- Q-PARTY-003. FMT-PARTY-001, FMT-PARTY-002, RULE-PARTY-004, RULE-MAGIC-002: Where does a character record keep
  gender, origin, alignment, classes, levels, experience, hit points and psionic strength points,
  what do `unk_02`, `unk_29`, `unk_3B` and the tail entries hold, are the scores stored before or
  after origin modifiers, and does the game accept a `version` other than 1? Settles it: the far
  routine that overlays 171 and 182 call with `CHAR` and 7 to load a record into a party slot
  (FND-PARTY-012, FND-PARTY-013), read against the 49-byte party records at `DS:19C9`. Tried: a
  search of the headers of records 40 and 42 for the label positions of their gender, origin,
  alignment and class (FND-PARTY-018). Blocks: slice 2.
- Q-PARTY-004. FMT-PARTY-003, RULE-PARTY-003: Does each bit of the `PSIN` byte stand for one
  psionic discipline, and which? Settles it: the code that reads the four-slot `PSIN` table
  overlay 186 fills (FND-PARTY-012), or the discipline list of SCR-UI-005. Blocks: slice 2.
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
  that reach the `DS:13FB` setter overlay 173 `+3CDA` and the pointer-image sites, and each
  unresolved transfer of the two runs resolved or shown unreachable. Tried: FND-PARTY-045 run 4
  found the routes; FND-PARTY-046 reads `ADD`, which increments the count and leaves no store
  that clears it on the way back to the start window. Blocks: slice 2.
- Q-PARTY-018. RULE-PARTY-006: After a confirmed load from the start window's Load Saved Game
  window (overlay 192 `+0785`, confirm branch `+0B21`), can START GAME still be chosen, and with
  what placed-object count and `DS:0DAB`? Settles it: the load routine overlay 192 `+05ED`,
  overlay 182 `+19F8`, `28C9:2522`, the gate routine called with 0, and whether window 19500 or
  its handler survives those calls (FND-PARTY-045). Blocks: slice 2.
- Q-PARTY-019. RULE-PARTY-006: When the placed-object count is 0, does `31E0:0EFF` return a
  table entry rather than -1, so that the placement routine `31E0:0121` that `ADD` calls
  reaches its increment at `31E0:01E3`? Settles it: the code of `31E0:0EFF` and every input it
  reads, with the table at `DS:67B7` as program start leaves it (FND-PARTY-046,
  FND-ACTOR-004). Blocks: slice 2.

## Emulated call

None.

## Agent run

None.

## Live session

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

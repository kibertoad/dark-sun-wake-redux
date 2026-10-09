# PARTY

Next ID: Q-PARTY-015

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
- Q-PARTY-011. RULE-PARTY-006: Can any of the 56 indirect calls that FND-PARTY-031 leaves
  unresolved, other than the four in `1038:0008` that run only as the program ends
  (FND-PARTY-032), on the paths from program start through START GAME to the gate at overlay 182
  offset `0x12DD`, reach a routine that changes the placed-object count at `DS:264E`
  (FND-PARTY-022) or makes the word at `DS:0DAB` nonzero (FND-PARTY-029)? Settles it: the
  producers of each pointer those calls read, shown to hold no such routine before the gate, or
  the first one that does. Tried: the count's start value and direct stores, the far references
  to the overlay writers and three routines on the path (FND-PARTY-022); the start values and
  writers of `DS:0DAB` and `DS:0D9C` (FND-PARTY-029); a recursive-descent graph of every direct
  call and bounded jump table from program start, the start window loop, the Start Game branch
  and the gate routine, which reaches none of those routines and leaves 60 calls
  (FND-PARTY-031); the exit table read by `1038:0008` (FND-PARTY-032). Blocks: slice 2.
- Q-PARTY-014. RULE-PARTY-006: What does the C run time pass as the main routine `277D:0004`'s
  `argv[0]`, the string whose directory part becomes `DS:44F2`, the directory `CHARSAVE.GFF` is
  opened from? Settles it: the start-up code from program entry to the call of `277D:0004`, read
  for how it builds the argument list on each DOS version. Split from Q-PARTY-012, which
  FND-PARTY-034 closed. Blocks: none.
- Q-PARTY-013. RULE-PARTY-006: Before START GAME reaches the gate, does a call of `3D72:120B`
  with a nonzero `ICON` number or of `3D72:12ED` with a nonzero image leave a pointer image in
  place at `DS:A14D`, with `DS:A13D` 0, whose frame at `DS:A151` makes the pointer save and the
  caret reservation together hold more than 1,006 paragraphs, so that one of the gate routine's
  calls to `1BF3:27A8` at `DSUN.EXE+0x00069AA5` and `0x00069AC1` returns `0xFFFF`? Settles it:
  the callers of `3D72:120B` and `3D72:12ED` on the paths of FND-PARTY-031 and the images and
  frames they pass, and, if a large image is possible, the caret heights in the tables at offset
  `0x9C` of the edit-field objects that `409B:11B7` and `409B:0D48` take before the gate. Tried:
  the reservation routine, its pool and the two calls' sizes (FND-PARTY-030); the startup
  reservation of 1,000 paragraphs, held until the program ends (FND-PARTY-032); the scroll,
  pointer and caret routines, of which only the last two keep an entry after they return
  (FND-PARTY-033); the caret's release of its earlier entries, the writers of its height, and the
  startup pointer image `ICON` 100, whose frames take at most 2 paragraphs (FND-PARTY-035).
  Blocks: slice 2.

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
  (Q-PARTY-011), and the earlier captures FND-PARTY-020, taken after
  play, match 40, 41 or 53, 42, and 33 or 43. Blocks: slice 2.

## Source

None.

## Blocked

None.

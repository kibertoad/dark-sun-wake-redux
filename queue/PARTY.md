# PARTY

Next ID: Q-PARTY-081

## Static

- Q-PARTY-076. BUG-PARTY-001: Can anything other than overlay 184 `+0DA7` run while the
  generation screen is up and point `DS:1429` at a stored details record before the screen draws
  its sheet? Settles it: the 34 unresolved calls of FND-PARTY-189's reach, the main loop's calls
  through `DS:A0F5` and `DS:A0F9` on routes into generation other than `NEW` and `EDIT` from View
  Character, and stores to `DS:1429` other than direct-address ones. Tried: FND-PARTY-187 runs
  View Character in play and finds codes 1 to 8; FND-PARTY-188 lists the sheet's three callers
  and the direct-address writers of `DS:1429`; FND-PARTY-189 shows generation's own handler
  reaches those writers only in `+1029`, after its sheet draw and as it closes the screen;
  FND-PARTY-190 and FND-PARTY-191 open generation by `NEW` (from Create Characters and from
  play) and by `EDIT` and find no other handler and Fighter's own threshold on the sheet.
  Blocks: nothing yet.
- Q-PARTY-077. BUG-PARTY-001: On the DUAL path, does the word at `4E71:0B44` name the slot
  overlay 209 `+123E` was called with when it calls overlay 184 `+1DCE` at `+14FF`, so that the
  sheet at `+151E` prints that character's own classes? Settles it: the routes from overlay 172
  `+05DB` through overlays 173, 188 and 210 to the overlay 209 routines that store `4E71:0B44`,
  given `DS:0DAB` 0 (FND-PARTY-191 reads 0 on the Create Characters route; whether the in-play
  route can offer `DUAL` with another value), and what those routines store (FND-PARTY-080).
  Tried: FND-PARTY-080 reads the calls before `+1DCE` on this path and FND-PARTY-188 finds the
  sheet call at `+151E` right after it; FND-PARTY-191 runs `DUAL` for a character made just
  before and finds the word naming that character's slot and the Fighter threshold on the sheet.
  Blocks: nothing yet.
- Q-PARTY-073. RULE-PARTY-001: Can View Character, opened from Create Characters before START
  GAME, pass an event to overlay 211 `+081B` that runs the damage routine overlay 173 `+22C4` on a
  party member, giving it `combat_mark` 3, 7 or 8? Settles it: the condition at overlay 190
  `+1649` (the far pointer at `55C9:0000` equal to `DS:1431`), what opens that window on this
  screen, and the conditions along overlay 211 `+0901`, overlay 193 `+2290`, `+0000`, `+003C` and
  `+04DE` and overlay 179 `+0D2F` to the call at `+0D9C`. Tried: FND-PARTY-183, FND-PARTY-184,
  FND-PARTY-192 and FND-PARTY-193 show ADD and NEW give mark 1; FND-PARTY-197 takes a census of
  the stores to combatant byte `+0x14`, finds `+22C4` the only one through `DS:19C9` writing a
  mark above 2, and a reach from the title screen's windows to it through this route. Blocks:
  nothing yet.
- Q-PARTY-079. RULE-PARTY-001: Can `set_leader` (overlay 182 `+1D3E`), reached from View
  Character's `+0802` before START GAME, run the attack routine overlay 173 `+1CC1` through
  `2D40:10AE`, and does that reach `+22C4` or `+3CDA` there? Settles it: the conditions at
  overlay 182 `+1E48`, in `2D40:10AE` before `2D40:13D9`, and in `+1CC1` before `+2290`. Tried:
  FND-PARTY-197's reach finds the route. Blocks: nothing yet.
- Q-PARTY-080. RULE-PARTY-001: Does any other write give a filled slot's combatant record a
  `combat_mark` of 3 or more, or hit points below 1, before START GAME? Settles it: the pointer
  sources of overlay 172 `+01AE`, 179 `+2515`, `+25AD` and `+2668`, 200 `+04DD`, resident
  `31E0:2350` and the resident word stores at `+0x14` FND-PARTY-197 lists; writes to `+0x14` not
  of its search's form (DS or SS segment, 16-bit displacement, string stores, block copies); the
  stores to word `+0x00`; and the value overlay 187 `+018F` writes to a type-1 chunk's byte
  `0x14` (FND-PARTY-054). Tried: FND-PARTY-197 classifies the 42 candidates of its search.
  Blocks: nothing yet.
- Q-PARTY-078. RULE-PARTY-001: What does the player see and what happens next when overlay 173
  `+3CDA` finds no member standing? Settles it: whether `39D1:0854` waits for input when the
  window has no handler, what `444C:0092` does with an image pointer, the value of `DS:4458` and
  the archive its `BMP` request searches, and what the three callers of `+3CDA` (overlay 173
  `+07A1` and `+2514`, overlay 188 `+14A3`) do after it returns with no leader set. Tried:
  FND-PARTY-194 reads overlay 201 `+0410`: window 19502, `BMP` number `DS:4458` or 10004, one
  event, then the window closes. Blocks: nothing yet.
- Q-PARTY-045. RULE-PARTY-015: Does any enemy's `kill_experience` divided by the filled party
  slots reach 32,768, so that killing it lowers each member's experience (FND-PARTY-086,
  FND-PARTY-094)? Settles it: what moves objects 430 and 541 out of the region `Limbo`, where
  their only `ETAB` records are, read to show whether the party can fight them. Blocks: nothing
  yet. Tried: FND-PARTY-094 finds `RDFF` 430 (107,000) and 541 (33,000), of kind 7, with CHAR
  chunk headers; FND-PARTY-153 shows `RDFF` resources load through the CHAR chunk walk and finds
  both objects only in `RGN0FF.GFF`. Scripts that move objects are the SCRIPT area's, outside
  this goal's claim.
- Q-PARTY-062. BUG-PARTY-006: Does anything other than a block copy, or a copy FND-PARTY-115's
  searches do not reach, put a value other than 0 in bytes `+0x0E` or `+0x0F` of a party member's
  FMT-COMBAT-001 record? Settles it: the reads of a saved game into the table at `DS:19C9` and the
  writes that produce them; the routines that reload the table pointer from `DS:142D`, `DS:6157`,
  `DS:6177` or `DS:619B` or from a local and pass it on, followed past branches and more than one
  call deep; and the indirect calls that take it. Tried: FND-PARTY-115 reads the block copies, the
  straight-line pushes and stores of the table segment and one level of calls below them. Blocks:
  nothing yet.
- Q-PARTY-003. FMT-PARTY-001, RULE-PARTY-004, RULE-MAGIC-002: What do the record's remaining `unk_`
  fields hold? Settles it: the other
  routines that print the character sheet from the record at `DS:1429` (FND-PARTY-055), and the
  generation code that sets the scores. Tried: a search of the first 79 bytes of records 40 and 42
  for the label positions of their gender, origin, alignment and class (FND-PARTY-061);
  FND-PARTY-063 and FND-PARTY-066 show the four codes of Cleric, Druid and Ranger are the spheres; FND-PARTY-049, FND-PARTY-051 and FND-PARTY-055 to
  FND-PARTY-057 place hit points, the object offset, the combatant identifier, the control flags,
  greatest hit points, origin, gender, alignment, class codes with their names, and levels, and
  FND-PARTY-058 and FND-PARTY-059 place experience at `0x45` and the current and greatest
  psionic points at `0x0C` and `0x51`. FND-PARTY-142 shows `0x5A` holds the base scores, and
  with FND-PARTY-071 that the scores are stored with the origin modifiers added. FND-PARTY-143 shows `0x1F` is the
  combatant kind, FND-PARTY-144 that `0x29` is the facing and FND-PARTY-145 that `0x66` is the base
  armor class; FND-PARTY-146 shows `0x1D` and `0x67` are the current and base movement rate, FND-PARTY-147 that `0x53` is the details record's key, and FND-PARTY-148 that `0x04`, `0x06` and
  `0x0E` hold the slot the character was written from and `0x12` to `0x16` the child words at that time; FND-PARTY-149 shows `0x81` and `0x83` are sound numbers, and FND-PARTY-150 that `0x68` is the magic resistance; FND-PARTY-151 places the footprint at `0x7C` and a spell list at `0x7D`. FND-PARTY-152 finds no reader through the combatant table of
  `0x1C` or `0x21`, and `0x2A` only as field 33 of overlay 204's field getter and setter, which FND-PARTY-156 shows are
  functions 48 and 49 of script opcode `0x22`, so what `0x2A` means waits on Q-SCRIPT-004; FND-PARTY-157 names `0x18` the special attack kind
  and `0x19` the immunity type, and FND-PARTY-158 names `0x85` the psionic power list; FND-PARTY-159 shows the saving throw never
  reads `0x7B`. Blocks:
  slice 2.
- Q-PARTY-020. FMT-PARTY-006: What does the rest of a 23-byte record of a type-2 or type-4 chunk
  hold, and what do the character's combatant words at `0x08`, `0x0A` and `0x0C`, and a record's
  word at `0x08`, which type-2 chunks fill with handles, lead to? Settles it: the code that reads
  the table at `DS:19C1` and those words, such as the View Character screen's possessions
  (SCR-UI-002), and the routine `+04F8` of overlay 188 that places records' objects. Tried:
  FND-PARTY-181 and FND-PARTY-182 give word `0x00` as the negated object number and byte `0x12`
  as the placed entry's low bits; FND-PARTY-196 lists the shipped values of every field and shows
  9999 ending each chain at `0x04`, but names no field from code. Blocks: slice 2.
- Q-PARTY-005. FMT-PARTY-004, FMT-PARTY-005: What does bit 0 of a `PSST` byte hold, and what
  does the game do when it reads a 9-byte `SPST` into its 15-byte table entry? Settles it: the
  code that reads and writes the four-slot tables overlay 186 fills (FND-PARTY-012), and the read
  entry `37FC:04AB` for a resource shorter than the size it is given. Tried: FND-PARTY-104 reads
  the level-gain windows, which show `SPST` as the known spells and bits 1 to 7 of `PSST` as power
  ranks, and keep bit 0. Blocks: slice 3.
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

- Q-PARTY-059. BUG-PARTY-006: Does the game end with `Math Err` when a party druid of priest level
  13 to 15 casts the spell `DATA` 225 at a single-class companion above level 1 while a Shift key
  is held? Settles it: an owner capture, from a save with such a druid that has level 7 spells to
  cast, of the cast window at level 7, the target cursor on the companion with Shift held, and the
  screen after the companion fails its saving throw, repeated until one fails (FND-PARTY-109,
  FND-PARTY-110, FND-PARTY-111). Required parts: Send it input, Capture frames and sound (frames)
  and Load a patched save, for a save holding such a druid, all of them the owner's. Tried: the static readings FND-PARTY-082, FND-PARTY-105 and FND-PARTY-109 to
  FND-PARTY-111. Blocks: nothing yet.
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
## Source

None.

## Blocked

- Q-PARTY-007. RULE-PARTY-004, SCR-UI-015: Which classes does the DUAL window offer, and which of its buttons
  are closed? Settles it: SCR-UI-015 taking the test of RULE-PARTY-004 and the window of
  FND-PARTY-161. Blocks: slice 5. Tried: FND-PARTY-161 reads the window, its test and its
  handler, and RULE-PARTY-004 gives the test. Waiting on: a goal that claims the UI area, which
  owns SCR-UI-015.
- Q-PARTY-050. RULE-PARTY-013: Does any trigger record's second entry point that can run during a
  fight hold script opcode `0x24`, which makes the slot in `4C13:0369` the current record through
  overlay 190 `+36D2` (FND-PARTY-097, FND-PARTY-098)? Settles it: the `GPL ` resources and offsets
  the trigger records name, decoded instruction by instruction and searched for opcode `0x24`.
  Blocks: slice 2. Tried: FND-PARTY-098 reads the conditions under which `2D40:207E` runs the
  entry point; a byte search for `0x24` would also match parameter bytes, so it cannot settle
  the question without the instruction layouts. Waiting on: Q-SCRIPT-004, the layout of every
  script instruction, in the SCRIPT area.
- Q-PARTY-051. FMT-PARTY-001, BUG-PARTY-006: Does any script call function 23 of opcode `0x22` with the effect
  code 59, which runs the level drain on a party slot, and on which slots does overlay 204 `+1A68`
  apply it (FND-PARTY-108), or call function 46 with 9999 as its second argument and a fourth
  argument whose remainder by 1000 is 104 or 225, which casts that spell (FND-PARTY-122)? Settles
  it: the `GPL ` resources decoded instruction by instruction and searched for opcode `0x22` with
  function 23 and code 59 or function 46 with those arguments, and a reading of `+1A68` and of
  where function 46's cast lands. Blocks: nothing yet. Tried: FND-PARTY-108 and FND-PARTY-122
  trace the code and the spell to the script's arguments; a byte search cannot separate
  parameters without the instruction layouts. Waiting on: Q-SCRIPT-004, the
  layout of every script instruction, in the SCRIPT area.

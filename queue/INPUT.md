# INPUT

Next ID: Q-INPUT-003

## Static

- Q-INPUT-002. RULE-INPUT-001, RULE-INPUT-002: What do the words `57E0:1A34` and `57E0:44E6`
  hold, and so when does a right-button press reach the fourth pointer mode, 3, and when does
  combat block the press? What do `362C:06B9` and overlay 190 `+2503`, which the step calls, do?
  Which code shows `ICON/19109`? Settles it: a reading of the stores to `57E0:1A34` in overlays
  175, 187 and 188 and to `57E0:44E6`, and of the two called routines. Tried: FND-INPUT-012 reads
  how overlay 190 `+139B` turns a key word into a screen; FND-INPUT-013 reads the right-button
  route from the `APFM/19200` callback `28C9:1261` to the step at overlay 182 `+1783`, the image
  routines `2C5F:0C8D` and `28C9:2608`, and finds that `2C5F:0C8D` gives `ICON/19110` for a
  value outside 1 to 5 and for Walk while `turn_combatant` is 4 or more.
  Blocks: nothing yet.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-INPUT-001. RULE-INPUT-001, RULE-INPUT-002, RULE-INPUT-003: In which mode does the pointer
  start, in which order does the right button step through the modes, when does the Attack
  pointer show the hand-to-hand pair instead of the ranged one, and do `C`, `U` and `E` open the
  Cast Spells and Use Psionics and the Current Spell Effects screens? Settles it: a live session
  with DOSBox screenshots after each right click and after each of `C`, `U`, `E`, `I` and `V`,
  with a melee-only and a ranged party leader. Tried: FND-INPUT-012 shows that the letters work
  in either case and with Shift, and that the handlers of the Game Menu and the character
  option screens pass keys to the same routine. FND-INPUT-013 reads the order of the modes from
  the code; the starting mode depends on which of several writers of `57E0:1440` runs last.
  Blocks: nothing yet.

## Source

None.

## Blocked

None.

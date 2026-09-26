# INPUT

Next ID: Q-INPUT-003

## Static

- Q-INPUT-002. RULE-INPUT-001, RULE-INPUT-002, RULE-INPUT-003: Which code reads the packet queue
  that the keyboard hook and the mouse handler fill at `4464:0230`, and how does it turn a key
  word or a mouse event into a mode change, a click or a screen? Which code shows `ICON/19109` and
  `ICON/19110`? Settles it: a reading of the routines that read the buffer set up at `39D1:0173`,
  and of the overlay caller of the pointer-image routine. Blocks: nothing yet.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-INPUT-001. RULE-INPUT-001, RULE-INPUT-002, RULE-INPUT-003: In which mode does the pointer
  start, in which order does the right button step through the modes, when does the Attack
  pointer show the hand-to-hand pair instead of the ranged one, and do the hotkeys work in lower
  case, with shift, and on the character option screens? Settles it: a live session with DOSBox
  screenshots after each right click and each key, with a melee-only and a ranged party leader.
  Blocks: nothing yet.

## Source

None.

## Blocked

None.

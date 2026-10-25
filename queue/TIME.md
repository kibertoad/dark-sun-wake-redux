# TIME

Next ID: Q-TIME-006

## Static

- Q-TIME-001. RULE-TIME-001, RULE-TIME-002: What clock drives the mouse pointer's movement and
  redraw on screen? Settles it: the code that moves and redraws the pointer image, and what calls
  it. Tried: the BIOS time-of-day reads (FND-TIME-001), the `INT 15h` sites (FND-TIME-006), the
  display-status copy (FND-TIME-003) and the millisecond wait (FND-TIME-004); FND-TIME-008,
  FND-TIME-010 and FND-EXPLORE-019 place animation and movement on the 96 Hz `game_tick`, which
  takes those parts out. Blocks: slices 3-7.
- Q-TIME-002. RULE-TIME-002: What does a music driver's function `0x67` do on each timer call,
  and in which order do the music driver (`4734:038D`), the game tick (overlay 200 `+02BE`) and
  the intro FLI take their slots at start-up? Settles it: a reading of function `0x67` of
  `SBP2FM.ADV` (`RESOURCE.GFF` `ADV` 11) and of the start-up sequence that calls those three.
  Tried: FND-TIME-008 reads the library, its handler and the five slot registrations, and
  FND-TIME-009 the writes of the timer control port. Blocks: slice 7.
- Q-TIME-003. RULE-TIME-001, RULE-CONFIG-005: What event does each call of `wait_ms` delay?
  Settles it: bounded readings of the routines around the 13 calls listed in
  FND-TIME-004. Tried: the Preferences hover and click paths identify the
  word at `57E0:26B7` as message delay (FND-UI-034, FND-CONFIG-010), and
  FND-TIME-004 identifies its 100-ms scaling at one call, but the calling
  event and the other waits remain unclassified. Blocks: slices 3-7.
- Q-TIME-005. RULE-TIME-001, RULE-TIME-002: Why does play after the opening run channel 0 at
  1,000 µs with no music driver slot, when the installed `SOUND.CFG` names a driver whose
  descriptor asks for 120 Hz, and how long does `wait_ms` wait once a 120 Hz slot runs? Settles
  it: a reading of the driver install `4842:0B79` and its callers with the installed `SOUND.CFG`,
  then a run that breaks there and times a `wait_ms` call with the slot running. Tried:
  FND-TIME-008 reads the slot registrations; FND-TIME-011 sees channel 0 at 1,000 µs and slots
  only for `4654:09A9`, the game tick and the BIOS chain. Blocks: none.

## Emulated call

None.

## Agent run

None.

## Live session

None.

## Source

None.

## Blocked

None.

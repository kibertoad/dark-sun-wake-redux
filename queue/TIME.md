# TIME

Next ID: Q-TIME-005

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

## Emulated call

None.

## Agent run

- Q-TIME-004. RULE-TIME-001: How long does `wait_ms` wait during play, when channel 0 runs at the
  reload the timer library set? Settles it: a run that breaks at the entry and exit of
  `1000:12FA` on a call during play and reads `game_tick` at both, with the `ms` argument.
  Tried: FND-TIME-004 reads the wait, and FND-TIME-008 the reload the library sets. Blocks: none.

## Live session

None.

## Source

None.

## Blocked

None.

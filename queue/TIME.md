# TIME

Next ID: Q-TIME-004

## Static

- Q-TIME-001. RULE-TIME-001, RULE-TIME-002: Which clocks drive animation, cursor updates and
  movement cadence, and at what rates? Settles it: the handlers that register timer slots with
  `choose_timer_period` and what they count, and the code that advances animation frames and
  moves between cells. Tried: the BIOS time-of-day reads (FND-TIME-001), the `INT 15h` sites,
  which are extended-memory services (FND-TIME-002), the display-status copy (FND-TIME-003), the
  millisecond wait (FND-TIME-004) and the timer-slot routine (FND-TIME-005), none of which is
  tied to a feature yet. Blocks: slices 3-7.
- Q-TIME-002. RULE-TIME-002, RULE-TIME-001: Which routines register timer slots, with which
  periods, what runs on each, what `g_4868_011E` and `g_4868_0120` are for, and does the game ever
  set channel 0 to a mode other than 3? Settles it: the callers of the routine at `4868:03DA` and
  of the slot routine at `4868:05EA`, and the interrupt handler the library installs. Tried:
  the FLI player, which takes a slot with a period of 1,000 microseconds for as long as a
  cinematic plays (FND-VIDEO-002, RULE-VIDEO-004). Blocks: slice 7.
- Q-TIME-003. RULE-TIME-001: What does each call of `wait_ms` wait for, and is the word at
  `57E0:26B7` the Preferences message delay? Settles it: the routines around the 13 calls
  FND-TIME-004 lists, and the writers of `57E0:26B7`. Blocks: slices 3-7.

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

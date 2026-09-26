# TIME

Next ID: Q-TIME-002

## Static

- Q-TIME-001. (entries pending migration): Which clocks drive animation,
  cursor updates and movement cadence, and at what rates? Settles it: the
  timer-chip programming and the code that counts its ticks or the display
  retrace. Tried: the INT 15h sites (extended-memory services only), a busy
  VGA-status poll in a copy routine, and the timer-chip latch and programming
  sites, none of which has a recovered feature owner. Blocks: slices 3-7.

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

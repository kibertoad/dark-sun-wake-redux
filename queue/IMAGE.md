# IMAGE

Next ID: Q-IMAGE-004

## Static

- Q-IMAGE-002. RULE-IMAGE-001, RULE-IMAGE-002, FMT-IMAGE-002: How do the lower routines that
  `2D40:3BEC` calls decode each encoding: does the game check the `0xFF` before the tag, what
  does bit 0 of a run's flags do, what does a `PLAN` frame with a bit count of 0 draw, what does
  a `PLNR` repeat before any nonzero code give, and are the bytes after the last code of some
  `PLNR` frames read? Settles it: a reading of the three routines. Blocks: nothing yet.
- Q-IMAGE-003. FMT-IMAGE-001, FMT-IMAGE-003: Which palette does the game draw each image with, what sets `CBMP`
  and `ICON` apart from `BMP `, and does the game read an image's `size` field? Settles it: a
  reading of the image cache at `31E0:3568`, of the code that requests `PORT` in overlay 199, and
  of which palette each overlay 187 picture routine pairs with its pictures. Blocks: nothing yet.
  Tried: FND-IMAGE-012 reads every routine that writes the whole DAC and their callers: play uses
  `PAL/current_region`, the start window `PAL/1000`, and overlay 187's picture routines
  `PAL/1002`, `PAL/11164` and the numbers in the table at `DS:1687`. The palette loads are
  removed from Settles it; which palette is current when each image is drawn remains.

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

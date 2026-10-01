# IMAGE

Next ID: Q-IMAGE-004

## Static

- Q-IMAGE-001. FMT-IMAGE-002, FMT-IMAGE-004: What pixel aspect does the original present its 320x200 canvas
  at? Settles it: the video mode the game sets and whether it changes the CRT timing. Blocks:
  slice 7.
- Q-IMAGE-002. RULE-IMAGE-001, RULE-IMAGE-002, FMT-IMAGE-002: How do the lower routines that
  `2D40:3BEC` calls decode each encoding: does the game check the `0xFF` before the tag, what
  does bit 0 of a run's flags do, what does a `PLAN` frame with a bit count of 0 draw, what does
  a `PLNR` repeat before any nonzero code give, and are the bytes after the last code of some
  `PLNR` frames read? Settles it: a reading of the three routines. Blocks: nothing yet.
- Q-IMAGE-003. FMT-IMAGE-001, FMT-IMAGE-003: Which palette does the game draw each image with, what sets `CBMP`
  and `ICON` apart from `BMP `, and does the game read an image's `size` field? Settles it: a
  reading of the palette loads and of the image cache at `31E0:3568`, and of the code that
  requests `PORT` in overlay 199. Blocks: nothing yet.

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

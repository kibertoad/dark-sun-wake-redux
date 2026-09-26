# TEXT

Next ID: Q-TEXT-004

## Static

- Q-TEXT-001. FMT-TEXT-001, FMT-TEXT-002: How does the game load and draw the font: does it read
  `char_map` and `unk_04`, in what order are a glyph's pixels stored, which index is transparent,
  and what spacing does it put between glyphs and lines? Settles it: a reading of the code that
  draws text, found from the resource request for `FONT/100` or from a routine that walks glyph
  widths. Blocks: nothing yet.
- Q-TEXT-002. FMT-TEXT-003: How does the game load a `TEXT` resource and split its lines, and
  which screen shows which resource? Settles it: a reading of the overlay code of overlays 186 and
  188 around the `TEXT` tag bytes. Blocks: nothing yet.
- Q-TEXT-003. FMT-TEXT-004: Which code reads the Preferences text block, which description
  belongs to which control, what `unk_010` holds, what `%C` does, and which difficulty the game
  starts with? Settles it: a reading of the code that builds the Preferences window, found
  through the relocated far pointers or the window's resources. Blocks: nothing yet.

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

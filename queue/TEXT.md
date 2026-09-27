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
- Q-TEXT-003. FMT-TEXT-004: What does `unk_010` hold, what does `%C` do in the About lines,
  and which difficulty does a new game start with? Settles it: a bounded reading of the
  remaining text-drawing path and the new-game initialization callers. Tried: the renderer's
  difficulty-label lookup (FND-CONFIG-009) and hover-description branches (FND-UI-034), which
  identify the readers and label choices but not these remaining behaviors. Blocks: nothing yet.

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

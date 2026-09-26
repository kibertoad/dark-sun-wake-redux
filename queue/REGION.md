# REGION

Next ID: Q-REGION-006

## Static

- Q-REGION-001. FMT-REGION-001: Does the game read a region's `RNME` resource, and where does it
  show the name? Settles it: a reading of the code that shows region names, such as the travel
  map, in resident or overlay code. Blocks: nothing yet.
- Q-REGION-002. FMT-REGION-004: What do bits 0 to 4 and bit 7 of a cell's flags do, and why does
  the region loader clear `occupied` only in columns 0 to 97? Settles it: a reading of every other
  routine that reads the buffer the far pointer at `DS:0538` names, `25AF:02FA` in full, and the
  caller of `2778:0006`. Tried: the cell routines of segment `25AF` (FND-EXPLORE-001), which settle
  bits 5 and 6. Blocks: nothing yet.
- Q-REGION-003. FMT-REGION-005, FMT-REGION-006: Which code reads the entity table, what do the
  flag bits do (the source reads bit 7 as a mirror), is `object` signed, and what happens to
  records outside the map? Settles it: a reading of the overlay code of overlays 187 and 200
  around the `ETAB` tag bytes. Blocks: nothing yet.
- Q-REGION-004. FMT-REGION-002, FMT-REGION-003, RULE-REGION-001: Who calls the region loader and
  with what second argument, where would an `RMAP` resource come from, and does the tile copy at
  `2707:001A` leave undrawn tile pixels as they were? Settles it: a reading of the loader's
  callers, which are not resident far calls, and of `2707:001A`. Blocks: nothing yet.
- Q-REGION-005. RULE-REGION-001: What sets the view origin when a region is entered? The first
  gameplay frame of the opening region shows `(1024,1368)` (FND-REGION-008). Settles it: a reading
  of the code that writes the view origin before the first terrain draw. Blocks: nothing yet.

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

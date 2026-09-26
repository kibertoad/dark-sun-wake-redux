# EXPLORE

Next ID: Q-EXPLORE-007

## Static

- Q-EXPLORE-001. RULE-EXPLORE-001: How wide is the band at the edge of the view that scrolls the
  map, how far does one step move the view and how often, and where do Center on Leader and the key
  `H` put the leader? Settles it: the code that compares the pointer's position with the view's
  edges and writes the view origin, which Q-REGION-005 also looks for, the Game Menu's Center on
  Leader handler, and the `H` handler at offset `0x1718` of overlay 190 (FND-COMBAT-025). Blocks:
  slice 3.
- Q-EXPLORE-002. RULE-EXPLORE-003, FMT-COMBAT-002: Which slots are of kind 2, what values does the
  details byte `footprint` take, what does `25AF:02FA` decide from a cell's bits 0 to 2, and what
  is the object numbered 430? Settles it: the writers of `4F49:0C33`, the code that fills the
  66-byte details records, `25AF:02FA` in full, and the callers of `25AF:071F`. Tried: the cell,
  footprint and area routines (FND-EXPLORE-001, FND-EXPLORE-002), which show how a footprint is
  laid out but not which values the records hold. Blocks: slice 3.
- Q-EXPLORE-003. RULE-EXPLORE-004: What does the movement routine `2D40:10AE` do after its route
  search, what do `28C9:305F`, `28C9:2535`, `2C5F:0182` and `2C5F:0C8D` do, and what are the
  19-byte records at `4F49:08A3` and the byte at `4E71:0000` for? Does the key routine read keys
  through a BIOS service that gives the grey arrow keys the keypad's key words? Settles it: a
  reading of those routines and of the code that fills the key word the key routine compares.
  Tried: the direction key handlers and the step routine `28C9:01CB` (FND-EXPLORE-004). Blocks:
  slice 3.
- Q-EXPLORE-004. RULE-EXPLORE-002: Where does key 5 put the members it shows, what do
  `2C5F:0271`, `2C5F:0139` and `28C9:2A81` redraw, what does bit 3 of an entry's byte 5 stand for,
  and which stores to `57E0:13FA` belong to Collapse Party and to loading a game? Settles it:
  `28C9:2C9E` in full, the three redraw routines, the writers of the entry flags, and the two
  stores of overlay 187 at offsets `0x0F35` and `0x0F76`. Tried: the handlers of keys 5 and 6 and
  a byte search for stores to the byte (FND-EXPLORE-005). Blocks: slice 3.
- Q-EXPLORE-005. RULE-EXPLORE-003, FMT-ACTOR-004: Is the opening leader's position `(1192,1496)`,
  its figure's top-left plus the definition's offsets, so that its cell is `(74,93)`? Settles it:
  the position routine the party loader of FND-PARTY-013 calls, and `31E0:435D`. Tried: the
  movement, move and draw routines (FND-EXPLORE-003), which relate the cell to the position but
  do not show how the party loader sets it. Blocks: slice 3.
- Q-EXPLORE-006. RULE-EXPLORE-005: Which routine plans the walk to a cell clicked with the Walk
  pointer, how does it choose the cells, what happens when no route exists or a cell on the route
  becomes blocked, how long does one step take, and how close to the clicked cell does the walk
  end? Settles it: the code that handles a left click on the map in Walk mode, which is not found
  yet, and the route search at the start of `2D40:10AE`, which Q-EXPLORE-003 also reads. Tried:
  nothing yet. Blocks: slice 3.

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

# EXPLORE

Next ID: Q-EXPLORE-011

## Static

- Q-EXPLORE-001. RULE-EXPLORE-001, RULE-INPUT-001: What does `g_57E0_0DAB` hold? Settles it: the
  stores to `DS:0DAB` (`28C9:252A` with its two callers in overlay 204, and the six direct stores in
  overlay 182), each placed in its routine with the value it stores, and the readers' tests of its
  values 0 to 5. Tried: FND-EXPLORE-006, FND-EXPLORE-007, FND-EXPLORE-009 and FND-EXPLORE-010 read
  the edge-scroll pass, its pass rate and `DS:0DA8`, which leave this word; split into
  Q-EXPLORE-008 and Q-EXPLORE-009. Blocks: slice 3.
- Q-EXPLORE-008. RULE-EXPLORE-001: Does the mouse setup run after the video mode's word is stored,
  which the 320 by 200 range of the pointer depends on? Settles it: the order of `39D1:0009`
  against the mode setter's call with 0x0113 in startup. Tried: split from Q-EXPLORE-001;
  FND-EXPLORE-006 and FND-CONFIG-158 give the range and the setup. Blocks: slice 3.
- Q-EXPLORE-002. RULE-EXPLORE-003, FMT-COMBAT-002: Which slots are of kind 2, what values does the
  details byte `footprint` take, what does `25AF:02FA` decide from a cell's bits 0 to 2, and what
  is the object numbered 430? Settles it: the writers of `4F49:0C33`, the code that fills the
  66-byte details records, `25AF:02FA` in full, and the callers of `25AF:071F`. Tried: the cell,
  footprint and area routines (FND-EXPLORE-001, FND-EXPLORE-002), which show how a footprint is
  laid out but not which values the records hold. Blocks: slice 3.
- Q-EXPLORE-003. RULE-EXPLORE-004: What do `28C9:305F`, `28C9:2535`, `2C5F:0182` and `2C5F:0C8D`
  do, and what is the byte at `4E71:0000` for? Does the key routine read keys through a BIOS
  service that gives the grey arrow keys the keypad's key words? Settles it: a reading of those
  routines and of the code that fills the key word the key routine compares. Tried: the direction
  key handlers and the step routine `28C9:01CB` (FND-EXPLORE-004); FND-RNG-068 and FND-EXPLORE-014
  read the 19-byte records at `4F49:08A3` and what `2D40:10AE` does after its route search, which
  leave Settles it. Blocks: slice 3.
- Q-EXPLORE-004. RULE-EXPLORE-002: Where does key 5 put the members it shows, what do
  `2C5F:0271`, `2C5F:0139` and `28C9:2A81` redraw, what does bit 3 of an entry's byte 5 stand for,
  and which stores to `57E0:13FA` belong to Collapse Party and to loading a game? Settles it:
  `28C9:2C9E` in full, the three redraw routines, the writers of the entry flags, and the two
  stores of overlay 187 at offsets `0x0F35` and `0x0F76`. Tried: the handlers of keys 5 and 6 and
  a byte search for stores to the byte (FND-EXPLORE-005). Blocks: slice 3.
- Q-EXPLORE-006. RULE-EXPLORE-005: How long does one step of a walk take, and how does the wall
  follower of the route search treat a follower that lands on an earlier node? Settles it:
  `31E0:271E`, `31E0:1808` and the per-pass mover that changes the position words while bit 5 of
  the slot record's first byte is set, `1695:01D1`, and `1B2E:042D..1B2E:0579`; or a run from a
  checkpoint whose leader has a position, timing a Walk click. Tried: FND-RNG-068 and
  FND-EXPLORE-008 read and run the click path; FND-EXPLORE-014 reads the planning and the step
  start, and FND-EXPLORE-015 shows why the play-ready click finds no route, which leave
  Settles it. Blocks: slice 3.
- Q-EXPLORE-010. RULE-EXPLORE-005: What do the calls in step 8 of `fn_2D40_2F34` (for slots 48
  and up, FND-RNG-068), `fn_1695_01D1` and the fight check `fn_2D40_1197` do, what are
  `g_4C13_0367`, `g_57E0_145E`, `g_57E0_19AE` and `g_57E0_19BF` for, what units does
  `fn_1B0B_000C` give, and what does `plan_walk`
  read where its procedure has no value: the distance local when the wait test did not run, the
  step-record bytes after a queue's end, the step record on its first two exits, and the records of
  slot 9999? Settles it: readings of those routines, of the writers and readers of the four words,
  and of the stack the planner's frame reuses, or a run reading the step records. Tried: split from
  Q-EXPLORE-006; FND-EXPLORE-014 reads `plan_walk` and names these; FND-EXPLORE-016 reads the
  two object-target routines, which leave it. Blocks: slice 3.
- Q-EXPLORE-007. SCR-EXPLORE-001: Which resources and routines compose the exploration view, place
  its region and actors, draw the pointer modes and switch the party display? Settles it: the
  bounded resource graph and drawing/input paths for the view, followed by owner captures where
  native presentation depends on the runtime. Blocks: Survey screen coverage and slice 3.

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

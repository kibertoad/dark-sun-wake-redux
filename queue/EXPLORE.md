# EXPLORE

Next ID: Q-EXPLORE-015

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
- Q-EXPLORE-004. RULE-EXPLORE-002: Where does key 5 put the members it shows, what do
  `2C5F:0271`, `2C5F:0139` and `28C9:2A81` redraw, what does bit 3 of an entry's byte 5 stand for,
  and which stores to `57E0:13FA` belong to Collapse Party and to loading a game? Settles it:
  `28C9:2C9E` in full, the three redraw routines, the writers of the entry flags, and the two
  stores of overlay 187 at offsets `0x0F35` and `0x0F76`. Tried: the handlers of keys 5 and 6 and
  a byte search for stores to the byte (FND-EXPLORE-005). Blocks: slice 3.
- Q-EXPLORE-013. RULE-EXPLORE-005: What does the route search `1B2E:02D2` read when a wall
  follower is boxed in on all four turns and its step returns -1, so that it looks up the link of
  node -2 (`1B2E:0446`)? Settles it: a reading of `1B2E:0441..1B2E:0470` with that value and of the
  memory before the node array. Tried: FND-EXPLORE-021 reads the follower handling and leaves this
  case. Blocks: none.
- Q-EXPLORE-010. RULE-EXPLORE-005: What do the calls in step 8 of `fn_2D40_2F34` (for slots 48
  and up, FND-RNG-068) and the fight check `fn_2D40_1197` do, what are
  `g_57E0_19AE` and `g_57E0_19BF` for, what units does
  `fn_1B0B_000C` give, and what does `plan_walk`
  read where its procedure has no value: the distance local when the wait test did not run, the
  step-record bytes after a queue's end, the step record on its first two exits, and the records of
  slot 9999? Settles it: readings of those routines, of the writers and readers of the two words,
  and of the stack the planner's frame reuses, or a run reading the step records. Tried: split from
  Q-EXPLORE-006; FND-EXPLORE-014 reads `plan_walk` and names these; FND-EXPLORE-016 reads the
  two object-target routines, which leave it; FND-SCRIPT-028 reads the tile trigger walker, which
  takes that part out. Blocks: slice 3.
- Q-EXPLORE-007. SCR-EXPLORE-001: Which resources and routines compose the exploration view, place
  its region and actors, draw the pointer modes and switch the party display? Settles it: the
  bounded resource graph and drawing/input paths for the view, followed by owner captures where
  native presentation depends on the runtime. Blocks: Survey screen coverage and slice 3.

## Emulated call

None.

## Agent run

- Q-EXPLORE-012. RULE-EXPLORE-004, RULE-EXPLORE-005: Does any code read the byte `4E71:0000`?
  Settles it: a run that scans every instruction's accesses for `4E71:0000` over a whole load,
  and over a load of a saved game where the byte is 0. Tried: FND-EXPLORE-018 finds three stores of 1
  and no read through a segment word loaded within six instructions or a pushed far pointer.
  Tried: FND-EXPLORE-023 scans a Walk click, a three-cell walk and a key step in the first fight and
  finds only those stores. Tried: FND-EXPLORE-026 scans a walk, two key steps and a save outside a
  fight and the first 6.9 s of a load, and finds only those stores; the rest of the load, from
  `preload` of run `slprep`, and a load of a game where the byte is 0 remain. Blocks: none.
- Q-EXPLORE-014. RULE-EXPLORE-005: How long does a cell take for a follower, for slot 2's set
  (SCMD 3917) outside a fight, and at other CPU speeds, and what sets the length of a map pass?
  Settles it: a run from `explore` (FND-EXPLORE-024) that walks the party with followers and times
  each follower's frame calls, the same at a second cycle count, and a reading of what the map
  loop does in a long pass. Tried: FND-EXPLORE-022 times slot 2 in the first fight and
  FND-EXPLORE-025 times the leader with sets 599, 25 and 598 outside it, one frame per pass in
  both. Blocks: slice 3.

## Live session

None.

## Source

None.

## Blocked

None.

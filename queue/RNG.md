# RNG

Next ID: Q-RNG-039

## Static

- Q-RNG-014. RULE-RNG-001, RULE-SOUND-004, RULE-VIDEO-006, RULE-VIDEO-005: Which game events
  empty overlay 173's removal list into overlay 206 `+0E1E`, whose dissolve and flag-1 cry draw?
  Settles it: the events that call overlay 188 `+1445` and `+14DE`, overlay 204 `+1322`, and the
  routines that hold overlay 204 `+02A4` and `28C9:0CE3` (with their starts), which FND-RNG-063
  finds among the nine drains, and when a list left filled by a path that returns before its
  drain, such as the fight routine's with its byte `bp-1` at 0, is emptied. Split on 2026-10-21
  into this item, the damage that fills the list (Q-RNG-026) and the flag-0 cry (Q-RNG-027).
  Tried: FND-RNG-030 and FND-RNG-043 read `+1257`, `+151E` and `+0E1E` and find `+24C7` emptying
  the list; FND-PARTY-140 reads `+0E1E`'s branch for party slots; FND-RNG-050 reads every call in
  `+0E1E` (its long routes are Q-RNG-022); FND-RNG-063 reads the nine drains and finds seven after
  the fight, attack and per-round effect routines, two hit routines and two damage loops of overlay
  188. RULE-SOUND-004 takes the cry choice and RULE-VIDEO-006 the dissolve. Blocks: slices 2-7.
- Q-RNG-026. RULE-RNG-001, RULE-VIDEO-005: Which game events reach overlay 173 `+22C4`, the
  damage that adds a removed combatant to the removal list? Settles it: the conditions under
  which its twelve callers call it: overlay 179 `+0D9C`, `+1D22`, `+27BF`, `+27D3` and `+2A3A`,
  overlay 195 `+0295`, `+050E`, `+0825`, `+0845`, `+0885` and `+0A5A`, and overlay 204 `+1277`.
  Tried: FND-RNG-043 lists the callers and reads `+22C4`'s branches and the list it fills;
  FND-RNG-063 finds the per-round effect routine and the fight routine's call of overlay 195
  `+0789` followed by drains. Blocks: slices 2-7.
- Q-RNG-027. RULE-RNG-001, RULE-SOUND-004: Which game events reach overlay 179 `+2A05` with the
  word at `DS:0DAB` 1, so that overlay 206 `+0B81` gives the flag-0 cry? Settles it: the
  conditions under which its thirteen callers call it (overlay 179 `+1078`, `+141D`, `+1470`,
  `+14D9`, `+1500` and `+21A1`, overlay 188 `+1478` and `+1553`, overlay 193 `+1215`, overlay 197
  `+1A85`, `+2ED3` and `+3ADE`, overlay 211 `+1EF5`), and the writers of `DS:0DAB`. Tried:
  FND-RNG-043 reads `+2A05` and lists its callers; FND-RNG-063 reads overlay 188's two as damage
  loops over the slots `2D40:0D94` returns. Blocks: slices 2-7.
- Q-RNG-022. RULE-RNG-001: Do the unload `31E0:05BB`, `2C5F:0139`, overlay 197 `+30ED` and
  overlay 206 `+0A4E` draw or reseed when overlay 206 `+0E1E` calls them? Settles it: whether the
  object the unload and `2C5F:0139` work on from `+0E1E` can be a party member's (state 2) with
  effect rows; which effect bytes the rows with word 0x120 that `+30ED` removes can carry; what
  overlay 195 `+1000` draws or reseeds for each effect byte `+1873` passes it; and whether overlay
  193 `+0DC7` takes its area-effect route `+12A8` when `+0A4E` reaches it through `31E0:2D6E`.
  Tried: split from Q-RNG-014. FND-RNG-050's per-callee walks reach the targets from these five
  starts with two unresolved table jumps in segment `2707`, and find no draw in the other six
  callees. Tried: FND-RNG-060's cut walks find every route of these four through overlay 193
  `+1DB0` and `+1873` into overlay 195 `+1000`, or for `+0A4E` through `+0DC7`; `1695:02E7` and
  overlay 201 `+05D5`'s script routes moved to Q-RNG-024. Blocks: slices 2-7.
- Q-RNG-024. RULE-RNG-001: Does the script run by `1695:02E7` from overlay 206 `+0E1E` (with the
  slot and 0xF460), or the event script overlay 206 `+0000` runs through `31E0:0121` and
  `1695:01D1` when overlay 201 `+05D5` calls it, draw or reseed? Settles it: the script resources
  each call selects, and which of the instructions they can reach have handlers that draw (such as
  `0x52`) or reseed (request 27 of `0x22`, FND-RNG-027), or a reading that shows those calls run
  none; and whether overlay 201 `+05D5`'s
  calls of `31E0:05BB` and overlay 197 `+30ED` draw (with Q-RNG-022). Tried: split from
  Q-RNG-022. FND-RNG-060 finds every route of `1695:02E7` to a draw through `1695:06FF`,
  `1695:075D`, `172C:000C` and `172C:00A1`, the script interpreter, and overlay 201 `+05D5`'s other
  routes through overlay 206 `+0000`, `31E0:05BB`, overlay 197 `+30ED` and `1695:02E7`. Blocks:
  slices 2-7.
- Q-RNG-003. RULE-RNG-001, RULE-VIDEO-005: Which combatants cast `DATA` 293 with the cast
  routine's byte `bp+0x1A` set? Settles it: the regions, scripts or routines that create objects
  428 and 430, the only shipped combatants with special attack byte 6 or 13, or a reading that
  shows none does. Split on 2026-10-21 from the party's target cursor, now Q-RNG-025.
  Tried: FND-RNG-053 finds the cast routine's reseed conditional on `DATA` 293 and its byte
  `bp+0x1A`; FND-RNG-055 finds that only overlay 174 `+084D`'s special attack 6 and overlay 197
  `+111A`'s return by special attack 13 pass 293 with the byte set among the combatants' routes;
  FND-RNG-062 finds a monster's byte `+0x0E` copied unchanged from its `RDFF` object's combatant
  chunk, no shipped `SAVE` resource, and 6 only in `RDFF` 428 and 13 only in `RDFF` 430.
  FND-RNG-009, FND-RNG-010, FND-RNG-014, FND-RNG-015, FND-RNG-021 and FND-RNG-061 answer the
  startup path, the second root and the near buffer. Blocks: reproducible seeding and
  Q-PARTY-081's later recorded gameplay run.
- Q-RNG-025. RULE-RNG-001, RULE-VIDEO-005: Can the party's target cursor cast `DATA` 293 with the
  cast routine's byte `bp+0x1A` set? Settles it: whether `DS:43F5` can hold 293 when overlay 189
  `+0B3C` runs, which needs the mode-0 reading of overlay 211 `+16ED`'s `SI` that Q-PARTY-105 asks
  for, and a census of the stores to an item's word `0x10` showing whether any can write 294.
  Tried: FND-RNG-055 finds overlay 208 `+179F` passing `DS:9BE2` to the spell dispatcher with
  the byte at `DS:9BD3` as `bp+0x1A`, and the party's spell, psionic and item numbers of
  FND-PARTY-242 and FND-PARTY-121 below 293 or never 294 in an item; the stores at `DS:9BE2` were
  not followed. Tried: FND-RNG-064 finds both stores in overlay 208 `+1612` and its six callers;
  only overlay 189 `+0B3C` (with `DS:43F5`) and overlay 213 `+0B82` (with an item's word `0x10`
  less 1) pass the byte as 1, so the other four callers are out. Blocks: reproducible seeding.
- Q-RNG-005. RULE-RNG-001: Do the string stores and copy helpers on FND-RNG-058's walk write the
  clock conversion's storage? Settles it: the destination of each string store and copy-helper
  call on that walk (among them the append into DS:33F2), each shown to miss the windows of
  Q-RNG-005's storage (the environment-array root DS:3920, classification-index window DS:34F5..3675, calendar tables DS:3950..3976, label pointers DS:3976..397A, offset/enable record DS:397A..3980, name DS:3980..3983 and label buffers DS:A4E4..A4EC) or listed with what it writes. Tried: FND-RNG-016, FND-RNG-017,
  FND-RNG-018, FND-RNG-019 and FND-RNG-020 read the calendar bodies, globals, initializers,
  constructor and timezone frames, which establish local sites only. Tried: FND-RNG-058 answers
  the direct reader/writer part with a census of displacement and literal-pointer operands from
  all 21 established entries, and FND-RNG-061 finds DS equal to SS at the helper's callers; the
  computed-store and unresolved-transfer parts moved to Q-RNG-029 and Q-RNG-030. Blocks: complete
  seed-source reading.
- Q-RNG-029. RULE-RNG-001: Does a computed or segment-aliased store on FND-RNG-058's walk, other
  than its string stores and copy helpers, write the clock conversion's storage (the windows
  Q-RNG-005 lists)? Settles it: each store whose address is formed from a register or a segment
  other than DS on that walk, shown to miss the windows or listed with what it writes. Tried:
  FND-RNG-058 covers only displacement and literal-pointer operands, and FND-RNG-061 finds DS
  equal to SS at the helper's callers. Split from Q-RNG-005. Tried: FND-RNG-067 watches the
  windows during 40 s of play from the two-character checkpoint and finds one change, the store
  of `2D40:2F34` that FND-RNG-068 reads at `DS:3956`; the census of the other computed stores is
  not taken. Blocks: complete seed-source reading.
- Q-RNG-031. RULE-RNG-001, BUG-COMBAT-001: Can a call of the copy routines `1000:0452` or
  `1000:3F5A` put 9999 in `DS:426D` or `4C13:0369`? Settles it: for the 38 calls whose destination
  is a far pointer in memory or an argument, and for `2D40:093F`'s index and `1AA0:0625`'s base
  (FND-RNG-087), where the destination can point, each shown to miss both words or listed with the
  values it can copy there. Tried: FND-RNG-073 finds no direct store of 9999 in `DS:426D`,
  `DS:0DA5` or `4C13:0369`, and FND-SAVE-015 finds `DS:426D` in no `SAVE` resource and
  `4C13:0369` in `SAVE` 46. Tried: FND-RNG-087 classifies the destinations of the 104 calls and
  finds 64 that cannot reach either word with 9999 as the index. Split into Q-RNG-031, Q-RNG-037
  and Q-RNG-038. Blocks: complete seed-source reading.
- Q-RNG-037. RULE-RNG-001, BUG-COMBAT-001: Can an inline string move or a copy or string routine
  other than `1000:0452` and `1000:3F5A` put 9999 in `DS:426D` or `4C13:0369`? Settles it: the
  destinations of the inline moves that `block_copies.py` lists and of the callers of each other
  copy, fill and string routine, including what `2D40:3E64`, `2D40:3DC2` and `1000:374E` write
  through `DS:420A` and `DS:4226` and how far (FND-RNG-087), each shown to miss both words or
  listed with the values it can write there. Tried: FND-RNG-067, FND-RNG-069 and FND-RNG-070 find
  the end of combat leaving 9999 until a leader key, and FND-RNG-078 with FND-PARTY-089 shows
  overlay 173 `+059A` storing 9999 only on its end-of-combat path; FND-RNG-087 finds the pointers
  formed between `DS:4200` and `DS:426D`. Split from Q-RNG-031. Blocks: complete seed-source
  reading.
- Q-RNG-038. RULE-RNG-001, BUG-COMBAT-001: Can a file read put 9999 in `DS:426D` or `4C13:0369`?
  Settles it: every DOS read request (`int 0x21` with `AH` 0x3F) and its callers, with each buffer
  shown to miss both words or listed with the file and count it reads there, including
  `44DE:01AF`'s 3 bytes at `DS:4252` (FND-RNG-087). Tried: FND-SAVE-015 finds `4C13:0369` in
  `SAVE` 46, which a load restores from what a save took, and `DS:426D` in no `SAVE` resource.
  Split from Q-RNG-031. Blocks: complete seed-source reading.
- Q-RNG-034. RULE-RNG-001, BUG-COMBAT-001: Can `31E0:2C86` pass 9999 to `2D40:10AE`? Settles it:
  the slot argument at the four calls of `31E0:0121` that FND-RNG-085 leaves unbounded (overlay 171
  `+0C25`, overlay 179 `+26DE`, overlay 182 `+04E0`, overlay 184 `+12E5`), the stores through
  copies of the far pointer `DS:67B7`, and the block copies and file reads into its entries, each
  shown not to put 9999 in an entry's word `+6` or listed with when it does. Tried: FND-RNG-072
  finds `31E0:2D6E` storing `2D40:10AE`'s result through the address of `DS:426D` when the placed
  slot equals it. Split from Q-RNG-031. Tried: FND-RNG-085 finds three direct stores of the word
  `+6`, of which only `31E0:0EFF`'s can store 9999, and bounds the slot at 21 of its callers'
  calls; FND-RNG-086 sees `31E0:2C86` pass only its own copy of the slot in the bug's run.
  Blocks: complete seed-source reading.

- Q-RNG-007. RULE-RNG-001: Can a reached computed, aliased or bulk write
  change rng_state apart from its generator and setter? Settles it: the
  handles the callers of `1000:07B0` and `1000:3EDA` (FND-RNG-059) can pass,
  shown below 67 or listed; the destinations of writes through the stream
  record pointers, heap and far pointers, string stores and copy helpers on
  FND-RNG-058's walk, other segment and offset pairs for `DS:384A..384E`,
  and the code behind the walk's nine unresolved transfers, each shown to
  miss rng_state or listed with what it writes. Tried: FND-RNG-001 and
  FND-RNG-002 identify direct generator/setter state accesses and retain
  explicit computed-access exclusions. FND-RNG-009, FND-RNG-021 and
  FND-RNG-024..026 qualify caller sequences without a complete state-access
  census. Split from Q-RNG-001 so state coverage can be answered separately
  from rule/draw mapping. Tried: FND-RNG-059's census of the walk finds only
  the setter and generator naming rng_state and no literal pointer into it,
  and one indexed array below it, the handle flags, reaching it for handles
  67 and 68; the direct and literal part is answered. Blocks: complete RNG
  state and reseeding coverage.
- Q-RNG-021. RULE-RNG-001, RULE-MAGIC-016, RULE-MAGIC-017, RULE-MAGIC-018, RULE-MAGIC-019,
  RULE-MAGIC-020, RULE-MAGIC-021, RULE-MAGIC-022: What do the 22 `DATA` numbers of overlay 179
  `+1373`'s rolled cases, the details and combatant bytes the cases read, the effect codes they
  apply, and the callees that take the totals stand for? Settles it: the names or uses of the 22
  `DATA` numbers; readings of overlay 197 `+2D05`, `+2E40` and `+2EC0`, overlay 177 `+0CB3`,
  overlay 193 `+1A2D`, `+166F` and `+194F`, overlay 173 `+08EE` and the other callees FND-RNG-047
  names, `fn_2D40_3E64` and `fn_31E0_0121`; and what reads byte `+5` of the entries at
  `57E0:67B7`. Tried: split from Q-RNG-017. FND-RNG-044 names the arguments and callees and
  records their uses of the totals. FND-RNG-047 reads every path of the rolled cases and
  RULE-MAGIC-016 to RULE-MAGIC-022 take them, leaving these names as open questions. Tried:
  FND-RNG-051 reads what every direct caller of the hit routine and every queued cast passes as
  its byte arguments `a` and `g`; the values passed on from callers' own arguments moved to
  Q-MAGIC-016. Blocks: slices 2-7.

## Emulated call

None.

## Agent run

- Q-RNG-006. RULE-RNG-001, RULE-VIDEO-005: Does the admitted pinned runtime route the clock
  requests through the reviewed date/time handlers with the required storage and register
  identity? Settles it: a session-package run of the pinned rust-dos, stopped at FND-RNG-014's
  startup clock caller and service-return stops, that reads the `INT 21` vector at each request
  (no guest hook), the registers and the game's buffers before and after each request (the
  service writes only its result registers and the buffers get only the caller's stores), and
  records the session's deterministic start and clock-offset settings. Parts it needs: start it
  and bring it to a given state (agent, tested states only; startup is one) and read memory and
  set breakpoints (agent, named fields and declared stops; FND-RNG-014's clock stops were verified
  under v1.7.2, so the probe needs a current-pin attempt). The release correspondence moved to
  Q-RNG-028. Moved from Static on 2026-10-21: the static readings left only what a run shows.
  Tried: FND-RNG-015 identifies returned word stores; FND-RNG-023 completely reads the game's
  request preparation and order. Tried: SRC-RUSTDOS-172 locates the v1.7.2 source tag and reads
  local handler assignments, separate CMOS samples and host-clock and offset access. Tried:
  SRC-RUSTDOS-172's connected real-mode reading answers default-vector dispatch, local
  DS/SI/SS/BP preservation, common caller-save and DOS-state writes, service flag return and the
  deterministic clock boundary and stall algorithm. Blocks: complete admitted seed-source input
  domain.

## Live session

None.

## Source

- Q-RNG-028. RULE-RNG-001, RULE-VIDEO-005: Do the clock services of rust-dos v1.7.5, the
  release the session package pins, match the v1.7.2 handler bodies SRC-RUSTDOS-172 reads?
  Settles it: the v1.7.5 tag's source for the `INT 21` date and time handlers and their CMOS and
  host-clock access compared with SRC-RUSTDOS-172, or the release notes from v1.7.2 to v1.7.5
  naming the changes there. Tried: SRC-RUSTDOS-172 reads tag v1.7.2 only; split from Q-RNG-006 on
  2026-10-21. Blocks: Q-RNG-006.

## Blocked

None.

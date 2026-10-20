# RNG

Next ID: Q-RNG-019

## Static

- Q-RNG-014. RULE-RNG-001, RULE-SOUND-004: Which game events reach overlay 206's cry choice and
  its dissolve of a removed combatant, and which rule or screen entry takes the dissolve? Settles
  it: the callers of overlay 206 `+0B81` (the cry with flag 0) and `+0E1E` (the cry with flag 1
  and the dissolve) with the conditions that reach each call, whether `+0E1E`'s calls before the
  dissolve draw, any computed or bulk writer of the voice tables at `DS:290A..291E`, and the rule
  or screen entry that takes FND-RNG-030's dissolve. Tried: split from Q-RNG-001 and Q-RNG-013.
  FND-RNG-030 reads `+1257` and `+151E` and the two calls in `+0E1E`, and FND-PARTY-140 reads
  `+0E1E`'s branch for party slots. RULE-SOUND-004 takes FND-RNG-030's cry choice; the events
  and the dissolve remain. Blocks: slices 2-7.
- Q-RNG-015. RULE-RNG-001, RULE-COMBAT-011: Which rules describe overlay 173's fight set-up words,
  target pick and removal drop, and what do the values its four draw routines read hold? Settles
  it: for FND-RNG-031's fight set-up words, target pick and removal drop, the meaning of the words
  at `4C4F:00DC + 4 * c` and the byte at `4C4F:0006`, the bodies of `27E5:02D9` and overlay 197
  `+254B` (and whether they draw), the lists overlay 173 `+0000` and `+0159` build, and the rule
  or screen entries that take each procedure; and for the item-break test of RULE-COMBAT-011, the
  meaning of the item and type record bytes it reads and what overlay 173 `+3E6B` does. Tried: split from Q-RNG-001.
  FND-RNG-031 reads the four routines and their direct callers; FND-PARTY-091, FND-PARTY-097,
  FND-PARTY-099 and FND-PARTY-244 read the swing, the fight routine, the saving throw and the
  trigger conditions. RULE-COMBAT-011 takes the item-break test; the other three routines and the
  record meanings remain. Blocks: slices 2-7.
- Q-RNG-008. RULE-RNG-001: Can the generator, a helper or the seed setter be reached through a
  stored far pointer, a computed call or a segment alias? Settles it: the words of the file equal
  to each routine's offset with a relocated or fixed-up segment, the computed call sites whose
  targets can include those routines, and the segment aliases on the admitted paths. Tried: split
  from Q-RNG-001 and Q-RNG-003. FND-RNG-001, FND-RNG-002 and FND-RNG-009 search direct calls only
  and list computed transfers as excluded. Blocks: slices 2-7.
- Q-RNG-009. RULE-RNG-001: Which rules do the eleven `random_between` sites of overlays 176 and
  179 serve, and with which inputs? Settles it: the callers of overlay 176's psionic action and
  resistance checks and of overlay 179's paired-coordinate samplers (through the overlay 193
  caller), the record and table fields that supply each site's endpoints, and the rule each
  belongs to. Tried: split from Q-RNG-001. FND-RNG-025 reads overlay 176's five sites (fixed and
  dynamic endpoints, two contest rolls before the comparison, the conditional second predicate
  call, no draw when the bounds are equal) and FND-RNG-026 overlay 179's six; their inputs and
  rule mapping remain. Blocks: slices 2-7.
- Q-RNG-003. RULE-RNG-001: When does the game reseed: which paths reach the two seed callers,
  overlay 180 +065E and overlay 204 +001E, and with what near buffers? Settles it: the callers
  that reach each seed caller and their branches, and on each such path the DS/SS identity of
  the near date and time buffers the clock helper fills. Tried: FND-RNG-009 (both call sequences
  seed from the helper's low return word), FND-RNG-010 (fresh startup seeds 44976 from overlay
  180), FND-RNG-014 (the startup path's buffer identity), FND-RNG-015 (the wrapper's return) and
  FND-RNG-021 (the second root reseeds before its offset loop) answer the startup path and the
  second root's local role; the other paths remain. Blocks: reproducible startup seeding and
  Q-PARTY-081's later recorded gameplay run.
- Q-RNG-004. RULE-RNG-001: Which environment strings reach the clock conversion's initializer,
  and with what bounds? Settles it: the population and bounds of the environment pointer array
  and its strings on the admitted paths, and the initializer's branches for parsed offsets and
  daylight labels beyond the default. Tried: FND-RNG-016 (calendar arithmetic and the daylight
  helper), FND-RNG-017 (the measured startup takes the default path) and FND-RNG-018 (the
  initializer's guard, default, integer offset and first-label branches) answer the local bodies
  and the default path; which strings arrive remains. Blocks: complete reproducible seed-source
  semantics.
- Q-RNG-012. RULE-RNG-001, RULE-VIDEO-005: Which date and time values does each caller of the clock conversion
  pass, and in which near buffers? Settles it: on each admitted caller path, the indexed date and
  time fields the conversion reads, their ranges, and the buffers' segment identity. Tried: split
  from Q-RNG-004. FND-RNG-009 identifies the conversion chain, FND-RNG-014 measures one startup
  input and return, FND-RNG-015 records the buffer access widths and FND-RNG-023 the date-before-
  time sampling without a retry; the other callers' inputs remain. Blocks: complete reproducible
  seed-source semantics.
- Q-RNG-005. RULE-RNG-001: Which paths initialize, read or change the clock
  conversion's global/table storage, and does each access retain its declared
  identity? Settles it: reached reader/writer and alias coverage for the
  environment-array root DS:3920, classification-index window DS:34F5..3675,
  calendar tables DS:3950..3976, label pointers DS:3976..397A and offset/enable
  record DS:397A..3980; label-buffer ownership/lifetime, indirect and bulk
  writes, and continued segment identity on admitted paths. Start with a
  mechanism/site census and positive controls from the initializer's explicit
  stores and helper-mediated label writes; candidates alone prove no absence.
  Tried: FND-RNG-016 reads the calendar bodies and shipped windows,
  FND-RNG-017 measures the default-path globals and FND-RNG-018 reads the
  local initializer and helper writes. These establish local sites, not
  complete storage/writer coverage. Split from Q-RNG-004 to separate that
  coverage from service/input admission. Tried: FND-RNG-019 identifies the
  environment constructor's record-backed entry, its root store/array
  construction, startup label-buffer bulk zero and the timezone initializer's
  separate stored caller. Those local producer/caller gaps are answered;
  native constructor metadata, other writers, source/callback/allocator
  admission and continued storage identity remain. Tried: FND-RNG-020
  verifies the actual near constructor frame, requested spans, root/mirror
  and terminated member metadata, then the distinct far startup timezone
  frame/default record. That native prerequisite is answered for this
  admitted case; other writers, full lifetime/allocator/source admission
  and other paths remain. Blocks: complete seed-source reading.

- Q-RNG-006. RULE-RNG-001: Does the admitted pinned runtime route the clock
  requests through the reviewed date/time handlers with the required storage
  and register identity? Settles it: source-to-release correspondence,
  installed INT 21 routing and guest-hook admission, nonpaged real-mode
  entry/ordinary return admission, common-memory writes separated from the
  game's buffers, and active deterministic start/clock-offset state and
  admitted date domain. Local preservation, common effects, clock construction
  and stall/update algorithms are answered conditionally in SRC-RUSTDOS-172;
  actual offset writers and continued storage identity remain. Local result
  fields and independent reads are described by
  SRC-RUSTDOS-172, rather than assumed generic DOS guarantees.
  Tried: FND-RNG-015 identifies returned word stores; FND-RNG-023 completely
  reads the game's request preparation/order without local validation.
  Split from Q-RNG-004 for external-service semantics. Tried:
  SRC-RUSTDOS-172 locates the exact pinned source tag and reads local
  handler assignments, separate CMOS samples and host-clock/offset access.
  The source acquisition and local handler-body obligations are answered;
  connected runtime admission remains. Moved from Source to Static now
  that the required source snapshot is available. Tried: SRC-RUSTDOS-172's
  connected real-mode reading answers default-vector dispatch, local
  DS/SI/SS/BP preservation, common caller-save/DOS-state writes, service
  flag return and the deterministic clock boundary/stall algorithm. These
  source-body obligations are removed from Settles it; actual route,
  memory separation, active clock state and release correspondence remain.
  Blocks: complete admitted
  seed-source input domain.

- Q-RNG-007. RULE-RNG-001: Which reached direct, computed, aliased or bulk
  accesses can read or change rng_state apart from its known generator and
  setter? Settles it: a reached access-mechanism census with controls for
  each covered kind, including overlapping/bulk writes and far/segment
  aliases, and storage/segment identity on the admitted paths. Separate
  unqualified operand candidates from reached accesses; literal displacement
  searches cannot settle computed pointers. Tried: FND-RNG-001 and
  FND-RNG-002 identify direct generator/setter state accesses and retain
  explicit computed-access exclusions. FND-RNG-009, FND-RNG-021 and
  FND-RNG-024..026 qualify caller sequences without a complete state-access
  census. Split from Q-RNG-001 so state coverage can be answered separately
  from rule/draw mapping. Blocks: complete RNG state and reseeding coverage.
- Q-RNG-017. RULE-RNG-001: Which rule entries take the dice of overlay 179 `+1373`'s effect
  switch (FND-RNG-034), the tests and formula FND-RNG-035 reads and the per-round effects of
  FND-RNG-039, and what do the `DATA`
  numbers, the arguments `a` and `g`, and the callees that take the totals stand for? Settles it:
  rule entries, new or revised, for the healing, damage, duration, dispel, death, raising and
  reincarnation cases with their dice, for overlay 204 `+214E`'s
  saving throw, with the callers of `+214E`, and for overlay 195's per-round
  effect damage; the names or
  uses of the 22 `DATA` numbers; the writers of `+0D2F`'s byte arguments at `bp+0x1C` and
  `bp+0x1E`; and readings of overlay 197 `+2D05`, `+2E40` and `+2EC0`, overlay 177 `+0CB3` and
  overlay 193 `+1A2D`. Tried: split from Q-RNG-013. FND-RNG-034 reads the nineteen sites and the
  cases that reach them; FND-PARTY-096, FND-PARTY-099 and FND-PARTY-112 read `+1373`'s table, the
  saving throw and the gates before it; FND-RNG-035 reads the seven routines of overlays 191,
  193, 198, 204 and 211 and their direct callers, and FND-RNG-036 overlay 195 `+000E`, `+00D1`
  and `+152F`, which FND-RNG-039 replaces with the record `+152F` reads corrected. Tried: RULE-MAGIC-005, RULE-MAGIC-006 and RULE-AI-003 take FND-RNG-035's `DATA`
  dice formula, `DATA` 306's d20 test and the spell-use test, with FND-RNG-038 for the byte the
  last one reads. Tried: RULE-COMBAT-012 and RULE-COMBAT-013 take overlay 195's d10 table and
  d100 test (FND-RNG-039). Tried: RULE-ITEM-007, RULE-ITEM-008 and RULE-ITEM-009 take overlay
  191's three item tests, with FND-ITEM-010 for the cases that run them; Q-ITEM-004 holds what
  they act on. Blocks: slices 2-7.
- Q-RNG-018. RULE-RNG-001: Which rule entries take overlay 197's ten `roll_sum` draws that
  FND-RNG-037 reads, and what do the values they test stand for? Settles it: rule entries, new or
  revised, for the d6 of `+111A` for a target with record byte `+0x0E` 13, the d100 block of
  `+1760` and the 2d6 total it queues after a saving throw, the d20 test of `+314E`, case 4 of
  `+1A94` with its d100 under the level in class code 11, the creature picks of `+3315` for its 18
  `DATA` numbers and the 4d20 pool of `+399F` for `DATA` 71; what the block bits these read,
  record byte `+0x0E` 13, details byte `+0x12` 8, 9 and 14 and class code 11 stand for; what
  overlay 179 `+1304` and the cast queue do with the 2d6 total; and what overlay 172 and overlay
  211 do with `+1A94`'s result. Tried: split from Q-RNG-013. FND-RNG-037 reads the ten sites, their
  routines and direct callers; FND-PARTY-107, FND-PARTY-112 and FND-PARTY-122 read the cast queue
  and the gates around the hit routine. Blocks: slices 2-7.

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

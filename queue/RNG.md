# RNG

Next ID: Q-RNG-017

## Static

- Q-RNG-013. RULE-RNG-001: Which rule does each of the 46 `roll_sum` sites that FND-RNG-028
  lists as not described and FND-RNG-030 does not read serve, with which count and sides, and how does each caller read the
  returned total? Settles it: for each site in overlays 179, 191, 193, 195, 197, 198, 204 and
  211 that FND-RNG-028 lists, a decode from an established entry showing it is an
  instruction, its two arguments' sources, the width and signedness at which the caller tests
  or stores the total, and the rule it belongs to. Tried: split from Q-RNG-001. FND-RNG-005 and
  FND-RNG-013 read the helper; FND-RNG-028 lists the sites, and FND-PARTY-071, FND-PARTY-074,
  FND-PARTY-082, FND-PARTY-099, FND-PARTY-150, FND-PARTY-168, FND-PARTY-249 and FND-TEXT-008
  describe the ten others. FND-RNG-030 qualifies overlay 206's four, the cry choice (moved to
  Q-RNG-014). Blocks: slices 2-7.
- Q-RNG-014. RULE-RNG-001: Which game events reach overlay 206's cry choice and its dissolve of a
  removed combatant, and which rules describe them? Settles it: the callers of overlay 206
  `+0B81` (the cry with flag 0) and `+0E1E` (the cry with flag 1 and the dissolve) with the
  conditions that reach each call, whether `+0E1E`'s calls before the dissolve draw, and the
  rule or screen entries that take FND-RNG-030's procedures. Tried: split from Q-RNG-001 and
  Q-RNG-013. FND-RNG-030 reads `+1257` and `+151E` and the two calls in `+0E1E`, and FND-PARTY-140
  reads `+0E1E`'s branch for party slots. Blocks: slices 2-7.
- Q-RNG-015. RULE-RNG-001: Which rules describe overlay 173's four draw routines, and what do the
  values they read hold? Settles it: for FND-RNG-031's fight set-up words, target pick, removal
  drop and item-break test, the meaning of the words at `4C4F:00DC + 4 * c` and the byte at
  `4C4F:0006`, the bodies of `27E5:02D9` and overlay 197 `+254B` (and whether they draw), the
  lists overlay 173 `+0000` and `+0159` build, the item and type record bytes the break test
  reads, and the rule or screen entries that take each procedure. Tried: split from Q-RNG-001.
  FND-RNG-031 reads the four routines and their direct callers; FND-PARTY-091, FND-PARTY-097,
  FND-PARTY-099 and FND-PARTY-244 read the swing, the fight routine, the saving throw and the
  trigger conditions. Blocks: slices 2-7.
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
- Q-RNG-012. RULE-RNG-001: Which date and time values does each caller of the clock conversion
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
- Q-RNG-016. RULE-RNG-001: Which rule entry takes the draw at overlay 204 +00E1, in the display
  offset loop script request 27 runs after it reseeds? Settles it: a rule or screen entry for the
  loop, its four modes and mode 4's draw of one of eight vectors, with the script requests that
  run it, or an existing entry revised to take it. Tried: split from Q-RNG-001. FND-RNG-021 reads
  the loop and its draw, FND-RNG-022 the script wrapper's argument widths and FND-RNG-027 the 44
  shipped instructions that issue request 27. Blocks: slices 2-7.

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

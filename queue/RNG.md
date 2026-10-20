# RNG

Next ID: Q-RNG-022

## Static

- Q-RNG-014. RULE-RNG-001, RULE-SOUND-004, RULE-VIDEO-006: Which game events reach overlay 206's
  cry choice and its dissolve of a removed combatant? Settles it: the conditions under which the
  callers FND-RNG-043 lists reach overlay 179 `+2A05` (the flag 0 cry, when the word at `DS:0DAB`
  is 1), overlay 173 `+22C4` (the damage that queues a slot) and overlay 173 `+24C7` (which empties
  the removal list into `+0E1E`); whether `+0E1E`'s calls other than `+151E` and overlay 173
  `+3136` draw; and any computed or bulk writer of the voice tables at `DS:290A..291E`. Tried:
  split from Q-RNG-001 and Q-RNG-013. FND-RNG-030 reads `+1257` and `+151E` and the two calls in
  `+0E1E`, and FND-PARTY-140 reads `+0E1E`'s branch for party slots. RULE-SOUND-004 takes the cry
  choice. Tried: FND-RNG-043 replaces FND-RNG-030 with the unload placed before the dissolve, and
  reads the one caller of each of `+0B81` and `+0E1E`, the removal list and `+22C4`'s branches
  that fill it; RULE-VIDEO-006 takes the dissolve. Blocks: slices 2-7.
- Q-RNG-009. RULE-RNG-001: Which rule do the six `random_between` sites of overlay 179's
  paired-coordinate samplers serve, and with which inputs? Settles it: overlay 193 `+0617`'s
  callers (the near call at `+03B5` and trampoline `573B:008E`), the source record and the 18-byte
  profile fields that supply each sampler's endpoints, what reads the paired words, and a rule
  entry for the samplers. Tried: split from Q-RNG-001. FND-RNG-025 reads overlay 176's five sites
  (fixed and dynamic endpoints, two contest rolls before the comparison, the conditional second
  predicate call, no draw when the bounds are equal) and FND-RNG-026 overlay 179's six; their
  inputs and rule mapping remain. Tried: FND-RNG-046 reads the callers and inputs of overlay 176's
  five, which RULE-MAGIC-014 and RULE-MAGIC-015 take; `direct_callers.py` finds overlay 179 `+0000`
  called only from overlay 193 `+0709`, `+0181` only from `+00C5` and `+00F4`, and `+025D` only
  from `+014D`. Blocks: slices 2-7.
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
  switch? Settles it: rule entries, new or revised, for the healing, damage, duration, dispel,
  death, raising and reincarnation cases of FND-RNG-044 with their dice and conditions. Tried:
  split from Q-RNG-013 and, for the parts below, into Q-RNG-019, Q-RNG-020 and Q-RNG-021.
  FND-RNG-044 replaces FND-RNG-034 with the default target of `+1373`'s table jump corrected to
  `+2752`, and reads the nineteen sites and the cases that reach them; FND-PARTY-096,
  FND-PARTY-099 and FND-PARTY-112 read `+1373`'s table, the saving throw and the gates before it.
  Blocks: slices 2-7.
- Q-RNG-019. RULE-RNG-001: Which rule entry takes overlay 204 `+214E`'s copy of the saving throw
  roll, and which callers reach `+214E`? Settles it: a rule entry for `+214E` with the callers
  and conditions that reach it. Tried: split from Q-RNG-017. FND-RNG-035 reads `+214E` and its
  direct callers; RULE-MAGIC-005, RULE-MAGIC-006, RULE-AI-003 and RULE-ITEM-007 to RULE-ITEM-009
  take FND-RNG-035's other routines. Blocks: slices 2-7.
- Q-RNG-020. RULE-RNG-001: Which rule entry takes overlay 195's per-round effect damage dice?
  Settles it: a rule entry for the per-round damage steps of overlay 195 `+00D1` that FND-RNG-039
  reads, with the record fields they read. Tried: split from Q-RNG-017. FND-RNG-039 replaces
  FND-RNG-036 and reads `+000E`, `+00D1` and `+152F`; RULE-COMBAT-012 and RULE-COMBAT-013 take
  its d10 table and d100 test. Blocks: slices 2-7.
- Q-RNG-021. RULE-RNG-001: What do the 22 `DATA` numbers of overlay 179 `+1373`'s rolled cases,
  the hit routine's byte arguments `a` and `g`, and the callees that take the totals stand for?
  Settles it: the names or uses of the 22 `DATA` numbers; the writers of `+0D2F`'s byte arguments
  at `bp+0x1C` and `bp+0x1E`; and readings of overlay 197 `+2D05`, `+2E40` and `+2EC0`, overlay
  177 `+0CB3` and overlay 193 `+1A2D`. Tried: split from Q-RNG-017. FND-RNG-044 names the
  arguments and callees and records their uses of the totals. Blocks: slices 2-7.

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

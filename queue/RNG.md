# RNG

Next ID: Q-RNG-023

## Static

- Q-RNG-014. RULE-RNG-001, RULE-SOUND-004, RULE-VIDEO-006, RULE-VIDEO-005: Which game events reach overlay 206's
  cry choice and its dissolve of a removed combatant? Settles it: the conditions under which the
  callers FND-RNG-043 lists reach overlay 179 `+2A05` (the flag 0 cry, when the word at `DS:0DAB`
  is 1), overlay 173 `+22C4` (the damage that queues a slot) and overlay 173 `+24C7` (which empties
  the removal list into `+0E1E`). Tried: split from Q-RNG-001 and Q-RNG-013. FND-RNG-030 reads
  `+1257` and `+151E` and the two calls in `+0E1E`, and FND-PARTY-140 reads `+0E1E`'s branch for
  party slots. RULE-SOUND-004 takes the cry choice. Tried: FND-RNG-043 replaces FND-RNG-030 with
  the unload placed before the dissolve, and reads the one caller of each of `+0B81` and `+0E1E`,
  the removal list and `+22C4`'s branches that fill it; RULE-VIDEO-006 takes the dissolve. Tried:
  FND-RNG-050 reads every call in `+0E1E`: six callees make no draw and overlay 201 `+05D5`, run
  for a combatant whose word `+0x1B` is 430, reseeds and draws through RULE-VIDEO-005's loop; the
  five callees that reach draws only through long routes moved to Q-RNG-022 and the voice tables'
  writers to Q-SOUND-009. Blocks: slices 2-7.
- Q-RNG-022. RULE-RNG-001: Do `1695:02E7`, overlay 206 `+0A4E`, overlay 197 `+30ED`, the unload
  `31E0:05BB` and `2C5F:0139` draw or reseed when overlay 206 `+0E1E` calls them, and do the
  calls of overlay 201 `+05D5` other than its two of overlay 204 `+0000`? Settles it: for each
  route FND-RNG-050 lists from these callees to the generator, `roll_sum`, `random_between` or the
  seed setter (through the movement routine `2D40:10AE` and the attack routine overlay 173
  `+1CC1`, overlay 193 `+0DC7`, `+166F` and `+1DB0`, the script handler `172C:170B`, or `+0E1E`
  itself), the conditions along it read from the callee's entry, or a walk with those conditions
  declared that reaches no target. Tried: split from Q-RNG-014. FND-RNG-050's per-callee walks
  reach the targets from these five starts with two unresolved table jumps in segment `2707`, and
  find no draw in the other six callees. Blocks: slices 2-7.
- Q-RNG-003. RULE-RNG-001, RULE-VIDEO-005: Which casts reseed the generator, and do the clock
  helper's stores reach its frame on the paths after startup? Settles it: the spells or actions
  that cast `DATA` 293 through overlay 193 `+003C` with its byte argument at `bp+0x1A` not 0
  (through overlay 193 `+0000`'s callers overlay 176 `+03BD`, overlay 177 `+04FE`, overlay 193
  `+22CF` and overlay 204 `+19AF`, and the cast queue), and `DS` at the entry of overlay 193
  `+003C`, overlay 201 `+05D5` and overlay 204 `+1F06` on the paths that reach them, or a check
  that every routine writing `DS` restores it. Tried: FND-RNG-009 (both call sequences
  seed from the helper's low return word), FND-RNG-010 (fresh startup seeds 44976 from overlay
  180), FND-RNG-014 (the startup path's buffer identity), FND-RNG-015 (the wrapper's return) and
  FND-RNG-021 (the second root reseeds before its offset loop) answer the startup path and the
  second root's local role; the other paths remain. Tried: FND-RNG-053 finds overlay 180 `+0141`
  called only from startup and overlay 204 `+0000` only from its four known calls, the cast
  routine's call conditional on `DATA` 293 and its byte `bp+0x1A`, `SS` written only at startup
  and in a balanced swap, and no `DS` write in the seed routines or their callers. Blocks: reproducible startup seeding and
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

None.

## Live session

None.

## Source

None.

## Blocked

None.

# RNG

Next ID: Q-RNG-007

## Static

- Q-RNG-001. RULE-RNG-001: Which rules call the generator and its modulo,
  inclusive-range and repeated-roll helpers, in what order, and can indirect
  code read or write the state? Settles it: verified callers of the generator
  and each helper, including relocations, far pointers and indirect state
  access; map each to a rule and its draw order. Tried: a complete
  instruction-text scan for the two state displacements and direct far calls
  to the setter (FND-RNG-001, FND-RNG-002), and the one direct consumer chain
  of the modulo helper (FND-RNG-007, FND-RNG-008), which identify no seed
  source, and whose consumer is the music selector of RULE-SOUND-003
  (FND-SOUND-012); a far-call search found a fourth direct
  caller of the generator, the script instruction `0x52` (FND-SCRIPT-011),
  which draws without a helper. Tried: FND-RNG-009 recovers two direct overlay
  seed callers; their complete source/reseeding contract is separated into
  Q-RNG-003. Its generator control query yields further overlay candidates
  requiring instruction and rule verification. Tried: FND-RNG-021 qualifies
  overlay 204 +00E1 as a direct draw conditional on retained offset state
  in the second reseeding root. That local consumer is answered; complete
  rule mapping, other candidates and indirect state access remain. Blocks: slices 2-7.
- Q-RNG-003. RULE-RNG-001: What complete input-to-seed contract and admitted
  reseeding paths do the two recovered seed callers use? Settles it: complete
  reading of the admitted caller paths and near-buffer DS/SS
  identity on other admitted paths; callers reaching overlay
  180 +065E and overlay 204 +001E; other indirect or aliased seed writes and
  calls. Tried: FND-RNG-009 proves both direct call sequences use only AX from
  the helper, whose optional output pointer is zero, and separates the complete
  conversion/reachability obligations from Q-RNG-001. FND-UI-039 observes a
  later state change without identifying its writer. Tried: FND-RNG-010
  observes the admitted startup caller, its argument and setter result, then
  saves and file-verifies the opener checkpoint with no known generator draw.
  Complete conversion, earlier near-buffer identity and other reseeding paths
  remain. FND-UI-040 subsequently verifies native restore of the opener
  checkpoint; the clock conversion and other reseeding obligations remain.
  Tried: FND-RNG-014 verifies the zero-output startup call's near-buffer
  identity, pushed offsets and unchanged data through conversion, whose
  actual return supplies the seed argument's low word. That startup identity
  obligation is answered; complete conversion, other input/path identities
  and reseeding coverage remain. Tried: FND-RNG-015 separates the wrapper's
  full-width return and optional-copy contract from conversion dependencies.
  Complete conversion and service/environment obligations move to Q-RNG-004;
  this item retains caller reachability, aliases and seed-write coverage.
  Tried: FND-RNG-021 reads the second root's unconditional reseeding before
  a count-controlled port-offset loop. FND-RNG-022 resolves request 27's
  wrapper and signed-byte argument narrowing, plus three direct caller
  argument windows. Those local role/width obligations are answered;
  shipped request instances, full upstream branch admission, DS/SS identity
  and the other seed-writer/alias census remain. Blocks: reproducible
  startup seeding and Q-PARTY-081's later recorded gameplay run.
- Q-RNG-004. RULE-RNG-001: Which date/time and environment inputs reach the
  reviewed clock conversion, and what input/service contract admits its reads?
  Settles it: population and bounds of the environment pointer array and
  strings, admission of indexed date/time values and near buffers on each
  caller path. Local service request preparation and sampling order are
  answered by FND-RNG-023; external service inputs/effects are separate in
  Q-RNG-006. Clock storage/writer identity is separate in Q-RNG-005.
  Tried: FND-RNG-009 identifies the conversion chain, FND-RNG-014 measures
  one startup input and return, and FND-RNG-015 records direct buffer access
  widths and the separate wrapper contract. These do not settle the
  environment branches, tables or full admitted domain. Split from
  Q-RNG-003 so conversion and reseeding reachability can be settled separately.
  Tried: FND-RNG-016 records the converter's local arithmetic widths,
  shipped calendar-table windows and daylight helper branches, with a
  conditional cross-check against the startup return. Those local-body
  obligations are answered; initializer semantics, global/table writers,
  continued identity and input admission remain. A conditional match does
  not identify the measured startup initializer path.
  Tried: FND-RNG-017 observes the actual zero-lookup default initializer
  path, offset/enable words and daylight return in the admitted startup.
  The measured default-path obligation is answered; parsed environment
  branches, service/input domain, state/table writers and other path
  identities remain. No complete conversion or writer census is claimed.
  Tried: FND-RNG-018 settles the local initializer's guard, default,
  integer-offset and first-label branches, including wrap and byte-classifier
  widths. Those local-body obligations are answered. Input/service admission
  remains here; global/table writers and storage identity split into Q-RNG-005.
  Tried: FND-RNG-023 settles local date-before-time sampling, AH-only request
  preparation, stored-result widths and absence of a local coherence/error
  retry. Those local service obligations are answered; external service
  behavior splits into Q-RNG-006. This item retains environment/string and
  indexed input/near-buffer admission.
  Blocks: complete reproducible seed-source semantics.
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
  installed INT 21 routing and guest-hook admission, common dispatch/return
  effects and the time handler's clock stall; preserve DS/SI destinations
  and read the admitted clock construction, offset writers and date domain.
  Local result fields and independent reads are described by
  SRC-RUSTDOS-172, rather than assumed generic DOS guarantees.
  Tried: FND-RNG-015 identifies returned word stores; FND-RNG-023 completely
  reads the game's request preparation/order without local validation.
  Split from Q-RNG-004 for external-service semantics. Tried:
  SRC-RUSTDOS-172 locates the exact pinned source tag and reads local
  handler assignments, separate CMOS samples and host-clock/offset access.
  The source acquisition and local handler-body obligations are answered;
  connected runtime admission remains. Moved from Source to Static now
  that the required source snapshot is available. Blocks: complete admitted
  seed-source input domain.

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

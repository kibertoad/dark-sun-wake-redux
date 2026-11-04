# ITEM

Next ID: Q-ITEM-014

## Static

- Q-ITEM-005. FMT-ITEM-002: What is the complete field contract of the shared
  fifteen-byte item-type records? Settles it: remaining semantic reader/writer
  coverage beyond the recorded attack, damage, removal, automatic equipment,
  completion, property-classification and conditional bitmap-cache routes,
  including independent reads/writes, arithmetic and other flag
  meanings and combinations. The local field purposes and equipment mask
  overlap are already recorded; they are not complete rules for all routes.
  The descriptor partition and 102-row structural coverage are settled by
  FND-ITEM-011; material indices and the guarded detail-renderer hand/heavy/
  armor-bonus bindings by FND-ITEM-012. Reuse FND-PARTY-248/252's field/index
  and classifier consumers, FND-RNG-043's byte-three removal threshold,
  FND-COMBAT-065/066/067/068 and FND-PARTY-100's combat consumers, and
  FND-EXE-616's equipment/class-mask consumer rather than repeating them.
  Higher material-index states remain outside the observed source domain.
  Archive/pointer lifetime and trigger getter admission remain Q-CONFIG-008
  and Q-SCRIPT-022. Blocks: slices 4-5.
  Tried: FND-ITEM-011 settles the complete installed generic descriptor,
  metadata and conditional width partition, with all 102 source rows covered.
  The remaining question is the semantic/native-consumer contract, not another
  descriptor census. Direct code may address interior bytes independently.
  Tried: FND-ITEM-012 removes the material-label and named display-binding
  questions from this item's remaining work. Full native-consumer and flag
  coverage stays open; source labels do not prove every live alias or effect.
  Tried: reconciliation of FND-COMBAT-065/067, FND-PARTY-100, FND-RNG-043,
  FND-EXE-616 and FND-PARTY-085 supplies the remaining unassembled local
  field purposes in FMT-ITEM-002. The superseded FND-PARTY-092 is not the
  current combat-field evidence. This removes another field-identification
  reading; independent consumers and unresolved flag meanings remain.
  Tried: FND-PARTY-248/252 already settle local byte-eleven masks 0x01,
  0x02 and 0x08 and byte-four mask 0x20, plus independent property uses of
  byte-zero mask 0x13 and byte-eleven mask 0x80. FMT-ITEM-002 now records
  those guarded completion/classifier/cache bindings and the result replacements.
  This removes those local flag-identification readings; universal labels,
  independent consumers and admitted-domain/lifetime coverage remain open.

- Q-ITEM-001. FMT-ITEM-001, RULE-ITEM-006: Does DSUN.EXE read ITEMS.BIN, and what does an
  `old_item` number: which records of a Dark Sun 1 saved game the transfer utility reads its item
  numbers from, and why the table's last pair is out of order? Settles it: the transfer utility's
  code that fills the records it translates, read with the Dark Sun 1 save format, and a search of
  DSUN.EXE for a file name built at run time. Tried: case-insensitive searches for the name in
  DSUN.EXE and SVIEW.EXE, which find none (FND-ITEM-004, FND-ITEM-007). Blocks: slice 5.
- Q-ITEM-003. RULE-ITEM-001, RULE-ITEM-003, RULE-ITEM-004, RULE-ITEM-005: Which slots each kind of
  item may go in, how weight and item count limit what a character carries, when an item's spell
  can be cast, what a store pays, which class limits apply to a character with several classes,
  and whether the game enforces the class weapon lists and by which item field? Settles it: the
  code of overlay 189 around the placement messages and of overlay 191 around the pick-up messages
  (FND-ITEM-009), read with the item records it indexes. Blocks: slice 5.
- Q-ITEM-004. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Do the item-test
  callers admit the storage and state required for complete recursive removal?
  Settles it: remaining caller/global preservation and storage contracts of
  `2D40:2452`, including pool chains/indices, graph/map identity, counts and
  parent indices, and setter copy spans disjoint from current-object,
  descriptor, graph, map and stack state. Reuse Q-PARTY-081 for its specific
  defeat-cleanup segment-identity dependency; do not infer later identity from
  startup equality alone. Blocks: slices 4-5.
  Tried: split the independent field, quantity, selector and result-consumer
  questions into Q-ITEM-006 through Q-ITEM-009. FND-ITEM-013/014 settle the
  release leaf, local recursive ordering and conditional reparenting;
  FND-ITEM-015 bounds pool writes and the setter's local callee set. Reuse
  FND-PARTY-214/218 for map and graph provenance and FND-CONFIG-140/137/182
  for resolver/copy children. FND-PARTY-120 alone is not complete reclamation;
  superseded FND-PARTY-172 is not current graph evidence. Remaining work is
  admission/preservation, not another reading of those settled local bodies.

- Q-ITEM-006. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What independent
  writers and admitted states give the item-test inputs their complete field
  contracts? Settles it: producer, lifetime and consumer coverage for attached
  record byte +0x12, signed position +0x13, signed bonus +0x16 and word +0x10,
  including the position values zero, six and nine used by the armour test.
  Keep the shared type record distinct from these attached fields.
  Blocks: slices 4-5.
  Tried: split from Q-ITEM-004. FND-RNG-035 reads the three tests;
  FND-PARTY-248 binds local signed bonus uses and FND-PARTY-217/221 bind
  signed inventory position. Their local purposes are already in the rules;
  neither those uses nor a field's name establish every writer/admitted state.

- Q-ITEM-007. RULE-ITEM-008: Which caller and record states admit the counted
  item helper's local quantity operations? Settles it: complete caller
  coverage and argument producers for overlay 173 `+2B88`, independent
  quantity-word writers/lifetime and table/index capacity, and consistency
  of its independently supplied graph, entry and attached-record arguments.
  Complete deletion effects remain Q-ITEM-004. Blocks: slices 4-5.
  Tried: split from Q-ITEM-004. FND-ITEM-016 settles local word arithmetic,
  initial-zero bypass and store/request/return order. FND-EXE-600 and
  FND-RNG-035 record both known direct callers requesting one. Neither admits
  every caller or quantity; do not repeat the settled arithmetic reading.

- Q-ITEM-008. RULE-ITEM-007: What lists and indices can reach the corroding
  hit's held-item selector? Settles it: callers and producer/lifetime coverage
  for the graph and current-index domain of overlay 173 `+2E1B`, including
  count, ancestor-chain and attached/shared-record bounds for its recorded
  -2/1 selection. Graph storage admission is shared with Q-ITEM-004.
  Blocks: slices 4-5.
  Tried: split from Q-ITEM-004. FND-COMBAT-065/FND-EXE-600 settle the local
  selector criteria: first qualifying kind one, kind-two ancestor field 0x11
  and shared type byte +6 equal to one. The remaining question is admitted
  input graphs/indices, not the same local predicate again.

- Q-ITEM-010. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What unresolved
  incoming mechanisms can reach the hit routine or its forwarding wrappers
  outside the qualified direct calls? Settles it: the unresolved/computed transfers, unmapped edges and
  undecoded paths retained by FND-ITEM-020, plus split or unrelocated target
  construction and runtime pointer copies/use. Qualify instruction ownership
  and native input domains rather than treating raw candidates as callers.
  Blocks: slices 4-5.
  Tried: split from Q-ITEM-009. FND-ITEM-019 exhausts the source-declared
  adjacent relocated-pair representation for this target; FND-ITEM-020
  qualifies the three direct matches with complete declared byte scanning.
  FND-ITEM-028 also retains the nonexhaustive resident kind-dispatch table
  assumption; its qualified local consumer does not close other incoming
  routes. Its gap lists remain open. Reuse the retained controls and reports
  instead of another direct-signature or adjacent-pair census.

- Q-ITEM-011. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: When does the
  effect-switch near flags pointer identify the hit wrapper's SS-local byte?
  Settles it: segment producers at the admitted hit entries and DS/SS
  preservation through the reached item-test children, interrupts and
  overlay transitions. Reuse startup FND-PARTY-215/219 and the qualified
  FND-RNG-053/061 evidence; startup equality alone is insufficient.
  Frame spans, byte writers and aliases are independently Q-ITEM-013.
  Blocks: slices 4-5.
  Tried: split from Q-ITEM-009. FND-ITEM-017 records the near offset producer
  and SS-local reload, and FND-ITEM-018 the forwarding wrapper save/reload.
  Neither establishes arbitrary DS/SS identity or whole-child preservation.
  Tried: FND-ITEM-029 refreshes the inventory-range segment output with
  known startup, temporary-SS and timer controls and retains its decoding,
  trampoline and child-effect exclusions. No segment assignment is reported
  in the three item root rows; do not repeat that local scan as a proof of
  the entry or child contract. Saved-frame admission is split to Q-ITEM-013.

- Q-ITEM-013. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: Which admitted
  item calls preserve the flags byte and saved result until their reloads?
  Settles it: valid nonwrapping frame and record spans, all direct or
  aliased writes to the flags and saved bytes, and child copy/output
  destinations and lifetimes that can overlap those bytes. Treat segment
  identity as Q-ITEM-011, not a substitute for disjoint storage. Blocks:
  slices 4-5.
  Tried: split from Q-ITEM-011. FND-ITEM-017/018 record the near producer,
  SS-local reads and save/reload continuations; FND-ITEM-029 separates the
  segment-register census from preservation of memory. Neither a saved
  byte nor a child returning proves that byte remained unchanged.
  Tried: FND-ITEM-030 classifies the six explicit far-block dereferences
  in the effect switch as reads and bounds their stable-counter read span
  away from the caller's flags byte. Those six uses are no longer unread
  output stores. Other direct/aliased writes, copied or computed pointers
  and child destinations/lifetimes remain; do not infer a whole read-only
  switch from the operand scan.
  Tried: FND-ITEM-031 settles the two near block destinations, the
  fifteen-byte clear and the builder's full byte-code write arithmetic.
  Remaining producer admission must establish row/index/owner lifetimes
  and codes bounded to the fifteen-byte span (at most 119), or account
  for the larger footprint; the second block can conditionally overlap
  the flags byte. No such native input or outcome has been observed.
  Tried: FND-ITEM-032 settles the preserved-argument connection from the
  general API's positive signed-code gate to the insertion code store and
  qualifies its one direct incoming call. That producer's conditional
  1..127 bound excludes codes 200..207, but does not prove the narrower
  bitmap bound. Remaining code admission is other writers, moved/restored
  rows and actual code sets, plus argument, row/index/count and alias
  preservation; do not repeat this gate/store path as producer completeness.

- Q-ITEM-012. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What admitted
  feedback state makes the hit requests return with their documented sound
  and visual effects? Settles it: visual number/table producers and lifetime,
  view/slot/resource/layout admission for overlay 206 +1073 and +1121, and
  their reached drawing/timer effects. Frame preservation is Q-ITEM-013
  and segment identity Q-ITEM-011;
  reuse Q-SOUND-002 for the sound library/driver dependency rather than
  duplicating its question. Blocks: slices 4-5.
  Tried: split from Q-ITEM-009. FND-MAGIC-024 binds DATA sound byte +0x16
  and visual word +0x14 or fallback to the hit continuation. FND-SOUND-007
  records sound gates and BVOC/loose-file selection; FND-EXE-602 records
  view/create gating for object feedback and the bitmap frame/save/wait/restore
  path, including its slot-four layout edge. The rules now identify these
  requests. Actual outcomes, deeper effects and admission remain.

## Emulated call

None.

## Agent run

None.

## Live session

- Q-ITEM-002. RULE-ITEM-001, SCR-UI-008, RULE-ITEM-002, RULE-ITEM-004, SCR-UI-019: Where does the inventory
  screen draw its item names, slots, data panel and money bar, what does clicking an item do, and
  how does a split halve an odd bundle? Settles it: the inventory-selection live session. Tried:
  the `TEXT/1000` lines of the two names an earlier capture shows, which bind no name to an item or
  slot (FND-ITEM-008). Blocks: slice 5.

## Source

None.

## Blocked

None.

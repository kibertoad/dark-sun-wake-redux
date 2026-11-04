# ITEM

Next ID: Q-ITEM-016

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
  FND-ITEM-036 settles the Look consumer's local nonzero/type-domain restriction;
  actual row-50 word-gate admission is Q-PARTY-115 and other property consumers Q-PARTY-108.
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
- Q-ITEM-003. RULE-ITEM-001, RULE-ITEM-005: Which caller and live-state
  contracts admit the recorded inventory placement and character-eligibility
  predicates? Settles it: incoming event/caller and logical-position producers
  for overlay 189 +2C6B/+2B64, item/type and current-character selection,
  DS/SS near-state identity, snapshot/removal child effects, and actual
  traversal/getter samples/termination with retained descriptors, widths,
  shared rows and disjoint graph/map/frame storage. Establish producer-to-use
  retention and the relation between named manual weapon/material limits
  and individual source rows; class combination is FND-ITEM-053. Local category/hand/container
  checks and capacity arithmetic are already FND-ITEM-048/049/050; do not
  repeat those readings. Spell use is Q-ITEM-014; stores are Q-ITEM-015.
  Blocks: slice 5.
  Tried: FND-ITEM-048 removes the local placement gate order, signed capacity/
  wrapped-count comparisons, hand thresholds and origin-category/class-overlap
  predicates. Remaining placement work is actual callers/positions, snapshot/
  removal and capacity children, graph/table/stack/current-character admission
  and preservation; spell and store questions remain independent obligations.
  Tried: FND-ITEM-049 removes the capacity result-selector, comparison-width
  and in-table Strength mapping obligations. The caller selects the minimum
  aggregate rather than its separate sum, then compares only its signed low
  word. Remaining capacity work is actual traversal/getter samples and
  termination, table/slot/record/copy/near-state admission and preservation.
  Tried: FND-ITEM-050 removes the canonical descriptor/byte-width/sample-domain
  alternative: only kind four supplies successful field-89 samples, all
  canonical bytes are 0..100, and this comparison cannot reject Strength
  3..25 under retained-source conditions. Actual sample selection, runtime
  mutations, width/table lifetime and traversal/storage admission remain.
  Tried: FND-ITEM-051 removes the local argument ambiguity for six encoded
  placement calls: five message-disabled queries and one enabled action
  prefix, with fixed loop ranges or identifier-minus-2BCC positions. The
  complete enumeration and immediate frame consumers are read; remaining
  work is actual dispatch/identifier and source-item admission, complete
  actions/transfers, snapshot/removal and shared state/storage preservation.
  Tried: FND-ITEM-052 removes the removal gate's local snapshot/target-item
  interpretation: seven initialized bits decide occupied positions 6/7/14,
  with effect 106 unconditional and six existing-item magnitude pairs.
  Remaining snapshot work is actual current-character/table/boundary and
  near-frame admission, map/graph/record lifetime and child preservation.
  Tried: FND-ITEM-053 removes the local class-combination and source flag/mask
  domains: the producer replaces the class word with admitted flags' OR;
  eligibility accepts any overlap. Canonical row 50 alone has mask 2000 hex
  outside producer union 1FFF. Producer-to-use retention, other writers,
  native record/table/near-state admission and named-row meanings remain.
  Tried: FND-ITEM-054 removes the transfer wrapper's L-minus-four position
  and target field/entry choices, plus its accepted empty-target entry path.
  Old proposed-item selection and the rebuilt first-kind-one source are not
  compared; actual identity/preservation, generic splice/store and post-call
  admission remain rather than another local field-selection reading.
- Q-ITEM-014. RULE-ITEM-003: Which original item-spell eligibility and
  activation paths implement the manual's item-use restrictions? Settles it:
  the item-summary action and underlying use/learning paths, their item/spell
  and character inputs, combat-turn restrictions, acceptance/rejection,
  charges or other persisted changes, and automatic readied-item effects,
  with actual dispatch/caller and state admission. The manual alone does
  not establish these branches. Split from Q-ITEM-003; the placement/class
  gate readings do not answer spell activation. Blocks: slice 5.

- Q-ITEM-015. RULE-ITEM-004: What transaction and money-state contract does
  the original store use? Settles it: buy/sell action dispatch and accepted
  item classes, price calculations and rejection conditions, party-money
  storage/width and debit/credit ordering, and item ownership transfer or
  failure effects, including the manual's party-wide balance. Split from
  Q-ITEM-003; inventory placement admission remains there and screen layout
  remains Q-ITEM-002. Blocks: slice 5.

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
  Tried: FND-COMBAT-165 removes reparenting, allocation and kind resurrection
  in the admitted root-zero finite-graph deletion class. Actual caller/storage
  admission, live-parent standalone deletion and other aliases remain open.
  Tried: FND-COMBAT-167 removes initialized pending bounds and positive-prefix
  earlier-parent ordering. Getter/storage admission, map values and zero-result
  retained root fields remain; zero count is not a no-operation guarantee.

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
  Tried: FND-ITEM-043 removes the canonical RDFF source-domain obligation:
  all 1931 item payloads have position -1 or zero, bonus zero through four,
  byte +12 in 0/3/4/5/6 and word +10 in zero through 327 (29 above 255).
  Position six/nine and negative bonus require another producer/input or
  changed copy. Initializer byte +12 equal to one already lies outside that
  source domain. Remaining work is independent writers, other inputs and
  admitted selection/lifetime, not another scan of these unchanged payloads.
  Tried: FND-ITEM-044 removes the placement helper's local range/collision/output
  uncertainty: a kind-two root without a same-position collision returns six
  or nine unchanged, and the recorded continuing field-ten request can write
  their byte under its binding/width conditions. Actual request/caller/graph,
  near DS/SS and setter admission remain. FND-ITEM-045 removes position-field
  ten and word-field eight selection from 253 unchanged top-level literal
  scalar-function calls; the dynamic GPL/27 request, nested calls, gap,
  other incoming routes, changed state and aliases remain.
  Tried: FND-ITEM-046 removes the dynamic helper's local loop and sixteen
  inspected literal caller function inputs: preserved setups select functions
  10/11/12/13 rather than 49, and a positive retained count is consumed.
  Other incoming entries and intervening global/function/dispatch effects
  remain; no unknown field-value trace is needed for those preserved routes.
  Tried: FND-ITEM-047 removes the printed nested-selector gap: all 374
  nested opcode-22 requests have literal functions; five select function 49,
  all field 25. Neither position field ten nor word field eight is selected.
  Remaining work is other entries, decoder gaps, changed streams/dispatch,
  independent stores, aliases and actual selection/storage/lifetime admission.
  Tried: FND-ITEM-054 removes the need for arbitrary request words in the
  position-six/nine route: UI identifiers 2BD6/2BD9 hex produce L=10/13,
  then P=6/9 and target field 16 in the reviewed wrapper. Actual event,
  selected source/target and graph/descriptor/width/store admission remain.

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
  Settles it: live nonwrapping frame spans, remaining direct or aliased
  writes to those bytes, and child output/copy destinations and lifetimes
  that can overlap the frame. Segment identity is Q-ITEM-011. Reuse the
  canonical table prerequisites: code admission Q-COMBAT-009, count bounds
  Q-COMBAT-017, restored rows/count Q-COMBAT-018, mutation caller admission
  Q-COMBAT-020 and retained rows across games Q-COMBAT-024. Do not repeat
  their table-domain investigation here. Blocks: slices 4-5.
  Tried: split from Q-ITEM-011. FND-ITEM-017/018/029 record the near
  producer, save/reload continuations and qualified segment output. None
  proves arbitrary frame preservation.
  Tried: FND-ITEM-030/031/034/035 settle the explicit block reads,
  builder write arithmetic and caller destinations, two disjoint word
  outputs, and handler-local bitmap ordering. Those footprints are read;
  remaining frame aliases and pointer/argument lifetimes stay here.
  Tried: separate previously combined table prerequisites into their
  already-authoritative COMBAT questions above. FND-ITEM-032/033 and
  FND-COMBAT-031/032/033/034/035/043/044/045/046/053/054 retain the local
  insertion/copy/restore, source-domain, backing and reinitialization
  evidence. The earlier item attempts removed local obligations; this
  reconciliation does not establish a global row invariant or close any
  prerequisite. Reuse those findings rather than repeat the same bodies.

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

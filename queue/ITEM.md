# ITEM

Next ID: Q-ITEM-010

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

- Q-ITEM-009. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What subsequent
  hit behavior consumes the item tests' result flag? Settles it: near-pointer
  identity and frame/record admission, reached child effects/preservation,
  direct entry-path coverage, split/computed/unrelocated incoming routes,
  runtime pointer copies/use and consumers beyond the forwarding wrapper
  or queue return, beyond the recorded presentation gates and three known
  direct hit-call continuations. Distinguish this local
  effect-test result from unrelated combat masks. Blocks: slices 4-5.
  Tried: split from Q-ITEM-004. FND-ITEM-010 records the effect-switch callers,
  conditional store of mask 0x02 and continuation entry, and FND-RNG-035
  records the test returns. The later consumers have not been read there.
  Tried: FND-ITEM-017 settles the pointer producer, numeric mask-02 versus
  mask-01 local gate, combatant-word fallback, mode-gated request order and
  AL return. Reuse FND-MAGIC-021 for the switch tail and FND-MAGIC-018 for
  the known caller's aggregate OR. Remaining work is admission, whole-child
  effects and later returned-byte consumers, not another local gate reading.
  Tried: FND-ITEM-018 settles the local result handling at all three direct
  hit-call sites recorded by FND-PARTY-096. The wrapper saves/forwards AL
  across queue processing; the queue continuation leaves AL untested;
  casting zero-extends it into an aggregate, tests mask 01 for a follow-up
  and returns separate bookkeeping. Remaining work excludes those local
  post-call readings, but still includes incoming coverage, admission,
  whole-child effects and consumers beyond the forwarding boundaries.
  Tried: FND-ITEM-019 enumerates the source-declared adjacent relocated
  offset/segment representation with independent MZ/FBOV controls and the
  known hit-call control. Its sole matching pair is the already-read casting
  call, with no alias or unresolved row and no truncation. Repeating that
  representation scan is unnecessary; the remaining incoming mechanisms
  above are outside it. This does not establish universal absence of callers.

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

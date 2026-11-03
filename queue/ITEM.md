# ITEM

Next ID: Q-ITEM-006

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
- Q-ITEM-004. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What remains of
  the item tests' removal, mutable-field and hit-result contract? Settles it:
  the remaining callee/global-preservation and admitted storage/caller
  contracts of `2D40:2452`, beyond its recorded local ordering, map builder,
  kind switch, reparenting and item-release leaf,
  independent writers and admission of attached-record byte +0x12, signed
  position +0x13, signed bonus +0x16 and word +0x10, the complete arithmetic
  and input domain of overlay 173 `+2B88`, admitted list/index domains for
  `+2E1B`, and the hit routine's consumers
  of result flag 0x02. Preserve the distinction between its local effect-test
  flag and other combat masks. Blocks: slices 4-5.
  Tried: FND-RNG-035 reads the three tests and FND-ITEM-010 their effect-switch
  callers and flag store. FND-PARTY-120 records part of the removal dispatch,
  not complete item-specific reclamation; do not treat the helper name as proof.
  Tried: FND-COMBAT-065/FND-EXE-600 settle the selector's local -2/1 criteria
  and the quantity/removal branch of `+2B88`; FND-PARTY-248 binds local signed
  bonus uses and FND-PARTY-217 binds signed inventory position +0x13.
  RULE-ITEM-007/008/009 now incorporate those facts. They remove those local
  identification readings, not the remaining writers, domains or result-flag
  consumer question. A complete reading is not established.
  Tried: FND-ITEM-013 reads the complete 61-byte release leaf and its adjacent
  52-byte allocator control. The leaf's free-list head/link and identifier
  stores are settled, under stated stability/non-aliasing conditions. Reuse
  FND-PARTY-214/218 for the map and kind switch; superseded FND-PARTY-172 is
  not current evidence. Remaining removal work is recursion/continuation and
  admitted storage/caller domains, not another local release-leaf reading.
  Tried: FND-ITEM-014 settles the recursive body's own release-before-child
  order, conditional reparenting and later child selection, and records its
  explicit non-returning failure branch without admitting shipped inputs.
  Remaining deletion work is reached callee effects, DS:60EB/descriptor
  preservation, graph/map identity and admitted counts/indices/parents,
  not another reading of this local continuation.

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

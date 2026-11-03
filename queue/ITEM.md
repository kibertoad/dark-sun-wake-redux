# ITEM

Next ID: Q-ITEM-006

## Static

- Q-ITEM-005. FMT-ITEM-002: What is the complete field contract of the shared
  fifteen-byte item-type records? Settles it: remaining semantic reader/writer
  coverage beyond the recorded attack, damage, removal and automatic equipment
  routes, including independent reads/writes, arithmetic and unexamined flag
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
- Q-ITEM-004. RULE-ITEM-007, RULE-ITEM-008, RULE-ITEM-009: What do overlay 191's item tests act
  on: which entry overlay 173 `+2E1B` picks with -2 and 1, what `2D40:2452` and overlay 173 `+2B88`
  do to the item, what bytes `+0x12`, `+0x13` and `+0x16` and word `+0x10` of an item record hold,
  and what the hit routine does with bit 0x02 of its flags byte? Settles it: readings of `+2E1B`,
  `+2B88` and `2D40:2452` from entry to return, of the writers of those item record fields, and of
  the hit routine's readers of the flags byte. Tried: FND-RNG-035 reads the three tests and
  FND-ITEM-010 their callers in the effect switch. Blocks: slices 4-5.

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

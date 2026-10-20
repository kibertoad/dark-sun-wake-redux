# ITEM

| Spec ID | Title | Spec status | Code | Tests | Deviations | Status | Notes |
|---|---|---|---|---|---|---|---|
| `FMT-ITEM-001` | Item translation pair in ITEMS.BIN | supported | missing | None | None | supported | The start-up extractor copies `ITEMS.BIN` into the asset pack as an opaque payload; nothing parses it. |
| `RULE-ITEM-001` | What a character's backpack and a pouch or chest can hold | sourced | missing | None | None | sourced | No code holds items or inventories. `ExplorationHotkeys` opens the inventory view on `I`, which draws the inventory background and no items. |
| `RULE-ITEM-002` | Splitting a bundle of grouped items | sourced | missing | None | None | sourced | No code splits items. |
| `RULE-ITEM-003` | Casting a spell from an item | sourced | missing | None | None | sourced | No code holds item spells. |
| `RULE-ITEM-004` | Buying from and selling to a store | sourced | missing | None | None | sourced | No code holds stores or the party's money. |
| `RULE-ITEM-005` | The armour, shields and weapons each class may use | sourced | missing | None | None | sourced | No code holds armour, shields or weapon limits. |
| `RULE-ITEM-006` | The transfer utility's translation of a Dark Sun 1 item | supported | missing | None | None | supported | The rebuild has no character transfer utility. |
| `RULE-ITEM-007` | A corroding hit destroys a picked held item when a d20 falls below 8 less the item's byte 0x16 | supported | missing | None | None | supported | No code applies `DATA` records or tests held items. |
| `RULE-ITEM-008` | DATA 235 hands the target's first held item of the lowest byte 0x12 below 6 to overlay 173 +2B88 when a d20 falls below 15 less the item's byte 0x16 | supported | missing | None | None | supported | No code applies `DATA` 235. |
| `RULE-ITEM-009` | A corroding hit destroys the first held item of type 0, 6 or 9 at once when its word 0x10 is 0, and otherwise on a d20 below 10 less that word | supported | missing | None | None | supported | No code applies `DATA` records or tests held items. |

---
id: RULE-ITEM-004
title: Buying from and selling to a store
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

A store shows its items for sale six at a time beside the inventory screen, with a MORE button
when there are more than six, and each item's price under its slot. An item the party can afford
has a flashing highlight when the pointer is on it, and one it cannot afford a solid highlight.
Clicking an item buys it: its price comes off the party's money and the item goes on the pointer.
SELL sells the item on the pointer to the shopkeeper.

## When it runs

`store_page_count` runs when a store opens; `can_afford` when the pointer moves onto an item for
sale; `buy_item` when the player clicks one (SRC-MANUAL-1994, pages 12 and 13).

## Parameters

`items_for_sale`, the number of items the store sells. `price`, the price of one item.

## Inputs

`party_money`, the money the whole party has.

## Procedure

```text
define store_page_count(items_for_sale):
    return (items_for_sale + 5) / 6

define can_afford(price):
    return price <= party_money

define buy_item(price):
    if not can_afford(price):
        return false
    party_money = party_money - price
    return true
```

## Outputs

`store_page_count` gives the number of pages of six items. `can_afford` gives true for a flashing
highlight and false for a solid one. `buy_item` gives true when the item was bought and is on the
pointer, and lowers `party_money` by the price.

## Edge cases

The manual does not say whether an item that costs exactly the party's money can be bought: the
procedure allows it. It does not say what happens on a click on an item the party cannot afford:
the procedure refuses it.

## What the sources say

SRC-MANUAL-1994, pages 12 and 13: when the party visits a store, the regular inventory screen is
shown alongside a store screen; stores have six item slots showing the items for sale, and a
MORE button shows the rest when there are more than six; prices appear below the slots; pointing
at an item shows a flashing highlight when the party can afford it and a solid highlight when it
does not have enough money; clicking an item buys it, the money is deducted and the cursor
becomes the item, to be placed in the inventory; SELL sells the selected item; the Return to Game
button leaves the store. Page 11 says the money shown along the bottom of the inventory screen is
the whole party's.

## Differences between builds

None known.

## Open questions

- What a shopkeeper pays for an item sold and whether a store takes every item (Q-ITEM-003).
- Where and as what type the game keeps `party_money` (Q-ITEM-003).
- The store screen has no screen entry: the two unexplained buttons of the inventory window may be
  SELL and MORE (SCR-UI-008, Q-ITEM-002).

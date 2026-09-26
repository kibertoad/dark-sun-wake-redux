---
id: RULE-ITEM-001
title: What a character's backpack and a pouch or chest can hold
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Each character carries items in fourteen body slots and a backpack of twelve slots. Pouches and
chests found in the game hold six items each and can go in the backpack, but they hold only
items: a pouch or chest cannot go inside another one. The player moves one item at a time on the
mouse pointer, and the inventory screen outlines in yellow each slot the item can go in.

## When it runs

`backpack_has_room` runs when the player places an item in a character's backpack, either on the
inventory screen (SCR-UI-008) or by clicking a character's icon with an item picked up in the
game; `container_accepts` runs when the player places an item in an open pouch or chest
(SRC-MANUAL-1994, pages 5 and 11).

## Parameters

`items_in_backpack`, the number of the character's twelve backpack slots that hold an item.
`item_is_container`, true when the item being placed is a pouch or a chest.
`items_in_container`, the number of items in the pouch or chest it is being placed in.

## Inputs

None beyond the parameters.

## Procedure

```text
define backpack_has_room(items_in_backpack):
    return items_in_backpack < 12

define container_accepts(item_is_container, items_in_container):
    if item_is_container:
        return false
    return items_in_container < 6
```

## Outputs

Whether the item can go there.

## Edge cases

What happens to an item clicked on a character's icon when that character's backpack is full is
not known.

## What the sources say

SRC-MANUAL-1994, page 5: clicking Pick Up turns the cursor into the object; clicking the object
on a character's icon puts it in that character's backpack, clicking an open area drops it on the
ground, and only one item can be on the pointer at a time. Page 11: fourteen slots round the
portrait stand for parts of the body; the player left-clicks an item to pick it up, a yellow
outline appears round every valid slot, flashes when the item is centred over one, and a second
left-click drops it there; weapons are readied by putting them in the hands, and missile weapons
and their ammunition have their own slots at the upper left; twelve backpack slots are at the
upper right; pouches and chests hold six items, can be put in the backpack, hold only items and
no other pouches or chests, open with a right-click and close with a left-click on the lid or
flap. Page 12: DROP drops the item on the pointer; clicking a character box with an item selected
opens that character's inventory to place it, and right-clicking the box gives the item without
leaving the current screen. Page 16: Strength sets how much a character can carry before being
slowed in combat.

## Differences between builds

None known.

## Open questions

- Which slots each kind of item may go in, what makes a slot valid, and how weight and item
  count limit what a character carries. `DSUN.EXE` has messages for each of these checks in
  overlay 189 and for items picked up in overlay 191 (FND-ITEM-009, Q-ITEM-003).
- What the screen shows while an item is placed, and the full list of the inventory screen's
  effects (Q-ITEM-002).

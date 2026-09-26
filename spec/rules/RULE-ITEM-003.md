---
id: RULE-ITEM-003
title: Casting a spell from an item
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Right-clicking an item in the inventory brings up its item summary. When the item is magical
and can be used, the summary shows a Cast Spell icon, which casts the item's spell. It cannot be
used in combat when it is not the holder's turn, or when the holder cannot cast that spell. A
spell scroll's summary lets a character learn its spell. Some items cast their spells by
themselves: a readied magical sword when it hits, and a worn item on its wearer as soon as it is
readied.

## When it runs

When the player clicks the Cast Spell icon in an item's summary (SRC-MANUAL-1994, page 12).

## Parameters

`in_combat`, true while a combat is under way. `holders_turn`, true when it is the turn of the
character holding the item. `holder_can_cast`, true when that character can cast the item's
spell.

## Inputs

None beyond the parameters.

## Procedure

```text
define can_cast_from_item(in_combat, holders_turn, holder_can_cast):
    if in_combat and not holders_turn:
        return false
    return holder_can_cast
```

## Outputs

Whether the spell is cast.

## Edge cases

None known.

## What the sources say

SRC-MANUAL-1994, page 12: right-clicking any item in the inventory brings up its item summary; if
the item is magical and can be used, a Cast Spell icon appears at the top right of the box, and
selecting it casts the spell; this cannot be done during combat if it is not the character's
turn, or if the spell cannot be cast by the character holding the item; if the player knows what
spell the item casts, its icon appears in the summary; right-clicking a spell scroll brings up a
window with a spell icon, which the player clicks to learn the spell. Some items are inherently
magical: a readied magical sword may cast spells at an opponent it hits, and other items cast
their effects on whoever wears them as soon as they are readied.

## Differences between builds

None known.

## Open questions

- What makes a character able to cast an item's spell, what learning from a scroll requires,
  and when a sword casts its spell (Q-ITEM-003).

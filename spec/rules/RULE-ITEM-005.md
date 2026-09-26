---
id: RULE-ITEM-005
title: The armour, shields and weapons each class may use
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994]
conflicting: []
split_with: []
related: []
---

## Summary

Druids and preservers may wear no armour and carry no shield. Psionicists and thieves may wear
leather armour and carry a small shield. Clerics, fighters, gladiators and rangers may use any
armour and any shield. Fighters, gladiators, rangers, druids and thieves may use any weapon.
Preservers, psionicists and clerics may use only the weapons the manual's table lists for them,
a cleric's by element (What the sources say).

## When it runs

When a character readies armour, a shield or a weapon (SRC-MANUAL-1994, page 77).

## Parameters

`character_class`, one of the character's classes, as a `character_class` code.

## Inputs

None beyond the parameters.

## Procedure

```text
# 0: none, 1: leather armour or a small shield only, 2: any
define armour_permitted(character_class):
    let armour: UINT8[8] = [2, 0, 2, 2, 0, 1, 2, 1]
    return armour[character_class]

define shield_permitted(character_class):
    let shield: UINT8[8] = [2, 0, 2, 2, 0, 1, 2, 1]
    return shield[character_class]
```

## Outputs

The widest kind of armour or shield the class allows: 0 for none, 1 for leather armour or a small
shield, 2 for any.

## Edge cases

A character with more than one class: the manual does not say which class's limit applies.

## What the sources say

SRC-MANUAL-1994, page 77, table "Armor and Weapons Permitted": clerics of each of the four
elements, any armour and any shield; druids, none and none; fighters, gladiators and rangers, any
and any; preservers, none and none; psionicists, leather and small; thieves, leather and small.
Its Weapon column gives, by class:

| Class | Weapons |
|---|---|
| Cleric of Air | bow, blowgun, sling, spear and throwing spear |
| Cleric of Earth | any weapon made of stone, metal or wood |
| Cleric of Fire | magically fired, obsidian or fire-heated weapons |
| Cleric of Water | bone or wood bows, club, maces, javelins, quarterstaff, spears and warhammers, which must be organic |
| Druid | any |
| Fighter | any |
| Gladiator | any |
| Ranger | any |
| Preserver | dagger, staff, darts, knife and sling |
| Psionicist | short bow, hand and light crossbow, dagger, dirk, knife, club, hand axe, throwing axe, horseman mace and pick, scimitar, spear, short sword and warhammer |
| Thief | any |

The procedure gives only the armour and shield limits, since the weapon lists name item kinds that
no identified item field records.

## Differences between builds

None known.

## Open questions

- Which limit applies to a character with several classes, what "leather" and "small" cover among
  the game's items, and whether the game enforces the weapon lists, and through which item field
  it tells a weapon's kind or material (Q-ITEM-003).

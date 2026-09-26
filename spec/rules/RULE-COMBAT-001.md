---
id: RULE-COMBAT-001
title: All four party members are drawn during combat, and only the leader again after it when the party is collapsed
status: sourced
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [SRC-MANUAL-1994, FND-COMBAT-021, FND-COMBAT-023]
conflicting: []
split_with: []
related: [RULE-PARTY-008]
---

## Summary

Outside combat the party can be shown as its leader alone. When combat begins the other three
members appear where they stand, and when it ends the party is drawn as the leader alone again.

## When it runs

`member_drawn` runs for each party slot whenever the map is drawn.

## Parameters

`slot`, the party slot, 0 to 3. `in_combat`, true while a combat is under way. `show_all`, true
when the player has chosen to see the whole party outside combat (Collapse Party off, or the key
`5`), false when the player has chosen the leader alone (the key `6`).

## Inputs

`leader`.

## Procedure

```text
define member_drawn(slot, in_combat, show_all):
    if in_combat or show_all:
        return true
    return slot == leader
```

## Outputs

True when the member in `slot` is drawn.

## Edge cases

An empty slot is drawn in no case. The rule says nothing about where the members who appear at
the start of combat stand.

## What the sources say

SRC-MANUAL-1994, page 4: only the leader appears on screen while exploring, and the other three
appear whenever combat is initiated; Collapse Party shows all four at all times. Page 77: `5`
shows all characters while moving and `6` the leader alone. The owner saw no transition screen at
either end of combat, and the party collapsed to the leader at once when combat ended
(FND-COMBAT-021).

## Differences between builds

None known.

## Open questions

- Where the game keeps the party display setting, where the members who appear are placed, and
  whether the owner's party was collapsed when combat began; the loader of FND-PARTY-013 sets a
  bit on every party slot but one, which may be how the others are hidden (Q-COMBAT-001).
- Whether the word the party loader compares each slot with is `leader` (Q-PARTY-009).

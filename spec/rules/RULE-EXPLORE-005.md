---
id: RULE-EXPLORE-005
title: How a character walks to the cell a left click with the Walk pointer chose
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: []
conflicting: []
split_with: []
related: [RULE-INPUT-001, RULE-EXPLORE-003, RULE-EXPLORE-004]
---

## Summary

A left click with the Walk pointer on the map makes the party walk to the place clicked
(RULE-INPUT-001). How the game picks the cells of the walk, how often a walking character steps
from one cell to the next, how close it must get to the clicked cell, and what it does when a
cell on its way becomes blocked are not known.

## When it runs

When the player left-clicks the map with the Walk pointer, outside combat and on a combatant's
turn in combat.

## Parameters

None known.

## Inputs

The clicked cell, the walking character's cell, and the cell bits that RULE-EXPLORE-003 sets.

## Procedure

Not known.

## Outputs

Not known.

## Edge cases

None known.

## What the sources say

SRC-MANUAL-1994, pages 2 and 4: in Walk mode a left click moves the party to the place clicked.
The manual does not describe the route.

## Differences between builds

None known.

## Open questions

- Which routine plans the walk, and whether it is the route search at the start of `2D40:10AE`
  that the keypad step calls (RULE-EXPLORE-004); how it orders cells, what it does when no route
  exists or a cell on the route becomes blocked, and how long each step takes (Q-EXPLORE-006).

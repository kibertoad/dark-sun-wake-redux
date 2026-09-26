---
id: RULE-EXPLORE-004
title: The keypad direction keys step the chosen character one cell, or in combat attack the object whose area holds the blocked cell
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-EXPLORE-001, FND-EXPLORE-002, FND-EXPLORE-003, FND-EXPLORE-004, FND-EXPLORE-005, FND-AI-004, FND-ACTOR-003, FND-ACTOR-005, FND-COMBAT-008, FND-COMBAT-013, FND-COMBAT-022, FND-COMBAT-023, FND-COMBAT-025, FND-PARTY-013, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-EXPLORE-003, RULE-COMBAT-006]
---

## Summary

The eight direction keys of the numeric keypad move the chosen character one cell up, down,
left, right or diagonally. Outside combat the party's own cells do not block the step. When the
cell is blocked, the character stays; in combat, if another object's area covers the cell, the
character acts on that object instead.

## When it runs

`direction_key` runs when the player presses Up, PgUp, Right, PgDn, Down, End, Left or Home of the
keypad, with `d` from 0 to 7 in that order, through the key routine of FND-AI-004.

## Parameters

`d`, a `UINT8` from 0 to 7: the direction. `c`, a `UINT16`: the character's slot. `tx` and `ty`:
the target cell.

## Inputs

`turn_combatant`, `combat_state`, `g_57E0_1440`, `object_slots`, `object_slot_kinds` and the
cells RULE-EXPLORE-003 reads.

## Procedure

```text
define try_step(c: UINT16, tx: INT16, ty: INT16) -> UINT8:
    let blocked: UINT8 = 0
    if c < 4:
        lift_party_cells()
        blocked = cell_blocked(tx, ty)
        restore_party_cells()
    else:
        blocked = cell_blocked(tx, ty)
    if blocked != 0:
        if combat_state == 0:
            return 0
        for s in 5..48:
            if object_slot_kinds[s] == 2:
                let byte = footprint_byte(s)
                let sx = INT16(object_slots[s].position_x >> 4)
                let sy = INT16(object_slots[s].position_y >> 4)
                let w = byte & 0xF
                let h = byte / 16
                if tx >= sx - w and tx <= sx + w and ty >= sy - h and ty <= sy + h:
                    fn_28C9_305F(c, s, 1)
                    return 1
        return 0
    walk_target_x[c] = tx
    walk_target_y[c] = ty
    g_4F49_08A7[c] = 15
    if combat_state == 0 and c < 4:
        lift_party_cells()
        fn_2D40_10AE(c, 1)
        restore_party_cells()
    else:
        fn_2D40_10AE(c, 1)
    g_4E71_0000 = 1
    return 1

define direction_key(d: UINT8):
    let step_x: INT8[8] = [0, 1, 1, 1, 0, -1, -1, -1]
    let step_y: INT8[8] = [-1, -1, 0, 1, 1, 1, 0, -1]
    if g_57E0_1440 != 1:
        fn_28C9_2535(1)
    let c = turn_combatant
    let tx = INT16(object_slots[c].position_x >> 4) + step_x[d]
    let ty = INT16(object_slots[c].position_y >> 4) + step_y[d]
    try_step(c, tx, ty)
    fn_2C5F_0182()
```

## Outputs

`try_step` returns 1 when the character moves or acts and 0 when it stays. On a move it sets the
character's `walk_target_x`, `walk_target_y` and `g_4F49_08A7` and sets `g_4E71_0000` to 1.
`direction_key` returns nothing.

## Edge cases

Slot 4 is never searched for an object to act on, and the search stops at the first object whose
area holds the cell. A character whose slot is 4 or more tests the cell with the party's cells in
place. A target cell outside the map is blocked (RULE-EXPLORE-003). The area searched in combat
uses the low four bits of the footprint byte for columns and the high four bits for rows, both as
distances from the object's cell, which differs from the footprint RULE-EXPLORE-003 occupies.

## What the sources say

SRC-MANUAL-1994, page 4: the characters can also be moved with the arrow keys of the numeric
keypad.

## Differences between builds

None known.

## Open questions

- What `fn_2D40_10AE` does with the walk target after lifting the character's footprint, and what
  `fn_28C9_305F`, `fn_28C9_2535` and `fn_2C5F_0182` do, read here as starting the move, the attack,
  the Walk pointer and a redraw (FND-EXPLORE-003, FND-EXPLORE-004, Q-EXPLORE-003).
- What `g_4F49_08A7`, `g_4E71_0000` and `g_57E0_1440` stand for (FND-EXPLORE-004, FND-COMBAT-013,
  Q-EXPLORE-003).
- Whether the grey arrow keys of an enhanced keyboard reach these handlers (FND-EXPLORE-004,
  Q-EXPLORE-003).

---
id: RULE-EXPLORE-003
title: Objects occupy the map cells of their footprint by setting the cells' blocked and occupied bits, which a cell test for movement reads
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-EXPLORE-001, FND-EXPLORE-002, FND-EXPLORE-003, FND-EXPLORE-005, FND-REGION-003, FND-REGION-005, FND-ACTOR-003, FND-ACTOR-005, FND-COMBAT-008, FND-COMBAT-022, FND-COMBAT-023, FND-PARTY-013]
conflicting: []
split_with: []
related: [RULE-EXPLORE-002, RULE-EXPLORE-004, FMT-REGION-003, FMT-REGION-004, FMT-ACTOR-004, FMT-COMBAT-001, FMT-COMBAT-002]
---

## Summary

A cell of the region's flag map blocks movement when its `blocked` bit is set. An object of the
kind that takes up room occupies a square of cells around its own cell, whose size and trimmed
corners come from its combatant details, by setting `blocked` and `occupied` on each free cell,
and frees them by clearing both where `occupied` is set, so a wall never becomes free. One object
number occupies a fixed 5 by 5 area without its corners instead. The game remembers each object's
first occupied cell and the four party members' cells, so that it can free the right cells and
take the party's own cells off the map while it tests a step.

## When it runs

`place_footprint` and `remove_footprint` run when an object is moved (`move_object`, FND-EXPLORE-003)
and when key 6 hides a member (RULE-EXPLORE-002). `lift_footprint` and `restore_footprint` run
around the movement routine's route search (FND-EXPLORE-003). `cell_blocked` runs for each cell a
step or a route tests (RULE-EXPLORE-004). `lift_party_cells` and `restore_party_cells` run around a
test or a move of a party member (RULE-EXPLORE-004).

## Parameters

`slot`, a `UINT16`: an object slot. `x` and `y`: a cell, column 0 to 127 and row 0 to 97; a value
outside those, taken as an unsigned word, is outside the map. `place`, true to occupy and false to
free. `first`, true for the first cell of a footprint. `cells_x` and `cells_y`, `INT16[]` lists the
caller keeps. `count`, the number of cells in them.

## Inputs

`cell_map`, `passability_slot`, `footprint_size`, `footprint_cut`, `first_cell_x`, `first_cell_y`,
`first_cell_numbers`, `area_x`, `area_y`, `saved_area_x`, `saved_area_y`, `party_cell_x`,
`party_cell_y`, `object_slots`, `object_slot_kinds`, `object_slot_combatants`, `combatants` and
`combatant_details`.

## Procedure

```text
define outside_map(x: UINT16, y: UINT16) -> UINT8:
    return cell_map == 0 or x >= 128 or y >= 98

define cell_blocked(x: UINT16, y: UINT16) -> UINT8:
    if outside_map(x, y):
        return 1
    let cell = cell_map.cells[y * 128 + x]
    if passability_slot != -1 and fn_25AF_02FA(passability_slot, cell.flags & 7) == 0:
        return 1
    return cell.flags & 0x40

define cell_occupied(x: UINT16, y: UINT16) -> UINT8:
    if outside_map(x, y):
        return 0
    return cell_map.cells[y * 128 + x].flags & 0x20

define place_cell(x: UINT16, y: UINT16):
    if outside_map(x, y):
        return
    let cell = cell_map.cells[y * 128 + x]
    if cell.blocked == 0:
        cell.occupied = 1
        cell.blocked = 1

define remove_cell(x: UINT16, y: UINT16):
    if outside_map(x, y):
        return
    let cell = cell_map.cells[y * 128 + x]
    if cell.occupied != 0:
        cell.occupied = 0
        cell.blocked = 0

define keep_first_cell(slot: UINT16, x: UINT16, y: UINT16, place: UINT8) -> UINT16:
    if slot >= 48:
        return 0
    let result: UINT16 = 1
    let old_x = first_cell_x[slot]
    let old_y = first_cell_y[slot]
    if place:
        if first_cell_numbers[slot] != 0 and cell_occupied(old_x, old_y) != 0:
            remove_cell(old_x, old_y)
        first_cell_x[slot] = x
        first_cell_y[slot] = y
    else:
        let same = old_x == x and old_y == y
        let off_map = x == 0x84 and y == 0x66
        if first_cell_numbers[slot] != 0 and not same and not off_map:
            result = 0
            if cell_occupied(old_x, old_y) != 0:
                remove_cell(old_x, old_y)
        first_cell_x[slot] = 0x84
        first_cell_y[slot] = 0x66
    first_cell_numbers[slot] = abs(INT16(object_slots[slot].object_number))
    return result

define occupancy_step(slot: UINT16, x: UINT16, y: UINT16, place: UINT8, first: UINT8) -> UINT16:
    if cell_map == 0:
        return 0
    let result: UINT16 = 1
    if first:
        result = keep_first_cell(slot, x, y, place)
    if result != 0:
        if place:
            place_cell(x, y)
        else:
            remove_cell(x, y)
    return result

define footprint_byte(slot: UINT16) -> UINT8:
    if slot >= 9999 or object_slot_kinds[slot] != 2:
        return 0
    let c = object_slot_combatants[slot]
    if c >= 42:
        return 0
    let d = combatants[c].details_index
    if d >= 17:
        return 0
    return combatant_details[d].footprint

define set_footprint(slot: UINT16):
    let byte = footprint_byte(slot)
    footprint_size = byte & 0xF
    footprint_cut = byte / 16

define footprint_x(index: INT16, x: INT16) -> INT16:
    let side = footprint_size + 1
    return x - side / 2 + index % side

define footprint_y(index: INT16, y: INT16) -> INT16:
    let side = footprint_size + 1
    return y - side / 2 + index / side

define next_footprint_index(prev: INT16, x: INT16, y: INT16) -> INT16:
    let side = footprint_size + 1
    let index = prev + 1
    while index < side * side:
        if side % 2 == 0:
            return index
        let dx = abs(footprint_x(index, x) - x)
        let dy = abs(footprint_y(index, y) - y)
        if dx + dy <= side - 1 - footprint_cut:
            return index
        index = index + 1
    return -1

define is_area_object(slot: UINT16) -> UINT8:
    return abs(INT16(object_slots[slot].object_number)) == 430

define occupy_area(x: INT16, y: INT16, place: UINT8):
    if place:
        for i in 0..5:
            for j in 0..5:
                remove_cell(area_x + i - 2, area_y + j - 2)
        area_x = x
        area_y = y
        for i in 0..5:
            for j in 0..5:
                let corner = (i == 0 or i == 4) and (j == 0 or j == 4)
                if not corner:
                    place_cell(area_x + i - 2, area_y + j - 2)
    else:
        let unset = area_x == 0 and area_y == 0
        let away = area_x == 0x84 and area_y == 0x66
        if not unset and not away:
            for i in 0..5:
                for j in 0..5:
                    remove_cell(area_x + i - 2, area_y + j - 2)
        for i in 0..5:
            for j in 0..5:
                remove_cell(x + i - 2, y + j - 2)
        area_x = 0x84
        area_y = 0x66

define place_footprint(slot: UINT16, x: INT16, y: INT16):
    if object_slot_kinds[slot] != 2 or object_slots[slot].unk_00 & 0x80 != 0:
        return
    if is_area_object(slot):
        occupy_area(x, y, 1)
        return
    set_footprint(slot)
    let first = 1
    let index = next_footprint_index(-1, x, y)
    while index != -1:
        occupancy_step(slot, footprint_x(index, x), footprint_y(index, y), 1, first)
        first = 0
        index = next_footprint_index(index, x, y)
    if slot < 4:
        party_cell_x[slot] = x
        party_cell_y[slot] = y

define remove_footprint(slot: UINT16, x: INT16, y: INT16):
    if object_slot_kinds[slot] != 2:
        return
    if is_area_object(slot):
        occupy_area(x, y, 0)
        return
    set_footprint(slot)
    let first = 1
    let index = next_footprint_index(-1, x, y)
    while index != -1:
        occupancy_step(slot, footprint_x(index, x), footprint_y(index, y), 0, first)
        first = 0
        index = next_footprint_index(index, x, y)

define lift_footprint(slot: UINT16, x: INT16, y: INT16, cells_x: INT16[], cells_y: INT16[]) -> INT16:
    if is_area_object(slot):
        saved_area_x = area_x
        saved_area_y = area_y
        occupy_area(x, y, 0)
        return 0
    let lifted = 0
    let index = next_footprint_index(-1, x, y)
    while index != -1:
        let cx = footprint_x(index, x)
        let cy = footprint_y(index, y)
        if cell_blocked(cx, cy) != 0:
            occupancy_step(slot, cx, cy, 0, lifted == 0)
            append(cells_x, cx)
            append(cells_y, cy)
            lifted = lifted + 1
        index = next_footprint_index(index, x, y)
    return lifted

define restore_footprint(slot: UINT16, count: INT16, cells_x: INT16[], cells_y: INT16[]):
    if is_area_object(slot):
        occupy_area(saved_area_x, saved_area_y, 1)
        return
    for i in 0..count:
        occupancy_step(slot, cells_x[i], cells_y[i], 1, i == 0)

define lift_party_cells():
    for s in 0..4:
        if object_slot_kinds[s] == 2 and object_slots[s].unk_00 & 0x80 == 0:
            remove_cell(party_cell_x[s], party_cell_y[s])

define restore_party_cells():
    for s in 0..4:
        if object_slot_kinds[s] == 2 and object_slots[s].unk_00 & 0x80 == 0:
            if combatants[object_slot_combatants[s]].hit_points > 0:
                place_cell(party_cell_x[s], party_cell_y[s])
```

## Outputs

`cell_blocked` returns 0 for an open cell and a value other than 0 for a blocked one, and
`cell_occupied` 0 or `0x20`. `keep_first_cell` and `occupancy_step` return 0 when they refused to
free the given cell, and `lift_footprint` returns the number of cells it freed and appends them to
the lists. The others return nothing. They change the `occupied` and `blocked` bits of cells of
`cell_map`, and `first_cell_x`, `first_cell_y`, `first_cell_numbers`, `area_x`, `area_y`,
`saved_area_x`, `saved_area_y`, `party_cell_x`, `party_cell_y`, `footprint_size` and
`footprint_cut`.

## Edge cases

The original's `place_cell` and `remove_cell` also skip the cell `(0x84, 0x66)`, which the bounds
already exclude. A cell outside the map is blocked, never occupied, and never changed; a
coordinate below 0 is outside the map because it is taken as an unsigned word. A footprint around
a cell near the map's edge occupies only its cells inside the map.

A footprint byte of 0, as for a slot whose kind is not 2 or whose indexes are out of range, gives
the object's cell alone. With an even side the whole square is occupied, from `side / 2` columns
and rows before the cell to `side / 2 - 1` after; with an odd side the square is centred and
`footprint_cut` trims it, 0 keeping it whole and 1 dropping the four corners.

`lift_footprint` uses the current `footprint_size` and `footprint_cut`, which its caller sets, and
frees every blocked cell of the footprint, a wall included, as far as `occupied` allows; it
records the blocked cells whether or not they were freed. `remove_footprint` frees the kept first
cell instead of the given one when they differ (`keep_first_cell`). `occupy_area` with `place` 0
does not free the area around `(0, 0)`, the kept cell's value in the file, and with `place` 1 it
does, which frees only occupied cells there.

`lift_party_cells` and `restore_party_cells` use the party cells as they were last placed; each
starts at `(0x81, 0x63)`, outside the map. A member with no hit
points left is not put back.

## What the sources say

None known.

## Differences between builds

None known.

## Open questions

- What `fn_25AF_02FA` decides from a cell's bits 0 to 2 for the slot in `passability_slot`, and
  which slots the movement routine puts there (FND-EXPLORE-001, FND-EXPLORE-003, Q-EXPLORE-002).
- Which objects are of kind 2 in `object_slot_kinds`, what values `footprint` takes, and what the
  object numbered 430 is (FND-EXPLORE-002, Q-EXPLORE-002).
- Whether the opening leader's cell is `(74,93)`, from its position less the definition's offsets,
  or `(74,91)`, from its figure's top-left (FND-EXPLORE-003, Q-EXPLORE-005).

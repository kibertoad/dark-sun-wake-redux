---
id: RULE-EXPLORE-005
title: How a character walks to the cell a left click with the Walk pointer chose
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-RNG-068, FND-RNG-072, FND-EXPLORE-001, FND-EXPLORE-002, FND-EXPLORE-003, FND-EXPLORE-004, FND-EXPLORE-014, FND-EXPLORE-015, FND-EXPLORE-016, FND-EXPLORE-019, FND-EXPLORE-020, FND-EXPLORE-021, FND-EXPLORE-022, FND-SCRIPT-028, FND-TIME-008, FND-TIME-010, FND-ACTOR-003, FND-ACTOR-005, FND-PARTY-013, FND-PARTY-098, FND-PARTY-199, FND-RNG-041, FND-COMBAT-022, FND-COMBAT-052, FND-EXE-602]
conflicting: []
split_with: []
related: [RULE-INPUT-001, RULE-EXPLORE-003, RULE-EXPLORE-004, RULE-SCRIPT-013]
---

## Summary

A left click with the Walk pointer on the map makes the party walk to the place clicked
(RULE-INPUT-001). The game plans a route as a straight 8-way line toward the clicked cell, cut
back to the last cell the walker fits, with the walls of each obstacle on the line followed both
ways round, and the walker steps one cell at a time along it. How long a step takes is not
known.

## When it runs

When the player left-clicks the map with the Walk pointer, outside combat and on a combatant's
turn in combat.

## Parameters

`slot`, an `INT16`: the walker's slot. `replan`, a `UINT8`: not 0 when the caller asks for a new
plan, as the click (FND-RNG-068) and a blocked step do.

## Inputs

`object_slots`, `object_slot_kinds`, `object_slot_combatants`, `combatants`, `leader`,
`combat_state`, `walk_mode`, `walk_target_x`, `walk_target_y`, the step records (`step_queue`,
`step_index`, `next_step`, `plan_target_x`, `plan_target_y`, `plan_start_x`, `plan_start_y`,
`plan_skip`), `game_tick`, `g_57E0_19AE`, and the cells RULE-EXPLORE-003 reads.

## Procedure

```text
define direction_toward(x1: INT16, y1: INT16, x2: INT16, y2: INT16) -> INT16:
    let table: INT8[16] = [0, 0, 0, 0, 0, 0, 6, 2, 0, 0, 0, 4, 7, 1, 5, 3]
    let dx = x2 - x1
    let dy = y2 - y1
    let index = 0
    if dx >= 0:
        index = index + 1
    if dy >= 0:
        index = index + 2
    if dx != 0:
        index = index + 4
    if dy != 0:
        index = index + 8
    return table[index]

define footprint_blocked(x: INT16, y: INT16) -> INT16:
    let index = next_footprint_index(-1, x, y)
    while index != -1:
        if cell_blocked(footprint_x(index, x), footprint_y(index, y)) != 0:
            return -1
        index = next_footprint_index(index, x, y)
    return 0

define straight_path(x1: INT16, y1: INT16, x2: INT16, y2: INT16, path_x: INT16[], path_y: INT16[]) -> UINT8:
    let step_x: INT8[8] = [0, 1, 1, 1, 0, -1, -1, -1]
    let step_y: INT8[8] = [-1, -1, 0, 1, 1, 1, 0, -1]
    let x = x1
    let y = y1
    let steps = 0
    while true:
        let d = direction_toward(x, y, x2, y2)
        x = x + step_x[d]
        y = y + step_y[d]
        if steps >= 78 or footprint_blocked(x, y) != 0:
            return 0
        append(path_x, x)
        append(path_y, y)
        steps = steps + 1
        if x == x2 and y == y2:
            return 1

define next_cell_of(size: INT16, cut: INT16, prev: INT16) -> INT16:
    let side = size + 1
    let index = prev + 1
    while index < side * side:
        if side % 2 == 0:
            return index
        let dx = abs(index % side - side / 2)
        let dy = abs(index / side - side / 2)
        if dx + dy <= side - 1 - cut:
            return index
        index = index + 1
    return -1

define cell_beside(object: INT16, walker: INT16, bx: INT16[], by: INT16[]) -> UINT8:
    set_footprint(walker)
    let byte = footprint_byte(object)
    let size = byte & 0xF
    let cut = byte / 16
    let side = size + 1
    let ox = INT16(object_slots[object].position_x >> 4)
    let oy = INT16(object_slots[object].position_y >> 4)
    let wx = INT16(object_slots[walker].position_x >> 4)
    let wy = INT16(object_slots[walker].position_y >> 4)
    let best = 999
    let found: UINT8 = 0
    let index = next_cell_of(size, cut, -1)
    while index != -1:
        let fx = ox - side / 2 + index % side
        let fy = oy - side / 2 + index / side
        let near_x: INT16[4] = [fx - 1, fx + 1, fx, fx]
        let near_y: INT16[4] = [fy, fy, fy - 1, fy + 1]
        for k in 0..4:
            if cell_blocked(near_x[k], near_y[k]) == 0:
                let d = fn_1B0B_000C(near_x[k], near_y[k], wx, wy)
                if d < best:
                    bx[0] = near_x[k]
                    by[0] = near_y[k]
                    best = d
                    found = 1
        index = next_cell_of(size, cut, index)
    return found

define cell_away(cx: INT16, cy: INT16, tx: INT16[], ty: INT16[]):
    let ox = tx[0]
    let oy = ty[0]
    tx[0] = cx
    ty[0] = cy
    let low = (footprint_size + 1) / 2
    let high = footprint_size / 2
    let best = fn_1B0B_000C(cx, cy, ox, oy)
    let x = cx - 4
    while x <= cx + 4:
        let y = cy + 4
        while y >= cy - 6:
            let inside = x >= cx - low and x <= cx + high and y >= cy - low and y <= cy + high
            if not inside:
                let d = fn_1B0B_000C(ox, oy, x, y)
                if d > best and footprint_blocked(x, y) == 0:
                    tx[0] = x
                    ty[0] = y
                    best = d
            y = y - 1
        x = x + 1

define end_plan(s: INT16, lifted: INT16, cells_x: INT16[], cells_y: INT16[]) -> INT16:
    if object_slot_kinds[s] == 2 and combatants[object_slot_combatants[s]].hit_points > 0:
        restore_footprint(s, lifted, cells_x, cells_y)
    if leader == s and next_step[s] != 0xFF:
        g_57E0_19BF = INT8(next_step[s])
    return s

define abandon_walk(s: INT16):
    next_step[s] = 0xFF
    if walk_mode[s] == 15:
        walk_mode[s] = 0

define plan_walk(slot: INT16, replan: UINT8) -> INT16:
    let cells_x: INT16[] = []
    let cells_y: INT16[] = []
    let lifted = 0
    let s = fn_2D40_2F34(slot)
    if s >= 48 or object_slots[s].unk_00 & 0x20 != 0:
        return end_plan(s, lifted, cells_x, cells_y)
    let cx = INT16(object_slots[s].position_x >> 4)
    let cy = INT16(object_slots[s].position_y >> 4)
    set_footprint(s)
    if replan != 0:
        next_step[s] = 0xFF
        lifted = lift_footprint(s, cx, cy, cells_x, cells_y)
    else:
        let kind = 2
        if s < 4:
            kind = 1
            if leader == s:
                kind = 0
        trigger_actor = s
        run_tile_triggers(s, cx, cy, kind)
        lifted = lift_footprint(s, cx, cy, cells_x, cells_y)
        if combat_state != 0:
            let fight = fn_2D40_1197(s)
            if fight != 0:
                if fight == 2:
                    s = 9999
                next_step[s] = 0xFF
                return end_plan(s, lifted, cells_x, cells_y)
    # The next queued step, when there is one
    if next_step[s] != 0xFF and step_index[s] < 16:
        let queued = INT8(step_queue[s][step_index[s]])
        if queued >= 0 and queued < 8:
            next_step[s] = queued
            step_index[s] = step_index[s] + 1
            return end_plan(s, lifted, cells_x, cells_y)
    # The target
    let mode = walk_mode[s]
    let tx = cx
    let ty = cy
    if mode == 15:
        tx = walk_target_x[s]
        ty = walk_target_y[s]
    else if mode == 1 or mode == 10 or mode == 11 or mode == 13 or mode == 18 or mode == 19:
        let object = walk_target_x[s]
        let found: UINT8 = 0
        let bx: INT16[1] = [0]
        let by: INT16[1] = [0]
        if mode != 1:
            found = cell_beside(object, s, bx, by)
        if found != 0:
            tx = bx[0]
            ty = by[0]
        else:
            tx = INT16(object_slots[object].position_x >> 4)
            ty = INT16(object_slots[object].position_y >> 4)
        if mode == 1:
            let ax: INT16[1] = [tx]
            let ay: INT16[1] = [ty]
            cell_away(cx, cy, ax, ay)
            tx = ax[0]
            ty = ay[0]
            plan_target_x[s] = -1
    if tx == cx and ty == cy:
        next_step[s] = 0xFF
        return end_plan(s, lifted, cells_x, cells_y)
    let search_failed = 0
    let unreachable = 0
    let arrived = 0
    # The original leaves this unset unless the wait test below runs; see Open questions
    let distance = 0
    while true:
        let again = 0
        if plan_target_x[s] != tx or plan_target_y[s] != ty:
            if search_failed != 0 and plan_start_x[s] == cx and plan_start_y[s] == cy:
                abandon_walk(s)
                return end_plan(s, lifted, cells_x, cells_y)
            plan_target_x[s] = tx
            plan_target_y[s] = ty
            plan_start_x[s] = cx
            plan_start_y[s] = cy
            plan_skip[s] = 0
        if combat_state == 0 and s >= 4 and UINT32(game_tick - g_57E0_19AE) < 32:
            distance = fn_1B0B_000C(cx, cy, tx, ty)
            if distance > 24:
                next_step[s] = 0xFF
                walk_mode[s] = -walk_mode[s]
                return end_plan(s, lifted, cells_x, cells_y)
        wall_follow_diagonals = 1
        passability_slot = s
        let route_x: INT16[] = []
        let route_y: INT16[] = []
        let result = fn_1B2E_07F3(plan_start_x[s], plan_start_y[s], tx, ty, plan_skip[s], route_x, route_y)
        passability_slot = -1
        if result != 0:
            passability_slot = s
            wall_follow_diagonals = 0
            result = fn_1B2E_07F3(plan_start_x[s], plan_start_y[s], tx, ty, plan_skip[s], route_x, route_y)
            passability_slot = -1
        if result != 0:
            plan_target_x[s] = -1
            again = 1
            search_failed = 1
        else:
            # The furthest cell of the route a straight line from the walker reaches
            let reach = -1
            let path_x: INT16[] = [cx]
            let path_y: INT16[] = [cy]
            let i = count(route_x) - 1
            while i >= 0 and reach == -1:
                if route_x[i] == cx and route_y[i] == cy:
                    reach = i
                else:
                    let line_x: INT16[] = [cx]
                    let line_y: INT16[] = [cy]
                    if straight_path(cx, cy, route_x[i], route_y[i], line_x, line_y) != 0:
                        reach = i
                        path_x = line_x
                        path_y = line_y
                i = i - 1
            if reach <= 0:
                if unreachable != 0:
                    abandon_walk(s)
                    return end_plan(s, lifted, cells_x, cells_y)
                unreachable = unreachable + 1
                again = 1
                plan_target_x[s] = -1
            else if route_x[reach] == cx and route_y[reach] == cy:
                plan_skip[s] = plan_skip[s] + reach
                if walk_target_x[s] == cx and walk_target_y[s] == cy:
                    arrived = 1
                    next_step[s] = 0xFF
                else:
                    again = 1
            else:
                let n = count(path_x) - 1
                let directions: INT16[] = []
                for k in 0..n:
                    append(directions, direction_toward(path_x[k], path_y[k], path_x[k + 1], path_y[k + 1]))
                append(directions, 8)
                step_index[s] = 0
                next_step[s] = directions[0]
                for j in 0..15:
                    if j + 1 < count(directions):
                        step_queue[s][j] = directions[j + 1]
                    else:
                        # The original keeps leftover bytes after the first of these; see Open questions
                        step_queue[s][j] = 0xFF
                step_queue[s][15] = 0xFF
                if n >= 15 and path_x[15] == tx and path_y[15] == ty:
                    step_queue[s][14] = 0xFF
        if again == 0 or arrived != 0:
            break
    if next_step[s] >= 8 and next_step[s] < 0x80:
        next_step[s] = 0xFF
    if next_step[s] < 0x80 and s >= 4 and distance > 24:
        g_57E0_19AE = game_tick
    return end_plan(s, lifted, cells_x, cells_y)
```

A Walk click sets `walk_target_x`, `walk_target_y` and `walk_mode` 15 for the slot and calls
`plan_walk(slot, 1)` (FND-RNG-068); when `next_step` is then `0xFF` it shows the no-path message
(FND-EXPLORE-014, FND-EXPLORE-015). Each pass `fn_28C9_19A1` steps the walker one cell in
`next_step`: it tests the cell, calls `plan_walk(slot, 1)` when the cell is blocked, at most twice,
and otherwise moves the footprint there and starts the move. At each cell `plan_walk(slot, 0)`
takes the next queued step (FND-RNG-072, FND-EXPLORE-014).

The move between cells is the walker's walk animation: each frame adds its pixel steps to the
position, one frame per map pass and no more than one per tick of `game_tick`, and the frame that
reaches a cell centre ends the step, so `plan_walk(slot, 0)` runs in the same pass and the next
step starts in the next (FND-EXPLORE-019). The most common walk animation in the shipped data
takes 4 frames per cell north or south and 6 in the other directions (FND-EXPLORE-020). With
animations turned off a walker moves a whole cell per pass. `fn_1B2E_07F3` follows each obstacle
round both ways and joins the route where a follower first comes out on a later point of the line
(FND-EXPLORE-021).

## Outputs

`plan_walk` returns the slot, or 9999 after a fight check that set the fight state to 4. It sets
`next_step`, and may change `step_queue`, `step_index`, `plan_target_x`, `plan_target_y`,
`plan_start_x`, `plan_start_y`, `plan_skip`, `walk_mode`, `trigger_actor`, `g_57E0_19AE`,
`g_57E0_19BF`, `passability_slot` and `wall_follow_diagonals`. The footprint it lifted is back in
place when it returns, unless the slot's combatant has no hit points left.

## Edge cases

- A blocked clicked cell: the route search plans toward the last open cell of the line, and the
  walk ends once no route starts from the walker's cell.
- A walker between cells (bit 5 of `object_slots[s].unk_00`) is not planned for until it reaches
  its next cell.
- A walker with no place on the map has position words `0xFFFF`, so its cell is (4095, 4095),
  where both searches fail (FND-EXPLORE-015).
- `direction_toward` gives 0 when both cells are the same. `fn_1B2E_07F3` sets the direction mode
  it uses to the 8-way one before any call here.
- `step_index` only grows while it is below 16, so `step_queue[s][step_index[s]]` stays in the
  record.
- A slot of 4 or more outside a fight that planned less than 32 ticks of `game_tick` after
  another such plan, for a target more than 24 away, gets no plan and has its `walk_mode` negated.

## What the sources say

SRC-MANUAL-1994, pages 2 and 4: in Walk mode a left click moves the party to the place clicked.
The manual does not describe the route.

## Differences between builds

None known.

## Open questions

- How long a step takes outside a fight and for walkers with other walk sets: in the first fight
  Thy'rokh's two-frame cells took one map pass per frame, 84 to 154 ms at 15,000 cycles per ms,
  bound by the pass and not the 96 Hz tick (FND-EXPLORE-019, FND-EXPLORE-020, FND-EXPLORE-022;
  Q-EXPLORE-006).
- What `fn_1B2E_07F3` does when a wall follower is boxed in on all four turns, where it reads the
  link of a node before its array (FND-EXPLORE-021; Q-EXPLORE-013).
- What `fn_1B0B_000C`'s distance is measured in (FND-PARTY-098; Q-EXPLORE-010).
- What the calls that end `fn_2D40_2F34` do for slots of 48 and up, what the fight check
  `fn_2D40_1197` does, and what
  `g_57E0_19AE` and `g_57E0_19BF` are for (FND-EXPLORE-014, FND-PARTY-199; Q-EXPLORE-010).
- What the original reads where the procedure has none: `distance` when the wait test did not
  run, the step-record bytes after a queue's end marker, which it fills from leftover stack
  memory, the step record of the leader on the exits before it takes its cell, and the records
  of slot 9999 after a fight check (FND-EXPLORE-014; Q-EXPLORE-010).

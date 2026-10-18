---
id: RULE-EXPLORE-001
title: Holding the pointer at an edge of the map view scrolls the view toward that edge until the pointer leaves it or the map ends
status: supported
builds: [BLD-GOG-EN-1.1]
superseded_by: []
evidence: [FND-EXPLORE-006, FND-COMBAT-049, FND-COMBAT-050, FND-PARTY-031, FND-PARTY-046, FND-CONFIG-158, FND-COMBAT-013, FND-COMBAT-025, FND-COMBAT-026, FND-CONFIG-171, FND-CONFIG-175, FND-EXPLORE-004, FND-EXPLORE-005, FND-INPUT-013, SRC-MANUAL-1994]
conflicting: []
split_with: []
related: [RULE-REGION-001]
---

## Summary

While the pointer is on the outermost columns or rows of the screen, each pass of the game's main
loop moves the view one step of `scroll_step_x` pixels across and `scroll_step_y` down, 16 and 16 in
play, in the one of eight directions that points from the screen centre to the pointer. A step
that would take the view past a bound on one axis keeps only its other axis; a straight step into
a bound does nothing. Center on Leader puts `leader`'s slot, and the key `H` the slot of
`turn_combatant`, at the centre of the view, as far as the bounds allow (FND-EXPLORE-006).

## When it runs

`edge_scroll` runs once in each pass of the main loop, while `g_57E0_0D9C` is not 0
(FND-PARTY-031, FND-EXPLORE-006).

## Parameters

`px` and `py`, the pointer's column and row as the mouse driver gives them, 0 to 319 and 0 to 199
in the game's video mode (FND-EXPLORE-006, FND-CONFIG-158).

## Inputs

`view_left`, `view_top`, `combat_state`, `turn_combatant`, `g_57E0_0DA8`, `g_57E0_0DAB`,
`pointer_mode`, `scroll_step_x`, `scroll_step_y`, `view_left_min`, `view_top_min`, `view_right_limit` and
`view_bottom_limit`.

## Procedure

```text
# The column and row change of directions 0 to 8; 8 is no direction.
let dir_dx: INT8[9] = [0, 1, 1, 1, 0, -1, -1, -1, 0]
let dir_dy: INT8[9] = [-1, -1, 0, 1, 1, 1, 0, -1, 0]

define pointer_direction(px: INT16, py: INT16) -> UINT16:
    let sx: INT16 = px - 160
    let sy: INT16 = py - 100
    if sx == 0:
        if sy > 0: return 4
        return 0
    if sy == 0:
        if sx > 0: return 2
        return 6
    let ax: INT16 = abs(sx)
    let ay: INT16 = abs(sy)
    # Divisions truncate toward zero; the shifts are arithmetic.
    let h: INT16 = ((ax * 128) / 160 * 50) >> 7
    let v: INT16 = ((ay * 128) / 100 * 80) >> 7
    if ay <= h:
        if sx > 0: return 2
        return 6
    if ax <= v:
        if sy > 0: return 4
        return 0
    if sx < 0 and sy < 0: return 7
    if sx < 0: return 5
    if sy > 0: return 3
    return 1

define scroll_allowed(d: UINT16, mx: INT16, my: INT16) -> UINT8:
    if mx < view_left or my < view_top: return 1
    if view_left + 320 < mx or view_top + 200 < my: return 1
    let rx: INT16 = mx - view_left + dir_dx[d] * scroll_step_x
    let ry: INT16 = my - view_top + dir_dy[d] * scroll_step_y
    if rx < 140 and dir_dx[d] < 1: return 1
    if ry < 70 and dir_dy[d] < 1: return 1
    if rx > 190 and dir_dx[d] > -1: return 1
    if ry > 130 and dir_dy[d] > -1: return 1
    if rx < 0 or rx > 320 or ry < 0 or ry > 200: return 1
    return 0

define scroll_view(d: UINT16, mx: INT16, my: INT16) -> UINT8:
    if scroll_allowed(d, mx, my) == 0: return 0
    let nl: INT16 = view_left + dir_dx[d] * scroll_step_x
    let nt: INT16 = view_top + dir_dy[d] * scroll_step_y
    let cx: INT16 = dir_dx[d]
    let cy: INT16 = dir_dy[d]
    if cx < 0 and nl < view_left_min: cx = 0
    if cx > 0 and nl + 319 >= view_right_limit: cx = 0
    if cy < 0 and nt < view_top_min: cy = 0
    if cy > 0 and nt + 199 >= view_bottom_limit: cy = 0
    let e: UINT16 = 8
    let k: UINT16 = 0
    while k < 8 and e == 8:
        if dir_dx[k] == cx and dir_dy[k] == cy: e = k
        k = k + 1
    if e != d:
        d = e
        nl = view_left + dir_dx[d] * scroll_step_x
        nt = view_top + dir_dy[d] * scroll_step_y
    if d == 8: return 0
    view_left = nl
    view_top = nt
    # visible: the view is drawn again from the new view words.
    emit ViewScrolled(d)
    return 1

define edge_scroll(px: INT16, py: INT16):
    if combat_state != 0 and turn_combatant >= 4: return
    if px != 0 and py != 0 and px < 318 and py < 199:
        g_57E0_0DA8 = 0
        return
    if g_57E0_0DA8 != 0: return
    if g_57E0_0DAB != 1: return
    if pointer_mode == 5: fn_3D72_0D83()
    let d: UINT16 = pointer_direction(px, py)
    scroll_view(d, px + view_left, py + view_top)
```

## Outputs

`view_left` and `view_top`, and `ViewScrolled` with the direction taken, 0 to 7, after each step.

## Edge cases

- At a corner of the bounds a diagonal step keeps its free axis; a straight step into a bound
  changes nothing.
- The pointer is not read while `combat_state` is not 0 and `turn_combatant` is 4 or more.
- `scroll_step_x` and `scroll_step_y` are 16 after every call of their setter but one, which uses 32
  while it pans the view to an object and sets 16 again before it returns.
- Center on Leader and `H` centre the view on a slot's `position_x` and `position_y`: the view's
  left edge becomes `(position_x - 160) & ~7` and its top `(position_y - 100) & ~7`, then each is
  moved inside the four bound words (FND-EXPLORE-006, FND-COMBAT-049). `H` does it only while
  `turn_combatant` is below 4 and `g_57E0_0DAB` is 1.

## What the sources say

SRC-MANUAL-1994, page 4: to scroll the screen, move the mouse cursor in the direction the screen
should move; the screen scrolls in that direction until the cursor moves away from the screen's
edge or the edge of the map is reached. Page 14 describes the Game Menu's Center on Leader, which
centres the screen on the party's leader, and page 77 gives the key `H` for the same.

## Differences between builds

None known.

## Open questions

- How often the main loop runs a pass, which sets how fast the view scrolls, and what stores a
  value other than 0 in `g_57E0_0DA8`, which holds the scrolling until the pointer leaves the
  band (Q-EXPLORE-001).
- Whether the mouse setup runs after the video mode's word is stored, which the 320 by 200 range
  depends on, and whether `fn_3D72_0D83`, which runs first while `pointer_mode` is 5, changes the
  view words (Q-EXPLORE-001).
- What `g_57E0_0DAB` holds beyond being 1 whenever edge scrolling and `H` act (Q-EXPLORE-001).

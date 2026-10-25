---
id: FND-EXPLORE-016
title: For an object target the planner walks to the open cell beside the object's footprint nearest the walker, and in mode 1 to the open cell around the walker farthest from the object
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:36E4..2D40:37C1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:38E2..2D40:3A03
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. `2D40:10AE` calls both routines
while it picks the target of an object mode (FND-EXPLORE-014); a cell is a position word shifted
right by 4.

**`2D40:38E2(object, &tx, &ty, walker)`** calls `2D40:342A(walker)`, so `DS:19B2` and `DS:19B4`
hold the walker's footprint, and `2D40:3440(object)` for the object's own footprint size and cut.
It takes the object's cell and the walker's cell and sets its best distance to 999 and its result
to 0. For each cell of the object's footprint, in the order `2D40:34B1` gives them with the
object's size and cut, it tests the four cells left, right, above and below, in that order. A
cell that `25AF:00E1` finds open and whose `1B0B:000C` distance to the walker's cell is below the
best (signed) is stored in `tx`, `ty`, becomes the best, and sets the result to 1. It returns the
result as a byte.

**`2D40:36E4(cx, cy, &tx, &ty)`** reads the cell in `tx`, `ty` as the object's, then stores
`cx`, `cy` there. With `low` = (`DS:19B2` + 1) / 2 and `high` = `DS:19B2` / 2, it sets its best to
the `1B0B:000C` distance from `(cx, cy)` to the object. For each column `x` from `cx - 4` to
`cx + 4`, and in each column each row `y` from `cy + 4` down to `cy - 6`, it skips the cell when
`x` is from `cx - low` to `cx + high` and `y` from `cy - low` to `cy + high`. Otherwise, when the
distance from the object to `(x, y)` is above the best (signed) and `2D40:192C` finds the
footprint at `(x, y)` open, it stores `(x, y)` in `tx`, `ty` and makes that distance the best.

## Interpretation

For modes 10, 11, 13, 18 and 19 the walker heads for the open cell next to the object's
footprint that is nearest to it, tested as a single cell, or for the object's own cell when none
is open. For mode 1 it heads away from the object: to the open place, in a window of 9 columns
and 11 rows around itself that reaches one row further up than down, that is farthest from the
object, and stands still when no such place is farther than it already is. The skipped box is
the walker's own footprint.

## Alternatives

`1B0B:000C`'s units were not read here (FND-PARTY-098, FND-RNG-041). Which actions set modes 1,
10, 11, 13, 18 and 19 was not searched.

## How to reproduce

`resident_listing.py <dsun> 2D40:36E4..2D40:37C1` and `... 2D40:38E2..2D40:3A03`.

---
id: FND-EXPLORE-014
title: 2D40:10AE plans a walk as an 8-way line toward the target, cut back to its last open cell, with two wall followers around each blocked stretch, queues up to 16 step directions, and clears mode 15 only when no route starts from the current cell
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:10AE..2D40:197A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:31F6..2D40:32B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1B2E:0000..1B2E:0B12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BDF:0002..1BDF:0143
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:04A9..28C9:050F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:19A1..28C9:1EDB
tool: Capstone 5.0.9 over resident code; tools/research/exec-census/resident_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. The walk record is the 19-byte
record at `4F49:08A3 + 19 × slot` (words `+0`, `+2` a cell or an object, `+4` the mode,
FND-RNG-068). The step record is the 28-byte record at `4F49:0363 + 28 × slot`:

| Offset | Use |
| --- | --- |
| `+0x00..+0x10` | queued directions, `0xFF` ends the queue |
| `+0x10` | index of the next queued direction |
| `+0x11` | the next direction, 0 to 7, or `0xFF` for none |
| `+0x12`, `+0x14` | target cell of the kept plan; -1 in `+0x12` forces a new plan |
| `+0x16`, `+0x18` | start cell of the kept plan |
| `+0x1A` | skip count into the kept plan's route |

`2D40:10AE(slot, flag)`, in order:

1. Leaves at once when `2D40:2F34` maps the slot to 48 or more, or bit 5 of the slot record's
   first byte is set. Takes the current cell (position words `+0x0A`, `+0x0C` shifted right by 4)
   and the footprint size through `2D40:342A`.
2. With `flag` not 0, sets `+0x11` to `0xFF`, lifts the slot's footprint through `2D40:0F89` and
   goes to step 4. With `flag` 0, stores the slot in `4C13:0367`, calls `1695:01D1`, lifts the
   footprint, runs the fight block of FND-PARTY-199 when `4C10:0019` is not 0, and then, when
   `+0x11` is not `0xFF`, `+0x10` is below 16 and the queued byte there is 0 to 7, moves that byte
   into `+0x11`, adds 1 to `+0x10` and leaves.
3. Picks the target through the 19-word table at `2D40:1906`, indexed by the mode less 1: the walk
   cell for mode 15; for modes 1, 10, 11, 13, 18 and 19 the cell of the object in walk word `+0`,
   or an open cell beside its footprint (`2D40:38E2`, and `2D40:36E4` in mode 1); the current cell
   for every other mode. When the target is the current cell, `+0x11` becomes `0xFF` and it leaves.
4. Keeps the plan when `+0x12`, `+0x14` equal the target. Otherwise, when a search failed earlier
   in this call and `+0x16`, `+0x18` equal the current cell, goes to `2D40:15D6`. Otherwise starts
   a new plan: the target into `+0x12`, `+0x14`, the current cell into `+0x16`, `+0x18`, 0 into
   `+0x1A`.
5. For slots 4 and up outside a fight, when fewer than 32 ticks of `DS:145E` have passed since
   `DS:19AE` and `1B0B:000C` gives a distance above 24 between the current cell and the target,
   sets `+0x11` to `0xFF`, negates the mode word and leaves.
6. Calls `1B2E:07F3` with `DS:0443` at 1 and `DS:19B6` at the slot, from the plan's start toward
   the target with 30 nodes, 80 points and the skip count; when that fails, again with `DS:0443`
   at 0. When both fail, sets `+0x12` to -1, marks the search failed and goes back to step 4.
7. Calls `2D40:31F6` for the furthest point of the route window it can reach from the current cell
   by an 8-way line of open footprints, scanning the window from its end and counting a point equal
   to the current cell as reached. A result of 0 or less counts as none. The first time there is
   none, it sets `+0x12` to -1 and goes back to step 4, which starts a new plan from the current
   cell; the second time it goes to `2D40:15D6`.
8. When the point found is the current cell, adds its index to `+0x1A`; then, when the walk cell is
   the current cell, sets `+0x11` to `0xFF`, and otherwise goes back to step 4. When the point is
   further on, sets `+0x10` to 0, `+0x11` to the first direction of the line to it and queued bytes
   0 to 14 to the next 15, where the direction of the line's last cell is 8 and the entry after it
   `0xFF`, and byte 15 to `0xFF`.
9. Puts the lifted footprint back when the slot's kind is 2 and its combatant's first word is
   above 0, and for the leader `4C13:0369` copies `+0x11` to `DS:19BF` when it is not `0xFF`. The
   exits of step 1 also come here.

`2D40:15D6..2D40:1601` sets `+0x11` to `0xFF` and stores 0 in the mode word when that is 15.

`1B2E:07F3` draws an 8-way line of at most 80 points from the start toward the target (`1B2E:0000`,
one cell per point in the directions the signs give through `1BDF:009B` and the table at
`4C0B:0000`). It steps back from its last point while `2D40:192C` finds a footprint cell there
blocked by `25AF:00E1`, and fails when it gets back to point 0. `1B2E:00B9` puts a node at the
start, at the last open point before and the first after each blocked stretch, and at the end,
failing at 30 nodes. `1B2E:02D2` goes straight from a node when the next cell is open, and
otherwise sends two wall followers with the turns -2 and +2 (`DS:0444`) one cell a round
(`1B2E:01D5`) for at most 500 rounds; the first to land on a later node links the two. With
`DS:0443` at 1 a follower also tries the diagonal between two turns. `1B2E:05C1` walks the linked
nodes and writes up to 79 cells after the skip count.

`28C9:04DD..28C9:04FB`, after the click calls `2D40:10AE`, shows the string at `DS:0DB5` through
overlay 172 `+05DB` when the step record's `+0x11` is `0xFF`.

`28C9:19A1(slot)`, which `28C9:1E41` calls for each slot every pass, acts for the modes the table
at `28C9:1E1B` names (1, 10, 11, 13, 15, 18, 19) when `+0x11` is not `0xFF` and bits 5 and 7 of the
slot record's first byte are clear. It calls `31E0:1808`, tests the cell one step in direction
`+0x11` with `2D40:192C` and calls `2D40:10AE(slot, 1)` when it is blocked, at most twice. When the
cell is open it moves the footprint there (`2D40:3388`, `2D40:32C5`), stores the direction at byte
`+0x1F` of the combatant's record, sets bit 5 of the slot record's first byte and calls
`31E0:271E`.

## Interpretation

A Walk click plans a route as a straight 8-way line to the clicked cell, cut back to the last cell
the walker's footprint fits, with each obstacle on the way followed round its walls in both
directions. The walker steps one cell at a time in the queued directions, popping the next one at
each cell (`31E0:2C86`, `31E0:2D6E`, FND-RNG-072) and replanning when the next cell is blocked.
Arriving leaves the mode at 15 with no direction. When no route starts from the cell the walker
stands on, the mode is cleared, and on the click itself the no-path message is shown. A blocked
target is walked toward as far as the last open cell of the line.

## Alternatives

`1695:01D1`, the pixel movement in `31E0:271E` and `31E0:1808`, and so how long a step takes,
were not read. `1B2E:02D2`'s handling of a follower that lands on an earlier node
(`1B2E:042D..1B2E:04F6`) and its backtracking (`1B2E:0515..1B2E:0579`) were read for their
structure only. What restores a mode word that step 5 negated was not searched.

## How to reproduce

`resident_listing.py <dsun>` over `2D40:10AE..2D40:197A`, `2D40:31F6..2D40:32B1`,
`2D40:36E4..2D40:37C1`, `2D40:38E2..2D40:3A03`, `1B2E:0000..1B2E:0B12`, `1BDF:0002..1BDF:0143`,
`28C9:04A9..28C9:050F` and `28C9:19A1..28C9:1EDB`; `immediate_search.py <dsun> 0374 0363 19BF`.

---
id: FND-EXPLORE-006
title: Each main-loop pass scrolls the view one step of 16 pixels toward the pointer's side of the screen centre while the pointer is on the screen's outer rows or columns, and Center on Leader and H centre the view on a slot
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:23F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:050F..28C9:0568
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:0051..2C5F:0139
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:0CD3..362C:0F12
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 45B9:0034..45B9:0059
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39D1:003F..39D1:00A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2C5F:018C..2C5F:01B2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:34B1..28C9:363C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00079A58..0x00079AA3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000682A9..0x000682ED
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068DBF..0x00068DEC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004DF43..0x0004DF4B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004F9F7..0x0004FA09
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004FA14..0x0004FA1C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0002F226..0x0002F236
tool: tools/research/exec-census/resident_listing.py, overlay_listing.py, store_values.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

`DS` is `57E0`, whose data starts at file `0x4D000` of the installed `DSUN.EXE` (634,416 bytes,
XXH3-128 `e296af55ba2ecde7e77f555c90f33d0b`). Arithmetic below is on 16-bit words, and every
division and comparison is signed. The instruction byte `0x99` without an operand-size prefix
is `cwd`.

**The pass.** The main loop calls `28C9:2322` once a pass (FND-PARTY-031); the routine returns at
once while `g_57E0_0D9C` is 0. Otherwise, when `combat_state` is 0 or `turn_combatant` is below
4, it reads the pointer through `45B9:0034`, which asks the mouse driver for service 3 and stores
CX, the column, through its first argument and DX, the row, through its second. When the column
is not 0, the row is not 0, the column is below 318 and the row below 199 (`28C9:23A8..+23C0`),
it stores 0 in the byte `DS:0DA8` and goes on. Otherwise, when that byte is 0 and `28C9:2522`
returns 1 (it returns the word `DS:0DAB`, FND-PARTY-046), it calls `3D72:0D83` if
`g_57E0_1440` is 5 and then `28C9:050F` with the column and the row. When `combat_state` is not 0
and `turn_combatant` is 4 or more, the pointer is not read.

**The range.** The mouse setup at `39D1:0009` stores 0 as the low end of both ranges, calls the
mode query `1BF3:2A4A` (the word `CS:295F` the mode setter stored, or the BIOS mode,
FND-CONFIG-158) and compares it with the four words at `39D1:0316`, 0x0010, 0x0201, 0x0202 and
0x0203; the first gives a high end of 649 by 349 and the other three 639 by 399; any other mode
gives 319 by 199. It then passes the ranges to services 7 and 8.

**The direction** (`2C5F:0051`). With `sx` the column less 160 and `sy` the row less 100: when
`sx` is 0 the result is 4 if `sy` is above 0 and 0 otherwise; when `sy` is 0 it is 2 if `sx` is
above 0 and 6 otherwise. Otherwise, with `ax` and `ay` their absolute values, `h` is
`((ax * 128) / 160 * 50) >> 7` and `v` is `((ay * 128) / 100 * 80) >> 7`, the shifts arithmetic.
When `ay` is at most `h` the result is 2 if `sx` is above 0 and 6 otherwise; else when `ax` is at
most `v` it is 4 if `sy` is above 0 and 0 otherwise; else it is the word of `DS:0F43` for the
quadrant, 7 for `sx` and `sy` below 0, 5 for `sx` below 0 and `sy` above 0, 3 for both above 0
and 1 for `sx` above 0 and `sy` below 0.

**The tables.** The signed bytes at `DS:29F7` are 0, 1, 1, 1, 0, -1, -1, -1, 0 and those at
`DS:2A00` are -1, -1, 0, 1, 1, 1, 0, -1, 0, the column and row change of directions 0 to 8. The
words at `DS:2A14` to `DS:2A1B` are 140, 70, 190 and 130; a search for stores to them finds none.

**The step** (`28C9:050F`). It computes the direction `d`, then calls `362C:0D80` with the
address of `d`, the addresses of `view_left` and `view_top`, and the column plus `view_left` and
the row plus `view_top` (`mx` and `my`). `362C:0D80` first calls `362C:0CD3` with `d`, the two
view words, `mx` and `my`; that routine returns 1 when `mx` is below `view_left`, `my` below
`view_top`, `view_left + 320` below `mx` or `view_top + 200` below `my`. Otherwise, with
`rx = mx - view_left + dxt[d] * scroll_step_x` and `ry = my - view_top + dyt[d] * scroll_step_y`, it
returns 1 when `rx` is below 140 and `dxt[d]` below 1, when `ry` is below 70 and `dyt[d]` below
1, when `rx` is above 190 and `dxt[d]` above -1, when `ry` is above 130 and `dyt[d]` above -1, or
when `rx` is below 0 or above 320 or `ry` below 0 or above 200; else 0. When it returns 0,
`362C:0D80` returns 0 and changes nothing. Otherwise it computes `nl = view_left + dxt[d] *
scroll_step_x` and `nt = view_top + dyt[d] * scroll_step_y` and four flags: `nl` below the word
`DS:14D7`, `nt` below `DS:14D9`, `nl + 319` at least `DS:14DB`, and `nt + 199` at least
`DS:14DD`. It drops a negative column change when the first flag is set and a positive one when
the third is, and a negative or positive row change by the second and fourth. It takes the
first direction from 0 to 7 whose table entries equal what is left, or 8 when none does. When
that differs from `d` it stores it in `d` and computes `nl` and `nt` again from it. When `d` is
then 8 it returns 0; otherwise it stores `nl` in `view_left` and `nt` in `view_top` and returns
1. On 1, `28C9:050F` calls `362C:06C4` with 1, `d` and the two view words and stores 0 in the
byte `DS:143F`. The same bound words are those the view-centring routine `2C5F:01F3` clamps to
(FND-COMBAT-049) and script request 33 sets (FND-COMBAT-050).

**The step size.** `362C:0EE5`, and its copy at overlay 182 `+056F` (trampoline `56BD:0066`),
store their four arguments in `DS:9BF2`, `DS:9BF0`, `DS:9BF6` (`scroll_step_x`) and `DS:9BF4` (`scroll_step_y`); a search for
stores to the last two finds only these two routines. The caller search finds no caller of
`362C:0EE5` and nine of `+056F`. Eight pass 16 and 16 as the last two arguments, after 4 and 4
when the byte `animations_on` (`DS:1437`) is not 0 and 16 and 16 otherwise: `28C9:360B`,
`28C9:3621`, `31E0:0344` (after it sets `animations_on` to 1), overlay 180 `+06AC`, 182 `+1E77`,
190 `+18E9`, 192 `+06CF` and 203 `+04C6`. The ninth, `28C9:354F` in `28C9:34B1`, passes 32 for
all four; that routine then repeats `362C:0D80` toward an object slot's position until the view
reaches it, and before it returns calls the setter again with 16.

**Center on Leader and H.** The Game Menu's button 10306 goes to overlay 181 `+03F9`
(FND-UI-046's table); after `+04C2`, overlay 182 `+19F8` and `3D72:0D83` it calls `2C5F:018C`
with `leader` (`4C13:0369`) and 1, calls `28C9:31D2` with `leader` three times and calls
`2C5F:0182`. The `H` handler at overlay 190 `+1718` (FND-COMBAT-025) does nothing unless
`turn_combatant` is below 4 and `28C9:2522` returns 1; then it calls `2C5F:018C` with
`turn_combatant` and 1, `28C9:31D2` with `leader` three times, and `2C5F:018C` with
`turn_combatant` and 1 again. `2C5F:018C` passes the object slot's `position_x` and
`position_y` and its mode to `2C5F:01F3`, which sets `view_left` to `(position_x - 160) & ~7` and
`view_top` to `(position_y - 100) & ~7` and clamps them to the four bound words (FND-COMBAT-049).

## Interpretation

While the pointer is on the left or top column or row of the screen, or on columns 318 and 319
or row 199, each pass of the main loop moves the view 16 pixels in the direction from the screen
centre to the pointer, in one of eight directions. At a bound the blocked part of a diagonal is
dropped, and a straight move into a bound does nothing. The scrolling band is of the whole
screen in the mouse driver's range, which is 320 by 200 for the game's mode. Center on Leader
centres the view on the leader, and `H` on the slot whose turn it is, rounded down to a multiple
of 8 and kept inside the bounds.

## Alternatives

- What stores a value other than 0 in the byte `DS:0DA8`, which stops the scrolling until the
  pointer leaves the band, was not searched.
- How often the main loop runs a pass is not decided here; the pass reads no clock in the part
  read.
- The order of the mouse setup against the mode setter's store of 0x0113 (FND-CONFIG-158) was
  not traced; the range is 320 by 200 when the setup runs after it.
- `3D72:0D83`, `362C:06C4`, `28C9:31D2` and `2C5F:0182` were not read; that they redraw and do not
  change the view words is not shown.

## How to reproduce

In `tools/research/exec-census/`, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: `resident_listing.py <dsun>` with `28C9:2322..28C9:250A`, `28C9:050F..28C9:0623`,
`2C5F:0051..2C5F:0139`, `2C5F:018C..2C5F:0271`, `362C:0CD3..362C:0F12`, `45B9:0034..45B9:0059`,
`39D1:0009..39D1:00B0` and `28C9:34B1..28C9:363C`; `overlay_listing.py <dsun> 190 78340 7BE16`,
`181 67EB0 6873C` and `182 68DBF 68DEC`; `store_values.py <dsun> 1408 140a 9bf4 9bf6 2a14 2a16
2a18 2a1a`; `direct_callers.py <dsun> 362C:0EE5 182+056F 2C5F:01F3`; read the words at file
`0x4DF43` and `0x4FA14`, the bytes at `0x4F9F7` and `0x4FA00`, and the eight words at `0x2F226`.
The listings are retained under `GAME_DIR/analysis/explore-scroll-20261019/`. No original
function or game process was run.

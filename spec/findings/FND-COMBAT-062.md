---
id: FND-COMBAT-062
title: 28C9:1E41 runs the walker sweep 2D40:197A with the constant 9999 for all slots, which moves walkers, runs idle entry scripts, unlinks idle walk records and puts followers in formation; slot 9999 indexes nothing on that path
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1E41..28C9:1EDB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:197A..2D40:1FB5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x000245B5..0x00024601
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2001..2D40:207E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0BA4..2D40:0C31
tool: tools/research/exec-census/resident_listing.py, direct_callers.py, segment_offset_refs.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000. Walk records are the 19-byte records at
`4F49:08A3` (FMT-SCRIPT-005).

**`28C9:1E41(p)`** pushes the constant 9999 and calls `2D40:197A`, stores 8 at `p`, then walks the
list headed by `DS:5AF5` while `DS:0D9C` is not 0, calling `28C9:19A1` for each slot, and for the
slot equal to `DS:426D` stores that slot's `next_step` at `p`. `direct_callers.py` finds one call,
`28C9:21E3` in `28C9:1EDB`, reached at `28C9:21D1` when the combat word `4C10:0019` is 0; `p` is
`28C9:1EDB`'s local `[bp-6]`. When `p` keeps 8, `28C9:1EDB` leaves at `28C9:21F7`.

**`2D40:197A(s)`** takes one argument. With `s` 9999 or more it sweeps from the resume slot
`4F49:0000` up to slot `0x2F`, otherwise slot `s` alone (`198E..19AA`); it stops when 4 ticks of
`game_tick` have passed, storing the next slot in `4F49:0000` (`1D3D..1D45`), and stores 0 there
when a sweep ends at `0x30` or above (`1D60..1D75`). When the double word `4F49:0002` is more than
`0x3C` above `4C13:035B` it calls `2D40:37C1` (`19B5..19E3`). For each slot it clears bit 7 of byte
`+0x10` and skips the slot once when that bit was set, restores a negated `walk_mode`, moves a
walker toward its target by its mode through two 19-entry jump tables at `2D40:1FB5`, storing mode
0 and `next_step` `0xFF` on arrival and calling `2D40:10AE(slot, 1)` otherwise, and calls
`2D40:2001(slot)` and `2D40:207E(slot)`. With `s` 9999, a second loop puts each party member
other than `leader` on mode 15 toward a cell behind the leader. No table is indexed with `s`.

**`2D40:2001(slot)`** returns unless `walk_mode` (word `+4`) is 0 and the slot is 4 or more. When
word `+0x0C` (`entry_script_0`) is not 0 it stores the slot in `trigger_actor` and runs that entry
point through `172C:000C`; otherwise, when word `+6` is 0, it calls `2D40:0BA4(slot)`.

**`2D40:0BA4(slot)`** unlinks the record from the list headed by `DS:5AF5`, mending its
neighbours' links and the head, fills its 19 bytes with 0 through `1000:3FA2` and stores -1 in both
link words.

## Interpretation

`28C9:1E41` is the walker step of the map loop: it moves the walkers, runs the entry script of
each idle object that has one, frees the walk records of idle objects with no script and no target,
keeps the party in formation, and reports the current character's next direction. In
BUG-COMBAT-001's passes it runs after each end of combat and returns 8, since no slot is 9999. The
records FND-COMBAT-061 sees cleared are idle records with no entry point and no target, which the
end of combat had re-linked to the leader through `2D40:3D4C`; clearing them changes nothing the
player sees. On these passes, as in normal play, walkers keep moving and idle scripts keep running.

## Alternatives

Which of the three tests of `2D40:2001` excludes each of the 12 other swept slots of 4 or more was not observed.
The callees `2D40:207E`, `2D40:2196`, `2D40:37C1` and the overlay calls of the mode branches were
read only at their calls. That the clock check does not fire on the passes rests on `4C13:035B`
only rising. `28C9:1EDB` from `28C9:21FA`, which runs with a direction, was not read. Calls of
`28C9:1E41` through stored far pointers were not searched.

## How to reproduce

`resident_listing.py <dsun> 28C9:1E41..28C9:1EDB 2D40:197A..2D40:1FB5 2D40:2001..2D40:207E
2D40:0BA4..2D40:0C31 28C9:1EDB..28C9:1F20 28C9:20F0..28C9:2240`; read the two 19-word tables at
`2D40:1FB5`; `direct_callers.py <dsun> 28C9:1E41 2D40:2001 2D40:0BA4`; `segment_offset_refs.py
<dsun> 4F49 0000` and `... 4F49 0002`.

---
id: FND-PARTY-089
title: The word at 4C10:0019 holds the fight state, 0 outside a fight and 1 to 4 within one, and the fight routine overlay 173 +059A, whose result its callers store there, ends a fight by running the level gain when a party member still stands and returning 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005AEFC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B175..0x0005B17D
    kind: file-data
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AFB0..0x0005AFBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B000..0x0005B049
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0AD..0x0005B175
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005DC24..0x0005DE0C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E750
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E9A1..0x0005E9C4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2A00:0DA7..2A00:0DF4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2B00:0DB4..2B00:0DE6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 27C5:0000..27C5:000C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2E00:07FC..2E00:083E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005CBB3..0x0005CBDE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073340..0x00073378
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073BAE..0x00073BBA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E7E6..0x0007E82A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00081626..0x00081632
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/store_values.py, overlay_listing.py, resident_listing.py, direct_callers.py, trampoline_target.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 173's code starts at file `0x5A920`. Slots, the state byte and the combatant
records are as FND-PARTY-081 gives; `combat_mark` is the combatant record's byte at `+0x14`.
Resident code is loaded at segment `0x1000`, so its unrelocated segment `0x3C10` is `4C10`.

**The stores.** `store_values.py <dsun> 19` lists 108 lines of stores with the displacement
`0x19`. Ten of them write through ES after a load of segment `4C10`, as the selector `0x2E0` in
overlay code or `0x4C10` in resident code:

| Site | Value | Condition |
| --- | --- | --- |
| `27C5:0005` | 0 | none, at the start of the code that follows |
| `2A00:0DF0` | the result of overlay 173 `+059A` | see below |
| `2B00:0DDE` | the result of overlay 173 `+059A` | see below |
| `2E00:0837` | 4 | a party slot is empty or its combatant's `combat_mark` is above 1, when a test through `5671:0089` leaves bit 0 clear |
| overlay 173 `+22B7` | 1 | the word is 0, a local byte is 0 and overlay 173 `+3304` returns a value that is not 0 |
| overlay 173 `+409D` | 2 | the word is 1 |
| overlay 188 `+04CF` | 1 | the word is 0 and overlay 173 `+3304` returns a value that is not 0 |
| overlay 188 `+0D13` | 0 | none; overlay 188 `+0CD7`, called through `5702:00D4` from overlay 180 |
| overlay 193 `+0533` | 1 | the word is 0 or 1 and tests on locals pass |
| overlay 194 `+04FB` | 0 | none, at the end of a routine that loads `WIND_MAINMENU` |

`store_values.py <dsun> 13FB` lists stores to the byte at `DS:13FB` at `27A7:000E` and
`27D3:0007` (0), overlay 173 `+3CE1` (1) and `+3D8C` (0), overlay 192 file `0x7D6EF` (1) and
overlay 204 file `0x8C1F8` (1); the last two were not checked to lie on an instruction path.

**Overlay 173 `+059A`** (trampoline `5671:0039`) takes a state and a pointer to a combat slot
number. `direct_callers.py` finds it called from the resident code at `2A00:0DE3` and `2B00:0DD1`.
The first passes the word at `4C10:0019` and stores the result back (`2A00:0DD6..2A00:0DF4`); the
second passes 3 when the word is not 0 and 0 when it is, and stores the result back
(`2B00:0DB4..2B00:0DE2`). Both pass `DS:426D` as the pointer. `+059A` jumps through the four
words at `+0855` (file `0x5B175`) for states 1 to 4, to `+05DC`, `+0660`, `+0690` and `+06C1`,
and to `+06C4` for any other state (`+05C9..+05DC`). Two paths reach `+0729`:

1. State 3 enters at `+0690`, which calls `+3304` and goes to `+0729` when it returns 0
   (`+0690..+069E`).
2. From `+06E0` it calls `+1098`; when that returns 9,999 it calls `+0C65` and goes to `+0729`
   when that returns 0 (`+06E0..+0729`).

From `+0729` it reloads resources, then calls `+3E30`, overlay 195 through `574E:0052`, overlay
206 through `5799:0039` and `+3CDA`, then, when the byte at `DS:13FB` is 0, overlay 210 `+0B44`
with overlay 188 `+0101` after it, and returns 0 (`+0729..+07D3`). Its other returns are 2 when
a local byte is 0 and otherwise the result of `+1238` (`+07D5..+0855`).

**Overlay 173 `+3304`** loops over the 48 combat slots. For a slot whose state byte is 2: when
the slot is a party slot whose combatant's `combat_mark` is below 3, it counts a standing member,
and when overlay 193 through `573B:0020` with the slot and `0x6E` returns a value that is not 0 it
sets a flag and adds 1 to the count; when the bit of the
combatant's byte at `+0x15` is set in the mask that `+0958` returns, it counts the combatant and,
unless a flag is already set, sets the flag when `+0000`, given the combatant's position, a mask
of the kinds opposed to it and the distance `0x18`, returns a value above 0; a slot with the bit
`0x80` of its byte at `DS:67BB` plus 37 times the slot counts through `573B:0020` with `0x1E`.
It returns the low byte of the count when the flag is set and a member was counted, and 0
otherwise (`+3304..+34EC`).

**Overlay 173 `+3CDA`** stores 1 at `DS:13FB` and, for each slot 0 to 3 whose state byte is 2,
calls overlay 193 through `573B:0020` and, when that returns a value that is not 0, `573B:004D`,
each with the slot and `0x45`; stores 0 in the word at `4F49:08A7` plus 19 times the slot and
`0xFF` in the byte at `4F49:0374` plus 28 times the slot; and, when the combatant's `combat_mark`
is 1, 2 or 3, stores 1 in its word at `+0x00` when that is below 1 and otherwise stores 0 at
`DS:13FB` unless the `combat_mark` is 3, and in both cases stores 1 in the `combat_mark`; for any
other `combat_mark` it sets the bits `0x50` of the byte at `+5` of an 8-byte record through the
far pointer `DS:67B7` (`+3CE1..+3DBB`). When `DS:13FB` is still 0 it calls overlay 182 through
`56BD:0020` with the first slot whose state byte is 2 and whose `combat_mark` is now 1, and then,
when the word at `4C10:0019` is not 0, calls `1695:02E7` with the word at `DS:426D` and `0xF461`;
when `DS:13FB` is 1 it calls overlay 201 through `5778:003E` (`+3DBB..+3E2F`).

## Interpretation

The word at `4C10:0019` is the fight state: 0 outside a fight, set to 1 when overlay 173 `+3304`
reports opposed combatants within reach of each other and a party member standing, and stepped
through 1 to 4 by the fight routine `+059A`, whose result its two resident callers store back.
While it is not 0, every experience award of overlay 188 `+1604` defers the level gain
(FND-PARTY-086). A fight ends when `+3304` stops reporting such combatants in state 3, or when
the turn order has no next combatant and `+0C65` returns 0. Then `+3CDA` brings every party member
with a `combat_mark` of 1 to 3 back to a `combat_mark` of 1 with at least 1 hit point; `DS:13FB`
stays 1 only when no such member had a `combat_mark` of 1 or 2 with 1 hit point or more, which
reads as the party's defeat. Otherwise the party gains levels for the experience the fight gave,
and `+059A` returns 0, so the state goes back to 0.

## Alternatives

- FND-PARTY-087 recorded the same reading of `+3CDA` and of the gain at `+07AD`, but listed only
  the six stores whose segment register is loaded with the selector `0x2E0`, missing the resident
  stores through `0x4C10` and the overlay 173 store at `+409D`, whose segment register is loaded
  further back. It read the word as set to 1 only while `+3304` reports a fight and never cleared
  when one ends; the fight routine's result clears it.
- A write to the word other than the ten: all ten stores through ES with the displacement `0x19`
  go to segment `4C10`, and DS is `57E0`, but a block write over segment `4C10`, or a store
  through a register-based address, would not be listed.
- What the states 1, 2 and 4 each do, and what the tests through overlay 193 count, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `store_values.py <dsun> 19` and `13FB`, keeping
the stores through ES; `overlay_listing.py <dsun> 173 5AEBA 5AF80`, `173 5AFDB 5B1A0`, `173
5DC24 5DE0C`, `173 5E5FA 5E760`, `173 5E9A0 5E9C4`, `173 5CB80 5CBE4`, `188 73330 73380`, `188
73B77 73BC0`, `193 7E7E0 7E830` and `194 815F0 81640`; `resident_listing.py <dsun>
2A00:0D90..2A00:0DF4 2B00:0D90..2B00:0DE6 27C5:0000..27C5:0030 2E00:07F0..2E00:0840`;
`direct_callers.py <dsun> 173+059A 173+3304 173+3CDA 188+0CD7`; and `trampoline_target.py <dsun>
5671:0039 5671:0089`. Read the 8 bytes at file `0x5B175`.

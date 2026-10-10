---
id: FND-PARTY-087
title: The word at 4C10:0019 that defers the level gain is set to 1 while overlay 173 +3304 reports a fight going on, and when the fight loop ends the party gains levels if any member was still standing
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-PARTY-089]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AFB0..0x0005AFBE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0AD..0x0005B0D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E5FA..0x0005E750
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
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/store_values.py, overlay_listing.py, direct_callers.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 173's code starts at file `0x5A920`. Slots, the state byte and the combatant
records are as FND-PARTY-081 gives; `combat_mark` is the combatant record's byte at `+0x14`.

**The stores.** `store_values.py <dsun> 19` lists 108 lines of stores with the displacement
`0x19`; six of them follow a load of the selector `0x2E0`, which the listings resolve to segment
`4C10`, into the segment register used. They are overlay 173 `+22B7` and overlay 188 `+04CF`,
which store 1 when the word is 0 and overlay 173 `+3304` returns a byte that is not 0 (for `+22B7`
also when a local byte is 0) (`+2293..+22BE`, `+04B4..+04D6`); overlay 193 `+0533`, which stores
1 after a call to `28C9:2E0E` when the word is 0 or 1 and tests on its locals pass (`+04F6..+053A`); and overlay 188 `+0D13`
and overlay 194 `+04FB`, which store 0, the second at the end of a routine that loads
`WIND_MAINMENU`. A store through a segment register loaded earlier
than the six instructions the tool prints, or a block write over the segment, is not found by
this search.

`store_values.py <dsun> 13FB` lists stores to the byte at `DS:13FB` at `27A7:000E` and
`27D3:0007` (0), overlay 173 `+3CE1` (1) and `+3D8C` (0), overlay 192 file `0x7D6EF` (1) and
overlay 204 file `0x8C1F8` (1); the last two were not checked to lie on an instruction path.

**Overlay 173 `+3304`** is called from overlay 173 `+0692`, `+1081`, `+10B2` and `+22AB`, and
through trampoline `5671:00A7` from overlay 188 `+04C1`. At `+0692` a result of 0 sends the code
to `+0729` (`+0690..+069E`), which runs on to `+078D`.

**Overlay 173 `+078D..+07B9`** calls `+3E30`, overlay 195 through `574E:0052`, overlay 206 through
`5799:0039` and `+3CDA`, then, when the byte at `DS:13FB` is 0, overlay 210 `+0B44` with overlay
188 `+0101` after it.

**Overlay 173 `+3CDA`** stores 1 at `DS:13FB` and, for each slot 0 to 3 whose state byte is 2
(`+3CE1..+3CFD`, `+3DB2..+3DBB`):

1. It calls overlay 193 through `573B:0020` with the slot and `0x45`, and when that returns a value
   that is not 0, through `573B:004D` with the same arguments (`+3CFD..+3D17`).
2. It stores 0 in the word at `4F49:08A7` plus 19 times the slot and `0xFF` in the byte at
   `4F49:0374` plus 28 times the slot (`+3D17..+3D38`).
3. When the slot's combatant has a `combat_mark` of 1, 2 or 3: when its word at `+0x00` is below
   1 it stores 1 there; otherwise, unless the `combat_mark` is 3, it stores 0 at `DS:13FB`. In both
   cases it stores 1 in the `combat_mark` (`+3D38..+3DB2`).
4. Otherwise it sets the bits `0x50` of the byte at `+5` of an 8-byte record through the far
   pointer `DS:67B7` (`+3D9B..+3DB2`).

When `DS:13FB` is still 0 it calls overlay 182 through `56BD:0020` with the first slot whose state
byte is 2 and whose `combat_mark` is now 1, and then, when the word at `4C10:0019` is not 0, calls
`1695:02E7` with the word at `DS:426D` and `0xF461`; when `DS:13FB` is 1 it calls overlay 201
through `5778:003E` (`+3DBB..+3E2F`).

## Interpretation

Overlay 173 `+3304` reads as the test that a fight goes on: the loop at `+0692` leaves for the code
that ends at `+07B9` when it returns 0, and the word at `4C10:0019` is set to 1 only where it
returns a value that is not 0. So while a fight goes on every experience award of overlay 188
`+1604` defers the level gain (FND-PARTY-086), and when the loop ends `+3CDA` brings every party
member with a `combat_mark` of 1 to 3 back to a `combat_mark` of 1 with at least 1 hit point.
`DS:13FB` stays 1 only when no such member had a `combat_mark` of 1 or 2 with 1 hit point or
more, which reads as the party's defeat; otherwise the party gains levels for the experience the
fight gave. Nothing in the code read here clears the word at `4C10:0019` at the end of a fight,
and the gain at `+07AD` does not test it.

## Alternatives

- `+3304` returning a value that is not 0 for another reason than a fight going on: its body was
  read only to its first party loop, and its other callers at `+1081`, `+10B2` and `+22AB` were
  not read.
- Other writers of the word at `4C10:0019`: the search finds only stores whose segment register
  is loaded within the six instructions before; one loaded earlier, or a block write, would be
  missed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `store_values.py <dsun> 19` and `13FB`, keeping
the hits whose printed instructions load `0x2e0`; `overlay_listing.py <dsun> 173 5AF80 5AFE0`,
`173 5B080 5B0E0`, `173 5E5FA 5E760`, `173 5CB80 5CBE4`, `188 73330 73380`, `188 73B80 73BC0`,
`193 7E7E0 7E830` and `194 815F0 81640`; and `direct_callers.py <dsun> 173+3304 173+3CDA`.

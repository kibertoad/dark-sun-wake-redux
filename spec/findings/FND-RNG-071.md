---
id: FND-RNG-071
title: Given slot 9999 by 2D40:2F34, the slot release changes no list, overlay 188 +1876 and 2D40:3D4C store only into slot records 0 to 47 and 4C13:0369, and overlay 193 +2327 moves effect rows within their table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:03C1..2D40:0453
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0CC2..2D40:0CE4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3D4C..2D40:3DBD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:00E1..25AF:014B
tool: Capstone 5.0.9 over the load image; Python 3.14.7
environment: null
---

## Observation

Addresses are static. FND-RNG-068 lists the routines `2D40:2F34` calls with its slot `n` and the
record `r` it took: `2D40:3D4C(n, r)`, the overlay 193 trampoline `573B:00AC` with `n` and `r`,
and `2D40:03C1(n)`.

**`2D40:03C1(9999)`.** For a slot of `0x30` or more it skips the clearing of the slot's entries,
chooses the list head `DS:56EF` for slots `0x140` to `0x207` and `DS:56F1` otherwise, and calls
`2D40:0CC2` with the slot and the head, then overlay 188 `+1876` (trampoline `5702:0106`) with the
slot. `2D40:0CC2` returns without a store for a slot below 5 or of `0x208` or more
(`2D40:0CD3..0CDC`). Overlay 188 `+1876` sets word `+4` to 0 in each 19-byte slot record 5 to
`0x2F` whose word `+0` is the slot and word `+4` is not 15 (FND-EXE-614).

**`2D40:3D4C(9999, r)`.** It returns at once when `r` is 9999 or more or the byte at
`4F49:0C33 + 3 × r` is 0. Otherwise, for each slot record 0 to `0x2F` at `4F49:08A3` whose word
`+4` is not 15 and word `+0` is 9999, it stores `r` in word `+0`, and when `4C13:0369` is 9999 it
stores `r` there.

**`573B:00AC`** enters overlay 193 `+2327`, which moves each 10-byte effect row owned by its
first argument to the second within the row table whose count is `DS:2100` (FND-COMBAT-033).

**`25AF:00E1`** takes a cell, not a slot: `28C9:0353` passes it the cell of the point
(FND-RNG-068). Its body reads the map byte of the cell through the far pointer `DS:0538` and makes
no store; its near call goes to `25AF:02FA`.

## Interpretation

With slot 9999, the routines `2D40:2F34` goes on to call store only into the slot records, the
current-combatant word `4C13:0369` and the effect-row table, so none of them writes the clock
conversion's windows. The stray stores of the 9999 path are those FND-RNG-068 lists.

## Alternatives

`25AF:02FA` and the copy routine overlay 193 `+2327` uses were not read here; neither takes the
slot.

## How to reproduce

Disassemble the address ranges above from the load image.

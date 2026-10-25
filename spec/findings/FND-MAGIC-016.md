---
id: FND-MAGIC-016
title: DS:9BD3 is 1 for a party spell or item power left pending by the cast window's DATA byte 0x0C of 3 when the inventory window closes, and overlay 189 +076A casts the same pending number on a clicked character box with the same byte 1; both routes carry the level 6, so overlay 211 +1FB5's level reaches the cast routine on no party route for a number below 235
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092EA2..0x00092F2E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00092FF1..0x0009304D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00080580..0x00080617
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000974EB..0x00097508
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099D3F..0x00099D4D
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000750D5..0x00075122
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007548D..0x000754F4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00099DC4..0x00099DEA
tool: tools/research/exec-census/overlay_listing.py, immediate_search.py and direct_callers.py at the commit that adds this finding; Capstone 5.0.7; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x91890 + x` for overlay 208, `0x7E2F0 + x` for 193,
`0x95E10 + x` for 211, `0x99260 + x` for 213 and `0x749B0 + x` for 189. `cur` is the word
`4E71:0B44`.

**The one writer.** `immediate_search.py <dsun> 9BD3 9BE5 43F7` finds one store to `DS:9BD3`, at
overlay 208 `+1662` (file `0x92EF2`), one to `DS:9BE5`, at `+166C` (`0x92EFC`), and the reads at
`+1776` (`0x93006`, `DS:9BD3`) and `+178E` (`0x9301E`) and `0x92AB6` (`DS:9BE5`). Overlay 208
`+1612` (`+1612..+169E`) stores its first argument at `DS:9BE0`, its third at `DS:9BE2`, the low
byte of its fourth at `DS:9BD3` and the low byte of its fifth at `DS:9BE5` (`+1652..+166C`), as
FND-RNG-064 reads.

**The cast from the cursor.** Overlay 208 `+1761` (`+1761..+17BD`) calls overlay 193 `+2290`
(`+179F`) with the words, first to last: `DS:9BE0`, `DS:9BDE`, `DS:9BE2`, the byte `DS:9BE5`
sign-extended, `DS:9BDC`, `DS:9BDA`, `DS:9BD8`, `DS:9BD6`, the far pointer `DS:9BE6`, the byte
`DS:9BD3` and 1 (`+1774..+179B`). So `+2290`'s fourth argument (`bp+0xC`) is the fifth argument
of `+1612`, and its byte at `bp+0x1A` is the fourth.

**The dispatcher.** Overlay 193 `+2290` (`+2290..+2327`) returns 0 for a number of 0x148 or more
(`+22A3`). When the byte at `bp+0x1A` is not 0, or the number is 0x10D or more, it calls `+0000`
with the number and the low byte of `bp+0xC` (`+22A9..+22CF`). Otherwise it sends a number below
0xEB to overlay 177 `+0492`, with no `bp+0xC` (`+22D7..+22F1`), and 0xEB to 0x10C to overlay 176
`+0196` as the number less 0xEB (`+22F8..+2317`). FND-MAGIC-003 reads `+0492` passing overlay 177
`+07AF`'s full class level.

**The pending number.** `immediate_search.py` finds two stores of 1 to `DS:43F7`. Overlay 211
`+16DB..+16F6`, in the cast handler `+10F4`, stores the chosen number `SI` at `DS:43F5` and 1 at
`DS:43F7` when byte `+0x0C` of `DATA SI` is 3 (FND-PARTY-109). Overlay 213 `+0ADF..+0AE5` stores
its `[bp-4]`, an item's power (FND-PARTY-243), at `DS:43F5` and 1 at `DS:43F7`. The other hits
store 0.

**The inventory handler** overlay 189 `+0559` (FND-UI-061) reads `DS:43F7` twice:

- At `+0725..+076A`, for a character box with `DS:43F7` set and a left click (`bp+0x18` below 8),
  it clears `DS:43F7` (`+0732`) and calls `+2290` with `cur`, the box's control less 0x2C24, the
  word `DS:43F5`, 6, four words of 0, a far pointer to its byte `bp-5`, 1 and 0
  (`+0744..+076A`). So `bp+0xC` is 6 and the byte at `bp+0x1A` is 1.
- At `+0ADD..+0B3C`, after control 0x2844 (`BUTN/10308`, which closes the window, FND-UI-061) with
  `[bp-4]` nonzero, `[bp-4]` being `DS:43F7` as it was at entry (`+0565..+056A`), and `DS:43F5`
  below 0x148 (`+0B26`), it calls overlay 208 `+1612` with the sign-extended byte `DS:6276`, an
  icon number, `DS:43F5`, 1 and 6 (`+0B2E..+0B3C`).

Overlay 213 `+0B64..+0B82` calls `+1612` the same way, with `cur`, `[bp-2]`, `[bp-4]` when below
0x148, 1 and 6. `direct_callers.py <dsun> 211+1FB5` finds the near calls that feed the other
`+1612` calls in overlay 211 (`+1096`, `+1725`, `+18B0`) and the far calls of overlay 172
(`+0DD5`, `+0E34`).

## Interpretation

`DS:9BD3` can be 1 for a party spell. A spell chosen in the cast window whose `DATA` byte `+0x0C`
is 3 is left pending in `DS:43F5` with `DS:43F7` set. Clicking a character box in the inventory
window casts it there through `+2290` with byte 1; closing the inventory window with the pending
number still set hands it to overlay 208's cursor with byte 1, so `+1761` calls `+2290` with
`DS:9BD3` at 1. An item power left pending by overlay 213 takes the same two routes, and overlay
213 can also start the cursor with byte 1 directly.

On each of these routes the level `+2290` passes to the cast routine `+0000` is the constant 6,
never `+1FB5`'s value. On the routes where `+1612`'s fifth argument is `+1FB5`'s value (overlay
211 and overlay 172) the byte is 0, so for a number below 0xEB `+2290` drops the level and
overlay 177 `+0492` passes the full class level from `+07AF` (FND-MAGIC-003). So the ranger's
level less 7 never reaches `+0000` for a party spell. A party spell reaches `+0000` either with
the caster's full class level, or with 6 when it was pending and cast from the inventory window.

## Alternatives

- A store to `DS:9BD3` through a computed address is outside `immediate_search.py`, which decodes
  at every byte and finds only `+1662`.
- Which spells have `DATA` byte `+0x0C` 3 was not tallied here; FND-PARTY-109 and FND-PARTY-242
  read the byte's cases.
- Whether a pending number in `DS:43F7` survives until the inventory window opens, and which
  other routines clear it (overlay 211 `+073D`, overlay 189 `+042F`, overlay 190), was not
  followed past FND-EXE-608 and FND-UI-061.
- What `+0000` and `+003C` do with the level, 6 or the class level, stays with Q-MAGIC-006.

## How to reproduce

From `tools/research/exec-census/`: `immediate_search.py <dsun> 9BD3 9BE5 43F7`;
`overlay_listing.py <dsun> 208 92EA2 93080`, `... 193 80580 80618`, `... 211 974E0 97508`,
`... 213 99D20 99D60`, `... 213 99DA0 99DEC`, `... 189 750B0 75130`, `... 189 754B0 754F6` and
`... 189 749B0 75380` (the entry `+0559` and its read of `DS:43F7` at `+0565`);
`direct_callers.py <dsun> 211+1FB5 189+0559`. Map the push order of each call to `+2290`'s
argument words.

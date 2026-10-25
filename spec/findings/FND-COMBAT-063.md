---
id: FND-COMBAT-063
title: Returning from a full-screen screen or loading a region stores 4C13:0369 in DS:426D with combat off, so F1 or F2 followed by EXIT, a save or a load would end BUG-COMBAT-001's passes; nothing rewrites record 9999's bytes
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006A248..0x0006A3CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E27..0x00069FD3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:2440
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1261..28C9:1584
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000796DB..0x00079E67
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007DEF3..0x0007E00C
tool: tools/research/exec-census/overlay_listing.py, resident_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Capstone 5.0.9; Python 3.14.7
environment: null
---

## Observation

Overlay offsets `+x` are at file `0x68850 + x` for overlay 182, `0x78340 + x` for 190 and
`0x7D300 + x` for 192. The state is FND-RNG-077's: `DS:426D` 9999, combat word `4C10:0019` 0, and
`DS:0CE6..0D0B` (record 9999 of the 37-byte records at `DS:67BB`) filled with `0xFF`.

**Stores of `4C13:0369` into `DS:426D` with combat off.** Overlay 182 `+19F8` stores 0 in
`DS:0DA4` and `right_button_held`, and only when `DS:0DAB` is 2 goes on; then, when the combat
word is 0, it stores `4C13:0369` in `DS:426D` at `+1AFF`. Overlay 182 `+15D7`, the region load,
stores `4C13:0369` in `DS:426D` at `+1772`. Neither tests `DS:426D` or record 9999.

**Screens.** Overlay 182 `+1856` sets `DS:0DAB` to 2 and `DS:0D9C` to 0, and the map loop at
`28C9:2322` calls `28C9:1EDB` only while `DS:0D9C` is not 0. In the key dispatcher, `F1`
(`190 +17BD`) and `F2` (`+1807`) do not test `DS:426D`; they open the overlay 192 screen, whose
shared routine `+024B` calls `182 +1856`. The Save and Load window callback, after a save or load
and with `DS:627C` 0, calls `182 +19F8` (`+0C21`); EXIT leaves through `+0CEA`.

**The other stores to `DS:426D`.** The leader keys are refused at `28C9:156C` (FND-PARTY-176).
Overlay 190 `+1A8E`, which the `C`, `E`, `I`, `U` and `V` keys reach, returns when `DS:426D` is 4
or more. Overlay 190 `+0969`, `+1090` and `+10AB` run only on character screens. `28C9:1EDB` and
`28C9:305F` store `DS:0DA5`, which only `28C9:2127..213D` sets from `4C13:0369`, with combat word
1. Overlay 182 `+1291` needs `DS:0DAB` 0. Inside a pass, `+1D3E`'s store is overwritten by
`173 +07CF` in the same call and `+19F8` returns because `DS:0DAB` is 1.

**Record 9999.** `immediate_search.py` finds no store to `DS:0CE6..0D0B` by displacement, nor
through the `[bx + 0x67BB]` and `[bx + 0x67BC]` forms with a slot that can reach it. A load reads
`SAVE/1` into `DS:67BB..95FB` (FND-SAVE-015), which does not cover it.

## Interpretation

The passes stop while a full-screen screen is open, and on the way back to the map, or after a
region load, `DS:426D` holds the leader again. F1 or F2 and then EXIT, a save or a load, or F3,
Save and EXIT (with BUG-SAVE-001's side effect), would each end the state. Record 9999 stays
`0xFF` until the program restarts, so a later end of combat that leaves 9999 in `DS:426D` would
start the passes again with no click.

## Alternatives

Whether the `F1` and `F2` keys reach the dispatcher while each pass flushes input (FND-COMBAT-060)
was not shown. That the EXIT and the finished action reach `+0CEA` and `+0C21` rests on overlay
192's listing and FND-SAVE-025's runs. Which clicks in pointer modes 2 and 3 open a full-screen
screen, what `173 +3304` tests beyond its loop, and whether a load rewrites the month lengths at
`DS:3950` were not read. Block copies with a computed destination are covered for record 9999
only as far as FND-RNG-087 and FND-RNG-092 go.

## How to reproduce

`overlay_listing.py <dsun> 182 6A248 6A3CA`, `... 182 69E27 69FD3`, `... 182 6A0A6 6A1F6`, `...
190 796DB 79E67`, `... 192 7DEF3 7E00C`; `resident_listing.py <dsun> 28C9:2322..28C9:2440
28C9:1261..28C9:1584 28C9:1EDB..28C9:1F20 28C9:20F0..28C9:2240`; `direct_callers.py <dsun> 182+19F8
182+15D7 182+1856`; `immediate_search.py <dsun> 426D 0CE6 0CE7`.

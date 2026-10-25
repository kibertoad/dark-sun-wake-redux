---
id: FND-UI-055
title: The Game Menu's buttons other than the overhead map, Exit to DOS and Load or Save close the menu and open the character, inventory, spell, effects or Preferences window, centre the view on the leader, set the pointer mode, toggle the party's collapse or return to the game
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00067F43..0x000682F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000684C2..0x000684F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068330..0x0006836D
tool: Capstone 5.0.9 over overlay code; tools/research/exec-census/overlay_listing.py and trampoline_target.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 181's code starts at file `0x67EB0`.
FND-UI-046 reads the handler's table of fourteen button numbers and targets, and FND-UI-053 the
targets of 10301 (Exit to DOS) and 10302 (Load or Save). `leader` is the word `4C13:0369`.

The opener `+0480` requests `WIND/10500` through overlay 182 `+016B` with the handler `56B9:0034`
and the doubleword `0x002A0037`, x 55 and y 42 (`+04AC`). `+0612` releases the menu window held at `4E46:0000`, when there is one, through overlay 182
`+0128` with 1, and clears that pointer and `DS:1431`.

Before the table lookup, for any button other than 0 and 10313 the handler sets the button's
frame to 4 through `3EBE:071F` and calls `28C9:2A81` with 1 (`+0048..+0072`). The targets:

| Button | Target | What it does |
|---|---|---|
| 10300 | `+02C0` | `+0612`, then overlay 212 `+061C` with the 66-byte record at the far pointer `DS:19C5` plus `0x42 × leader`, the 49-byte record at `DS:19C9` plus `0x31 × leader`, and `leader` |
| 10303 | `+025E` | sets button 10303's frame to 5, calls `28C9:2A81` with 1 and `+0612`, then overlay 203 `+0000` |
| 10306 | `+03F9` | `+0612`, overlay 182 `+19F8` and `3D72:0D83`; then `2C5F:018C` with `leader` and 1, and `28C9:31D2` with `leader` three times |
| 10308 | `+03E0` | `+0612`, overlay 182 `+19F8`, `3D72:0D83`, then `28C9:2A44` with 1 |
| 10310, 10311, 10312 | `+0296`, `+029A`, `+029E` | `28C9:2535` with 1, 2 or 4; then `+0612`, `28C9:2A44` with 1, overlay 182 `+19F8` and `3D72:0D83` |
| 10313 | `+0093` | nothing while the combat word `4C10:0019` is not 0; otherwise overlay 182 `+1F1D` with 0 when the byte `DS:13FA` is not 0 and overlay 182 `+1E81` with 0 when it is 0, then overlay 203 `+082D` with the menu window, 10313 and 1 when `DS:13FA` is now 0, or 0 |
| 11304 | `+0302` | `+0612`, then overlay 190 `+36D2` with `leader` and 9999 |
| 11305 | `+031F` | sets button 11305's frame to 4, calls `28C9:2A81` with 1 and `+0612`, then overlay 211 `+02B5` with the same three arguments as 10300 |
| 11306 | `+037E` | the same as 11305 with overlay 202 `+0287` |

Every target except 10301's quit branch then calls `2C5F:0182` and returns 0 (`+0434`).

## Interpretation

The menu calls the routines that overlay 190's keys `v`, `i`, `c` and `e` and `p` call
(FND-INPUT-012) for the leader: View Character opens the character window (overlay 212 `+061C`),
View Inventory the inventory (`+36D2`), Cast spells the spell window (overlay 211 `+02B5`),
Current spell effects the effects window (overlay 202 `+0287`) and Preferences overlay 203. Walk,
Look and Attack store 1, 2 and 4 in the pointer mode `DS:1440` through `28C9:2535`
(FND-INPUT-013) and close the menu. Collapse party does nothing in combat; otherwise it calls
the routine that key 6 calls when the party is shown and the one key 5 calls when it is collapsed
(FND-EXPLORE-005), and sets the button's frame from the new state. Center on leader closes the
menu and centres the view on `leader` through `2C5F:018C` (FND-COMBAT-049). Return to game closes
the menu.

## Alternatives

Overlay 182 `+1F1D` and overlay 203 `+082D` were not read here; that they are key 6's routine
and a button-frame setter rests on FND-EXPLORE-005 and on the arguments. What `28C9:2A44`,
`28C9:2A81`, `28C9:31D2` and `2C5F:0182` do was not read. Whether the menu draws a description
of the pointed-at button was not settled: the opener registers overlay 190 `+363B` and per-control
callbacks at overlay 181 `+0648`, which were not read past their first calls.

## How to reproduce

`overlay_listing.py <dsun> 181 67F43 68340`, `... 181 68330 68370` and `... 181 684C2 684F8`; `trampoline_target.py
<dsun> 571F:009D 56B9:002F`.

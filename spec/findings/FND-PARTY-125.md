---
id: FND-PARTY-125
title: The main view's tick 28C9:2322 does nothing while DS:0D9C is 0, which overlay 182 +1856 stores on entering display mode 2, so its fight step does not run while a window opened that way is up
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2322..28C9:24EA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:1EDB..28C9:2322
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0353..28C9:050F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0C43..28C9:0C80
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:0283..28C9:02B4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070D3A..0x00070DD1
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/direct_callers.py, resident_listing.py, overlay_listing.py); the routine rows of coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-PARTY-124 lists the stores to the view-mode
word `DS:0DAB` and to `DS:0D9C`: overlay 182 `+1856` stores 0 in `DS:0D9C` and then 2 in
`DS:0DAB`; `DS:0D9C` is set to 1 only in overlay 182 `+1167`, `+15D7` and `+19F8`.

**The tick.** `28C9:2322` (row `28C9:2322..28C9:24EA` of the inventory) returns at `28C9:2368`
when the word at `DS:0D9C` is 0, before any of its tests of the fight state or its calls of
`28C9:050F` and `28C9:1EDB`. Past that test it calls the getter of `DS:0DAB` (`28C9:2522`) and
calls `28C9:050F` with the mouse position only when the getter returns 1 (`28C9:23CC..28C9:23EF`).
It calls `28C9:1EDB` at `28C9:2429` when the word at `DS:1440` is 1, and again from `28C9:24AE`
while the fight state word `4C10:0019` is not 0 (`28C9:23F6..28C9:24B1`). `direct_callers.py
<dsun> 28C9:1EDB` finds only that near call. `28C9:1EDB` calls the fight routine through
trampoline `5671:0039` at `28C9:2153` (FND-PARTY-089) and holds no instruction that addresses
`DS:0DAB` or calls the getter.

**The other fight step.** `28C9:305F`, which calls `5671:0039` at `28C9:3141` (FND-PARTY-089), is
called near from `28C9:02B1`, after a search of the 0x30 placed objects for one under a map point
(`28C9:0283..28C9:02B4`), from `28C9:0428` in `28C9:0353`, and from `28C9:0C7B`, after a call of
`2D40:35EF` with the object at `DS:426D` (`28C9:0C43..28C9:0C80`), and the load image holds one
far reference to it (`30F0:0005` as a paragraph). None of the three near callers' code before the
call addresses `DS:0DAB` or `DS:0D9C`.

**Region loads.** `direct_callers.py <dsun> 182+15D7` finds four far callers, all in overlay 187
(`+0F21`, `+0F9C`, `+1169` and `+12D0`), which follow the opening of `darkrun.GFF` and the call of
overlay 182 `+1856` at `+0F0A` (`+0F0A..+0FA1`).

## Interpretation

While a window that overlay 182 `+1856` opened is up, `DS:0D9C` stays 0 unless `+1167`, `+15D7` or
`+19F8` runs, and the main view's tick then runs no fight step, so a fight's end, and the level
gain overlay 173 `+07AD` runs there, does not come from the tick in display mode 2. Overlay 182
`+15D7` belongs to the region load of overlay 187, which sets mode 4 and then 1.

## Alternatives

- The fight step `28C9:305F` is reached from map click and object routines whose own callers were
  not read for a test of `DS:0D9C` or `DS:0DAB`; if one runs while a mode 2 window is up, a fight
  can end in that mode.
- Overlay 182 `+1167` stores 1 in `DS:0D9C` before a gate at `+12DD` and stores 1 in `DS:0DAB`
  only after it, so a return at the gate could leave `DS:0D9C` at 1 with `DS:0DAB` at 2; when
  `+1167` runs was not read.
- The experience award of overlay 188 `+16E9`, which also runs a level gain, is reached from
  script opcode `0x21` and from kills; the display mode on those paths was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `direct_callers.py <dsun> 28C9:1EDB 28C9:305F
28C9:2522 182+15D7`; `resident_listing.py <dsun> 28C9:2322..28C9:24EA 28C9:1EDB..28C9:2322
28C9:0353..28C9:050F 28C9:0283..28C9:02B4 28C9:0C43..28C9:0C80`; and `overlay_listing.py <dsun>
187 70D3A 70DD1`.

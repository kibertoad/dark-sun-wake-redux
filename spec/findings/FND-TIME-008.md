---
id: FND-TIME-008
title: The timer library at 4842 runs its slots from its own interrupt 8 handler through per-slot accumulators; during play the slots are the BIOS chain at 54,925 µs, the music driver at 120 Hz, a 100 Hz sound counter and the 96 Hz game tick, with 1,000 µs added while an FLI plays
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:040C..4842:06CF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:077F..4842:0A23
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0B4D..4842:0C5E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4734:07FD..4734:082C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:09A9..4654:0A96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000894A9..0x000894DB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00083384..0x000833C2
tool: Capstone 5.0.9 over resident and overlay code; tools/research/exec-census/resident_listing.py, overlay_listing.py, direct_callers.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static, load image at segment 1000, `DS` = `57E0`. Every far call into the timer
library names segment `4842`, so its `CS`-relative words are at `4842:x`. FND-TIME-007 gives the
same code as segment `4868`, where `4868:x` is `4842:(x + 0x260)`: its `4868:03DA` is `4842:063A`,
`4868:05EA` is `4842:084A`, `4868:033C` is `4842:059C` and `4868:0365` is `4842:05C5`.

**Slot tables**, 17 slots, 0 to 16: state words at `4842:006C + 2 × slot` (0 free, 1 allocated or
stopped, 2 running), callbacks at `4842:0006 + 4 × slot`, the caller's `DS` at `4842:004A + 2 ×
slot`, accumulators at `4842:008E + 4 × slot` and periods at `4842:00D2 + 4 × slot`. The count of
allocated slots is at `4842:0002`, the current period at `4842:0116`.

**`4842:040C`**, the interrupt 8 handler, adds 1 to the nesting count `4842:0004`; only at 1 does it
switch to its own stack and scan slots 0 to 16 with `4842:011E` as the index. For each slot in state
2 it loads the slot's `DS`, adds the current period to the accumulator, and when the sum is below
the slot's period stores it; otherwise it stores the sum less the period and calls the callback.
It then sends the end of interrupt (`0x20` to port `0x20`) and, when the bytes at `4842:01B2` still
read `Test`, takes 1 off the nesting count and returns; otherwise it loops on `int 3`.

**`4842:063A`** (`choose_timer_period`) stores `0xFFFFFFFF` at `4842:0120`, lowers it to the
smallest period of the slots whose state word is not 0, and, when that differs from `4842:0116`,
stores `0xFFFF` at `4842:011E`, stores it at `4842:0116`, calls `4842:05C5` with its low word and
clears the 34 words at `4842:008E` (`4842:06B7..4842:06C2`). Its one caller is `4842:0978`, in the
period setter `4842:093D(slot, µs)`, which sets the slot's accumulator to 0. `4842:0992(slot, hz)`
divides 1,000,000 by `hz` and calls `4842:093D`. `immediate_search.py <dsun> 011E 0120 0122 0116
0118` finds no other instruction that uses `4842:011E` or `4842:0120`; `4842:056C`, which restores
the old vector, also stores `0xFFFF` at `4842:011E`.

**`4842:077F(callback)`** takes the first free slot of 0 to 15 (`0xFFFF` when none) in state 1 with
period `0xFFFFFFFF`. On the first allocation it clears the tables, installs the handler
(`4842:0520`, which keeps the old vector at `4842:011A`), and gives slot 16 the callback
`4842:0519`, which calls the old vector, with 54,925 µs, started. `4842:08B1` starts a slot (state
2), `4842:08F7` stops it, `4842:084A` releases it; the last release writes reload 0 and restores
the vector, and a release does not call `4842:063A`. `direct_callers.py` finds five callers of
`4842:077F`:

| Caller | Callback | Period | Callback does |
| --- | --- | --- | --- |
| `4734:07FD`, called at `4734:039D` after the music driver's install at `4734:038D` | `4654:09A9` | 10,000 µs | adds 10 to `4E71:0C15` and 1 to `4E71:0C19`, `0C1B`, `0C1D`, `0C20`, `0C23` |
| overlay 200 `+034F` (`+02BE`, start-up) | `31E0:31A7` | 96 Hz, 10,416 µs | stores 1 at `DS:265A`, adds 1 to the dword `DS:145E`, sets `DS:1424` when its low 3 bits are 0, and when its low 6 bits are 0, `DS:1440` is 1, `4C10:0019` is 0 and `DS:0DAB` is not 2 adds 1 to the dword `4C13:035B` |
| `4842:0BBF`, in the driver install `4842:0B79` | the driver's function `0x67` | word `+0x14` of the descriptor its function `0x64` returns, in Hz; no slot when that word is `0xFFFF` or there is no function `0x67` | the driver's own routine |
| overlay 196 `+041D`, from the FLI player (FND-VIDEO-008) | overlay 196 `+040B` | 1,000 Hz, 1,000 µs | adds 1 to `DS:21E0` |
| `4842:1483`, in DIGPAK function `0x66` | none | never started | none |

In `RESOURCE.GFF` the descriptor word `+0x14` is 120 in the `ADV` drivers that have function
`0x67`, `SBP2FM.ADV` among them, and `0xFFFF` in those without, `SBPDIG.ADV` among them.

## Interpretation

The library runs the timer interrupt at the shortest requested period and calls each slot at its
own average rate, since each accumulator grows by the interrupt's period. With the installed
`SOUND.CFG` (FND-EXE-590) the interrupt runs at 120 Hz during play, and the game's tick
`DS:145E` advances 96 times a second. While an FLI plays the interrupt runs at 1,000 Hz, and since a
release does not choose again it stays there until the next period change. `4842:011E` is the
handler's slot index; setting it to `0xFFFF` during a callback makes the scan start again from slot
0. `4842:0120` is the chooser's working minimum.

## Alternatives

The drivers' function `0x67` was not read; that word `+0x14` is a rate in Hz rests on how
`4842:0B79` passes it to `4842:0992`. The rates are computed from the code, not observed;
FND-RNG-077's run, which saw `4C13:035B` step every 667 ms (64 ticks of 96 Hz), agrees. The start-up
order of the music driver, overlay 200 and the intro FLI was not traced.

## How to reproduce

`resident_listing.py <dsun>` over the resident ranges above; `overlay_listing.py <dsun> 200 894A9
894DB` and `... 196 83384 833C2`; `direct_callers.py <dsun> 4842:077F 4842:063A 4842:093D
4842:0992`; `immediate_search.py <dsun> 011E 0120 0122 0116 0118`. The `ADV` resources of
`RESOURCE.GFF` at the offsets FND-EXE-590 gives, read at `+0x2D` for the function table.

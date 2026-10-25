---
id: FND-COMBAT-061
title: In the repeated passes 2D40:0BA4 clears slot record 21 and twenty other walk records after each end of combat, through 28C9:1E41 and 2D40:197A(9999), and overlay 173 +0C65 draws no random number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/pass_counts.py with the configs counts/qc034-probe.json, qc034-a.json, qc034-b.json, qc034-c.json and qc034-d.json, and pass_bins.py, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Each run restores `play-ready`, moves the pointer to (160, 199) at 500 ms and to (160, 100) at
2,500 ms, and clicks the left button at (120, 118) at 3,000 ms, as FND-COMBAT-057's runs do.
Times are emulated ms after the restore. Run addresses are given here as the repository's: a
resident segment `S` runs at `S - 0A46`; overlay code was located from the far jump the overlay
manager writes into the trampoline of `173 +059A` at each pass, and each overlay return was
matched against the file's bytes before it. Overlay 173 was loaded at a different segment from
pass to pass. Each config sets breakpoints on the sites it names and a write watchpoint on the
word `4F49:0A32`, word `+0` of slot record 21, stopping at most 50 times at each site.

| Run | Counting window (ms) | Sites |
|---|---|---|
| `qc034-probe-20261025` | 3,300 to 3,600 | the `173 +059A` trampoline |
| `qc034-a-20261025` | 3,290 to 3,700 | `173 +059A`, `197 +29E0`, `2D40:03C1`, `31E0:05BB`, `1000:0822`, `371D:0334`, `173 +0C65`, `+0E99`, `+0EDB` |
| `qc034-b-20261025` | 4,480 to 4,620 | as a, with `2D40:197A`, `2D40:2001` and `2D40:0BA4` |
| `qc034-c-20261025` | 5,000 to 5,700 | as b, without `197 +29E0` |
| `qc034-d-20261025` | 3,020 to 3,340 | as b, the first pass after the click |

Every complete pass shows the same order inside one call of `28C9:1EDB`:

1. `173 +059A(0, &DS:426D)` from `28C9:2158`; `173 +0C65` is entered from `+0720` and calls
   `371D:0334`; `197 +29E0` runs from overlay 195 `+1508` for 34 slots (1, 3, 15 to 18, 20 to
   47); overlay 188 `+1820` calls `2D40:3D4C(0, 1)`, which changes `4F49:0A32` from 0 to 1 at
   `2D40:3DA0` (return `188 +1870`).
2. After `173 +059A` returns, `28C9:1E41` (return `28C9:21E6`) calls `2D40:197A(9999, 0xFFE2)`
   (return `28C9:1E51`), which calls `2D40:2001(slot)` (return `2D40:1D0D`) for 35 slots (1, 3,
   15 to 47), which calls `2D40:0BA4(slot)` (return `2D40:207B`) for 21 of them (21, 23, 26,
   27, 30, 32 to 47). For slot 21 it calls `1000:3FA2(4F49:0A32, 0, 0x13)` (return `2D40:0C1F`),
   which changes the word from 1 to 0 at `1000:3F9C` and leaves `ES:DI` at `4F49:0A44`.

Over run c's whole length the watchpoint records 73 changes: one 1 to 0 at `1000:3F6F` at 3,313
ms (in `1000:3F5A`, return `188 +1852`, on the first pass after the click, which starts at 3,249
ms), then 36 changes 0 to 1 at `2D40:3DA0` and 36 changes 1 to 0 at `1000:3F9C`, 54 to 68 ms
after each pass start. Passes start 57 to 71 ms apart.

`2D40:03C1` is called once, with 9999, from `2D40:31E9` at 3,056 ms, and in no pass.
`31E0:05BB` is not called. No breakpoint at `173 +0E99` or `+0EDB` is hit in any pass where the
overlay 173 breakpoints were placed (all passes but the first of run a), and `1000:0822` is not
called in any pass; the only draws in the windows are two at 3,316 and 3,317 ms returning to
`172C:1710`, script instruction `0x52`, between `2D40:2001(16)` and `2D40:2001(17)` in run d.

The cap cut the counts of `197 +29E0` (runs a and b), `2D40:2001` and `2D40:0BA4` (runs b and c)
in some passes. In run d the first pass's `2D40:2001` calls run for slots 16 to 47.

## Interpretation

The walk record of slot 21, and those of the other 20 slots FND-COMBAT-056 lists, are cleared on
each pass by `2D40:0BA4` called through `28C9:1E41` with slot 9999, after the end of combat has
set word `+0` of record 21 again. The round step takes the branch without initiative draws, so the
repeated passes use no random numbers of `1000:0822`. The slot release `2D40:03C1` that
FND-COMBAT-060 named as a candidate does not run.

## Alternatives

What `2D40:197A`, `2D40:2001` and `2D40:0BA4` do apart from this fill, why `2D40:2001` calls
`2D40:0BA4` for 21 of its 35 slots, and why the first pass starts at slot 16, were not read.
`DS:426D` and `DS:0CE6` were not read in these runs; the bug state rests on the passes of
`173 +059A` with `&DS:426D` and on the writes matching FND-COMBAT-057's.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoint above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/pass_counts.py
tools/research/rust-dos-runs/counts/qc034-<x>.json` for each config, then `pass_bins.py
<run>/log.json`. Each run writes `log.json` under `D:/rust-dos-games/work/qc034/<name>`.

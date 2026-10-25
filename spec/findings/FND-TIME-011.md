---
id: FND-TIME-011
title: In play after the opening the timer library runs channel 0 at 1,000 µs with no music driver slot, and wait_ms waits about ms / 55: 91.25 ms for 5,000 and 0.23 to 0.27 ms for 1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walkruns-recon1.json, walkruns-recon2.json and walkruns-wait1.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint talk of run v175-talk-final-20261019, and checkpoint fight of run fightprep (D:/rust-dos-games/work/walkruns)
---

## Observation

Times are emulated ms after the restore. Run addresses are given as the repository's. Each run
breaks at `wait_ms` `1000:12FA`, reading its `ms` argument and `game_tick` (`DS:145E`), and at the
far return read from the stack; the exit time is the stop's emulated time. `DS:3904` reads 2,386
in every run.

**Timer state** (`wait1`, read at 0, 1,500 and 7,000 ms from `fight`, FND-EXPLORE-022): the
current period `4842:0116` is 1,000 µs. The running slots are 0 (callback `4654:09A9`, 10,000 µs),
1 (`31E0:31A7`, 10,416 µs) and 16 (`4842:0519`, 54,925 µs). No slot has the music driver's callback.

**`wait1`**: from `fight`, a left click at (184, 126), Thy'rokh's own cell. `wait_ms(5000)` is
entered at 748.986950 ms (tick 18774) from the call at file `0x59B69`, overlay 172's pause of
`DS:26B7 × 100` (FND-TIME-004; `DS:26B7` reads 50), and returns at 840.240616 ms (tick 18783):
91.25 ms and 9 ticks.

**`recon2`**: from `talk`, with the conversation ended by a click at (160, 40) at 500 ms, 60 calls
of `wait_ms(1)` between 3,553 and 3,582 ms, all from the call at file `0x8FD23` in overlay 206
(FND-TIME-004), take 232.0 to 270.7 µs each, mean 252.2 µs, 0 or 1 tick. `DS:1440` is 1 and
`4C10:0019` 0 then. `recon1` shows the same first 20 calls.

## Interpretation

During this play channel 0 runs with a reload of 1,193, 1,000 times a second, the rate FND-TIME-008
reads the FLI player setting and no later change undoing. With a reload that small every target
`last + 2386 × ms` passes `0x10000`, and each reload takes 65,536 off it, so the wait ends after
about `2386 × ms / 65536` half waves of 0.5 ms: 0.0182 ms per ms asked. That gives about 91 ms for
5,000, and the rest of the current half wave for 1, as measured. The 120 Hz interrupt
FND-TIME-008 expects from the installed `SOUND.CFG` did not run here.

## Alternatives

Why no music driver slot is allocated was not looked into; the driver install `4842:0B79` was not
watched. A wait while channel 0 runs at 120 Hz, where the same arithmetic gives about 0.152 ms per
ms asked, was not measured, nor the callers that ask for 400, 50, 10 and 16,000 ms.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoints above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walkruns-<x>.json` for `recon1`, `recon2`, `fightprep` (which
writes the checkpoint `fight`) and `wait1`. Each run writes `log.json` under
`D:/rust-dos-games/work/walkruns/<name>`.

---
id: FND-TIME-014
title: From program start to the start menu the installed launch clears DS:3444 twice, installs only the digital driver with no timer slot, and runs channel 0 at 10,000 µs until the first FLI and at 1,000 µs from then on, with no wait_ms call
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/start_timer.py with the configs counts/qt005-t1.json, qt005-t2.json and qt005-t3.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, HDMA 5, rust-dos's default OPL3, no Ultrasound; default muted host audio; C: a manifest-checked copy of the installation from probe_drive.py, D: game.ins; started at program entry
---

## Observation

Times are emulated ms after program entry. Run addresses are given as the repository's. Each run
breaks at the sound library's set-up and driver routines (`4734:0128`, `0133`, `0293`, `039B`,
`03A6`, `49E9:018F`) and the timer library's (`4842:0A23`, `0B73`, `0B79`, `0B9F`, `0BA8`, `0BE6`,
`077F`, `093D`, `0992`, `08B1`, `08F7`, `084A`, `0C2B`), and at `wait_ms` `1000:12FA`, and reads the
slot tables at `4842:0000..0124` at each stop and at fixed times.

**`t1`**, `DSUN.EXE -W0 -L` with the installed files, 15,000 ms, no input:

| ms | Event |
| --- | --- |
| 23.320 | `4734:0128(0)` from overlay 180 `+0091` |
| 23.349 | `4734:0133` entered; `SOUND.CFG` word `0x14` `0x000B`, `DS:14E3` 1, `DS:3444` 0 |
| 23.355 | `4734:0128(0)` from `4734:015E` |
| 24.135 | `4734:0293` with `DS:3444` 0 and `DS:343E` `0xFFFF` |
| 24.139 | `4734:039B`, with no call of `4842:0A23`, `0B4D`, `0B73` or `0B79` before it |
| 24.140 to 24.197 | the first allocation: slot 16 (`4842:0519`, 54,925 µs) started; slot 0 (`4654:09A9`) at 10,000 µs; period 10,000 µs |
| 24.413 to 254.239 | `4842:0B73` with `0x220, 5, 1, 1` for the digital driver returns 1 |
| 254.239 | `4842:0B79(0, 0x220, 5, 1, 1)` from `49E9:0229` |
| 254.245 | `4842:0B9F`: descriptor word `+0x14` `0xFFFF`; `4842:0BE6` follows, `0BA8` and `0BBF` are not reached |
| 689.872 to 689.915 | slot 1, `31E0:31A7`, `0992(1, 96)`, `093D(1, 10,416)`; period stays 10,000 µs |
| 765.504 to 765.556 | slot 2, overlay 196 `+040B`, `0992(2, 1,000)`, `093D(2, 1,000)`; period 1,000 µs |
| 1,000 to 15,000 | period 1,000 µs; slots 0, 1, 2 and 16 running |

**`t2`**, the same launch to 185,000 ms with Esc at 20,000 ms and every 10,000 ms to 130,000 ms:
the same events to 765.556 ms; at 129,173.906 ms overlay 196 releases slot 2 through `4842:084A`
with no call of `4842:093D`; from 130,000 ms to the end the period is 1,000 µs and the running
slots are 0 (10,000 µs), 1 (10,416 µs) and 16 (54,925 µs). The screen at 185,000 ms is the start
menu. `game_tick` reads 17,697. Neither run calls `wait_ms`, `4842:0C2B` or `4842:0B79` again.

**`t3`**, `DSUN.EXE -W0` with the run's own copy of `SOUND.CFG` given word `0x14` = 1: no breakpoint
is hit; the emulator's log shows the open of `C:\RESFLOP.GFF` failing and the program ending with
exit code 1.

## Interpretation

The installed launch never installs the FM music driver, as FND-TIME-013 reads, and the digital
driver asks for no timer slot. Channel 0 runs at 10,000 µs from the first allocation to the intro
FLI and at 1,000 µs from then on, through the start menu and into play, since releasing the FLI
slot does not choose a new period. So every `wait_ms` call of the installed game runs at 1,000 µs,
the rate FND-TIME-011 measured. A launch without `-L` ends on the missing `RESFLOP.GFF`
(FND-CONFIG-062), so the installed files cannot reach the music driver install by another launch.

## Alternatives

A second FLI during play allocates a 1,000 Hz slot again and chooses 1,000 µs; it was not
observed. `wait_ms` at 120 Hz could not be measured, since no state of the installed game has that
period.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/start_timer.py
tools/research/rust-dos-runs/counts/qt005-<x>.json` for `t1`, `t2` and `t3`. Each run writes
`log.json` under `D:/rust-dos-games/work/qt005/<name>`.

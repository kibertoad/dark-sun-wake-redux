---
id: FND-EXPLORE-024
title: After the opening talk a scripted scene holds input until about 3.7 s, so a Walk click is taken late and a key is not read; computer control on all four members wins the first fight by 55 s, and outside a fight a leader key puts the new leader on the old leader's point
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walk2-r1.json, walk2-r2.json, walk2-f1.json, walk2-f2.json, walk2-f3.json, walk2-explore.json, walk2-vcheck.json and walk2-k2.json, at the commit that adds this finding; Python 3.14.7; Capstone 5.0.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint talk of run v175-talk-final-20261019, checkpoint fight of run fightprep (D:/rust-dos-games/work/walkruns), and the checkpoints won of run f3 and explore of run exprep (D:/rust-dos-games/work/walk2)
---

## Observation

Times are emulated ms after the restore; ticks are the dword `DS:145E`; mouse positions are the
game's 320 by 200 pixels. Run addresses are given as the repository's (a resident segment `S` runs
at `S - 0A46`). Settings as FND-EXPLORE-022.

**`r1`**, from `talk`: a left click at (160, 40) at 500 ms ends the conversation, and key Up is sent
at 1,200 ms. From 1,100 to 6,000 ms the key routine `28C9:0CFF` is never entered, and neither
`28C9:01CB` nor `28C9:0353` runs; slot 0 stays at (1192, 1496). `DS:1440` is 1 from 1,000 ms;
`4C10:0019` goes 0, 1 (4,500 ms), 2 (6,000 ms). Other walkers move from 2,568 ms, and slots 1 to 3
are placed from `28C9:2E07` at 4,221 ms. The screen shows a figure appearing in the street by
3,500 ms.

**`r2`**, from `talk`: the same first click, then a left click at (168, 158) at 2,600 ms
(delivered at 2,900 ms). `28C9:0353(0, 168, 158)` runs from `28C9:06C5` at 3,699 ms and
`2D40:10AE(0, 1, 2)` from `28C9:04C8` at 3,700 ms; slot 0's word `+0x0C` goes from 1496 to 1481
by 4,000 ms.

**`f1`, `f2`, `f3`**, from `fight`: key `v` opens View Character. Left clicks on the four
computer-control buttons at about (47, 52), (98, 52), (47, 112) and (98, 112), then Return to game
at (267, 172). With no further input, `4C10:0019` goes 2 to 3 and back eleven times at `28C9:2164`
(5,100 to 50,625 ms). At 55,000 ms the message box reads that each party member receives 10000
experience points, and the screen stays the same to 120,000 ms with `4C10:0019` 2 and `DS:1440` 0.
In `f3` (checkpoint `won` at 55,000 ms) a left click at 56,000 ms sets `DS:1440` to 1 at 56,888 ms
(`28C9:2519`) and `4C10:0019` to 0 at 56,910 ms (`28C9:2164`). Slots 1 to 3 then hold (2112, 1632)
with byte `+0` 0x98, as at `talk`.

**`exprep`**, from `won`: the click at 1,000 ms, Space at 3,000 ms (`28C9:0CFF` entered at
3,004 ms), checkpoint `explore` at 5,000 ms: `4C10:0019` 0, `DS:1440` 1, `4C13:0369` 0, `DS:426D`
0, slot 0 at (1208, 1512), `4E71:0000` 1. The walk sets, from the cache records at `DS:6577 + 9 x
set`: slot 0 SCMD 599, slot 1 SCMD 25, slot 2 SCMD 3917, slot 3 SCMD 598. **`vcheck`** opens View
Character from `explore`: the four computer-control buttons' pixels match `f1` before any toggle
and differ from `f2` after all four were turned on.

**`k2`**, from `explore`: key `2` at 500 ms (`28C9:0CFF` at 503 ms). At 1,500 ms `4C13:0369` and
`DS:426D` are 1, slot 1 holds (1208, 1512) with byte `+0` 0x18, and slot 0 holds (2112, 1632) with
0x98.

## Interpretation

Between the end of the opening conversation and the first fight the game plays a scripted scene
that does not run the key routine; a mouse click made during it is kept and acted on when the
scene ends, about 3.7 s after the restore, and the fight then cuts the walk short. Computer
control set on all four members fights the first fight to its end without player input. Outside a
fight, choosing a leader by key moves the new leader onto the old leader's position and hides the
old leader, as the leader slot's byte `+0` and position show.

## Alternatives

- Whether walk1's click at 1,000 ms (FND-EXPLORE-022) would have been taken after 3,400 ms was not
  run.
- The computer-control setting after Space was seen only through the buttons' pixels; the
  `combatants` byte was not read back.
- Keys sent during the scene were not followed past 6,000 ms, so whether they are dropped or kept
  is open.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoints above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walk2-<name>.json` for each config named in `tool`, in the
order given (`walk2-f3.json` writes `won`, `walk2-explore.json` writes `explore`). Each run writes
`log.json` under `D:/rust-dos-games/work/walk2/<name>`.

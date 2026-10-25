---
id: FND-EXPLORE-025
title: Outside a fight the leader walks one frame per main-loop pass; a cell takes 4 frame calls north and 6 east for SCMD 599 and 25 (57 to 71 ms and 143 to 200 ms) and 2 north and 3 east for SCMD 598 (43 to 57 ms and 59 to 84 ms) at 15,000 cycles per ms
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walk2-n0.json, walk2-e0.json, walk2-n1.json, walk2-e1.json, walk2-n3.json, walk2-e3.json and walk2-w0.json, at the commit that adds this finding; Python 3.14.7; Capstone 5.0.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint explore of run exprep (D:/rust-dos-games/work/walk2, FND-EXPLORE-024)
---

## Observation

Times are emulated ms after restoring `explore`; ticks are the dword `DS:145E` read at each stop.
Run addresses are given as the repository's. Each run watches the walker's words `+0x0A` and
`+0x0C`, breaks on `31E0:1B60` only when its record argument is the walker's (`DS:67BB + 37 x
slot`), on `2D40:10AE` for the walker's slot, on `28C9:0353`, `28C9:01CB`, `28C9:04EF` and
`28C9:0CFF`, and on `31E0:2BB9`.

| Run | Walker | Inputs |
| --- | --- | --- |
| `n0` | slot 0, SCMD 599 | Walk click (216, 112) at 1,500 ms; Up at 4,000; Right at 5,500 |
| `e0` | slot 0, SCMD 599 | Walk click (264, 160) at 1,500 ms |
| `n1`, `e1` | slot 1, SCMD 25 | key `2` at 300 ms, then as `n0`, `e0` |
| `n3`, `e3` | slot 3, SCMD 598 | key `4` at 300 ms, then as `n0`, `e0` |

Click (216, 112) names a point three cells north of the leader, and (264, 160) three cells east.
Each click reaches `28C9:0353(slot, x, y)` from `28C9:06C5` and `2D40:10AE(slot, 1, 2)` from
`28C9:04C8` within 1 ms; no run reaches `28C9:04EF`. Up and Right reach `28C9:01CB` from
`28C9:1126` and `2D40:10AE(slot, 1, ...)` from `28C9:032A`.

A cell runs from the walk's start, or the previous pop (`2D40:10AE(slot, 0)` from `31E0:2DB2`), to
the next pop. Every `31E0:1B60` call for the walker's record in a cell moves it:

| Set | Direction | Calls per cell | Steps per call | Click walk: cell 1, 2, 3 (ms) | Key step (ms) |
| --- | --- | --- | --- | --- | --- |
| 599 (`n0`, `e0`) | north | 4 | (0,-7) (0,-1) (0,-7) (0,-1) | 198.43, 71.44, 57.08 | 114.54 |
| 599 | east | 6 | (4,0) (3,0) (1,0) twice | 355.20, 199.76, 142.68 | 243.16 |
| 25 (`n1`, `e1`) | north | 4 | as 599 | 202.00, 71.42, 57.09 | 86.94 |
| 25 | east | 6 | as 599 | 344.50, 142.68, 142.68 | 114.64 |
| 598 (`n3`, `e3`) | north | 2 | (0,-8) twice | 187.07, 57.15, 42.81 | 59.51 |
| 598 | east | 3 | (7,0) (7,0) (2,0) | 299.93, 58.62, 84.04 | 115.35 |

In ticks, cells 2 and 3 took 5 to 7 north and 13 to 19 east for 599 and 25, and 4 to 5 north and
6 to 8 east for 598. The first frame call of a click walk comes 128 to 213 ms after the click, with
no `31E0:2BB9` entry between. After a walk's last pop one more call for the walker's record, 1.9 to
62.3 ms later, sets a standing frame without moving it: byte `+0` reads 0x39 at every moving call
and 0x19 at that one. Byte `+0x15` reads 0 at every call. The moving frames start at frame 1 north
and frame 18 east.

`31E0:2BB9` is entered about every 1.7 ms while nothing walks, and during a walk once per frame
call: 12 entries for 12 calls in `n0` and `n1`, 18 for 18 in `e0` and `e1`, 6 for 6 in `n3`; in
`e3`, 13 entries for 9 calls, four entries at one tick applying no frame. No two calls for a walker
fall on the same tick: consecutive calls in a cell were 1 to 4 ticks apart (9.7 to 143.5 ms), with
one gap of 8 ticks (83 ms, `e0` cell 2) and one of 14 (143 ms, `n0`'s Right step). `w0`, the same
north walk as `n0` started at 500 ms, gives cells of 200.9, 71.4 and 71.3 ms.

## Interpretation

Outside a fight, as in the first fight (FND-EXPLORE-022), a walker gets one frame call per pass of
the map loop and the tick gate never holds a second call: the pass, 10 to 35 ms here against 44 to
108 ms in the fight, sets the walking speed at this emulated CPU speed. Sets 599 and 25 play the
four-frame north and six-frame east sequences that FND-EXPLORE-020 finds most common; set 598
plays two frames north and three east. A cell's time follows from the number of frames times the
pass time, plus a first-frame delay of 128 to 213 ms for a click walk.

## Alternatives

- Only the leader walked, north and east, three cells each, from one start cell; followers and
  slot 2 (SCMD 3917) were not walked outside a fight. The runs differ only in the walker, so their
  pass lengths are not independent samples.
- Other CPU speeds were not tried. What makes the first-frame delay and the two long passes was not
  read.
- Computer control after the fight rests on the Space press (FND-EXPLORE-024).

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoint above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walk2-n0.json`, and the same for each config named in `tool`.
Each run writes `log.json` under `D:/rust-dos-games/work/walk2/<name>`.

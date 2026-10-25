---
id: FND-EXPLORE-022
title: In the first fight Thy'rokh (slot 2, SCMD 3917) walks one cell north in two frame calls of 31E0:1B60, one per main-loop pass, taking 84 to 154 ms (8 to 15 ticks of DS:145E) at 15,000 cycles per ms
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walkruns-recon1.json, walkruns-recon2.json, walkruns-walk1.json, walkruns-fightprep.json, walkruns-walk2.json, walkruns-walk3b.json and walkruns-walk4.json, at the commit that adds this finding; Python 3.14.7; Capstone 5.0.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint talk of run v175-talk-final-20261019, and checkpoint fight of run fightprep (D:/rust-dos-games/work/walkruns)
---

## Observation

Times are emulated ms after the restore; ticks are the dword `DS:145E`; mouse positions are the
game's 320 by 200 pixels. Run addresses are given as the repository's (a resident segment `S` runs
at `S - 0A46`).

**Starting state.** From `talk` (the opening conversation of a new game), a left click at
(160, 40) at 500 ms ends the conversation. By 2,500 ms `DS:1440` is 1, the leader `4C13:0369` is 0
and slot 0's record holds the point (1192, 1496). A fight starts between 3,000 and 6,000 ms: at
6,000 ms the combat word `4C10:0019` is 2 and the leader and `DS:426D` are 2 (Thy'rokh). In run
`walk1` a Walk click at (168, 158) at 1,000 ms, before the fight, is not acted on: `2D40:10AE` is
not entered for slot 0 from 900 to 3,400 ms and the screen does not change. Run `fightprep` saves
the checkpoint `fight` at 7,000 ms, with `DS:1408` and `DS:140A` 992 and 1352 and slot 2 at the
point (1176, 1480), cell (73, 92). Runs `walk2`, `walk3b` and `walk4` restore `fight`.

**The walker's set.** Slot 2's word `+0x16` is -3917 at `talk` and 52 at `fight`. Cache record 52
at `DS:6577 + 9 × 52` reads `01 4D 0F 04 00 61 91 3D 00`, word `+1` 3917 and word `+7` 61.
`OBJEX.GFF` SCMD 3917's north frames each move (0, -8). At `talk` slots 0, 1 and 3 hold -599,
-25 and -598.

**The walk.** A left click at (184, 80) at 500 ms chooses the point (1176, 1432), cell (73, 89),
three cells north. Watchpoints on slot 2's words `+0x0A` and `+0x0C` (`DS:680F`, `DS:6811`);
breakpoints on `31E0:1B60` counted when its record argument is `0x6805` (`DS:67BB + 37 × 2`), on
`2D40:10AE` and on `28C9:0353`. The three runs give the same times to the ns.

| Event | ms | Tick | Word `+0x0C` after |
| --- | --- | --- | --- |
| `28C9:0353(2, 0xB8, 0x50)` from `28C9:06C5` | 558.95 | 18756 | 1480 |
| `2D40:10AE(2, 1)` from `28C9:04DA` | 559.75 | 18756 | |
| `31E0:1B60(0x6805)`, store at `31E0:1DA9` | 705.12 | 18770 | 1472 |
| `31E0:1B60(0x6805)` | 760.57 | 18775 | 1464 |
| `2D40:10AE(2, 0)` from `31E0:2DB2`, cell (73, 91) | 791.47 | 18778 | |
| `31E0:1B60(0x6805)` | 804.98 | 18780 | 1456 |
| `31E0:1B60(0x6805)` | 860.69 | 18785 | 1448 |
| `2D40:10AE(2, 0)`, cell (73, 90) | 891.37 | 18788 | |
| `31E0:1B60(0x6805)` | 968.88 | 18795 | 1440 |
| `31E0:1B60(0x6805)` | 1,017.94 | 18800 | 1432 |
| `2D40:10AE(2, 0)`, arrival at (73, 89) | 1,045.62 | 18803 | |

Word `+0x0A` stays 1176. Before the click `31E0:2BB9` is entered about every 0.84 ms, 11 or 12
times per tick. From the click to 1,091 ms it is entered only at 705, 760, 804, 860, 968, 1,017,
1,051 and 1,091 ms, each time calling `31E0:1B60` for slot 2 while it is between cells: the first
four through `2C5F:02CB` from `28C9:24CE`, the later ones through `362C:0BC5` from `28C9:2303`.

**Key steps.** In `walk3b`, after the walk, grey Up at 2,500 ms reaches `28C9:01CB(2, 0x49, 0x58)`
at 2,503.61 ms (tick 18943), with frame calls at 2,530.37 and 2,558.90 ms and the pop at 2,587.52
ms (tick 18951). Keypad Up with no character at 5,500 ms starts at 5,503.72 ms (tick 19231) and
ends at 5,598.97 ms (tick 19240). In `walk4` Space at 2,500 ms leaves `DS:426D` at 2.

## Interpretation

Each cell of this walk is two passes of the frame scan, one 8-pixel frame each, as FND-EXPLORE-019
reads. Pop to pop a cell took 99.9 ms (10 ticks) and 154.3 ms (15 ticks); the first cell, from the
click, 232.5 ms (22 ticks), of which 145 ms passed between the plan and the first frame. A key step
took 83.9 ms (8 ticks) and 95.3 ms (9 ticks). During the walk a main-loop pass took 44 to 108 ms,
far above the 10.4 ms tick, so the step time here is set by the pass time at this emulated CPU
speed, not by the tick gate.

## Alternatives

A walk outside a fight was not measured, since the Walk click before the fight was not taken, and
why was not read. Only slot 2 walking north was measured; the sets of the other members (SCMD 599,
25 and 598) and other directions were not walked. The pass time on any other CPU speed was not
measured.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoint above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walkruns-<x>.json` for `recon1`, `recon2`, `walk1`,
`fightprep` (which writes the checkpoint `fight`), then `walk2`, `walk3b` and `walk4`. Each run
writes `log.json` under `D:/rust-dos-games/work/walkruns/<name>`.

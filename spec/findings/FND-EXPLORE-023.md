---
id: FND-EXPLORE-023
title: Over a Walk click, a three-cell walk and a direction key step in the first fight, no instruction reads 4E71:0000; it is only written with 1 at 28C9:0500, 31E0:2D76 and 28C9:0344
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/walk_drive.py with the configs counts/walkruns-trace1.json and walkruns-trace2.json, at the commit that adds this finding; Python 3.14.7; Capstone 5.0.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; SB16 at 0x220, IRQ 5, DMA 1, no Ultrasound; default muted host audio; checkpoint fight of run fightprep (D:/rust-dos-games/work/walkruns)
---

## Observation

Times are emulated ms after restoring `fight` (FND-EXPLORE-022). Run addresses are given as the
repository's. rust-dos 1.7.5's watchpoints stop only when an instruction changes the watched
bytes, so the driver instead records the instruction trace in slices of at most 40 ms and checks
every traced instruction against the run's linear address of `4E71:0000`: each explicit memory
operand, decoded with Capstone from the traced registers (default `DS`, `SS` for a `BP` base,
segment prefixes honoured, read or write from Capstone's access flags), the source and destination
spans of string instructions with their `REP` count and direction flag, `XLAT`, and the buffers of
`INT 21h` functions `3Fh` and `40h`.

**`trace1`**: a left click at (184, 80) at 500 ms and the whole three-cell walk, traced from 450 to
1,200 ms (7,971,450 instructions), and grey Up at 2,500 ms, traced from 2,400 to 2,800 ms
(4,957,355 instructions). Instruction counts are contiguous within each window and no entry was
dropped. The byte reads 1 at 0, 1,200 and 2,800 ms. The only accesses are writes of 1 by
`mov byte ptr es:[0], 1`:

| Site | When |
| --- | --- |
| `28C9:0500` | once, during the click |
| `31E0:2D76` | once per cell, three times, and once at the end of the key step |
| `28C9:0344` | once, on Up |

**`trace2`** scans the same key step, 2,450 to 2,650 ms, for the two bytes of `DS:426D` and finds
1,633 accesses, all reads, among them `28C9:10EF`, `28C9:1107` and `28C9:111E` and a `repne scasb`
at `1000:407D`.

## Interpretation

In these windows `4E71:0000` is written at the three sites FND-EXPLORE-018 lists and read by no
instruction. `trace2` shows the scan finds reads, and the three stores show it finds this byte.

## Alternatives

Stack pushes and pops, DMA and the emulator's own services other than `INT 21h` functions `3Fh`
and `40h` were not scanned. Play outside a fight, other screens, a save and a load were not traced.

## How to reproduce

From the repository root, with `GAME_DIR` the installed BLD-GOG-EN-1.1 and the checkpoint above:
`artifacts/evidence-python/Scripts/python.exe -I tools/research/rust-dos-runs/walk_drive.py
tools/research/rust-dos-runs/counts/walkruns-trace1.json`, and the same with `walkruns-trace2.json`.
Each run writes `log.json` under `D:/rust-dos-games/work/walkruns/<name>`.

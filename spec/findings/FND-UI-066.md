---
id: FND-UI-066
title: With Preferences open, F4 turns music off and A turns animations off, the window stays open, and within a second the music and animation buttons show their grey form
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py and the plan prefs-f4.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Run `prefs-f4-20261025` opens Preferences from the Game Menu as FND-UI-058 does, then presses F4
at 200,500 ms and `a` at 206,000 ms. Times are the plan's absolute ones; the bytes are
`DS:1435..1438` in that order.

| Time (ms) | `DS:1431` | `DS:1435..1438` | Screenshot (MD5) |
|---|---|---|---|
| 200,500 | `81CF:0004` | 1, 1, 1, 0 | `7798a76854405c2810e74a1c3bd682ef`: all three toggle buttons coloured |
| 201,500 | `81CF:0004` | 1, 0, 1, 0 | `687d1a6c0d80a9351352e91822a2ed80`: the music button grey, the others coloured |
| 206,000 | `81CF:0004` | 1, 0, 1, 0 | `c30c2195524f22d9974e89fa56c3d33c`: as at 201,500 |
| 207,000 | `81CF:0004` | 1, 0, 0, 0 | `a9eb87fc4a21a0aa2b195bee4e2ee4a1`: the music and animation buttons grey |
| 212,000 | `81CF:0004` | 1, 0, 0, 0 | `94a7c57b690887b8fe7973709ef61fe8`: as at 207,000 |

In every screenshot the three bars and the difficulty name are as at 200,500 ms and the
description bar below the panel is empty.

## Interpretation

The keys change the settings as FND-CONFIG-218 reads, leave Preferences open, and the buttons
that show those settings are repainted. The dispatcher draws nothing after overlay 190 `+139B`
returns (FND-UI-065), so the repaint comes from another path; which one was not traced. No
message text appeared in the description bar at the times read.

## Alternatives

F5 and F6 were not pressed. Whether a message was shown and removed between the reads was not
seen.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/prefs-f4.json`.

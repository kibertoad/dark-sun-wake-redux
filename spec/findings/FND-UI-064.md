---
id: FND-UI-064
title: Esc closes the Preferences window opened from the Game Menu while DS:9DEE holds 0x0010
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py and the plan prefs-esc.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Run `prefs-esc-20261025` restores `play-ready`, presses key 2 and Tab and clicks the Game Menu's
Preferences button at (160, 101), as FND-UI-058 does. At 200,500 ms `DS:1431` holds `81CF:0004`,
`DS:9DEE` reads `0x0010`, and the screenshot (MD5 `7798a76854405c2810e74a1c3bd682ef`) shows the
Preferences panel. Esc is then pressed; at 202,500 ms `DS:1431` is null, `DS:9DEE` still reads
`0x0010`, and the screenshot (MD5 `fb93d1f73f4a08c2215991c63857047e`) shows the game view with
no window.

## Interpretation

Esc closes Preferences. With the value 2 clear in `DS:9DEE`, the event builder does not turn Esc
into a kind-1 event (FND-UI-065), so the close came through the key routine overlay 190 `+139B`.

## Alternatives

The route was not traced in the run; that it was `+139B` rests on FND-UI-065's reading.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/prefs-esc.json`.

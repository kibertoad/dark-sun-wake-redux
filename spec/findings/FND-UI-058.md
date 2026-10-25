---
id: FND-UI-058
title: Opened from the Game Menu, the Preferences window takes the menu's place with its corner at (55, 42); P from the game view opens nothing within 4 s, and at the start window DS:1431 holds no window
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py and the plans window-prefs.json, window-p.json and window-start.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoints play-ready of run v175-play-ready-b-20261019 and start of run v175-menu-discovery-20261019
---

## Observation

Times are the plans' absolute ones; positions are in the 320 x 200 frame.

**`window-prefs-20261025`** restores `play-ready`, presses key 2 (to 196,000 ms) and Tab (to
197,000 ms), moves the pointer to (160, 101), the Preferences button of the Game Menu at (55, 42)
(FND-UI-055), and clicks there. At 200,500 ms `DS:1431` holds `81CF:0004`, whose record words
`+0x96` and `+0x98` read 55 and 42, and the screenshot (MD5 `7798a76854405c2810e74a1c3bd682ef`)
shows the Preferences panel, with its two volume bars, the difficulty bar and its row of
buttons, in the frame and at the place the Game Menu occupied, with the empty bar below.

**`window-p-20261025`** restores `play-ready`, presses key 2 and then `p` at 196,000 ms; at
200,500 ms `DS:1431` is null and the screenshot shows the game view with no window.

**`window-start-20261025`** restores `start` of `v175-menu-discovery-20261019` and at 131,500 ms
reads `DS:1431` as null while the screenshot shows the start window's four entries.

## Interpretation

The Preferences window (`WIND/16500`, 210 x 116, FND-UI-033) opened from the Game Menu is placed
with its corner at (55, 42), the menu's own place. The `p` key did not open it in this state. The
start window is not the window `DS:1431` names.

## Alternatives

The Preferences window opened another way, if any, was not seen. Where the start window is kept
and placed was not read here.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/<plan>.json` with
`window-prefs.json`, `window-p.json` and `window-start.json`.

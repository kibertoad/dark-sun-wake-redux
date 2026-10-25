---
id: FND-UI-056
title: From play-ready, Tab opens the Game Menu with its window's corner at (55, 42), and pointing at a button draws no text in the bar below the buttons
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py with the plans menu-open.json and menu-hover.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Both runs restore `play-ready`, at about 195,167 ms of emulated time, and press Tab; times are the
plans' absolute ones and positions are in the 320 x 200 frame. Screenshots are 640 x 400.

**`menu-open-20261025`** reads `DS:1431` at 196,500 ms as offset `0x0004`, segment `0x81CF`,
and the window record's words `+0x96` and `+0x98` there as 55 and 42, and captures `menu.png` (MD5 `6fb0d722bbbe5750ade99369facaabe5`): the Game Menu panel spans about x 55
to 265 and y 44 to 158, with the fourteen buttons in three rows and an empty bar below them.

**`menu-hover-20261025`** then moves the pointer to (112, 73), (208, 100), (113, 127) and
(160, 150), one second apart, capturing `hover-10300.png`, `hover-10306.png`, `hover-10313.png`
and `hover-bar.png`. The pointer is over the View Character, Center on leader and Collapse party
buttons in the first three. The 260 x 20 crop of the bar from (190, 285) of the 640 x 400 frame
has the same MD5 in `menu.png` and the first three hover frames; it differs in `hover-bar.png`,
where the pointer is drawn over the bar. The button under the pointer is drawn with a lighter
border than in `menu.png`.

## Interpretation

Tab opens the Game Menu from play-ready, with the window's corner at (55, 42). Pointing at a button changes its border but writes
nothing in the bar below the buttons within a second, so the menu shows no description of the
pointed-at button there.

## Alternatives

One checkpoint, three buttons and one second per position; a description shown after a longer
wait or for other buttons was not tested. The crop's position was chosen from `menu.png`; the bar's rectangle was not read from the
window data.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/menu-open.json` and
`... menu-hover.json`, and compare the crops of the bar with Pillow.

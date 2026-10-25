---
id: FND-UI-057
title: After the leader key, the keys V, C and E open the View Character, Use and Effects windows with their corner at (0, 9), and I and O open the inventory and the overhead map at (0, 0)
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py (with its read_far step) and the plans window-v.json, window-i.json, window-c.json, window-e.json, window-p.json and window-o.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Each run restores `play-ready`, at about 195,167 ms of emulated time, presses key 2 and reads
`DS:426D` as 1 at 196,000 ms, then presses one key and at 197,500 ms reads the window pointer
`DS:1431`, the words `+0x96` and `+0x98` of the record it names, and a screenshot. Times are the
plans' absolute ones.

| Run | Key | `DS:1431` | `+0x96`, `+0x98` | Screenshot (MD5) |
|---|---|---|---|---|
| `window-v-20261025` | `v` | `8112:0004` | 0, 9 | the View Character window, title `VIEW CHARACTER` (`e999118dc5412f1fe10356f48f5f870b`) |
| `window-c-20261025` | `c` | `8112:0004` | 0, 9 | the same frame titled `USE`, with spell symbols for the leader (`852e61f0c9dc92e8791226bfb703d2c0`) |
| `window-e-20261025` | `e` | `8112:0004` | 0, 9 | the same frame titled `EFFECTS` (`162b97fb98fa3f27c493ba9771a623d5`) |
| `window-i-20261025` | `i` | `81CF:0004` | 0, 0 | the inventory window with the leader's picture (`b32816556ab043adb5b0c22c86a524c8`) |
| `window-o-20261025` | `o` | `8112:0004` | 0, 0 | the overhead map in its frame (`1565afce020d833b6f1350744f78c49b`) |
| `window-p-20261025` | `p` | null | | the game view with no window (`5e9026484e9ada18408507bc304dba96`) |

In earlier runs of the same plans with no key, or key 1, pressed first, `DS:426D` stayed 9999 and
the same keys left `DS:1431` null and the view without a window.

## Interpretation

The character, spell and effects windows, all `WIND/11500` (FND-INPUT-012), are placed with their
corner at (0, 9), and the inventory (`WIND/13500`) and the overhead map (`WIND/19502`) at (0, 0).
The keys open nothing while `DS:426D` is 9999.

## Alternatives

Which code passes these corners was not read. `p` opened nothing within 1.5 s, so the
Preferences window's place was not seen; whether it needs longer or another state was not tested.
That the windows are the resources named rests on FND-INPUT-012's reading and on the screenshots.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/window-<key>.json` for each key.

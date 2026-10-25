---
id: FND-COMBAT-059
title: From play-ready a left click on the view snaps the view back and starts the round steps, which stop while right clicks hold the pointer in modes 4 and 2 and go on when it returns to mode 1, with the leader key still refused
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/run_plan.py and indirect_targets.py, with the plans bug-view.json, bug-input.json, bug-input-control.json, bug-right.json and bug-right-control.json, at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Every run restores `play-ready`, at about 195,167 ms of emulated time, where `DS:426D` already
holds 9999 and `4C13:0369` holds 1. Times are the plans' absolute ones. The run addresses
`4D9A:` and `41CD:` are `DS` and `4C13`. Screenshots are compared by their MD5.

**`bugview-20261025`** (plan `bug-view.json`) moves the pointer to (160, 199) until 195,700 ms,
which scrolls the view down, and to (160, 100) until 197,700 ms, then left-clicks at (120, 118).
By 198,300 ms the view is back where it was at the restore, with the pointer at the click, record
9999's first three bytes (`DS:0CE6`) have gone from `0A 09 43` to `FF FF FF`, and `4C13:035B` has
gone from `0x123` to `0x290`; it reads `0x615`, `0xD20` and `0x1AFA` at 199,300, 201,300 and
205,300 ms. The four screenshots from 198,300 to 205,300 ms are identical.

**`bug-input-20261025`** makes the same moves and click, then at 200,300 ms moves the pointer to
(160, 199), which scrolls the view again, at 201,800 ms to (250, 150), and at 202,800 ms clicks
the left button there: the view and pointer do not change and `4C13:035B` keeps rising.
**`bug-input-control-20261025`** makes the same inputs without the first click: the view stays
scrolled and `DS:0CE6` unchanged until the click at (250, 150), after which the view is back at its
restore position, `DS:0CE6` reads `FF FF FF` and `4C13:035B` reads `0xE91` at 206,800 ms.

**`bug-right-b-20261025`** and **`bug-right-c-20261025`** (plan `bug-right.json`) make the moves
and first click, then right-click at (120, 118) at 200,300, 207,800, 210,800 and 213,800 ms and
press the key `2` at 216,800 ms. Readings of `DS:1440` and `4C13:035B`:

| Time (ms) | Event before | `DS:1440` | `4C13:035B` |
|---|---|---|---|
| 200,300 | first click | 1 | `0x99B` |
| 200,800 | right click | 4 | `0x99B` |
| 201,800, 203,800, 207,800 | | 4 | `0x99B` |
| 208,800, 210,800 | right click | 2 | `0x99B` |
| 211,800 | right click | 1 | `0xCE4` |
| 213,800 | | 1 | `0x13EF` |
| 214,800, 216,800 | right click | 1 | `0x1739`, `0x1E08` |
| 218,800 | key `2` | 1 | `0x24D7` |

`DS:426D` reads 9999 at every reading and `DS:054E` reads 0. The pointer is drawn as an orange
circle with a bar in mode 4. A run of `bug-right.json`'s inputs without the right clicks
(`bug-right-control-20261025`) reads `4C13:035B` at `0xB7C`, `0xF01`, `0x160C` and `0x23AA` at
200,800, 201,800, 203,800 and 207,800 ms.

`indirect_targets.py` runs (`right-20261025`, `rightonly6500-20261025`,
`rightonly9000-20261025`, `rightonly-m-20261025`) send the right click as a move and then a
press held 200 ms, at 5,000 ms after the restore: `DS:1440` stays 1, `28C9:1EDB` goes on calling
overlay 173 `+059A` through `28C9:2146` about 15 times a second, and `4C13:035B` goes on rising.

**`end-i-20261025`**, **`end-v-20261025`**, **`end-c-20261025`** and **`end-e-20261025`**
(`indirect_targets.py`, 8,000 ms) make the two moves and the left click at 3,000 ms after the
restore and press the key `i`, `v`, `c` or `e` at 4,000 ms, watching `DS:426D`, `DS:0CE7` and the
combat word: each run records only the click's store of -1 at `DS:0CE7` (at 3,056 ms, by
`1000:3F9C`), and each ends with `DS:426D` at 9999, the combat word at 0 and `4C13:035B` at
`0x11D3`.

## Interpretation

Any left click on the view in this state starts the defect of BUG-COMBAT-001, and the click moves
the view back to where it was. While the defect runs, edge scrolling still works and the view
does not otherwise change. Stepping the pointer out of mode 1 with the right button stops the
round steps, as FND-COMBAT-058 reads, and stepping it back to mode 1 starts them again; neither
ends the state, and the leader key is still refused afterwards. The keys of the character screens
leave the steps as they were. A right click does not always step
the mode: the fourth one here did not, nor did the held presses of the `indirect_targets.py`
runs.

## Alternatives

One checkpoint and one place on the view. When a right click steps the mode was not read; the
plan's clicks give the press and release no hold time.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory and the evidence interpreter, run
`run_plan.py artifacts/runs/<run> tools/research/rust-dos-runs/plans/<plan>.json` for
`bug-view`, `bug-input`, `bug-input-control` and `bug-right` (whose last five steps were added for
`bug-right-c`), and `bug-right-control` for the run without right clicks; and
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms>` with an inputs file holding
the two moves, the left click at 3,000 ms and a right click at (120, 118) at 5,000 ms, reading
`4C13:035B` and `57E0:1440`, and for the key runs the moves, the click and the key at 4,000 ms with
`--read 4C13:035B:4`, `--watch 57E0:426D:2`, `--watch 57E0:0CE7:2` and `--watch 4C10:0019:2`.

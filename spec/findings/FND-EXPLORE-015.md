---
id: FND-EXPLORE-015
title: At play-ready the leader's slot 1 holds 0xFFFF in its position words, so a Walk click plans from cell (4095, 4095), both route searches fail, 2D40:15D6 clears the walk and the no-path message runs
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Run `walk-c-20261025` restores `play-ready`, presses `2` at 200 ms, moves the pointer to
(120, 186) at 1,500 ms and left-clicks there at 2,000 ms, as FND-EXPLORE-008 did, and runs
6,000 ms. Counts start at 2,000 ms; times are from the restore.

Slot 1's record at `DS:67E0` holds `0xFFFF` in words `+0x0A` and `+0x0C` at the start and at the
end. Its step record at `4F49:037F` holds zeros but `0xFF` at `+0x11` at the start. At the end
`+0x11` is `0xFF`, `+0x12` is -1, `+0x14` is 11, and `+0x16` and `+0x18` are `0x0FFF`.

| Counted | Hits | Callers, first two stack words | Times (ms) |
|---|---|---|---|
| `1B2E:07F3` | 13 | `2D40:16C7` and `2D40:170B` once each with (`0xF850`, `0x0FFF`); `2D40:16C7` 11 times with (`0xF91C`, `0x003F`) or (`0xF91C`, `0x006B`) | 2,057 to 5,793 |
| `2D40:15D6` | 1 | | 2,061 |
| `28C9:04EF` | 1 | | 2,061 |
| `2D40:31F6` | 11 | `2D40:1734` | 3,185 to 5,809 |
| `28C9:19A1` | 50 (the cap) | `28C9:1E68`, slots 0 to 3 and twelve slots from 15 to 31 | 2,000 to 2,006 |

## Interpretation

The leader's slot has no position at this checkpoint, so its cell is `0xFFFF` shifted right by 4,
(4095, 4095). The plan starts there (`+0x16`, `+0x18`); both runs of the route search fail, the
first at `2D40:16C7` with `DS:0443` at 1 and the second at `2D40:170B` with it at 0. Since the plan
started at the current cell, `2D40:15D6` clears the mode word that FND-EXPLORE-008 saw cleared, and
the click handler reaches the no-path message (FND-EXPLORE-014). The click fails because the
leader has no place on the map, whatever cell it names. The other searches and the 11 calls of
`2D40:31F6` are for other walkers, whose start cells are on the map.

## Alternatives

What the message looked like on screen was not captured. Why slot 1 has no position at this
checkpoint, while a new game places the leader (FND-EXPLORE-013), was not looked into.

## How to reproduce

`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 6000 --clicks <inputs>` with key
`2` at 200 ms, a move to (120, 186) at 1,500 ms and a left click there at 2,000 ms, `--count
28C9:04EF --count 1B2E:07F3 --count 2D40:31F6 --count 28C9:19A1 --count 2D40:15D6 --count-from 2000
--read 4F49:037F:40 --read 57E0:67E0:55 --read 57E0:19B2:6`.

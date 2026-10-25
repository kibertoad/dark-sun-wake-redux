---
id: FND-EXPLORE-009
title: In rust-dos, while the pointer is held at the right edge from play-ready, the mouse driver's position call returns column 319 on only 3 of 50 calls and 160 on the rest, and those 3 are the passes that step the view
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; tools/research/exec-census/resident_listing.py; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

The runs make FND-EXPLORE-007's inputs: key 2 at 200 ms, the pointer moved to (160, 100) at
1,500 ms, to (319, 100) at 3,000 ms and back to (160, 100) at 4,500 ms, all from the restore,
over 6,000 ms. Counts start at 2,990 ms and keep 50 hits.

`45B9:0034` asks the mouse driver for the position with `int 33h` and `AX` = 3 at `45B9:003E`, and
stores `CX` and `DX` through its two pointer arguments from `45B9:0043`. `45B9:00EB` is the only
routine of the module that sets the position (`AX` = 4 at `45B9:00F3`). The edge-scroll pass
`28C9:2322` calls `45B9:0034` at `28C9:23A0` and goes on to `28C9:23C2` only when the column is 318
or more, or the row 199 or more, or either is 0 (FND-EXPLORE-006).

| Run | Counted | Result |
|---|---|---|
| `loop-e-20261025` | `28C9:23A0`, `28C9:23C2`, `28C9:23F1`, `28C9:23D1` with `AX` | 50 entries of the test between 2,990 and 5,085 ms; `28C9:23C2` 3 times, at 3,005, 3,911 and 4,482 ms; `28C9:23D1` 3 times with `AX` = 1 |
| `loop-f-20261025` | `45B9:0043` with `CX`, and `28C9:23C2` | of the 50 calls between 2,990 and 5,085 ms, 3 with `CX` = 319 and 47 with `CX` = 160; `28C9:23C2` at the same 3 times as in `loop-e` |
| `loop-g-20261025` | `45B9:00FC`, `45B9:00EC` | no entry |

## Interpretation

The passes that did not step read column 160 from the driver, though the input put the pointer
at 319 for 1,500 ms, and the game made no call that sets the position in that time. The low step
rate of FND-EXPLORE-007 is therefore set by what the emulator's driver reports, not by anything
the game paces: on the original with a driver that keeps reporting 319, every pass that reaches
the test with the pointer in the band and `28C9:2522` returning 1 would step.

## Alternatives

Why the emulated driver reports 160 between the three reads of 319 was not looked into; the
session's pointer input may be delivered once per move and lost afterwards. A run on another
emulator, or a recording of the original's pointer reads, would show the step rate with a
steadily held pointer.

## How to reproduce

`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 6000 --clicks <inputs>` with the
inputs above as `{at_ms, key}` and `{at_ms, x, y, button: none}` entries and `--count-from 2990`:
for `loop-e`, `--count 28C9:23A0 --count 28C9:23C2 --count 28C9:23F1 --count 28C9:23D1:ax`; for
`loop-f`, `--count 45B9:0043:cx --count 28C9:23C2`; for `loop-g`, `--count 45B9:00FC:cx --count
45B9:00EC`. `resident_listing.py <dsun> 45B9:0000..45B9:0200`.

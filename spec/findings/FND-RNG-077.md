---
id: FND-RNG-077
title: From play-ready, the dword 4C13:035B rises by 1 every 667 ms through 31E0:31F5, and after the view click that fills record 9999 overlay 173 +0C90 also adds 60 to it every 57 to 71 ms, still after 17 s and after a leader key, which leaves DS:426D at 9999
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

`time35b-click-20261025` restores `play-ready`, moves the pointer to (160, 199) at 500 ms and to
(160, 100) at 2500 ms, clicks the left button at (120, 118) at 3000 ms, and runs to 5000 ms with
watchpoints on `4C13:035B..035F` and `DS:054E`.

`4C13:035B` read `0x123` at the start and `0x743` at the end, and `DS:054E` read 0 throughout, with
no change. The dword changed 34 times:

- by 1 at 166, 833, 1500, 2167, 2832, 3499, 4166 and 4833 ms, each time after the instruction
  ending at `31E0:31F5`;
- by 60 at 3259 ms and then 25 more times to 4939 ms, 57 to 71 ms apart, each time after an
  overlay instruction whose 16 preceding bytes end with `66 26 83 06 5B 03 3C`, the
  `add dword es:[0x35B], 0x3C` at overlay 173 `+0C90`.

`time35b-long-20261025` sends the same inputs and runs to 20000 ms with watchpoints on `DS:426D`
and `4C10:0019`: neither changed, and `4C13:035B` read `0x3B9D` at the end, 14970 above `0x123`:
30 steps of 1 at the rate above and 249 of 60.

`time35b-key2-20261025` sends the same inputs and the key 2 at 4000 ms, the key that set
`DS:426D` to 1 in FND-RNG-069, and runs to 8000 ms with watchpoints on `4C13:035B`, `DS:426D` and
`4C10:0019`. `DS:426D` and `4C10:0019` did not change; the steps of 60 went on 57 to 80 ms apart
to 7948 ms, and `4C13:035B` read `0x11D3` at the end.

## Interpretation

Before the click the dword advances only through `31E0:31F5`. From the first pass after the click,
the round step of FND-RNG-076 runs once on each pass of `28C9:1EDB` (FND-RNG-075), so the dword
advances by 60 about 15 times a second while no combat is going on, some 600 times its rate before
the click. It goes on for at least 17 s, and the leader key that ends the 9999 window before a
click (FND-RNG-069) does not end it.

## Alternatives

The runs last at most 17 s after the click; when the extra advance stops, if ever, was not
observed, nor why the key 2 sets no leader in this state. That
the call comes through `28C9:1EDB` rests on FND-RNG-074 and on FND-RNG-075's counts in a separate
run with the same inputs; these runs counted no breakpoints.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready time35b-click-20261025 5000` with an
inputs file holding the two moves and the left click above, `--read 4C13:035B:4`,
`--read 4F49:0002:4`, `--read 57E0:054E:1`, `--watch 4C13:035B:4` and `--watch 57E0:054E:1`; the
same command for `time35b-long-20261025` with 20000, `--read` for `4C13:035B:4`, `57E0:426D:2`
and `4C10:0019:2` and `--watch` for `57E0:426D:2` and `4C10:0019:2`; and for
`time35b-key2-20261025` with 8000, the inputs and key `2` at 4000 ms, the same reads and `--watch`
for `4C13:035B:4`, `57E0:426D:2` and `4C10:0019:2`.

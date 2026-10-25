---
id: FND-EXPLORE-007
title: From play-ready the main loop runs about 50 passes in 88 ms with the pointer at the centre, and about 24 a second with the pointer on the right edge, where only three passes in 1.5 s step the view by 16
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

Each run restores `play-ready` and runs 6,000 ms with the same inputs: key 2 at 200 ms, the
pointer moved to (160, 100) at 1,500 ms, to (319, 100) at 3,000 ms and back to (160, 100) at
4,500 ms. Times are from the restore. The main loop calls `28C9:2322`, the edge-scroll pass,
once a pass (FND-EXPLORE-006).

**`loop-a-20261025`** counts entries of `28C9:2322` from 2,000 ms and watches `DS:0DA8` and
`DS:0DAB`: the 50 counted entries fall between 2,001 and 2,089 ms, each returning to
`277B:0574`; neither watched field changes in the run, and `DS:0DAB` reads 1 at the start and at
the end.

**`loop-b-20261025`** watches `view_left` (`DS:1408`) and `DS:0DA8`: `view_left` goes from 0 to
16 at 3,005 ms, to 32 at 3,911 ms and to 48 at 4,482 ms, each time by the instruction ending at
`362C:0ED3`; `DS:0DA8` does not change. `DS:0D9C` and `DS:0DAB` read 1 at the start and at the
end.

**`loop-c-20261025`** counts entries of `28C9:2322` and `28C9:050F` from 2,990 ms: the 50 entries
of `28C9:2322` fall between 2,990 and 5,085 ms, and `28C9:050F` is entered three times, at 3,005,
3,911 and 4,482 ms, each from `28C9:23EC` with the column 319 and the row 100.

## Interpretation

The main loop is not held to a fixed rate: with nothing to redraw it ran a pass about every
1.8 ms here, and while the pointer was on the right edge about every 42 ms. With the pointer on
the edge, `DS:0DA8` at 0 and `DS:0DAB` at 1, the step routine ran on only three of those passes,
906 and 571 ms apart. `DS:0D9C` did not change either (run `loop-d-20261025`, which watches it),
and `28C9:2522` only returns `DS:0DAB`, so by FND-EXPLORE-006's reading the other passes must have
read a pointer column below 318 from `45B9:0034`, or not reached the test; which of these held was
not seen. The scrolling speed in this emulator setting is about one 16-pixel step every 0.6 to 0.9 s.

## Alternatives

The counts keep 50 entries each and record only the first and last times, so how the passes
spread over the window was not seen. What `28C9:2322` does on the passes that do not step was not
read; the pass and step rates depend on the emulator's 15,000 cycles setting.

## How to reproduce

With the evidence interpreter and the game directory configured, run
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 6000 --clicks <inputs>` with the
inputs above as `{at_ms, key}` and `{at_ms, x, y, button: none}` entries, and: for `loop-a`,
`--count 28C9:2322 --count-from 2000 --watch 57E0:0DA8:1 --watch 57E0:0DAB:2 --read
57E0:0DAB:2`; for `loop-b`, `--watch 57E0:1408:2 --watch 57E0:0DA8:1 --read 57E0:1408:4 --read
57E0:0D9C:2 --read 57E0:0DAB:2`; for `loop-c`, `--count 28C9:2322 --count 28C9:050F --count-from
2990`; for `loop-d`, `--watch 57E0:0D9C:2`.

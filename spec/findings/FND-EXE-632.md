---
id: FND-EXE-632
title: With a once-breakpoint on every overlay trampoline, four rust-dos runs from the two-character play checkpoint enter 24 overlay routines and none of the ten that no direct call reaches
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/trampoline_census.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019 (FND-UI-054)
---

## Observation

`trampoline_census.py` restored the checkpoint `play-ready` (FND-UI-054) in a new session, set a
once-breakpoint on each of the 854 trampolines of the FMT-EXE-004 overlay headers, ran for a set
time with a list of pointer moves and clicks in virtual 320 x 200 coordinates, and recorded each
trampoline entered, the return address on the stack and the call site that address follows. A
return into resident code was placed by the relocated segment (run segment plus `0x0A46`); a
return into overlay code was placed by matching the code before it against each overlay's file
bytes.

| Run | Time | Input | Trampolines entered |
|---|---|---|---|
| `census-view-20261025` | 2 s | none | 1 |
| `census-scroll-20261025` | 4 s | pointer to (160, 199) at 0.5 s and (160, 100) at 2.5 s | 2 |
| `census-walk2-20261025` | 40 s | the two moves, then left clicks at (120, 118), (120, 75), (40, 160) and (125, 180) at 3, 9, 15 and 21 s, a right click at (160, 100) at 27 s and a left click at (24, 18) at 33 s | 24 |
| `census-walk3-20261025` | 36 s | the same as `census-walk2-20261025` | 24 |

The two long runs entered the same 24 routines, the first at 0.04 s and the last at 3.32 s, so
after the first click at 3 s and none in the 30 s that followed. With the caller placed:

- from resident code: 173 `+059A` (`28C9:2158`), 177 `+033B` (`28C9:21D1`), 182 `+059C`
  (`28C9:1304`), 186 `+055E` (`2C5F:026A`), 188 `+1876` (`2D40:044E`), 193 `+0DC7` (`31E0:2D89`)
  and 204 `+0169` (`172C:0FB0`);
- from overlay 173: 176 `+03CD` (return `+1027`), 182 `+1D3E` (`+3DFF`), 187 `+2B91` (`+078C`),
  188 `+1820` (`+07B7`), 193 `+1C95` (`+338B`), 195 `+0A98` (`+0CA6`), 195 `+0AAD` (`+0CA1`), 195
  `+14D5` (`+079A`), 206 `+0B49` (`+079F`) and 210 `+0B44` (`+07B2`);
- from overlay 195: 193 `+1BA3` (`+14FA`) and 197 `+29E0` (`+1508`);
- from overlay 199: 182 `+19F8` (`+0CE3`) and 190 `+11A1` (`+0BF3`);
- from overlay 204: 204 `+259D` (`+1AAF`);
- from overlay 210: 199 `+0C21` (`+0B4C`);
- 185 `+0000`, from overlay 210 at one of three returns (`+0D1D`, `+0C6E` or `+0C1B`) whose
  preceding bytes match equally.

The short runs entered 193 `+0DC7`, and the scroll run also 182 `+059C`. None of the four runs
entered the trampolines `572F:005C`, `56A7:0057`, `5660:0025`, `573B:00BB`, `5702:00ED`,
`5702:00F7`, `56B9:002F`, `5691:007A`, `5691:005C` or `57B9:0052` (FND-EXE-631).

## Interpretation

From the play checkpoint with this input, the game enters 24 overlay routines once each in its
first seconds and then, with every breakpoint spent or never reached, none that had not already
been entered. The ten routines of FND-EXE-631 are not reached by this play; the runs say nothing
about states they do not visit, such as the Game Menu whose controls FND-EXE-631 ties to
`56B9:002F`.

## Alternatives

A once-breakpoint stops only at the first entry, so a routine entered again later is counted once.
The clicks did not move the party (the view origin stayed at (0, 0) in both long runs), so walking,
combat, conversation and menus are not covered.

## How to reproduce

From the repository, with `GAME_DIR` the installed BLD-GOG-EN-1.1 directory holding
`captures/rust-dos-regenerated/v175-play-ready-b-20261019`, run
`python tools/research/rust-dos-runs/trampoline_census.py v175-play-ready-b-20261019 play-ready
<run> <ms> [clicks.json]` with the times above and a clicks file of `{at_ms, x, y, button}`
entries holding the inputs above (`button` `none` for a move). Each run writes `census.json`
under `GAME_DIR/captures/rust-dos-census/<run>`.

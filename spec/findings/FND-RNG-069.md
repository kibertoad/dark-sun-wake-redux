---
id: FND-RNG-069
title: From the two-character play state, combat ends 66 ms after the restore and overlay 173 leaves DS:426D at 9999 until a leader key sets it, so the play-ready checkpoint holds 9999
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoints start, create and play of run v175-play-final-20261019 (FND-UI-054)
---

## Observation

`DS:426D`, the word `4C13:0369` and the combat word `4C10:0019` read 0, 0 and 0 in the checkpoints
`start` and `create`, and 0, 0 and 2 in `play`, the state from which the key 2 that FND-UI-054
sends makes `play-ready`.

Restored from `play` with watchpoints on `DS:426D` and `4C10:0019` and no input
(`watch426d-idle-20261025`, 5 s), `DS:426D` became 1 at 46 ms, after the instruction ending at
overlay 182 `+1DA8`, and 9999 at 66 ms, after the instruction ending at overlay 173 `+07D1`; at
66 ms the store at `28C9:2160` set `4C10:0019` to 0. Nothing else changed either word, and both held 9999 and
0 at 5 s. Overlay code was located by the 16 bytes before each stop, matched against the file
with the far call's segment word left out.

With key 2 sent at 100 ms (`watch426d-key2-20261025`, 1.5 s, and `watch426d-key2b-20261025`,
0.4 s), the same three changes happened at the same times, `4C13:0369` became 1 at 46 ms through the
store at `2D40:3DB4`, and at 263 ms the instruction ending at `28C9:0E58` set `DS:426D` to 1.

FND-RNG-067 restored `play-ready` and found `DS:426D` still 9999 and `4C10:0019` 0 more than 3 s
later, until its click.

## Interpretation

Combat ends on its own 66 ms after `play` is restored, and its end leaves `DS:426D` at 9999. The
key that selects a leader sets it again; until a key or another store does, a view click sends
9999 down the path FND-RNG-068 reads. The `play-ready` checkpoint was saved inside that window.

## Alternatives

The runs cover one route into play. Other ends of combat, and the stores to `DS:426D` they are
followed by, are read in FND-RNG-070 and Q-RNG-031.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-final-20261019 <state> <run> <ms>`: `start`, `create` and `play`
with 100 and `--read` for `57E0:426D:2`, `4C13:0369:2` and `4C10:0019:2`;
`watch426d-idle-20261025` from `play` with 5000, `--watch 57E0:426D:2` and
`--watch 4C10:0019:2`; `watch426d-key2-20261025` from `play` with 1500, an inputs file holding key
`2` at 100 ms and `--watch` for `57E0:426D:2`, `4C13:0369:2` and `4C10:0019:2`. Search each
overlay's code for the stop's 16 bytes with the two bytes of the far call's segment word as
wildcards.

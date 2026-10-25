---
id: FND-RNG-086
title: From play-ready, with and after the view click that leaves DS:426D at 9999, 31E0:2C86 calls 31E0:2D6E only with its own stack copy of the slot, and 31E0:0EFF stores only slots 0xDC to 0x10B in the placed-object entries
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

Both runs restore `play-ready`, move the pointer to (160, 199) at 500 ms and to (160, 100) at
2500 ms, click the left button at (120, 118) at 3000 ms, and run to 6000 ms; this is the input of
FND-RNG-075's post-click run, after which `DS:426D` holds 9999. Addresses are static; stack words
are as the run holds them, with the run's `SS` at `4D9A`.

**`q034a-20261025`** counts entries of `31E0:2C86`, `31E0:2D6E` and `2D40:10AE`.
`31E0:2C86` reached the cap of 50 hits by 5072 ms. `31E0:2D6E` was entered 19 times, from 42 to
5799 ms, each from `31E0:2D5F`, with the far pointer argument `4D9A:FF32` (17 times) or
`4D9A:FEF4` (twice). `2D40:10AE` was entered 21 times: from `31E0:2DB2` with slots 15 (10 times) and
16 (9 times) and second argument 0, from `2D40:095C` with 16 and 1, and from `28C9:04DA` with 9999
and 1.

**`q034b-20261025`** counts `31E0:0EFF` and the store `31E0:1127`, with `SI` at the store.
`31E0:0EFF` was entered 9 times between 569 and 587 ms, each from `31BA:013C`, and `31E0:1127` ran
9 times with `SI` at `0x10B` and `0xDC` to `0xE3`.

## Interpretation

In this run `31E0:2C86` never passes the address of `DS:426D` to `31E0:2D6E`: the far pointers are
its frame's copy of the slot (FND-RNG-072), at `DS:FF32` and `DS:FEF4` in the stack. So no record
that reached the call had an entry naming the slot `DS:426D` held, before or after it became
9999. The call of `2D40:10AE` with 9999 is the one FND-RNG-068 reads on the view click's path. `31E0:0EFF`'s stores in this run come
from `31BA:0137`'s slot -1 and take free slots.

## Alternatives

One input sequence from one checkpoint; the four calls of `31E0:0121` that FND-RNG-085 leaves
unbounded were not reached.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 6000` with an inputs file holding
the two moves and the left click: `q034a-20261025` with `--count` for `31E0:2C86`, `31E0:2D6E` and
`2D40:10AE`; `q034b-20261025` with `--count 31E0:1127:si` and `--count 31E0:0EFF`.

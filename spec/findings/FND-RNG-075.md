---
id: FND-RNG-075
title: From play-ready, 28C9:1EDB returns through 28C9:1FE8 before the view click and through overlay 173 +059A after it, and neither leaves DS:426D, the combat word or the month lengths changed beyond the link store
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
2500 ms, and count the hits of breakpoints at `28C9:1EDB`, `1F03`, `1F14`, `1F2B`, `1F51`, `1FE8`,
`1FFF`, `2127`, `2146`, `2171`, `21D1`, `21FA` and `230B`, each up to 50.

`branch1edb-pre-20261025` (2900 ms, counts from 2000 ms): `1EDB`, `1F03`, `1F14`, `1FE8` and `230B`
were hit 50 times each from 2222 to 2644 ms, and the other sites not at all. `DS:426D` read 9999,
`4C10:0019` 0, `DS:0CE6` the file's text and `4F49:8160` `2E 00 CD` at the start and the end.

`branch1edb-post-20261025` (5000 ms, a left click at (120, 118) at 3000 ms, counts from 3300 ms,
watchpoints on `DS:3950..395C`, `DS:426D` and `4C10:0019`): `1EDB`, `1F03`, `2127` and `2146` were
hit 25 times each from 3332 to 4929 ms, and `2171`, `21D1` and `230B` 26 times from 3313 ms; `1F14`,
`1F2B`, `1F51`, `1FE8`, `1FFF` and `21FA` not at all. At the end `DS:0CE6..0D0B` held `0xFF`,
`DS:426D` 9999 and `4C10:0019` 0, and `4F49:8160` was unchanged. The only watched change was
`DS:3954..3958` from `1F 1E 1F 1F` to `1F 1E 0E 00` at 3056 ms by `2D40:310B`, the link store of
FND-RNG-068.

## Interpretation

Before the click, slot 9999 takes the path of FND-RNG-074 that makes no store. After it, every
pass calls overlay 173 `+059A(0, &DS:426D)`, which in this run returned 0 each time and left
`DS:426D` at 9999, so combat did not start; `21FA` is never reached, so each pass leaves at
`21F7`.

## Alternatives

The runs cover one checkpoint and one click for about 2 s each. Overlay code was not counted, so
which path inside overlay 173 `+059A` returned 0 was not observed.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms>` with `--count 28C9:<off>`
for each site above, `--read 57E0:426D:2`, `--read 4C10:0019:2`, `--read 57E0:0CE6:25` and
`--read 4F49:8160:3`: for `branch1edb-pre-20261025`, 2900, `--count-from 2000` and an inputs file
holding the two moves; for `branch1edb-post-20261025`, 5000, `--count-from 3300`, the two moves and
the left click, and `--watch` for `57E0:3950:C`, `57E0:426D:2` and `4C10:0019:2`.

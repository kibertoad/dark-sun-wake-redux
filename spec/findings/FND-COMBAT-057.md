---
id: FND-COMBAT-057
title: After the view click that fills record 9999, 2D40:2F34 gets slot 9999 only once; each later pass calls 2D40:3D4C(0, 1) from overlay code, block routines clear and 2D40:3DA0 restores the owner word of slot record 21, and 1B0B:00A0 flips 4C13:0373 between 1 and -1
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

All runs restore `play-ready`, move the pointer to (160, 199) at 500 ms and to (160, 100) at
2500 ms and click the left button at (120, 118) at 3000 ms.

`count-args-20261025` (4400 ms) counts from 2900 ms the entries of `2D40:2F34`, `2D40:3D4C`,
`2D40:10AE` and `2D40:197A`, with the far return address and the first two argument words at
each. `2D40:2F34` and `2D40:197A` take one argument, so their second word is the caller's next
stack word.

| Routine | Hits | Calls (static return address, arguments: count) |
|---|---|---|
| `2D40:2F34` | 9, 3056 to 4203 ms | `2D40:10D9` (9999, 0x78): 1; `2D40:0A17` (16, 9999): 1; `2D40:088C` (16, 9999): 1; `2D40:10D9` (16, 15): 1, (15, 2): 2, (16, 2): 3 |
| `2D40:10AE` | 7, 3056 to 4203 ms | `28C9:04DA` (9999, 1): 1; `2D40:095C` (16, 1): 1; `31E0:2DB2` (15, 0): 2, (16, 0): 3 |
| `2D40:3D4C` | 18, 3056 to 4398 ms | `2D40:31BE` (9999, 14): 1; run address `61B2:1870`, outside the load image (0, 1): 17 |
| `2D40:197A` | 17, 3314 to 4398 ms | `28C9:1E51` (9999, 0xFFE2): 17 |

`count-pass-20261025` (4400 ms, counts from 3300 ms) found 16 entries of `28C9:1EDB` in the same
span, from 3332 to 4344 ms.

`watch-owner-20261025` (6000 ms, watchpoints on `4F49:0A32..0A34`, `DS:6C12`, `DS:8F7B`,
`4C13:0373..0375` and `4F49:0000`): the word `4F49:0A32`, word `+0` of slot record 21 at
`4F49:08A3`, went from 1 to 0 at 3313 ms after `1000:3F6F`, and then, 41 times each, from 0 to 1
after `2D40:3DA0` and from 1 to 0 after `1000:3F9C`. `4C13:0373` went from 1 to -1 after
`1B0B:00A0` 41 times and back to 1 after the same instruction 40 times, from 3326 ms. `DS:8F7B`
went from `0x64` to `0x63` at 5071 ms and `DS:6C12` from `0x6E` to `0x6D` at 5328 ms, each once,
after `289B:008D`. `4F49:0000` counted up from 0 to `0x30` and back to 0 on every pass, after
`2D40:1D49` and `2D40:1D7C`.

## Interpretation

The click relocates record 9999 once; the later relocation calls take slots 15 and 16, which
`2D40:2F34` returns unchanged (FND-RNG-068), so record 9999 is not relocated again. On each pass
some overlay routine calls `2D40:3D4C(0, 1)`, which gives the slot records owned by slot 0 to slot
1 (FND-RNG-071), after block routines have cleared slot record 21's owner word. `4F49:0A32` and
`4C13:0373` alternate on each pass, so a snapshot shows whichever phase it caught (FND-COMBAT-056).

## Alternatives

The overlay that calls `2D40:3D4C` from run address `61B2:1870` was not identified, nor the
callers of the block routines and of `1B0B:00A0`. `289B:008D` changed each record byte once in
2.7 s and may run without the click.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> <ms>` with an inputs file holding
the two moves and the left click: `count-args-20261025` with 4400, `--count-from 2900` and
`--count` for `2D40:2F34`, `2D40:3D4C`, `2D40:10AE` and `2D40:197A`; `count-pass-20261025` with
4400, `--count-from 3300` and `--count` for `2D40:2F34`, `2D40:3D4C`, `2D40:10AE`, `2D40:03C1`,
`1000:3F5A`, `1000:3FA2`, `28C9:1EDB`, `2D40:197A` and `1B0B:0000`; `watch-owner-20261025` with
6000 and `--watch` for `4F49:0A32:2`, `57E0:6C12:1`, `57E0:8F7B:1`, `4C13:0373:2` and
`4F49:0000:1`.

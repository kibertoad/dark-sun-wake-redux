---
id: FND-RNG-079
title: From play-ready, the page words at 1BF3:0004 hold segments from A000 to AE55 or small even numbers, and the segment 2707 copy routines are called with pages 0 and 1, and 10 and 9
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

`pages2707-20261025` restores `play-ready`, moves the pointer to (160, 199) at 500 ms and to
(160, 100) at 2500 ms, clicks the left button at (120, 118) at 3000 ms and runs to 6000 ms. It
reads `1BF3:0004..0044` and `1BF3:0204..0244` at the start and the end, and counts the entries of
`2707:0109`, `2707:055A`, `2707:02A8` and `2707:0395` from the start, with the return address and
the next two stack words at each.

The 17 words at `1BF3:0004` read, for pages 0 to 16, `A400`, `A000`, 2, 2, `AE55`, `A7E8`,
`0x0A`, `ABD0`, `ABD7`, `AC0D`, `AD4D`, `AE15`, `AE55`, 0, 2, 2 and `0x18` at the start; at the end
pages 12 and 14 held 2 and `0x0E`, and the others were unchanged. The other 15 words read 0.

`2707:0109` was entered 16 times, each from `362C:0B53` with the words 0 and 1 (its arguments
`[bp+6]` and `[bp+8]`, the source and destination pages). `2707:055A` was entered 16 times, each
from `362C:0B6B` with the words `0x0A` and 9. `2707:0395`, a near routine, was entered 16 times
with the return offset `0x0637` inside segment `2707`, after the case `2707:062C` of the second
table. `2707:02A8` was not entered.

## Interpretation

In this run the copy routines of segment `2707` move bytes between pages whose segments lie in
video memory, from `A000` to `AE55`. The small even words are the byte offsets of other pages,
which `1BF3:42FB` follows when bit `0x40` of the page's word at `1BF3:0C04` is set (FND-PARTY-195),
so they do not name segments themselves.

## Alternatives

One run from one checkpoint. The flag words at `1BF3:0C04` were not read, and nothing here shows
which code writes the page words; a page whose word held the data segment would make these
copies write it.

## How to reproduce

With `GAME_DIR` the installed BLD-GOG-EN-1.1 directory, run with the evidence interpreter
`indirect_targets.py v175-play-ready-b-20261019 play-ready pages2707-20261025 6000` with an inputs
file holding the two moves and the left click, `--read 1BF3:0004:40`, `--read 1BF3:0204:40` and
`--count` for `2707:0109`, `2707:055A`, `2707:02A8` and `2707:0395`.

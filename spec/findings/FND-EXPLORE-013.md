---
id: FND-EXPLORE-013
title: After Start Game, slot 0's record holds the point (1192, 1496) and the top-left (1184, 1459), and the other three slots (2112, 1632)
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint start of run v175-menu-discovery-20261019
---

## Observation

Run `newgame-pos-20261025` restores `start` (the start window), moves the pointer to (157, 76)
at 300 ms and left-clicks there, on Start Game, at 800 ms, and runs 40,000 ms. Times are from the
restore.

`31E0:0121` is entered four times, between 4,529 and 4,535 ms, each from the same return address,
with the object and slot arguments (305, 0), (300, 1), (313, 2) and (308, 3).

At the start and at the end, `4C13:0369` reads 0 and `DS:140C..1411` read 50, 1192 and 1496. The
slot records at `DS:67BB` hold `0xFF` in every byte at the start. At the end:

| Slot | Words `0x0A`, `0x0C` | Words `0x03`, `0x05` | Byte `0x07`, word `0x08` |
|---|---|---|---|
| 0 | 1192, 1496 | 1184, 1459 | 8, 37 |
| 1 | 2112, 1632 | 2101, 1606 | 11, 26 |
| 2 | 2112, 1632 | 2095, 1602 | 17, 30 |
| 3 | 2112, 1632 | 2097, 1583 | 15, 49 |

## Interpretation

This is FND-EXPLORE-012's reading on a new game: the leader, slot 0, object 305, at the point
(1192, 1496), cell (74, 93), with its top-left the point less the offsets 8 and 37; the other
members at `0x840`, `0x660`.

## Alternatives

The words were read at the end of the run, not at the return of `31E0:0121`; nothing moved the
leader in between, since the words match the values the call would store.

## How to reproduce

`indirect_targets.py v175-menu-discovery-20261019 start <run> 40000 --clicks <inputs>` with a
move to (157, 76) at 300 ms and a left click there at 800 ms, `--count 31E0:0121 --count-from 0
--read 57E0:67BB:148 --read 4C13:0369:2 --read 57E0:140C:6`.

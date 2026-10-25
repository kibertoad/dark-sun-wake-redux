---
id: FND-EXPLORE-008
title: From play-ready, a Walk click on the view stores the clicked cell for the leader's slot and calls 2D40:10AE, which clears the record's mode word within 4 ms, and nothing on screen moves in the next 6 s
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: dinorefurb-dosbox-session 0.11.0 RustDosSession; tools/research/rust-dos-runs/indirect_targets.py, run_plan.py and the plans walk-click.json and walk-click-b.json at the commit that adds this finding; Python 3.14.7
environment: Windows; pinned rust-dos 1.7.5 commit 5a23d2cd400a8f9986d278ba97dc52b270b2ac14, executable SHA-256 1635427ad8eaddd08e11ca31c9fc81cabff5c264bfaa708a1eca477d54184830; deterministic 15000 cycles; default muted host audio; checkpoint play-ready of run v175-play-ready-b-20261019
---

## Observation

Each run restores `play-ready`, where the pointer mode `DS:1440` is 1 (Walk) and the leader
`4C13:0369` is 1, presses key 2 at 200 ms and left-clicks the view at 2,000 ms. Times are from the
restore.

**`walk-b-20261025`** clicks at (120, 186). `DS:426D` reads 9999 after the restore and 1 at the
end. From 1,900 ms:

- `28C9:0353` is entered once, at 2,057 ms, from `28C9:06C5` with 1 and 120.
- `2D40:10AE` is entered once from `28C9:04C8` with 1 and 1. Its other 49 entries counted, up to
  4,137 ms, come from `31E0:2DB2` with 15 or 16 and 0.
- `28C9:01CB` is not entered.
- At 2,057 ms, `28C9:0485`, `28C9:0497` and `28C9:04A9` change the words at `4F49:08B6`, `08B8`
  and `08BA`, slot 1's record at `4F49:08A3 + 19 × slot`, from `0x0FFF`, `0x0FFF` and 0 to 7, 11
  and 15.
- At 2,061 ms the instruction ending at `2D40:1601` sets the word at `4F49:08BA` back to 0.
- In slot 1's record at `DS:67E0`, the word at `+0x16` changes at 2,252 ms and 2,775 ms (to
  `0xF0B3`, then 56).

**`walk-a-20261025`** clicks at (100, 110) and sees the same calls of `2D40:10AE`, with one from
`28C9:04C8` with 1 and 1.

**`walk-click-b-20261025`** and **`walk-click-20261025`** make the same inputs at 196,000 and
197,000 ms of absolute time and take screenshots before the click and 1 s and 6 s after it. Each
run's three screenshots are identical (MD5 `81b7424888d3422e89f8204ee2026833` for the click at
(120, 186)), and show a room with the view's corner at (0, 0) and no party member in it.

## Interpretation

The click goes the way FND-RNG-068 reads `28C9:0353`: the cell (7, 11) of the point (120, 186)
and the mode 15 are stored for the current slot, 1, and `2D40:10AE` runs for it between the
party-cell calls. Within the same call `2D40:10AE` clears the mode word, and no step follows: the
step routine of the direction keys (FND-EXPLORE-004) is not entered and nothing moves. Whether the
route search found no way to the cell or the walk is done elsewhere was not seen.

## Alternatives

The leader's own cell was not read, so whether the clicked cells were reachable is not known. The
counts keep 50 entries and record only the first and last times.

## How to reproduce

`indirect_targets.py v175-play-ready-b-20261019 play-ready <run> 8000 --clicks <inputs>` with
key 2 at 200 ms, a move to the point at 1,500 ms and a left click there at 2,000 ms, and
`--count 28C9:0353 --count 2D40:10AE --count 28C9:01CB --count-from 1900 --watch 4F49:08B6:6
--watch 57E0:67E0:37 --read 57E0:426D:2`; and `run_plan.py artifacts/runs/<run>
tools/research/rust-dos-runs/plans/walk-click-b.json` (and `walk-click.json`).

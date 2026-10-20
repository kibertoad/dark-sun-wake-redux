---
id: FND-RNG-056
title: A path walk of every routine that writes DS finds 24 resident routines that can return with DS changed and no overlay routine; only overlay 170's two copy routines call out with DS changed, and the two blit routines of segment 2707 that return with DS off by one slot are never called with the modes that take those exits
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..1000:0163
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:02A8..2707:02FF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:0395..2707:03DA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:055A..2707:0591
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057CB0..0x00058023
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:1D4A..1000:1E8C
tool: tools/research/exec-census/ds_restore_census.py, resident_listing.py, overlay_listing.py and direct_callers.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`. FND-RNG-053 finds 255 routines of the coverage
inventory that write `DS` and asks (Q-RNG-023) whether `DS` equals `SS` at the entry of overlay 193
`+003C`, overlay 201 `+05D5` and overlay 204 `+1F06`.

`ds_restore_census.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` walks every path of each
routine that writes `DS` or calls a routine found to change it, keeping `DS`, the stack and the
registers as `entry`, `data` (0x57E0) or `other` under the rules its docstring lists. It reports
255 routines with 628 writes, 257 routines walked in 3 rounds, and 24 routines with a path that
reaches `ret`, `retf` or `iret` with `DS` `other`:

| Routines | Reading |
|---|---|
| `1000:1367`, `1000:13CA`, `1000:14C4`, `1000:1528`, `1000:1582`, `1000:1622` | near helpers of the far heap; their callers reload `DS` from the code-segment word at `1000:1361` |
| `1000:02AD` | the exit path |
| `1000:1E12`, `1000:1E2D` and their far wrappers `1000:1D4A`, `1000:1D58` | the resume of the critical-error handler `1000:1D66`, which leaves through the handler's saved frame (`1000:1D4A..1000:1E8C`) |
| `1000:1F50` | calls an `INT` stub it builds on the stack with `DS` from the caller's register record and pops a saved `DS` at `1000:1FEC`; the flagged path was not traced by hand |
| `289B:02DB`, `45CD:0138` | normalize a far pointer and return its segment in `DS` |
| `1BF3:40DD`, `1BF3:438D`, `1BF3:4404`, `1BF3:4C91`, `1BF3:5C17`, `1BF3:5C2C`, `1BF3:5CDE`, `4842:182E` | not read |
| `2707:02A8`, `2707:0395` | see below |

No routine inside an overlay is among them. The overlay routines walked report a call made with
`DS` `other` only in overlay 170 `+0040` and `+019D` (`0x00057CB0..0x00058023`): each loads `DS`
from its far-pointer argument and calls `1BF3:738F` with it, and each pops the `DS` it pushed on
entry before its `retf`.

**Segment 2707.** `2707:02A8` (`2707:02A8..2707:02FF`) pushes `DS`, `SI` and `DI`, then a segment
word returned by `1BF3:42FB`, and jumps through a far table at `2707:02FF` on its third argument.
Arguments 0 and 4, and any argument of 8 or more, go to `2707:0390`, which pops `DI`, `SI` and `DS`
without first popping the pushed segment, so `DS` takes the caller's `SI`. The other entries pop
it at `2707:0381`. `2707:0395` has the same layout with its table at `2707:03DA`, whose entries 2
and 6 skip the pop at `2707:0486`. `direct_callers.py` finds both called only from `2707:055A`
(`2707:055A..2707:0591` and the cases from `2707:05B1`), which passes the literals 1, 2, 3, 5, 6
and 7 to `2707:02A8` and 0, 1, 3, 4, 5 and 7 to `2707:0395`, and pops its own saved `DS` before
its `retf`.

**Startup.** `1000:0000` loads `DX` with 0x57E0, calls `INT 21` with `AH` 0x30, which returns its
values in `AX`, `BX` and `CX`, then writes `DS` from `DX` at `1000:0014` and `SS` from `DX` at
`1000:00BB`; it writes neither again before the far call of the game's main routine `277B:0024` at
`1000:0158`. The census, which takes every register but `SI`, `DI` and `BP` as changed by an `INT`,
reports that call and the 165 other calls and 92 `INT`s it lists as made with `DS` `other`.

## Interpretation

Under the census rules, a routine outside the 24 returns with the `DS` it was entered with, and
the two segment 2707 routines do so for every value their one caller passes. No overlay routine
changes `DS` across a call to it. So `DS` at the entry of the three seed routines is the `DS` of
the resident routine whose far call starts the overlay chain, unless a call the census lists as
made with `DS` `other` leads to them. The main routine is entered with `DS` and `SS` both 0x57E0.

## Alternatives

- `other` covers a `DS` loaded from memory that holds 0x57E0; several listed calls may be made
  with the data segment.
- Indirect jumps and calls end a path; code outside the inventory is not read.
- Far calls through trampolines and the overlay manager are taken to keep `DS`.
- Eight of the 24 routines were not read by hand, and `1000:1F50` only in part.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`: `ds_restore_census.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` (about ten
minutes); `resident_listing.py <dsun> 1000:0000..1000:0160 1000:1D4A..1000:1FA0
1000:1F97..1000:2010 2707:02A8..2707:0420 2707:03D0..2707:0400 2707:0480..2707:0490
2707:0590..2707:0700`;
`overlay_listing.py <dsun> 170 57CB0 58023`; `direct_callers.py <dsun> 2707:02A8 2707:0395`.
Outputs are kept under `GAME_DIR/analysis/rng-ds-20261020/`. Nothing was executed.

---
id: FND-PARTY-129
title: A reach walk from the 24 openings of display mode 2, stopped where a window leaves it through overlay 182 +19F8, reaches the script interpreter and the experience award's level gain only through overlay 182 +15D7 while DS:0DAB is 4
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00069E38..0x00069F1A
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008A09E..0x0008A154
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, overlay_listing.py, q065_mode2_window_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `direct_callers.py <dsun> 182+1856` finds 24 far calls of overlay 182 `+1856`, which
enters display mode 2 (FND-PARTY-124), in overlays 172, 175, 178, 181, 184, 187, 190, 192, 199,
201, 202, 203, 204, 209, 211 and 212.

The query `q065_mode2_window_reach.json` starts a walk at the instruction after each of the 24
calls. Its targets are the script interpreter `172C:000C` and the four calls of the level gain:
overlay 173 `+07AD`, overlay 188 `+16E9`, and overlay 190 `+1444` and `+144C` (FND-PARTY-081). Its
control is the message window's call of overlay 182 `+19F8` at file `0x00059BFB`. It declares the
exit worker `1000:0388` and overlay 182 `+19F8` as leaves, and `+19F8` as a routine that does not
return, so that no walk reads past a call of it.

The walk reads 1,171 routines and 94,558 instructions and reaches the control. It reaches the
interpreter and overlay 188 `+16E9`, and not overlay 173 `+07AD` or overlay 190 `+1444` or
`+144C`. Both reached targets have a shortest route that starts after overlay 187's call at
`+0F0A` and enters overlay 182 `+15D7` and then, at its near call at `+16C7`, overlay 182
`+0365`. The route to the interpreter goes on through `31E0:0121`, `1695:01D1`, `1695:0435` and
`1695:04A9`. Every route the walk reads to overlay 188 `+16E9` passes overlay 173 `+22C4` and
`+30C7` and overlay 188 `+1604`. 37 computed calls and jumps stay unresolved, all in resident
code (file `0x5C37` to `0x3FDE2`), so the walk reports that its negative result is not usable.

In overlay 182 `+15D7`, the code from `+168F` reaches the call at `+16C7` only when the word at
`DS:0DAB` is 4 (`+168F..+16CA`); the branch that stores 4 there, at `+160C`, ends at `+168C`
(`+15E8..+168F`).

Without `+19F8` as a leaf, the shortest route to the interpreter starts after overlay 201's call
at `+06A0` and reaches `1695:02E7` at `+098F`. Overlay 201 calls `+19F8` at `+08DE`, earlier on
that route (`+08DE..+0994`).

## Interpretation

On the routes the walk reads, nothing a window does between entering mode 2 and its call of
overlay 182 `+19F8` runs a script or a level gain, except the region load of overlay 187, which
reaches both through `+15D7` in mode 4. A gain run there goes through overlay 199 `+0C21` to
`+19F8` with `DS:0DAB` at 4, which FND-PARTY-123 shows returns after its first part. A script
that overlay 201 runs through `1695:02E7` runs after that window has returned to mode 1.

## Alternatives

- The 37 unresolved transfers include the near call through `[bx + 0x30A]` in the routine at file
  `0xC64F`, next to the interpreter, and the far calls through memory at `DS:3F42` and `DS:3F46`;
  a route through one of them is not read.
- Each walk starts inside the window's own routine and ends where that routine returns. A window
  that returns to its caller while mode 2 holds leaves the caller's following code unread.
- A window opened while `DS:0DAB` is 0 does not enter mode 2 (FND-PARTY-124), so the walk covers
  more than mode 2 for those calls.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 182+1856`,
`overlay_listing.py <dsun> 182 69E27 69F1C` and `overlay_listing.py <dsun> 201 89E00 8A160`. In
the repository root run `python -I tools/research/exec-census/reach_config.py <dsun>
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q065_mode2_window_reach.json` and then `python -I -m
scientific_method_engine reach <config>`. For the walk without the leaf, remove the `182+19F8`
leaf and `noReturn` entry from the query first.

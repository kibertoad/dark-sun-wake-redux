---
id: FND-PARTY-262
title: Overlay 206 +0E1E returns before its unload calls for a slot below 4, and the DUAL window's handler overlay 209 +169C also reaches the unload 31E0:05BB through overlay 179 +1373, overlay 193 +003C, 371D:050F and overlay 206 +0000
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008F76E..0x0008F9C3
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:050F..371D:0770
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 371D:06F0..371D:0719
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, resident_listing.py, overlay_listing.py, q026_mode2_handlers_release_reach.json)
environment: null
---

## Observation

This supersedes FND-PARTY-140. The overlay-206 and retained reach-walk
observations below are preserved; the resident unload gate is corrected by
a complete entry-based reading. No new reach walk or original run is claimed.

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. **Overlay 206 `+0E1E`** takes its slot argument as `SI`. Every path from its start
reaches the test at `+0EAB`; for a slot below 4 it goes on to `+0EB3`, calls overlay 190 `+26F8`
at `+0F4E` among others, and jumps at `+0F54` to its return at `+106F`. Its calls of `31E0:05BB`
at `+1023` and `+1067`, with word `+1` of the slot's record at `DS:67BB`, come only after the
branch for a slot of 4 or above (`+0E1E..+1073`).

FND-PARTY-139's walk from overlay 209 `+169C` alone, with `31E0:05BB` as the only target, was run
again with overlay 187 `+1B67` and overlay 206 `+0E1E` cut, and then each time with the routine
that calls `31E0:05BB` on the shortest route also cut. The routines so found, in order:

- overlay 179 `+1373`, which calls it at `+1A17` with the entry of the slot in `DI`;
- overlay 193 `+003C`, which calls it at `+0258` and `+0334`;
- **`371D:050F`**, which takes its first argument as `SI` (`371D:050F..371D:0538`) and calls it at
  `371D:0714` with the entry of slot `SI` when its removal count at BP-2 is
  zero OR its second-argument byte is nonzero (`371D:06F0..371D:0719`); `direct_callers.py` finds it called from overlay 193 `+05B6` and at
  `371D:04C2`;
- overlay 206 `+0000`, which calls it at `+01F6` with the entry of the slot in `DI`.

With those four also cut the walk no longer reaches `31E0:05BB`; it leaves 34 computed transfers
unresolved. The shortest route to each of the four runs from overlay 209 `+169C` through `+17DC`, overlay 184
`+2107`, overlay 191 `+05DB`, the object placement `31E0:0121` and overlay 193 `+0DC7` and `+12A8`.

## Interpretation

At 371D:06F0 a zero removal count jumps directly to the unload-argument
setup. Only a nonzero count tests the second argument; zero there selects
the alternative redraw/deferred-record path at 071E. With that byte zero,
the unload request is therefore selected exactly when the local count is
zero. The second argument zero does not exclude this request.

The local count starts at zero. The object-record byte +0F minus one is
computed unsigned into DI; byte zero produces FFFF and skips the scan,
leaving count zero and reaching the request even with the second argument
zero. A scan that removes no matching row has the same result. For the
zero-argument route, the local removal cap is ten; actual row/table and
callee preservation remain prerequisites, not a native reachability proof.
The unload arguments are object slot SI's word +1, one, one, one on this
zero-byte branch. With a nonzero second byte the third argument is zero.
No own slot-below-four guard protects this resident entry.

Overlay 193 +05B6 passes zero, as FND-PARTY-141 originally noted, so that
call can request unload under the zero-count condition. FND-ITEM-062
identifies a conditional canonical D=42 source path to it. Its actual area
list, target state and unload child effects are not established here.
This correction supersedes the zero-argument exclusion used by the old
DUAL slot-lifetime conclusion, without proving a native party-slot release.


Overlay 206 `+0E1E` handles a party member's slot without unloading its object, so it frees no
party slot. On the routes the walk reads from the DUAL window's handler, a party slot can be
freed only by one of the four routines above, whose shortest routes all pass overlay 193 `+12A8`
after an object is placed. Which slot each one unloads there was not read.

## Alternatives

- The 34 unresolved transfers leave the walk's negative result unusable: another caller of
  `31E0:05BB` may be reached through one of them.
- The slot that overlay 179 `+1373`, overlay 193 `+003C`, `371D:050F` and overlay 206 `+0000`
  unload on these routes, and the conditions on the route from overlay 209 `+169C` to
  `31E0:0121`, were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 206 8F76E 8F9C3`,
`resident_listing.py <dsun> 371D:050F..371D:0730` and `direct_callers.py <dsun> 371D:050F`. In
the repository root, set `q026_mode2_handlers_release_reach.json`'s starts to `209+169C`, its
targets to `31E0:05BB` and its controls to none, add `187+1B67` and `206+0E1E` to `leaves` and
`noReturn`, and run `python -I tools/research/exec-census/reach_config.py <dsun>
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python -I -m scientific_method_engine
reach <config>`; add the routine before `31E0:05BB` on the shortest route to `leaves` and
`noReturn` and run again until the target is not reached.

For the correction, use installed DSUN.EXE length 634416, XXH3-128
e296af55ba2ecde7e77f555c90f33d0b with resident_listing.py at 29f1ce0a,
range 371D:050F..371D:0770. The complete body is retained as
placement-area-target-child.txt under GAME_DIR/analysis/item-removal-20261104/.
Follow the zero initialization, early FFFF branch, count increments and
06F0/06F6 gates to 0714; no original is executed or committed.

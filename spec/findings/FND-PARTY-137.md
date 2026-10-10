---
id: FND-PARTY-137
title: A reach walk from the DUAL routine overlay 209 +123E reaches the object slot release 2D40:03C1 only through the view draw's object load, and not at the party-management releases of overlays 175, 184, 187 and 189
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0F18..31E0:0F4E
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, resident_listing.py, q026_dual_slot_release_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `direct_callers.py <dsun> 2D40:03C1` finds the slot release (FND-PARTY-136) called
at file `0x00027556`, `0x00027CA2` and `0x00027F48` (`31E0:0556`, `31E0:0CA2`, `31E0:0F48`), near
at `2D40:31E6`, and from overlay 175 `+0F06`, overlay 184 `+1F8D` and `+2093`, overlay 187
`+15C3`, overlay 188 `+0D8E` and `+12C3`, and overlay 189 `+2B5A`.

The query `q026_dual_slot_release_reach.json` starts at the DUAL routine overlay 209 `+123E`
(FND-PARTY-080) with the release and the ten call sites other than overlay 188 `+0D8E` as
targets, the exit worker `1000:0388` as a leaf and no control. The walk reads 993 routines and
82,232 instructions and leaves 34 computed transfers unresolved. It reaches the release and the
calls at `31E0:0556`, `31E0:0CA2`, `31E0:0F48`, overlay 188 `+12C3` and `2D40:31E6`, and not those
in overlays 175, 184, 187 and 189. Every route to each passes `2C5F:0139`, the view draw `31BA:000E` and `31E0:0EFF`; the shortest pass the
message window's call of overlay 182 `+1856` at overlay 172 `+064F`. The routes to `31E0:0556`
also pass `31E0:05BB` and `31E0:044C`, and those to `31E0:0CA2` pass `31E0:0D09` and `31E0:0B4A`.

**`31E0:0F48`** frees `SI` when `38FF:04AB`, asked for the object's `OJFF` resource, returns a
value other than 0, before any slot is taken; `SI` is the third argument, or 0x13F when that is -1 (`31E0:0F18..31E0:0F4E`), and the
view draw passes -1 (FND-PARTY-135).

## Interpretation

On the routes the walk reads, the DUAL path frees object slots only inside the view draw's object
load: `31E0:0F48` frees slot 0x13F, and `31E0:0556` and `31E0:0CA2` lie below the eviction loop,
which handles slots of 0x30 and above (FND-PARTY-135). None of those frees a party slot, so they
do not put one on a free list for a load to reuse (FND-PARTY-136). Overlay 188 `+12C3` and
`2D40:31E6` were reached through other routines below `31E0:0EFF`, and the slots they free were
not read.

## Alternatives

- The walk has no control and 34 unresolved transfers, so its negative results are not usable:
  the party-management releases may be reached through a computed transfer.
- The routes to overlay 188 `+12C3` pass overlay 188 `+0F27` and `+1184`, and the routes to
  `2D40:31E6` pass `2D40:2F34`; which slot each frees was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `direct_callers.py <dsun> 2D40:03C1` and
`resident_listing.py <dsun> 31E0:0EFF..31E0:0F51`. In the repository root run `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q026_dual_slot_release_reach.json` and `python -I -m
scientific_method_engine reach <config>`.

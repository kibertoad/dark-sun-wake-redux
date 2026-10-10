---
id: FND-PARTY-134
title: Every route a reach walk reads from the message window overlay 172 +05DB to the experience award overlay 188 +1604 passes the view draw's object load and a call of overlay 193 +1DB0 or 371D:0002 made only for an object slot whose state byte is 2
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:0499
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0C68..31E0:0C96
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, resident_listing.py, q026_message_award_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. The query `q026_message_award_reach.json` starts at overlay 172 `+05DB`, with overlay
188 `+1604` as target, the call of overlay 182 `+19F8` at file `0x00059BFB` as control and the
exit worker `1000:0388` as a leaf. The walk reads 927 routines and 76,448 instructions, reaches
the control, leaves 34 computed transfers unresolved, and reaches the target. Every route it
reads passes overlay 182 `+1856`, `2C5F:0139`, the view draw `31BA:000E`, `31E0:0EFF`, overlay
173 `+22C4` and `+30C7`.

Two more walks from the same query, each with one more leaf:

- With `31E0:0B4A` a leaf, every route also passes `31E0:05BB` and `31E0:044C`, and the shortest
  goes on to `371D:0002` at `31E0:0491`.
- With `371D:0002` a leaf, every route also passes overlay 193 `+1DB0` (trampoline `573B:002F`).

`direct_callers.py <dsun> 193+1DB0 371D:0002` finds overlay 193 `+1DB0` called only at
`31E0:0483` and `31E0:0C7E`, and `371D:0002` at `31E0:0491` and `31E0:0C8E` and from overlay 187
`+1186` and overlay 193 `+1943` and `+19EA`. **`31E0:044C`** makes its calls at `31E0:0483` and
`31E0:0491`, with its slot argument, only when the byte at `DS:265B`, the byte at `4F49:0C33`
plus 3 times the slot, which must be 2, and its second argument are all set
(`31E0:044C..31E0:0499`). **`31E0:0B4A`** makes its calls at `31E0:0C7E` and `31E0:0C8E` only
when that slot byte is 2 (`31E0:0C68..31E0:0C96`, FND-PARTY-130).

## Interpretation

On the routes the walk reads, the message window gives experience only through the view draw's
loading of an object, and only where that load works on an object slot whose state is 2, which
marks a party member's slot (FND-PARTY-131). The view draw loads only unloaded entries, and no
referenced code unloads a party member's entry (FND-PARTY-132). So on the DUAL path a message does
not lead to an award that would run a level gain and change the word at `4E71:0B44`, unless the
slot `31E0:0EFF` hands on for an unloaded object can be a party member's.

## Alternatives

- `371D:0002` is also called from overlay 187 `+1186` and overlay 193 `+1943` and `+19EA`; with
  `31E0:0B4A` a leaf, no route passed them, but with other leaves one might.
- Which slot `31E0:0EFF` passes to `31E0:0D09`, `31E0:05BB` and `31E0:044C` when it loads an
  unloaded entry was not read.
- The 34 unresolved transfers leave the walk's negative results unusable.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in the repository root run `python -I tools/research/exec-census/reach_config.py
<dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q026_message_award_reach.json` and `python -I -m
scientific_method_engine reach <config>`, then again with `31E0:0B4A` and then `371D:0002` added
to `leaves`. In `tools/research/exec-census/`, run `direct_callers.py <dsun> 193+1DB0 371D:0002`
and `resident_listing.py <dsun> 31E0:044C..31E0:04A0`.

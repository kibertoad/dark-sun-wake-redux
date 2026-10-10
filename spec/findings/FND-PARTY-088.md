---
id: FND-PARTY-088
title: From the calls that end a fight, a reach walk finds the stores to 4E71:0B44 and DS:142D only inside the level gain's overlay 209 routines, with 34 computed transfers unresolved
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005B0AD..0x0005B0C4
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, store_values.py, trampoline_target.py, q043_fight_end_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Before the level gain at overlay 173 `+07AD`, the end of a fight calls overlay 173
`+3E30`, overlay 195 through `574E:0052`, overlay 206 through `5799:0039` and overlay 173 `+3CDA`
(`+078D..+07A4`, FND-PARTY-089). `trampoline_target.py` gives `574E:0052` as overlay 195 `+14D5`
and `5799:0039` as overlay 206 `+0B49`.

`store_values.py <dsun> B44`, keeping the stores whose printed instructions load the selector
`0x370` (segment `4E71`), lists eleven stores to `4E71:0B44`, at file `0x58342`, `0x6D874`,
`0x75CBB`, `0x78933`, `0x7BA2D`, `0x8A743`, `0x93386`, `0x93BF2`, `0x9316F`, `0x9614A` and
`0x98828`. `store_values.py <dsun> 142D` lists stores to `DS:142D` at `0x6D88B`, `0x78990`,
`0x7BA67`, `0x8A737`, `0x933C0`, `0x93C2C`, `0x96141` and `0x98839` (FND-PARTY-084).

The reach walk starts at those four routines, with the nineteen stores as targets, the call at
file `0x5E71A` in `+3CDA` as the control and the exit worker `1000:0388` as a leaf. It reads 931
routines and 76,818 instructions, reaches the control, and reaches five targets: `0x9316F`,
`0x93386`, `0x933C0`, `0x93BF2` and `0x93C2C`. Every route it reads to each passes through
overlay 173 `+22C4`, overlay 173 `+30C7`, overlay 188 `+1604` and overlay 210 `+0B44`, `+08BC` and
`+0740`, and then overlay 209 `+0000` or `+0A02`; the shortest enters through overlay 182 `+1D3E`,
which `+3CDA` calls, and the resident routine at file `0x236AE`. The other fourteen targets are
not reached. 34 computed calls and jumps stay unresolved, all in resident code (file `0x5C37` to
`0x3FDE2`), so the walk reports that its negative result is not usable.

## Interpretation

Before the level gain that follows a fight, the routines that end it set neither the slot at
`4E71:0B44` nor the far pointer at `DS:142D`, except through an experience award inside them that
runs the gain itself; so the wisdom the gain reads is that of the record `DS:142D` pointed to when
the fight ended, or the levelling character's after a new Psionicist level (FND-PARTY-084). Which
record that is depends on what set it before or during the fight. The routes through overlay 182
`+1D3E` to the damage routine show that the end of a fight can deal damage, and so give
experience.

## Alternatives

- A store reached only through one of the 34 unresolved computed transfers: the walk cannot rule
  it out, and the resident code they sit in was not read for those targets.
- A store to either through a segment register loaded more than six instructions before it: the
  search would not list it.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `store_values.py <dsun> B44`, keeping the hits
whose printed instructions load `0x370`, `store_values.py <dsun> 142D 142F` and
`trampoline_target.py <dsun> 574E:0052 5799:0039`. In the repository root run `python -I
tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q043_fight_end_reach.json` and then `python -I -m
scientific_method_engine reach <config>`.

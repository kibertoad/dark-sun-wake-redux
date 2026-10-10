---
id: FND-PARTY-090
title: From the fight routine overlay 173 +059A, a reach walk finds the stores to 4E71:0B44 and DS:142D only inside the level gain's overlay 209 routines, every route passing through overlay 210 +0B44, with 36 computed transfers unresolved
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005AEBA..0x0005AEC9
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, q043_fight_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 173 `+059A` is the fight routine whose result its resident callers store in the
fight state word at `4C10:0019` (FND-PARTY-089). The targets are the nineteen stores FND-PARTY-088
lists: eleven to `4E71:0B44` and eight to `DS:142D`.

The reach walk starts at `+059A`, with those targets, the call to `+3CDA` at file `0x5B0C1` as the
control and the exit worker `1000:0388` as a leaf. It reads 1,013 routines and 85,421
instructions, reaches the control, and reaches five targets, `0x9316F`, `0x93386`, `0x933C0`,
`0x93BF2` and `0x93C2C`. Every route it reads to each passes through `+059A` and overlay 210
`+0B44`, `+08BC` and `+0740`, and then overlay 209 `+0000` or `+0A02`. The other fourteen targets
are not reached. 36 computed calls and jumps stay unresolved, so the walk reports that its
negative result is not usable.

## Interpretation

Within one call of the fight routine, the slot at `4E71:0B44` and the far pointer at `DS:142D`
change only inside the level gain: for a new Preserver level (overlay 209 `+0000`) or a new
Psionicist level (overlay 209 `+0A02`, FND-PARTY-084). So when the gain runs as a fight ends, the
pointer is where code outside the fight routine left it, between its calls or before the fight,
or at the levelling character after a new Psionicist level earlier in the same gain.

## Alternatives

- A store reached only through one of the 36 unresolved computed transfers: the walk cannot rule
  it out.
- A store through a register holding the address: FND-PARTY-084's searches cover direct-address
  stores and immediates only.
- Which code outside the fight routine runs between its calls during a fight, and so which of the
  stores FND-PARTY-084 lists can move the pointer during a fight, was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in the repository root `python -I tools/research/exec-census/reach_config.py
<dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config>
tools/research/exec-census/q043_fight_reach.json` and then `python -I -m scientific_method_engine
reach <config>`.

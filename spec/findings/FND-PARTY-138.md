---
id: FND-PARTY-138
title: Of the object slot releases, only the unload 31E0:05BB frees a party member's slot, and the DUAL routine reaches it only through overlay 187 +1B67, which unloads slots 5 to 0x13F; overlay 171 +0D15, which unloads the current character, is reached from seven mode-2 handlers but not from the message window's
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2F34..2D40:2F44
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:31E2..2D40:31F6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007405B..0x00074168
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0006F016..0x0006F128
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000712DD..0x000713F8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000609F8..0x00060AEB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000773C0..0x0007750F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:044C..31E0:055B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:05BB..31E0:05E6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:09A8..31E0:0ACC
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00071997..0x00071A17
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00058F25..0x00058FDE
tool: Python 3.14.7 with Capstone 5.0.9, pypcode 4.0.1 and xxhash 4.0.1 (scientific_method_engine reach; tools/research/exec-census/reach_config.py, direct_callers.py, immediate_search.py, resident_listing.py, overlay_listing.py, q026_dual_unload_reach.json, q026_mode2_handlers_release_reach.json)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-137 lists the callers of the slot release `2D40:03C1`. `2D40:0453` pops a
slot from the list at `DS:56F1` and `2D40:0C31` from the list at `DS:56EF`; `immediate_search.py
<dsun> 56EF` finds that list filled only by `2D40:0C92`, for slots 0x140 to 0x207, and cleared by
overlay 188 (FND-PARTY-136). What each release frees:

- **`2D40:31E6`** frees the argument of `2D40:2F34`, which jumps past the release to its return
  when the argument is below 0x30 (`2D40:2F3B`). Above that it takes a slot from `DS:56ED`, copies
  the argument's 0x25-byte record into it, frees the argument and returns the new slot
  (`2D40:2F34..2D40:2FB3`, `2D40:31E2..2D40:31F6`).
- **Overlay 188 `+12C3`** frees `SI`, set at `+11C0` from `2D40:0453` or at `+11CC` from
  `2D40:0C31`, and left as it is up to the release (`+11BB..+12C8`).
- **Overlay 184 `+1F8D` and `+2093`** free `DI`, a slot from `2D40:0453` that the loop takes at
  `+1F93` and frees at the next pass and at the end (`+1F86..+2098`).
- **Overlay 187 `+15C3`** frees `SI` only when it is 4 or above (`+15BD`). For a pass below 4, `SI`
  is the pass number; above, it is a slot from `2D40:0453` (`+14AD..+15C8`).
- **Overlay 175 `+0F06`** and **overlay 189 `+2B5A`** free a slot from `2D40:0453` that the routine
  takes at `+0E18` and `+2A10` and does not change before the release (`+0E18..+0F0B`,
  `+2A10..+2B5F`).
- **`31E0:0CA2`** frees the argument of `31E0:0B4A`, which only the eviction loop calls, at
  `31E0:0DE3` (FND-PARTY-135).
- **`31E0:0556`** frees `DI`, the first argument of `31E0:044C`, unless the byte at `DS:265B` is 0
  (`31E0:044C..31E0:055B`). `31E0:044C` is called only at `31E0:0AC9`, in the unload `31E0:05BB`,
  with `SI`. `31E0:05BB` sets `SI` to word `+6` of the entry its argument names in the table at
  `DS:67B7`, the entry's slot, and does not change it (`31E0:05BB..31E0:05E6`). For a slot whose
  state byte is 2 it calls `25AF:071F` with the slot when it is below 4, and goes on to the call
  of `31E0:044C` (`31E0:09A8..31E0:0ACC`).

`q026_dual_unload_reach.json` is FND-PARTY-137's query with `31E0:05BB` added as a target, the
message window's call of overlay 182 `+19F8` at file `0x00059BFB` as control, and the eviction
loop `31E0:0D09` cut as a leaf that does not return. The walk reads 992 routines and 81,951
instructions, reaches the control, and leaves 34 computed transfers unresolved. It reaches
`31E0:05BB`, `31E0:0556`, `31E0:0F48`, overlay 188 `+12C3` and `2D40:31E6`, and none of the other
releases. Every route to `31E0:05BB` passes `31E0:0EFF`, `2D40:000A` and overlay 187 `+1B67`.
**Overlay 187 `+1B67`** goes through slots 5 to 0x13F and calls `31E0:05BB` with the entry of
each slot whose state byte is not 0, that is not the slot in the word at `DS:157C`, whose entry's
kind (the low 3 bits of entry byte `+5`) is not above its first argument, and whose state byte
equals its second argument when that is not 0 (`+1B67..+1BE7`).

`q026_mode2_handlers_release_reach.json` starts at the 18 mode-2 handlers of FND-PARTY-133 with
the same targets and cut. The walk reads 1,261 routines and 107,486 instructions, reaches the
control, and leaves 52 computed transfers unresolved. It reaches every target except
`31E0:0CA2`, overlay 175 `+0F06` and overlay 189 `+2B5A`. The shortest route to `31E0:05BB` runs
from overlay 202 `+05CA` through overlay 190 `+0D89` to overlay 171 `+0D15`. **Overlay 171
`+0D15`** takes as `SI` the word at `4E71:0B44`, or word `+2` of a record at `DS:40C8`, calls
overlay 184 `+1446` with it, moves the leader word at `4C13:0369` to another filled slot from 0
to 3 when it was `SI`, and calls `31E0:05BB` with the entry of slot `SI` (`+0D15..+0DCE`).

Walking from each handler alone, with overlay 171 `+0DC9` (file `0x00058FD9`) as a target and no
control, reaches it from overlay 175 `+0B61`, 181 `+0000`, 189 `+0559`, 202 `+05CA`, 203 `+00EE`,
211 `+081B` and 212 `+0967`, every route through overlay 190 `+0D89`, and not from the message
window's handler overlay 172 `+04C5` or from the other ten.

## Interpretation

Every release except `31E0:0556` frees a slot of 4 or above: a slot taken from a list, a slot the
eviction handles, the slot 0x13F, or one that a test keeps at 0x30 or 4 and above. The unload
`31E0:05BB` frees whatever slot the entry it is given holds, so it frees a party member's slot when
it is given a party member's entry. The DUAL routine's own routes reach it only through overlay
187 `+1B67`, which starts at slot 5, so they free no party slot. Overlay 171 `+0D15` unloads the
current character's object and so frees a party slot, but the message window's handler does not
reach it; the windows whose handlers do are not shown to be open on the DUAL path.

## Alternatives

- The 34 and 52 unresolved transfers leave both walks' negative results unusable; among them are
  the calls through the handler slots `DS:A0F1`, `DS:A0F5` and `DS:A0F9`, which run whatever handlers
  are registered at the time.
- The branches of `31E0:05BB` between `31E0:05E6` and `31E0:09A8` were not read; some may return
  before the unload.
- Overlay 187 `+1B67` takes the entry from word `+1` of the slot record and `31E0:05BB` reads the
  slot back from the entry; that the two agree for each loaded object is assumed.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `immediate_search.py <dsun> 56EF`,
`direct_callers.py <dsun> 31E0:0B4A 31E0:05BB 31E0:044C`, `resident_listing.py <dsun>
2D40:2F34..2D40:31F7 2D40:0C31..2D40:0CC2 31E0:044C..31E0:05BB 31E0:05BB..31E0:0AFC` and
`overlay_listing.py` over the overlay ranges above (`overlay_listing.py <dsun> 188 74024 74170`
and so on). In the repository root run `python -I tools/research/exec-census/reach_config.py
<dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <config> <query>` and `python -I -m
scientific_method_engine reach <config>` for each of the two queries, then for the second with
one start at a time, `0x58FD9` as the only target and no control.

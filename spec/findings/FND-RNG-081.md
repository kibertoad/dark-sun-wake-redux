---
id: FND-RNG-081
title: Of the timer callbacks, 31E0:31A7 stores DS:265A, DS:145E, DS:1424 and 4C13:035B, overlay 196 +040B adds 1 to DS:21E0, and 4654:09A9 stores into segments 4E71 and 50C3 and calls driver function 0xC1
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:31A7..31E0:31F7
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4654:09A9..4654:0A96
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4842:0D72..4842:0D78
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008337B..0x00083384
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047103..0x00047109
tool: Capstone 5.0.9 over the load image; tools/research/exec-census/overlay_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Addresses are static; segment immediates in load-image code are relative to 0, so `0x1000` is
added. FND-EXE-585 names the callbacks `4842:0498` can call: `4654:09A9`, overlay 196 `+040B`,
`31E0:31A7`, and function `0067` of a driver loaded from an `ADV ` resource.

**`31E0:31A7`** stores 1 at `DS:265A`, adds 1 to the double word `DS:145E`, stores 1 at `DS:1424`
when the low three bits of `DS:145E` are then 0, and adds 1 to the double word `4C13:035B` when
its low six bits are 0, `DS:1440` is 1, the combat word `4C10:0019` is 0 and `DS:0DAB` is not 2.

**Overlay 196 `+040B`** (file `0x8337B`) adds 1 to the word `DS:21E0`.

**`4654:09A9`** adds `0x0A` to `4E71:0C15` and 1 to each of `4E71:0C19`, `0C1B`, `0C1D`, `0C20`
and `0C23`. When `4E71:0B54` is not 0 it runs its loop body once, for entry `0xF1` of the 6-byte
entries at `50C3:0D2D` (`SI` starts at `0xF1` and the loop ends when it is above `0xF1`): when the
entry's byte `+0` is 1 it subtracts `0x0A` from the word `+0x0B` of the record its far pointer at
`+2` names and, when that is no longer above 0, calls `4842:0D72` with that record's word `+0`
and byte `+2`, subtracts 1 from `4E71:0B54`, stores the record's word `+9` at its `+0x0B` and
`0x11` at its `+2`, and stores 0 in the entry's byte `+0`.

The entry's far pointer is `0000:40C3` in the file (offset `0x47105`), which is `50C3:0000`. A
search of the code for the operands `0xD2F` and `0xD31` finds only loads, so no instruction with
those displacements stores the pointer.

**`4842:0D72`** loads `0xC1` into `AX` and jumps to `4842:03F4`, the driver dispatch.

## Interpretation

The three callbacks in the program's own code write only `DS:145E..1462`, `DS:1424`, `DS:21E0`,
`DS:265A`, `4C13:035B`, `4E71:0B54..0C25`, entry `0xF1` of the table at `50C3:0D2D` and the record
at `50C3:0000`, none of them in the clock conversion's windows (Q-RNG-005). What the driver's
function `0xC1` and its timer function `0067` write is left.

## Alternatives

The search for `0xD2F` and `0xD31` covers only those displacements; a block copy over the table
would not show. The driver code is loaded from a resource and was not read here.

## How to reproduce

Disassemble `31E0:31A7..31F7`, `4654:09A9..0A96` and `4842:0D72..0D78` from the load image;
`overlay_listing.py <dsun> 196 8337B 83384`; read 6 bytes at file offset `0x47103`;
`immediate_search.py <dsun> 0D2F` and `... 0D31`.

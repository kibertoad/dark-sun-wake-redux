---
id: FND-PARTY-168
title: A script's function 23 adds its effect with the duration -9999, and overlay 195 +147C returns 1, so an effect's count at +0x255 falls by 1 a use and never passes below 0
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DE17..0x0008DE45
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00082BEC..0x00082C45
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`.

**Overlay 204 `+2057`**, the target of a script's function 23 of opcode `0x22` (FND-PARTY-108),
returns 0 when its dword argument at `bp+8` is 113 or more. Otherwise it calls overlay 193
`+166F` with its first argument, its argument at `bp+0xC`, -1, the low byte of `bp+8` as the
effect code, the dword -9999 (`0xFFFFD8F1`) and 0, and returns 1 (`+2057..+2084`).

**Overlay 195 `+147C`**, whose result overlay 193 `+1AA4` subtracts from an effect's byte at
`+0x255` (FND-PARTY-167), draws with `28C9:391D` and the arguments 2 and 4. When its second
argument is 1 it calls overlay 193 `+1A2D` and then overlay 179 `+0C7F` with the slot twice, the
draw and 0x63. On both paths it returns 1 in `al` (`+147C..+14D4`).

## Interpretation

Every source of effect 59 now read gives a duration of -9999 or a count of 0: the spells
`LEVEL DRAIN` and `ENERGY DRAIN` (FND-PARTY-167), the item of `DATA` 313 (FND-PARTY-096) and a
script's function 23. A duration of -9999 gives a count of 15. `+1AA4` passes over a count of 0
and subtracts 1 otherwise, and overlay 195 `+0558` raises a count only while it is below 4, so
the count stays from 0 to 15. The term of overlay 197 `+026F` then takes at most 30 off the save
modifier, which cannot wrap a `CHAOS` save back below the save (FND-PARTY-167).

## Alternatives

- A write of the byte from outside the inventoried code, or through a copy of the effect record,
  is outside the store search of FND-PARTY-167.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `overlay_listing.py <dsun> 204 8DE17 8DE45` and
`overlay_listing.py <dsun> 195 82BEC 82C45`.

---
id: FND-PARTY-102
title: Overlay 179 +0C7F passes its item number to the hit routine +0D2F, and its far callers pass -1 from the overlay 173 swing and from overlays 195 and 204 and the DATA numbers 100, 79, 221 and 18 from overlay 193, leaving overlay 193 +145E and overlay 204 +130E with computed numbers
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064F5F..0x00065005
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005D190..0x0005D1CE
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 179 `+0D2F` applies the effect code at `+0x19` of `DATA` `i` when `i` is not
-1 (FND-PARTY-101). `direct_callers.py` finds it called from overlay 179 `+0D1A` and `+1299` and
overlay 193 `+04DE`.

**Overlay 179 `+0C7F`** (trampoline `56A7:0066`) takes a slot, a second slot `d` and a number `i`,
calls the overlay 193 routine of trampoline `573B:0057` with `d` and 0x35 and with `d` and 0x36
when `i` is not -1 and the low two bits of `DATA` `i`'s byte `+0` are 0 or 2, or when its argument
at `bp+0xE` is above 0, and then calls `+0D2F` with its arguments in the same order, `i` third
(`+0C8D..+0D1A`). Its 16 far calls push as `i`, the third word before each call:

| Caller | `i` |
| --- | --- |
| overlay 173 `+28A9` | -1 |
| overlay 193 `+1170`, `+11A0`, `+11D0` and `+1200` | 100 |
| overlay 193 `+123D` | 79 |
| overlay 193 `+129E` | 221 |
| overlay 193 `+145E` | the routine's argument `bp+8`, in `+13EC` |
| overlay 195 `+0242`, `+05BC`, `+0615`, `+066E` and `+0733` | -1 |
| overlay 195 `+14C8` | 18 |
| overlay 204 `+130E` | the routine's argument `bp+6`, in `+12B4` |
| overlay 204 `+150F` | -1 |

`data_bytes.py <install>/RESOURCE.GFF 19` gives `DATA` 100, 79, 221 and 18 a byte other than 59 at
`+0x19`. `direct_callers.py` finds overlay 193 `+13EC` called near from `+1359` and `+14A0`, and
overlay 204 `+12B4` called near from seven sites, `+0F94` to `+100A`.

## Interpretation

The fight's swing at overlay 173 `+28A9` passes no item to the hit routine, so a weapon's `DATA`
byte `+0x19` takes effect only through the other routes. None of the fixed numbers names
`DATA` 104 or `DATA` 225, so a drain through `+0C7F` needs overlay 193 `+13EC` or overlay 204
`+12B4` to pass one of those numbers.

## Alternatives

- Overlay 193 `+13EC` and overlay 204 `+12B4` pass `DATA` 104 or 225: their callers were not read.
- The two other routes into `+0D2F`, through `+11DC` (from `+0D25` and overlay 193 `+0032`) and
  through overlay 193 `+003C` (from overlay 179 `+125C` and overlay 193 `+0029`), were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 179 64F5F 65005` and
`173 5D190 5D1D0`; `direct_callers.py <dsun> 179+0C7F 179+11DC 193+003C 193+13EC 204+12B4`,
reading the pushes before each far call of `179+0C7F`; and `data_bytes.py
<install>/RESOURCE.GFF 19`.

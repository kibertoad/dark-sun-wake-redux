---
id: FND-PARTY-103
title: Overlay 204 +12B4's callers pass the DATA numbers 148, 157, 159 and 178 to the hit routine, and overlay 193 +13EC passes the word at +0 of an 18-byte record of the table at 51F1:0000, which overlay 193 +12A8 indexes by its second argument
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008CD41..0x0008CDD0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F598..0x0007F6D9
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007F77E..0x0007F798
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/overlay_listing.py, direct_callers.py, field_stores.py, data_bytes.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. FND-PARTY-102 leaves overlay 204 `+12B4` and overlay 193 `+13EC` passing a computed
`DATA` number `i` to the hit routine through overlay 179 `+0C7F`.

**Overlay 204 `+12B4`** passes its first argument as `i`. `direct_callers.py` finds only seven
near calls of it, `+0F94` to `+100A`, which push as that argument the immediates 0x94, 0x9D,
0x9F, 0x94, 0x9D, 0x9F and 0xB2 (`+0F81..+100D`). `data_bytes.py <install>/RESOURCE.GFF 19` gives
the byte at `+0x19` of `DATA` 148 as -27, of `DATA` 157 and 159 as 0, and of `DATA` 178 as -71.

**Overlay 193 `+13EC`** passes its second argument as `i`. It is called near from `+1359` and from
`+148E`, a wrapper (trampoline `573B:00D9`) that passes its own second argument and is called near
from `+13B8`. `+1359` and `+13B8` lie in **`+12A8`**, which takes a value and an index `s`,
returns when `s` is 0xFF or -1, reads the 18-byte record `s` of the table at `51F1:0000`
(segment word `0x3D0`, descriptor 122), and passes the record's word at `+0` as the second
argument at both calls (`+12AC..+13B8`). `field_stores.py` with `--es 0` finds no store through
ES at displacement 0 whose preceding instructions load the segment word `0x3D0` together with the
record size 0x12; overlay 208 `+03BA` forms a far pointer with the segment word `0x3D0` in locals
before storing through it at `+040A`.

## Interpretation

Through `+0C7F`, only the records of the table at `51F1:0000` can bring an item whose `DATA` byte
`+0x19` is 59 to the hit routine: every other caller passes -1 or a fixed number whose byte is
not 59. Which `DATA` numbers that table holds depends on what fills it at run time.

## Alternatives

- What fills the table at `51F1:0000`: its writers form far pointers, and overlay 208 `+03BA` was
  not read, nor its other writers searched for through pointers kept in locals.
- The two other routes into the hit routine, through overlay 179 `+11DC` and overlay 193 `+003C`,
  were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `overlay_listing.py <dsun> 204 8CD30 8CDD0`,
`193 7F598 7F6AE` and `193 7F770 7F798`; `direct_callers.py <dsun> 204+12B4 193+13EC 193+148E`;
`field_stores.py <dsun> ../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv --es 0`, keeping the hits
whose printed instructions name `0x3d0`; and `data_bytes.py <install>/RESOURCE.GFF 19`.

---
id: FND-PARTY-159
title: No shipped DATA record that allows a saving throw names save 6 or 7, so the saving throw never reads details bytes 0x36 and 0x37
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00066DBA..0x00066E30
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/data_bytes.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 179 `+2A46` returns 0 when bit 0 of byte `+0x1F` of `DATA n` is clear, takes
`k`, bits 5 to 7 of that byte, returns 0 when `k` is 0, and reads the byte at `0x30 + k` of the
target's details record (FND-PARTY-099, `+2ADA..+2B50`).

`data_bytes.py <RESOURCE.GFF> 1F` lists the byte at `+0x1F` of all 323 `DATA` resources of the
installed `RESOURCE.GFF`. The values are -127 (5 records), -96 (125), -95 (70), -91 (1), -85 (1),
-71 (2), -67 (4), 0 (85), 1 (2), 28, 32, 45, 60 and 70 (1 each), 33 (20) and 97 (3). Of the 109
values with bit 0 set, `k` is 0 for 2, 1 for 21, 3 for 3, 4 for 5 and 5 for 78; none has `k` of 2,
6 or 7.

## Interpretation

The saving throw reads only the details bytes `0x31`, `0x33`, `0x34` and `0x35` for the shipped
effects, never `0x36` (CHAR byte `0x7B`, `unk_7b`) or `0x37` (CHAR byte `0x7C`, `footprint`). The
open case of FND-PARTY-099, that save numbers 6 and 7 would read those bytes, does not arise with
the shipped data.

## Alternatives

- `DATA` records that only a script or a saved game supplies were not searched; none is known.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `data_bytes.py <RESOURCE.GFF> 1F` and, for each
value with bit 0 set, take bits 5 to 7; run `overlay_listing.py <dsun> 179 66DBA 66E30`.

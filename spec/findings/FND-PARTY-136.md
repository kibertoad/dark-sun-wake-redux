---
id: FND-PARTY-136
title: The start-up code of overlay 188 puts object slots 4 to 0x2F on the free list at DS:56ED and frees 0x30 to 0x13E, never the party slots 0 to 3, and 2D40:03C1 is what returns a slot below 0x30 to that list
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00073BC6..0x00073C65
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:03C1..2D40:03DB
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/immediate_search.py, overlay_listing.py, resident_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. `2D40:0461` hands out object slots from the lists whose heads are the words at
`DS:56ED` and `DS:56F1` (FND-PARTY-135). `immediate_search.py <dsun> 56ED 56F1` finds them used in
`2D40:03C1`, `2D40:0453`, `2D40:0461`, `2D40:0C3F`, `2D40:0CC2` and `2D40:2F34`, and in overlay
188 at file `0x00073194`, `0x00073BCC`, `0x00073BD2` and `0x00073C53`.

**Overlay 188 `+0D20..+0DC5`** stores 9999 in the words at `DS:56EF`, `DS:56F1` and `DS:56ED`,
fills 0x390 bytes at `4F49:08A3` and the 0x624 bytes of the slot state bytes at `4F49:0C33` through
`1000:3FA2`, stores 0xFF at `+0x12` and `+0x11` of each of the 48 records of 0x13 bytes at
`4F49:08A3`, calls `2D40:03C1` for each slot from 0x30 to 0x13E, `2D40:0C92` for each from 0x140
to 0x207, and `2D40:0CC2` with each slot from 4 to 0x2F and `DS:56ED`.

**`2D40:03C1`** takes a slot. It starts with `DS:56F1` as its list; for a slot below 0x30 it
calls `2D40:0BA4` with the slot, takes `DS:56ED` instead and clears the slot's 0x13-byte record at
`4F49:08A3` (`2D40:03C1..2D40:03DB`).

## Interpretation

After start-up the party slots 0 to 3 are on neither list, so an object load takes one of them
from `2D40:0461` only after `2D40:03C1` has freed it. A load into a party slot is what FND-PARTY-134
needs for the message window to reach the experience award, so on the DUAL path that needs a
party member's slot to be freed first.

## Alternatives

- `2D40:0C92`, `2D40:0CC2`, `2D40:0BA4` and the rest of `2D40:03C1` past `2D40:03DB` were not read;
  which list each puts a slot on is read from the arguments only.
- The callers of `2D40:03C1`, and whether any runs on the DUAL path for slots 0 to 3, were not
  read.
- A saved game may restore the lists; the load of a save was not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `immediate_search.py <dsun> 56ED 56F1`;
`overlay_listing.py <dsun> 188 73BC0 73C70`; and `resident_listing.py <dsun>
2D40:03C1..2D40:03F0`.

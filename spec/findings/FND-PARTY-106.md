---
id: FND-PARTY-106
title: The table at 51F1:0000 that overlay 193 +12A8 passes to the hit routine is 112 records of 18 bytes loaded from the executable and never written, and none of their DATA numbers is 104 or 225
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x00047110..0x000478F0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00091C4A..0x00091E50
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/segment_references.py, overlay_listing.py)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. Overlay 193 `+12A8` passes the word at `+0` of record `s` of the 18-byte records at
`51F1:0000` to the hit routine as its `DATA` number (FND-PARTY-103).

Segment-table descriptor 122 names segment `51F1` with `0x08BE` bytes in the load image, so its
bytes start at file `0x00047110`. The 112 records of 18 bytes fill `0x0000..0x07E0`, and overlay 209
reads 3-byte records from `0x07E0` (FND-PARTY-104). The 112 words at `+0` of the records are, in
order:

4, 7, 11, 15, 16, 1, 14, 21, 22, 24, 26, 27, 29, 32, 34, 36, 40, 41, 43, 45, 49, 50, 54, 55, 56,
57, 58, 66, 67, 115, 116, 119, 155, 173, 180, 199, 202, 198, 311, 278, 282, 279, 306, 309, 285,
69, 70, 71, 75, 83, 85, 86, 93, 94, 99, 100, 90, 102, 105, 107, 108, 109, 110, 111, 112, 113,
114, 130, 135, 137, 140, 145, 154, 160, 161, 181, 186, 187, 188, 164, 205, 206, 208, 209, -1, 214,
216, 221, 222, 223, 226, 227, 228, 229, 230, 231, 232, 233, 234, 89, 286, 289, 290, 291, 292,
293, 294, 295, 296, 299, 300, 302.

`segment_references.py` finds 72 words naming the segment: the descriptor's own entry in the
segment table, 66 loads of it into a general register, and five stores of it into a local word,
in overlay 193 `+0664` and overlay 208 `+00DD`, `+03F8`, `+099D` and `+0D86`. In the 16
instructions after each register load, the only write through ES or DS is at `30BC:000C`, after ES
has been loaded with `4E71`. Every use of the five local far pointers in their routines is a
read: overlay 208 `+03BA` (the routine of `+03F8`) reads bytes `+7`, `+8` and `+9` of the record
and writes only through its argument pointers at `bp+0xA`, `bp+0xE`, `bp+0x12` and `bp+0x16`
(`+03FF..+055C`), and the other four routines load their pointer with `les` 5, 1, 2 and 1 times
and store nothing through ES before ES changes.

## Interpretation

Nothing writes the records at `51F1:0000`, so their words at `+0` are the numbers the executable
holds, and none is 104 or 225. No attack that reaches the hit routine through overlay 179 `+0C7F`
passes `DATA` 104 or `DATA` 225: with FND-PARTY-102 and FND-PARTY-103, every caller passes -1, a
fixed number or a word of this table.

## Alternatives

- A copy of one of the five local far pointers to other memory or to a callee, used to write
  later, is outside the reading; the pointers were followed only inside their routines.
- A segment computed at run time that equals `51F1`, such as one formed from another segment by
  arithmetic, is not found by the search.
- The two other routes into the hit routine, through overlay 179 `+11DC` and overlay 193 `+003C`,
  were not read.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`, run in `tools/research/exec-census/`: `segment_references.py <dsun> 51F1`, decoding the
16 instructions after each hit and following each local far pointer through its routine; and
`overlay_listing.py <dsun> 208 91C4A 91E50`. Read the segment table row of descriptor 122 and the
words at file `0x00047110 + 18 * k` for `k` from 0 to 111.

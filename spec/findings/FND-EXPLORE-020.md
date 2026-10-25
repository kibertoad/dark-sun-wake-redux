---
id: FND-EXPLORE-020
title: In the shipped SCMD resources the most common walk animation reaches a cell centre in 4 frames going north or south and in 6 frames in the other six directions, with delay 0; the walk sets take 2 to 7 frames per cell
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x337352..0x3375EA
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x33F89E..0x33FB2E
tool: tools/research/exec-census/scmd_walk_frames.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

`scmd_walk_frames.py` reads the 543 `SCMD` resources of `OBJEX.GFF` and `RESOURCE.GFF` as
FND-EXPLORE-019's layout gives them and follows, for each direction `d` from 0 (north) to 7, the
frames from the one that word `DS:1413[d]` of the 36-word table names (words 1, 7, 9, 15, 17, 23,
25, 31) until a frame with flag 1 or 2. For 302 to 405 resources, depending on the direction, the
word names no frame (`0xFFFF`).

The most common moving sequence in each direction, as (x step, y step) per frame, all with delay 0:

| Direction | Resources | Frames | Steps |
| --- | --- | --- | --- |
| 0, north | 55 | 4 | (0, -7), (0, -1), (0, -7), (0, -1) |
| 1, north-east | 55 | 6 | (4, -4), (3, -4), (1, 0), (4, -4), (3, -4), (1, 0) |
| 2, east | 32 | 6 | (4, 0), (3, 0), (1, 0), (4, 0), (3, 0), (1, 0) |
| 3, south-east | 36 | 6 | (4, 4), (3, 4), (1, 0), (4, 4), (3, 4), (1, 0) |
| 4, south | 55 | 4 | (0, 7), (0, 1), (0, 7), (0, 1) |
| 5, south-west | 55 | 6 | (-4, 4), (-3, 4), (-1, 0), (-4, 4), (-3, 4), (-1, 0) |
| 6, west | 41 | 6 | (-4, 0), (-3, 0), (-1, 0), (-4, 0), (-3, 0), (-1, 0) |
| 7, north-west | 54 | 6 | (-4, -4), (-3, -4), (-1, 0), (-4, -4), (-3, -4), (-1, 0) |

`OBJEX.GFF` `SCMD` 25 and 596 (the two locations), 599, 691, 779, 780 and 2408 hold the north
and east sequences. Other
common sets move 4 pixels a frame for 4 frames, or 8 pixels a frame, which reaches a centre after
2 frames. Over all moving sequences the frames until both summed steps are multiples of 16 run
from 2 to 7, and a few sequences reach the first centre only at 32 or 48 pixels. The delays seen
are 0, except 4 frames with 5 in every direction and 8 frames with 30 in direction 1.

## Interpretation

With FND-EXPLORE-019, an object with the most common walk set takes 4 passes, and at least 4 ticks
of 1/96 s (about 42 ms), per cell north or south, and 6 passes, at least 62.5 ms, per cell in the
other directions, a diagonal cell included.

## Alternatives

Which `SCMD` resource each party member or other walker uses (record `+0x16`) was not traced, so
the frame counts for a given character are not shown here. The tally does not decide which
direction a sequence the table names for one direction actually plays in.

## How to reproduce

`scmd_walk_frames.py <OBJEX.GFF> <RESOURCE.GFF>` from the installed game directory.

---
id: FND-UI-031
title: WIND 3020 places three 15 x 15 action buttons, a close button and a 145 x 87 frame
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x325A4..0x3273F
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/3020` (FMT-UI-001) is 92 x 77 with no image at `0xC2` and five children:

| Child | Resource | Position | Size | Mask | Image |
|---|---|---|---|---|---|
| 0 | `BUTN/15308` | (23, 59) | 15 x 15 | 0 | `ICON/15107`, 16 x 15, 4 frames |
| 1 | `BUTN/15307` | (43, 59) | 15 x 15 | 0 | `ICON/15106`, 16 x 15, 4 frames |
| 2 | `BUTN/15306` | (3, 59) | 15 x 15 | 0 | `ICON/15105`, 16 x 15, 4 frames |
| 3 | `BUTN/15309` | (61, 60) | 27 x 11 | 84 | `ICON/15109`, 28 x 11, 1 frame |
| 4 | `APFM/15200` | (0, 0) | 145 x 87 | 486 | None |

`BUTN/15309` has a 4-byte tail. `WIND/15503`, which names `BMP/15002` (145 x 87) at `0xC2`,
holds the same frame `APFM/15200`.

## Interpretation

`WIND/3020` is the Look panel of FND-UI-018. Its frame is larger than the window. The close button
has the value 4 in its mask, so the pointer search passes over it (RULE-UI-001), and it must be
handled another way.

## Alternatives

What handles the close button, and what `BMP/15002` is for, are not known.

## How to reproduce

Read `WIND/3020` and its children through the directory.

---
id: FND-UI-027
title: WIND 18501 places ten list rows, four buttons, two scroll buttons and a name box
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x6A434..0x6A737
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/18501` (FMT-UI-001) is 320 x 181 with no image at `0xC2` and 17 children:

| Child | Resource | Position | Size | Mask | Image |
|---|---|---|---|---|---|
| 0 | `BUTN/18300` | (110, 0) | 67 x 23 | 4 | `ICON/18107`, 67 x 23, 1 frame |
| 1 | `BUTN/18301` | (231, 30) | 44 x 15 | 0 | `ICON/18108`, 44 x 15, 4 frames |
| 2 | `BUTN/18302` | (231, 50) | 44 x 15 | 0 | `ICON/18109`, 44 x 15, 4 frames |
| 3 | `BUTN/18303` | (215, 148) | 62 x 15 | 0 | `ICON/18110`, 62 x 15, 4 frames |
| 4 to 13 | `BUTN/18304` to `/18313` | (46, 31) to (46, 130), 11 apart | 163 x 11 | 208 | `ICON/18100`, 165 x 11, 4 frames |
| 14 | `BUTN/10314` | (215, 30) | 14 x 14 | 0 | `ICON/12102`, 14 x 10, 4 frames |
| 15 | `BUTN/10315` | (215, 130) | 14 x 14 | 0 | `ICON/12101`, 14 x 14, 4 frames |
| 16 | `EBOX/18401` | (49, 147) | 164 x 12 | 10 | `BMP/10002`, 161 x 12 |

Each of `BUTN/18304` to `/18313` has a 33-byte tail. `BMP/10005` is one 320 x 200 frame.

## Interpretation

`WIND/18501` is a list of ten rows with up and down scroll buttons, three buttons at the right, a
title and a name box: the list of stored characters the party screen offers.

## Alternatives

A recorded playthrough (SRC-YOUTUBE-FLOMVOSHEOM, near 2:30 to 2:40) shows this list after the
add choice of an empty party slot over `BMP/10005`, with a title from `ICON/18103` and a button
from `ICON/18104` where the records name `ICON/18107` and `ICON/18108`; the records' icons show the
words of the save screen, so the game may swap them at run time. That is recorded from the video
only. What the rows show and what the buttons do are not known.

## How to reproduce

Read `WIND/18501` and its children through the directory, and decode each icon and `BMP/10005`
with `PAL/1000`.

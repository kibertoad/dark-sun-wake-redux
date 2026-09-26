---
id: FND-UI-024
title: WIND 19500 places four start buttons, and BMP 20029 and 20028 are the start window's frame and crest
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x20711..0x2088E
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x1E7C4..0x1E97C
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/19500` (FMT-UI-001) is 320 x 200 with no image at `0xC2` and four children:

| Child | Resource | Position | Size | Mask | Image |
|---|---|---|---|---|---|
| 0 | `BUTN/19300` | (94, 70) | 127 x 12 | 0 | `ICON/19111`, 127 x 12, 4 frames |
| 1 | `BUTN/19301` | (50, 87) | 220 x 12 | 0 | `ICON/19112`, frames 220x12, 220x12, 1x1, 220x12 |
| 2 | `BUTN/19302` | (64, 104) | 192 x 12 | 0 | `ICON/19113`, frames 191x13, 191x13, 1x1, 191x13 |
| 3 | `BUTN/19303` | (92, 120) | 127 x 12 | 0 | `ICON/19114`, frames 127x12, 127x12, 1x1, 127x12 |

Decoded with `PAL/1000`, the first frames of the four icons show the labels of the manual's start
choices, in the order start the game, create characters, load a saved game and exit to DOS.
`BMP/20029` is one 314 x 112 frame of a stone frame, and `BMP/20028` one 222 x 33 frame of a
flame and medallion crest. Both look right under `PAL/1000`.

## Interpretation

`WIND/19500` is the start window. The third frame of three of its icons is a 1 x 1 placeholder, so
at most three states of those buttons have art.

## Alternatives

Where the game draws `BMP/20029` and `BMP/20028`, and in which order, is not in any record; a
recorded playthrough (SRC-YOUTUBE-FLOMVOSHEOM, near 2:10) shows the frame at (3, 44) and the crest
at (47, 24) on a black screen, under the four buttons. Which frame shows which state is not known.

## How to reproduce

Read `WIND/19500` and its four buttons through the directory, and decode the four icons and the
two images with `PAL/1000`.

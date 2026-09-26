---
id: FND-UI-025
title: WIND 19501 holds two frames and WIND 19502 one 319 x 199 button, and BMP 11000 and 20079 are the View Character picture and title
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x2088E..0x20AF2
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/19501` (FMT-UI-001) is 320 x 200 with two children: `APFM/19200` at (0, 0),
320 x 200 with mask 494, and `APFM/19201` at (10, 10), 28 x 16 with mask 486.
`RESOURCE.GFF#WIND/19502` is 320 x 200 with one child: `BUTN/2099` at (0, 0), 319 x 199, mask 0,
with no icon. Neither window names an image at `0xC2`.

`BMP/11000` is one 320 x 200 frame of a screen with four character panels, status areas and a
bar along the bottom. `BMP/20079` is one 210 x 23 frame of a title.

## Interpretation

`BMP/11000` is the picture of the View Character screen and `BMP/20079` its title (FND-UI-020).
The manual says that choosing to create characters on the start window leads to the View Character
screen with four empty slots (SRC-MANUAL-1994, page 7). `WIND/19501` and `WIND/19502` hold only
frames and one full-screen button, and what they are for is not known.

## Alternatives

Earlier notes took `WIND/19501` and `WIND/19502` for the windows of the screen reached from the
start window, because their numbers follow `WIND/19500` and a recorded playthrough
(SRC-YOUTUBE-FLOMVOSHEOM, near 2:40 to 2:50) shows `BMP/11000` under `BMP/20079` there. No code
that opens them is known (FND-UI-012).

## How to reproduce

Read the two windows and their children through the directory, and decode `BMP/11000` and
`BMP/20079` with `PAL/1000`.

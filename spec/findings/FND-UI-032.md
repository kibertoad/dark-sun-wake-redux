---
id: FND-UI-032
title: WIND 12500 to 12503 are the conversation windows, with a text box, five response rows and scroll buttons
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x3CEF9..0x3D4B1
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x16324C..0x16417F
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In `RESOURCE.GFF` (FMT-UI-001):

| Window | Size | Children |
|---|---|---|
| `WIND/12500` | 318 x 72 | `BUTN/12300` (0, 0) 300 x 58, no icon; `EBOX/12400` (75, 6) 236 x 46, mask 4, image `BMP/12002`; `BUTN/2093` (305, 4) 14 x 14, `ICON/12102`; `BUTN/2094` (305, 18) 14 x 35, `ICON/12100` |
| `WIND/12501` | 318 x 58 | `BUTN/2096` (305, 18) 14 x 35, `ICON/12100`; `BUTN/2076` to `/2080` at (3, 13), (3, 21), (3, 29), (3, 37), (3, 45), each 300 x 10 with `ICON/12104` to `/12108` (302 x 10, 1 frame) and a 53-byte tail; `BUTN/2095` (305, 4) 14 x 14, `ICON/12102` |
| `WIND/12502` | 318 x 58 | `EBOX/12401` (56, 6) 239 x 46, mask 10, image `BMP/12002` |
| `WIND/12503` | 320 x 200 | `BUTN/2082` (0, 0) 320 x 200, no icon; `EBOX/12402` (33, 38) 251 x 127, mask 0, no image |

Every button has mask 0. `BMP/12002` is 243 x 47. `BMP/12003` is one 320 x 200 frame with 18,516
drawn pixels, a textured panel with a dark outline in its top rows. `GPLDATA.GFF#PORT/18`, at
`0x16324C`, is one 72 x 72 frame.

## Interpretation

`WIND/12500` is the upper conversation window with the speech box and its scroll buttons, and
`WIND/12501` the lower window with five response rows. FND-UI-016 shows both in use. `WIND/12502`
and `WIND/12503` are further text windows whose use is not known.

## Alternatives

The rows' icons are one frame each, so a chosen or pointed-at row is not shown by another frame
of the same icon. When the game uses `WIND/12502` and `WIND/12503` is not known.

## How to reproduce

Read the four windows and their children through the directory, and decode the images with
`PAL/1000`.

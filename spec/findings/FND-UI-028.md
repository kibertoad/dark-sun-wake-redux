---
id: FND-UI-028
title: WIND 10500 places the fourteen Game Menu buttons in rows of four, five and five
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x5B222..0x5B6AB
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/10500` (FMT-UI-001) is 210 x 116 with no image at `0xC2` and 30 children: 14
buttons and 16 frames. `BMP/10000` is one 210 x 116 frame.

| Child | Resource | Position | Size | Image | Symbol, compared with SRC-MANUAL-1994 pages 14 to 16 |
|---|---|---|---|---|---|
| 2 | `BUTN/10300` | (49, 24) | 16 x 16 | `ICON/10100`, 4 frames | View character |
| 3 | `BUTN/11304` | (81, 24) | 16 x 16 | `ICON/11102`, 4 frames | View inventory |
| 4 | `BUTN/11305` | (113, 24) | 16 x 16 | `ICON/11103`, 4 frames | Cast spells or use psionics |
| 5 | `BUTN/11306` | (145, 24) | 16 x 16 | `ICON/11104`, 4 frames | Current spell effects |
| 6 | `BUTN/10301` | (49, 51) | 16 x 16 | `ICON/10101`, 4 frames | Exit to DOS |
| 7 | `BUTN/10302` | (73, 51) | 16 x 16 | `ICON/10102`, 4 frames | Load or save |
| 8 | `BUTN/10303` | (97, 51) | 16 x 16 | `ICON/10103`, 4 frames | Preferences |
| 9 | `BUTN/10305` | (121, 51) | 16 x 16 | `ICON/10105`, 3 frames | Overhead map |
| 10 | `BUTN/10306` | (145, 51) | 16 x 16 | `ICON/10106`, 4 frames | Center on leader |
| 14 | `BUTN/10313` | (44, 78) | 28 x 16 | `ICON/10113`, 4 frames | Collapse party |
| 11 | `BUTN/10310` | (76, 78) | 16 x 16 | `ICON/10110`, 4 frames | Walk |
| 12 | `BUTN/10311` | (97, 78) | 16 x 16 | `ICON/10111`, 4 frames | Look |
| 13 | `BUTN/10312` | (116, 78) | 16 x 16 | `ICON/10112`, 4 frames | Attack |
| 1 | `BUTN/10308` | (139, 78) | 28 x 16 | `ICON/10108`, 2 frames | Return to game |

Every button has mask 0. Child 0 is `APFM/11270`, 4 x 4 at (18, 102), and child 15 is
`APFM/10201`, 132 x 78 at (40, 18) with mask 110. Children 16 to 29 are frames with mask 0 whose
rectangles equal those of the 14 buttons.

## Interpretation

`WIND/10500` is the Game Menu, and `BMP/10000` its picture. Each button has a frame of the same
rectangle under it.

## Alternatives

The symbols were matched to the manual's pictures by eye. The window holds no screen position;
where the game places it is not known (FND-UI-011).

## How to reproduce

Read `WIND/10500` and its children through the directory, and decode each icon and `BMP/10000`
with `PAL/1000`.

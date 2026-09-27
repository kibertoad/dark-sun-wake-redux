---
id: FND-UI-029
title: WIND 16500 places the thirteen Preferences buttons
status: superseded
builds: [BLD-GOG-EN-1.1]
superseded_by: [FND-UI-033]
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x5C787..0x5CA4E
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/16500` (FMT-UI-001) is 210 x 116 with no image at `0xC2` and 15 children: the
frames `APFM/10201` (132 x 78 at (40, 18), mask 110) and `APFM/11270` (4 x 4 at (18, 102)), and
13 buttons with mask 0:

| Child | Resource | Position | Size | Image | Symbol, compared with SRC-MANUAL-1994 page 15 |
|---|---|---|---|---|---|
| 2 | `BUTN/16300` | (49, 24) | 16 x 16 | `ICON/16100`, 4 frames | Music on or off |
| 4 | `BUTN/16305` | (66, 29) | 9 x 8 | `ICON/16105`, 4 frames | Music volume down |
| 3 | `BUTN/16304` | (159, 29) | 9 x 8 | `ICON/16104`, 4 frames | Music volume up |
| 5 | `BUTN/16301` | (49, 43) | 16 x 16 | `ICON/16101`, 4 frames | Sound effects on or off |
| 7 | `BUTN/16307` | (66, 48) | 9 x 8 | `ICON/16105`, 4 frames | Sound effects volume down |
| 6 | `BUTN/16306` | (159, 48) | 9 x 8 | `ICON/16104`, 4 frames | Sound effects volume up |
| 11 | `BUTN/16309` | (56, 67) | 9 x 8 | `ICON/16105`, 4 frames | Difficulty down |
| 10 | `BUTN/16308` | (149, 67) | 9 x 8 | `ICON/16104`, 4 frames | Difficulty up |
| 9 | `BUTN/16303` | (49, 78) | 16 x 16 | `ICON/16103`, 4 frames | About |
| 8 | `BUTN/16302` | (67, 78) | 16 x 16 | `ICON/16102`, 4 frames | Animations on or off |
| 14 | `BUTN/16310` | (85, 78) | 16 x 16 | `ICON/16106`, 4 frames | Voice on or off |
| 12 | `BUTN/11308` | (109, 78) | 28 x 16 | `ICON/11101`, 3 frames | Game Menu |
| 13 | `BUTN/10308` | (139, 78) | 28 x 16 | `ICON/10108`, 2 frames | Return to game |

## Interpretation

`WIND/16500` is the Preferences screen, the same size as the Game Menu and sharing two of its
frames and its return button.

## Alternatives

The symbols were matched to the manual's descriptions by eye; which of each pair of small buttons
lowers and which raises is read from the arrow each icon shows. Whether the game draws
`BMP/10000` under this window, as under the Game Menu, is not recorded.

## How to reproduce

Read `WIND/16500` and its children through the directory, and decode each icon with `PAL/1000`.

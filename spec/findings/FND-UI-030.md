---
id: FND-UI-030
title: WIND 11500 and WIND 13500 share the portrait and navigation buttons of the character and inventory screens
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x294EC..0x2A005
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4B3EF..0x4BF62
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#WIND/11500` (FMT-UI-001) is 320 x 189 with 86 children: 21 buttons, 64 frames and
one edit box. `RESOURCE.GFF#WIND/13500` is 320 x 200 with 89 children. Neither names an image at
`0xC2`. Both place the same controls in different places:

| Controls | In `WIND/11500` | In `WIND/13500` |
|---|---|---|
| `BUTN/10300`, `/11304`, `/11305`, `/11306`, 16 x 16, the first four Game Menu icons (FND-UI-028) | (43, 155), (67, 155), (91, 155), (114, 155) | (163, 181), (187, 181), (211, 181), (235, 181) |
| `BUTN/11308`, 28 x 16, `ICON/11101` | (223, 155) | (258, 181) |
| `BUTN/10308`, 28 x 16, return to game | (253, 155) | (288, 181) |
| `BUTN/11300` to `/11303`, 34 x 34, `ICON/11100` | (53, 30), (104, 30), (53, 90), (104, 90) | (12, 5), (12, 53), (12, 101), (12, 149) |
| `BUTN/11309` to `/11316`, 10 x 9, `ICON/11106` and `/11111` | beside the four above | beside the four above |
| `BUTN/11318`, 123 x 11, mask 84, `ICON/11108` | (151, 26) | (57, 3) |
| `EBOX/4003`, 95 x 8, mask 10 | (153, 28) | (59, 5) |
| `APFM/11200`, 320 x 189, mask 70 | (0, 0) | (0, 6) |

`WIND/11500` also places `BUTN/11319` (42 x 12) and `/11320` (48 x 12) at (132, 157) and
(173, 157), both with mask 160, `APFM/11269` (136 x 108) at (147, 41), groups of 18 x 18 frames in
7 by 3 and 7 by 2 arrangements from x 148 to 280, and six 98 x 5 frames at x 43. Of its 64
frames, the full-size one has mask 70, six have mask 486, 29 have mask 230 and 28 have mask 0.
`WIND/13500` also places four 42 x 12 buttons with mask 160 and `ICON/13006` at (186, 130),
(186, 142), (235, 159) and (277, 159), and `APFM/13200` (90 x 125, mask 160) at (75, 36).

`BMP/13001` is one 320 x 200 frame.

## Interpretation

`WIND/11500` lays out the View Character screen and `WIND/13500` the inventory screen. The manual
describes both (SRC-MANUAL-1994, pages 7, 10 and 11): four character boxes with two small buttons
beside each, for computer control and for the leader, and a row of six icons along the bottom.
The row is the Game Menu's first four buttons, `BUTN/11308` for the Game Menu, whose icon is the
one the Preferences screen's Game Menu button shows (FND-UI-029), and `BUTN/10308` for returning
to the game. `EBOX/4003` is the name box.

## Alternatives

Which screens use these windows is inferred from the shared buttons and the manual; no code that
names them was found (FND-UI-012), and the View Character captures were compared only with its
images (FND-UI-020). Which of the two small buttons beside a box is which is not known.

## How to reproduce

Read both windows and their children through the directory, and group the children by resource.

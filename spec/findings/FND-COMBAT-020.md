---
id: FND-COMBAT-020
title: The capture labelled as an enemy moving shows the conversation windows and no panel; the next capture shows the panel with an hourglass pointer
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: dynamic
locations: []
tool: DOSBox screenshot compared with Python 3.14.7 and Pillow 12.3.0
environment: GOG DOSBox 0.74-2 with dosbox_darksun2.conf and dosbox_darksun2_single.conf (machine=svga_s3, memsize 16, cycles fixed 15000, sbtype sb16), game started by RAVAGER.BAT
---

## Observation

Two owner captures from the same session as FND-COMBAT-018, each 320 x 200 and kept outside the
repository:

| Capture | XXH3-128 | Owner's label |
|---|---|---|
| `dsun_009.png` | `d240572d80fd67afaf2663d6d8a24132` | an enemy moving |
| `dsun_010.png` | `dd7846be772fb2ba815616f5725e887d` | confirmed with `dsun_011.png` to `dsun_024.png`; its own label was not written down |

`dsun_009.png` shows the two conversation windows of SCR-UI-012, the upper one with a portrait and
speech and the lower one with a title row and four responses, over the street, with one figure
between them and no status panel.

`dsun_010.png` shows the street with no conversation windows. At (215, 4) `BMP/19003` matches
2,594 of its 3,098 opaque pixels, with `Draxan`, `???/???`, `Okay` and `Move : 6` in the region
(243, 8) to (284, 31). The pointer is the hourglass, and two figures are drawn in walking poses
between the house on the left and the party at the bottom right.

## Interpretation

The status panel is absent while the conversation is shown. In `dsun_010.png` an enemy's turn is
under way with 6 movement left, and the pointer shows that the game is busy (SRC-MANUAL-1994,
page 4).

## Alternatives

The owner's label of `dsun_009.png` as an enemy moving does not match what the frame shows, a
conversation; `dsun_010.png` fits that label better. Capture names repeat across sessions, and
the label may have been given for another file of the same name. The labels are kept as the
owner gave them.

## How to reproduce

Find the captures by their hashes, compare `BMP/19003` at (215, 4) as in FND-COMBAT-018, and look
at the rest of each frame.

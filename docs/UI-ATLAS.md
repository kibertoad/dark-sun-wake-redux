# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Title image | Game identity and title text | `RESOURCE.GFF` `BMP ` #11011 with `PAL ` #11011 is one 320x200 frame | Extracted transactionally and displayed from the verified pack; pixel aspect and preceding/following timing unverified |
| Start window | Start Game and Create Characters are documented | `WIND` #19500 is 320x200; four button rectangles begin at (94,70), (50,87), (64,104), and (92,120), with mapped label icons | title plus first icon frames composed through one logical-canvas transform; interaction/frame states not implemented |
| Party creation/modification | Four-character create/select/modify flow | `WIND` #19501 contains 320x200 and 28x16 application frames at (0,0) and (10,10); `WIND` #19503 is the mapped character-generation window with a 95x8 edit box | bounded shell mapped; dynamic content and interaction not implemented |
| Exploration | Leader-only or expanded party, edge scrolling, cursor modes | unknown | not implemented |
| Character view and inventory | Dedicated character/inventory screens | unknown | not implemented |
| Spells, psionics, and effects | Cast/use and current-effects screens | unknown | not implemented |
| Game menu and overhead map | Menu routing and map command documented | unknown | not implemented |
| Combat | Expanded party and combat-specific commands | unknown | not implemented |

## Documented semantic actions

The manual records keyboard actions including character, inventory, map,
spell/psionic, effects, center-on-leader, animation/music/effects toggles,
leader selection, party collapse/expansion, save/load/quit, guard, wait, target
cycling, and end turn. These are intended action names only. Exact keys, mouse
regions, enabled states, and transitions will be transcribed and verified as
their screens enter a slice.

## Open measurements

The title and the mapped start/party windows establish a recurring 320x200
logical canvas, but whether all screens share it and how DOS pixel aspect should
be reproduced remain open. Palette behavior, viewport, panel bounds,
cursor hotspot, layer/draw order, frame counts, animation cadence, dialogue hit
rectangles, and scaling tolerances are all `unknown`. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

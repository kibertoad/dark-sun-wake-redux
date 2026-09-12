# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Start window | Start Game and Create Characters are documented | unknown | not implemented |
| Party creation/modification | Four-character create/select/modify flow | unknown | not implemented |
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

Logical resolution, DOS pixel aspect, palette behavior, viewport, panel bounds,
cursor hotspot, layer/draw order, frame counts, animation cadence, dialogue hit
rectangles, and scaling tolerances are all `unknown`. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Title image | Game identity and title text | `RESOURCE.GFF` `BMP ` #11011 with `PAL ` #11011 is one 320x200 frame | Extracted transactionally and displayed from the verified pack; pixel aspect and preceding/following timing unverified |
| Start window | Start Game and Create Characters are documented | `WIND` #19500 is 320x200; controls begin at (94,70), (50,87), (64,104), and (92,120), sized 127x12, 220x12, 192x12, and 127x12; mapped icon frames differ only for Load Saved Game at 191x13 | first frames render through one logical transform; rising-edge mouse clicks use declared control bounds and route to Core; destination screens, exact edge parity, and frame states remain unvalidated |
| Party creation/modification | Four-character create/select/modify flow | `WIND` #19501 contains 320x200 and 28x16 application frames at (0,0)/(10,10), with appearance codes 494/486; all #19500-#19505 windows reference 96x9 `BMP` #19004; #19503 maps the edit box, class labels, EXIT, and DONE | image-backed controls and shared window image extracted; appearance-code meaning, #19004 tiling, shell composition, dynamic content, and interaction not implemented |
| Character-generation modals | Manual documents spell, psionic, and cleric-sphere choices | `WIND` #19504/#19505 are 110x64 five-control windows; their ten icons label psionics/spells/half-giants/spheres and four elements/psionics, including one blank control | all three-frame labels extracted; owning transitions, selection semantics, and rendering remain unknown |
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
rectangles, text palette/spacing/layout, and scaling tolerances are all
`unknown`. `DATA-GOG-FONT-001` maps one glyph bitmap structure but does not
settle those presentation details. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

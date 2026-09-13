# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Title image | Game identity and title text | `RESOURCE.GFF` `BMP ` #11011 with `PAL ` #11011 is one 320x200 frame | Extracted transactionally; runtime sequencing/display, pixel aspect, and preceding/following timing remain pending |
| Start window | Start Game and Create Characters are documented | On a black 320x200 canvas, `BMP ` #20029 is a 314x112 stone shell at (3,44) and #20028 is a 222x33 flame overlay at (47,24), both with `PAL ` #1000; `WIND` #19500 controls begin at (94,70), (50,87), (64,104), and (92,120), sized 127x12, 220x12, 192x12, and 127x12; mapped icon frames differ only for Load Saved Game at 191x13 | Both shell layers and the four first frames are drawn in recorded order; rising-edge mouse clicks resolve DSUI WIND/BUTN coordinates, dimensions, images, and semantic resource IDs through one logical transform; destination screens, exact edge parity, and frame states remain unvalidated |
| Party creation/modification | Four-character create/select/modify flow | `BMP ` #11000 is the complete 320x200 overview base with four slot panels and a bottom bar; #20079 overlays the VIEW CHARACTER title at (55,0); `WIND` #19501 contains 320x200 and 28x16 application frames at (0,0)/(10,10), with event masks 494/486; all #19500-#19505 windows reference 96x9 `BMP` #19004, but the original generic WIND path does not read that field; #19503 maps the edit box, class labels, EXIT, and DONE | the overview shell renders on entry from CREATE CHARACTERS; the complete six-window/39-control graph, image-backed controls, and shared window image are extracted; #19004 awaits an app-specific consumer or observation, while portraits, status fields, slots, later shells, dynamic content, and interaction remain unimplemented |
| Add existing character | ADD from an empty party slot | `BMP ` #10005 is the complete 320x200 base; `WIND` #18501 places ten 165x11 rows at (46,31) through (46,130), up/down controls at (215,30)/(215,130), ADD/EXIT at (231,30)/(231,50), DELETE at (215,148), and the ADD title at (110,0); runtime imagery uses `ICON` #18100/#12102/#12101/#18104/#18109/#18110/#18103 with `PAL ` #1000 | the static first frames render whenever Core enters `AddExistingCharacter`; stored-character names, scrolling, selection, focus, frame states, delete/add actions, and the party-slot click path remain unimplemented |
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

The title and mapped start/party windows establish a recurring 320x200
logical canvas, but whether all screens share it and how DOS pixel aspect should
be reproduced remain open. Palette behavior, viewport, panel bounds,
cursor hotspot, layer/draw order, frame counts, animation cadence, dialogue hit
rectangles, text palette/spacing/layout, and scaling tolerances are all
`unknown`. `DATA-GOG-FONT-001` maps one glyph bitmap structure and verifies that
the supported font's character map is identity; the derived rasterizers can
compose explicit glyph-index runs and multiline blocks without altering them,
using spacing supplied by the caller. These facts still do not settle
generalized map semantics, authentic spacing/alignment, color, or other
presentation details. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

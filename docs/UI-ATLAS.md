# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Title image | Game identity and title text | `RESOURCE.GFF` `BMP ` #11011 with `PAL ` #11011 is one 320x200 frame | Extracted transactionally; runtime sequencing/display, pixel aspect, and preceding/following timing remain pending |
| Start window | Start Game and Create Characters are documented | On a black 320x200 canvas, `BMP ` #20029 is a 314x112 stone shell at (3,44) and #20028 is a 222x33 flame overlay at (47,24), both with `PAL ` #1000; `WIND` #19500 controls begin at (94,70), (50,87), (64,104), and (92,120), sized 127x12, 220x12, 192x12, and 127x12; mapped icon frames differ only for Load Saved Game at 191x13 | Both shell layers and the four first frames are drawn in recorded order; rising-edge mouse clicks resolve DSUI WIND/BUTN coordinates, dimensions, images, and semantic resource IDs through one logical transform; destination screens, exact edge parity, and frame states remain unvalidated |
| Party creation/modification | Four-character create/select/modify flow | `BMP ` #11000 is the complete 320x200 overview base with four slot panels and a bottom bar; #20079 overlays the VIEW CHARACTER title at (55,0); `WIND` #19501 contains 320x200 and 28x16 application frames at (0,0)/(10,10), with event masks 494/486; `WIND` #19502 delegates a 319x199 surface to image-less `BUTN` #2099; all #19500-#19505 windows reference 96x9 `BMP` #19004, but the original generic WIND path does not read that field; #19503 maps the edit box, class labels, EXIT, and DONE | the overview shell renders on entry from CREATE CHARACTERS; the complete seven-window/56-control start-flow graph, image-backed controls, shared window image, and exclusive #2099 surface bounds are validated; its internal slot partition remains unknown, while portraits, status fields, later shells, dynamic content, and interaction remain unimplemented |
| Add existing character | ADD from an empty party slot | `BMP ` #10005 is the complete 320x200 runtime base despite static `WIND` #18501 naming #10002; it places ten 163x11 row controls at (46,31) through (46,130), backed by 165x11 art, up/down controls at (215,30)/(215,130), ADD/EXIT at (231,30)/(231,50), DELETE at (215,148), and the ADD title at (110,0); runtime imagery uses `ICON` #18100/#12102/#12101/#18104/#18109/#18110/#18103 with `PAL ` #1000 | the static first frames render whenever Core enters `AddExistingCharacter`; the exact graph, substitutions, and exclusive hit rectangles resolve, and EXIT returns to the party overview; stored-character names, scrolling, selection, focus, frame states, ADD/DELETE actions, and the party-slot click path remain unimplemented |
| Character-generation modals | Manual documents spell, psionic, and cleric-sphere choices | `WIND` #19504/#19505 are 110x64 five-control windows; their ten icons label psionics/spells/half-giants/spheres and four elements/psionics, including one blank control | all three-frame labels extracted; owning transitions, selection semantics, and rendering remain unknown |
| Exploration | Leader-only by default; right-click cycles Walk/Attack/Look; screen edges scroll; 5/6 select expanded/leader-only display | the observed opening background is Tyr `(1024,1368)` on the 320x200 logical canvas; `GMAP` bit `0x40` blocks terrain cells; exact spawn, scroll rate, cursor art/hotspot, and party sprites remain unknown | deterministic Core camera/mode/display commands are implemented; the outermost logical row/column scrolls and rerasterizes a clamped Tyr viewport; a reusable planner maps an active Walk click through camera coordinates to a plan command, and a clock-free Core session executes one semantic route step per command with replan/cancel/interrupt events; runtime click wiring, spawn/footprint/occupancy, cadence, party, and cursor rendering remain pending |
| Character view and inventory | Dedicated character/inventory screens; V and I hotkeys | shell geometry and dynamic content unknown | reusable hotkey bindings and deterministic Core view/return routing implemented; presentation pending |
| Spells, psionics, and effects | C/U opens Cast Spells/Use Psionics; E opens Current Spell/Effects | shell geometry and dynamic content unknown | aliases and deterministic Core view/return routing implemented; presentation and spell behavior pending |
| Game menu and overhead map | Tab opens Game Menu; O opens map; Escape closes an active menu and requests exit from play | `WIND` #10500 and `BMP ` #10000 are 210x116; 14 controls form exact 4/5/5 rows and decoded icons corroborate every manual label; the graph omits canvas origin, so centered (55,42) placement is provisional; frame states, description text, map drawing, and confirmation flow remain unknown | the authentic base and first-frame icons render over Tyr; a reusable DSUI page object supplies absolute hit rectangles and routes character/inventory/cast/effects/map, Walk/Look/Attack, and Return; five actions remain visibly inert pending their state or destination behavior |
| Combat | Expanded party and combat-specific commands | unknown | not implemented |

## Documented semantic actions

The manual records keyboard actions including character, inventory, map,
spell/psionic, effects, center-on-leader, animation/music/effects toggles,
leader selection, party collapse/expansion, save/load/quit, guard, wait, target
cycling, and end turn. V/I/C/U/E/O/Tab/Escape and 5/6 now map through one
reusable binding table. Remaining keys, mouse regions, enabled states, and
transitions will be transcribed and verified as their screens enter a slice.

## Open measurements

The title and mapped start/party windows establish a recurring 320x200
logical canvas, but whether all screens share it and how DOS pixel aspect should
be reproduced remain open. Static region tile/object composition is bounded by
`DATA-GOG-SCENE-001`; the opening camera and full-canvas viewport are observed,
while cursor hotspot, dynamic layer/draw order, frame counts, animation cadence, dialogue hit
rectangles, text palette/spacing/layout, and scaling tolerances are all
`unknown`. `DATA-GOG-FONT-001` maps one glyph bitmap structure and verifies that
the supported font's character map is identity; the derived rasterizers can
compose explicit glyph-index runs and multiline blocks without altering them,
using spacing supplied by the caller. These facts still do not settle
generalized map semantics, authentic spacing/alignment, color, or other
presentation details. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

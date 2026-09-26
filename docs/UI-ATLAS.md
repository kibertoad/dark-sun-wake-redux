# UI atlas and interaction map

Original screenshots and captures are local-only. This document stores only
independently written measurements and diagrams.

## Known screen families

| Screen/family | Manual evidence | Runtime measurements | Implementation |
|---|---|---|---|
| Title image | Game identity and title text | `RESOURCE.GFF` `BMP ` #11011 with `PAL ` #11011 is one 320x200 frame | Extracted transactionally; runtime sequencing/display, pixel aspect, and preceding/following timing remain pending |
| Start window | Start Game and Create Characters are documented | On a black 320x200 canvas, `BMP ` #20029 is a 314x112 stone shell at (3,44) and #20028 is a 222x33 flame overlay at (47,24), both with `PAL ` #1000; `WIND` #19500 controls begin at (94,70), (50,87), (64,104), and (92,120), sized 127x12, 220x12, 192x12, and 127x12; mapped icon frames differ only for Load Saved Game at 191x13 | Both shell layers and the four first frames are drawn in recorded order; rising-edge mouse clicks resolve DSUI WIND/BUTN coordinates, dimensions, images, and semantic resource IDs through one logical transform; destination screens, exact edge parity, and frame states remain unvalidated |
| Party creation/modification | Four-character create/select/modify flow | `BMP ` #11000 is the complete 320x200 overview base with four slot panels and a bottom bar; #20079 overlays the VIEW CHARACTER title at (55,0); `WIND` #19501 contains 320x200 and 28x16 application frames at (0,0)/(10,10), with event masks 494/486; `WIND` #19502 delegates a 319x199 surface to image-less `BUTN` #2099; all #19500-#19505 windows reference 96x9 `BMP` #19004, but the original generic WIND path does not read that field; #19503 maps the edit box, class labels, EXIT, and DONE. `EXE-GOG-UI-006` finds no direct immediate-ID activation lead for any of those controls | the overview shell renders on entry from CREATE CHARACTERS. In `CharacterGeneration`, a provisional retained #11000 character-sheet base receives the ten first-frame image controls at their exact #19503 coordinates; the complete ordered 22-child graph validates and EXIT returns to the overview. The original dynamic fields/title composition, class and DONE behavior, focus, slot partition, portraits, status fields, and interaction remain unimplemented |
| Observed supplied-party strip | Owner-confirmed `OBS-GOG-PARTY-001` captures | Local-only 320x200 captures show four visible slots in AR'ANDA, TERRANNUS, THY ROKH, GERAKIS order; selection feedback accompanies their spell/use, inventory, and character-view shells. The matching USE captions are MAGE LEVEL 1, CLERIC LEVEL 1, MAGE LEVEL 1, and PSIONIC Metabolic respectively; they are observed captions, not record-field meanings. Character views visibly label TERRANNUS as Male/Human/Lawful Good/Cleric, THY ROKH as Female/Thri-Kreen/True Neutral/Fighter, and GERAKIS as Male/Half-Giant/Chaotic Good/Gladiator; AR'ANDA visibly shows Female/Elf/Chaotic Neutral with a role label beginning PRESERVER. `EXE-GOG-CHAR-008` bounds the native vocabulary but not a field link. AR'ANDA's visible Strength 18 narrows her catalog record to #40; THY ROKH's letter order narrows his to #42, while same-name candidate records leave TERRANNUS and GERAKIS unresolved | no default-party resource table is encoded; party source/origin, all dynamic status fields, selection action, membership semantics, and caption-to-record-field mapping remain gated |
| Add existing character | ADD from an empty party slot | `BMP ` #10005 is the complete 320x200 runtime base despite static `WIND` #18501 naming #10002; it places ten 163x11 row controls at (46,31) through (46,130), backed by 165x11 art, up/down controls at (215,30)/(215,130), ADD/EXIT at (231,30)/(231,50), DELETE at (215,148), and the ADD title at (110,0); runtime imagery uses `ICON` #18100/#12102/#12101/#18104/#18109/#18110/#18103 with `PAL ` #1000 | the static first frames render whenever Core enters `AddExistingCharacter`; the exact graph, substitutions, and exclusive hit rectangles resolve, and EXIT returns to the party overview; stored-character names, scrolling, selection, focus, frame states, ADD/DELETE actions, and the party-slot click path remain unimplemented |
| Character-generation modals | Manual documents spell, psionic, and cleric-sphere choices | `WIND` #19504/#19505 are 110x64 five-control windows; their ten icons label psionics/spells/half-giants/spheres and four elements/psionics, including one blank control | all three-frame labels extracted; owning transitions, selection semantics, and rendering remain unknown |
| Exploration | Leader-only by default; right-click cycles Walk/Attack/Look; screen edges scroll; 5/6 select expanded/leader-only display | the observed opening background is Tyr `(1024,1368)` on the 320x200 logical canvas; exact comparison identifies BMP #599 frame 0 as the 17x35 leader at world `(1184,1459)` and bounded executable analysis establishes anchor cell `(74,91)`; BMP #599 has 13 variable-size frames with unknown state meanings; `GMAP` bit `0x40` blocks terrain cells. `DATA-GOG-CURSOR-001` maps `ICON` #19101-#19110 to Walk/Can't Walk, melee/invalid melee, ranged/invalid ranged, Look/invalid Look, invalid cast, and wait, with exact 10x13 through 16x17 geometry and a manual-defined `(0,0)` hotspot; native footprint/cadence, scroll rate, generalized target eligibility, and remaining party sprite resources remain unknown | deterministic camera/mode/display commands and rerasterized edge scrolling are implemented. `COMPAT-INPUT-001` adds right-button grab-drag panning while preserving a stationary right click for mode cycling. Active Walk clicks execute live collision-aware routes and fixed-point interpolation moves verified frame 0 between authoritative anchors without driving rules. The original cursor renders last with reachability-based Walk feedback, topmost-entity/leader Look feedback, and melee eligibility for the first observed OJFF #9258 target; a provisional single-cell footprint and 125 ms semantic step are isolated policies pending native evidence, while sprite-frame animation, expanded party, generalized NPC behavior, and broader target semantics remain pending |
| Interaction and dialogue | Look exposes Talk, Pick Up, and Use; a sole available action happens directly; dialogue uses portrait/speech and response areas | hostile `WIND` #3020 is 92x77 at `(68,45)`, with disabled Talk/Pick Up/Use and dismiss controls at exact measured positions. An award capture uses only upper `WIND` #12500; the owner-confirmed dialogue capture maps textured `BMP ` #12003 to `(0,0)` and `(0,140)`, and speech texture `BMP ` #12002 to `(75,6)`. The five response controls begin at `(3,153)`. The captured exchange resolves to `GPL` #135 and its `showpic 18` identifies a 72x72 `PORT` #18 portrait; the five rows correlate to choices 0, 1, 2, 3, and 7, whose final label comes from MAS #99 global string #5; the award script path remains unknown | deterministic interaction targets/actions and one-action shortcut are modeled. The required-revision-33 full-corpus pack includes all three windows, two panel assets, thirteen interaction/dialogue images, portrait #18, and provenance-checked GPL #135/MAS #99 DSGP v2 envelopes; response hit rows resolve exactly. F9 temporarily previews the fixed-canvas portrait, extracted panel/chrome, first literal speech source, and the five captured opening labels in source order. While the preview is active, its backdrop switches from the aspect-expanded map to the same centered 320x200 letterboxed canvas, so dialogue and world share a consistent scale; closing it restores the expanded map. Each row retains its original choice index and branch target; clicking a row records that pair in Core and is consumed before world input. Choices 0-4 validate and apply their bounded returned effects plus the intervening continuation. Choices 0/1 advance to the filtered second menu; choices 2/3 increment local number 0 and collectively reveal choice 4; choice 4 resets the counter, sets local flag 9, and advances. Choice 1 applies its known global-357 conditional effects. Targets 1825/3479 enable then consume source choice 2; target 1996 selects the opening or alternate transcript and effects from `GNUM22` and `GNUM84 & 2`, and target 2352 then consumes source choice 4. Target 2415 sets local flags 12/13, conditionally derives flag 10, and enters the seven-entry third menu; its exit label resolves from MAS #99 global string #6. Targets 2921/3089/3257 consume the primary third-page sequence, targets 3686/3786 consume its alternate flag sequence, and target 3976 completes the page. Implemented branches replace the speech with bounded projected output, including explicit newlines, and target 1597 stays on its calling page. The shared exit target on either of the first two menus applies local flags 14/4 and closes the completed preview. False and unknown conditions are hidden; visible unimplemented targets are inert without selecting Core state. Wrapping, generic native variable initialization, capability derivation, and broader consequences remain pending |
| Character view and inventory | Dedicated character/inventory screens; V and I hotkeys | `WIND` #11500 is 320x189 with 86 controls over the previously mapped 320x200 character shell; `WIND` #13500 is 320x200 with 89 controls over 320x200 `BMP ` #13001; both share the five navigation buttons with exact page-specific positions. `OBS-GOG-PARTY-001` confirms all four owner-labelled character-view and inventory variants use the visible party strip and selection feedback | character and inventory shells render; one reusable DSUI page object routes View Character, View Inventory, Cast/Use Psionics, Current Effects, and Return while unknown interior controls remain inert; dynamic fields, items, portraits, frame states, and native draw order remain pending |
| Spells, psionics, and effects | C/U opens Cast Spells/Use Psionics; E opens Current Spell/Effects | `OBS-GOG-UI-001` verifies that both destinations reuse `BMP ` #11000 and `WIND` #11500; Cast centers 104x23 `BMP ` #20080 (USE) at (108,0), while Effects centers 152x23 #20075 at (84,0). `OBS-GOG-PARTY-001` adds one owner-labelled USE-screen capture for each observed party member and its visible caption: AR'ANDA/THY ROKH MAGE LEVEL 1, TERRANNUS CLERIC LEVEL 1, and GERAKIS PSIONIC Metabolic | both observed shells and exact titles render; the shared five-control destination page routes navigation while spell/power/effect lists, selection, targeting, caption semantics, and other interior actions remain pending |
| Game menu, Preferences, and overhead map | Tab opens Game Menu; O opens map; Escape closes an active menu and requests exit from play | `WIND` #10500 and `BMP ` #10000 are 210x116; 14 controls form exact 4/5/5 rows. Preferences `WIND` #16500 reuses the same 210x116 base and contains two APFM plus 13 exact controls corroborated by manual page 15; both graphs omit a canvas origin, so centered (55,42) placement is provisional; setting ranges, frame states, description text, map drawing, exact center policy, and confirmation flow remain unknown | both authentic panels and first-frame icons render over Tyr; reusable DSUI page objects supply absolute hit rectangles. Game Menu routes character/inventory/cast/effects/Preferences/map, Walk/Look/Attack, Return, Center on Leader, Collapse Party, and clean Exit; Preferences routes Game Menu and Return while unevidenced setting mutations remain inert; Load/Save remains pending |
| Combat | Combat expands the party. The manual assigns Guard `G`, next target `N`, previous target `P`, end turn `Q`, Wait `W`, and Space to disable computer control; Walk attacks after approaching a selected opponent | owner-confirmed `dsun_009` depicts enemy movement; `dsun_011` is enemy striking and visibly displays a red `11` damage-feedback glyph plus the dynamic panel text `Draxan`/`Moves 20`; `dsun_012` is combat during Thy'rokh's turn with the party cluster, compact right panel, and dynamic text `Thy'rokh`/`90/85`/`Moves 15`. The differing text supports the owner-confirmed active-combatant panel only. `EXE-GOG-COMBAT-008` reaches the static #19003 panel through a guarded four-record (49-byte-stride) operation, but its immediate continuation is a shared `stdpatch`-related initialization path; it provides no independent combat attribution, record owner, or field meaning. The owner reports no separate target-switching or confirmation presentation: clicking an enemy makes the active character approach and strike it. The owner also reports no visible turn-transition treatment. On combat exit, presentation immediately resumes ordinary exploration with the single leader; there is no dedicated return frame. This establishes neither panel-value meanings, exit condition, nor turn progression. Movement-point scale/cost, distance/path/speed/collision, attacker, target, action, range, hit resolution, timing, and command input/layout remain unknown | deterministic Core command vocabulary and rising-edge hotkey mapping are implemented but deliberately unconnected until a combat state can resolve them from further evidence |

Combat panel detail: the static 98x32 BMP #19003 matches at (215,4) in both
confirmed frames. Only a bounded (243,8) through (284,31) region differs from
the static art; it is an unknown dynamic overlay, not a movement, text, timing,
or action contract. Required-revision-34 extraction preserves the static panel
as an evidence-only DSIX asset at that placement; the Game does not render it.

The owner-labelled enemy-motion frame `dsun_009` visibly retains dialogue
chrome and lacks this compact panel. It is therefore not a source for the
stable combat-panel layout, despite providing bounded motion evidence.

The manual's wording about an opponent being selected does not establish a
separate visible selection state: the owner reports one direct enemy click
causes approach and striking without target-switching or confirmation chrome.
Do not add a target-selection UI from that wording.

Character-view graph detail: the 86-child WIND #11500 graph consists of 64
APFM, one EBOX, and 21 BUTN records. Its root image-less frame is 320x189 at
(0,0); the only EBOX is 95x8 at (153,28); the 136x108 image-less frame starts
at (147,41); four image-less 34x34 frames occupy (53,30), (104,30), (53,90),
and (104,90). Two groups of image-less 18x18 frames form 7-by-3 and 7-by-2
grids spanning x=148..280. The grid preserves three distinct event-mask
classes (six cells 486, 29 cells 230, and all other non-root frames zero);
three BUTN records also have nonzero masks (84 for #11318 and 160 for
#11319/#11320). The 320x200 captures establish the visible character-view
composition, but no static record attaches a field, item, portrait,
interaction, or generic widget treatment to any of these controls.

Inventory vocabulary boundary: the owner-confirmed AR'ANDA inventory capture
visibly includes `Longsword` and `Dagger`. Each exact label occurs once in the
owned text corpus, in `TEXT` #1000 at zero-based CRLF line 25 and 28
respectively (`DATA-GOG-ITEMS-002`). This establishes source vocabulary and
that local order only. It does not bind either string to an item record, slot,
quantity, owner, statistics, selection treatment, or any inventory action.

Third-menu status clarification: target 3686 clears local flag 17 and sets flag
18; target 3786 then clears flag 18. Both return, are dispatched by target, and
present bounded projected output. Target 3976 clears flag 8, conditionally sets
flag 14 from post-assignment state, and completes the dialogue.

## Documented semantic actions

The manual records keyboard actions including character, inventory, map,
spell/psionic, effects, center-on-leader, animation/music/effects toggles,
leader selection, party collapse/expansion, save/load/quit, and combat Guard
(`G`), Wait (`W`), target cycling (`N`/`P`), end turn (`Q`), and disable
computer control (Space). V/I/C/U/E/O/Tab/Escape and 5/6 now map through one
reusable binding table. Remaining keys, mouse regions, enabled states, and
transitions will be transcribed and verified as their screens enter a slice.

## Open measurements

The title and mapped start/party windows establish a recurring 320x200
logical canvas, but whether all screens share it and how DOS pixel aspect should
be reproduced remain open. Static region tile/object composition is bounded by
`DATA-GOG-SCENE-001`; the opening camera and full-canvas viewport are observed,
while dynamic layer/draw order, frame counts, animation cadence, dialogue hit
rectangles, text palette/spacing/layout, and scaling tolerances are all
`unknown`. `FMT-TEXT-001` maps one glyph bitmap structure and verifies that
the supported font's character map is identity; the derived rasterizers can
compose explicit glyph-index runs and multiline blocks without altering them,
using spacing supplied by the caller. These facts still do not settle
generalized map semantics, authentic spacing/alignment, color, or other
presentation details. Slice 2 must record them
from controlled GOG runs before presentation parity is claimed.

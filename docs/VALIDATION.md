# Validation

CI and routine repository checks require no proprietary content. Synthetic GFF,
indexed-image, palette, indexed-font, text, DSIX, DSFT, DSTX, DSUI, DSRG, and
DSOB fixtures exercise successful decoding plus
truncation, bounds, invalid-component, unsafe-path, inventory, and transactional
replacement failures. Core rule tests use explicit inputs and no ambient state.
Start-flow tests also prove identical seeds and commands yield identical events
and state hashes, snapshots restore exactly, rejected commands are sequenced,
and replay stops at the first divergent hash.
Party-rule coverage includes the manual-documented all-three Psionicist rule,
the exactly-one discipline rule for other characters, Cleric-only elemental
spheres, canonical hash sensitivity to the selected sphere, the exact
six-ability modifier values for all eight origins, and immutable
dual-class progression across the human-only, level-three, repeated-career,
three-career, monotonic-advancement, and former-benefit boundaries. It also
proves ordered class/progression consistency, atomic DUAL selection and
cancellation, exact command payloads, progression-sensitive hashes, and
snapshot restore both during and after selection.
Start-flow coverage also proves occupied-slot edits replace rather than append,
invalid replacements are atomic, DROP moves a member to character storage, ADD
restores it, and an active edit target survives snapshot restore.
Presentation-independent input tests prove viewport letterboxing cannot change
the logical start-button choice, exercise DSUI-derived exclusive rectangle edges,
and reject incomplete, unexpected, or image-mismatched start-window graphs.
UI-resource tests also cover the executable-evidenced nonzero-intersection rule
for serialized/runtime event masks without assigning speculative names to bits.
Font tests compose variable-width indexed glyph runs and multiline blocks,
preserve palette-index bytes, zero-fill explicit glyph/line spacing, retain
blank lines, accept zero-width glyphs, and reject malformed fonts, negative
spacing, and oversized output.
Region-scene tests prove row-major tile placement, clipped cross-tile viewports,
the corroborated OJFF/ETAB offset transform and mirror bit, first-frame selection,
transparent-pixel preservation, ordered object overdraw, and rejection of
invalid viewport, tile, frame, and cross-catalog references.
Terrain-grid tests prove the executable-evidenced `GMAP` `0x40` mask, preserve
the independent `0x80` flag, close out-of-bounds cells, validate pixel/cell
edges and centers, and reject malformed planes. Planner tests prove stable
camera-to-cell Walk routes, obstacle detours, unreachable destinations, and
inactive mode/view/outside-canvas rejection without assigning route cadence.
Actor-controller tests compose those boundaries through logical Walk clicks,
prove fixed-step accumulation is independent of frame chunking, cap catch-up
work without discarding backlog, reset partial cadence on replanning, require
a placed actor, interrupt on a new blocker in shared occupancy, and drive the
camera-relative sprite from the same anchor. Presentation tests cover exact
fixed-point forward, reverse, and diagonal interpolation plus malformed progress
and interval rejection; controller tests prove continuity across a semantic
step boundary.
Viewport-layout tests prove that wide and tall displays expose additional
bounded map area, preserve the base camera center, clamp at world edges, map
physical input back into the expanded slice, and retain fixed 320x200
letterboxing when expansion is disabled. Dialogue-layout tests prove the exact
two window rectangles, portrait anchor, four scrollbar controls, five response
strip placements, F9 rising-edge toggle contract, bounded greedy wrapping,
five-label preview selection, overflow rejection, and unsupported-character
handling. Dialogue-session tests prove stable page identities, atomic one-shot
row selection, rejection without mutation, original branch retention, matching
branch enforcement, atomic local-flag application, and deterministic menu
reselection after a returning branch. Unknown local-number increments are
rejected before mutation. Synthetic projection tests reject drift in choice
0's prints, newlines, assignment, offsets, and local return; choices 2/3's
prints, matching flag clears, local-number-0 increments, offsets, and returns;
and choice 7's print, assignments, offsets, and local return.
Choice 1 projection tests cover all three print boundaries, its local flag clear,
the extended global-357 condition, conditional local flag 6/7 assignments,
global assignment, and return. Core tests exercise both known condition outcomes
and reject an unknown global flag before any mutation.
Owned content smoke additionally proves the actual projected first page fits
the measured text widths with the extracted font, selects its fifth row as
original choice 7 with the projected branch target intact, and reaches the
completed state with local flags 14 and 4 set. A separate owned-pack path selects
choice 0, clears local flag 0, returns to `AwaitingChoice`, advances through the
opening continuation, and verifies second-menu source order 0, 5, and 6. It
then reidentifies the shared target-2905 completion for source 6 and completes.
A counter-progression path selects source choices 2 and 3, verifies both flag
clears and increments, and proves that local number 0 reaches 2 while source
choice 4 becomes visible in the resulting 0, 1, 4, 6, and 7 order. Selecting it
sets local flag 9, resets local number 0, and enters second-menu order 0, 5, 6.
Another owned-pack path selects choice 1 and verifies local flag 1 clears, local
flags 6/7 become true from the fresh-opening condition, global flag 357 becomes
true, and the second menu resolves as 1, 3, 5, and 6.
Exploration command tests also center on arbitrary world points with both-edge
clamping, reject partial/out-of-world targets, suspend centering outside world
and Game Menu views, and restore leader-only display through Collapse Party.
Game Menu routing tests supply the moving actor's visual center only to the
context-dependent Center action and keep the three unsupported actions inert.
Destination-page tests resolve both the character and inventory WIND graphs
through one semantic page object, prove exact shared-button placement and image
identity, exclusive hit rectangles, cross-page navigation/return commands, and
malformed-page rejection.
Movement-session tests prove deterministic command/event traces, exactly one
semantic step per advance, atomic replanning failure, cancellation/completion,
snapshot isolation, and interruption when a step or diagonal side becomes
blocked after planning.
Occupancy-session tests prove canonical immutable multi-cell footprints,
deterministic occupant ordering, atomic place/move/remove and explicit rejected
transitions, terrain/bounds/overlap exclusion, own-cell movement overlap, live
whole-footprint passability, and composition with route planning.
Actor-movement tests prove route/occupancy lockstep, whole-footprint detours,
atomic step commits, rejection and dynamic-blocker interruption without partial
movement, repeatable traces, immutable snapshots, and external drift detection.

The smoke modes have distinct purposes:

- `--smoke-test` exits before content or graphics initialization and is safe on
  a content-free CI worker.
- `--content-smoke-test --asset-pack <path>` verifies the exact pack inventory,
  opens all seventy-eight DSIX images, the DSFT interface font, the DSTX text
  catalog, the resolved start-flow, Game Menu/Preferences, and
  character/inventory/Cast/Effects and hostile-interaction DSUI graphs, and DSCH character metadata
  catalog, opens the DSRG Tyr region and DSOB object-frame graph, and checks their
  frame, geometry, glyph, reference, and inventory contracts without a window.
- `--platform-smoke-test` creates the MonoGame platform surface and exits; it is
  reserved for installed-package environments with a display server.
- Normal startup verifies the pack before opening a window and renders the
  evidenced start shell/controls, party-overview shell, ADD-list shell, Tyr,
  Game Menu, Preferences, character, inventory, Cast/Use, and Current Effects
  shells; title sequencing, ADD-list content, dynamic destination content, and
  remaining destinations remain pending.

Owned-build validation currently targets GOG product `1432903719`, installed
build `52095422060333615`. Metadata-only FONT inspection verifies that all 256
character-map entries are identity values and summarizes its pixel-index range
without emitting glyph pixels. The retained ignored owned-source pack has been
transactionally refreshed and verifies as the exact 99-asset manifest including start/party/ADD
assets, all seven start-flow windows and 56 controls, the 210x116 Game Menu/Preferences base,
the 14-button/30-control Game Menu and 13-button/15-control Preferences graphs,
the 320x200 inventory base, the observed USE/EFFECTS title images, the
86-control character/Cast/Effects and 89-control inventory graphs, and 19 bounded character metadata entries,
plus the bounded Tyr region with 94 tiles
and 867 entity records, its 287 definitions, 246 images, and 477 frames, and
the exact 13-frame opening-leader image and all ten `ICON` #19101-#19110 cursor
images with their exact one-frame geometry, plus the three-window interaction/dialogue graph,
thirteen action/dialogue control images, `PORT` #18 portrait, and GPL #135/MAS #99 DSGP scripts.
The runtime content-smoke path verifies
all 13 frame dimensions plus frame 0's 367-pixel alpha coverage and rasterizes 320x200 viewports at
both opposite region corners successfully. It also verifies Tyr's exact four
`GMAP` values and 8,169 terrain-open cells through the bounded navigation
contract; these diagnostic checks are not a claim about party spawn.
The no-window smoke also plans and advances one real Tyr step from the evidenced
opening anchor through the runtime actor controller. Its provisional
single-cell footprint and 125 ms step are implementation policies, not native
timing or footprint evidence.
The owner-requested right-button grab-drag path is covered independently of
MonoGame: stationary press/release retains mode cycling, held logical motion
emits incremental inverse camera deltas at the documented 13:10 pan multiplier
with deterministic fractional carry,
canvas re-entry cannot jump, and a
completed drag suppresses cycling. Core separately bounds and clamps pan
commands. Either Alt key completing an Enter chord toggles once, and a held or
partial chord does not repeat. These are `COMPAT-INPUT-001`, not
original-parity claims.
Exact comparison of the retained ignored native opening frame against the
compositor at `(1024,1368)` isolates the visible leader to a 367-differing-pixel
component bounded by logical `(160,91)`-`(176,125)`. Exhaustive same-geometry
resource comparison identifies OJFF #305/BMP #599 frame 0 as an exact match;
bounded executable analysis ties its world top-left `(1184,1459)` to collision
anchor cell `(74,91)`. The multi-cell footprint remains unknown.
Six controlled local cursor captures at the unchanged opening camera establish the
Walk/Can't Walk, melee/invalid melee, and Look/invalid Look pairs and the
manual-defined upper-left hotspot. The valid melee hotspot resolves through the
static compositor transform to OJFF #9258 -> BMP #346. Three additional owned
captures establish hostile Look, award-notification, and two-window dialogue
layouts; GPL disassembly independently anchors the captured exchange in chunk
#135. Synthetic tests exercise the bounded first-conversation projection,
including exact observed offsets, literal and variable text sources, compound
condition parsing, menu bounds, and fail-closed identity/opcode/truncation
handling. Core tests distinguish true, false, and unknown constant/local flag/
local-number conditions; adapter tests preserve their parsed identity. Owned
content smoke confirms two speech variants, eight initial menu entries, and the
ordered condition shapes without checking proprietary text into Git. Local-only decoded previews established the title
and start-window mappings recorded as `DATA-GOG-TITLE-001`,
`DATA-GOG-UI-001`, `DATA-GOG-UI-006`, `DATA-GOG-UI-007`,
`DATA-GOG-UI-008` through `DATA-GOG-UI-010`; screenshots and decoded outputs stay
under ignored `analysis/original/` and never become golden files. Presentation
goldens in Git must use synthetic stand-ins. Visual comparison, input traces,
animation timing, and audiovisual synchronization remain open and will be
recorded per parity row rather than inferred from passing parsers.

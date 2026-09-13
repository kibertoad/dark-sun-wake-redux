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
Movement-session tests prove deterministic command/event traces, exactly one
semantic step per advance, atomic replanning failure, cancellation/completion,
snapshot isolation, and interruption when a step or diagonal side becomes
blocked after planning.
Occupancy-session tests prove canonical immutable multi-cell footprints,
deterministic occupant ordering, atomic place/move/remove and explicit rejected
transitions, terrain/bounds/overlap exclusion, own-cell movement overlap, live
whole-footprint passability, and composition with route planning.

The smoke modes have distinct purposes:

- `--smoke-test` exits before content or graphics initialization and is safe on
  a content-free CI worker.
- `--content-smoke-test --asset-pack <path>` verifies the exact pack inventory,
  opens all fifty-two DSIX UI images, the DSFT interface font, the DSTX text
  catalog, the resolved start-flow and Game Menu DSUI graphs, and DSCH character metadata
  catalog, opens the DSRG Tyr region and DSOB object-frame graph, and checks their
  frame, geometry, glyph, reference, and inventory contracts without a window.
- `--platform-smoke-test` creates the MonoGame platform surface and exits; it is
  reserved for installed-package environments with a display server.
- Normal startup verifies the pack before opening a window and renders the
  evidenced start shell/controls, party-overview shell, ADD-list shell, Tyr,
  and Game Menu; title sequencing, ADD-list content, and destination screens remain pending.

Owned-build validation currently targets GOG product `1432903719`, installed
build `52095422060333615`. Metadata-only FONT inspection verifies that all 256
character-map entries are identity values and summarizes its pixel-index range
without emitting glyph pixels. The retained ignored owned-source pack has been
transactionally refreshed and verifies as the exact 59-asset manifest including start/party/ADD
assets, all seven start-flow windows and 56 controls, the 210x116 Game Menu base,
its 14 button images and 30-control graph, and 19 bounded character metadata entries,
plus the bounded Tyr region with 94 tiles
and 867 entity records and its 287 definitions, 246 images, and 477 frames. The
runtime content-smoke path opens that pack and rasterizes 320x200 viewports at
both opposite region corners successfully. It also verifies Tyr's exact four
`GMAP` values and 8,169 terrain-open cells through the bounded navigation
contract; these diagnostic checks are not a claim about party spawn.
Exact comparison of the retained ignored native opening frame against the
compositor at `(1024,1368)` isolates the visible leader to a 367-differing-pixel
component bounded by logical `(160,91)`-`(176,125)`; this is not treated as
evidence of its collision anchor or footprint.
Local-only decoded previews established the title
and start-window mappings recorded as `DATA-GOG-TITLE-001`,
`DATA-GOG-UI-001`, `DATA-GOG-UI-006`, `DATA-GOG-UI-007`,
`DATA-GOG-UI-008`, and `DATA-GOG-UI-009`; screenshots and decoded outputs stay
under ignored `analysis/original/` and never become golden files. Presentation
goldens in Git must use synthetic stand-ins. Visual comparison, input traces,
animation timing, and audiovisual synchronization remain open and will be
recorded per parity row rather than inferred from passing parsers.

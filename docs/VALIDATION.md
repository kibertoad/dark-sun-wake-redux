# Validation

CI and routine repository checks require no proprietary content. Synthetic GFF,
indexed-image, palette, indexed-font, text, DSIX, DSFT, DSTX, and DSUI fixtures exercise successful decoding plus
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

The smoke modes have distinct purposes:

- `--smoke-test` exits before content or graphics initialization and is safe on
  a content-free CI worker.
- `--content-smoke-test --asset-pack <path>` verifies the exact pack inventory,
  opens all thirty-three DSIX UI images, the DSFT interface font, the DSTX text
  catalog, the resolved six-window DSUI graph, and the DSCH character metadata
  catalog, and checks their frame, geometry, glyph, reference, and inventory contracts
  without a window.
- `--platform-smoke-test` creates the MonoGame platform surface and exits; it is
  reserved for installed-package environments with a display server.
- Normal startup verifies the pack before opening a window and renders the
  evidenced start shell/controls, party-overview shell, and ADD-list shell;
  title sequencing and ADD-list content/interaction remain pending.

Owned-build validation currently targets GOG product `1432903719`, installed
build `52095422060333615`. Metadata-only FONT inspection verifies that all 256
character-map entries are identity values and summarizes its pixel-index range
without emitting glyph pixels. A temporary owned-source extraction produced and
verified the exact 41-asset manifest including both start-shell layers, both
party-overview layers, the ADD-list base and control family, all six windows, 39 controls, and
19 bounded character metadata entries;
the runtime content-smoke path opened that pack successfully, after which the
temporary pack was removed. Local-only decoded previews established the title
and start-window mappings recorded as `DATA-GOG-TITLE-001`,
`DATA-GOG-UI-001`, `DATA-GOG-UI-006`, `DATA-GOG-UI-007`, and
`DATA-GOG-UI-008`; screenshots and decoded outputs stay
under ignored `analysis/original/` and never become golden files. Presentation
goldens in Git must use synthetic stand-ins. Visual comparison, input traces,
animation timing, and audiovisual synchronization remain open and will be
recorded per parity row rather than inferred from passing parsers.

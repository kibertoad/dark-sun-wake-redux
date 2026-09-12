# Rules and evidence

## Sources

| ID | Source | Type | Use | Confidence |
|---|---|---|---|---|
| `MANUAL-1994` | `C:\GOG Games\Dark Sun 2\ds_wakerave_manual_pdf.pdf` (local only) | original rule book | Intended controls, menus, party and character rules, combat commands, magic, psionics, advancement, credits | high for documented intent; not proof of shipped edge cases |
| `GOG-1432903719` | English GOG build `52095422060333615` | owned release | Exact source fingerprints and future runtime/data observations | verified for recorded hashes and metadata |
| `FAQ-81038` | kibbitz, GameFAQs guide v1.13 | community research | Mechanics, route/branch index, reported bugs and manual conflicts | medium; confirm with controlled observations |
| `DSUN-MUSIC` | John Glassmyer, [`dsun_music`](https://github.com/JohnGlassmyer/dsun_music), MIT licensed | community technical research | GFF, image, region, and XMI structure and resource identification | medium; confirm each applicable result against the fingerprinted owned build |
| `OBS-GOG-*` | Future controlled runs | runtime observation | Player-visible state transitions, coordinates, timing, outcomes | unknown until recorded per finding |
| `DATA-GOG-*` | Bounded inspection of the owned build | data fact | Format fields and resource relationships | recorded per finding |

### DATA-GOG-GFF-001 - GFF container directory

- **Question:** How are resource identities, offsets, and lengths represented in
  the owned build's `.GFF` containers?
- **Method:** Fingerprint the supported edition, inspect bounded header/index
  windows, compare the DSUN-MUSIC GFF results, implement an independently
  bounded reader, and enumerate metadata from every installed `.GFF` without
  retaining payload bytes.
- **Finding:** Version `0x00030000` uses a 28-byte header and little-endian
  primary/secondary tag tables as specified in `docs/ORIGINAL-FORMATS.md`.
  All 26 installed GFF files parse, producing 16,168 bounded descriptors.
- **Confidence:** verified for directory structure and counts in
  GOG-1432903719; tag payload semantics remain unknown unless separately
  recorded.
- **Implementation:** `DarkSunWakeRedux.Resources.GffArchive` and the read-only
  `DarkSunWakeRedux.Inspect gff` command.
- **Tests:** synthetic primary, primary-only, segmented-secondary, truncation,
  out-of-range, and partial-overlap cases.

### DATA-GOG-IMAGE-001 - Indexed images and palettes

- **Question:** Which bounded image and palette payload structures occur in the
  owned build's GFF resources?
- **Method:** Compare the DSUN-MUSIC image/palette research with bounded samples
  from the fingerprinted installation, independently implement defensive
  readers, and decode every matching resource in all 26 installed GFF files
  without retaining or committing decoded content.
- **Finding:** `BMP `, `CBMP`, and `ICON` share a framed indexed-image envelope
  with sparse-row, `PLAN`, and `PLNR` encodings. `PAL ` contains 256 three-byte
  VGA colors. The readers decode 4,510 images containing 9,279 frames and all
  40 palettes in the installed build.
- **Confidence:** verified for these payload structures and counts in
  GOG-1432903719; resource meaning, palette pairing, placement, and timing are
  still unknown.
- **Implementation:** `DarkSunWakeRedux.Resources.IndexedImage`,
  `IndexedPalette`, and the read-only `image-catalog` inspection command.
- **Tests:** synthetic row/planar decoding, transparency, palette conversion,
  and malformed size, bounds, component, and bitstream cases.

### DATA-GOG-TITLE-001 - Title image resource mapping

- **Question:** Which resource and palette form the static game-title image?
- **Method:** Decode the full-screen `BMP ` candidates and all plausible palette
  pairings from the fingerprinted `RESOURCE.GFF`, render previews under ignored
  `analysis/original/`, and inspect them without retaining original content in
  Git. DSUN-MUSIC corroborates the image and palette decoding method, while the
  mapping is established from the owned build.
- **Finding:** `BMP ` #11011 is one 320x200 frame containing the *Dark Sun: Wake
  of the Ravager* title artwork, and `PAL ` #11011 supplies its correct colors.
- **Confidence:** verified for GOG-1432903719 by exact resource identity and
  locally rendered content; sequence timing and menu overlay behavior remain
  unknown.
- **Implementation:** transactional conversion to `images/title.dsix` and
  verified-pack runtime display.
- **Tests:** original-free DSIX round trips and rejection cases, plus synthetic
  GFF-to-verified-pack extraction using the recorded resource identities.

### DATA-GOG-UI-001 - Start-window and button resource mapping

- **Question:** How does the owned build identify and place the first start-menu
  controls over the title screen?
- **Method:** Independently inspect bounded `WIND` and `BUTN` payloads in the
  fingerprinted `RESOURCE.GFF`, compare repeated record structure across every
  resource of those tags, validate every nonzero child/image reference, and
  render the referenced `ICON` frames with plausible palettes under ignored
  `analysis/original/` paths.
- **Finding:** All 28 `WIND` and 139 `BUTN` resources parse within bounds and
  their references resolve. `WIND` #19500 is 320x200 and places `BUTN` #19300,
  #19301, #19302, and #19303 at (94,70), (50,87), (64,104), and (92,120).
  Those buttons are 127x12, 220x12, 192x13, and 127x12 and reference four-frame
  `ICON` #19111 through #19114, visibly labelled START GAME, CREATE CHARACTERS,
  LOAD SAVED GAME, and EXIT TO DOS. Palette #1000 visibly corrupts the title;
  the title palette #11011 also renders these overlays plausibly, which is
  evidence that it remains active, but runtime confirmation is still required.
- **Confidence:** verified for resource identities, dimensions, references, and
  logical coordinates in GOG-1432903719; medium for the active palette; unknown
  for frame-state meanings, focus, hit boundaries, and transitions.
- **Implementation:** bounded `UiWindowResource` and `UiButtonResource` readers
  plus the read-only `ui-catalog` inspection command. Runtime composition waits
  for an OBS-GOG state observation.
- **Tests:** synthetic signature, size, child-record, printable-tag, coordinate,
  repeated-ID, extension-tail, dimension, and reference-contract coverage;
  whole-catalog validation against the owned archive.

## Initial rules

### RULE-INPUT-001 - Mouse-first interaction

- **Behavior:** The original requires a mouse and uses cursor modes for walking,
  looking, attacking, and targeted actions. Escape exits the active menu.
- **Preconditions:** Relevant exploration or menu state is active.
- **Evidence:** MANUAL-1994, introduction and "How to Play".
- **Confidence:** high for intended behavior; exact hit regions and shipped edge
  behavior are unknown.
- **Implementation:** not implemented.
- **Tests:** planned semantic-input and menu-routing tests.
- **Uncertainty:** Cursor art, complete mode transitions, right-click behavior,
  and coordinate boundaries require OBS-GOG evidence.

### RULE-PARTY-001 - Four-character party

- **Behavior:** The creation flow accepts one to four characters and recommends
  four. An empty party cannot start and a fifth character cannot be added.
- **Evidence:** MANUAL-1994, quick-start and party-creation sections, pages 2 and
  7-9.
- **Confidence:** high for intended party bounds and flow.
- **Implementation:** deterministic Core party aggregate and validation result;
  start/menu presentation is not implemented.
- **Tests:** empty/one/four/fifth-member boundaries and invalid-character
  rejection.
- **Uncertainty:** Exact pregenerated party data, on-disk created-character
  format, cancellation state, and shipped edge behavior require observation.

### RULE-PARTY-002 - Character creation invariants

- **Behavior:** Creation offers human, dwarf, elf, half-elf, half-giant,
  halfling, mul, and thri-kreen. Muls are male and thri-kreen female in the
  creation flow. Characters use one of six good/neutral alignments, ability
  scores fall from 9 through 24, and each class has the documented ability
  minima. Humans begin single-classed; non-humans may select as many as three
  classes; cleric and druid cannot be combined.
- **Evidence:** MANUAL-1994, pages 7-9 and 16-23.
- **Confidence:** high for documented intent; not yet observed in the shipped
  build.
- **Implementation:** `PartyCreationRules` exposes stable diagnostics and
  rejects unsupported drafts without I/O or presentation dependencies.
- **Tests:** sex, alignment/enum, ability range and class minima, human
  multiclass, class count/duplicates, cleric+druid, and agreed eligibility
  constraints.
- **Uncertainty:** Initial HP adjustment, ability-generation distribution,
  racial adjustments during editing, valid multiclass combinations, and exact
  screen defaults need DATA-GOG/OBS-GOG evidence.

### CONFLICT-PARTY-001 - Manual race/class eligibility lists

- **Claim A:** The race descriptions list the classes allowed for each race.
- **Claim B:** The class descriptions independently list allowed races.
- **Conflict:** The lists disagree for half-giant ranger and thief, mul druid,
  and thri-kreen druid and thief. They may contain additional combination rules
  that prose alone does not expose.
- **Evidence:** MANUAL-1994, race descriptions on pages 17-18 and class
  descriptions on pages 19-22.
- **Implementation:** these pairs return `EvidenceConflict` and validation emits
  `class_race_unresolved`; no eligibility is guessed.
- **Resolution needed:** record the selectable class list for every race in the
  fingerprinted GOG build, then update the matrix and tests.

### RULE-COMBAT-001 - Party expansion on combat entry

- **Behavior:** Exploration may show only the leader; combat makes all four party
  members visible. The manual also documents wait, guard, target cycling, and
  end-turn commands.
- **Evidence:** MANUAL-1994, "How to Play" and hotkeys.
- **Confidence:** high for intended commands; resolution order and formulas are
  unknown.
- **Implementation:** not implemented.
- **Tests:** planned deterministic combat command traces.
- **Uncertainty:** Activation order, RNG, THAC0/AC details, timing, and difficulty
  effects require OBS-GOG and possibly targeted Ghidra evidence.

## Conflict handling

FAQ-81038 reports discrepancies between documentation and shipped behavior.
Each conflict receives its own rule ID, both claims, reproduction procedure, and
owner decision. No compatibility behavior is selected from plausibility alone.

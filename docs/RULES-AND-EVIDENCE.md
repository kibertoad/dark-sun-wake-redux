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

### DATA-GOG-FONT-001 - Indexed bitmap font structure

- **Question:** How is the indexed glyph data in the owned build's `FONT`
  resource represented?
- **Method:** Inspect `FONT` #100 from the fingerprinted `RESOURCE.GFF`, account
  for every byte using a bounded independent reader, and validate the structure
  without retaining or committing the payload.
- **Finding:** The payload declares 256 glyphs of a shared height, followed by
  an eight-byte prefix, a 256-byte character map, and 256 absolute 16-bit glyph
  offsets. Each glyph record is a 16-bit width followed by exactly width times
  height palette-index bytes; zero-width control glyphs are valid. `FONT` #100
  is 8,299 bytes, has height 9, and balances exactly through its final record.
- **Confidence:** verified for `FONT` #100 in GOG-1432903719; character
  encoding, palette selection, string storage, spacing, and layout remain open.
- **Implementation:** `DarkSunWakeRedux.Resources.IndexedBitmapFont`, derived
  DSFT v1 extraction, and the metadata-only `font-catalog` inspection command.
- **Tests:** synthetic valid, zero-width, invalid-header, offset, dimension,
  and record-length cases.

### DATA-GOG-TEXT-001 - TEXT resource envelope

- **Question:** What bounded text encoding occurs in `RESOURCE.GFF`?
- **Method:** Validate every `TEXT` payload from the fingerprinted archive with
  an independent reader while emitting only resource and line metadata.
- **Finding:** All 62 resources (2,994 bytes total) are printable 7-bit ASCII,
  use CRLF-delimited lines, have a final CRLF, contain 316 lines, and have a
  maximum observed line length of 18 bytes. No NUL or extended bytes occur.
- **Confidence:** verified for `TEXT` resources in GOG-1432903719; resource-ID
  meanings, string interpolation, and screen routing remain open.
- **Implementation:** `DarkSunWakeRedux.Resources.GffTextResource`, deterministic
  ID-preserving DSTX v1 extraction, and the metadata-only `text-catalog` command.
- **Tests:** synthetic multiline/empty-line parsing and invalid terminator,
  control-byte, non-ASCII, and size-limit cases.

### DATA-GOG-UI-001 - Start-window and button resource mapping

- **Question:** How does the owned build identify and place the first start-menu
  controls over the title screen?
- **Method:** Independently inspect bounded `WIND`, `BUTN`, `APFM`, and `EBOX` payloads in the
  fingerprinted `RESOURCE.GFF`, compare repeated record structure across every
  resource of those tags, validate every nonzero child/image reference, and
  render the referenced `ICON` frames with plausible palettes under ignored
  `analysis/original/` paths.
- **Finding:** All 28 `WIND`, 139 `BUTN`, 97 `APFM`, and 7 `EBOX` resources parse
  within bounds and their references resolve. `WIND` #19500 is 320x200 and places `BUTN` #19300,
  #19301, #19302, and #19303 at (94,70), (50,87), (64,104), and (92,120).
  Those controls are 127x12, 220x12, 192x12, and 127x12 and reference four-frame
  `ICON` #19111 through #19114, visibly labelled START GAME, CREATE CHARACTERS,
  LOAD SAVED GAME, and EXIT TO DOS. The corresponding visible frame sizes are
  127x12, 220x12, 191x13, and 127x12, so the third control and art bounds differ
  by one pixel in both axes. Palette #1000 visibly corrupts the title;
  the title palette #11011 also renders these overlays plausibly, which is
  evidence that it remains active, but runtime confirmation is still required.
  `WIND` #19501 contains `APFM` #19200 (320x200) at (0,0) and #19201 (28x16)
  at (10,10), establishing the bounded shell of the next party-flow screen.
  `WIND` #19503 contains a 95x8 `EBOX` and controls whose referenced icons spell
  out all eight class names, the three psionic disciplines, and cleric spheres,
  establishing it as the character-generation window without assigning meaning
  to its still-uninterpreted control fields.
- **Confidence:** verified for resource identities, dimensions, references, and
  logical coordinates in GOG-1432903719; medium for the active palette; unknown
  for frame-state meanings, focus, hit boundaries, and transitions.
- **Implementation:** bounded window, button, application-frame, and edit-box
  readers plus the read-only `ui-catalog` inspection command. The four complete
  icon frame sets are extracted with palette #11011 and their first frames are
  composed at the recorded logical coordinates; frame-state interaction remains
  gated on OBS-GOG evidence.
- **Tests:** synthetic signature, size, child-record, printable-tag, coordinate,
  repeated-ID, extension-tail, dimension, and field-contract coverage;
  whole-catalog validation against the owned archive.

### DATA-GOG-UI-002 - Character-generation image controls

- **Question:** Which image-backed controls belong to character generation?
- **Method:** Resolve every child of `WIND` #19503 through bounded `BUTN` and
  `ICON` records, inspect all frames locally, and compare palette candidates
  without retaining previews in Git.
- **Finding:** `BUTN` #2002-#2009 reference three-frame `ICON` resources with
  the eight class labels; #18302/#19304 reference four-frame EXIT/DONE icons
  #18109/#19100. Their coordinates, separate control/art dimensions, and frame
  counts are recorded in `OriginalContent.CharacterGenerationButtons`.
  Palette #11011 renders the complete family coherently.
- **Confidence:** verified for identities, labels, coordinates, dimensions, and
  frame counts; medium for palette #11011; unknown for frame-state semantics,
  hit boundaries, focus, and dynamic fields.
- **Implementation:** all ten image sets are transactionally converted to DSIX
  in the verified pack; runtime composition remains pending the rest of the shell.
- **Tests:** exact unique mappings, canvas bounds, frame contracts, synthetic
  extraction, manifest provenance, real-pack verification, and content smoke.

### DATA-GOG-UI-003 - Shared party-window image reference

- **Question:** Does the WIND fixed record name a shared image resource?
- **Method:** Compare offset 58 across all 28 bounded WIND records and resolve
  every nonzero value against the archive before inspecting decoded images.
- **Finding:** Offset 58 is zero in 20 windows, `BMP` #10002 in two, and `BMP`
  #19004 in all six #19500-#19505 windows. #19004 is one 96x9 UI bar/fill frame.
- **Confidence:** verified for the field, references, dimensions, and local
  appearance; medium for palette #11011; unknown for tiling and draw role.
- **Implementation:** `UiWindowResource.ImageResourceNumber`; #19004 is
  extracted as DSIX without assigning presentation semantics.
- **Tests:** synthetic field parsing, whole-catalog reference validation,
  exact frame contract, extraction provenance, and content smoke.

## Initial rules

### RULE-INPUT-001 - Mouse-first interaction

- **Behavior:** The original requires a mouse and uses cursor modes for walking,
  looking, attacking, and targeted actions. Escape exits the active menu.
- **Preconditions:** Relevant exploration or menu state is active.
- **Evidence:** MANUAL-1994, introduction and "How to Play".
- **Confidence:** high for intended behavior; exact hit regions and shipped edge
  behavior are unknown.
- **Implementation:** start-window clicks map the declared BUTN rectangles
  through a single letterboxed logical-canvas transform into semantic Core
  commands; later screens remain unimplemented.
- **Tests:** rectangle edges, wide/tall letterboxing, inverse coordinates, and
  resulting Core routing.
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

### RULE-START-FLOW-001 - Start and party-creation routing

- **Behavior:** The Start Window offers START GAME, CREATE CHARACTERS, LOAD
  SAVED GAME, and EXIT TO DOS. START GAME immediately enters play with the
  supplied pregenerated party. CREATE CHARACTERS opens View Character with four
  empty slots. Activating an empty slot offers NEW, ADD, and CANCEL; NEW opens
  character generation, ADD selects a previously created character, and CANCEL
  closes the menu. DONE accepts a valid new character. A created party may begin
  with one through four members.
- **Evidence:** MANUAL-1994, quick-start and "Creating Your Party," pages 2 and
  7-10; `DATA-GOG-UI-001` corroborates the four start controls and the bounded
  start/party/character-generation window families.
- **Confidence:** high for documented semantic destinations; exact input event,
  focus, animation, and transition timing require OBS-GOG evidence.
- **Implementation:** deterministic `StartFlow` screen transitions with stable
  rejection diagnostics and integration with the existing `Party` validation.
- **Tests:** every start choice, NEW/ADD/CANCEL routing, valid and invalid
  completion, empty/nonempty party start, cancellation, and wrong-screen
  rejection.
- **Uncertainty:** Pregenerated member records, saved/created-character formats,
  the exact early-start control, and shipped cancellation edge cases remain
  unimplemented until their data and runtime behavior are observed.

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

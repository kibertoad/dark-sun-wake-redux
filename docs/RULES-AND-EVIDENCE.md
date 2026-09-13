# Rules and evidence

## Sources

| ID | Source | Type | Use | Confidence |
|---|---|---|---|---|
| `MANUAL-1994` | `C:\GOG Games\Dark Sun 2\ds_wakerave_manual_pdf.pdf` (local only) | original rule book | Intended controls, menus, party and character rules, combat commands, magic, psionics, advancement, credits | high for documented intent; not proof of shipped edge cases |
| `GOG-1432903719` | English GOG build `52095422060333615` | owned release | Exact source fingerprints and future runtime/data observations | verified for recorded hashes and metadata |
| `FAQ-81038` | kibbitz, GameFAQs guide v1.13 | community research | Mechanics, route/branch index, reported bugs and manual conflicts | medium; confirm with controlled observations |
| `DSUN-MUSIC` | John Glassmyer, [`dsun_music`](https://github.com/JohnGlassmyer/dsun_music), MIT licensed | community technical research | GFF, image, region, and XMI structure and resource identification | medium; confirm each applicable result against the fingerprinted owned build |
| `PLAYTHROUGH-VIDEO-001` | [Public YouTube playthrough](https://www.youtube.com/watch?v=FLoMVOSHeOM) | community runtime capture | Corroborating start-window composition and sequence | medium for visible composition; precise source edition and capture conditions unknown |
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
- **Implementation:** transactional conversion to `images/title.dsix`. Runtime
  sequencing and display are pending evidence that distinguishes the title
  interval from the later start window.
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
  Its 256 character-map entries are the identity sequence `00` through `FF`
  (SHA-256 `40aff2e9d2d8922e47afd4648e6967497158785fbd1da870e7110266bf944880`),
  so direct and map-mediated byte indexing are equivalent for this edition.
  Glyph pixels use three distinct palette indices spanning 0 through 254.
- **Confidence:** verified for `FONT` #100 in GOG-1432903719; the map field's
  generalized semantics, palette selection, spacing, and layout remain open.
- **Implementation:** `DarkSunWakeRedux.Resources.IndexedBitmapFont`, derived
  DSFT v1 extraction, encoding-neutral bounded glyph-run and multiline-block
  rasterization with caller-selected spacing, and the
  metadata-only `font-catalog` inspection command, including map identity/hash
  and pixel-index range summaries.
- **Tests:** synthetic valid, zero-width, invalid-header, offset, dimension,
  record-length, run/block composition, blank-line, explicit-spacing, and
  raster-output-bound cases.

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
  by one pixel in both axes. Later start-window composition evidence establishes
  interface palette #1000 for these controls; title palette #11011 is limited to
  the separate title image.
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
  readers plus the read-only `ui-catalog` inspection command. DSUI v1
  transactionally extracts #18501 and #19500-#19505 with all 56 referenced controls as a
  resolved runtime graph. A shared runtime graph resolver validates identity and
  dimensions and preserves each window's ordered typed controls, coordinates,
  event masks, and optional image reference without assigning unknown behavior.
  The four complete start icon frame sets are extracted
  with interface palette #1000 and their first frames are composed at the recorded
  logical coordinates; frame-state interaction remains gated on OBS-GOG evidence.
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
  Interface palette #1000 renders the complete family coherently.
- **Confidence:** verified for identities, labels, coordinates, dimensions, and
  frame counts; medium for palette #1000; unknown for frame-state semantics,
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
  appearance; medium for palette #1000. `EXE-GOG-UI-002` establishes that the
  generic WIND lookup/register/redraw/activate path does not consume this field;
  an app-specific consumer and its draw role remain unknown.
- **Implementation:** `UiWindowResource.ImageResourceNumber`; #19004 is
  extracted as DSIX without assigning presentation semantics or treating the
  field as an automatic tile/stretch instruction.
- **Tests:** synthetic field parsing, whole-catalog reference validation,
  exact frame contract, extraction provenance, and content smoke.

### DATA-GOG-UI-004 - Character-generation modal controls

- **Question:** Which image controls compose `WIND` #19504 and #19505?
- **Method:** Resolve their ten BUTN/ICON pairs, decode every frame, and inspect
  local-only previews with the established provisional palette.
- **Finding:** #19504 contains PSIONICS, SPELLS, HALF-GIANTS, one visually blank
  control, and VIEW SPHERES; #19505 contains AIR, EARTH, FIRE, WATER, and VIEW
  PSIONICS. All icons have three 7-pixel-high frames; exact control/art widths
  and modal-relative coordinates are recorded in `OriginalContent`.
- **Confidence:** verified for identities, labels, geometry, and frame counts;
  medium for palette #1000; semantic effects and state transitions unknown.
- **Implementation:** all ten icon sets are transactionally extracted as DSIX;
  no rule behavior is assigned from labels alone.
- **Tests:** unique mappings, frame contracts, deterministic synthetic
  extraction, provenance, owned-pack verification, and content smoke.

### DATA-GOG-UI-005 - Application-frame event mask

- **Question:** Which non-geometry data varies across bounded `APFM` records?
- **Method:** Compare every 16-bit-aligned field in all 97 exact-size `APFM`
  payloads from `RESOURCE.GFF`, then test whether observed values resolve as
  resource numbers in that archive.
- **Finding:** Apart from identity and dimensions, only the 16-bit value at
  offset 88 varies. Ten values occur. The party frames use 494 for `APFM`
  #19200 and 486 for #19201; neither resolves as a resource number. `BUTN`
  shares offset 88, although all four start buttons store zero there and only
  one currently mapped generation/modal control stores a nonzero value (`BUTN`
  #2046 stores 2). EBOX uses offset 150; the seven owned edit boxes store masks
  0, 4, or 10, including mask 10 on generation-name `EBOX` #4003.
- **Executable evidence:** `EXE-GOG-UI-001` verifies the field is a mask used
  during APFM dispatch, not an appearance parameter.
- **Confidence:** verified for GOG-1432903719 as stored data and high for the
  mask role and nonzero-intersection matching; individual bit meanings remain
  unknown.
- **Implementation:** `UiApplicationFrameResource.EventMask` and
  `UiButtonResource.EventMask` preserve the shared field and the UI catalog
  reports it; `UiEditBoxResource.EventMask` preserves EBOX's corresponding
  field at offset 150. `UiEventMasks.Matches` implements the evidenced nonzero
  intersection rule. No input handler assigns meanings to the bits.
- **Tests:** synthetic nonzero event-mask decoding for all three record types,
  intersection/non-intersection cases, and existing size/dimension boundaries.
- **Uncertainty:** Map each bit to its event/input meaning with bounded call-site
  evidence and controlled runtime observations.

### DATA-GOG-UI-006 - Start-window composition and interface palette

- **Question:** What surrounds the four start controls, and which palette colors
  that composed window?
- **Method:** Decode bounded single-frame `BMP ` resources from the fingerprinted
  owned `RESOURCE.GFF`, compare palette candidates locally, and correlate exact
  resource geometry against `PLAYTHROUGH-VIDEO-001` near 2:10.
- **Finding:** `BMP ` #20029 is a 314x112 stone shell placed at (3,44), and
  `BMP ` #20028 is a 222x33 flame-and-medallion overlay placed at (47,24), on a
  black 320x200 canvas. Both layers and the four start controls use `PAL ` #1000.
  The title image appears earlier in the public sequence rather than beneath the
  controls.
- **Confidence:** high for owned resource identity, dimensions, and palette;
  medium for placement and sequencing because the corroborating playthrough's
  precise source revision is not identified and its video is compressed.
- **Implementation:** the two layers are transactionally extracted as DSIX in
  the current asset pack. The runtime draws them in recorded order, then draws the
  DSUI-resolved controls, through the shared point-sampled logical transform.
- **Tests:** exact mapping/order/geometry, distinct synthetic title/interface
  palettes, extraction provenance, exact inventory, pack verification, and
  content smoke.
- **Uncertainty:** Exact title-to-start timing, animation state, pixel aspect,
  audio, and frame-state transitions still require controlled observation.

### DATA-GOG-UI-007 - Party-overview shell composition

- **Question:** Which original images compose the first screen reached by CREATE
  CHARACTERS before individual character generation begins?
- **Method:** Decode geometry-matched `BMP ` resources from the fingerprinted
  owned archive with interface palette #1000, layer transparent candidates, and
  correlate their structural edges against `PLAYTHROUGH-VIDEO-001` around
  2:40-2:50.
- **Finding:** `BMP ` #11000 is the complete 320x200 party-overview base,
  including the outer shell, four character-slot panels, status regions, and
  bottom bar. `BMP ` #20079 overlays its 210x23 VIEW CHARACTER title at (55,0).
  This order reproduces the public frame without stretching source images.
- **Confidence:** high for owned image identities, dimensions, draw order, and
  palette; medium for coordinates because the public capture is compressed and
  its precise source revision is not identified.
- **Implementation:** both layers are transactionally extracted as DSIX in
  asset-pack format 13 and rendered when Core enters `PartyOverview`.
- **Tests:** exact mapping/order/geometry, synthetic extraction provenance,
  exact 41-file inventory, pack verification, and content smoke.
- **Uncertainty:** Party portraits, status fields, empty-slot art, BEGIN and
  slot interaction, focus, and the runtime label substitutions visible in the
  separate ADD list remain open. `EXE-GOG-UI-003` narrows the input boundary to
  the exact 319x199 `BUTN` #2099 application surface but does not establish its
  internal slot partition.

### DATA-GOG-UI-008 - Add-existing-character shell composition

- **Question:** Which original resources and placements compose the list shown
  after choosing ADD for an empty party slot?
- **Method:** Decode `WIND` #18501 and its referenced controls from the
  fingerprinted owned archive, inspect geometry-matched `BMP ` and `ICON`
  resources with `PAL ` #1000, and correlate the result against
  `PLAYTHROUGH-VIDEO-001` near 2:30-2:40.
- **Finding:** `BMP ` #10005 is the complete 320x200 runtime base even though
  the static WIND image field names #10002. Ten 163x11 controls,
  backed by four-frame `ICON` #18100 (165x11), occupy x=46 and y=31 through 130
  in 11-pixel steps. Up/down controls use #12102 at (215,30) and #12101 at (215,130);
  ADD, EXIT, and DELETE use #18104 at (231,30), #18109 at (231,50), and #18110
  at (215,148); the ADD title uses #18103 at (110,0). The static WIND records
  name SAVE imagery for two reused controls, while the observed ADD screen uses
  the listed ADD imagery, so those runtime substitutions are recorded explicitly.
- **Confidence:** high for owned resource identities, frame dimensions,
  interface palette, and WIND geometry; medium for runtime substitution because
  the corroborating public capture's exact source revision is unidentified.
- **Implementation:** seven newly mapped images are transactionally extracted
  as DSIX in asset-pack format 13. Their first frames, plus the already extracted
  EXIT image, render when Core enters `AddExistingCharacter`. WIND #18501 and
  its 17-child graph are retained in DSUI; a dedicated resolver validates the
  320x181 shell, static #10002/source-image references, runtime substitutions, child order,
  event masks, and exclusive hit rectangles. The unambiguous EXIT control maps
  to Core cancellation; row, scroll, ADD, DELETE, title, and edit-box behavior
  remain deliberately unassigned.
- **Tests:** exact asset, graph, substitution, placement, and hit contracts; frame bounds; synthetic
  extraction provenance, exact 41-file inventory, pack verification, and
  content smoke.
- **Uncertainty:** Stored-character names and portraits, row selection,
  scrolling, focus, frame-state transitions, ADD/DELETE behavior, and the
  party-slot interaction that reaches this state remain open.

## Initial rules

### RULE-INPUT-001 - Mouse-first interaction

- **Behavior:** The original requires a mouse and uses cursor modes for walking,
  looking, attacking, and targeted actions. Escape exits the active menu.
- **Preconditions:** Relevant exploration or menu state is active.
- **Evidence:** MANUAL-1994, introduction and "How to Play".
- **Confidence:** high for intended behavior; exact hit regions and shipped edge
  behavior are unknown.
- **Implementation:** the runtime resolves WIND #19500 child coordinates and
  BUTN dimensions/image references from DSUI, maps original button resource IDs
  to semantic choices, and routes clicks through a single letterboxed
  logical-canvas transform into Core commands; later screens remain unimplemented.
- **Tests:** graph completeness and unexpected identities, catalog order,
  image-reference matching, rectangle edges, wide/tall letterboxing, inverse
  coordinates, resulting Core routing, and owned-pack content smoke.
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

### DATA-GOG-CHAR-001 - Character identity slot

- **Question:** Can original character records be identified without assigning
  meanings to unknown character-state fields?
- **Method:** Enumerate every `CHAR` resource in the fingerprinted owned
  `CHARSAVE.GFF`, compare the repeated header region, and validate the candidate
  name slot independently for bounds, termination, and printable bytes.
- **Finding:** All 19 records contain a non-empty printable ASCII name beginning
  at offset 43 and terminated within a 16-byte slot. Several records retain
  unrelated nonzero bytes after the first terminator, so those bytes are not
  part of the name and must be ignored. Record sizes vary from 145 to 1,036
  bytes; no other field is assigned semantics by this finding. Later findings
  separately cover the record envelope and ability-score fields.
- **Confidence:** high for the owned GOG build's name slot; unknown for other
  editions. Other `CHAR` fields are tracked by their own evidence entries.
- **Implementation:** bounded `GffCharacterIdentity` parsing and a metadata-only
  `character-catalog` Inspect command. No original record bytes enter the
  derived asset pack or Git.
- **Tests:** exact versioned-envelope validation, first-NUL behavior, maximum
  name length, empty name, missing terminator, and non-printable byte rejection.

### DATA-GOG-CHAR-002 - Raw PSIN companion mask

- **Question:** Does each original character record have a bounded same-number
  `PSIN` companion independently of the variable `CHAR` body?
- **Method:** Enumerate `PSIN` and `CHAR` resources in the fingerprinted owned
  `CHARSAVE.GFF`, compare identities and lengths, and inspect the complete
  one-byte value domain without assigning names to bits.
- **Finding:** Every one of the 19 `CHAR` identities has exactly one same-number
  `PSIN` companion and there are no unmatched companions. Every `PSIN` payload
  is exactly one byte; the owned values are 1, 2, 4, 5, 6, and 7, all nonzero
  subsets of the low three bits. Value 3 is the only in-envelope combination
  not present in this archive.
- **Corroboration:** MANUAL-1994 documents three psionic disciplines, exactly
  one selection for created non-psionicists, and all three for created
  psionicists. That supports investigating a three-bit relationship, but owned
  values 5 and 6 show that the complete archive cannot be interpreted as only
  those creation-screen cardinalities; NPC data or different semantics remain
  possible.
- **Confidence:** high for one-to-one resource correlation, one-byte size, and
  the nonzero three-bit structural envelope; unknown for individual or combined
  bit meanings and whether other supported editions use the same representation.
- **Implementation:** bounded `GffPsionicMask` raw-mask parsing and a
  shared `GffCharacterCatalog` that requires exact `CHAR`/`PSIN` correlation;
  the metadata-only Inspect command reports the catalog's raw mask. Core
  discipline names are deliberately not inferred.
- **Tests:** all observed values, the unobserved in-envelope combination, zero,
  unknown bits, wrong lengths, deterministic identity ordering, and missing or
  orphan companion rejection in synthetic GFF archives.

### DATA-GOG-CHAR-003 - Ordered ability scores

- **Question:** Does the fixed `CHAR` header store the six player ability scores,
  and in which order?
- **Method:** Compare bytes 35 through 40 across all 19 owned `CHAR` records,
  check every value against the manual's documented 9..24 score range, compare
  tuples for repeated character identities, and retain the manual's presentation
  order rather than assigning fields from value plausibility alone.
- **Finding:** Every record has six values within 9..24 at offsets 35..40. The
  manual introduces its six ability descriptions in Strength, Dexterity,
  Constitution, Intelligence, Wisdom, and Charisma order. Repeated identities
  preserve the same tuple in three groups; another variant changes only one score.
- **Evidence:** MANUAL-1994, "Ability Scores," page 16; fingerprinted owned
  `CHARSAVE.GFF` metadata inspection.
- **Confidence:** high for the six fields, their order, and the supported GOG
  build; runtime presentation and generation behavior remain unobserved.
- **Implementation:** bounded `GffCharacterAbilityScores` parsing with named
  ordered fields; `GffCharacterCatalog` exposes the value without translating
  it into Core character state or applying origin modifiers.
- **Tests:** exact order, inclusive manual bounds, lower/upper rejection,
  resource-size limits, and synthetic catalog integration.
- **Uncertainty:** Ability generation distribution, origin-modifier application
  order and caps, edit behavior, and the supplied-party record selection remain
  open.

### DATA-GOG-CHAR-004 - Versioned record envelope

- **Question:** What structural boundary contains the fixed `CHAR` header and
  its still-uninterpreted variable tail?
- **Method:** Compare the first two bytes and total resource size across all 19
  owned records, testing a single fixed-header-plus-repeated-record equation.
- **Finding:** Byte 0 is version 1 in every record. Byte 1 ranges from 2 through
  29, and every resource size exactly equals `79 + byte1 * 33`; there are no
  residual bytes or exceptions. The repeated tail record's meaning is unknown.
- **Confidence:** verified for the supported GOG archive's structural envelope;
  unknown for other versions and tail semantics.
- **Implementation:** `GffCharacterRecordEnvelope` requires version 1, a
  complete 79-byte header, the exact byte-counted tail size, and no trailing
  data. Identity and ability readers validate this envelope before interpreting
  their fields; the catalog exposes the neutral `TailRecordCount`.
- **Tests:** zero, ordinary, and maximum byte counts; unsupported version;
  truncated header; truncated tail; trailing byte; and integration through the
  identity, ability, and catalog readers.

### DATA-GOG-CHAR-005 - Bounded derived character catalog

- **Question:** Can the verified character subset be extracted for later start
  flow work without preserving unknown original state or guessing which records
  form the supplied party?
- **Method:** Correlate same-number `CHAR` and `PSIN` records, retain only the
  separately evidenced identity, ordered abilities, neutral tail count, and raw
  mask, then round-trip that subset through a canonical derived format. Validate
  the complete result during a temporary owned-source extraction.
- **Finding:** All 19 correlated records convert to deterministic `DSCH` v1 in
  numeric resource order. The derived catalog contains no uninterpreted source
  bytes and makes no claim about party membership or PSIN bit meanings.
- **Confidence:** verified for extraction of the bounded subset from the
  supported GOG build; runtime selection and remaining field semantics are
  unknown.
- **Implementation:** `PackedCharacterCatalog` plus transactional extraction to
  `characters/catalog.dsch`; the asset-pack manifest is version 13 and records
  `CHARSAVE.GFF` provenance.
- **Tests:** synthetic round-trip and deterministic ordering; duplicate,
  malformed-field, noncanonical-order, truncation, and trailing-data rejection;
  synthetic extraction and content-smoke validation without original content.

### DATA-GOG-CHAR-006 - Disc and installed character sets

- **Question:** Which character resources are immutable disc data, which were
  added by the installed 1.1-era files, and does either set identify the four
  members selected by START GAME?
- **Method:** Parse `game.gog` as a bounded Mode 2/2352 ISO 9660 image, inspect
  only its 3,864-byte `CHARSAVE.GFF`, compare the resulting resource identities
  with the installed 11,735-byte archive, and remove the temporary extraction.
  The original files and character names were not retained. A public gameplay
  report that identifies one named default member was correlated locally by
  identity, but only numeric resource facts are recorded here.
- **Finding:** The disc archive contains exactly eight correlated `CHAR`/`PSIN`
  pairs: #40-#43 and #50-#53. The installed archive contains 19 pairs:
  #29-#43 and #50-#53. The externally identified default member matches disc
  resource #43 and an installed duplicate at #33; the report also identifies
  that character as a half-giant gladiator. This establishes one default member
  and shows that installed character storage is not an immutable edition
  fingerprint, but it does not prove which other three records START GAME uses.
- **Static cross-check:** In the fingerprinted executable, neither candidate
  block appears as four adjacent 16-bit little-endian values. The byte sequence
  for #40-#43 is absent. Two byte-sequence matches for #50-#53 at `5000:a368`
  and `5000:a379` are the upper- and lower-case ASCII hexadecimal digit tables,
  so they were rejected as false positives.
- **Evidence:** fingerprinted GOG `game.gog` and installed `CHARSAVE.GFF`;
  `EXE-GOG-CHAR-001`; secondary corroboration from the Steam community review
  at <https://steamcommunity.com/profiles/76561198045525236/recommended/1904580>.
- **Confidence:** verified for both resource inventories; medium for #43 as one
  default member because independent gameplay reporting and the disc identity
  agree; unknown for the complete four-member selection.
- **Implementation consequence:** keep `CHARSAVE.GFF` structural and mutable,
  preserve numeric identities in DSCH, and do not encode a default-party table
  until runtime or another independent source establishes all four members.

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
  Occupied-slot EDIT replaces the selected member atomically; DROP transfers
  the member to bounded recreation-native character storage and ADD can restore
  it. When a party member carries eligible class progression, DUAL enters a
  dedicated selection state and atomically applies an accepted next class; the
  separate evidenced progression rule is implemented by `RULE-PARTY-004`.
- **Tests:** every start choice, NEW/ADD/CANCEL routing, valid and invalid
  completion, occupied-slot edit/drop/add/DUAL, atomic replacement, persisted
  edit and DUAL targets, empty/nonempty party start, cancellation, and
  wrong-screen rejection.
- **Uncertainty:** Pregenerated member records, saved/created-character formats,
  DUAL presentation/class-choice filtering, the exact early-start control, and
  shipped cancellation edge cases remain unimplemented until their data and
  runtime behavior are observed.

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
  origin-modifier application timing/caps during generation and editing, valid
  multiclass combinations, and exact screen defaults need DATA-GOG/OBS-GOG
  evidence. The modifier values themselves are recorded by `RULE-PARTY-005`.

### CONFLICT-PARTY-001 - Manual origin/class eligibility lists

- **Claim A:** The race descriptions list the classes allowed for each race.
- **Claim B:** The class descriptions independently list allowed races.
- **Conflict:** The lists disagree for half-giant ranger and thief, mul druid,
  and thri-kreen druid and thief. They may contain additional combination rules
  that prose alone does not expose.
- **Evidence:** MANUAL-1994, race descriptions on pages 17-18 and class
  descriptions on pages 19-22.
- **Implementation:** these pairs return `EvidenceConflict` and validation emits
  `class_origin_unresolved`; no eligibility is guessed.
- **Resolution needed:** record the selectable class list for every origin in the
  fingerprinted GOG build, then update the matrix and tests.

### RULE-PARTY-003 - Psionic disciplines and clerical spheres

- **Behavior:** A psionicist specializes in Psychokinesis, Psychometabolism,
  and Telepathy. Every other character selects exactly one of those disciplines,
  with Psychokinesis documented as the initial selection. A cleric selects one
  elemental sphere from Air, Earth, Fire, and Water, with Air documented as the
  initial selection; non-clerics do not select a clerical sphere.
- **Evidence:** MANUAL-1994, character-generation instructions on pages 7-9.
- **Confidence:** high for documented intent; the supported executable's
  initial selections and selection transitions have not yet been observed.
- **Implementation:** `CharacterDraft` records both choices and
  `PartyCreationRules` validates their class-dependent cardinality. Both fields
  participate in the canonical start-flow state hash and snapshot schema 4.
- **Tests:** all-three Psionicist requirement, exactly-one non-Psionicist
  requirement, Cleric-only elemental sphere, and sphere-sensitive state hashes.
- **Uncertainty:** Shipped defaults, control-state frames, click transitions,
  and whether any exceptional class combination changes these rules need
  OBS-GOG evidence.

### RULE-PARTY-004 - Human dual-class progression

- **Behavior:** Only humans may become dual-classed. The current class must be
  at least level three before changing; the new class begins at level one and
  the old class never advances again. Former-class abilities remain unavailable
  until the new class level exceeds the former level. A human may repeat the
  process once, for at most three sequential careers.
- **Evidence:** MANUAL-1994, party-modification instructions on pages 9-10 and
  "Character Classes" on page 19.
- **Confidence:** high for documented intent; XP thresholds, shipped DUAL
  availability, class-choice filtering, and multi-career edge behavior remain
  unobserved.
- **Implementation:** immutable `DualClassProgression` records ordered
  class/level careers, starts accepted new careers at level one, advances only
  the current career, reports stable rejection diagnostics, and evaluates the
  documented strict level-exceeds boundary for former benefits. `CharacterDraft`
  carries that progression, validates it against the ordered class list, and
  `StartFlow` exposes a deterministic DUAL selection command. Snapshot schema 4
  and the canonical state hash include every career and level.
- **Tests:** human-only and level-three gates, duplicate-class and three-career
  limits, transition immutability, monotonic current-level advancement, and
  equal/exceeded former-level boundaries, class/progression consistency,
  atomic DUAL selection, cancellation, command payload validation, snapshot
  restore, and progression-sensitive state hashes.
- **Uncertainty:** Player-visible selection, native-save integration, initial
  shipped class levels/experience, XP thresholds, and exact DUAL UI choices
  require later Slice 2/5 data and observations.

### RULE-PARTY-005 - Origin ability modifiers

- **Behavior:** Human ability scores are unmodified. Dwarves receive Strength
  +1, Dexterity -1, Constitution +2, and Charisma -2; elves receive Dexterity
  +2, Constitution -2, Intelligence +1, and Wisdom -1; half-elves receive
  Dexterity +1 and Constitution -1; half-giants receive Strength +4,
  Constitution +2, Intelligence -2, Wisdom -2, and Charisma -2; halflings
  receive Strength -2, Dexterity +2, Constitution -1, Wisdom +2, and Charisma
  -1; muls receive Strength +2, Constitution +1, Intelligence -1, and Charisma
  -2; thri-kreen receive Dexterity +2, Intelligence -1, Wisdom +1, and Charisma
  -2. Unlisted abilities receive zero.
- **Evidence:** MANUAL-1994, "Racial Ability Adjustments Table," page 77.
- **Confidence:** high for the published modifiers; application order, edit
  behavior, and whether final scores are capped remain unobserved.
- **Implementation:** `PartyCreationRules.AbilityModifiers` returns an immutable
  six-field modifier value for every defined origin and rejects undefined enum
  values. It does not mutate `CharacterDraft` or invent generation behavior.
- **Tests:** exact six-ability values for all eight origins, complete enum
  coverage, and undefined-origin rejection.
- **Uncertainty:** Observe generation and editing at origin-specific and global score
  boundaries before applying the table to final character state.

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

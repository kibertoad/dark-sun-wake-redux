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
  VGA colors. Each six-bit channel expands to eight bits by repeating its high
  two bits into the low two positions; this matches the native DOSBox palette
  rather than leaving full intensity at 252. The readers decode 4,510 images
  containing 9,279 frames and all 40 palettes in the installed build.
- **Confidence:** verified for these payload structures and counts in
  GOG-1432903719; resource meaning, palette pairing, placement, and timing are
  still unknown.
- **Implementation:** `DarkSunWakeRedux.Resources.IndexedImage`,
  `IndexedPalette`, and the read-only `image-catalog` inspection command.
- **Tests:** synthetic row/planar decoding, transparency, palette conversion,
  and malformed size, bounds, component, and bitstream cases.

### DATA-GOG-REGION-001 - Region map structural catalog

- **Question:** Which bounded region structures identify Tyr and supply its
  terrain grid, geometry bytes, tile images, and placed-object references?
- **Method:** Compare DSUN-MUSIC commit
  `79b692770caebda3de685feaf42906aae31572d1` with an independent bounded
  reader, then inspect all 20 owned `RGN*.GFF` files against the fingerprinted
  `OBJEX.GFF`. Record only structural summaries; do not retain decoded maps or
  images in Git.
- **Finding:** Every region contains one same-number `RNME`, `PAL `, `MAP `,
  `GMAP`, and `ETAB`. `RNME` is printable ASCII terminated by NUL. Both map
  planes contain exactly 12,544 bytes arranged as 128 columns by 98 rows.
  Every `MAP ` byte resolves to a local, single-frame 16x16 `TILE`. `ETAB` is a
  sequence of eight-byte records containing two signed 16-bit coordinates, a
  signed vertical-offset byte, a flags byte, and a signed 16-bit object number;
  the absolute object number resolves to `OJFF` in `OBJEX.GFF`. The geometry
  bytes and entity flags are retained without assigning gameplay meaning at
  this structural layer. `EXE-GOG-REGION-001` separately establishes geometry
  bit `0x40` as the movement-blocking bit.
  Across the owned set, 3,043 tiles decode and 13,559 entity references resolve.
  `RGN032.GFF` (64,641 bytes, SHA-256
  `6224aefe947062141f2102bfa1098d9e7fa0e1f6eab8d385eaf2a7d2a434d04e`)
  identifies resource #50 as `Tyr`.
- **Confidence:** verified for sizes, identities, decoding, and reference
  integrity in GOG-1432903719; medium for coordinate/vertical-field names from
  the corroborating research; high for the separately recorded `0x40`
  terrain-blocking meaning; unknown for other geometry bits, entity flags other
  than the separately corroborated mirror bit and actor-specific collision;
  the opening actor anchor is established separately by `DATA-GOG-ACTOR-001`.
- **Implementation:** `DarkSunWakeRedux.Resources.GffRegion`, canonical bounded
  `PackedRegion` DSRG v1, the read-only `region-catalog` inspection command, and
  transactional extraction of region #50 to `regions/tyr.dsrg`. The pack keeps
  decoded data only and records `RGN032.GFF` plus `OBJEX.GFF` reference-validation
  provenance; it does not copy GFF payloads.
- **Tests:** synthetic successful decoding plus malformed name, plane length,
  tile frame, partial entity record, missing tile, and missing object cases;
  DSRG round-trip, deterministic tile order, exact length/header/dimensions,
  map reference, and binary-alpha rejection; synthetic transactional extraction
  and owned pack/content-smoke validation.

### DATA-GOG-OBJECT-001 - Object-frame structural catalog

- **Question:** Which bounded object definitions and indexed images are
  referenced by Tyr's placed-object table?
- **Method:** Compare DSUN-MUSIC commit
  `79b692770caebda3de685feaf42906aae31572d1` with an independent bounded
  reader, validate every `OJFF` in the fingerprinted `OBJEX.GFF`, and then
  restrict the same validation to the absolute object numbers referenced by
  Tyr's `ETAB`. Record metadata summaries only; do not retain decoded images.
- **Finding:** Every owned `OJFF` is exactly 16 bytes. Signed 16-bit X and Y
  offsets occur at bytes 2 and 4, an unsigned 16-bit `BMP ` resource reference
  occurs at byte 12, and the final word is zero. Words at bytes 0, 6, 8, and 10
  remain uninterpreted and are preserved raw. All 4,479 definitions resolve to
  decodable images: 3,002 distinct images containing 5,600 frames, with maximum
  dimensions 64x64. Tyr references 287 definitions, resolving to 246 distinct
  images and 477 frames, also no larger than 64x64.
- **Confidence:** verified for record size, zero final word, reference integrity,
  image decoding, counts, and dimensions in GOG-1432903719; medium for the X/Y
  offset names from corroborating research; unknown for the raw words, frame
  animation, draw order, anchoring, collision, and interaction semantics.
- **Implementation:** `DarkSunWakeRedux.Resources.GffObjectFrameCatalog`, the
  read-only `object-catalog` inspection command, and canonical transactional
  extraction of Tyr's bounded graph to `regions/tyr-objects.dsob`. DSOB retains
  decoded indices/alpha and bounded raw definition words, but no source offsets
  or GFF payload envelope.
- **Tests:** synthetic full/subset decoding, canonical order, shared-image cache,
  DSOB round-trip/extraction, and malformed length, ordering, reserved word,
  disconnected reference, frame, alpha, missing definition/reference, empty
  image, and trailing-data rejection; owned pack/content-smoke validation.

### DATA-GOG-SCENE-001 - Static region composition

- **Question:** How do the bounded terrain tiles and first object frames compose
  into region pixel coordinates without inferring gameplay behavior?
- **Method:** Compare the region renderer at DSUN-MUSIC commit
  `79b692770caebda3de685feaf42906aae31572d1`, including its note that the
  object procedure was informed by the original Shattered Lands executable,
  against the independently decoded DSRG/DSOB structures. Implement the same
  transform with strict clipping and synthetic pixel-level tests. Then start
  the fingerprinted owned build in the GOG overlay, select the shipped party,
  skip the opening cinematics, capture the first stable 320x200 gameplay frame,
  and compare 808 background samples against every valid Tyr viewport while
  excluding the party, pointer, and interface button. At the winning origin,
  compare all 64,000 RGB pixels with the compositor and enumerate the
  eight-connected components of differing pixels.
- **Finding:** Terrain `MAP ` entry `(x,y)` places its 16x16 tile at
  `(x*16,y*16)`. After terrain, ETAB entries compose in stored order using the
  first referenced BMP frame. The frame's top-left is
  `(entityX-objectXOffset, entityY-objectYOffset-verticalOffset)`, and ETAB flag
  bit `0x80` mirrors it horizontally. Tyr's GMAP low five bits are zero, so the
  corroborating renderer's wall-resource branch does not add wall images there.
  Controlled observation `OBS-GOG-SCENE-001` used `DSUN.EXE` SHA-256
  `CE02EE1F31C2339FC3E16926E370639AF782A5ECD6C8A6081140FA23445FC92C`;
  its ignored native frame SHA-256 is
  `C4C75F4CDCAA19EBD825EE560887E0667984E5A3878D695703587863F535CED9`.
  The first stable gameplay background matches Tyr origin `(1024,1368)` on
  807 of 808 sampled pixels; the sole difference is a dynamic overlay sample.
  An exact full-frame comparison at that origin identifies 1,028 pixels that
  differ from the static compositor. The visually identified party-leader
  component contains 367 differing pixels in logical bounds
  `(160,91)`-`(176,125)`, corresponding to world bounds
  `(1184,1459)`-`(1200,1493)`. `DATA-GOG-ACTOR-001` identifies its exact
  source image and `EXE-GOG-REGION-001` ties the stored top-left to collision
  anchor cell `(74,91)`; the occupied-cell footprint remains unknown.
- **Confidence:** high for tile placement from the format and all owned map
  dimensions; medium for object placement, first-frame choice, ETAB order, and
  mirror bit from community executable-informed research; verified for the
  opening static background in the supported owned build. Animation, party
  footprint, pointer, and interface overlays remain unvalidated.
- **Implementation:** `RegionSceneRasterizer` produces a bounded indexed/alpha
  viewport at an explicit caller-supplied world origin. `OpeningTyrScene`
  records the observed `(1024,1368)` 320x200 viewport, and Gameplay displays
  that static background. The exact leader is a separate actor overlay; the
  compositor does not mutate state, interpret collision, draw the interface,
  or advance animation.
- **Tests:** synthetic cross-tile cropping, offsets, vertical adjustment,
  horizontal mirroring, transparent pixels, ETAB overdraw order, edge clipping,
  viewport bounds, malformed region data, and disconnected object references;
  owned no-window content smoke rasterizes both opposite map corners and the
  evidenced opening viewport.

### DATA-GOG-ACTOR-001 - Opening leader resource and placement

- **Question:** Which owned sprite produces the isolated opening leader, and
  how do its visible and collision coordinates relate?
- **Method:** Compare every decoded `OBJEX.GFF` OJFF/BMP frame having the
  measured 17x35 geometry, in both orientations, over the exact Tyr compositor
  background against the ignored native observation `OBS-GOG-SCENE-001`.
  Independently trace the supported executable's actor coordinate fields into
  its render and collision call paths.
- **Finding:** Of 12 same-geometry orientation candidates, unmirrored OJFF #305
  -> BMP #599 frame 0 is the unique exact match: all 17x35 pixels agree and 367
  are opaque. The BMP contains 13 frames with dimensions
  `17x35,17x36,11x33,18x38,18x38,16x38,16x38,13x33,22x32,15x32,20x32,28x30,28x27`;
  their state/direction meanings are not established. Frame 0's top-left is
  logical `(160,91)` at the observed camera, hence
  world `(1184,1459)`. The executable reads the same stored actor X/Y fields
  for rendering and shifts them right by four before footprint enumeration, so
  the evidenced collision anchor is cell `(74,91)`.
- **Confidence:** verified for the resource, first-frame orientation, palette,
  opening position, and visible pixels in GOG-1432903719; high for the anchor
  relationship from bounded executable paths. Footprint, animation sequence,
  cadence, and later actor state remain unknown.
- **Implementation:** pack v21 retains
  `images/exploration/opening-leader.dsix`, preserving OBJEX and Tyr-palette
  provenance. Gameplay draws frame 0 through reusable world-to-camera actor
  placement, with viewport-edge culling. Runtime Walk clicks now move its
  authoritative anchor. Fixed-point interpolation moves that verified frame
  continuously toward the next semantic route anchor without affecting Core
  occupancy or route timing; other frame selection remains evidence-gated.
- **Tests:** synthetic OJFF/BMP extraction, exact provenance/palette/geometry,
  camera translation, rectangle-edge visibility, anchor constants, pack
  inventory verification, and owned no-window content smoke.

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
  the current asset-pack format and rendered when Core enters `PartyOverview`.
- **Tests:** exact mapping/order/geometry, synthetic extraction provenance,
  exact current-pack inventory, pack verification, and content smoke.
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
  as DSIX in the current asset-pack format. Their first frames, plus the already extracted
  EXIT image, render when Core enters `AddExistingCharacter`. WIND #18501 and
  its 17-child graph are retained in DSUI; a dedicated resolver validates the
  320x181 shell, static #10002/source-image references, runtime substitutions, child order,
  event masks, and exclusive hit rectangles. The unambiguous EXIT control maps
  to Core cancellation; row, scroll, ADD, DELETE, title, and edit-box behavior
  remain deliberately unassigned.
- **Tests:** exact asset, graph, substitution, placement, and hit contracts; frame bounds; synthetic
  extraction provenance, exact current-pack inventory, pack verification, and
  content smoke.
- **Uncertainty:** Stored-character names and portraits, row selection,
  scrolling, focus, frame-state transitions, ADD/DELETE behavior, and the
  party-slot interaction that reaches this state remain open.

### DATA-GOG-UI-009 - Game Menu graph and artwork

- **Question:** Which owned resources compose the Game Menu opened during
  exploration, and which controls can be mapped without guessing behavior?
- **Method:** Decode `WIND` #10500 and its complete child graph from the
  fingerprinted `RESOURCE.GFF`; correlate its geometry with `BMP ` #10000,
  decode every referenced `ICON` with `PAL ` #1000, and compare their visible
  symbols and relative positions with MANUAL-1994 pages 14-16.
- **Finding:** `WIND` #10500 and `BMP ` #10000 are both 210x116. The window has
  30 ordered children: 14 `BUTN` controls and 16 `APFM` records. The controls
  form exact 4/5/5 rows and their decoded symbols identify View Character,
  View Inventory, Cast Spells/Use Psionics, Current Spell/Effects, Exit to DOS,
  Load/Save, Preferences, Overhead Map, Center on Leader, Collapse Party, Walk,
  Look, Attack, and Return to Game. Every button identity, coordinate,
  dimension, image reference, and frame count is bounded.
- **Confidence:** verified for owned graph/image identity and geometry; high for
  semantics from the manual/image correlation. The source graph contains no
  canvas origin. Centering the 210x116 panel on the 320x200 canvas at (55,42) is
  a provisional presentation choice pending controlled native measurement.
- **Implementation:** asset-pack format 21 includes the base, all 14 button images,
  and `ui/game-menu.dsui`. `GameMenuInput` resolves the DSUI graph into semantic
  absolute hit rectangles; MonoGame draws first frames over the Tyr viewport.
  Character, inventory, cast/psionic, effects, overhead-map, Walk/Look/Attack,
  Preferences, and Return route to existing deterministic Core commands. Center on Leader
  supplies the moving sprite's world center to a bounded Core camera command;
  despite its counterintuitive label, the manual says Collapse Party selects
  expanded all-party display outside combat, so it routes to that distinct Core
  command. Exit to
  DOS issues a distinct deterministic exit request, so it does not accidentally
  share Escape's menu-dismissal behavior; the runtime then exits cleanly.
  Load/Save remains visibly inert until its destination behavior is implemented.
- **Tests:** exact unique mapping, synthetic transactional extraction and
  provenance, complete graph resolution, image/frame contracts, absolute and
  exclusive hit rectangles, semantic routing, explicit exit handling, the
  remaining inert action,
  actor-relative center routing, camera clamping/menu return, collapse state,
  malformed image references, pack verification, and no-window content smoke.
- **Uncertainty:** Native panel origin, frame-state selection, hover/press timing,
  description-bar text, Load/Save behavior, exact native centering policy,
  and destination-screen presentation.

### DATA-GOG-UI-010 - Character and inventory destination graphs

- **Question:** Which owned UI resources establish the character and inventory
  destination shells and their reusable navigation boundary?
- **Method:** Decode every `WIND`, `BUTN`, `APFM`, and `EBOX` record in the
  fingerprinted `RESOURCE.GFF`; compare the 11xxx and 13xxx resource families,
  their 320-wide geometry, decoded `BMP ` artwork under `PAL ` #1000, and the
  shared button identities already correlated with MANUAL-1994.
- **Finding:** `WIND` #11500 is 320x189 with 86 ordered controls and shares
  BUTN #10300/#11304/#11305/#11306/#10308 at `(43,155)`, `(67,155)`,
  `(91,155)`, `(114,155)`, and `(253,155)`. Its character shell is the already
  mapped 320x200 `BMP ` #11000 plus the VIEW CHARACTER title. `WIND` #13500 is
  320x200 with 89 controls and places those same five navigation identities at
  `(163,181)`, `(187,181)`, `(211,181)`, `(235,181)`, and `(288,181)` over the
  320x200 `BMP ` #13001 inventory shell. The shared identities preserve View
  Character, View Inventory, Cast/Use Psionics, Current Effects, and Return.
- **Confidence:** verified for resource identity, geometry, image decoding,
  graph membership, and shared navigation semantics; character/inventory
  association is high from resource-family structure, artwork, and manual
  function. Interior-control meaning, dynamic draw order, and frame states are
  unknown.
- **Implementation:** pack format 21 includes the inventory base and a separate
  two-window destination DSUI while reusing the already extracted character
  shell and five icon assets. `ExplorationDestinationInput` is one semantic
  page object for both layouts. MonoGame renders each shell, routes only those
  five established controls, and leaves all uninterpreted controls inert.
- **Tests:** page selection, exact window/button/image/geometry contracts,
  exclusive hit rectangles, semantic commands, malformed-page rejection,
  synthetic transactional extraction, complete owned graph counts, pack
  verification, and no-window content smoke.
- **Uncertainty:** Character fields, inventory objects and transfers, party
  selection, interior actions, and native focus/hover/press frames remain open.

### OBS-GOG-UI-001 - Cast and Current Effects stable screens

- **Question:** Which shell, title resources, and shared controls are visibly
  active after the documented C/U and E hotkeys?
- **Method:** Run the supported GOG build in DOSBox, enter Tyr with the supplied
  party, select leader AR'ANDA, invoke C and E separately, and retain the two
  resulting 320x200 captures only under ignored `analysis/original/`. Compare
  their stable pixels and control placement with the bounded RESOURCE.GFF
  image and UI catalogs.
- **Finding:** Cast Spells / Use PSI reuses the 320x200 `BMP ` #11000 character
  shell, centers 104x23 `BMP ` #20080 (USE) at `(108,0)`, and retains the
  `WIND` #11500 bottom navigation. Current Spell / Effects reuses the same
  shell and navigation, centering 152x23 `BMP ` #20075 (EFFECTS) at `(84,0)`.
  Their populated interior regions differ, but those fields and actions are not
  established by these two captures.
- **Confidence:** verified for the observed owned-build composition and title
  placement; high for the shared navigation association corroborated by the
  manual and exact #11500 graph. Dynamic content and state-dependent variation
  remain unknown.
- **Implementation:** pack format 21 extracts both title images. The runtime
  composes each title over the shared character base and resolves the same five
  #11500 navigation controls for Character, Cast, and Effects destinations.
- **Tests:** exact title identity/geometry/placement, transactional extraction,
  shared page resolution, semantic navigation, pack verification, and
  no-window content smoke.

### DATA-GOG-UI-011 - Preferences graph and artwork

- **Question:** Which owned resources form the Preferences destination, and
  which interactions are safe to expose before setting ranges are measured?
- **Method:** Decode `WIND` #16500 and every referenced `BUTN`, `APFM`, and
  `ICON` from the fingerprinted `RESOURCE.GFF`; render with `PAL ` #1000 and
  correlate the graph with MANUAL-1994 page 15.
- **Finding:** #16500 is 210x116 with 15 ordered children: two application
  frames and 13 buttons. It reuses `BMP ` #10000 at the same provisional
  centered origin as Game Menu. The graph identifies music, sound-effects,
  animations, voice-effects, About, paired music/sound/difficulty controls,
  Game Menu, and Return. All identities, positions, dimensions, images, and
  frame counts are bounded. `EXE-GOG-UI-004` establishes a four-entry
  Easy/Balanced/Hard/Hideous difficulty label table, an exact ten-string
  Preferences description span, and a nine-line centered About payload. The
  manual establishes the toggle and slider roles but not
  their numeric ranges, increments, toggle defaults, or exact frame-state policy;
  its “Average” default wording conflicts with the executable's Balanced label.
- **Confidence:** verified for the owned graph/image contracts; high for the
  labeled roles and navigation from the manual, and high for the executable
  text-table shapes. Description-role ordering remains medium confidence until
  hover behavior is observed. Numeric setting behavior, the selected difficulty
  default, and panel-origin parity remain open.
- **Implementation:** pack format 26 expands `ui/game-menu.dsui` with #16500,
  extracts eight additional unique images, and renders the authentic base and
  first frames. `PreferencesInput` supplies exclusive absolute hit rectangles;
  Game Menu and Return navigate deterministically while setting mutations stay
  inert until their boundaries are evidenced. A bounded fixed-edition reader
  extracts the four difficulty labels, ten descriptions, and nine About lines
  into `text/preferences.dstx` without committing their payload.
- **Static-analysis boundary:** the bounded direct-reference and scalar probes
  recorded in `EXE-GOG-UI-004` found no literal executable binding between the
  difficulty table, its labels, or the two difficulty-button IDs. This is not
  evidence that the settings path is absent; it is evidence that a direct
  literal mapping cannot safely supply its behavior.
- **Tests:** exact graph/button/image geometry, duplicate/drift rejection,
  exclusive hit edges, navigation and inert-action routing, transactional
  extraction, executable string truncation/prefix/length checks, content smoke,
  and exact owned-pack verification.
- **Uncertainty:** numeric ranges, adjustment steps, toggle defaults, selected
  difficulty default, About presentation/dismissal, selected/disabled frames,
  description-role ordering/presentation, and native origin.

### DATA-GOG-SOUND-001 - SOUND.CFG structural envelope

- **Question:** What safe structural facts can be established about the owned
  sound configuration before assigning it to Preferences behavior or writing a
  reader?
- **Method:** Inspect the fingerprinted 59-byte `SOUND.CFG` from
  GOG-1432903719 as a bounded byte sequence. Record only repeated-field,
  ASCII-envelope, padding, and length facts; do not execute or copy its
  payload into the repository.
- **Finding:** bytes `0x00..0x09` and `0x0a..0x13` are identical ten-byte
  blocks. Two fixed ten-character ASCII identifiers ending in `.adv` begin at
  `0x16` and `0x24`, each followed by zero padding. The remaining bytes begin
  at `0x32` and form a 13-byte scalar area. The two identifiers and remaining
  scalar values have no assigned semantics.
- **Confidence:** high for the observed file length, repeated prefixes,
  identifier envelopes, padding, and trailing boundary in the supported build;
  unknown for all field meanings and cross-file relationships.
- **Implementation consequence:** no production reader or setting mutation is
  introduced. A future reader must first extend the source manifest contract,
  bound each fixed region, reject unsupported length/terminator/padding forms,
  and acquire independent evidence before mapping a field to UI, volume,
  driver selection, playback, or timing.
- **Uncertainty:** source versus runtime ownership, module-selection semantics,
  every scalar-field role, interaction with `SOUND.INI`/`SOUND_DS.EXE`, device
  detection, mixer defaults, voice/music routing, codecs, and playback timing.

### DATA-GOG-ITEMS-001 - ITEMS.BIN fixed-width pair envelope

- **Question:** Does the owned `ITEMS.BIN` have a bounded record envelope that
  can be described without assigning item, equipment, or combat semantics?
- **Method:** Inspect its fingerprinted 936 bytes as little-endian 16-bit
  values and calculate only count, uniqueness, ordering, and range properties.
  No source bytes are retained in the repository.
- **Finding:** the entire file divides exactly into 234 four-byte pairs. Every
  first value is unique; pair indices 0 through 232 increase strictly by their
  first value, while pair 233 is a unique final out-of-order entry. The second
  values contain 159 distinct values across the 234 pairs. Both fields are
  unsigned 16-bit envelopes only; no field names or roles are assigned.
- **Confidence:** high for length, pair width/count, endianness, uniqueness,
  ordering boundary, and second-value duplication in GOG-1432903719; unknown
  for pair meanings, lookup direction, the last entry's role, and all item or
  gameplay semantics.
- **Implementation consequence:** no reader is introduced yet. A future
  approved reader must require the exact supported source fingerprint, parse
  all 234 pairs transactionally, reject any incompatible envelope, and retain
  neutral pair names until independent executable or runtime evidence assigns
  semantics.
- **Uncertainty:** whether either value is an item ID, image/object reference,
  resource alias, equipment property, lookup key, or compatibility remap;
  whether the final pair is a sentinel, exceptional alias, or normal record;
  and all inventory/combat effects.

### DATA-GOG-CURSOR-001 - Exploration cursor family and hotspot

- **Method:** Render every low-numbered `RESOURCE.GFF` image candidate with
  `PAL ` #1000, correlate the resulting family against MANUAL-1994 pages 4-6,
  and compare six controlled owned captures made at the unchanged Tyr opening
  camera with Walk, melee Attack, and Look over possible and impossible targets.
  Use the manual's instruction to aim with the upper-left corner as the hotspot
  contract. At the valid melee hotspot, resolve the topmost opaque Tyr entity
  through the already verified ETAB/OJFF first-frame transform.
- **Finding:** `ICON` #19101 through #19110 are ten single-frame cursor images:
  Walk 10x13, Can't Walk 16x16, melee Attack 16x17, invalid melee Attack 16x17,
  ranged Attack 14x15, invalid ranged Attack 16x16, Look 14x15, invalid Look
  16x16, invalid spell target 16x17, and processing hourglass 13x15. The six
  controlled captures visibly reproduce the first four and the Look pair. The
  valid melee target resolves to Tyr OJFF #9258 -> BMP #346 at the documented
  `(0,0)` upper-left hotspot. The manual independently identifies the ranged,
  invalid-spell, and hourglass roles.
- **Confidence:** verified for owned resource identity, palette, geometry, the
  six live Walk/melee/Look states, the upper-left hotspot, and the first melee
  target; high for the manual-correlated remaining four roles.
- **Implementation:** pack format 21 extracts all ten images as distinct DSIX
  assets. MonoGame hides the host pointer and draws the selected original image
  last through the logical-canvas transform. Walk validity uses a non-mutating
  deterministic reachability query; a reusable reverse-draw-order alpha hit
  tester identifies displayed Tyr entities. Look is enabled over such entities
  and the visible leader, while melee eligibility is intentionally limited to
  the first observed OJFF #9258 until object behavior data is bounded.
- **Tests:** exact sequential IDs, names, dimensions, single-frame contracts,
  extraction provenance and inventory, all six mode/eligibility outcomes,
  overlay fallback, transparent/mirrored/topmost entity hit-testing, malformed
  catalog rejection, and owned content smoke.
- **Uncertainty:** generalized attackable/lookable object classification,
  ranged-mode selection from readied weapons, spell targeting, processing-state
  timing, clipping at canvas edges, and native cursor update cadence.

### DATA-GOG-INTERACTION-001 - First hostile Look panel

- **Method:** Capture the result of a valid Look action over the first hostile
  Tyr character, measure its 320x200 logical-canvas bounds, then correlate the
  panel against `RESOURCE.GFF` `WIND`/`BUTN`/`APFM` records and decoded `ICON`
  frames under `PAL ` #1000. Cross-check action semantics and the documented
  one-action shortcut against MANUAL-1994 pages 5-6.
- **Finding:** `WIND` #3020 is a 92x77 interaction panel observed at `(68,45)`.
  It contains disabled Talk `BUTN` #15306 / `ICON` #15105 at `(3,59)`, disabled
  Pick Up #15308 / #15107 at `(23,59)`, disabled Use #15307 / #15106 at
  `(43,59)`, dismiss #15309 / #15109 at `(61,60)`, and clipped application
  frame #15200 at `(0,0)`. The observed hostile Draxan panel labels level 10 and
  leaves Talk disabled. The manual says Look exposes Talk, Pick Up, and Use and
  performs an available sole action directly instead of showing the choice box.
- **Confidence:** verified for the owned panel identity, origin, control graph,
  disabled hostile state, visible name/level, and action semantics; unknown for
  how arbitrary OJFF/GPL records determine hostility and capabilities.
- **Implementation:** pack format 26 extracts the seven active/disabled/dismiss
  interaction images plus `WIND` #3020 as part of a bounded DSUI graph. Core models
  validated creature/object targets, enabled actions, the documented sole-object
  shortcut, inert disabled selections, and deterministic dismissal. Runtime
  presentation and target-to-capability derivation remain pending.
- **Tests:** target validation, hostile disabled Talk, all three sole-action
  shortcuts, multi-action selection, dismissal, exact graph/resource/image
  mapping, stored order, measured coordinates, exclusive hit edges, malformed
  graph rejection, transactional extraction, and content smoke.

### OBS-GOG-DIALOGUE-001 - Award notice and first measured conversation

- **Method:** Retain owner-supplied native captures immediately before a
  conversation and while its first choices are visible. Measure the two layouts,
  correlate them with `WIND` #12500/#12501 and their controls, and disassemble
  all 350 aligned `GPL`/`MAS` chunks from the fingerprinted `GPLDATA.GFF` using
  the public MIT-licensed OpenDS GPL disassembler. Search decoded instruction
  strings for the captured opening sentence and response labels. Inspect
  the public MIT-licensed `dsoageofheroes/libgff` commit
  `839b11d0ac63492e28f70968cfc3d967828958f5`, `src/gpl/state.c`, as a
  secondary clean-room implementation reference for
  local/global-variable reset behavior, then compare that behavior with GPL
  #135's own counter-gated menu structure. Sweep all 350 decoded GPL/MAS chunks
  for reads and writes of global flag 357.
- **Finding:** the 10,000-experience award is a one-way upper notification with
  an empty portrait well and no lower response window. Conversation uses the
  upper 318x72 portrait/speech window and a distinct lower 318x58 response
  window at logical y=142. The retained capture establishes opaque black for
  the upper well and opaque neutral gray (`#7d7d7d`) for the lower panel, even
  though both `WIND` records omit an image resource. The captured exchange and
  its initial response menu
  occur in `GPL` #135; the script's first response says “sniveling,” confirming
  the native pixels and correcting the earlier informal transcription.
  `WIND` #12500 owns the speech edit box and vertical controls; #12501 owns five
  response-row buttons and its own vertical controls. The script's preceding
  `showpic 18` instruction identifies `PORT` #18; all 178 owned PORT chunks
  decode through the bounded image reader, and #18 is one 72x72 frame. The five
  captured response rows correlate to GPL #135 choices 0, 1, 2, 3, and 7. MAS
  #99 assigns the variable-backed final label to global string slot 5. Choice
  source indexes 2 and 3 each print one literal, clear their matching local
  flag, increment local number 0, and return. Choice 4 becomes visible exactly
  when that number equals 2. The secondary libgff implementation clears local
  16-bit numbers to zero; together these establish an opening-only initial
  value of zero for local number 0, but not a generic native initialization
  rule.
  The corpus sweep finds global flag 357 only in GPL #135: two greeting/menu
  setup reads and choice 1's conditional read plus assignment. Choice 1 prints
  three literals, clears local flag 1, conditionally sets local flags 6 and 7
  when global flag 357 is zero, always sets that global flag to one, and returns.
  The secondary implementation clears global flags to zero at state startup;
  this supports a fresh-opening-only false value for global flag 357, not a
  generic saved-game assumption.
  Choice 4 prints three literals, sets local flag 9, resets local number 0, and
  returns. The common post-menu block reads local flags 1/2/3/9, derives flags
  4/5, and either repeats the
  opening menu or initializes flags 1/6/7/8 before entering the seven-entry
  second menu. That menu's source choice 6 shares the completion target at 2905
  with opening source choice 7.
  Its target 1825 prints three literals, clears local flag 6, conditionally sets
  local flag 10 when local flag 16 is zero, and returns.
  Target 3479 then prints three literals, sets local flag 16, clears local flag
  10, and returns.
  Global number 22 equal to one selects the opening-menu subroutine at script
  entry, so the captured opening establishes that value for this path. Target
  1996's matching branch prints two literals, sets local flag 11, skips its
  alternate body, clears local flag 7, and returns.
  When global number 22 differs from one, target 1996 instead tests global
  number 84 bit 2, selects one of two lead-ins, prints two common parts, applies
  `GNUM84 |= 1`, clears local flag 7, and returns.
  Target 2352 prints one literal, clears local flag 11, and returns.
  Target 2415 prints three literals, sets local flags 12 and 13, conditionally
  sets local flag 10 when local flag 16 is zero, and flows through a
  local-flag-8 while header into the seven-entry menu at offset 2616. That menu
  is gated by local flags 12/13/15/10/17/18 and has a constant final choice
  whose label is MAS #99 global string #6.
  Its source-choice-0 target at 2921 prints three literals, clears local flag
  12, and returns.
  Target 3089 prints three literals, sets local flag 15, clears local flag 13,
  and returns. Target 3257 prints four literals, clears local flag 15, and returns.
  Target 3686 prints two literals, clears local flag 17, sets local flag 18, and
  returns. Target 3786 prints three literals, clears local flag 18, and returns.
  Constant target 3976 clears local flag 8, calls a helper that sets local flag
  14 when local flags 1/6/7/11/10/8 are all zero after that clear, selects a
  two-part completed or one-part early output, and returns.
- **Confidence:** verified for the captured layouts, first exchange's GPL chunk,
  and visible response text; high for the static UI graph; medium for the
  opening-only local number 0, local flags 9/16, global flag 357, and global
  number 22 initial values
  because they
  combine corpus/script structure with a secondary clean-room implementation.
  The award-producing script path, broader dialogue consequences,
  generic variable initialization, and generic GPL
  execution semantics remain open.
- **Implementation:** pack format 26 stores the byte-identical GPL #135 and MAS
  #99 payloads in bounded versioned DSGP envelopes, extracts `PORT` #18 with
  the interface palette, and expands the interaction DSUI with #12500/#12501
  plus six dialogue-control images. The response adapter validates the speech
  edit box, exact five row resources, observed opaque panel fills, measured
  origin `(1,142)`, overlapping hit priority, and exclusive edges. A bounded decoder covers GPL's 7-bit
  compressed string primitive without embedding original dialogue text in Git.
  A fail-closed first-conversation reader validates the independently observed
  `showpic` at offset 16, two conditional print sources at 118/199, and the
  eight-entry initial menu at 253. It projects literal/variable labels, branch
  offsets, and the exact constant/local-flag/local-number-equality conditions.
  A Game adapter maps those resource expressions into Core's deterministic
  true/false/unknown evaluator. A bounded selector preserves source order,
  includes only proven-true conditions, and stops after the five physical rows;
  false and unknown conditions are both hidden without conflating their states.
  The preview passes the capture-correlated opening state, retains each selected
  choice's original index and branch offset, and selects indices 0, 1, 2, 3,
  and 7. Fail-closed projections validate MAS #99's byte-20 and byte-66
  assignments to type-6 global strings #5 and #6 and supply those labels only
  from the ignored owned pack. The capture-correlated opening state now includes local number 0 as zero,
  local flags 9/16 as false, global flag 357 as false, and global number 22 as
  one; generic initialization
  remains unknown. Core dialogue state stores
  all choice definitions and the script identity, derives stable visible
  source-index/branch-offset pairs, accepts one bounded physical-row selection,
  and preserves the selected pair atomically. Choice 0 has an exact bounded
  projection at offsets 1017-1147: three literal prints with two newlines,
  immediate zero to local flag 0, and a local return. Core applies that effect
  only to the matching branch. The bounded offsets 547-740 continuation derives
  flags 4/5 and advances to the seven-choice menu at offset 750; Game rebuilds
  its filtered response rows and replaces the speech with the projected output,
  preserving the two explicit newlines.
  Source choices 2 and 3 have exact bounded projections at offsets
  1148-1182 and 1183-1231: each prints one literal, clears its matching local
  flag, increments local number 0, and returns. Applying both in either menu
  order advances the known counter to 2, sets local flag 5, and reveals source
  choice 4 while retaining the opening menu.
  Source choice 1 has an exact bounded projection at offsets 1597-1824: three
  literal prints, local flag 1 clear, global flag 357 equals-zero conditional,
  conditional local flag 6/7 sets, global flag 357 set, and local return. Core
  resolves the known global condition before mutation, applies all effects
  atomically, rejects an unknown global input without mutation, and advances
  to the second menu. Choice 4 has an exact bounded projection at offsets
  1232-1394: three literal prints, local flag 9 set, local number 0 reset, and
  local return; it then advances through the common continuation. The second
  menu's conditions and targets are projected fail-closed. Runtime branch
  dispatch follows branch targets so reused source indexes cannot collide;
  target 1597 returns to its calling page. All implemented returned branches
  replace the speech with bounded projected output, while unprojected visible
  targets remain inert without selecting the session.
  Target 1825 has an exact bounded projection through offset 1995: three
  literal prints, local flag 6 clear, local flag 16 equals-zero condition, local
  flag 10 set, and local return. Core resolves the local condition before any
  mutation; the fresh-opening path returns to page two with source order
  2, 3, 5, 6 and presents the projected transcript.
  Target 3479 has an exact bounded projection through offset 3685: three literal
  prints, local flag 16 set, local flag 10 clear, and local return. It presents
  the transcript and recomputes page two as source order 3, 5, 6.
  Target 1996's unified projection validates both global-number-22 paths. Core
  conditionally sets local flag 11 on the equals-one path; otherwise it selects
  output by global-number-84 bit 2 and applies `GNUM84 |= 1`. Both paths clear
  local flag 7 and return at 2351. Unknown active-path inputs reject before
  mutation, while inactive alternatives require no irrelevant state.
  Target 2352 has an exact bounded print, local-flag-11 clear, and return
  projection through offset 2414; runtime presents it and recomputes page two
  as source order 5, 6.
  Target 2415 has an exact bounded projection through the third-menu entry at
  2616: three literal prints, local flags 12/13 set, a local-flag-16 conditional
  local-flag-10 set, and the local-flag-8 while header. Core exposes this as a
  legal second-to-third-page transition and filters the projected seven-entry
  menu. The owned path already has flag 16 set, so it presents third-page source
  order 0, 1, 6 and resolves the final label from GSTRING #6.
  Target 2921 has an exact three-print, local-flag-12-clear, and local-return
  projection through offset 3088. Runtime presents it and recomputes the third
  page as source order 1, 6.
  Target 3089 has an exact three-print, local-flag-15-set/local-flag-13-clear,
  and return projection through offset 3256; it exposes source order 2, 6.
  Target 3257 has an exact four-print, local-flag-15-clear, and return projection
  through offset 3478; it leaves source choice 6. Runtime presents both outputs.
  Target 3686 has an exact two-print, flag-17-clear/flag-18-set, and return
  projection through offset 3785. Target 3786 has an exact three-print,
  flag-18-clear, and return projection through offset 3975. Both are registered
  for target-based runtime dispatch and their output fits the owned font area.
  Target 3976 validates its flag-8 clear, condition/output helper calls, exact
  six-flag expression, flag-14 conditional assignment, both output paths, and
  all return boundaries through offset 4123. Core evaluates the composite
  condition after direct assignments, rejects unknown inputs without mutation,
  and completes the dialogue; owned smoke proves the completed two-part path.
  Choice 7's target has an exact bounded projection at offsets 2905–2920: print
  GSTRING #5, assign immediate one to local flags 14 and 4, then return locally.
  Core validates the selected identity before applying both flags atomically and
  entering `Completed`; Game closes the preview from either menu. Other GPL
  instruction paths, third-menu consequence mutations, and generalized text routing remain
  next Slice 3 work.
- **Tests:** DSGP identity/payload round-trip and malformed envelopes; packed
  string empty/text/control-byte decoding and malformed inputs; exact dialogue
  window/control/image geometry, response order/hit boundaries, synthetic
  transactional extraction, synthetic projection/drift/malformed cases,
  condition mapping, known/unknown evaluation, ordered/bounded fail-closed
  selection, retained branch identities and invalid selection immutability,
  response/completion opcode/operand/offset/truncation drift, choice 4's
  assignment path, opening continuation and second-menu projection, caller-page
  return, target-1825 local-condition/assignment drift, target-3479 flag/return
  drift, target-1996 condition/control-flow/return drift, global-number
  equality/bitmask path selection and atomic rejection, target-2352 assignment/return
  drift, target-2415 assignment/conditional/loop/menu-transition drift,
  target-2921 print/assignment/return drift,
  target-3089/3257 print/assignment/return drift,
  target-3686/3786 print/assignment/return drift,
  target-3976 branch/helper/output drift and post-assignment atomicity,
  explicit-newline transcript rendering and malformed-output rejection,
  atomic flag
  application, conditional global-flag effects, deterministic menu reselection,
  unknown local-number/global-condition rejection, mismatched-branch rejection,
  bounded MAS #5/#6 assignment/drift cases, variable-label resolution, and
  owned content smoke proving portrait, speech-source, three-menu transitions,
  five-row bounds, and ordered condition contracts.

## Initial rules

### RULE-INPUT-001 - Mouse-first interaction

- **Behavior:** The original requires a mouse and uses cursor modes for walking,
  looking, attacking, and targeted actions. Escape exits the active menu.
- **Preconditions:** Relevant exploration or menu state is active.
- **Evidence:** MANUAL-1994, introduction and "How to Play".
- **Confidence:** verified for the owned Walk/melee/Look cursor visuals and
  hotspot; high for intended behavior. Broader target eligibility remains open.
- **Implementation:** the runtime resolves WIND #19500 child coordinates and
  BUTN dimensions/image references from DSUI, maps original button resource IDs
  to semantic choices, and routes clicks through a single letterboxed
  logical-canvas transform into Core commands. In exploration, right-click
  deterministically cycles Walk, Attack, and Look modes. `DATA-GOG-CURSOR-001`
  maps, extracts, and renders all ten cursor resources at the manual-defined
  upper-left hotspot, with live valid/invalid feedback for the implemented
  target subset. A reusable ordered hotkey table maps V/I/C/U/E/O/Tab and Escape
  to Core navigation commands.
- **Tests:** graph completeness and unexpected identities, catalog order,
  image-reference matching, rectangle edges, wide/tall letterboxing, inverse
  coordinates, resulting Core routing, and owned-pack content smoke.
- **Uncertainty:** generalized mode-specific target eligibility and shipped
  coordinate boundaries beyond the upper-left hotspot remain open.

### RULE-EXPLORATION-001 - Camera and party-display controls

- **Behavior:** Moving the pointer to a screen edge scrolls continuously in that
  direction until the pointer leaves the edge or the map boundary is reached.
  The default exploration display shows only the leader; hotkeys 5 and 6 select
  the expanded-party and leader-only displays respectively.
- **Preconditions:** Exploration is active on a bounded region map.
- **Evidence:** MANUAL-1994, "How to Play" pages 4-5 and the hotkey table.
- **Confidence:** high for intended direction, stopping conditions, default,
  and hotkey meanings; exact scroll rate and shipped edge thickness are unknown.
- **Implementation:** `ExplorationSession` owns deterministic camera, cursor,
  party-display, and active-view state. One-pixel scroll commands clamp to the
  region bounds; the MonoGame input adapter emits them from the outermost
  logical-canvas row or column and rerasterizes the Tyr viewport. Keys 5 and 6
  update the Core display mode. Documented hotkeys open character, inventory,
  cast/psionic, current-effects, overhead-map, and game-menu views. Escape
  returns from any such view and requests exit only from the world; world input
  is suspended while a view is open. The original Game Menu shell and controls
  now render and route evidenced actions; expanded-party sprites and destination
  shells remain pending.
- **Tests:** initial state, all four edges and diagonal corners, interior/outside
  coordinates, map clamping, mode-cycle order, display idempotence, and invalid
  scroll deltas; every documented view and alias, ordered rising-edge hotkeys,
  menu escape/exit behavior, suspended world input, and explicit mode selection.
- **Uncertainty:** Scroll timing, generalized cursor eligibility, party sprite composition,
  Game Menu frame states/origin, destination-menu presentation, and exact native
  center-on-leader pixel policy remain open; deterministic visual-center
  targeting is the current implementation policy.

### COMPAT-INPUT-001 - Modern mouse and fullscreen controls

- **Decision:** Preserve the original pointer-at-edge camera scrolling and
  right-click Walk/Attack/Look cycle, while adding right-button grab-drag
  panning and Alt+Enter fullscreen toggling as documented modern control
  improvements.
- **Authority:** Repository owner, 2026-09-14.
- **Implementation:** a stateful input adapter defers the mode-cycle command
  until a stationary right-button gesture is released. Logical pointer motion
  while held emits bounded incremental Core pan commands at thirteen world
  pixels per ten logical pointer pixels in grab-the-world direction and suppresses the
  click action. Leaving the letterboxed canvas
  clears the motion anchor so re-entry cannot jump the camera; leaving the
  world view cancels the gesture. Edge scrolling remains available whenever a
  right drag is not active. Either Alt key combined with Enter toggles
  MonoGame fullscreen once when the chord becomes active, on every screen.
  Fullscreen adopts the current display mode's dimensions and restores the
  960x600 windowed backbuffer on exit.
- **Tests:** stationary click/release, successive drag deltas, click suppression
  after dragging, canvas exit/re-entry anchoring, inactive-view cancellation,
  bounded Core pan validation, camera clamping, and suspended world input.
  Fullscreen tests cover either Alt key, either chord-completion order, held
  chord suppression, and partial chords.
- **Parity boundary:** this gesture is intentionally not attributed to the
  original. Mouse acceleration and operating-system pointer capture are not
  part of deterministic Core state.

### COMPAT-DISPLAY-001 - Expanded world and fixed interface canvases

- **Decision:** Travel and combat may expose additional map area to fill the
  physical display. Fixed-layout screens and overlays retain the original
  320x200 coordinate system and may letterbox.
- **Authority:** Repository owner, 2026-09-14.
- **Implementation:** the world viewport derives a bounded logical width and
  height from the physical aspect ratio, preserves the observed camera center,
  clamps at region boundaries, and fills the whole backbuffer. Menus and other
  fixed screens still use the centered 320x200 transform. A temporary F9
  validation hook switches its world backdrop to that centered fixed canvas, so
  the map, dialogue chrome, cursor, and response hit areas share one scale;
  closing the preview restores the expanded Tyr slice. The hook draws the
  measured dialogue windows, portrait #18, scrollbar controls, five response
  strips, and the projected first literal speech plus proven-visible choices in
  the extracted bitmap font. Greedy wrapping is provisional. The capture-correlated
  opening flags are explicit; all other variables remain unknown and fail closed.
  Clicking a response stores its source index and branch target in Core. The
  hook claims only choice 0's bounded flag-clear/menu-return projection and
  choice 7's bounded completion projection, not generalized GPL branch execution
  or quest consequences.
- **Tests:** wide, tall, edge-clamped, and fixed viewport layout/inverse mapping;
  exact dialogue window, portrait, control-image, and response-row placement;
  bounded wrapping/selection, returned-menu rebuilding, and malformed text; F9
  rising-edge behavior; owned content-smoke fit and graph validation.
- **Parity boundary:** the extra visible map and preview key are modern
  conveniences. Original 320x200 UI geometry remains unchanged.

### COMPAT-PATH-001 - Modern deterministic pathfinding

- **Decision:** Pathfinding does not need to reproduce the original route
  planner verbatim. A modern implementation may replace it as long as reachable
  targets are served correctly and movement retains the game's evidenced world
  and collision constraints.
- **Authority:** Repository owner, 2026-09-13.
- **Implementation:** `GridPathfinder` provides bounded deterministic
  eight-direction A* with octile costs, stable tie-breaking, explicit
  unreachable results, and diagonal corner-cut prevention. It accepts a
  caller-supplied passability predicate. `EXE-GOG-REGION-001` establishes
  `GMAP` bit `0x40` as terrain/occupancy blocking, so `RegionTerrainGrid`
  supplies the bounded static terrain predicate and
  `ExplorationTerrainRoutePlanner` maps active Walk-mode canvas clicks through
  the deterministic camera to reusable plan-route commands. The clock-free
  `ExplorationMovementSession` atomically plans/replans, advances exactly one
  semantic cell per command, completes or cancels routes, and interrupts before
  entering a newly blocked step or diagonal corner. Its optional step-commit
  boundary lets `ExplorationActorMovementSession` keep route and occupancy
  anchors synchronized or interrupt without partial advancement.
  `ExplorationActorController` composes the evidenced opening anchor, logical
  click routing, shared live occupancy, and bounded fixed-step advancement; the
  runtime placement currently supplies a provisional single-cell footprint.
  Its 125 ms semantic step and four-step catch-up cap are explicit modern
  runtime policies, not original-parity claims.
- **Tests:** optimal open-grid route, stable obstacle detour, blocked endpoint,
  zero-length route, diagonal corner, endpoint bounds, maximum-grid limits,
  exact flag/bounds/pixel-cell semantics, stable camera-to-destination routing,
  inactive-mode rejection, deterministic command/event order, atomic rejected
  replanning, per-step blocker revalidation, cancellation, and snapshot
  isolation.
- **Uncertainty:** Actor-specific low-bit policy outside Tyr, moving blockers,
  native actor footprint, movement cadence, destination tolerance, and
  sprite-frame animation remain open; none are inferred by the terrain grid,
  planner, or route session. Fixed-point positional interpolation is an
  explicit presentation policy.

### RULE-EXPLORATION-002 - Atomic dynamic occupancy

- **Behavior:** Dynamic actors occupy one or more region cells. Placement and
  removal mutate each cell in an actor's footprint as one logical operation;
  another actor cannot overlap those cells or blocked terrain.
- **Evidence:** `EXE-GOG-REGION-001` establishes the `0x40`/`0x20` dynamic pair
  and a coordinator called once per iterated footprint cell. The same bounded
  analysis establishes the opening anchor relationship and a separate sentinel
  path for actor records whose leading signed value has magnitude 430. Its
  guarded dynamic placement path occupies a 5x5 cell area without its four
  corners (21 cells), while its paired path clears that same area. The sentinel
  bypasses ordinary coordinate enumeration, so the normal loop cannot prove a
  universal footprint shape. Its concrete actor category, source-data mapping,
  and mutation cadence are not established.
- **Confidence:** high for per-cell occupied/open exclusion and the guarded
  sentinel pattern in the supported executable; implementation-policy for
  atomic rejection and stable ordering; unknown for the opening actor and other
  concrete actor shapes.
- **Implementation:** `GridFootprint` is immutable, duplicate-free, and
  canonically ordered. `ExplorationOccupancySession` applies place/move/remove
  commands atomically over bounded terrain, emits deterministic events and
  explicit rejection reasons, snapshots placements in occupant-ID order, and
  exposes a live whole-footprint anchor predicate to pathfinding.
  `ExplorationActorMovementSession` composes that predicate and the route
  session, commits each accepted route step to occupancy, and detects external
  anchor drift. Runtime assigns the opening leader a documented provisional
  single-cell footprint; no evidence-backed party or NPC shape is claimed.
- **Tests:** multi-cell placement, own-cell overlap during movement, terrain,
  bounds and other-occupant rejection without partial mutation, cell release,
  missing-occupant handling, snapshot isolation/order, invalid payloads, live
  blocker updates, route planning through the composed predicate, synchronized
  actor advancement, commit rejection, interruption, and drift detection.
- **Uncertainty:** Native occupant IDs, the meaning and source-data mapping of
  the magnitude-430 sentinel, party formation, actor-specific footprints, NPC
  placement, and movement timing remain open. The opening anchor convention
  itself is now evidenced by `DATA-GOG-ACTOR-001` and `EXE-GOG-REGION-001`.

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
  `characters/catalog.dsch`; the asset-pack manifest is version 17 and records
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
  members visible. The manual documents Guard (`G`), target-next (`N`),
  target-previous (`P`), end-turn (`Q`), Wait (`W`), and Space to disable
  computer control. During combat, a Walk click makes the character approach
  and automatically attack the selected opponent. Melee requires adjacency and
  a readied weapon; ranged requires an in-range opponent plus a readied missile
  weapon or ammunition. A two-weapon melee configuration requires one-handed
  weapons in both hands. The documented invalid cursor means the attempted
  target is not eligible for that attack.
- **Evidence:** MANUAL-1994, "How to Play" pages 4-6 and the visually reviewed
  hotkey table on manual page 77. `EXE-GOG-COMBAT-001` excludes only two
  direct-literal dispatcher forms; it does not establish the shipped command
  implementation.
- **Confidence:** high for documented command bindings and intended attack
  eligibility; resolution order, exact computer-control semantics, and formulas
  are unknown.
- **Implementation:** `CombatCommand` owns the six semantic requests without
  depending on MonoGame, while `CombatHotkeys` maps only their documented keys
  on a rising edge. `CombatAttackEligibilityRules` evaluates only the supplied
  manual preconditions for melee, ranged, and two-one-handed-weapon readiness;
  it does not define range, adjacency, equipment readiness, pathing, target
  selection, damage, or attack resolution. The mapper is intentionally not
  connected to exploration: no combat state exists yet to resolve these
  requests into target selection, turn advancement, guard/wait effects, or
  automation changes.
- **Tests:** each documented key, rising-edge suppression, stable simultaneous
  ordering, invalid command rejection, and the full true/false matrix for
  melee, ranged, and two-weapon-readiness predicates are covered.
  Deterministic combat command traces remain planned.
- **Uncertainty:** Activation order, RNG, THAC0/AC details, timing, and difficulty
  effects, the computer-control default and re-enable behavior, turn effects of
  Guard/Wait, the Dexterity threshold and non-ranger two-weapon penalty, and
  exact target/range calculations require OBS-GOG and targeted Ghidra evidence.

### RULE-COMBAT-002 - THAC0 hit threshold

- **Behavior:** Resolve an attempted attack from a supplied integer roll in the
  inclusive range 1 through 20. It hits exactly when the roll is greater than
  or equal to the attacker's THAC0 minus the target's Armor Class. Lower Armor
  Class is harder to hit. The manual examples establish 5 THAC0 versus 3 AC
  requires 2 or higher, and 5 THAC0 versus -2 AC requires 7 or higher.
- **Evidence:** MANUAL-1994 page 25, "Armor Class" and "THAC0". It explicitly
  defines the random roll range and inclusive threshold.
- **Confidence:** high for the base threshold and bounds. The manual names
  range, rear attacks, magic weapons, and magic spells as THAC0 modifiers but
  does not establish their numerical values or application order here.
- **Implementation:** `CombatAttackRules.Resolve` takes a caller-supplied roll
  and unmodified/effectively precomputed THAC0. It uses wide arithmetic before
  reporting the threshold, so malformed or future extreme values cannot wrap.
  It does not create randomness, apply modifiers, assign damage, add automatic
  1/20 outcomes, mutate a combatant, or advance a turn.
- **Tests:** both manual examples, equality/below-threshold boundaries,
  documented roll bounds, and overflow-safe extreme statistics.
- **Uncertainty:** RNG algorithm and consumption, modifier values/order, natural
  1/20 behavior, weapons, damage, resistance, incapacitation, and all combat
  state transitions remain open.

### RULE-COMBAT-003 - Hit-point incapacity thresholds

- **Behavior:** Damage subtracts from the target's hit points. Positive hit
  points are conscious; zero through -9 are unconscious; -10 or less is dead.
- **Evidence:** MANUAL-1994 page 25, "Hit Points". `EXE-GOG-COMBAT-002`
  records that the threshold literal is not a unique static-analysis lead and
  therefore contributes no original-state semantics.
- **Confidence:** high for the stated thresholds and subtractive damage model.
  The manual section does not establish stabilization, recovery, healing,
  death saves, event ordering, or mechanical effects of either status.
- **Implementation:** `CombatHitPointRules` classifies a supplied hit-point
  total and subtracts nonnegative supplied damage using wide arithmetic with a
  lower saturation boundary. It does not calculate damage, revive a combatant,
  alter turn order, or apply status effects.
- **Tests:** conscious, zero, -9, -10, lower-bound, subtraction, saturation,
  and negative-damage rejection boundaries.
- **Uncertainty:** weapon and spell damage, healing/revival, incapacitation and
  death consequences, target removal, experience, and combat-state transitions
  remain open.

## Conflict handling

FAQ-81038 reports discrepancies between documentation and shipped behavior.
Each conflict receives its own rule ID, both claims, reproduction procedure, and
owner decision. No compatibility behavior is selected from plausibility alone.

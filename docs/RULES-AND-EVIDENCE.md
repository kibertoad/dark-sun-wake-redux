# Rules and evidence

## Sources

The spec's source entries in [`spec/sources/`](../spec/sources/) describe each
outside source, what it is used for and its known errors, and the build entry
[`BLD-GOG-EN-1.1`](../spec/builds/BLD-GOG-EN-1.1.md) describes the owned
release. Records below that have not moved into the spec yet keep their legacy
IDs: `OBS-GOG-*` for controlled runs and `DATA-GOG-*` for bounded inspection of
the owned build.

### DATA-GOG-MEDIA-001 - Cinematic header inventory

The voice-file and music parts of this record moved to FND-SOUND-001 and
FND-SOUND-002.

- **Question:** What fixed media-file envelopes can be established for the
  supported installation before selecting a cinematic decoder?
- **Method:** Enumerate only paths, extensions, and sizes beneath the owned
  installation. For each numbered FLI file, read exactly the first 128 bytes
  and summarize little-endian header fields without decoding chunks or frames.
- **Finding:** the installation contains five root-level FLI files totalling
  19,498,251 bytes. Every FLI file is at least 128 bytes
  and has a little-endian file-size field equal to its physical length, magic
  `0xAF11`, 320x200 dimensions, 8-bit depth, and flags `0x0003`. In numeric
  filename order, their frame-count/raw-speed pairs are 1175/7, 373/7, 575/7,
  284/7, and 1398/5. Starting at byte 128, a bounded six-byte
  declared-size/type scan completely covers every physical FLI file using only
  `0xF1FA` records. Their physical-record counts are 1176, 374, 576, 285, and
  1399 respectively: exactly one more than their corresponding header counts.
  For FLI 1 through 4, treating each record as a 16-byte fixed header followed
  by its declared number of six-byte size/type chunks fully covers all records,
  allowing two total unclassified trailing bytes in each of FLI 1 and 2. Their
  raw chunk-type/count inventories are `0x000B`/`0x000C`/`0x000F`/`0x0010` =
  17/578/1/1, 3/266/1/1, 4/429/1/0, and 1/270/1/0. FLI 5 rejects that nested
  model at its first record: after one of its two declared chunks, the next
  stated chunk length is 4,486 bytes with only 4,485 bytes remaining. Extending
  that record by one byte makes its first two chunks fit, but then the next
  alleged record header has type `0x01f1` and declares 4,194,304,014 bytes with
  only 5,749,959 bytes physically remaining. The mismatch is therefore not
  accepted as a simple physical-padding variation.
- **Confidence:** high for these bounded inventory/header facts in
  BLD-GOG-EN-1.1; unknown for FLI chunk types, palette behavior, raw-speed
  units, effective playback cadence, whether the one extra FLI record is a
  loop/sentinel/displayed frame, raw chunk-type meanings, FLI 5's nested
  variant, and all audiovisual sequencing.
  `EXE-GOG-MEDIA-002` finds raw executable occurrences of every numbered FLI
  name and four CINE-directory templates, but no direct Ghidra reference to
  any inspected entry; it does not establish a loader, fallback, or sequence.
- **Implementation consequence:** no decoder, extractor entry, media mapping,
  or time-based runtime behavior is introduced yet. A future media reader must
  validate these fixed envelopes first, bound every subsequent record/chunk
  and decoded output, preserve the header-versus-physical-record count
  distinction, support each nested FLI variant only after independent
  validation, use an explicit monotonic playback clock, and treat the raw FLI
  speed field as data until its unit is independently established.

## Initial rules

### RULE-EXPLORATION-001 - Camera and party-display controls

- **Behavior:** Moving the pointer to a screen edge scrolls continuously in that
  direction until the pointer leaves the edge or the map boundary is reached.
  The default exploration display shows only the leader; hotkeys 5 and 6 select
  the expanded-party and leader-only displays respectively.
- **Preconditions:** Exploration is active on a bounded region map.
- **Evidence:** SRC-MANUAL-1994, "How to Play" pages 4-5 and the hotkey table.
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
  runtime policies, not original-parity claims. `FND-TIME-001` rules out
  deriving a cadence from the bounded BIOS-tick paths, which are not an actor
  scheduler.
- **Tests:** optimal open-grid route, stable obstacle detour, blocked endpoint,
  zero-length route, diagonal corner, endpoint bounds, maximum-grid limits,
  exact flag/bounds/pixel-cell semantics, stable camera-to-destination routing,
  inactive-mode rejection, deterministic command/event order, atomic rejected
  replanning, per-step blocker revalidation, cancellation, snapshot isolation,
  and convergence of capped long-frame catch-up to the same semantic state as
  partitioned elapsed-time updates.
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
  itself is now evidenced by `FND-ACTOR-002` and `EXE-GOG-REGION-001`.

### DATA-GOG-SMALLTAG-001 - Opaque PLYL and CSEQ envelope inventory

- **Question:** Do the short `PLYL` or `CSEQ` resources yield a structural
  format or feature assignment that can support game behavior?
- **Method:** Read the verified local-pack manifest's source mapping and DSOP
  lengths. Each DSOP has the fixed ten-byte envelope header; subtracting that
  header reports the original payload lengths without retaining source bytes.
- **Finding:** `RESOURCE.GFF` contributes six `PLYL` resources (#0, #10,
  #50-#53) whose payload lengths are 3, 5, or 7 bytes, and one 78-byte `CSEQ`
  #1000. These inventory facts establish no shared record layout, field
  boundary, consumer, or feature meaning. `EXE-GOG-SMALLTAG-001` finds no
  literal executable tag path for either family. The `GREQ` and `CACT`
  resources of `CHARSAVE.GFF` are `FMT-SAVE-002` and `FMT-SAVE-001`.
- **Confidence:** high for supported-edition identities, counts, sources, and
  payload lengths; unknown for every field, loader, ownership, and
  player-visible behavior.
- **Implementation:** retain both families as DSOP and do not add format
  readers or assign sequence, interaction, or combat roles.
- **Tests:** exact owned-pack inventory/hash verification and generic DSOP
  envelope bounds/read-back; no semantic fixture is introduced.

### DATA-GOG-CSEQ-001 - CSEQ stride does not establish the runtime selector source

- **Question:** Does `RESOURCE.GFF` `CSEQ` #1000's exact 78-byte envelope
  establish the source or record layout of the independently observed 13-byte
  runtime selector table?
- **Method:** The metadata-only `record-profile` inspector verifies that 78
  divides into six candidate 13-byte records and reports aggregate byte-column
  statistics only. The `lane-word-namespace-profile` inspector then compares
  each complete aligned little-endian word at offsets 0, 2, 4, 6, 8, and 10 in
  those candidate records with all 330 `GPL ` resource numbers in the owned
  `GPLDATA.GFF`, reporting counts only.
- **Finding:** `CSEQ` #1000 is exactly six 13-byte units. At the runtime
  selector's leading-word position, only one of six aligned values belongs to
  the GPL resource-number set; the other tested word positions have 1, 1, 0,
  1, 0, and 2 GPL-set members respectively. The aggregate byte profile also
  finds five distinct values at offset 11 (including two zeroes) and four at
  offset 12 (including three zeroes). These facts do not reproduce a uniform
  leading GPL identity, a 200-record source set, or a proven offset-11 link
  contract.
- **Interpretation:** matching the 13-byte arithmetic stride is a structural
  coincidence or, at most, a candidate transformation lead. It does not show
  that `CSEQ` populates the runtime table, that its six units are selector
  records, or that either data family has a dialogue, quest, map, combat, or
  script-execution role. Indirection, expansion, transformation, and another
  source remain possible.
- **Confidence:** high for the owned resource identity/length, six-unit
  arithmetic, aggregate word-membership counts, and aggregate offset-11/12
  column counts; unknown for every field and runtime relationship.
- **Implementation consequence:** retain `CSEQ` as DSOP. Do not add a CSEQ
  reader, connect it to the 13-byte selector table, or use it to select a GPL
  resource without a constrained native population path or controlled runtime
  observation.

## Conflict handling

SRC-GAMEFAQS-81038 reports discrepancies between documentation and shipped behavior.
Each conflict receives its own rule ID, both claims, reproduction procedure, and
owner decision. No compatibility behavior is selected from plausibility alone.

# Rules and evidence

## Sources

The spec's source entries in [`spec/sources/`](../spec/sources/) describe each
outside source, what it is used for and its known errors, and the build entry
[`BLD-GOG-EN-1.1`](../spec/builds/BLD-GOG-EN-1.1.md) describes the owned
release. Records below that have not moved into the spec yet keep their legacy
IDs: `OBS-GOG-*` for controlled runs and `DATA-GOG-*` for bounded inspection of
the owned build.

### DATA-GOG-GPLI-001 - GPLI fixed-lane envelope and non-unique GPL-ID candidate

- **Question:** Does the owned `GPLDATA.GFF` `GPLI` #1 payload itself establish
  a unique, fixed-lane association with the known first-Tyr `GPL` resource
  #135?
- **Method:** Resolve `GPLI` #1 through the verified secondary GFFI descriptor
  table, require its known 7,896-byte envelope, and partition it into 329
  records of four six-byte lanes. First examine only the 3,948 aligned
  little-endian 16-bit lane words for the numeric value 135. A follow-up
  read-only `lane-word-namespace-profile` Inspect command compares each of the
  twelve aligned word positions to the archive's 330 `GPL ` resource numbers
  and reports only counts, distinct counts, set membership, and duplication
  statistics; it retains no raw bytes or text.
- **Finding:** the payload exactly fits the four-lane shape. Value 135 occurs
  twice: in zero-based record 63, lane 1, word 2 and in zero-based record 133,
  lane 3, word 0. It therefore has no unique occurrence and no consistent lane
  position under that narrow candidate interpretation. The aggregate check
  finds a stronger but still non-semantic structure: lane word 2 (the third
  16-bit word) has 1,315 GPL-number-set members across its 1,316 occurrences.
  The one non-member is in the first lane position; the other three third-word
  positions each have complete 329-of-329 GPL-set membership. Together the
  third-word positions cover all 330 GPL resource numbers, but only 331
  distinct values across 1,316 occurrences, with individual values occurring
  from once through 27 times and 162 records repeating a value across lanes.
  The eight non-third positions also have 846 GPL-set hits across 2,632 words,
  so namespace membership alone does not assign their roles.
- **Confidence:** high for the verified secondary-table resolution, fixed
  envelope, lane arithmetic, two numeric occurrences, and aggregate
  GPL-number-set membership counts; unknown for every lane's field role,
  byte order beyond this checked word interpretation, runtime lookup, and
  relationship to GPL/MAS, dialogue, encounters, or quests.
- **Implementation consequence:** retain `GPLI` #1 as lossless DSOP. The
  repeated third-word correlation is a candidate GPL-reference column, not a
  field contract: do not add a GPLI reader, record-to-script map, or dialogue
  behavior from it. A constrained native call path or controlled observation
  remains required.

### DATA-GOG-MEDIA-001 - Cinematic and voice-file header inventory

- **Question:** What fixed media-file envelopes can be established for the
  supported installation before selecting a cinematic or audio decoder?
- **Method:** Enumerate only paths, extensions, and sizes beneath the owned
  installation. For each numbered FLI file, read exactly the first 128 bytes
  and summarize little-endian header fields without decoding chunks or frames.
  For every VOC file, read exactly its fixed 26-byte header and the single byte
  at its declared data offset; aggregate the results without retaining names,
  frames, samples, or block payloads.
- **Finding:** the installation contains five root-level FLI files totalling
  19,498,251 bytes, 147 VOC files totalling 33,989,656 bytes, and 40 Ogg files
  under `MUSIC` totalling 99,131,938 bytes. Every FLI file is at least 128 bytes
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
  accepted as a simple physical-padding variation. All 147 VOC files are at
  least 26 bytes, begin with the
  standard `Creative Voice File` signature, declare data offset 26, version
  `0x010a`, checksum `0x1129`, and contain first block type 1 at that offset.
  Each declared first-block length fits within its physical file; the raw
  time-constant/codec-byte groups are 165/0 for 30 files, 210/0 for 115, and
  131/0 for two, with declared block lengths ranging from 365 through 3,160,452
  bytes.
- **Confidence:** high for these bounded inventory/header facts in
  BLD-GOG-EN-1.1; unknown for FLI chunk types, palette behavior, raw-speed
  units, effective playback cadence, whether the one extra FLI record is a
  loop/sentinel/displayed frame, raw chunk-type meanings, FLI 5's nested
  variant, later VOC block layout, codec-byte/sample-rate semantics,
  audio routing, Ogg track mapping, loops, and all audiovisual sequencing.
  `EXE-GOG-MEDIA-002` finds raw executable occurrences of every numbered FLI
  name and four CINE-directory templates, but no direct Ghidra reference to
  any inspected entry; it does not establish a loader, fallback, or sequence.
  `EXE-GOG-SOUND-002` separately finds no complete VOC header signature,
  `EXE-GOG-SOUND-003` finds no `INT 15h` opcode, and `EXE-GOG-SOUND-004` finds
  no `.VOC` filename-extension literal in the loaded sound-helper image.
  `EXE-GOG-SOUND-006` extends the first result through the helper's complete
  physical MZ overlay: the same 19-byte VOC signature is absent everywhere in
  the 204,593-byte file. `EXE-GOG-SOUND-007` does find bounded direct port
  output, including a caller-word-derived fixed-port sequence and a separate
  runtime-base-plus-offset sequence, but neither has a recovered VOC, device,
  settings, or timing owner. `EXE-GOG-SOUND-008` independently scans the
  complete physical main `DSUN.EXE` and finds the same whole-header signature
  absent there as well; this is not evidence that any executable cannot read
  VOC data through a partial, indirect, or delegated path. The same main
  executable has five raw `.VOC` extension fragments, but
  `EXE-GOG-SOUND-009` finds all five unreferenced and non-instruction data;
  they identify no filename loader or playback path. `EXE-GOG-SOUND-010` also
  finds no complete literal `SOUND_DS.EXE` helper filename in the physical main
  executable, while `EXE-GOG-SOUND-011` finds no decoded `INT 21h` with a
  nearby literal DOS EXEC setup. Neither result excludes an indirect,
  constructed, or wrapped helper path.
  None of those bounded absences establishes a decoder, codec, filename mapping, or timing
  contract.
- **Implementation consequence:** no decoder, extractor entry, media mapping,
  or time-based runtime behavior is introduced yet. A future media reader must
  validate these fixed envelopes first, bound every subsequent record/chunk or
  block and decoded output, preserve the header-versus-physical-record count
  distinction, support each nested FLI variant only after independent
  validation, use an explicit monotonic playback clock, and treat the raw FLI
  speed field as data until its unit is independently established.

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
- **Finding:** the window layout of the award notice and of the conversation,
  measured from `dsun_007.png` and `dsun_008.png`, is SCR-UI-012 (FND-UI-016,
  FND-UI-017, FND-UI-032). The captured exchange and
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
- **Implementation:** the current required-revision-34 pack stores the byte-identical GPL #135
  and MAS #99 payloads in bounded DSGP v2 envelopes that retain the exact
  source tag as well as identity, extracts `PORT` #18 with
  the interface palette, and expands the interaction DSUI with #12500/#12501
  plus six dialogue-control images. The response adapter validates the speech
  edit box, exact five row resources, the transparent panel canvas with its
  visible 320x58 textured chrome strip at each measured placement, measured
  origin `(1,142)`, overlapping hit priority, and exclusive edges. A bounded
  decoder covers GPL's 7-bit
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
  runtime policies, not original-parity claims. `EXE-GOG-TIMING-001` rules out
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

### OBS-GOG-PARTY-001 - Owner-confirmed combat captures

- **Question:** What player-visible combat-feedback and turn-state facts can be
  observed without assigning combat mechanics?
- **Method:** The owner confirmed the semantic labels for local-only DOSBox
  Ctrl+F5 captures `dsun_010.png` through `dsun_024.png`; on 2026-09-21 the
  owner additionally labels `dsun_009.png` as an enemy-moving frame,
  `dsun_011.png` as enemy striking, and `dsun_012.png` as player turn. The
  durable owner authorization for the configured DOSBox capture folder permits
  inspection regardless of filename timestamp; semantic identity remains only
  as owner-confirmed. Inspect the 320x200 captures in place; do not copy,
  rename, or commit them. The party members these captures show, and their
  Use, inventory and View Character screens, are in FND-PARTY-020,
  FND-MAGIC-001 and FND-UI-019 to FND-UI-021.
- **Finding:** The owner reports that before combat the map
  and character presentation remain substantially unchanged except that the
  compact right-side status panel is absent; the first stable combat view shows
  the currently active character's panel. There is no separate visible
  target-switching or confirmation step: clicking an enemy makes the active
  character approach and strike it. The owner identifies `dsun_009` as an
  enemy-moving frame. It visibly retains the dialogue chrome and lacks the
  compact right-side status panel. `dsun_011` is enemy striking, and
  `dsun_012` is combat during the third party member's turn. It visibly shows
  the party cluster and a compact right-side panel with that member's name,
  `90/85`, and `Moves 15`; these are displayed strings, not assigned
  status/value semantics. `dsun_011` is confirmed as combat
  damage being inflicted and visibly shows a red `11` feedback glyph over the
  actor cluster. The
  pure-red components in that visible glyph area have a union bound of `(151,95)` through `(178,113)` in
  the 320x200 frame; that is feedback geometry, not a damage-value or timing
  contract. The static top-right panel in both captures template-matches
  RESOURCE.GFF BMP #19003 (98x32) at (215,4): 2,594 of its 3,098 opaque
  pixels match (83.73%), while the same remaining pixels are bounded to
  (243,8) through (284,31) in each capture. This establishes the static panel
  artwork, placement, and a dynamic overlay region only; it identifies no
  panel text source, value meaning, movement rule, or update timing. The
  dynamic panel text differs between the owner-confirmed enemy-striking and
  player-turn frames: `dsun_011` visibly draws an enemy's name and `Moves 20`,
  while `dsun_012` draws the party member's name, `90/85`, and `Moves 15`.
  Together with the owner's statement that the stable panel represents the
  currently active character, this establishes active-combatant-dependent
  presentation only; none of the strings is assigned a field, actor-record,
  movement, health, or turn-order meaning. The observed player-turn frame
  establishes only that this labelled combat turn state is visible; the owner
  reports no visible turn-transition treatment.
  This does not establish how turns start, advance, or cycle.
  These captures do not establish movement-point scale, initial amount, cost,
  distance, path, collision, speed, attacker, target, damage rule, action,
  command input, turn progression, timing, hit resolution, or the condition
  that ends combat.
  EXE-GOG-COMBAT-004 separately establishes the panel's native BMP
  request/cache path but identifies no combat owner or overlay semantics.
  `EXE-GOG-COMBAT-008` reaches that static panel from a four-record operation,
  but its immediate continuation is a shared `stdpatch`-related initialization
  route and a nearby `0x92e0` operand has no matching `RESOURCE.GFF` resource;
  it supplies no independent combat attribution.
  EXE-GOG-COMBAT-006 classifies its direct eight-call fan-in as shared and
  feature-neutral: the visible boundary has only `0`/`1` arguments and no
  recovered combat identity.
  The owner additionally confirms that combat has no dedicated post-combat
  return frame: on exit, presentation immediately resumes ordinary exploration
  and the visible party collapses to the single leader. This establishes only
  the resulting presentation mode, not an exit condition, outcome, timing, or
  state-transition rule.
- **Implementation:** required-revision-34 extraction emits the bounded one-frame
  `images/combat/status-panel.dsix` derivative from `RESOURCE.GFF:BMP #19003`
  with the interface palette and the observed 98x32 frame. The manifest conversion
  retains source provenance; no Game renderer, value model, record reader, or
  combat update loop consumes this evidence-only asset.
- **Confidence:** high for the owner-confirmed screenshot labels, reported
  direct click interaction and absent visible target/turn-transition treatment,
  visible enemy motion, movement-point-display change, the visible combat turn
  panel, and visible damage glyph; unknown for dynamic field meanings and all
  combat mechanics.
- **Implementation consequence:** do not create a movement-point cost model,
  damage pipeline, combat state machine, or timing policy from these
  still-bounded visual facts.

### DATA-GOG-COMBAT-001 - Panel caption has no direct ASCII source lead

- **Question:** Can the visible `Moves` caption in the owner-confirmed
  Thy'rokh combat panel be assigned to a static asset or an exact printable
  ASCII string in the owned source corpus?
- **Method:** Decode the already identified `RESOURCE.GFF:BMP #19003` with
  `PAL #1000` to a temporary local preview outside the repository and inspect
  its 98x32 static artwork. Then run the compiled, bounded
  `resource-pattern` inspector for the exact printable ASCII pattern `Moves`
  over each of the 26 owned top-level GFF archives. The inspector returns only
  archive/tag/descriptor metadata on a match; no payload is retained.
- **Finding:** BMP #19003 contains the panel chrome but no baked `Moves`
  caption. No GFF resource has the exact printable ASCII pattern `Moves`.
  The observed caption therefore has no direct static bitmap or exact-ASCII
  source lead from these queries.
- **Confidence:** high for the inspected static panel and the exact,
  case-sensitive printable-ASCII corpus query; unknown for different text
  encodings, split/constructed strings, runtime formatting, font drawing,
  source ownership, and every value or turn semantic.
- **Implementation consequence:** preserve the existing evidence-only panel
  asset and keep its dynamic area opaque. Do not hard-code the caption, assign
  its values, or add combat presentation/logic from a failed direct source
  search.

### RULE-COMBAT-001 - Party expansion on combat entry

- **Behavior:** Exploration may show only the leader; combat makes all four party
  members visible. The manual documents Guard (`G`), target-next (`N`),
  target-previous (`P`), end-turn (`Q`), Wait (`W`), and Space to disable
  computer control. During combat, a Walk click makes the character approach
  and automatically attack the selected opponent. The owner reports that this
  is a direct enemy click, with no separately visible target-switching or
  confirmation presentation; the manual's word “selected” therefore does not
  establish a distinct visible selection state. Melee requires adjacency and a
  readied weapon; ranged requires an in-range opponent plus a readied missile
  weapon or ammunition. A two-weapon melee configuration requires one-handed
  weapons in both hands. The documented invalid cursor means the attempted
  target is not eligible for that attack.
- **Evidence:** SRC-MANUAL-1994, "How to Play" pages 4-6 and the visually reviewed
  hotkey table on manual page 77. `OBS-GOG-PARTY-001` records the owner's
  direct-click observation. `EXE-GOG-COMBAT-001` excludes only two
  direct-literal dispatcher forms, `EXE-GOG-COMBAT-003` excludes the queried
  `COMBAT`/`GUARD` label occurrences and the exact null-terminated `ATTACK`
  literal as direct command-path leads,
  `FND-INPUT-004` leaves the native mouse route screen-neutral and finds no
  caller of the button-press wrapper, and `FND-INPUT-005` shows the mouse and
  keyboard events queued as packets, but no packet consumer or screen owner.
  `EXE-GOG-COMBAT-013` now maps that coordinate branch through a bounded
  five-guard validation sequence, but identifies neither its flags/constants
  nor an enemy, target, action, or decision owner.
  `EXE-GOG-COMBAT-019` finds four literal computer-control labels only as
  unreferenced overlay data, so it establishes no default, toggle, or
  automation behavior.
  `EXE-GOG-COMBAT-014` places that branch's resident value in a broader
  combat-adjacent mode switch without assigning value five an attack or target
  meaning.
  `EXE-GOG-COMBAT-015` finds that the mode setter's only recovered direct call
  supplies value two, not the coordinate branch's value five.
  `EXE-GOG-COMBAT-016` proves a separate direct writer assigns value 19 to the
  same word, so the local one-through-five branch is not a finite mode model.
  `EXE-GOG-COMBAT-017` exhausts recovered direct writers without finding a
  literal-five producer; indirect and computed writes remain open.
  `EXE-GOG-COMBAT-018` finds no decoded function carrying both the static
  panel and interface-font request identities, which excludes only that direct
  renderer hypothesis.
  `FND-INPUT-007` excludes only two direct keyboard-controller I/O
  forms, and `FND-INPUT-008` finds no decoded function containing all
  six documented combat key values; none establishes the shipped command
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
- **Evidence:** SRC-MANUAL-1994 printed page 24, "Armor Class" and "THAC0". It explicitly
  defines the random roll range and inclusive threshold. SRC-GAMEFAQS-81038 section
  2.1 independently restates the subtraction model but describes a strict
  greater-than comparison and explicitly leaves automatic 1/20 behavior
  uncertain. That secondary-report discrepancy is retained as an open native
  behavior question rather than changing the manual-bounded helper.
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
- **Uncertainty:** Whether the shipped comparison is inclusive at equality
  (the manual and FAQ wording differ), RNG algorithm and consumption, modifier
  values/order, natural 1/20 behavior, weapons, damage, resistance,
  incapacitation, and all combat state transitions remain open.

### RULE-COMBAT-003 - Hit-point incapacity thresholds

- **Behavior:** Damage subtracts from the target's hit points. Positive hit
  points are conscious; zero through -9 are unconscious; -10 or less is dead.
- **Evidence:** SRC-MANUAL-1994 printed page 24, "Hit Points". `EXE-GOG-COMBAT-002`
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

### FAQ-81038-COMBAT-001 - Difficulty is a hostile-hit-point hypothesis

- **Question:** Does the shipped difficulty setting alter combat state, and if
  so, which state and at what lifecycle point?
- **Secondary report:** SRC-GAMEFAQS-81038 section 2.8 reports that a new game begins on
  Balanced, that Easy appears to use roughly half the Balanced hostile hit
  points while Hideous appears to use roughly twice them, and that the effect
  is applied when a creature is spawned rather than retroactively. The report
  does not establish Hard's multiplier, a rounding rule, whether player state
  is affected, an encounter identity, or the native implementation path.
- **Evidence status:** community research only. The manual assigns difficulty to
  combat but gives no numerical contract; the executable exposes the four
  labels but no direct label-to-setting or setting-to-rule binding
  (`FMT-TEXT-004`, `FND-CONFIG-002`).
- **Reproduction target:** from equivalent clean saves, set each difficulty
  before entering a fresh controlled encounter; capture the same identified
  hostile's inspectable hit-point state before any action, then repeat after
  changing difficulty only after that hostile has appeared. Confirm the
  proposed encounter, difficulty, and state labels with the owner before using
  captures semantically.
- **Confidence:** medium that the guide reports its own observation; unknown
  for every claimed native multiplier, timing boundary, state field, and scope.
- **Implementation consequence:** keep Preferences difficulty mutations inert
  and do not add combat HP scaling, spawn behavior, rounding, or compatibility
  handling until the reproduction target and a traceable data/executable path
  corroborate the contract.

## Conflict handling

SRC-GAMEFAQS-81038 reports discrepancies between documentation and shipped behavior.
Each conflict receives its own rule ID, both claims, reproduction procedure, and
owner decision. No compatibility behavior is selected from plausibility alone.

# Complete implementation and migration plan

**Status:** approved - implementation may proceed.
**Approved by:** repository owner on 2026-09-12, covering slices 1-7.

This is the approval gate for specializing the clean-room template as a
restoration of *Dark Sun: Wake of the Ravager*. Approval covers the proposed
identity, extractor/runtime boundary, and ordered slices below. Unverified
behavior remains an open question until evidence closes it.

## Game profile

| | |
|---|---|
| Original title | *Dark Sun: Wake of the Ravager* |
| Developer | Strategic Simulations, Inc. (SSI); story and game design are credited to the SSI Special Projects Team |
| Original release year | 1994 |
| Genre | Single-player, party-based computer role-playing game using AD&D 2nd Edition rules in the Dark Sun setting |
| Editions available for validation | Legally owned English GOG installation at `C:\GOG Games\Dark Sun 2`, GOG product ID `1432903719`, installed build ID `52095422060333615`. The underlying DOS game-data revision is not yet established. |
| Existing research relied on | Local manual: `C:\GOG Games\Dark Sun 2\ds_wakerave_manual_pdf.pdf`; kibbitz, *Dark Sun: Wake of the Ravager - Guide and Walkthrough*, v1.13, GameFAQs FAQ 81038, updated 2026-06-18: <https://gamefaqs.gamespot.com/pc/564927-dark-sun-wake-of-the-ravager/faqs/81038>; John Glassmyer's MIT-licensed `dsun_music` resource tools and research: <https://github.com/JohnGlassmyer/dsun_music>; read-only inspection and reproducible runtime observations of the owned GOG copy |

### Durable original-analysis source

The repository owner confirms that agents may always rely on the legally owned
installation at `C:\GOG Games\Dark Sun 2` being available for read-only analysis
throughout this migration, including focused analysis of
`C:\GOG Games\Dark Sun 2\DSUN.EXE`. Agents do not need to ask again before using
that installation for in-scope evidence work. Before interpreting executable
addresses or data offsets, verify the file against the supported-edition
manifest. The current `DSUN.EXE` is 634,416 bytes with SHA-256
`ce02ee1f31c2339fc3e16926e370639af782a5ecd6c8a6081140fa23445fc92c`.
If the path is unavailable or the fingerprint changes, record the mismatch and
stop drawing edition-specific conclusions until the source-edition record is
updated; never silently substitute another copy.

This availability does not relax the clean-room boundary: the installation is
an evidence source only. Original executables, assets, extracted bytes, analysis
projects, disassembly, decompiler output, screenshots, and saves remain local
and must never enter Git, CI artifacts, or distributed packages.

### Terminology mapping

The original game and manuals use **race** as the conventional fantasy term for
peoples such as humans, elves, dwarves, and similar character origins. It is
not intended by this project in an offensive or real-world racial sense. The
reimplementation maps original **race** terminology to **origin** so that new
code, APIs, UI, tests, and project-authored descriptions use respectful modern
language. Evidence records may retain **race** or **racial** only when quoting
or naming an original heading, table, field, or claim; those source terms map
to **origin** and **origin-based** in implementation.

### Initial evidence register

- **MANUAL-1994.** The locally installed 41-page landscape PDF rule book (77
  numbered manual pages plus credits and legal material). It is primary evidence
  for intended controls, menus, party creation, exploration, combat commands,
  character rules, magic, psionics, advancement, and original credits. The path
  is a local reference only; the PDF must never enter Git.
- **GOG-1432903719.** Installed English GOG metadata plus a future exact
  fingerprint inventory. Storefront branding is provenance, not a fingerprint.
- **FAQ-81038.** kibbitz's walkthrough, v1.13, used as secondary evidence for
  mechanics, route conditions, bugs, soft locks, and conflicts with the manual.
  Runtime confirmation is required where feasible. The guide credits
  contributors Seraphiel, @revcrussell, UndeadHalfOrc, classiccola, GHostLPs,
  and rattus 128.
- **DSUN-MUSIC.** John Glassmyer's MIT-licensed `dsun_music` project, whose
  `gff-tool`, `image-tool`, `region-tool`, and `xmi-tool` describe and extract
  resources used by *Shattered Lands*, *Wake of the Ravager*, and *Crimson
  Sands*. The repository owner explicitly authorizes reuse of useful results
  from this project. Treat its format descriptions, resource tags, mappings,
  and tool behavior as secondary technical evidence: credit the project, note
  any directly reused code under its license, and confirm applicable facts
  against the fingerprinted GOG-1432903719 files before making a verified
  format or parity claim. Production readers must still satisfy this plan's
  bounds, diagnostics, synthetic-test, and clean-room requirements.
- **OBS-GOG-*.** Reproducible observations recorded from controlled runs of the
  supported copy. Original screenshots, recordings, and saves stay outside Git;
  measurements and clean-room diagrams may be committed.
- **DATA-GOG-*.** Facts established by bounded, read-only inspection of owned
  files. No original bytes, source, decompiler output, or disassembly enter the
  repository.

MANUAL-1994 describes intended behavior; FAQ-81038 documents cases where the
shipped game differs. Conflicts will be preserved explicitly, never silently
resolved.

### Recommended Ghidra usage

Ghidra is recommended as a targeted evidence tool when the manual, walkthrough,
runtime observation, and bounded data inspection do not establish an exact rule,
file field, state transition, or RNG/timing behavior. Analysis should begin with
the exact fingerprinted `DSUN.EXE` from GOG-1432903719, with the Ghidra version,
executable SHA-256, load settings, address or symbol, method, interpretation,
confidence, and reproducible follow-up recorded in `docs/GHIDRA.md`.

The Ghidra project, executable, memory dumps, screenshots, raw disassembly, and
decompiler output remain under ignored `analysis/original/` or another local-only
location. Repository documentation must describe findings independently in our
own words; implementation must express independently designed behavior and link
to a stable evidence entry and synthetic test. Prefer narrow scripted queries
that can be rerun against a fingerprinted binary. Do not use Ghidra to recover or
copy original source structure when observation or data evidence already answers
the question.

### Proposed project identity

| Field | Proposed value |
|---|---|
| Project/namespace name | `DarkSunWakeRedux` |
| Display name | `Dark Sun: Wake of the Ravager Redux` |
| Game/package identifier | `dark-sun-wake-redux` |
| Publisher and copyright holder for new code | `kibertoad` |
| Repository URL | `https://github.com/kibertoad/dark-sun-wake-redux` |
| Summary | Clean-room MonoGame restoration of SSI's 1994 computer role-playing game *Dark Sun: Wake of the Ravager* |

The configurator will generate the application ID, which then remains stable.
The root README will credit the original team from MANUAL-1994, led by the SSI
Special Projects Team, producers Dan Cermak and Nick Beliaeff, associate
producer Rick White, lead programmer Robert Calfee, programmer Mike Coustier,
and lead artist Maurie Manning. It will link and credit FAQ-81038 to kibbitz and
name the guide contributors above. The acknowledgement will not imply
endorsement or transfer of rights.

## Scope and architecture

The project will follow the proven `C:\sources\rechaos-overlords` product
shape: two separately runnable deliverables with a strict proprietary-content
boundary.

1. **`DarkSunWakeRedux.Extractor`** is a separate asset extractor. It accepts a
   user-selected GOG installation, verifies an exact supported fingerprint,
   decodes/transforms every required proprietary resource into a versioned local
   asset pack, generates a provenance manifest, verifies the complete staged
   output, and installs it transactionally. It never modifies the GOG copy.
2. **`DarkSunWakeRedux.Game`** is the reimplemented MonoGame runtime. It runs
   only against the extractor's verified asset pack and never loads, executes,
   or depends on the original executable or DOSBox. Missing/incompatible packs
   produce an actionable error and a route to the extractor.
3. **`DarkSunWakeRedux.Core`** owns deterministic commands, events, rules, RNG,
   quest state, saves, and replay state with no MonoGame, parsing, or I/O.
4. **`DarkSunWakeRedux.Resources`** owns bounded original-file parsing and asset
   pack contracts with no MonoGame. The extractor and read-only Inspect tool use
   it; the Game consumes only the verified pack contract.

The generic template `Import` shell will be replaced by the player-facing
Extractor, while Inspect remains a separate read-only research tool. Extractor
logic and tests will reuse the Rechaos principles of exact inventory and SHA-256
validation, versioned manifests, full staged-pack verification, stale-output
replacement, rollback on failure, and machine-readable diagnostics; no code is
to be copied blindly between games.

The intended outcome is a portable recreation of party creation, exploration,
dialogue, combat, inventory, character advancement, magic, psionics, quests,
cinematics, sound, music, saving, and the ending flow. Original mouse and
documented keyboard actions are in scope. Controller support is a clearly
labeled modern mapping rather than an original-parity claim.

**Non-goals.** The repository and release packages will not contain original
assets, executables, archives, manuals, clue books, screenshots, save files, or
extracted data. They will not copy original source or static-analysis output,
reproduce DOSBox, invent cut content, rewrite the campaign, or claim support for
unowned/unfingerprinted editions. Multiplayer, a level editor, and unrelated
AD&D titles are outside the initial plan. Original-save or Shattered Lands party
compatibility is not promised until each format is evidenced.

## Slices

Every slice leaves the application runnable and keeps CI independent of the
original game.

| # | Slice | Player-visible outcome | Depends on | Status |
|---|---|---|---|---|
| 1 | Identity, source recognition, and diagnostic boot | The named runtime starts, finds a verified pack or explains how to create one, and quits cleanly; the separate Extractor recognizes the supported GOG copy | approval | complete |
| 2 | Extraction and title-to-party flow | The Extractor creates a verified local pack; the runtime reaches the start flow and creates/selects a four-character party | 1 | in progress - title extraction, evidenced start/party-overview composition, UI resource mapping, party invariants, and semantic start routing implemented |
| 3 | First Tyr exploration and conversation | The party enters Tyr, moves, interacts, completes the opening conversation, and uses character/inventory/game menus | 2 | planned |
| 4 | First deterministic combat | The opening encounter is playable through victory or defeat | 3 | planned |
| 5 | Full character systems | Equipment, advancement, magic, psionics, camping, and training work from evidenced rules | 4 | planned |
| 6 | Quest graph and campaign traversal | The critical route and evidenced branches can be played through the finale | 5 | planned |
| 7 | Persistence, presentation, parity, and packaging | Native saves/replays, full audiovisual presentation, validated campaign coverage, and clean packages complete the restoration | 6 | planned |

### Slice 1 - Identity, source recognition, and diagnostic boot

**Status:** complete on 2026-09-12. The source manifest uses seven exact immutable
fingerprint anchors; full extraction-input coverage expands with each bounded
decoder rather than being guessed now.

- **Outcome.** Configure the approved identity. The separate Extractor accepts
  an explicit GOG path and reports supported, missing, changed, or unsupported
  sources. The assetless runtime names the required pack, points to the
  Extractor, writes a local diagnostic log, and quits cleanly. README status and
  acknowledgements are accurate.
- **Evidence.** MANUAL-1994 for identity and creators; GOG-1432903719 for owned
  distribution metadata; FAQ-81038 for guide attribution.
- **Acceptance - rules.** Recognition is deterministic and exact. Relative paths
  are normalized; duplicate files and path escapes are rejected; every required
  file has an expected length and SHA-256; failure uses stable diagnostic codes.
- **Acceptance - presentation.** Both tools have readable success/error output,
  keyboard/mouse dismissal where graphical, and no unhandled failure. Native UI
  parity is not yet claimed.
- **Acceptance - original content.** One manifest records the exact installed
  GOG revision, language, required files, sizes, hashes, and provenance. No
  extraction occurs until the full source fingerprint passes. Nothing from the
  selected source enters Git, build output, telemetry, or CI logs.
- **Automated tests.** Configuration/architecture checks; source-manifest schema;
  matching, missing, wrong-size/hash, duplicate, traversal, casing, inaccessible
  path, and stable diagnostic tests using synthetic files; assetless runtime and
  repository-policy tests.
- **Observed parity.** Verify titles and credits against the manual and source
  recognition against the owned install. Record fingerprints only.

### Slice 2 - Extraction and title-to-party flow

- **Outcome.** The Extractor decodes the minimum complete resource set into a
  versioned local pack, verifies it, and promotes it transactionally. The runtime
  opens that pack, reaches the original-style start flow, and creates or selects
  a legal four-character party.
- **Evidence.** MANUAL-1994 sections on quick start, party creation, character
  options, and menus; `DATA-GOG-GFF-001` and `DATA-GOG-IMAGE-001`, corroborated
  by DSUN-MUSIC, for bounded container, indexed-image, and palette structures;
  `DATA-GOG-FONT-001` for bounded indexed glyphs; `DATA-GOG-UI-001` and
  `DATA-GOG-UI-006`-`008` for bounded start-window/button mappings, composition,
  party-overview and ADD-list shells, and interface palette; further DATA-GOG
  for party-screen resource mapping; OBS-GOG for screen states,
  coordinates, and navigation.
- **Acceptance - rules.** Party size, available origins/classes, ability/alignment
  constraints, psionic-discipline and clerical-sphere choices, cancellation,
  selection, and derived initial state follow recorded evidence. A fixed seed
  makes allowed random generation repeatable. The manual-documented discipline
  and sphere cardinality is implemented; shipped selection behavior remains to
  be observed. The manual's complete origin ability-modifier table is exposed
  as immutable Core data without assuming application order or score caps.
  Pre-adventure occupied-slot EDIT and DROP-to-ADD storage are
  implemented. The manual-evidenced human dual-class level gates, sequential
  career limit, former-benefit boundary, DUAL selection command, and atomic
  party update are deterministic Core rules. Class progression participates in
  snapshots and state hashes; player-visible DUAL presentation and shipped
  initial-level evidence remain open.
- **Acceptance - presentation.** Start/party screens preserve measured logical
  coordinates, aspect treatment, palette semantics, focus order, mouse hit
  regions, and Escape behavior; scaling cannot change rules or hit testing.
  Implemented start-window drawing composes the two evidenced interface-palette
  shell layers and DSUI-resolved controls over black. Clicks resolve child
  coordinates, dimensions, image identities, and semantic button identities,
  then use a single inverse canvas transform. A shared bounded resolver now
  materializes typed controls for each of the seven extracted start-flow windows
  while preserving child order, geometry, event masks, and optional image
  references. The party screen's full-canvas #2099 application surface is
  validated exactly, while its internal slot partition remains gated on
  application-specific observation. Encoding-neutral DSFT glyph-run and multiline-block composition and the owned
  font's identity map are verified; generalized map semantics, shipped spacing,
  palette, edge/focus behavior, and later screens remain open.
- **Acceptance - original content.** Readers bound offsets, counts, sizes,
  decompression, names, and output paths. The pack records extractor/format
  version, source fingerprint, output inventory, per-file hashes, media types,
  source mapping, and conversion method. Staging is fully verified before
  replacement, and failure restores the last valid pack.
- **Automated tests.** Synthetic parser boundary/fuzz tests; traversal and
  decompression-bomb limits; deterministic extraction; exact output inventory;
  pack verification, stale-file removal, atomic promotion/rollback; party
  invariants, exact origin-modifier table, and menu-transition tests.
  Implemented start-flow commands produce
  sequenced events, versioned snapshots, stable hashes, and verified replays.
  Snapshot schema 4 includes psionic disciplines, clerical sphere, ordered
  class/level progression, the active member during DUAL selection or editing,
  and recreation-native stored characters;
  DSUI round-trip, malformed graph, deterministic ordering, mixed-control graph resolution, resource-ID routing,
  rectangle-edge, synthetic extraction, content-smoke, and owned 99-asset pack verification cover the derived
  start-flow layout contract;
  gameplay-wide replay remains a later-slice requirement.
- **Observed parity.** Compare boot, start, create/select/cancel, and quit state
  transitions and UI measurements against the supported GOG build.

### Slice 3 - First Tyr exploration and conversation

- **Outcome.** A party enters the first Tyr area, moves and scrolls, changes
  leader/formation display, uses look/interaction, completes the first dialogue,
  and opens character, inventory, effects, map, and game menus.
- **Evidence.** MANUAL-1994 "How to Play", mouse modes, character interaction,
  character options, and game menu; FAQ-81038 section 3.1; DATA-GOG region facts;
  OBS-GOG opening traces.
- **Acceptance - rules.** Click-to-walk, collision, leader selection, party
  placement, interaction eligibility, dialogue choices, item transfer, and
  initial quest flags are deterministic Core commands/events. Per
  `COMPAT-PATH-001`, routes may use modern deterministic pathfinding rather than
  reproduce the original planner's deficiencies.
- **Acceptance - presentation.** Viewport and scrolling, cursor modes and target
  hotspot, dialogue/menu layering, portraits, and control states match measured
  atlas entries within recorded tolerances.
- **Acceptance - original content.** Required map, region, sprite, palette, text,
  portrait, and item resources are extracted. Unknown records remain explicitly
  unknown; dangling references and corrupt bounds produce contextual errors.
  `DATA-GOG-REGION-001` now bounds the shared region identity, 128x98 map and
  geometry planes, 16x16 local tiles, and eight-byte external-object references;
  `DATA-GOG-OBJECT-001` bounds exact 16-byte object-frame definitions, their
  signed offsets, and image references. Canonical DSRG and DSOB extraction is
  implemented for Tyr. `DATA-GOG-SCENE-001` now supplies a clipped static
  tile/first-object-frame compositor; controlled observation fixes the opening
  camera at `(1024,1368)` and the static viewport is visibly integrated.
  `DATA-GOG-ACTOR-001` identifies, extracts, and displays the exact opening
  leader at its observed world position through reusable camera-relative actor
  placement; its collision anchor cell is evidenced. Active Walk clicks now
  plan and execute from that anchor through a reusable controller, with a
  documented provisional single-cell footprint and 125 ms semantic step while
  fixed-point presentation interpolates toward the next route anchor. Native
  footprint/cadence and the extracted 13-frame image's animation semantics
  remain open.
  Manual-defined edge scrolling now drives a clamped deterministic Core camera
  and rerasterizes that viewport. Per owner-approved `COMPAT-INPUT-001`, a
  held right-button grab-drag also emits bounded logical-camera pans while a
  stationary right click retains the original mode cycle; Alt+Enter toggles
  native-resolution fullscreen on a single chord edge. `COMPAT-DISPLAY-001`
  expands the bounded world slice to the physical aspect ratio while fixed UI
  remains on the original canvas; F9 previews the measured dialogue chrome over
  that live world pending conversation routing and text. A fail-closed GPL #135
  reader projects the evidenced portrait, two conditional speech sources, and
  eight-entry initial menu and maps its three observed condition shapes to a
  deterministic Core true/false/unknown evaluator. The F9 validation hook now
  filters the initial menu in source order, hides false and unknown conditions,
  bounds the result to the five physical rows, and retains each choice's source
  index and branch target. The captured five-row state establishes choices 0,
  1, 2, 3, and 7 for this opening only. GPL #135's paired counter branches and
  the MIT libgff local-clear implementation support an opening-only local number
  0 value of zero. A corpus-wide reference sweep plus the same implementation's
  global reset support fresh-opening global flag 357 as false; generic
  initialization remains open. A bounded MAS #99
  projection resolves choice 7's global string #5 and the third menu's global
  string #6 exit label from
  the ignored owned pack without committing its text. A deterministic Core
  dialogue session owns the visible source-index/branch-target identities and
  records one physical-row selection atomically; runtime row clicks are consumed
  before world movement. Choice 0's straight-line target is now bounded at
  offsets 1017-1147: it validates three literal prints separated by two
  newlines, clears local flag 0, and returns locally. The bounded opening-menu
  continuation at offsets 547-740 then sets local flag 4 and advances into the
  seven-entry second menu projected at offset 750. Its transcript is projected
  but is not yet presented. Choice 7's target is a bounded completion projection:
  it validates a GSTRING #5 print followed by local flag 14/4 assignments and
  a return, advances Core with those effects to completed, and closes the
  preview. Choices 2 and 3 now validate their literal-print, matching flag-clear,
  local-number-0 increment, and local-return paths; after both execute, Core
  deterministically reveals source choice 4 while the continuation sets local
  flag 5 and retains the opening menu. Choice 1 validates three prints,
  its flag clear, the global-357 conditional local flag 6/7 effects, the global
  assignment, and return; Core applies these atomically from the known opening
  global state before advancing to the second menu. Choice 4 validates three
  prints, sets local flag 9, resets local number 0, and advances through the
  same continuation. The completion target works from both menus despite their
  reused source indexes because runtime branch dispatch is keyed by target.
  Returned choices 0-4 now replace the speech with their projected output,
  retaining explicit print-newline instructions; target 1597 returns to its
  calling page, and unprojected visible targets remain inert without partially
  selecting the session. Second-menu target 1825 is bounded through three
  prints, local flag 6 clear, and a local-flag-16 condition that sets local flag
  10; Core evaluates that condition before mutation and runtime returns to the
  second page with the newly enabled choice. Target 3479 is bounded through
  three prints, local flag 16 set, local flag 10 clear, and return, leaving
  source choices 3, 5, and 6. The captured opening also proves global number 22
  equals one because that condition selects the opening-menu subroutine. Target
  1996's matching path is bounded through its two prints, local flag 11 set,
  local flag 7 clear, and return, leaving source choices 4, 5, and 6; the
  alternate global-number path remains open. Target 2352 then prints its Acar
  answer, clears local flag 11, and leaves source choices 5 and 6. Target 2415
  then validates three prints, sets local flags 12/13, conditionally sets flag
  10 from flag 16, and advances through the local-flag-8 loop header to the
  projected seven-entry third menu at offset 2616. On the path where target
  3479 already set flag 16, its visible source order is 0, 1, and 6. Remaining
  third-menu responses, the second-menu alternate branch, and generalized GPL
  execution remain open. The hook renders the
  first literal speech and filtered labels with bounded provisional
  wrapping in the extracted font. Reusable ordered hotkey bindings and Core
  navigation now cover character, inventory, cast/psionic, current-effects,
  overhead-map, and game-menu views plus menu-return/exit semantics. Their
  original Game Menu base and 14 controls now extract and render through a
  DSUI-resolved semantic page object; evidenced destinations and mode/return
  actions are clickable. Center on Leader now targets the authoritative moving
  sprite center through a deterministic clamped Core camera command, and
  Collapse Party returns to leader-only play. Character and inventory now render
  their resource-backed shells and route the
  five shared bottom-navigation controls through one reusable destination page
  object. Controlled owned observation establishes that Cast/Use and Current
  Effects reuse the character shell and #11500 navigation with exact
  #20080/#20075 title placement; both now render. The Preferences #16500 graph
  and artwork now render and its Game Menu/Return actions route deterministically.
  Controlled cursor observations map `ICON` #19101-#19110, verify the
  manual-defined upper-left hotspot and all six Walk/melee/Look valid/invalid
  states, and identify OJFF #9258 as the first observed melee target. Pack v21
  extracts all ten cursor images; the runtime renders reachability-based Walk,
  topmost-entity/leader Look, and bounded first-target melee feedback.
  Dynamic fields/interior actions, generalized target eligibility, Exit, Load/Save, setting mutations,
  other destination presentation, native panel/centering validation, animation,
  and party/interface overlays remain pending. `EXE-GOG-REGION-001` now establishes
  `GMAP` bit `0x40` as the terrain/occupancy block. A bounded terrain grid and
  reusable camera-to-grid Walk-click planner connect Tyr to deterministic A*.
  A clock-free Core movement session now covers atomic plan/replan, one-cell
  advancement, cancellation, completion, and newly blocked route interruption
  independently of runtime timing. A separate deterministic
  occupancy session atomically places, moves, and removes caller-supplied
  multi-cell footprints over terrain and supplies live whole-footprint route
  predicates. A Core actor-movement aggregate now keeps route and occupancy
  anchors synchronized and interrupts rejected commits. A reusable runtime
  actor controller now executes those commands from the evidenced opening
  anchor, bounds catch-up work, and exposes fixed-point progress used to
  interpolate the sprite without changing Core state. Native party/NPC
  footprints, cadence, and sprite-frame animation remain open.
- **Automated tests.** Synthetic-map navigation/collision, deterministic command
  traces, dialogue branches, inventory conservation, menu routing, invalid
  resource reference, and malformed-region tests.
- **Observed parity.** Replay identical opening inputs and compare positions,
  available responses, state flags, inventory, and UI geometry without
  committing original captures.

### Slice 4 - First deterministic combat

- **Outcome.** The opening Tyr encounter can be completed through victory or
  party defeat using movement, targeting, attacks, wait, guard, previous/next
  target, and end-turn actions.
- **Evidence.** MANUAL-1994 combat mouse modes and hotkeys; FAQ-81038 sections
  2.1, 2.4, 2.8, and 3.1; OBS-GOG controlled combat traces.
- **Acceptance - rules.** Activation order, movement, range, target legality,
  hit/damage resolution, armor class, THAC0, incapacitation, experience,
  difficulty, guard/wait, victory, and defeat are deterministic from state,
  commands, and an explicit RNG stream. Each implemented formula has an evidence
  record; conflicts stay visible.
- **Acceptance - presentation.** Combat transition, expanded party, reachable
  and target feedback, cursor state, animation/event order, messages, and sound
  triggers meet captured tolerances. Frame rate never drives rules.
- **Acceptance - original content.** Combat visuals/audio/data are extracted and
  pack-verified. Missing optional presentation degrades explicitly; missing
  required rule data blocks play with an actionable diagnostic.
- **Automated tests.** Golden synthetic combat traces; RNG consumption; hit,
  damage, movement, targeting and difficulty boundaries; victory/defeat;
  serialization snapshots; presentation-independent resolution; malformed
  combat-resource tests.
- **Observed parity.** Repeat the opening fight at controlled difficulty and
  compare command availability, outcomes, state transitions, and audiovisual
  event ordering.

### Slice 5 - Full character systems

- **Outcome.** Players equip legal items, inspect statistics/effects, use the
  evidenced spell and psionic set, camp, recover, earn experience, and train.
- **Evidence.** MANUAL-1994 character, ability, class, equipment, spell,
  psionic, camping, training, and advancement sections; FAQ-81038 sections
  2.1-2.9 and recorded discrepancies; OBS-GOG rule probes.
- **Acceptance - rules.** Every implemented modifier, restriction, resource
  cost, target, duration, effect, recovery rule, multiclass behavior, experience
  threshold, and level gain has an evidence ID and deterministic test. A verified
  original defect may become a named compatibility choice only by decision.
- **Acceptance - presentation.** Inventory, spell, psionic, current-effect,
  camping, and training screens preserve measured navigation and feedback.
  Controller actions map to the same semantic commands.
- **Acceptance - original content.** Item/spell/power definitions and associated
  art/audio are extracted through bounded readers. Parsed data cannot introduce
  executable behavior.
- **Automated tests.** Table-driven rule tests; equip/unequip conservation;
  target/duration edges; rest interruption/recovery; advancement invariants;
  deterministic random effects; save snapshots; parser property/fuzz tests.
- **Observed parity.** Purpose-built parties probe boundaries in both runtimes;
  results and source conflicts are recorded by rule ID.

### Slice 6 - Quest graph and campaign traversal

- **Outcome.** The critical campaign route from Tyr through the artifact regions
  to the finale is finishable; optional content and alternate outcomes are added
  as separately validated increments.
- **Evidence.** MANUAL-1994 for intended player systems; FAQ-81038 sections
  3.1-3.25 as a route, branch, and defect index; DATA-GOG facts; OBS-GOG
  checkpointed playthroughs. The guide is not an executable specification.
- **Acceptance - rules.** Quest flags, dialogue prerequisites, travel edges,
  item gates, timers, triggers, alternative outcomes, NPC survival, rewards, and
  ending prerequisites are explicit deterministic state machines. Known soft
  locks receive an evidence-backed fidelity decision.
- **Acceptance - presentation.** Region transitions, dialogue/cutscene
  sequencing, maps, and feedback for gated actions match observations.
- **Acceptance - original content.** Narrative/region resources remain only in
  the local pack. Readers bound identifiers and graph/text references; invalid
  links fail with source context.
- **Automated tests.** Model-based quest graph, critical and alternate route
  replays, unreachable/duplicate state checks, timer boundaries, invalid/cyclic
  reference safety, and synthetic end-to-end campaign fixtures.
- **Observed parity.** Checkpoint every critical transition and documented branch
  point; compare flags, inventory, party state, encounters, and outcome.

### Slice 7 - Persistence, presentation, parity, and packaging

- **Outcome.** Native versioned saves and deterministic replays work; the full
  campaign uses extracted graphics, animation, text, cinematics, speech, sound,
  and music; clean packages install the runtime and separate Extractor on each
  declared platform. Original save and Shattered Lands party import ship only if
  their formats become fully evidenced and bounded.
- **Evidence.** All prior evidence; MANUAL-1994 save/load, hotkeys, and party
  transfer; FAQ-81038 section 2.10 and complete route; comprehensive OBS-GOG and
  DATA-GOG inventories.
- **Acceptance - rules.** Save schemas and migrations are explicit; writes are
  atomic with last-valid recovery; corrupt data is bounded/rejected; identical
  initial state, seed, and commands produce identical hashes. Presentation clocks
  do not alter rules. Imported legacy fields, if supported, all have evidence.
- **Acceptance - presentation.** Every screen has a UI-atlas entry for logical
  dimensions, palette, layers, transparency, frames, draw order, hit rectangles,
  transitions, and timing. Audio/video synchronization is measured. Modern
  fullscreen, controller, and accessibility options are labeled extensions.
- **Acceptance - original content.** The Extractor produces the complete,
  versioned, exact-inventory asset pack transactionally. Runtime, installer,
  archives, symbols, logs, CI artifacts, and tests contain no original bytes.
  Manual source selection always works and the original install is untouched.
- **Automated tests.** Save/replay round trips, migrations, corruption and atomic
  recovery; full synthetic campaign replay; asset referential integrity and
  coverage; screenshot tests with synthetic stand-ins; audio/video scheduling;
  clean-machine package inspection; install/upgrade/uninstall and smoke tests.
- **Observed parity.** Complete a recorded critical-path playthrough and validate
  optional routes by parity-matrix row. No broad parity claim is allowed while a
  required row is unknown or merely implemented.

## Open questions

| ID | Question | Blocks | Owner | Status |
|---|---|---|---|---|
| Q1 | Is `DarkSunWakeRedux` / `Dark Sun: Wake of the Ravager Redux` the approved identity? | configuration | repository owner | closed - approved 2026-09-12 |
| Q2 | Is the installed GOG build the only initial supported edition, with later revisions represented by separate manifests? | slices 1, 7 | repository owner | closed - initial work targets the supplied GOG build; later revisions require separate fingerprints |
| Q3 | Which underlying DOS/CD-ROM revision is in GOG build `52095422060333615`? | slices 1, 4, 6 | evidence investigation | open |
| Q4 | Which source files and GFF/resource records are required, and what are their bounded structures? | slices 1-3 | evidence investigation | open - GFF, image/palette, FONT, TEXT, core UI records, the versioned `CHAR` envelope/identity/ability fields, raw `PSIN` companion envelope, region `RNME`/`PAL `/`MAP `/`GMAP`/`TILE`/`ETAB`, and exact `OJFF` object-frame records with image references are bounded; title/start, party overview, ADD-list, interface palette, generation/modal controls, shared window image, start-flow window graphs, APFM event masks, the owned font's identity map, encoding-neutral glyph composition, canonical Tyr DSRG/DSOB extraction, static first-frame scene composition, the opening Tyr camera, cursor image family, and `GMAP` `0x40` terrain blocking are implemented; #19004 still awaits an app-specific consumer; remaining character state, other mask/geometry/entity meanings, authentic text presentation, dynamic party data, later-window semantics, later cameras/animation/gameplay behavior, other region dependencies, and the rest of the minimum packs remain unknown |
| Q5 | What are the logical resolution, pixel aspect, palettes, cursor geometry, animation cadence, and audio timing? | slices 2-7 | runtime observation | open - the logical resolution is 320x200; `PAL ` #1000 supplies the interface/cursor colors; `ICON` #19101-#19110 geometry and the upper-left cursor hotspot are verified; pixel aspect, animation cadence, cursor update cadence, and audio timing remain open |
| Q6 | For each verified manual/guide/runtime conflict or original defect, should compatibility preserve it, fix it, or expose an option? | slices 4-7 | repository owner after evidence | open |
| Q7 | Are original save compatibility and Shattered Lands party transfer desired once their formats are evidenced? | slice 7 | repository owner | open |
| Q8 | Are Windows, Linux, and macOS all first-release targets, or should the initial release target Windows? | slice 7 | repository owner | open |
| Q9 | May the installed clue book be consulted as an additional local secondary source? | slices 4-7 | repository owner | open |
| Q10 | What measured tolerances define acceptable visual, input, animation, and audio parity? | slices 2-7 | repository owner/evidence investigation | open |
| Q11 | Which origin/class eligibility list does the shipped creation screen enforce where the original manual's race descriptions on pages 17-18 conflict with its class descriptions on pages 19-22 (half-giant ranger/thief, mul druid, thri-kreen druid/thief)? | slice 2 | OBS-GOG evidence investigation | open - Core preserves these as `EvidenceConflict` |
| Q12 | Which four disc `CHAR` resources does START GAME select as the supplied pregenerated party? | slice 2 | OBS-GOG/DATA-GOG evidence investigation | open - disc blocks #40-#43 and #50-#53 are bounded; one independently reported default member maps to #43, but the other three selections are not established |
| Q13 | Must pathfinding reproduce the original route planner verbatim? | slice 3 | repository owner | closed - no; owner approved a modern fit-for-purpose implementation on 2026-09-13 |

## Risks

- **Unknown containers.** The observed installation contains `.GFF`, `.FLI`,
  `.VOC`, `.BIN`, and disc-image resources whose exact roles/layouts are not yet
  fully established. Use DSUN-MUSIC as a starting point where it covers the
  format, validate those results against this exact build, then preserve the
  evidence in our own format notes. Start with read-only inventory, bound every
  field, use synthetic fixtures, and extract the smallest complete vertical
  slice before breadth.
- **Storefront drift.** GOG may change files without changing the product name.
  Match exact manifests; retain product/build metadata only as provenance.
- **Defects and conflicts.** FAQ-81038 reports manual discrepancies, performance
  sensitivity, and soft locks. Preserve conflicts and require explicit fidelity
  decisions after controlled reproduction.
- **Extractor correctness.** Partial or stale packs could mix revisions. Use a
  versioned manifest, exact output inventory and hashes, staging, complete
  read-back verification, atomic promotion, rollback, and machine-readable
  diagnostics as in the Rechaos architecture.
- **Large state space.** Quest branches and NPC survival are combinatorial. Use
  explicit state machines, model-based tests, checkpoints, and deterministic
  replays.
- **Content leakage.** Extend denied extensions from the real inventory, keep
  scratch/captures outside tracked paths, inspect staged changes, and run
  repository verification throughout.
- **Trademark/credit ambiguity.** Obtain owner approval for identity, retain a
  clean-room/no-endorsement disclaimer, and use accurate manual and guide
  attribution.
- **Cross-platform variance.** Canonicalize internal paths and isolate platform
  input/audio/filesystem behavior behind tested adapters.

## Done when

All seven slices are complete; the separate Extractor recognizes a licensed,
fingerprinted GOG copy and transactionally produces a complete verified local
asset pack; the reimplemented runtime alone runs that pack through a finishable
campaign with deterministic saves/replays; the README, source-edition record,
formats, rules/evidence ledger, UI atlas, fidelity ledger, bootstrap checklist,
and parity matrix match demonstrated reality; all questions are closed or
explicitly deferred as non-goals; the solution and smoke test build; all
repository/configuration/test scripts pass without proprietary content; and
each declared package passes clean-machine install, extract, launch, save,
reopen, and uninstall checks.

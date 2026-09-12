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
| 2 | Extraction and title-to-party flow | The Extractor creates a verified local pack; the runtime reaches the start flow and creates/selects a four-character party | 1 | planned |
| 3 | First Tyr exploration and conversation | The party enters Tyr, moves, interacts, completes the opening conversation, and uses character/inventory/game menus | 2 | planned |
| 4 | First deterministic combat | The opening encounter is playable through victory or defeat | 3 | planned |
| 5 | Full character systems | Equipment, advancement, magic, psionics, camping, and training work from evidenced rules | 4 | planned |
| 6 | Quest graph and campaign traversal | The critical route and evidenced branches can be played through the finale | 5 | planned |
| 7 | Persistence, presentation, parity, and packaging | Native saves/replays, full audiovisual presentation, validated campaign coverage, and clean packages complete the restoration | 6 | planned |

### Slice 1 - Identity, source recognition, and diagnostic boot

**Status:** complete on 2026-09-12. The source manifest uses six exact
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
  further DATA-GOG for resource mapping; OBS-GOG for screen states,
  coordinates, and navigation.
- **Acceptance - rules.** Party size, available races/classes, ability/alignment
  constraints, cancellation, selection, and derived initial state follow
  recorded evidence. A fixed seed makes allowed random generation repeatable.
- **Acceptance - presentation.** Start/party screens preserve measured logical
  coordinates, aspect treatment, palette semantics, focus order, mouse hit
  regions, and Escape behavior; scaling cannot change rules or hit testing.
- **Acceptance - original content.** Readers bound offsets, counts, sizes,
  decompression, names, and output paths. The pack records extractor/format
  version, source fingerprint, output inventory, per-file hashes, media types,
  source mapping, and conversion method. Staging is fully verified before
  replacement, and failure restores the last valid pack.
- **Automated tests.** Synthetic parser boundary/fuzz tests; traversal and
  decompression-bomb limits; deterministic extraction; exact output inventory;
  pack verification, stale-file removal, atomic promotion/rollback; party
  invariants and menu-transition tests.
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
  initial quest flags are deterministic Core commands/events.
- **Acceptance - presentation.** Viewport and scrolling, cursor modes and target
  hotspot, dialogue/menu layering, portraits, and control states match measured
  atlas entries within recorded tolerances.
- **Acceptance - original content.** Required map, region, sprite, palette, text,
  portrait, and item resources are extracted. Unknown records remain explicitly
  unknown; dangling references and corrupt bounds produce contextual errors.
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
| Q4 | Which source files and GFF/resource records are required, and what are their bounded structures? | slices 1-3 | evidence investigation | open - GFF directories and indexed-image/palette payloads are bounded; the minimum pack and resource mappings remain unknown |
| Q5 | What are the logical resolution, pixel aspect, palettes, cursor geometry, animation cadence, and audio timing? | slices 2-7 | runtime observation | open |
| Q6 | For each verified manual/guide/runtime conflict or original defect, should compatibility preserve it, fix it, or expose an option? | slices 4-7 | repository owner after evidence | open |
| Q7 | Are original save compatibility and Shattered Lands party transfer desired once their formats are evidenced? | slice 7 | repository owner | open |
| Q8 | Are Windows, Linux, and macOS all first-release targets, or should the initial release target Windows? | slice 7 | repository owner | open |
| Q9 | May the installed clue book be consulted as an additional local secondary source? | slices 4-7 | repository owner | open |
| Q10 | What measured tolerances define acceptable visual, input, animation, and audio parity? | slices 2-7 | repository owner/evidence investigation | open |
| Q11 | Which race/class eligibility list does the shipped creation screen enforce where manual pages 17-18 conflict with pages 19-22 (half-giant ranger/thief, mul druid, thri-kreen druid/thief)? | slice 2 | OBS-GOG evidence investigation | open - Core preserves these as `EvidenceConflict` |

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

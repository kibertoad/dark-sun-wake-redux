# Implementation plan

This file says what the project intends and what is true now, following the
[work protocol](../vendor/upstream/work-protocol.md). It holds no dated
checkpoints, no status narration and no research questions: git keeps the
history, `queue/` holds the research questions, `docs/DECISIONS.md` the
owner's decisions, and `docs/HANDOVER.md` says where the last session stopped.

## Game profile

| | |
|---|---|
| Original title | *Dark Sun: Wake of the Ravager* |
| Developer | Strategic Simulations, Inc. (SSI), SSI Special Projects Team |
| Publisher | Strategic Simulations, Inc. |
| Release year | 1994 |
| Genre | Single-player, party-based computer role-playing game using AD&D 2nd Edition rules in the Dark Sun setting |
| Eligibility | Qualifies: released in 1994, and no official remake or remaster is on sale. GOG and Steam sell the original 1994 game running in DOSBox. Checked 2026-09-26 by a web search for a remake, remaster or enhanced edition, which found none. Settled unless the owner asks for a re-check. |
| Latest version | 1.1, the version `SRC-README-1.1` gives the installed game data. No later official patch is recorded. |
| Editions available for validation | `BLD-GOG-EN-1.1`; `docs/SOURCE-EDITIONS.md` holds the detail. |
| Existing research relied on | `SRC-MANUAL-1994`, `SRC-GAMEFAQS-81038`, `SRC-DSUN-MUSIC-79B6927`, `SRC-LIBGFF-839B11D`, `SRC-OPENDS-5C6CBD7`, `SRC-README-1.1`, `SRC-YOUTUBE-FLOMVOSHEOM` |
| Stage | Slices. Intake and Runtime access have ended. The earlier Survey exit is recorded in `docs/BOOTSTRAP-CHECKLIST.md`; its refreshed complete-file-denominator re-audit remains open as `Q-EXE-003`. |

## Scope

The rebuild recreates party creation, exploration, dialogue, combat,
inventory, character advancement, magic, psionics, quests, cinematics, sound,
music, saving and the ending flow, with the original's mouse and documented
keyboard actions. Controller support is a labelled modern mapping, and makes
no parity claim.

It ships as two separately runnable programs with a strict boundary around the
original's content:

1. `DarkSunWakeRedux.Extractor` accepts a user-selected GOG installation,
   verifies an exact supported fingerprint, inventories every source file and
   resource, and turns every immutable game-data payload into a revisioned
   local asset pack. Known formats use bounded contracts, and unknown payloads
   a bounded lossless opaque contract with no meaning assigned. It writes a
   provenance manifest, verifies the complete staged output, installs it
   transactionally, and never modifies the GOG copy.
2. `DarkSunWakeRedux.Game` runs only against the verified pack. It never
   loads, runs or depends on the original executable or DOSBox, and a missing
   or incompatible pack produces an actionable error pointing to the
   Extractor.

`DarkSunWakeRedux.Core` holds deterministic commands, events, rules, the
random number generator, quest state, saves and replays, with no MonoGame,
parsing or I/O. `DarkSunWakeRedux.Resources` holds bounded parsing of the
original's files and the asset-pack contracts, with no MonoGame.

**Non-goals.** The repository and packages contain no original assets,
executables, archives, manuals, clue books, screenshots, saves or extracted
data. The project does not copy original source or static-analysis output,
reproduce DOSBox, invent cut content, rewrite the campaign, or support an
edition nobody has fingerprinted. Multiplayer, a level editor and other AD&D
titles are out of scope. Loading the original's saves and importing a
*Shattered Lands* party are not promised (owner question O2).

## Slices

Every slice leaves the application runnable and keeps CI independent of the
original game. `docs/RUNTIME.md` records that no agent runs the original here
and that the owner's live sessions capture frames only, so rule and screen
rows reach `validated` only where an owner capture is the evidence their tests
compare with. Format rows whose entries list files can reach `validated` from
the files alone. Every other target stops at `implemented`.

| # | Slice | Player-visible outcome | Depends on | Status |
|---|---|---|---|---|
| 1 | Identity, source recognition and diagnostic boot | The runtime starts, finds a verified pack or explains how to create one, and quits cleanly; the Extractor recognizes the supported GOG copy | none | complete |
| 2A | Complete source-corpus inventory and extraction | Every supported source payload is in a verified, source-mapped local pack, with no meaning inferred | 1 | complete |
| 2B | All-region structural catalogs | The pack holds a bounded structural catalog of every region archive | 2A | complete |
| 2 | Title-to-party flow | The runtime reaches the start flow and creates or selects a four-character party | 2A | in progress |
| 3 | First Tyr exploration and conversation | The party enters Tyr, moves, interacts, completes the opening conversation, and uses the character, inventory and game menus | 2 | in progress |
| 4 | First deterministic combat | The opening encounter can be played through to victory or defeat | 3 | evidence only: no combat behaviour is implemented |
| 5 | Full character systems | Equipment, advancement, magic, psionics, camping and training work | 4 | planned |
| 6 | Quest graph and campaign traversal | The critical route and the recorded branches can be played to the finale | 5 | planned |
| 7 | Persistence, presentation, parity and packaging | Native saves and replays, full audiovisual presentation and clean packages | 6 | planned |

Behaviour work beyond the extraction gate follows `AGENTS.md` ("Extract before
extending behavior" and "Measured reproduction, never speculative gameplay").

### Slice 1: Identity, source recognition and diagnostic boot

- **Outcome.** The Extractor accepts an explicit GOG path and reports a
  supported, missing, changed or unsupported source. The assetless runtime
  names the required pack, points to the Extractor, writes a local diagnostic
  log, and quits cleanly.
- **Evidence.** `BLD-GOG-EN-1.1` and its manifest; `SRC-MANUAL-1994` and
  `SRC-GAMEFAQS-81038` for credits.
- **Acceptance: rules.** Recognition is exact and deterministic. Relative paths
  are normalized, duplicates and path escapes are rejected, every required file
  has an expected length and XXH3-128, and failures carry stable diagnostic
  codes.
- **Acceptance: presentation.** Both tools give readable success and error
  output and never fail unhandled.
- **Acceptance: original content.** One manifest records the installed GOG
  revision's files, sizes, hashes and provenance. Nothing is extracted until
  the whole fingerprint passes, and nothing from the source reaches Git, build
  output, telemetry or CI logs.
- **Automated tests.** Manifest schema; matching, missing, wrong-size or hash,
  duplicate, traversal, casing and inaccessible-path sources on synthetic
  files; assetless runtime and repository-policy tests.
- **Exit.** Every row in `parity/EXE.md` whose entry lists files has Code
  `complete` or `partial` with a note, and the owned build verifies.

### Slice 2A: Complete source-corpus inventory and extraction

- **Outcome.** The Extractor walks the whole fingerprinted source tree and
  creates one verified local pack holding every source file and every resource
  record, each with a stable source identity, source hash, payload hash,
  length, contract type and conversion method.
- **Evidence.** `BLD-GOG-EN-1.1` and its manifest; `FMT-GFF-001` to
  `FMT-GFF-007`.
- **Acceptance: rules.** No gameplay rule, screen transition or inferred
  resource meaning. Decoders fail closed on malformed input, and unknown
  formats and records are kept as bounded opaque envelopes.
- **Acceptance: original content.** Every file in the installation has one
  disposition: game data to extract, executable or data evidence kept locally
  as opaque, or an excluded wrapper, document, uninstaller, save or capture.
  The staged pack is read back and verified before atomic promotion.
- **Extractor boundary.** The Extractor accepts only the fingerprinted GOG
  installation, and promotes a complete staged pack or nothing.
- **Automated tests.** Synthetic recursive inventories, duplicate and traversal
  rejection, format round trips and malformed bounds, opaque-envelope checks,
  deterministic manifest order, staged verification, stale-file replacement,
  rollback and reproducibility.
- **Exit.** The owned-install inventory shows a disposition for every file,
  no unrepresented game-data payload and no unrepresented container record.

### Slice 2B: All-region structural catalogs

- **Outcome.** The pack holds a bounded structural catalog for every region
  archive whose shared envelope validates, and the Extractor names the source
  path and failed sub-contract of any region it cannot represent. No travel,
  region selection or map presentation is added.
- **Evidence.** `FMT-REGION-001` to `FMT-REGION-006`.
- **Acceptance: rules.** No party placement, collision, interaction, travel
  edge, quest flag or timing behaviour.
- **Acceptance: original content.** The region set is discovered from the
  manifest, each catalog is written under a stable source-derived path, read
  back and promoted with the pack. The contract change raises
  `OriginalContent.RequiredAssetPackRevision`, and a test proves the earlier
  revision is rejected.
- **Automated tests.** Synthetic multi-region extraction: path order,
  duplicate identity, a malformed non-Tyr region, read-back, rollback and
  earlier-revision rejection.
- **Exit.** The owned pack holds a catalog for each of the 20 region archives.

### Slice 2: Title-to-party flow

- **Outcome.** The runtime opens the verified pack, reaches the start flow and
  creates or selects a legal four-character party.
- **Evidence.** The PARTY area; `FMT-GFF-001`; `FMT-IMAGE-001` to
  `FMT-IMAGE-004`, `RULE-IMAGE-001`, `RULE-IMAGE-002`; `FMT-TEXT-001`,
  `FMT-TEXT-002`; `FMT-UI-001` to `FMT-UI-005` and `SCR-UI-001` to
  `SCR-UI-004`.
- **Acceptance: rules.** Party size, origins, classes, ability and alignment
  limits, psionic disciplines, clerical spheres, dual-class progression and
  the initial state follow the PARTY entries. A fixed seed makes random
  generation repeatable.
- **Acceptance: presentation.** The start and party screens keep the measured
  logical coordinates, palette, control order, hit regions and Escape
  behaviour of their screen entries; scaling never changes rules or hit
  testing.
- **Acceptance: original content.** Readers bound offsets, counts, sizes,
  decompression, names and output paths.
- **Headless runner.** The protocol puts the runner in the first slice, and
  this plan adopted the protocol after Slice 2B, so it is built here. The
  runner opens a session from a fixture (pack, seed, starting state and
  commands), applies the commands without a window, and returns the events,
  the state hash and the 320x200 logical frame. Every later test that
  compares the rebuild with an owner capture or an emulated-call fixture
  drives the rebuild through it.
- **Automated tests.** Parser boundary and malformed-input tests; party
  invariants; the origin modifier table; start-flow commands, events,
  snapshots, hashes and replays; UI graph resolution and hit testing; a test
  that drives the runner from a synthetic fixture in CI, and a
  `// needs: GAME_DIR` test that drives it from a fixture naming a capture
  under `GAME_DIR/captures/` and skips when the capture is absent.
- **Exit.** Every row of `parity/PARTY.md` and the rows of `SCR-UI-001` to
  `SCR-UI-004` reach Code `complete` with Status `implemented`, or `partial`
  with a `Spec gap:` note; `Q-PARTY-001` is closed or accepted in Risks; both
  runner tests exist, and the synthetic one passes in CI.

### Slice 3: First Tyr exploration and conversation

- **Outcome.** A party enters the first Tyr area, moves and scrolls, changes
  leader and display mode, looks and interacts, completes the first
  conversation, and opens the character, inventory, effects, map and game
  menus.
- **Evidence.** The REGION, ACTOR, EXPLORE, INPUT, SCRIPT, TALK and CONFIG
  areas, and the UI screens they name.
- **Acceptance: rules.** Walking, collision, leader selection, party
  placement, interaction, dialogue choices, item transfer and the opening's
  quest flags are deterministic Core commands and events. Routes follow
  `DEV-EXPLORE-002`.
- **Acceptance: presentation.** Viewport and scrolling, cursor modes and
  hotspots, dialogue and menu layering, portraits and control states match
  their screen entries. `DEV-EXPLORE-001`, `DEV-INPUT-001`, `DEV-TALK-001` and
  `DEV-UI-001` describe where the rebuild departs.
- **Acceptance: original content.** The map, region, sprite, palette, text,
  portrait and item resources the slice uses are extracted. Unknown records
  stay unknown, and dangling references and bad bounds give contextual errors.
- **Automated tests.** Synthetic-map navigation and collision, deterministic
  command traces, dialogue branches, inventory conservation, menu routing,
  invalid references and malformed regions.
- **Exit.** Every row of `parity/REGION.md`, `parity/ACTOR.md`,
  `parity/EXPLORE.md`, `parity/INPUT.md` and `parity/TALK.md`, and the SCRIPT
  and UI rows the opening conversation and the menus use, reach Code
  `complete` with Status `implemented`, or `partial` with a `Spec gap:` note;
  `Q-EXPLORE-001` to `Q-EXPLORE-006`, `Q-ACTOR-001`, `Q-TALK-001` and
  `Q-CONFIG-001` are closed or accepted in Risks.

### Slice 4: First deterministic combat

- **Outcome.** The opening Tyr encounter can be played to victory or party
  defeat with movement, targeting, attacks, wait, guard, previous and next
  target, and end turn.
- **Evidence.** The COMBAT and AI areas, `RULE-RNG-001` and the owner's
  captures cited there.
- **Entry gate.** No combat code before the COMBAT entries it needs are
  `supported`: entry, command availability, targeting, attack resolution,
  turn order and exit. `docs/live-sessions/opening-combat.md` is the capture
  request for the parts the code does not decide.
- **Acceptance: rules.** Activation order, movement, range, target legality,
  hit and damage, Armor Class, THAC0, incapacitation, experience, difficulty,
  guard and wait, victory and defeat are deterministic from state, commands
  and an explicit random number stream.
- **Acceptance: presentation.** The combat transition, party display, target
  feedback, cursor states, animation and event order, messages and sound
  triggers match the screen entries. Frame rate never drives rules.
- **Acceptance: original content.** Missing optional presentation degrades
  explicitly; missing rule data blocks play with an actionable diagnostic.
- **Automated tests.** Golden synthetic combat traces, random number
  consumption, boundaries of hit, damage, movement, targeting and difficulty,
  victory and defeat, snapshots, and malformed combat resources.
- **Exit.** Every row of `parity/COMBAT.md` reaches Code `complete` with
  Status `implemented`, or `partial` with a `Spec gap:` note; `Q-COMBAT-001`
  to `Q-COMBAT-005` are closed or accepted in Risks.

### Slice 5: Full character systems

- **Outcome.** Players equip items, inspect statistics and effects, use spells
  and psionic powers, camp, recover, earn experience and train.
- **Evidence.** The PARTY, MAGIC and ITEM areas.
- **Acceptance: rules.** Every modifier, restriction, cost, target, duration,
  effect, recovery rule, multi-class behaviour, experience threshold and level
  gain cites its entry and has a deterministic test.
- **Acceptance: presentation.** Inventory, spell, psionic, effects, camping
  and training screens match their screen entries. Controller actions map to
  the same commands.
- **Automated tests.** Table-driven rule tests, equip and unequip
  conservation, target and duration edges, rest interruption and recovery,
  advancement invariants, seeded random effects, snapshots, parser fuzzing.
- **Exit.** Every row of `parity/MAGIC.md` and `parity/ITEM.md`, and the rest
  of `parity/PARTY.md`, reach Code `complete` with Status `implemented`, or
  `partial` with a `Spec gap:` note.

### Slice 6: Quest graph and campaign traversal

- **Outcome.** The critical route from Tyr through the artifact regions to the
  finale can be finished; optional content and alternate outcomes follow as
  separate increments.
- **Evidence.** The QUEST, SCRIPT and TALK areas.
- **Acceptance: rules.** Quest flags, dialogue prerequisites, travel edges,
  item gates, timers, triggers, outcomes, NPC survival, rewards and ending
  prerequisites are explicit deterministic state machines.
- **Acceptance: presentation.** Region transitions, dialogue and cutscene
  order, maps and feedback for gated actions match the screen entries.
- **Automated tests.** Quest-graph model tests, critical and alternate route
  replays, unreachable and duplicate state checks, timer boundaries, invalid
  and cyclic reference safety.
- **Exit.** Every row of `parity/QUEST.md` and `parity/SCRIPT.md` reaches Code
  `complete` with Status `implemented`, or `partial` with a `Spec gap:` note.

### Slice 7: Persistence, presentation, parity and packaging

- **Outcome.** Native versioned saves and deterministic replays work, the
  campaign uses extracted graphics, animation, text, cinematics, speech, sound
  and music, and clean packages install the runtime and the Extractor on each
  declared platform.
- **Evidence.** The SAVE, SOUND, VIDEO, TIME and CONFIG areas.
- **Acceptance: rules.** Save schemas and migrations are explicit, writes are
  atomic with last-valid recovery, corrupt data is rejected within bounds, and
  identical state, seed and commands give identical hashes. Presentation
  clocks never change rules.
- **Acceptance: presentation.** Every screen has a screen entry, and audio and
  video timing follows the TIME entries. Fullscreen, controller and
  accessibility options are labelled extensions.
- **Acceptance: original content.** Runtime, installers, archives, symbols,
  logs, CI artifacts and tests hold no original bytes, and the original
  installation is never modified.
- **Automated tests.** Save and replay round trips, migrations, corruption and
  atomic recovery, full synthetic campaign replay, asset referential
  integrity, audio and video scheduling, package inspection, install, upgrade,
  uninstall and smoke tests.
- **Exit.** Every row in `PARITY.md` is `validated`, or `implemented` with a
  note saying why it cannot be, and each declared package passes the
  clean-machine checks.

## Features outside the parity matrix

The launch options screen (`DEV-UI-001`), decided on 2026-09-26 in
`docs/DECISIONS.md`, opens at every launch before anything of the original's
is shown.

- **Outcome.** The screen lists each option the deviations offer with its
  current value. The player changes values with the mouse or keyboard and
  confirms, and the original's flow starts with those values. Exit quits.
  The first option is Wide map view (`DEV-EXPLORE-001`), on by default.
- **Storage.** `settings.json` in the per-user data folder beside
  `UserContent`, versioned, bounded in size, written atomically with a backup
  of the last good copy, and read field by field: a missing, unreadable or
  out-of-range value falls back to the backup and then to the default, and
  never stops the game from starting.
- **Presentation.** Drawn by the rebuild with the extracted font on the fixed
  320x200 canvas, using no original screen's layout.
- **Automated tests.** Settings round trip, defaults, backup recovery,
  corrupt, oversized and unknown-version files, a missing field in an older
  file, option navigation, confirm and exit routing, the wide map view on and
  off, and the content smoke test drawing the screen.

## Owner questions

Questions only the repository owner can answer. Research questions about the
original are in `queue/`, and runs the owner is asked to make are requests in
`docs/live-sessions/`. A question is closed by an entry in
`docs/DECISIONS.md`.

| ID | Question | Blocks | Owner | Status |
|---|---|---|---|---|
| O1 | For each conflict between the manual, the guide and the shipped game, and each original defect, should the rebuild keep it, fix it, or offer a setting? | slices 4-7 | repository owner, once the evidence is in the spec | open |
| O2 | Are loading the original's saves and importing a *Shattered Lands* party wanted once their formats are in the spec? | slice 7 | repository owner | open |
| O3 | Are Windows, Linux and macOS all first-release targets, or only Windows? | slice 7 | repository owner | open |
| O4 | May the installed clue book be read as a further local source? | slices 4-7 | repository owner | open |
| O5 | What measured tolerances make visual, input, animation and audio parity acceptable? | slices 2-7 | repository owner | open |
| O6 | Should the F9 conversation preview (`DEV-TALK-001`) stay mandatory, or become an option on the launch options screen? | slice 3 | repository owner | open |

## Risks

- **Unknown containers.** `.GFF`, `.FLI`, `.VOC`, `.BIN` and disc-image
  resources are not all described yet. `SRC-DSUN-MUSIC-79B6927` is a starting
  point where it covers a format, checked against this build. Unknown data
  stays in the opaque contract until an entry describes it.
- **Overlay code.** Much of the game's code sits in the `FBOV` overlay pack,
  which an emulated call cannot reach (`docs/RUNTIME.md`). Rules implemented
  there need a complete static reading, or an owner capture where the code
  does not decide the outcome.
- **No runs by agents.** Rule and screen rows whose evidence needs a run wait
  for owner live sessions, and stay at `implemented` until a request is
  accepted and held.
- **Storefront drift.** GOG may change files without changing the product
  name. Manifests match exactly; product and build metadata are provenance
  only.
- **Defects and conflicts.** `SRC-GAMEFAQS-81038` reports manual discrepancies,
  performance sensitivity and soft locks. Conflicts stay in the entries, and
  what the rebuild does about each is a deviation after owner question O1.
- **Extractor correctness.** A partial or stale pack could mix revisions: the
  versioned manifest, exact inventory and hashes, staging, read-back, atomic
  promotion and rollback guard against it.
- **Large state space.** Quest branches and NPC survival are combinatorial:
  explicit state machines, model-based tests, checkpoints and replays.
- **Content leakage.** Denied extensions come from the real inventory, captures
  stay outside tracked paths, and repository verification runs on every
  change.
- **Cross-platform variance.** Internal paths are canonical, and platform
  input, audio and file-system behaviour sit behind tested adapters.

## Done when

All seven slices have met their exits; the Extractor recognizes a licensed,
fingerprinted GOG copy and produces a complete verified local pack; the
runtime alone plays that pack through a finishable campaign with
deterministic saves and replays; `README.md`, `docs/SOURCE-EDITIONS.md`, the
spec, `deviations/`, `PARITY.md` and `parity/` match what is true; the owner
questions are closed or recorded as non-goals; the solution and smoke test
build; the repository checks and tests pass without proprietary content; and
each declared package passes clean-machine install, extract, launch, save,
reopen and uninstall checks.


## Tooling maintenance: merged v1 evidence workflow

Adopt template PRs 19, 21 and 22 from `e698e5b` and the merged toolkit 10/11
checker while retaining Standard v1. The owner requested adoption, resolved-gap
cleanup and push. This is a tooling batch; no game behavior, evidence status,
licensed-source identity or owner-only runtime policy changes.

- Outcome: bounded relocation, flow, table and inventory tools; conditional
  evidence review; verified offline authority/checker; rules in Core guidance.
- Evidence: the merged template and toolkit source, their MIT licenses, and the
  accumulated tooling requests in `gaps.md`. Validation uses synthetic inputs.
- Preserve the established FBOV mapped-image conversion and existing coverage
  paths. Its specialized join handles mapped overlay coordinates that the generic
  join does not accept directly. Adopt the safer transactional exporter.
- Store immutable rule text under `vendor/upstream/` because project-authored
  docs are citation-checked. Verify identical upstream digests and matching CI pin;
  configuration must preserve all vendor bytes. Keep the game-specific test gate.
- Acceptance: `tools/Test.ps1`, solution build, synthetic evidence and snapshot
  tests pass without original content; CI runs the same Node suites and checker.
  No claim is promoted. Conditional review guidance is not an automated effect
  analyzer. Verify each complete original request before removing it; grouping
  requests under ten priorities or adding review guidance does not establish
  individual completion. Retain unmet and partially addressed requests.
- Exit: commit the adoption and resolved-gap cleanup, audit processes, write the
  handover separately and push. No owner question or live session is needed.


## Tooling maintenance: template PR 17 local rules

Adopt merged template PR 17 (`18a67f3`): add the exact methodology snapshot to
`vendor/upstream/`, make the pinned local pages the task authority, and replace
published-page links with local section links carrying checked line ranges.
Refresh/freshness checks run only at the owner's request. Preserve the configured
project identity, owner-only native runs, the absent/limited emulator harness,
source identities and the corrected individual gap tracking.

Acceptance: snapshot digests, heading/range checks, synthetic link tests and
`tools/Test.ps1` pass. The link checker runs in CI and detects missing/stale
ranges and external rule-page links. Shorten canonical summaries by referring
to the local sections, retaining local runtime exceptions and blank entry forms.
Exit: commit the tooling adoption, update handover separately and push main.


## Tooling maintenance: template PR 24 restored rules

Adopt PR 24 at `9349a89280de5f5f86a8f1b803d43cb1ad7575d0`: restore the
detailed planning, research, fidelity and entry-template guidance, and pin the
exact Standard v1 and Protocol snapshots supplied by that commit. This supersedes
PR 17's instruction to shorten summaries. Keep local snapshots in `vendor/`,
configured identity, existing coverage paths and owner-only native-run limits.

- Outcome: research instructions remain fully available offline, including
  complete-reading and emulated-call acceptance criteria. No gameplay changes.
- Evidence: PR 24's source files, lock digests and retained upstream licenses.
- Acceptance: snapshot verification, local section links, synthetic Node tests,
  `tools/Test.ps1` and solution build pass without reading original content.
  Remove only gaps whose entire request is satisfied; record closure evidence
  in `docs/TEMPLATE-ADOPTION.md`. Reporter requests remain open when only
  guidance was restored. No spec or parity status changes.
- Risks: generic run permissions must not override the local prohibition, and
  upstream example IDs must stay outside the project documentation scan.
- Exit: review the adapted diff and passing checks, commit the tooling batch,
  audit processes and update the handover separately. No owner questions.


## Tooling maintenance: latest template and PR 26

Adopt template main `8ed674008cd9ce2860b19cd6ac6a569d70d2bc03`, including
PRs 25 and 26: refresh exact local rules and the checker, align CI, and update
research/implementation procedures for code ranges, hardware boundaries,
checkpoint contracts and recorded draws. Preserve owner-only DOSBox access:
Probe remains none, and no recorded native session or RNG hook is claimed.

- Outcome: current offline research rules and precise mapped-range checks.
- Evidence: pinned template snapshots and existing executable-layout findings.
- Acceptance: lock verification, section links, synthetic Node tests,
  `tools/Test.ps1` and solution build pass. Populate required Code ranges
  from the existing bounded overlay-header procedure, retaining each overlay
  boundary; do not infer code from a coverage inventory's function sizes.
  Gap closure audits each full request, retaining unimplemented reporters.
- Risks: newer Survey requirements may expose unreconciled installation paths;
  record missing listing evidence rather than claiming an exhaustive inventory.
  Recorded-run guidance grants no native-process access under the local policy.
- Exit: tested tooling commit and separate current handover; no gameplay or
  evidence-status changes.


## Tooling maintenance: merged bounded reporters

Adopt template PR 27 at `3e8805ea474c60e7c3234213a108cb85a9e86265`, toolkit
PR 14 at `c2b21ee62fc404391e8dcfafd7029185f81241a9`, and standards PR 26
at `94f8f678afb05171567f48d9fb19488e48309f12`, as the owner requested.

- Outcome: ten instruction-derived research reporters with exact source pins,
  explicit unsupported paths and offline integrity checks. Preserve configured
  identity, existing coverage joins, XXH3 source identities and owner-only runs.
- Evidence: merged source, retained MIT licenses and individual gap requests.
  No original program is launched and no gameplay or evidence status changes.
- Acceptance: reporter pin and configuration-preservation regressions, Python
  and Node synthetic suites, `tools/Test.ps1` and full solution build pass.
  Both CI and release validation install the pinned Python dependency. Audit
  each gap against its own case: upstream synthetic examples alone do not
  close a game-specific request. Verify historical rule replacement directly
  with the adopted checker before closing gap 28.
- Risks: bounded x86 support is not a complete reading; string operations,
  indirect targets and hardware boundaries can leave requested cases open.
  Keep reports/configurations in GAME_DIR. Pinned documentation must be outside
  the project-authored citation scan while retaining its exact upstream bytes.
- Exit: reviewed tooling commit, process audit and separate handover commit.


## Tooling verification: Dark Sun reporter cases

The owner requests verification of partially implemented gaps against their
recorded game cases, and upstream PRs for reproducible remaining reporter gaps.
This is tooling on the research side of the clean room, not gameplay work.

- Outcome: a per-request acceptance audit, removal of fully verified gaps, and
  synthetic upstream regressions/fixes for reporter failures encountered.
- Evidence: the existing findings named by gaps 13, 14, 18, 21, 26, 27, 32,
  35, 36 and 42, plus related operand/segment/cleanup cases where applicable.
  Use the documented BLD-GOG-EN-1.1 executable and declared MZ/FBOV mappings.
- Acceptance: compare actual reports with established entry hits, canonical
  call controls, consumed widths, path order and missing-producer conditions.
  Unsupported paths are explicit unmet cases, never a negative finding. Keep
  raw reports/configurations in GAME_DIR; commit only an own-words audit and
  synthetic tooling tests. New native reachability/status claims are excluded.
- Upstream fixes are game-agnostic and tested with entirely constructed bytes.
  Plan changes before implementation in each target and run its canonical gate.
  PR descriptions identify the concrete defect, its fix and remaining scope.
- Exit: full local gate, reviewable tooling commit and separate handover; open
  requested upstream PRs for necessary refinements. Preserve owner-only runs,
  configured identity and existing licensed-source fingerprints.


## Adopt revised upstream reporter, template and rules

Tooling maintenance, 2026-09-30. Adopt website 3b4e6fcfca887620cdf13c8a8e62f9ca53133d60, toolkit 926e287a4134512d59fe021efe6507c933da03f1 and template b9f542549840cf7ce2d254a8f9f7bf0f502daaa7. Outcome: the configured project uses the latest reviewed bounded reporters and local rules, including PE32 support and separated conditional operand observations. Dark Sun stays on its evidenced MZ/FBOV mapping; no gameplay or evidence status changes.

Acceptance: exact upstream source/test/guide/license digests, updated checker/CI pins and section ranges; template adoption script includes PE files and the canonical gate discovers both Python suites. Retain project-specific inventory paths and owner-only runtime constraints. Rerun the recorded Dark Sun controls with the adopted source, close only passing requests, and preserve explicit unresolved callee/path conditions. Synthetic PE and segmented regressions, configuration preservation and the full tools/Test.ps1 gate prove adoption. Exit: all pins verify, gate/build pass, case outcomes and remaining gaps are documented. Risks: shared instruction-boundary and conditional-report changes may alter old report consumers; update the local verification script to the reviewed report schema. No owner questions.


## Full upstream gap resolution

The goal in docs/goals/upstream-gap-resolution.md owns the remaining shared-tool requests. Outcome: all numbered intake requests are delivered and individually verified, with related upstream deficiencies fixed through reviewable PRs and adopted revisions. Existing findings provide source identity, locations, widths, branch contracts and expected controls; no new game claims or behavior are implemented. Acceptance includes complete request coverage, negative controls, explicit remaining assumptions, no proprietary committed fixtures, exact upstream pins, canonical tests and a per-request closure record. New missing tooling formats/readers get a specific planned batch before implementation. First checks replay the evidenced two-entry table and relocated pushed pointers; next verify the actual inventories before expanding instruction/string/loop support. Automated tests are constructed source cases upstream plus local owned-source acceptance outside Git. Exit is the goal's full ledger proven closed, not a number of green generic tests. Risk: incomplete static summaries must not be reported as native execution or whole-program proofs. No owner decisions are needed for shared tooling.


## Bounded memory maps and JVM diagnostics

Tooling batch, 2026-09-30. Outcome: a researcher can select a bounded page or exact named block from a large analyzed map without exporting the entire map, and JVM crash/replay diagnostics stay local even if someone force-stages an ignored file. Evidence: the configured Dark Sun map has 3,546 blocks and the old reporter fails before filtering; crash logs can contain local memory and environment data. Acceptance: page offset/limit are explicit, output never exceeds 512 rows, exact-name selection diagnoses absence/ambiguity, headers state total/selected/emitted and partial scope; default still rejects an oversized whole-map request. Ignore and policy rules cover diagnostic basenames at any depth; synthetic scratch Git tests prove ordinary logs remain permitted, ignored diagnostics are omitted and forcibly staged diagnostics are rejected. Java tests use constructed maps; compile against the public installed Ghidra API and verify the recorded large map when available. No original bytes or reports committed. Exit: canonical fast gate, bounded-map cases and policy regressions pass; document upstream provenance and local-only storage. No owner questions.


## Verify committed function inventories

Tooling batch. Outcome: shared tooling validates a committed coverage TSV against the selected build/manifest identity, mapped source ranges and portable destination, including documented configured-project legacy paths. Existing export/join preserves only start/size; committed format may additionally contain researcher-authored name and out_of_scope reason columns. Acceptance: require start/size plus only those optional columns; reject empty/duplicate/aliased starts, invalid sizes, mismatched manifest prefixes, unmapped source offsets, oversized body counts, wrong destination and undocumentated legacy paths. An explicit legacyPath requires evidence and must be a safe repository-relative .tsv path; it never changes portable generation. CLI inventory-check reads bounded TSV data and uses the hash-guarded MZ/FBOV parser. Synthetic cases prove corruption/mapping/identity failures and optional columns. Actual Dark Sun shared export must reproduce the retained mapped export; joined output matches 2,153 rows, and the installed plus separately owned disc inventories pass with their own sources and documented CD path. No original contents or rich exports in Git. Exit: all controls, canonical gates, and source identity checks pass; upstream PR tracks shared delivery.


## Latest template maintenance adoption

Adopt template c048c63523a1b061b5325f6b05055d98819780d7, including PR 31's authorized-work and durable-narrative policy and PR 32's revised bounded-map/JVM diagnostics changes. Outcome: policy-driven filename denials include heap dumps; bounded map headers retain requested counts and ambiguity diagnostics give capped matching indices. Preserve configured source identity, local content restrictions and owner-only runtime. Acceptance: exact upstream tests and adapted script pass, actual mapped-project page/name controls still pass, repository policy reads deniedFileNamePatterns, canonical gate and full build pass. Update guidance and adoption provenance without rewriting historical evidence. Exit: verified current template delta and locally completed requests 2/38 close.


## Bounded string effects and saved flags

Tooling batch: support direction-sensitive MOVS/STOS/LODS in segmented and flat instruction reports, including REP with concrete bounded counts. Acceptance: source segment overrides and fixed ES destinations remain distinct; pointer increments/decrements wrap at selected address width; zero repetitions touch no memory; unknown directions split explicit conditional cases and retain one producer identity; unknown counts, address-size overrides, unsupported repeat forms and exhausted iteration budgets stop with named gaps. Report every read/write and direction assumption, including unknown alias invalidation; saved PUSHF/POPF must restore only an intact locally saved word's arithmetic/direction provenance, otherwise expose unknown flags. Unknown call models invalidate direction/interrupt assumptions along with other flags. Synthetic tests cover forward/backward and overlapping copies, zero/unknown/large counts, prefixed width, segment overrides, flags restoration/corruption and independent flag producers. The actual FND-CONFIG-154 prefix must reproduce the stated forward writes under an explicit starting hypothesis and keep incoming direction unknown by default. No proprietary fixtures, native execution or original-derived implementation. IRET/internal overlapping-frame support is a subsequent explicit acceptance step, so gap 25 stays partial until that helper case passes. Exit: toolkit canonical checks and source-case controls pass; reviewed source/guide/tests propagate by exact pin through an upstream template PR.

## Latest template inventory refinement adoption

Adopt template e0325e0b063735e94b7e3ac94b0b8b89d0a38a79 (merged PR 33), preserving configured identity, evidence and existing unfinished string-tooling plans. Outcome: inventory validation rejects noncanonical starts and analyzer default names, and reads the file at its declared repository path. Acceptance: exact merged source/tests, actual installed inventory rerun, canonical gate and full build. Exit: merged inventory capability adopted; close gap 1 only, retain gap 5 pending distinct disc-source verification.

## Explicit overlapping paths and local IRET frames

Tooling batch. Outcome: a direct verified control-flow edge can establish an alternate instruction start inside another reached instruction; raw scan hits or independently asserted conflicting entries cannot. Keep edge provenance and independently decode both continuations. A proving edge must itself have an unconflicted boundary. Synthetic incoming/use controls retain rejection of operand-byte false calls. In segmented16, permit IRET only for a traced local push-CS/near-call frame above an intact locally saved FLAGS word at frame creation; check return IP, CS and stack balance, then consume FLAGS with normal snapshot/corruption semantics. Reject root/external, flat32, prefixed, missing or overwritten return frames. No interrupts, privilege or hardware simulation. Tests cover explicit overlap acceptance, false boundary rejection, saved caller DF restoration, corruption and invalid frames. Actual FND-CONFIG-155 must report its complete local writes and restored incoming direction under explicit nonaliasing stack hypotheses. FND-CONFIG-154 remains conditional. Exit: gates and source controls pass; upstream PRs and exact template pin track delivery without promoting game claims.


## Instruction-owned segment operand provenance

Tooling batch. Outcome: an operand query verifies the selected segment word belongs to a reached instruction's 16-bit immediate, retaining instruction/operand locations, raw representation, destination kind and declared MZ/FBOV membership. Decode from established entries, never bless a stripped prefix or data candidate. Known relocations report mapped segment/descriptor and supplied field offset; undeclared words stay raw/unresolved. Reject mismatched/partial/wrong-width immediates and query starts outside the verified entry path. Report walk gaps and native uncertainty independently of the selected operand result. Synthetic register/store/push and boundary/width/provenance cases, hash-guarded CLI controls, and actual resident/overlay/stored/pushed cases prove acceptance. No native execution or proprietary fixtures. Exit: toolkit gates and exact template adoption candidate pass; gap 16 closes only after reviewed upstream delivery and local adoption.

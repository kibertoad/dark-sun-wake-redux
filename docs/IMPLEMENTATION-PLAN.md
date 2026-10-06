# Implementation plan

## Issue 5 nonzero region-count caller producers

Use FND-CONFIG-173/175/176 as read-only inputs. Find actual incoming calls to the
count-one setup from declared inventory batches, preserving exclusions and
partial/contested starts. Follow caller-supplied destination and source address
producers with the real fill/copy/normalizer at unchanged bounds. No supplied
memory, fixed target model, stitched state or native geometry admission. Check
count-one provenance only where an actual connected caller reaches its store;
wrong producer, omitted setup and cap controls must reject or lose anchors.
Exit: actual input/count witnesses and remaining alias/storage/route limits.
Reports/selectors remain local under GAME_DIR; whole Gap 34 stays open until
known append cardinality and pair/split/copy/terminator exits all pass.

## Resident-only emulated-call evidence tooling

Build the permitted Unicorn harness under tools/emu using the published MZ
reader for hash, bounds and source relocations. Load only resident code; deny
overlay transfers, undeclared execution, unsupported interrupts/ports, modified
code and step/write limits. Execute one declared function and record numeric
instruction/branch coverage, registers and bounded memory effects, without a
game process, DOSBox or pixels. Hash-locked test-only Unicorn runs synthetic CI
controls without licensed content.
Original reports/configs stay in GAME_DIR; never write a prepared image to Git.
Verify the FND-CONFIG-193 no-argument initializer without seeded memory fields,
comparing loaded high bytes and source-written roots.
Seeding waits for supported fields/parameters. Synthetic relocation/return,
boundary, port, interrupt and limit
controls plus full Test.ps1 must pass. Exit: working resident-call capability
and conditional evidence. Full five-gap exits still require caller/input
admission; existing game findings stay read-only.

Completed bounded adoption plans: [archive](implementation-plans/COMPLETED-SHARED-ADOPTION.md).
## Issue 5 callback writer candidate census

Search the committed DSUN function inventory for encoded A119/A11B operands
with the published operand-candidates reporter. Derive resident/source-container
mappings through the shared MZ/FBOV loader; overlay coordinates are analysis
views, not admitted runtime segments. Batch declared function entries within the
reporter's limits. Keep candidate classifications, overlap, partial search and
boundary gaps explicit. Validate known callback reads as positive controls and
reject wrong-site controls. Exit: local writer leads and their entry ownership
for bounded follow-up, never a universal absence or semantic producer claim.
Keep original-derived selectors and reports under GAME_DIR only.
Follow the writer input with a bounded actual-entry trace and scan declared
inventory batches for incoming calls and relocated pointer pairs. Preserve
partial/contested coverage;
unknown stacked input is provenance, never an admitted callback target.
Wrong producer, cap and omitted writer controls must reject or lose anchors.

## Issue 5 actual middle-service dependencies

Use FND-CONFIG-171/174/188/189/191/197 as read-only inputs. Extend the existing actual
bracket query with the complete 409B:1675 and 3D72:0B84/0942 bodies, without
new call models, input memory or increased query bounds. Preserve callback
origin/order controls and report which real service entries and unread child
calls are reached. Omitted middle-service and one-step controls must remove
those witnesses. Exit: record actual connected dependency coverage and remaining
producer/storage/guard gaps; local entries never establish whole callback routes.
If earlier caller stops prevent entry, separately test the documented middle
entry with unknown input state; do not import caller memory or claim a join.
Extend with the documented refresh and handle-forwarding bodies. Verify actual
temporary-field argument producers on reached writes, with a wrong producer,
omitted wrapper and one-step negatives. Separate explicit restoration from
whole preservation; unknown aliases, targets and external services remain open.

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

Completed template and bounded-reporter adoption plans are retained in
[the adoption archive](implementation-plans/COMPLETED-SHARED-ADOPTION.md).

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

## Bounded string effects and saved flags

Tooling batch: support direction-sensitive MOVS/STOS/LODS in segmented and flat instruction reports, including REP with concrete bounded counts. Acceptance: source segment overrides and fixed ES destinations remain distinct; pointer increments/decrements wrap at selected address width; zero repetitions touch no memory; unknown directions split explicit conditional cases and retain one producer identity; unknown counts, address-size overrides, unsupported repeat forms and exhausted iteration budgets stop with named gaps. Report every read/write and direction assumption, including unknown alias invalidation; saved PUSHF/POPF must restore only an intact locally saved word's arithmetic/direction provenance, otherwise expose unknown flags. Unknown call models invalidate direction/interrupt assumptions along with other flags. Synthetic tests cover forward/backward and overlapping copies, zero/unknown/large counts, prefixed width, segment overrides, flags restoration/corruption and independent flag producers. The actual FND-CONFIG-154 prefix must reproduce the stated forward writes under an explicit starting hypothesis and keep incoming direction unknown by default. No proprietary fixtures, native execution or original-derived implementation. IRET/internal overlapping-frame support is a subsequent explicit acceptance step, so gap 25 stays partial until that helper case passes. Exit: toolkit canonical checks and source-case controls pass; reviewed source/guide/tests propagate by exact pin through an upstream template PR.

## Latest template inventory refinement adoption

Adopt template e0325e0b063735e94b7e3ac94b0b8b89d0a38a79 (merged PR 33), preserving configured identity, evidence and existing unfinished string-tooling plans. Outcome: inventory validation rejects noncanonical starts and analyzer default names, and reads the file at its declared repository path. Acceptance: exact merged source/tests, actual installed inventory rerun, canonical gate and full build. Exit: merged inventory capability adopted; close gap 1 only, retain gap 5 pending distinct disc-source verification.

## Explicit overlapping paths and local IRET frames

Tooling batch. Outcome: a direct verified control-flow edge can establish an alternate instruction start inside another reached instruction; raw scan hits or independently asserted conflicting entries cannot. Keep edge provenance and independently decode both continuations. A proving edge must itself have an unconflicted boundary. Synthetic incoming/use controls retain rejection of operand-byte false calls. In segmented16, permit IRET only for a traced local push-CS/near-call frame above an intact locally saved FLAGS word at frame creation; check return IP, CS and stack balance, then consume FLAGS with normal snapshot/corruption semantics. Reject root/external, flat32, prefixed, missing or overwritten return frames. No interrupts, privilege or hardware simulation. Tests cover explicit overlap acceptance, false boundary rejection, saved caller DF restoration, corruption and invalid frames. Actual FND-CONFIG-155 must report its complete local writes and restored incoming direction under explicit nonaliasing stack hypotheses. FND-CONFIG-154 remains conditional. Exit: gates and source controls pass; upstream PRs and exact template pin track delivery without promoting game claims.

## Instruction-owned segment operand provenance

Tooling batch. Outcome: an operand query verifies the selected segment word belongs to a reached instruction's 16-bit immediate, retaining instruction/operand locations, raw representation, destination kind and declared MZ/FBOV membership. Decode from established entries, never bless a stripped prefix or data candidate. Known relocations report mapped segment/descriptor and supplied field offset; undeclared words stay raw/unresolved. Reject mismatched/partial/wrong-width immediates and query starts outside the verified entry path. Report walk gaps and native uncertainty independently of the selected operand result. Synthetic register/store/push and boundary/width/provenance cases, hash-guarded CLI controls, and actual resident/overlay/stored/pushed cases prove acceptance. No native execution or proprietary fixtures. Exit: toolkit gates and exact template adoption candidate pass; gap 16 closes only after reviewed upstream delivery and local adoption.

## Full upstream migration, 2026-10-01

Adopt website 82deb767ab64ca9922bb6347d66d9856b7640e91, toolkit 7da1b93cdd9ac0d59dbaf82b66b4db95d578ab9d and template 7b3bbe46b251b163ee02a6539ac0d81559dbe921. Outcome: source-derived operand/string/flag reporters, contested overlap reachability, executable file-data contracts, research tracking and isolated window capture are available with the configured identity and owner-only runtime preserved. Acceptance: exact source/checker/reporter pins, valid section ranges, active questions tracked without speculative answers, synthetic capture/tracking regressions, actual reporter controls, canonical gate and full build. No proprietary content or gameplay changes. Exit: complete reviewed delta, passing checks and push to main.

## Migration acceptance audit: CI coverage

Ensure CI runs every adopted Python reporter suite and the same synthetic upstream/evidence checks as Test.ps1, including research tracking and Windows capture. Run the tracking check as a required step. Use the adopted local repository checker so CI enforces configured diagnostic-filename denials rather than the older remote action. Acceptance: canonical gate passes, assetless Release publish starts and exits via smoke-test, CI command coverage includes all migrated regressions, and exact pin checks remain valid. Preserve platform-specific skips and owner-only original runtime. Exit: audited workflow and local acceptance pass.

## Relevant golden-template infrastructure acceptance

Tooling maintenance, authorized by the owner's request to address the remaining
relevant migration gaps. Evidence is the capability audit in
`docs/TEMPLATE-ACCEPTANCE.md` and golden template
`7b3bbe46b251b163ee02a6539ac0d81559dbe921`; no gameplay or evidence-status change.

- Outcome: maintainers get enforced bootstrap facts, reproducible dependencies,
  one serialized validation entry point, optional verified signing and a root
  launcher that forwards arguments and permits content-free smoke checks.
- Bootstrap: add the template gate and synthetic missing-fact, false-status,
  mismatched-version and configured-no-rewrite controls. Preserve known version
  1.1; do not promote "no later patch recorded" into established patch status or
  invent SHA-256/provenance. An unestablished status blocks executable analysis,
  not original-free builds. Record any unresolved provenance explicitly.
- Dependencies and validation: generate project dependency locks and enforce
  locked restore. Integrate an adapted infrastructure gate into Test.ps1 and
  CI. Serialize validation with a checkout-specific lock, run the existing
  original-free gate, full Release build and assetless publish/smoke, and retain
  optional test filters/count controls without bypassing policy checks.
- Signing: adopt pinned template signing adapters and opt-in release workflow
  support, fail closed on missing configuration, wrong/expired/revoked keys,
  silent signer refusals and invalid Authenticode. Synthetic process doubles
  exercise contracts without real credentials or service calls. Existing
  unsigned release defaults remain; no release, upload or signing is executed.
  Compare signing with New Chrome main
  `88f4112791db0caaf488b5bcd906cabd842bd4e1`: adopt main-only preparation,
  bounded jobs, exact project-executable selection, verified installed binaries
  and idempotent tag reuse for the same commit. Keep this project's explicit
  version/platform inputs and template download/error-handling improvements.
- Launcher: retain play.bat and licensed-source refresh for ordinary play; check
  dotnet availability, forward arguments and bypass extraction for assetless
  and platform smoke. Synthetic command doubles prove error propagation,
  whitespace paths, forwarding and no original-source access in smoke modes.
- Analysis: adopt relevant guarded generic Ghidra exporters with local-only
  destination guards and public-API compilation acceptance. Exclude PE-only
  Inspect/citation helpers from this DOS MZ/FBOV project. Map additional bounded
  helpers to existing reporters or adopt useful generic equivalents, preserving
  existing overlay joins and neutral names. No original is opened or run.
- Risks: configuration must preserve additional original fields on reconfigure;
  filtered validation must not imply the whole gate ran; signing remains pending
  real owner-configured release acceptance; exporter adoption cannot validate
  game-specific address mappings; native mixed-DPI capture remains owner-only.
- Exit: adapted infrastructure gate, synthetic negative controls, canonical
  Test.ps1, locked full build and assetless publish/smoke pass; every audit row
  has an explicit disposition and handover names any evidence-only remainder.
  No new owner decision is needed for tooling; release/live-session decisions
  stay with the owner.

## Explicit offline validation rerun

Tooling migration for gap 4. Add -NoRestore to Test.ps1 and Invoke-Validation.ps1
so an explicitly requested rerun reuses dependencies already restored for the
same checkout and artifact paths. Normal validation and CI still restore. Keep
policy, configuration, infrastructure, Python/Node gates, locked dependency
settings, test filters/counts, Release build and assetless publish/smoke intact.
No synthetic success substitutes for a full local NoRestore run after normal
validation. Missing restore state fails; callers must restore again after dependency input changes. No automatic restore fallback
is allowed. Tests verify default/offline argument forwarding and failure
propagation with command doubles, plus the real configured full gate.
Exit: shared template PR, local normal/offline gates and exact pins pass; gap 4
closes only after reviewed upstream delivery. No owner questions or persisted
layout changes.

## Final merged ownership reporter adoption

Adopt reviewed toolkit PRs 35?37 through template PR 42: exact source/test/guide
pin, source-derived exports, reached owner/analyzer ranges and explicit boundary
checks. Preserve configured routing and remove the superseded PYTHONPATH override.
Acceptance: synthetic malformed-metadata and incomplete/contested/entry-limited
join controls, installed-source setup/handler and caller/pointer controls, exact
hashes, normal and offline canonical gates. Conditional reachability is tooling
evidence only; no game claims change. Exit: reviewed adoption and whole-contract
Gap 11 record. No persisted format or owner decision changes.

## Published toolkit package migration

Merged toolkit PR 42 moves shared executable reading, instruction analysis,
documentation checking and Ghidra scripts into released packages. The configured
restoration must migrate its tooling consumers together: exact npm/Python locks,
package imports/CLI entry points, CI and validation, hooks, updater/checker
configuration and local tooling guides. Keep game-specific wrappers, evidence,
source mappings and configured policy; remove copied shared modules, tests and
obsolete sync locks only after equivalent package behavior is verified. Local
standard snapshots remain pinned and unchanged. No src/ or game behavior changes
belong to this tooling batch.

Acceptance: a registry-delivered version of every required package is available,
installed with reproducible locks, and passes prepared-protocol, source hash,
existing game-case controls, documentation/policy and full canonical normal and
NoRestore validation. Synthetic checks cover actionable missing-engine/protocol
errors and package entry-point routing without reproducing shared tests. A
source-build wheel or placeholder version alone is not adopted delivery. The registry now delivers engine 0.1.0 and npm reader/checker 0.1.0; these
bootstrap npm releases are adopted by exact archive locks, not source builds. Exit:
reviewed template migration, exact published dependencies, full source/control
acceptance and canonical gates; no vendored competing implementation remains.

## Locked package tooling bootstrap

Tooling outcome: install exact npm dependencies from the repository lock and the engine/Capstone wheel hashes from Python requirements. The default Python environment lives under artifacts/evidence-python (Scripts/python.exe on Windows, bin/python elsewhere); EVIDENCE_PYTHON can select an explicitly supplied interpreter. Normal validation restores those dependencies; NoRestore checks existing dependencies and never installs or falls back to network. Standalone wrappers select that same project environment and give an actionable setup command when it is missing. Existing runtime/source-specific Ghidra wrappers combine the packaged shared scripts with retained project-specific scripts. Acceptance: locked clean normal restore, offline rerun, missing/mismatched engine and prepared-protocol controls, wrapper routing and configured source/control regressions pass; published archives contain the required modules/scripts. No gameplay, source manifest or rule snapshot refresh. Exit: canonical gates and reviewable template migration; no copied competing shared implementation remains.

## Ordered effect paths and restoration witnesses

Tooling outcome: existing effects reports summarize each bounded return or stopped
path as an ordered timeline of writes, local/traced calls, assumed service
returns, branch predicates and hardware boundaries. Preserve entry/depth/order,
child writes and the last recorded flag producer; a later common return never
merges distinct path contracts. Summaries name writes already made before each
unread/conditional service, retain unknown child effects separately, and never
infer rollback or transactionality from a result encoding. Explicit restoration
witnesses must demonstrate matching pre-call read and post-call write storage,
width and value provenance on a completed path, with intervening unknown effects
remaining visible; partial restoration never becomes a transactional claim.

Synthetic acceptance: early bypass vs shared return, mutations before service
failure, traced child mutation, unrelated predicate producers, per-path snapshot
restore/bypass, different segment/width/value rejection, incomplete paths and
nonvacuous controls. A real prepared-reader integration case verifies delivery.
No proprietary data or game constants in this repository. Source request acceptance
stays in the restoration: verify every cited case and preserve pending contracts.
Exit: canonical package gates and local source controls pass; publish a reviewed
candidate. Request closure requires merged registry delivery and complete adopted
source reruns. Several slices may be required; no first slice narrows that exit.

## Merged call-order release adoption

Adopt toolkit PR 41 merge e1628488 and registry engine 0.2.0 by exact wheel hash.
The already published npm reader forwards the command without a reader upgrade;
no source-built substitute or rule snapshot refresh. Acceptance: the recorded
caller sequences/shared guards, cleanup bytes, flat inventory and unresolved
callee effects pass; false-alternative and all nonvacuous cap controls reject.
Configured and template normal/offline gates pass. Existing source controls stay
passing. Exit: exact installed delivery and whole Gap 15 contract rerun; remove
only Gap 15 after those gates, retaining other stable IDs and requests.

## Merged return-flow release adoption

Adopt toolkit PR 43 merge 23bf64e3 through registry engine 0.3.0 by exact wheel
hash. This supersedes the intermediate engine 0.2.0 lock and retains PR 41's
call-order delivery. Acceptance: configured initializer, unsigned-reader,
conditional wrapper and signed-dimension result-width/encoding controls and
rejected false encoding/out-of-width/capped controls pass. Keep conditional
callee models and stopped paths explicit. Existing source controls and Gap 15
controls remain passing; configured/template normal and offline gates pass.
Exit: complete cited contracts verified on installed packages; only then close
proven Gap 15/26 contracts. No gameplay or rule snapshot changes.

## Merged effect-ordering release adoption

Adopt toolkit PR 45 merge eab782d7 through published engine 0.4.0 by exact
wheel hash. It retains the merged call-order and return-flow commands while
adding bounded effect timelines and qualified local restoration witnesses.
Acceptance: normal and unreachable-proxy NoRestore canonical gates pass;
installed-package call-order, return-flow and existing source controls pass.
Verify the SCRIPT wrapper early-exit control through installed packages; stopped
or capped source paths remain incomplete and cannot close Gap 27. No gameplay,
research claims or rule snapshot changes. Exit: reproducible registry delivery,
reviewable template lock update and recorded controls; full unresolved request
contracts remain open until every cited case is covered.

## Resource-result and last-predicate effect control

Tooling acceptance for Gap 27 uses the recorded FND-CONFIG-179 resource suffix
as a conditional starting-state slice, not a complete function or native run.
The installed reporter must retain service results, unused AX flag producers,
subsequent field comparisons, actual branch producers and write prefixes.
Resource and later primitive models assume balanced returning calls and explicitly
preserved DS/SS/BP only; memory and flags remain unknown. Synthetic start state,
stopped suffix exits and incomplete parent/callee coverage stay visible.
Acceptance: positive source timelines show the field comparison replaces the
AX OR flags; nonvacuous trace/path limits destroy that positive control. A
nonzero third pointer route skips the mode test and final local clear. No
resource acceptance, pointer assignment by an unread callee or successful
rollback is inferred. Configs/reports stay in GAME_DIR. Exit: recorded installed
package controls; Gap 27 remains open until every cited source contract passes.

## Conditional table-target effect continuations

Gap 27's linked-child case is stopped by an evidenced computed jump even though
CFG discovery retains its source target table. Plan shared tooling work without
new instruction semantics: retain the original unresolved path, and add separate
conditional continuation paths for declared near-word targets. Preserve prefix
writes, stack/child state and target-selection/table-content assumptions. A
return on one conditional route proves neither selection nor whole-call coverage.
Reject contested/overlapping target starts and contradictory concrete operands;
partial tables retain missing routes. Existing path/step/visit/total budgets apply
to all continuations. Inputs and prepared protocol remain unchanged; no guessed
selector, table contents or game-specific dispatch enters the engine.
Acceptance: synthetic prefix mutation and child-result exits, duplicate targets,
partial declarations, concrete mismatch, overlapping targets, loops and nonvacuous
budgets plus a real prepared-reader synthetic case pass. Local full-entry source
acceptance must retain unread inputs and native reachability. Exit: upstream PR,
merged registry delivery and the complete cited restoration controls; a candidate
alone cannot close Gap 27. No gameplay or spec claim changes.

## Published reader 0.2.0 adoption

Adopt the registry executable-reader 0.2.0 archive from toolkit release d938689
with an exact npm lock. Engine 0.4.0 and checker 0.1.0 remain pinned. The release
includes merged command help and prepared-reader integration; do not adopt the
unmerged conditional-table candidate. Acceptance: normal and unavailable-proxy
NoRestore canonical gates in configured and template checkouts, version/missing-
dependency controls and all retained installed-package source drivers pass.
Configs/reports stay in GAME_DIR; no candidate PYTHONPATH is used. Exit: validated
locks, reviewable template PR update and source regression record. No gameplay,
rule snapshot or persisted content-contract change.

## Returning pointer-consumer caller control

Gap 27 acceptance uses FND-CONFIG-161/168's complete local outer-caller entry
and installed reader 0.2.0/engine 0.4.0. Explicit balanced-return hypotheses
retain each of the three consumer results as word zero or FFFF, followed by
its independently addressed dword field clear. Verify the zero selector argument,
field width/segment, returned result and absence of an intervening result branch.
Other service results, polling return assumptions and DS/SS/BP preservation stay
conditional; all modeled memory/flag effects remain unknown. Full-function caps
or unreturned services cannot establish successful cleanup or native progress.
Acceptance: all three returning-call/clear contracts pass for both result
hypotheses; nonvacuous step/path caps and removing models reject positive coverage.
Configs/reports stay in GAME_DIR, no proprietary fixtures or game claims change.
Exit: installed-package source controls and canonical Test.ps1 pass. Gap 27 stays
open until head/list, fill, cleanup/hardware, cache and snapshot contracts also pass.

The full outer-caller query is explicitly state-capped in later services/polling.
The acceptance split keeps the actual entry and stops immediately after all
three local field clears; the epilogue and later effects remain unread. Both
zero/FFFF return hypotheses pass exact field width/segment, selector push and
no-intervening-result-branch controls. This proves only the bounded conditional
caller contract and cannot establish completion of the whole helper.

## Head and predecessor mutation effect controls

Gap 27 acceptance uses FND-CONFIG-168's actual resident entry bounded through
the list/count phase and the following local service. Installed packages must
retain the head-link copy, count-one bypass and found-predecessor link copy as
separate guarded paths, followed by the word decrement before the service call.
Compare copied value provenance and widths without equating source/destination
storage or inventing record inputs. Search repetition, missing membership bounds,
unknown segment/address values and callee effects remain explicit. Balanced
returns and DS/SS/BP preservation are hypotheses; no memory preservation is added.
Acceptance: all three local phase contracts and a repeated-search witness pass;
step/path caps, a lower visit bound and unread-service controls remove their
respective positive claims. Source configs/reports stay in GAME_DIR. Exit:
installed controls and Test.ps1 pass; whole-call/native coverage and remaining
Gap 27 contracts stay open. No src/, spec claims or parity statuses change.

## Cleanup continuations and hardware effect controls

Gap 27 acceptance uses FND-CONFIG-183/184's mapped initializer failure path,
cleanup handle suffix and resident gated VGA service. Installed reporters must
retain cleanup before the error request under explicit returning-call hypotheses,
handle writes after unchecked services, and port effects on paths that skip later
slot handling. A stopped port instruction must remain a hardware dependency,
never become successful presentation evidence. Preserve actual entry boundaries
where practical; conditional suffixes explicitly leave earlier loops unread.
No model may assert callee memory preservation, successful release or process
survival. Acceptance: ordered-call/field-write and hardware-gate witnesses plus
nonvacuous step/path/unread-service controls; any unsupported instruction or
missing effect is an upstream tooling limitation, not a positive claim.
Configs/reports stay under GAME_DIR. Exit: installed-package controls, or a
bounded limitation reproduction and upstream request, with Test.ps1 passing.
The full Gap 27 contract remains open until all cited paths are covered.

## Layered cache and result-gated size controls

Gap 27 acceptance uses FND-CONFIG-186/187's temporary-byte bracket, mode
cache writer and mapped replacement-helper entry. Trace the first callee's own
byte clear before the parent's next call; retain the mode assignment before
the unchecked child request. In the replacement helper, separate successful
query length rejection from both failed-query routes to the common transfer
continuation, retain cache writes after unchecked transfer/services and expose
post-call SI provenance rather than preserving the original number by default.
Balanced returns and DS/SS/BP are hypotheses; memory/flags and native effects
remain unknown. Conditional caller splits leave other parent paths unread.
Acceptance: positive ordering/bypass/width/guard cases, zero/FFFF result
hypotheses and nonvacuous step/path/unread-service controls through installed
packages. Configs/reports stay in GAME_DIR; no original content enters Git.
Exit: source controls and Test.ps1 pass. Computed-dispatch, all callers,
accepted replacement content and complete Gap 27 remain separate open gates.

The joined parent bracket stops after the first callee's own clear: nested
unknown-memory call models invalidate its ancestor return-frame bytes. Balanced
stack height does not establish those bytes. Toolkit PR 63 has a locally verified scoped-
memory candidate and synthetic/source controls; preservation is not adopted
and its external delivery awaits explicit approval. Keep this joined acceptance open while independent cache controls proceed.

## Replacement-pointer branch and normalization controls

Gap 27 acceptance uses FND-CONFIG-188's complete resident wrapper. Installed
packages must retain the pre-call null/nonnull split, both conditional old-pointer
release gates, default/input pointer assignment, dword clears and word-to-dword
zero extension. Later reads must stay separate from the original stacked inputs
after unknown-memory services. Verify explicit AX zero after returning final
service hypotheses, without accepted-content, release or presentation claims.
Models preserve only declared DS/SS/BP and balanced returns; other effects remain
unknown. Acceptance: both branches and release bypasses, zero/FFFF local-service
return hypotheses, width/value provenance and nonvacuous step/path/unread controls.
Source configs/reports remain in GAME_DIR. Exit: installed controls and Test.ps1
pass; active child services, neighboring-byte/word gates, snapshots and full Gap
27 remain separate open contracts. No gameplay, spec claims or parity changes.

## Snapshot bypass and local restoration-path controls

Gap 27 acceptance uses FND-CONFIG-189's two complete resident service entries.
Retain word-gate bypasses without snapshots, snapshot-bearing early exits and
reachable local restoration stores as distinct paths. Verify saved-word and
restoration value provenance only on paths whose intervening instructions were
read; unknown child effects never establish rollback. Returning stack-guard
models preserve only declared DS/SS/BP, with memory/flags unknown. Other callees,
callbacks, handle-failure commits and native outcomes remain unread dependencies.
Acceptance: positive bypass/restoration distinctions, guarded-return witnesses,
word widths and nonvacuous step/path/unread-guard controls. Source configs/reports
stay in GAME_DIR. Exit: installed controls and Test.ps1 pass; every remaining
snapshot/commit path and the complete Gap 27 request remain open.

## Scoped-memory candidate implementation

Forward-port the completed scoped-memory helper to latest reviewed toolkit main
in a new isolated implementation candidate: at most 32 scopes and 4,096 total bytes per modeled call,
concrete pre-call segments/offsets, no wrap or alias overlap, explicit provenance
and unknown memory outside the scopes. Coordinate reader/engine protocol 2 and
major release metadata. Synthetic near/far/PE32, saved-frame, wrong/partial scope,
explicit overwrite, invalid/unreachable declarations and cap controls must pass.
Candidate archive source controls must join FND-CONFIG-186's caller bracket
under explicitly declared frame hypotheses while retaining unknown native effects.
No candidate imports enter adopted controls; no package adoption before reviewed
merge and registry delivery. Exit: full upstream gates and bounded source cases,
a new reviewable PR body, root Test.ps1 and handover. Preserve pypcode-only
instruction semantics and existing budgets. Full goal scope is unchanged.

## Signed-handle, byte-result and coordinate gate controls

Gap 27 acceptance uses FND-CONFIG-191's complete resident graphics wrapper,
coordinate validator and four getter bodies. Installed packages must keep
signed-negative handle bypasses distinct from AL-zero validation failures,
verify that nonzero AH does not override AL zero, and retain the primitive
request only after two nonzero low-byte results. Model primitive results as
forwarded words without normalization or successful graphics claims.

Trace the coordinate validator with all four actual getter regions, retaining
its signed first/last comparisons, equality admission and separate 320/200
limits, getter input/width/segment provenance, local AL outcomes and unread
native slot-capacity assumptions. Do not invent concrete incoming stack values
or interpret symbolic paths as native reachability. Acceptance: source-derived
bounds and complete local conditional paths where possible, positive gates
and ordering, malformed/width controls and nonvacuous step/path/unread-region
rejections. Model effects remain explicitly unknown. Configs/reports remain
in GAME_DIR. Exit: bounded installed-package controls, Test.ps1, acceptance
record and handover pass. Full request, active primitives, caller state and
native outcomes remain open; no game spec or parity status changes.

## Slot/reference and partial-metadata ordering controls

Gap 27 acceptance uses FND-CONFIG-191's complete handle-request entry and
FND-CONFIG-183's slot scanner as existing evidence. Installed reports must
retain the carry-based exhaustion exit separately from paths that write slot
reference/count metadata before coordinate validation. Verify the four ordered
signed comparisons, each accepted coordinate store before the next comparison,
FFFF failure without an own success-flag store, and the later success marker
and divided slot result. Trace reference-chain stops as unread dependencies.

Start with the complete request region and a bounded returning scanner model
with an explicitly declared selected-slot register hypothesis. Its carry and
memory remain unknown: no case may imply a native free slot, initial capacity,
argument identity or unchanged state. If limits prevent complete entry coverage,
retain the stopped paths and qualify any reached local write-prefix witnesses;
never raise a default or freeze a reference chain silently. Acceptance includes
exhaustion and partial-write/success controls, flag/word/ordering provenance and
nonvacuous step/path/unread scanner rejections. Configs/reports remain in
GAME_DIR. Exit: bounded installed controls, explicit retained limits, Test.ps1,
acceptance record and handover. Full caller/scan/chain/native coverage and the
complete request remain open; no game spec or parity change is authorized.

## Neighboring-byte and word-consumer provenance controls

Gap 36 acceptance uses FND-CONFIG-187's resident byte helper and
FND-CONFIG-188's word-gated resident services. Trace the zero/count helper
through its declared service body with conditional returning stack guards,
retaining own byte clears separately from later full-word reads and branches.
Unknown-memory guard models must invalidate earlier byte provenance; a balanced
return or preserved segment does not establish either neighboring byte.

Acceptance: exact segment, offset, width and call order; distinguish bypasses
from active-service routes; no low-byte-only admission claim or native high-byte
assumption. Wrong-width/neighbor controls and bounded step/path/unread controls
must reject the positive local contract. Source configs/reports remain in
GAME_DIR and skip absent licensed input. Exit: installed controls, explicit
unknown-byte and unread-child qualifications, Test.ps1, acceptance record and
separate handover. Gap 36 remains open until the complete producer-through-call
contract passes adopted tooling; no game spec, parity or gameplay change.

## Fill allocation, pre-transfer writes and failure-continuation controls

Gap 27 acceptance uses FND-SCRIPT-019's complete fill body as existing
evidence. Verify call/boundary mapping, then use explicitly conditional local
windows around allocation, slot bounds/age writes and transfer/error continuations
when fixed scans or unknown age branches prevent a complete joined query. Do
not repeat the previous capped whole-fill query or treat separate windows as a
joined path. Existing local/segment/register inputs are hypotheses, not native
state; no memory is seeded or silently retained across services.

Acceptance: stop-byte exit versus slot-bound writes; word arithmetic and byte
age-width provenance; a transfer-result gate separate from pre-transfer writes;
failure reaches returning error hypotheses without own rollback, while success
appends the stop byte and updates identity/current fields. Assert exact ordering
and nonvacuous step/path/unread controls. Unknown memory after modeled services,
loop limits, allocator effects, capacities and original input identity remain
explicit. GAME_DIR profiles/reports stay local and skip missing licensed input.
Exit: bounded installed controls, retained whole-fill gaps, Test.ps1, acceptance
record and handover; no spec, parity, gameplay or native-runtime changes.

## Synthetic capture fixture readiness

Tooling batch, 2026-10-06. Outcome: the Windows validation gate verifies its synthetic capture source is ready before its single capture assertion. Evidence: repeated positive-control failures recorded in template issue 73; actual decorated client width differs from the requested small fixture width, and offscreen-first presentation does not prove patterned pixel readiness. Acceptance: use exact-sized undecorated synthetic forms; independently verify known source pixels within a bounded readiness wait before moving offscreen; use independently initialized patterned and uniform forms. Preserve positive pixel comparison, uniform rejection, invalid-handle rejection and no rejected output. Do not retry capture assertions, change the production helper or control an original-game window. Tests: direct synthetic capture, deliberate uniform positive, worker timeout/recovery, canonical assetless gate. Exit: all these controls pass and fixture setup failure gives bounded diagnostics. No owner questions; no game behavior or content contract changes.

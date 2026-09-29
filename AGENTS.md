# Agent instructions

These instructions apply to the whole repository and to humans and coding agents
alike. Read them before changing anything.

This repository is a template for clean-room MonoGame restorations of classic
games. A checkout is in one of two states, and
`tools/project-config.json` says which:

- **Unconfigured template** (`"configured": false`): generic scaffolding with
  `{{PLACEHOLDER}}` tokens and `Restoration.*` project names. Changes here must
  stay game-agnostic and keep working for every future project.
- **Configured project** (`"configured": true`): a restoration of one specific
  game. Changes here are game-specific and must keep the evidence trail intact.

### Terminology mapping

The original game and manuals use **race** as the conventional fantasy term for
peoples such as humans, elves, dwarves, and similar character origins. It is
not intended by this project in an offensive or real-world racial sense. The
reimplementation maps original **race** terminology to **origin** so that new
code, APIs, UI, tests, and project-authored descriptions use respectful modern
language. Evidence records may retain **race** or **racial** only when quoting
or naming an original heading, table, field, or claim; those source terms map
to **origin** and **origin-based** in implementation.

## Plan before you build

**Do not write implementation code before `docs/IMPLEMENTATION-PLAN.md`
describes the work.** This applies to a new vertical slice and to any change
that introduces a format reader, a rule, or a persisted file layout.

A slice is described when the plan states its player-visible outcome, the
evidence it relies on, the acceptance criteria for rules, presentation, and
original-content behavior, the automated tests that prove it, and an Exit a
script or reviewer can check. Guesses belong in `queue/` or an entry's Open
questions, not in an API.

The owner decides only what the work protocol's
[What needs the owner](vendor/upstream/work-protocol.md#what-needs-the-owner) (lines 286-288)
lists: eligibility and supported editions, scope and non-goals, any deviation
whose Default is `on` or `mandatory`, features outside the parity matrix,
releases, and live sessions. Those decisions go in `docs/DECISIONS.md`, and a
question only the owner can answer goes in the plan's owner questions while
the work that depends on it waits under `Blocked` in the queue. Everything else
goes ahead without approval, and the owner reviews the result.

## Specializing this template for a game

Work in this order.

**0. Establish the facts.** Identify the original game, its developer, release
year, genre, and the editions the owner legally has. Record which storefronts or
media they came from, and what research already exists (manuals, community
documentation, prior reverse-engineering). Ask the owner for anything you cannot
determine; never invent an edition, a fingerprint, or a file format.

**1. Write the implementation plan.** Fill in `docs/IMPLEMENTATION-PLAN.md`: the
game profile with its eligibility and stage, the scope and non-goals, the
ordered vertical slices with acceptance criteria and exits, the risks, and the
questions only the owner can answer. Eligibility (released in 2004 or earlier,
no official remake or remaster on sale) is checked once and recorded there,
and not re-checked unless the owner asks.

**2. Configure the project identity.** Fill in `tools/project-config.json` and
run `./tools/Configure-Project.ps1`, then `./tools/Verify-Configuration.ps1`.
`docs/CUSTOMIZATION.md` documents every field and every derived default. Do not
hand-edit placeholders the script can substitute.

**3. Record what is known about the original.** Write it in `spec/` under the
documentation standard: a build entry for each edition, a source entry for each
manual, guide or outside tool, and findings, formats, rules and screens for what
they show. Give every entry the status its evidence supports; `unknown` is a
valid status and a plausible-sounding guess is not.

**4. Make extraction real.** Replace the sample manifest under
`src/<Project>.Extractor/source-manifests/` with one fingerprint manifest per
supported edition, extend `tools/repository-policy.json` with the extensions the
original actually uses, and make a missing or unsupported source produce an
actionable error rather than a crash. The separately runnable Extractor verifies
a licensed source and transactionally creates a complete local asset pack; the
Game consumes only that verified pack.

**5. Build the first vertical slice.** Follow the plan. Prefer a thin
end-to-end slice (identify, import, start, show something real, quit cleanly)
over broad but unplayable systems.

**6. Verify and hand over.** Run the commands below, bring the rows in `parity/`
and the files in `deviations/` in line with what is actually true (the check
regenerates `PARITY.md` from them), and tick off `docs/BOOTSTRAP-CHECKLIST.md` as
decisions are captured elsewhere.

## The local copy of the standard

`vendor/upstream/` holds the methodology, the documentation standard and the
work protocol exactly as published at dinorefurb.com, and
`tools/upstream-lock.json` names the commit they were copied from. Every
reference to those pages in this repository points at that copy.

- Read the local copy. Do not fetch dinorefurb.com, or the website's source
  repository, to read the methodology, the standard or the protocol, and do
  not go online because the published rules might have changed.
- Assume the local copy is up to date and rely on it. The template is updated
  when the website changes; until then the copy is the rules.
- Links inside the copy are the site's own: `/work-protocol/#emulated-calls`
  is `vendor/upstream/work-protocol.md#emulated-calls`, and the same for
  `/methodology/` and `/documentation-standard/`. Only links to other pages of
  the site lead outside the copy.
- A link to a section gives its lines, such as
  `work-protocol.md#batches (lines 137-183)`. Read only those lines (with an
  offset and a limit), never the whole page for one section, and never a
  section again once it is in context. The lines include the section's
  subsections, so a link to a subsection inside one already read adds
  nothing. The upstream tests keep every range right; after a refresh,
  `node tools/upstream.mjs links --write` rewrites them.
- Skills and summaries in this repository are enough to do the work. Open a
  linked section only when a step leaves a question it answers.
- Checking whether a newer version has been published, and refreshing the
  copy, is always started by a person. Do it only when the owner asks for it
  in the current task, and then follow `docs/UPSTREAM-RULES.md`.

## Planning and tracking work

Work is planned, tracked and handed on under the work protocol
(`vendor/upstream/work-protocol.md`). Read only the section a task needs, by the
lines its link below gives; this list says where each thing lives in this
repository.

- Stage: `docs/IMPLEMENTATION-PLAN.md`
  ([Stages](vendor/upstream/work-protocol.md#stages) (lines 32-78)). What can be done with the
  original running, and by whom: `docs/RUNTIME.md`
  ([Runtime access](vendor/upstream/work-protocol.md#runtime-access) (lines 42-60)).
- Open questions: `queue/<AREA>.md`, with IDs such as `Q-COMBAT-012`
  ([The queue](vendor/upstream/work-protocol.md#the-queue) (lines 80-119),
  [Order of work](vendor/upstream/work-protocol.md#order-of-work) (lines 112-119)).
- Coding agents never run the game here. Every run is an owner live session
  requested in `docs/live-sessions/`; the queue's `Agent run` section stays
  empty and agents never take the machine's run lock
  ([Live sessions](vendor/upstream/work-protocol.md#live-sessions) (lines 205-215)).
- Emulated calls use the harness in `tools/emu/`; any agent may build it and
  make them, whatever limits on runs of the game say
  ([Emulated calls](vendor/upstream/work-protocol.md#emulated-calls) (lines 217-249)).
- Reports from testers: `docs/reports/`, recorded with the `triage-report`
  skill. Screenshots of the rebuild go in `GAME_DIR/reports/`, of the original
  in `GAME_DIR/captures/`, never in Git
  ([Reports from testing](vendor/upstream/work-protocol.md#reports-from-testing) (lines 251-267)).
- Batches, commits and their `Spec:`, `Parity:` and `Queue:` trailers:
  [Batches](vendor/upstream/work-protocol.md#batches) (lines 137-183). Statuses and how a claim
  moves between them:
  [The life of a claim](vendor/upstream/work-protocol.md#the-life-of-a-claim) (lines 121-135).
- Handover: `docs/HANDOVER.md`, running goals in `docs/goals/`, the owner's
  decisions in `docs/DECISIONS.md`
  ([Sessions](vendor/upstream/work-protocol.md#sessions) (lines 185-193),
  [Coding agents and long-running goals](vendor/upstream/work-protocol.md#coding-agents-and-long-running-goals) (lines 290-326)).
- Progress is what scripts compute, never a hand-written percentage. Function
  inventories are committed as `coverage/<build ID>/<manifest path>.tsv`
  ([Measuring progress](vendor/upstream/work-protocol.md#measuring-progress) (lines 269-284)).

The procedures are skills in `.claude/skills/`: `runtime-access`,
`plan-work`, `start-session`, `research-item`, `implement-rows`,
`triage-report`, `live-session` and `end-session`. For a `/goal`, write the goal file with
`plan-work`, keep to its scope, and end every batch with the status block the
skills print.

### Local research and runtime constraints

Static work precedes emulated calls and owner runs within the protocol's order.
A live-session item needs its own static attempt or asks to confirm a static
reading. Never wait idle for an owner run or tester report.

The planned Unicorn harness in `tools/emu/` calls one function of `DSUN.EXE`
without starting the game or DOSBox. It cannot reach the FBOV overlay pack and
does not exist yet; those items wait for a tooling batch (`docs/RUNTIME.md`).
Emulated calls need no owner decision or run lock. The owner-only native capture
rules under "Native runtime visual validation" remain binding.

Every batch runs `./tools/Test.ps1` before commit, ends with the skills' status
block, and uses the protocol's Spec, Parity, Queue and Report trailers as needed.

## Rules that never bend

- **No original content in Git, ever.** No assets, executables, archives, save
  files, screenshots, or data extracted from them. `UserContent/`,
  `analysis/original/`, and `reference/original/` are local-only, and
  `tools/Verify-Repository.ps1` enforces this. Synthetic fixtures go under
  `tests/fixtures/synthetic/`.
- **Clean room.** Do not copy original source, decompiler output, disassembly,
  byte dumps, or analysis databases into this repository. Describe behavior and
  data formats in your own words in `spec/`, and write the implementation from
  that description. Do not translate the original machine code into matching
  source, and do not patch the original executable one function at a time.
- **Evidence before claims.** Every spec entry cites the findings, experiments,
  and sources its status requires. Evidence from the original that contradicts
  an entry makes it `disputed`, with both sides cited, until new evidence
  settles it. A conflict between the manual, a guide and the shipped game is
  kept in the entry, and what the rebuild does about it is a deviation.
- **Measured reproduction, never speculative gameplay.** Do not turn a manual,
  a walkthrough, a generic genre convention, an opaque resource, or an
  unconfirmed static-analysis lead into executable game behavior. This is
  especially strict for combat: no encounter, turn loop, AI, target selection,
  damage/effect pipeline, timing, UI transition, or resource schema may be
  implemented until controlled native observations and a traceable data or
  executable finding establish the relevant behavior. Isolated manual-derived
  helpers may remain research artifacts only when their scope is explicitly
  documented; they must not be presented as a playable combat foundation.
  Prefer closing a measured UI/layout gap or recording a bounded negative
  finding over adding a plausible system.
- **CI never needs proprietary content.** Every packaging check, and every test
  that does not compare against the original, passes on a machine with no copy
  of the game. Tests that read the original find it through `GAME_DIR`,
  report themselves skipped when it is absent, and carry the comment
  `// needs: GAME_DIR`. They run on a maintainer's machine, and the run is
  recorded in `VALIDATION.md` (`docs/VALIDATION.md`).
- **Parse defensively.** Original files are untrusted input: bound every length,
  reject path traversal, and fail with a diagnosable error instead of throwing
  from deep inside a reader.
- **Revision derived-content contracts.** Whenever a required extracted asset,
  UI/resource graph, or semantic pack contract changes, increment
  `OriginalContent.RequiredAssetPackRevision` and add a test proving that the
  previous otherwise hash-valid revision is rejected. The launcher must then
  refresh the pack transactionally from the licensed source. Never assume a
  self-consistent manifest proves it satisfies newer runtime expectations. This
  is a required extraction revision, not a compatibility version: it makes no
  promise about saves, editions, or gameplay, and deliberately invalidates old
  local packs when their required output contract changes.
- **Extract before extending behavior.** Once the plan's full-corpus extraction
  gate is active, do not add gameplay rules, UI semantics, or new screen flows.
  First inventory every supported source file and resource, extract each into a
  source-mapped, bounded, hash-verified local-pack representation, and record
  unknown structures as opaque rather than guessing their meaning. Resume
  behavior work only after the owner-approved gate acceptance criteria pass.

## Reverse-engineering discipline

The methodology (`vendor/upstream/methodology.md`) and the documentation
standard (`vendor/upstream/documentation-standard.md`) govern the spec. Read
only the section a task needs, by the lines its link below gives; the points
below are the ones every session relies on.

- The executable has the final word on what the shipped game does; the manual
  and fan sources are leads
  ([Ground rules](vendor/upstream/methodology.md#ground-rules) (lines 12-22)).
- Use the standard's statuses and no other scale, and never promote a
  plausible interpretation
  ([Status](vendor/upstream/documentation-standard.md#status) (lines 118-167)).
- Unidentified functions and globals keep neutral names (`fn_00478CD0`,
  `g_004C1F20`) until evidence shows what they do
  ([Notation](vendor/upstream/documentation-standard.md#notation) (lines 169-202)).
- The spec describes the original only, never names a class, file or setting
  of this repository, and never copies the game's writing, art or code
  ([Where it lives](vendor/upstream/documentation-standard.md#where-it-lives) (lines 14-92),
  [Licence](vendor/upstream/documentation-standard.md#licence) (lines 888-898)). Blank entries
  of each kind are in `docs/SPEC-ENTRY-TEMPLATES.md`; tool procedure is in
  `docs/GHIDRA.md`.
- Never commit broad decompiler, instruction or Version Tracking exports. The
  function inventories in `coverage/` are the one export that is committed.

## Fidelity

What the rebuild keeps and what it may change is set by
[Where fidelity stops](vendor/upstream/methodology.md#where-fidelity-stops) (lines 52-64).
Every departure from the spec is a `DEV-AREA-NNN` file in `deviations/`
([Deviation log](vendor/upstream/documentation-standard.md#deviation-log) (lines 774-795)). The
validation suite runs with every setting switched off, and a test that reaches
a mandatory deviation cites its ID. Rebalancing and new features belong in a
separate mode or project.

## Citing the spec

Code comments and tests cite the spec IDs they implement or check. A guessed
formula carries a `PLACEHOLDER: <spec ID>` comment, and that parity row cannot
be `complete` while it does. `PARITY.md` holds the totals and `parity/` the
rows ([Parity matrix](vendor/upstream/documentation-standard.md#parity-matrix) (lines 797-865));
behavior without a spec entry gets an `unknown` entry before any code. Manual
play never counts as a test.

## Architecture boundaries

- `<Project>.Core`: deterministic rules and serializable state. No MonoGame, no
  file-format parsing, no I/O.
- `<Project>.Resources`: bounded binary parsing and original-content contracts.
  No MonoGame.
- `<Project>.Game`: MonoGame DesktopGL presentation, and the only project that
  may depend on both of the above.
- `<Project>.Extractor`: separately runnable licensed-source verification and
  transactional asset extraction over `Resources`.
- `<Project>.Inspect`: read-only tooling over `Resources`.
- `<Project>.Tests`: architecture, safety, and behavioral tests.

**Rules live in `Core`; screens map them.** Every decision the original makes â€”
a flag cascade, a gate, a branch table, an outcome selector, a state
transition â€” lives in `Core` as a pure function of the serializable state and
its inputs, even when only `Game` calls it. `Game` translates those decisions
into screens, art, input, and timing; a rule may not have `Game` as its only
home, and a rule already written inline in a screen handler is extracted the
first time it is touched. The placement test: if demonstrating a behavior needs a
window, a graphics device, or the asset pack, the rule is not in `Core` yet.

Implement each entry's branch table whole: every branch the entry describes
is handled by its Core function and has its own test, including the branches
`Game` cannot reach yet and the "impossible" arms of a guard. The branch that
lives only in a code comment is the one that gets implemented inverted. A
branch no entry describes is never a guess: it becomes a question in the
entry's Open questions and a `Spec gap:` note on the parity row, as the
`implement-rows` skill says.

Each rule ships with fast-gate tests over synthetic state. The rule itself is
usually a static class over the serializable state type, called by `Game`.
When a bug is traced to branch logic in `Game`, extract the rule into `Core`,
pin every branch with a test, and fix it there.

Determinism is a feature: identical commands and seed must produce identical
state, because saves, replays, and parity validation depend on it. Every
compiled C# file is limited to 1,000 lines; split responsibilities instead of
raising the limit.

## Commands

```powershell
./tools/Verify-Configuration.ps1   # placeholders and template leftovers
./tools/Verify-Repository.ps1      # original-content and large-file policy
./tools/Test.ps1                   # repository policy plus the test suite
dotnet build <Project>.slnx        # full solution
dotnet run --project src/<Project>.Game -- --smoke-test
```

## Static executable analysis

Treat static analysis as evidence, not a search-engine oracle. Establish and
document the approved executable's exact path, size, and XXH3-128 once, then
reuse that stable named target for focused queries without rehashing it each
time. Revalidate only when the path, size, last-write metadata, source package,
or documented edition changes, when a fresh analysis environment lacks the
recorded baseline, or when there is a concrete reason to suspect replacement.
Ask one narrow player-visible question at a time and record the answer as a
finding in `spec/findings/`, with the build, the tool and its version, the
locations, how to reproduce it, and the competing interpretations under
Alternatives.

For a known filename, label, or other ASCII text, do **not** conclude it is
absent merely because Ghidra does not expose it through `getDefinedData()` or
its ASCII-string analyzer. First search the raw loaded bytes for the explicit
null-terminated ASCII pattern with `tools/ghidra/ReportBytePattern.java`. For
each bounded match, inspect its references with `ReportReferences.java` and its
instruction context before considering a bounded decompilation window. A raw
string hit with no recognized direct reference may still be reached indirectly
or copied at runtime; it is not behavior evidence. Conversely, an analyzer
classification miss is not a negative finding and must not be documented as
one. Keep the query artifacts under ignored analysis locations and describe
findings in repository-authored words rather than copying decompiler output.

## Native runtime visual validation

The Codex computer-control surface currently available for this repository
exposes browser tabs only; it cannot target native MonoGame or DOSBox windows.
Check the available surfaces once before attempting native-window automation.
When native apps are unavailable, do not keep retrying browser-only automation
or claim that a live visual check ran. Use purpose-built headless/content smoke
tests and owner-produced screenshots or captures instead, and record any visual
comparison that still requires manual validation.

### Image-less UI controls and dialogue chrome

An image-less `WIND`, `BUTN`, `EBOX`, or `APFM` record is not evidence that the
original rendered an unstyled rectangle. It can delegate its fill, bevel,
corners, clipping, or focus treatment to native widget code. Preserve the
record's identity, bounds, child order, and event fields; do not replace an
unknown native widget with invented pixels or assume a neighbouring bitmap is
its background.

For a presentation mismatch, first compare the complete ordered control graph
and the extracted image dimensions/alpha bounds against an owner-confirmed
native capture at the same logical resolution. If the graph has no image that
accounts for the missing chrome, treat the generic widget path as an open
evidence question: make a narrow static-analysis query against the documented
approved executable target, revalidating its identity only under the static
analysis conditions above, and obtain a bounded capture that shows each
corner/state required. Record measured geometry and colours in findings and in
the screen's entry in `spec/screens/`, at the status the evidence supports, add
synthetic tests for those measurements, and keep the capture itself outside
Git. Do not claim pixel parity until the native
widget treatment is observed.

Coding agents must never launch, control, capture, or stop DOSBox on their own.
When an evidence question requires an original-game observation, give the
repository owner an exact, bounded live session request in
`docs/live-sessions/` and wait for
the owner to confirm that the requested material has been produced with
DOSBox's built-in Ctrl+F5 screenshot command. After that confirmation, parse
the configured DOSBox screenshots folder and inspect only images whose file
timestamps fall within either an owner-confirmed capture window, an
owner-authorized local date-and-hour slot (including UTC offset), or an
owner-confirmed explicit filename list. An authorized hour means the half-open
local interval from that hour through the next hour. An explicit filename list
authorizes only those exact filenames, not similarly named files.
The repository owner may instead grant durable umbrella authorization for every
file in the configured DOSBox screenshots folder; when recorded below or in
`docs/live-sessions/README.md`, that authorization supersedes timestamp and filename
selection only. Captures remain local-only original content, and semantic
labels still require owner confirmation before they are recorded as evidence.
Do not operate the DOSBox window, invoke Ctrl+F5, use another screen-capture
mechanism, or infer that an unconfirmed request, an old capture, or the
presence of a DOSBox process means the requested observation was performed.
Before recording or relying on a screenshot's semantic identity, such as a
named screen, actor, turn, action, or transition, ask the owner to confirm that
proposed label. Geometry/pixel measurements may be recorded as provisional
observations without assigning that identity. If neither a timestamp window,
date-and-hour slot, nor explicit filename list is authorized, ask the owner
before inspecting any candidate image.

## Post-commit orphan-process audit

After every commit, inspect running processes for orphaned work launched while
building, testing, validating, rendering, or analyzing this repository. Check at
least PowerShell (`powershell` and `pwsh`), Ghidra/Java, .NET (`dotnet` and
`testhost`), DOSBox when used for controlled original-game observation, and any
other process family started during the completed batch.

A process belongs to this work only when its command line, parent process,
task/session, or source paths connect it to this repository or to the authorized
reference installation at `C:\GOG Games\Dark Sun 2`. Never terminate a process
merely because its executable name matches. Preserve user/IDE/system processes,
work for other repositories or games, and validation intentionally still in
progress. Reusable .NET/MSBuild/test workers (including MSBuild nodes started
with `nodeReuse:true`) are expected to remain alive by design and are not
orphans unless separate evidence shows they are stuck, abandoned, and tied
exclusively to failed repository work. If ownership is uncertain, leave the
process running.

Stop only confirmed orphaned processes. Whenever one is stopped, append an
entry to `orphanCleanupLog.md` with the local timestamp and UTC offset, PID,
process name, start time or task/session when known, the evidence that made it
an orphan, and any related process deliberately left running. If the audit
finds nothing to stop, do not create a log entry.

## Definition of done

A change is finished when the solution builds, `./tools/Test.ps1` passes, new
behavior has tests for every branch its spec entry describes, the spec entries it relies on exist with the status their
evidence supports, the documents that assert status (`README.md`, `PARITY.md`,
`parity/`, `deviations/`) match reality, and `queue/` and the plan's owner
questions have been updated with whatever the work settled or newly raised.

Commits describe the change and its evidence, not the tooling that produced it.

## Evidence review and offline rules

Apply the claim-relevant procedure in [EVIDENCE-REVIEW](docs/EVIDENCE-REVIEW.md)
before asserting a complete reading. How the local copy of the standard is
verified and, when the owner asks, refreshed is in
[UPSTREAM-RULES](docs/UPSTREAM-RULES.md). These procedures keep Standard v1.

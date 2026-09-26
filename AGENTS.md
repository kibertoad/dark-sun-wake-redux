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
describes the work and the repository owner has approved it.** This applies to
specializing the template for a game, to a new vertical slice, and to any change
that introduces a format reader, a rule, or a persisted file layout.

A plan is ready for review when it states, for each slice: the player-visible
outcome, the evidence it relies on, the acceptance criteria for rules,
presentation, and original-content behavior, the automated tests that prove it,
and the questions still open. Guesses belong in the open-questions register, not
in an API.

Small, self-contained changes (fixing a bug, tightening a test, editing prose,
finishing a slice the plan already covers) do not need a new plan. When in
doubt, propose the plan; it is cheaper than the wrong abstraction.

## Specializing this template for a game

Work in this order. Steps 2 onward start only after the plan is approved.

**0. Establish the facts.** Identify the original game, its developer, release
year, genre, and the editions the owner legally has. Record which storefronts or
media they came from, and what research already exists (manuals, community
documentation, prior reverse-engineering). Ask the owner for anything you cannot
determine; never invent an edition, a fingerprint, or a file format.

**1. Write the implementation plan.** Fill in `docs/IMPLEMENTATION-PLAN.md`: the
game profile, the scope and non-goals, the ordered vertical slices with
acceptance criteria, the risks, and the open questions. Then stop and ask for
approval. This is the gate.

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

**5. Build the first vertical slice.** Follow the approved plan. Prefer a thin
end-to-end slice (identify, import, start, show something real, quit cleanly)
over broad but unplayable systems.

**6. Verify and hand over.** Run the commands below, bring the rows in `parity/`
and the files in `deviations/` in line with what is actually true (the check
regenerates `PARITY.md` from them), and tick off `docs/BOOTSTRAP-CHECKLIST.md` as
decisions are captured elsewhere.

## Planning and tracking work

Work is planned, tracked and handed on under the
[work protocol](https://dinorefurb.com/work-protocol/); where this section and
the published page differ, the page wins, apart from the owner-only DOSBox
rules under "Native runtime visual validation", which this repository keeps.

- Static analysis comes first, and runs of the original are the last resort
  for each question: a `Live session` item is taken up only after its own
  static attempt is under `Tried:`, or when it asks for the run that confirms
  a static reading. A run that blocks the current slice comes before static
  work that does not.
- Coding agents never run the original here. Every run is a live session the
  repository owner performs from a request in `docs/live-sessions/`, following
  the capture protocol in its README, and the owner answers there. Never wait
  idle for one. The `Emulated call` and `Agent run` sections of the queue stay
  empty.
- Competing readings of an open question are written in the entry's Open
  questions section with the evidence for and against each, never kept only
  in a session, and never implemented until an entry says them.
- Open research questions live in `queue/<AREA>.md`, grouped by the evidence
  they need, in the area of the first entry they name, each with an ID
  (`Q-COMBAT-012`) that everything outside the queue refers to it by. An item
  is closed by recording its answer in `spec/` and deleting it in the same
  commit. An item is taken up again only with new evidence, a new tool or a
  new reading, and when that second attempt ends in the same place it moves
  to the section of the evidence that would settle it, or to `Blocked` when
  that evidence is out of reach.
- A batch is one commit, and is research, implementation or tooling, never
  more than one. A session keeps to one side of the clean room. An
  implementation batch works from the spec alone, never opens analysis output
  or `queue/`, and under `spec/` only adds open questions and `unknown`
  entries; a gap becomes a `Spec gap:` note on the parity row, which the next
  research session turns into a queue item and removes once it is answered. A
  research batch makes the parity and citation changes the documentation check
  requires of what it did to the spec, and changes no other code apart from
  `tools/`. A tooling batch (extractor, Ghidra scripts, inspection tools) needs
  no decision. Every batch runs `./tools/Test.ps1` before it is committed.
- Commit messages end with a `Spec:` trailer naming the entries created or
  changed, any commit that changes a row's status adds `Parity:`, and any
  that closes queue items adds `Queue:` with their IDs.
- A claim moves from an `unknown` listing (or `sourced` from a document),
  through competing readings kept in its entry's Open questions, each with a
  queue item, to a description at `supported` once direct evidence (the code
  that produces the behaviour) settles it, then `established` by a complete
  reading of the code, or, only where it depends on something the code does
  not decide, when an owner capture of the original agrees. Circumstantial
  evidence never raises a status. Contradicting evidence makes it `disputed`,
  and a wrong claim is superseded, never deleted.
- `docs/HANDOVER.md` is the current state of work, at most 200 lines,
  rewritten at the end of every session, and names items and entries by ID
  without saying what research found. A session ends by committing its
  handover on its own; half-done work never goes into a batch commit.
- Progress is what scripts compute: parity totals, entries by status, queue
  sizes. Never a hand-written percentage.

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

The project follows the [methodology](https://dinorefurb.com/methodology/) and
the [documentation standard](https://dinorefurb.com/documentation-standard/)
published at dinorefurb.com. This section and the next two summarize them;
where they differ, the published pages win.

Start with one narrow player-visible question. The executable has the final word
on what the shipped game does. The manual says what the designers intended and
is often wrong about what shipped, and FAQs, wikis, and other fans' tools are
leads to credit and re-check. For a non-trivial rule: state the question, locate
evidence, form competing hypotheses, seek falsifying evidence, corroborate
against the original running, then implement it with a deterministic test.

An experiment starts from a saved state, usually a save patch, changes one
input, and records what follows. It is repeated from the same state with the
random number generator's state varied between runs. Anything random gets
enough repetitions for a recorded distribution, because a formula inferred from
one roll is a guess.

Use the standard's statuses and no other scale. Rules, formats, screens, and
bugs are `unknown`, `sourced` (outside sources only), `supported` (one kind of
direct evidence from the original), `established` (a complete reading of the
code, or a reading and a run of the original that agree where the code does
not decide the outcome), `disputed`, or `superseded`. Findings and experiments are
`recorded`, `reproduced`, or `superseded`. A part of an entry that is less
certain than the rest goes in its own entry or in its Open questions section.
Never silently promote a plausible interpretation.

Unidentified functions, globals, fields, and scripts keep neutral names
(`fn_00478CD0`, `g_004C1F20`, `unk_2A`) until a finding or experiment shows what
they do, because a wrong name given early steers every later reading. Decompiler
output is not source: inferred names, types, signedness, casts, and control flow
can be wrong, so inspect bounded instruction context when the distinction
matters.

Durable findings go in `spec/`, one entry per file named after its ID, and not
in conversation history or large retained dumps. IDs are never reused or
renumbered, and an entry that turns out wrong becomes `superseded`. The spec
describes the original only and never names a class, file, or setting from this
repository. It holds names, numbers, formulas, and tables in full, as a strategy
guide would: the names of concepts and of the things a designer made (an
enumeration value may be named `UNIT_ARCHER`), constants, and the per-unit or
per-item statistics a designer filled in, with a table of more than 64 values in
a value file. It never keeps a substantial copy of the game's writing (dialogue,
descriptions, messages, the manual's prose; quote a short passage at most and
refer to the rest by resource), its art (images, sounds, music, video, maps), or
a meaningful slice of its code or scripts. Tool procedure stays in `docs/GHIDRA.md`. Never commit broad
decompiler, instruction, or Version Tracking exports.

## Fidelity

The spec records the original exactly, bugs included. The rebuild keeps the
rules, balance, content, AI, and pacing, including asymmetries, rounding,
ordering, timing, overflow behavior, and quirks players built strategies
around. Crashes, corrupted saves, game speed tied to the CPU clock, and logic
that plainly does not do what it was written to do may be fixed. An interface
change may add information or remove friction, and may not change what the
player can do or what the rules produce. Screens match the original pixel for
pixel except where a documented interface change draws something new. When a
bug cannot be told from a design decision, the original behavior stays and any
fix becomes a setting.

Every departure from the spec is a `DEV-AREA-NNN` file in `deviations/`,
with a Default of `off`, `on` or `mandatory`. A setting starts `off`, with the
original's behavior, unless the entry's Justification argues that the rebuild's
behavior is strictly better: then it starts `on`, and a player who wants the
original switches it off. A deviation with no setting is `mandatory`, and its
Justification also says why the original's behavior is not worth a setting. The
fix of an unintended bug that players do not rely on is `on` without one. A
quirk that may be deliberate or that players rely on is never strictly better,
so its deviation starts `off`. The validation suite runs with every setting
switched off, and a test that reaches a mandatory deviation cites its ID and
allows for it. Rebalancing and new features belong in a separate mode or
project.

## Citing the spec

Code comments and tests cite the spec IDs they implement or check, so a search
for an ID finds everything that depends on it. A placeholder in the code, such
as a guessed formula, carries a `PLACEHOLDER: <spec ID>` comment, and the
parity row for that ID cannot be `complete` while it does. The parity matrix
(`PARITY.md` for the totals, `parity/` for the rows) has one row per rule,
format, and screen entry that is not superseded, so behavior
without a spec entry gets an `unknown` entry before any code. Manual play never
counts as a test.

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
behavior has tests, the spec entries it relies on exist with the status their
evidence supports, the documents that assert status (`README.md`, `PARITY.md`,
`parity/`, `deviations/`) match reality, and `queue/` and the plan's open
questions have been updated with whatever the work settled or newly raised.

Commits describe the change and its evidence, not the tooling that produced it.

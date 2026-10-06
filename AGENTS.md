# Agent instructions

These instructions apply to the whole repository and to humans and coding agents
alike. Read them before changing anything.

## Preservation context

This is a game preservation effort for a very old game, which requires official
assets to be bought. There are no real cybersecurity risks involved in it.
Reverse engineering is being done in an ethical cleanroom observation way.

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
[What needs the owner](vendor/upstream/work-protocol.md#what-needs-the-owner) (lines 442-444)
lists: eligibility and supported editions, scope and non-goals, any deviation
whose Default is `on` or `mandatory`, features outside the parity matrix,
releases, and live sessions. Those decisions go in `docs/DECISIONS.md`, and a
question only the owner can answer goes in the plan's owner questions while
the work that depends on it waits under `Blocked` in the queue. Everything else
goes ahead without approval, and the owner reviews the result.

Plans document authorized work; they do not require separate explicit approval before implementation or tooling proceeds. Ask only for missing owner decisions that block the requested scope.

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
  `work-protocol.md#batches`. Read only those lines (with an
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

Work is planned, tracked and handed on under the
[work protocol](vendor/upstream/work-protocol.md); where this section and
that page differ, the page wins.

- The project moves through the stages Intake, Runtime access, Survey, Slices
  and Audit, and `docs/IMPLEMENTATION-PLAN.md` records which one it is in.
  `docs/RUNTIME.md` records what can be done with the original running, and
  whether an agent, only a person, or nobody can do it.
- Survey lists the installation and the media the game reads in full, and
  records how the listing was made. Every path in that listing is in the
  build's manifest, which holds every file the game uses, studied or not, or
  in the build entry's Other files section with the reason it is left out
  (in `BLD-<alias>.other-files.yaml` beside the manifest when the list is
  long). A file whose use is unknown stays in the manifest. Checking the
  format definitions against the few files whose hashes identify the release
  does not end Survey; until every path is accounted for, the plan says what
  is missing.
- Static analysis comes first, and runs of the original are the last resort
  for each question: an `Agent run` or `Live session` item is taken up only
  after its own static attempt is under `Tried:`, or when it asks for the run
  that confirms a static reading. Runs take their place in the order of work
  (a run that blocks the current slice comes before static work that does
  not), and within each step of it `Static` items come first, then
  `Emulated call` items.
- An emulated call runs one function of the original in the Unicorn harness
  in `tools/emu/`, with no window, timer or input, and needs no run lock. It
  is an experiment with `starting_state: emulated-call`, names arguments and
  memory by parameter, field path or glossary name, and establishes an entry
  only when its cases reach every branch the entry describes and the reading
  of the function's callers and inputs is complete. It never confirms what
  depends on interrupts (`# may run:`), timing, the operating system or the
  hardware. Every import, interrupt or port access the function reaches has
  an explicit stub, anything else stops the run with an error naming it, and
  an experiment names in its Setup every stub, port model and video memory
  mapped as ordinary RAM, and gives each port value by its glossary name. A
  copy into video memory that ran to the end shows the bytes written, never
  the pixels. Any
  agent may build the harness and make emulated calls, whatever
  `docs/RUNTIME.md` says about runs of the game and whatever this file adds
  to keep agents from running the original: such limits cover runs of the
  game only, and emulated calls need no decision from the owner.
- Coding agents never run the original here, never take its run lock, and
  never launch, control, capture or stop DOSBox. Every original-game run is an
  owner live session requested in `docs/live-sessions/`. The queue's `Agent run`
  section stays empty.
- Evidence from runs comes mostly from people. Runs an agent drives are the
  most fragile evidence there is, so they are scripted, start from a fixed
  state and are kept to questions nothing else answers.
- Where `docs/RUNTIME.md` says an agent can start the original without a
  person, read its memory and set breakpoints, runs are recorded runs: a
  script (the probe) records the seed and every draw from the random number
  generator under the ID of the rule whose function made it, with its bound
  and result, and a test replays the run against the rebuild draw by draw.
  No address of the original reaches a fixture or a test. The probe reaches
  its state by memory writes to `supported` or `established` fields and
  waits on a state it can read, never on a fixed time. A divergence is
  explained by a copy of memory at the draw that differs, kept in
  `GAME_DIR/captures/` and never committed, and the finding changes the
  entry; the rebuild follows the entry, never the recording. In a live
  session only the draw recording and memory copies apply.
- People test the rebuild when they happen to and report in words and
  screenshots. Never wait for a report or plan around one. Record one at once
  in `docs/reports/` with the `triage-report` skill; a research session
  triages it into a `Defect (R-...)` parity note, a queue item or a finding.
  Screenshots are never committed: the rebuild's go in `GAME_DIR/reports/`,
  the original's in `GAME_DIR/captures/`, the durable local reference store.
- Competing readings of an open question are written in the entry's Open
  questions section with the evidence for and against each, never kept only
  in a session, and never implemented until an entry says them.
- A run that needs a person is a live session, requested in a file in
  `docs/live-sessions/` that the owner answers there. Never wait idle for one.
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
  research session turns into a queue item and removes once it is answered. A research batch makes the parity
  and citation changes the documentation check requires of what it did to
  the spec, and changes no other code apart from `tools/`. A tooling batch
  (extractor, Ghidra scripts, inventory export, the emulator harness in
  `tools/emu/`, live session measurements, headless runner, fixture harness)
  needs no decision.
- Commit messages end with a `Spec:` trailer naming the entries created or
  changed, any commit that changes a row's status adds `Parity:`, and any
  that closes queue items adds `Queue:` with their IDs.
- A claim moves from an `unknown` listing (or `sourced` from a document),
  through competing readings kept in its entry's Open questions, each with a
  queue item, to a description at `supported` once direct evidence (the code
  that produces the behaviour) settles it, then `established` by a complete
  reading of the code, or, only where it depends on something the code does
  not decide, when a run or a tester's capture of the original agrees.
  Circumstantial evidence never raises a status. Contradicting evidence makes
  it `disputed`, and a wrong claim is superseded, never deleted. The
  protocol's "The life of a claim" section has the details.
- `docs/HANDOVER.md` is the current state of work outside any goal, at most
  200 lines, rewritten at the end of every session that works under no goal,
  and names items and entries by ID without saying what research found.
  `docs/goals/` holds one file per running goal, which claims its areas and
  has a handover of its own for sessions under it. `docs/DECISIONS.md`
  records the owner's decisions and moves its oldest entries to
  `docs/decisions/` before it passes 1,000 lines. A session ends by
  committing its handover on its own and pushing only when explicitly requested; half-done work
  never goes into a batch commit.
- Progress is what scripts compute: parity totals, entries by status,
  executable and file coverage, queue sizes. Never a hand-written percentage.
  Executable coverage is measured against the function inventories,
  `coverage/<build ID>/<manifest path>.tsv` (the established `CD:` inventory uses the `CD/`
  directory in this configured project), one for each file the analysis reads. An inventory holds only each function's start address, its size, and
  optionally a name the researcher gave it and why it is out of scope, never
  code, bytes, strings, constants or names that came from the original, so it
  is committed.

The procedures are skills in `.claude/skills/`: `runtime-access`,
`plan-work`, `start-session`, `research-item`, `implement-rows`,
`triage-report`, `live-session` and `end-session`. For a `/goal`, write the goal file with
`plan-work`, keep to its scope, and end every batch with the status block the
skills print.

### Local research and runtime constraints

Static work precedes emulated calls and owner runs within the protocol's order.
A live-session item needs its own static attempt or asks to confirm a static
reading. Never wait idle for an owner run or tester report.

The delivered Unicorn harness in `tools/emu/` calls one declared resident
function of `DSUN.EXE` without starting the game or DOSBox. Its current contract
supports far roots without arguments; raw memory and argument seeding require
supported named parameter/layout bindings before the tooling can admit them.
It cannot reach the FBOV overlay pack. Use `docs/RUNTIME.md` and
`tools/emu/README.md` for the current capabilities and limits.
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

The project follows the [methodology](vendor/upstream/methodology.md) and
the [documentation standard](vendor/upstream/documentation-standard.md). This
section and the next two summarize them; where they differ, the pages win.

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

A complete reading also covers what the standard's
[Complete readings](vendor/upstream/documentation-standard.md#complete-readings) (lines 191-291)
and [Findings](vendor/upstream/documentation-standard.md#findings) (lines 606-680) sections
list, among them: two addresses are the same storage only where the reading
shows the segment each is formed in and accessed through (a BP offset read
through DS is the caller's stack only where DS equals SS there); a stored
call target is followed through every part it carries, such as an object
adjustment or two words that form one far pointer; a byte stored into a word
read whole names what writes the other byte; an allocation keeps apart the
bytes requested, the width they are computed in, the allocator's unit, the
header's size and the range later written, and a failed request may leave
state changed; the number of outputs a procedure can produce is bounded on
its own, apart from each input's bound; a return value is followed into each
caller at the width it is tested; cleanup is read once per path into it; and
an error passed back through recursion is traced to what can produce it. A
finding that a function has no other callers checks the analyzer's list with
a second search that does not depend on function boundaries, and one about a
dispatch table reads how the input becomes an index and what bounds it before
naming which input selects which entry. An `offset` into overlay code lies
wholly inside a row of its build's Code ranges section. Bytes in an executable read as data use `kind: file-data` and shipped-file offsets; unpacker-written bytes outside the load image add `unpacked: true` and use unpacked-file offsets. File-data locations cannot support Code ranges rows.

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
decompiler, instruction, or Version Tracking exports. The function inventories
in `coverage/` are the one export that is committed, and only with the columns
the planning section above allows.

Evidence lives in the spec, not in the code that relies on it. An address,
offset or constant that a code comment, test or commit message gives as evidence
must already be recorded in an entry it cites, directly or in the evidence of an
entry that one cites; when none records it, write that finding first, in the
same research batch. The documentation check enforces this for the neutral
names (`fn_…`, `g_…`) a code comment gives, and for plain `0x…` addresses once
the CI job gives the image's range (see `docs/VALIDATION.md`). Before taking a
new ID, look for it on the open pull request branches as well as `main`,
because parallel branches each take the next free number and the check sees
only one branch:

```sh
git fetch origin
git grep -l <ID> $(git for-each-ref --format='%(refname)' refs/remotes/origin)
```

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
with a Default of `off`, `on` or `mandatory`. A deviation may be `on` or
`mandatory` when its Justification argues that the rebuild's behavior is
strictly better, or that it is a small judgement call that makes the game
better to play, such as keeping precision the original threw away or pacing by
a fixed clock where the original followed the speed of the machine, and that
touches nothing players build strategies around. A change some players would
reasonably prefer the original's way, as a matter of taste or because it
changes results players notice, gets a setting that starts `off`, with the
original's behavior. A deviation with no setting is `mandatory`, and its
Justification also says why the original's behavior is not worth a setting.
The fix of an unintended bug that players do not rely on is `on` without one.
A quirk that may be deliberate or that players rely on never qualifies, so its
deviation starts `off`. The validation suite runs with every setting
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

## Durable narrative documentation

Keep changing inventory totals out of narrative documentation: test-case, file, line and imported-asset counts belong in generated reports or validation logs. Keep numbers that define behavior, constrain validation, support evidence or justify a decision. A dated measurement belongs in prose only when that context needs it. Refer to the generating command instead of maintaining a copied total.

## Architecture boundaries

- `<Project>.Core`: deterministic rules and serializable state. No MonoGame, no
  file-format parsing, no I/O.
- `<Project>.Resources`: bounded binary parsing and original-content contracts.
  No MonoGame.
- `<Project>.Game`: MonoGame DesktopGL presentation, and the only project that
  may depend on both of the above.
- `<Project>.Extractor`: separately runnable licensed-source verification and
  transactional asset extraction over `Resources`.
- `<Project>.Inspect`: read-only tooling over the original's media.
- `<Project>.Tests`: architecture, safety, and behavioral tests.

Game-independent readers and runtime helpers come from the `RefurbishedDinosaurs.*`
NuGet packages, pinned at an exact version in `Directory.Build.props`: media
sources, edition identification and legacy formats from
`RefurbishedDinosaurs.LegacyFormats`; asset-pack manifests, staging, safe paths,
per-user locations and startup failure reporting from `RefurbishedDinosaurs.Core`.
Use a package type before writing a local one, and keep game-specific formats,
names and rules in the projects above; the
[shared runtime libraries guide](https://github.com/kibertoad/refurbished-dinosaurs-toolkit/blob/main/docs/runtime-libraries.md)
says what each package covers.

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

Where `Core` computes a rule with a different algorithm from the entry's
procedure, tests compare the state and outputs a later call reads, not only
the result: for a search with a work queue, a node queued twice, a stored
score that improves while an older queue entry waits, and a search stopped at
its step limit and resumed. Wiring a rule into `Game` is tested through its
intermediate states: one shared value followed through input, `Core`
updates, presentation, and a save and restore, with distinct values per axis;
what a second actor sees at each `# visible:` point; reservations across a
table refresh and a save; a requester's rejection apart from a failed route.
A checkpoint or replay API states what its identity covers (every field and
behaviour-driving resource that decides how play continues, hashed in a
stated, versioned encoding), rejects a mismatched checkpoint without changing
the host, says whether a snapshot may be restored more than once, and never
drops unsaved state such as a paused path search silently. These tests
compare the rebuild with the spec or with itself, so none of them validates a
parity row; see the protocol's
[Implementation batches](vendor/upstream/work-protocol.md#implementation-batches) (lines 162-178)
and [Checkpoints and replay](vendor/upstream/work-protocol.md#checkpoints-and-replay) (lines 256-264).

Each rule ships with fast-gate tests over synthetic state. The rule itself is
usually a static class over the serializable state type, called by `Game`.
When a bug is traced to branch logic in `Game`, extract the rule into `Core`,
pin every branch with a test, and fix it there.

Determinism is a feature: identical commands and seed must produce identical
state, because saves, replays, and parity validation depend on it. The
generator takes the ID of the rule making each draw and offers a hook that
lets a test observe every draw; nothing but tests uses the hook. Every
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

Before further executable analysis, run `./tools/Bootstrap-Project.ps1
-ValidateFactsOnly`. It refuses unestablished latest-patch provenance or missing
recorded executable identity. Version 1.1 is recorded as the latest official
version, with the analysis executable's path, length and XXH3-128; see
`docs/SOURCE-EDITIONS.md`. Do not repeat the patch investigation unless the owner
asks. Original-free tooling and rebuild validation remain
available while the gate is closed.

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

Enable the pre-commit hook once in each clone, before the first commit, with
`git config core.hooksPath .githooks`, and do not bypass it with
`--no-verify`. It runs the gate's node checks (`tools/Invoke-NodeChecks.mjs`)
on the staged tree in under a second, so a spec, queue or checker-pin problem
fails before the commit instead of in CI.

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

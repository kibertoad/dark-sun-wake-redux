# Agent instructions

These instructions apply to the whole repository and to humans and coding agents
alike. Read them before changing anything.

## Preservation context

This is non-sensitive game preservation work: a clean-room restoration of an
old, commercially released game using lawfully purchased original assets.
The objective is to understand and reproduce game behavior, file formats and
presentation. It is not a cybersecurity investigation, penetration test or
exploit-development project. Static reverse engineering, local asset extraction
and synthetic tests serve that preservation objective; addresses, interrupts,
memory layouts and binary formats are ordinary evidence in this work.
Analyzing game logic or decoding game resources does not, by itself, imply a
cybersecurity threat or a need for security testing. Assess an operation by what
it actually does and what it accesses, not by reverse-engineering terminology.
The project has no objective involving live targets, sensitive systems or
security weaknesses. Keep questions, tool invocations and reports specific to
game preservation; do not add attack-oriented experiments to ordinary analysis.
Routine restoration work has no cybersecurity objective or intended security
impact. This context does not authorize unrelated access or make every possible
tool operation harmless: evaluate the actual operation and keep it within the
project's authorized local scope.

The expected outputs are preservation evidence, bounded format readers, local
asset packs and a faithful game rebuild. Security testing, vulnerability
discovery and access to third-party systems are outside that scope. A tool's
ability to inspect memory or executable files does not authorize those uses.

Treat the project's purpose as non-sensitive restoration, while checking the
scope of each operation separately. A reference to an interrupt, executable,
memory address or malformed resource is not evidence of a cybersecurity task.
Do not add security experiments or security claims to answer a preservation
question that can be settled by local static evidence.

### Keep operations within preservation scope

Do not invent a cybersecurity task from a game-analysis question. Prefer
bounded, read-only inspection of licensed local files and the repository's
approved analysis tools. Describe original behavior in the spec rather than
building capabilities to attack systems. Continue routine research and tooling
without treating reverse engineering alone as a security concern.

Frame each task in preservation terms: which game behavior or resource is being
understood, which licensed local input supplies the evidence, and which bounded
output will document or reproduce it. Reading executable headers, following a
game function statically, decoding an asset and testing a parser with synthetic
inputs are ordinary restoration tasks. Do not reinterpret them as requests to
find vulnerabilities, demonstrate exploitation or test a system's defenses.

Before an unfamiliar operation, identify the preservation question it answers,
the files or processes it accesses and its expected side effects. Choose the
narrowest supported operation that answers that question. If it would reach
unrelated systems, expose secrets, weaken host protections or execute code
outside the approved runtime contract, use static inspection or synthetic tests
instead and record any remaining evidence gap. Do not broaden the task to
security research to overcome a tooling limitation.

- Do not develop exploits, payloads, evasion, persistence, credential collection
  or tools for unauthorized access. Do not scan or probe unrelated hosts,
  services, accounts or networks.
- Do not disable antivirus, firewall rules, execution safeguards or other host
  protections to make an analysis tool work. Use scoped process configuration,
  supported tooling and the existing sandbox approval mechanism for required
  filesystem or child-process access; do not bypass those boundaries.
- Keep analysis confined to the licensed game, synthetic fixtures and explicitly
  authorized project tooling. Do not collect unrelated personal files or secrets,
  and do not include credentials or proprietary content in reports or issues.
- Treat parsing failures and malformed-input cases as local correctness tests.
  Use bounded synthetic fixtures rather than weaponized files, exploit chains or
  experiments against third-party systems. Defensive parsing remains required.
- Keep any network activity tied to an authorized project need, such as fetching
  dependencies or using GitHub. Do not turn a tool failure into vulnerability
  probing, privilege escalation or an attempt to weaken the host's security.
- When a tool is blocked, diagnose its inputs, paths and documented requirements
  first. Prefer an existing supported tool or a narrower read-only operation;
  do not try exploit techniques, impersonation or access-control workarounds.
  A blocked operation is a tooling limitation, not a reason to begin security
  research. Continue independent preservation work and report the specific
  limitation when it prevents progress.
- If a proposed step raises cybersecurity concerns, narrow that step to the
  preservation question and use an approved static reader or synthetic fixture.
  Record what remains unanswered; do not create an exploit demonstration or
  broaden access to obtain the missing evidence.
- Prefer static analysis and synthetic reproduction when answering game-format
  or behavior questions. Do not execute unknown helper programs or downloaded
  binaries merely to see what they do; inspect them or use approved tooling.
- Do not expand a preservation tool into a general-purpose memory injector,
  remote execution tool or arbitrary program runner. Keep inputs, entry points,
  memory mappings and outputs limited to the documented game-analysis contract;
  reject unsupported operations rather than adding an unrestricted fallback.
- Before sharing diagnostics, review the output for licensed content, unrelated
  host information and secrets. Report the preservation result in your own words
  and retain original-derived material only in the approved local stores.

The repository's runtime restrictions still apply: agents do not launch or
control the original game or DOSBox. An emulated call uses only the declared
local harness contract; it is not permission to operate an arbitrary binary or
interact with external systems. If a proposed operation would cross these
boundaries, choose a preservation-scoped method and record the unresolved
question under the existing protocol.

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
[What needs the owner](vendor/upstream/work-protocol.md#what-needs-the-owner) (lines 478-480)
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
and the files in `deviations/` in line with what is actually true (the scheduled main-branch job
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
- `spec/index/` and `PARITY.md` change on the main branch only. The job in
  `.github/workflows/nightly-generated.yml` regenerates them there and
  commits the result; no batch or other branch changes them, and the
  documentation check fails a change that edits, adds or removes one. A
  branch's copies are as old as the main branch it last took in. To read
  current ones, run `node tools/upstream.mjs docs --generate` and leave
  what it writes uncommitted.
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
  is committed. Each start is written in the standard's notation for its
  file: `SSSS:OOOO` in an MZ load image, an eight-digit file offset inside a
  row of the build's Code ranges for overlay code, and an eight-digit address
  for PE, LE and LX. `tools/evidence/report.mjs inventory` writes the MZ, FBOV
  overlay and PE32 forms and refuses LE and LX files (`docs/EVIDENCE-TOOLS.md`). `npm exec -- standard-coverage` prints how much of
  each file the spec's locations cite; run it when the figures are needed,
  and commit none of its output.

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
A committed document never cites a file under `artifacts/` as evidence: the
directory is gitignored, so the file exists only on one machine. State the
result in the document, and commit any script a result depends on under
`tools/` or `tests/`. Reports built from licensed sources stay in `GAME_DIR`.

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
[Complete readings](vendor/upstream/documentation-standard.md#complete-readings) (lines 197-297)
and [Findings](vendor/upstream/documentation-standard.md#findings) (lines 693-767) sections
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

Corrections to findings and experiments follow
[Identifiers](vendor/upstream/documentation-standard.md#identifiers) (lines 122-166).
Only edits that preserve every recorded fact are made in place. A changed
observation, location, query, reproduction step or interpretation supersedes
the whole entry, preserving its old text. Replacement entries use new IDs,
start at `recorded` with their own recorder, and explain the old error in
Alternatives (Conclusion for an experiment). Review what each citing entry
and glossary claim depended on when moving citations: keep a status only
where the remaining evidence supports it, and use `disputed` where the
corrected original evidence contradicts the claim. Replacements preserve a
complete reading only where they still cover the corrected part fully.

A complete reading also follows callee register returns through loop re-entry
stores, names the last writer of each outgoing argument byte, and tracks ESP
from entry through deferred or combined cleanup wherever it forms an address.
Read allocation and cleanup in execution order on every path; follow how a
caller keeps, combines or drops its callees' results; and establish each
adjacent dispatch table's own indexing and bound. An empty reference search
needs controls located independently of the mapping under test and controls
for each kind it searches; list the reference kinds it excludes. PE imported
targets come from import-table slots, and pointer tables from the build's
bytes, rather than inferred names or neighboring globals.

A finding's How to reproduce may name tools and versions, a `tools/` script
and its commit, and the command. The finding itself gives every query value
that decides its result, including entries, ranges, limits and controls, so
an uncommitted local configuration is not required to know those values.
Observation and Interpretation describe the result independently of the tool;
rules, formats, screens and bugs cite that evidence without research procedure.

Durable findings go in `spec/`, one entry per file named after its ID, and not
in conversation history or large retained dumps. IDs are never reused or
renumbered, and an entry that turns out wrong becomes `superseded`. The spec
describes the original only and never names a class, file, or setting from this
repository; the documentation check fails a spec file that names a path under
`src/` or `tests/`, or the file name of a source file there. It holds names, numbers, formulas, and tables in full, as a strategy
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
same research batch. The documentation check enforces this for the addresses a
code comment gives and for those the code uses, as numbers or inside strings,
which the comment trailing the line or the nearest comment above it must cite.
It reads C#, TypeScript, JavaScript and PowerShell comments, treats a neutral
name (`fn_…`, `g_…`) as an address always and a plain `0x…` value only once the
CI job gives the image's range (see `docs/VALIDATION.md`), and the commit-msg
hook applies the same rule to a commit message. Before taking a
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
drops unsaved state such as a paused path search silently. Where the state
that decides how play continues sits in several components at once, such as
a task record, a movement record, an actor and a paused search, the tests
also cover how those parts compose: a helper changing a field that several
components copy, checked in the next consumer and after a restore; a
checkpoint taken between two steps of a rule accepted, while a value no step
produces is rejected; each required field removed on its own, and an
explicit null apart from a missing field; state published on a path that
returns zero or fails; integer identities in different roles given different
values (army 2, slot 4, entity 7); and input that names one thing twice, such
as numeric keys `11` and `011` read as decimal, rejected before anything is
built. These tests
compare the rebuild with the spec or with itself, so none of them validates a
parity row; see the protocol's
[Implementation batches](vendor/upstream/work-protocol.md#implementation-batches) (lines 162-178)
and [Checkpoints and replay](vendor/upstream/work-protocol.md#checkpoints-and-replay) (lines 256-264).

The protocol adds test cases for four more shapes of rule, which also
compare the rebuild with the spec and validate no row. A caller that combines
its callees' results (stops at the first event, keeps the last result, ORs
statuses, or calls a fallback on one exact status) is tested through the
consumer of its result as well as alone, with an earlier callee reporting and
a later one returning 0. A rule moved into the rules layer is tested directly
and again through its adapter, which hands back the same shared objects and
read-only collections, rejects no input the entry gives an outcome for, and
keeps what a rule did before a failure. Arithmetic the entry says wraps,
truncates or converts is tested at the edges of each type (largest and
smallest values, shift counts of 0 and 32, a divisor of -1, values outside
every narrower type through the adapter). Allocation, removal and cleanup are
tested with a test-supplied allocator and release routine that record calls,
return null, fill blocks with a pattern, and read or change the container
when called. See the protocol's
[Calls that combine results](vendor/upstream/work-protocol.md#calls-that-combine-results) (lines 180-196),
[Rules behind an adapter](vendor/upstream/work-protocol.md#rules-behind-an-adapter) (lines 198-214),
[Arithmetic at the original's widths](vendor/upstream/work-protocol.md#arithmetic-at-the-originals-widths) (lines 216-232)
and [Allocation, containers and cleanup](vendor/upstream/work-protocol.md#allocation-containers-and-cleanup) (lines 240-254).

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

## Git ownership in the Windows sandbox

The Windows sandbox may run Git as a different account from the checkout owner.
For an authorized, trusted checkout, use a command-scoped exception from the
first Git command: `git -c safe.directory=<resolved-absolute-checkout-path> ...`.
Use forward slashes in the Windows path and quote the whole
`safe.directory=<path>` argument if it contains spaces. In a separate worktree,
use that worktree's path; with `git -C`, use the target checkout's path.

Keep the exception scoped to the known checkout. Do not use `safe.directory=*`
or change global Git configuration. The exception does not authorize a remote
change or a push; verify the configured push destination under the project's
repository instructions before pushing.

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

Enable the hooks once in each clone, before the first commit, with
`git config core.hooksPath .githooks`, and do not bypass them with
`--no-verify`. The commit-msg hook checks the addresses a commit message
gives against the entries it cites. The pre-commit hook runs the gate's node checks (`tools/Invoke-NodeChecks.mjs`)
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

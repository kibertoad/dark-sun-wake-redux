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

Small, self-contained changes — fixing a bug, tightening a test, editing prose,
finishing a slice the plan already covers — do not need a new plan. When in
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

**3. Record what is known about the original.** Replace the instructional text in
`docs/SOURCE-EDITIONS.md`, `docs/ORIGINAL-FORMATS.md`,
`docs/RULES-AND-EVIDENCE.md`, `docs/UI-ATLAS.md`, and `docs/FIDELITY.md` with
game-specific content, keeping each document's structure and confidence
vocabulary. State confidence honestly; `unknown` is a valid answer and a
plausible-sounding guess is not.

**4. Make extraction real.** Replace the sample manifest under
`src/<Project>.Extractor/source-manifests/` with one fingerprint manifest per
supported edition, extend `tools/repository-policy.json` with the extensions the
original actually uses, and make a missing or unsupported source produce an
actionable error rather than a crash. The separately runnable Extractor verifies
a licensed source and transactionally creates a complete local asset pack; the
Game consumes only that verified pack.

**5. Build the first vertical slice.** Follow the approved plan. Prefer a thin
end-to-end slice — identify, import, start, show something real, quit cleanly —
over broad but unplayable systems.

**6. Verify and hand over.** Run the commands below, update the README status
table and `docs/PARITY-MATRIX.md` to match what is actually true, and tick off
`docs/BOOTSTRAP-CHECKLIST.md` as decisions are captured elsewhere.

## Rules that never bend

- **No original content in Git, ever.** No assets, executables, archives, save
  files, screenshots, or data extracted from them. `UserContent/`,
  `analysis/original/`, and `reference/original/` are local-only, and
  `tools/Verify-Repository.ps1` enforces this. Synthetic fixtures go under
  `tests/fixtures/synthetic/`.
- **Clean room.** Do not copy original source, decompiler output, or
  disassembly into this repository. Describe behavior and data formats in your
  own words, with the evidence recorded in `docs/RULES-AND-EVIDENCE.md` and
  `docs/GHIDRA.md`.
- **Evidence before claims.** A rule, format field, or parity claim needs a
  reproducible test or a recorded observation. Conflicting sources are preserved
  as a conflict, not silently resolved.
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
- **CI never needs proprietary content.** Every test and packaging check must
  pass on a machine that has no copy of the original game.
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

Treat static analysis as evidence, not a search-engine oracle. Reverify the
approved executable's exact size and SHA-256 before interpreting a new address
or reference. Ask one narrow player-visible question at a time and record the
edition, tool versions, query, result, competing interpretations, and confidence
in `docs/GHIDRA.md`.

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
evidence question: make a narrow static-analysis query only after verifying the
approved executable fingerprint, and obtain a bounded capture that shows each
corner/state required. Record measured geometry, colours, and confidence in the
UI evidence documents, add synthetic tests for those measurements, and keep
the capture itself outside Git. Do not claim pixel parity until the native
widget treatment is observed.

Coding agents must never launch, control, capture, or stop DOSBox on their own.
When an evidence question requires an original-game observation, give the
repository owner an exact, bounded screenshot or capture checklist and wait for
the owner to confirm that the requested material has been produced with
DOSBox's built-in Ctrl+F5 screenshot command. After that confirmation, parse
the configured DOSBox screenshots folder and inspect only images whose file
timestamps fall within either an owner-confirmed capture window or an
owner-authorized local date-and-hour slot (including UTC offset). An authorized
hour means the half-open local interval from that hour through the next hour.
Do not operate the DOSBox window, invoke Ctrl+F5, use another screen-capture
mechanism, or infer that an unconfirmed request, an old capture, or the
presence of a DOSBox process means the requested observation was performed.
Before recording or relying on a screenshot's semantic identity—such as a
named screen, actor, turn, action, or transition—ask the owner to confirm that
proposed label. Geometry/pixel measurements may be recorded as provisional
observations without assigning that identity. If neither a timestamp window nor
a date-and-hour slot is authorized, ask the owner before inspecting any
candidate image.

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

## Remote publication authorization

Pushing is an external publication action, not an implied consequence of making
a local commit. Before the first push to a remote, and again after any remote
URL, push URL, branch-tracking, or account change, establish the exact target
with read-only Git commands:

```powershell
git remote -v
git branch -vv
git config --get-regexp '^remote\\.origin\\.(url|pushurl|fetch)$'
git rev-list --left-right --count origin/main...main
```

Show the owner the resulting fetch/push URL, target ref, and ahead/behind
counts. The owner must explicitly approve that exact push destination and ref
after seeing those facts. Record the approval in `docs/RELEASING.md` with the
URL, branch, date, approving owner, and scope (for example, ordinary
fast-forward pushes from `main`). Do not treat a similarly named remote, an
account name, a prior fetch, or a generic request to "push" as approval for a
different URL or branch.

### Establishing the repository owner

A Git remote URL establishes an endpoint, not its author, owner, or the
current account's permission to publish. Before recording a GitHub destination
as owner-approved, verify all of the following read-only facts and show the
complete results to the owner:

1. The normalized fetch URL and any `remote.origin.pushurl` exactly name the
   approved GitHub `OWNER/REPOSITORY`; a missing `pushurl` means the fetch URL
   is also the push destination. A redirect, fork, look-alike owner name, or
   different protocol/path is a fresh target, not an equivalent one.
2. The authenticated GitHub identity and effective repository permission match
   the owner-approved publishing account. When GitHub CLI is available, use:

   ```powershell
   gh auth status --hostname github.com
   gh repo view OWNER/REPOSITORY --hostname github.com --json nameWithOwner,url,owner,isPrivate,viewerPermission
   ```

   The result must name the same `OWNER/REPOSITORY` and report a permission
   capable of the intended ordinary push. If the CLI is unavailable, have the
   owner inspect the signed-in GitHub account and repository Settings/Access in
   the browser and explicitly attest to the account and write permission; do
   not substitute a public clone/fetch test for that proof.
3. The owner confirms that the displayed GitHub owner is their intended
   personal account or organization and that its `main` branch is the intended
   publication branch. `git ls-remote`, a successful fetch, or a familiar
   repository name only proves reachability, not ownership or authorization.

Record only the non-secret result: canonical URL, `OWNER/REPOSITORY`, branch,
verified account name, effective permission, verification date, approving
owner, and permitted scope. Never place authentication tokens, cookies,
credential-helper data, private keys, or screenshots containing them in the
repository or an approval record. If any identity or permission differs, stop
and obtain fresh owner direction instead of changing the remote or attempting a
push.

Before each later push, re-run the same read-only checks and require an exact
match with the recorded destination and scope. A changed URL, push URL,
tracking branch, unrecorded force push, or ambiguous owner instruction requires
fresh approval. Repository documentation is an audit trail, not a credential:
it does not bypass the execution environment's separate network-authorization
or remote-protection checks. When that environment asks for confirmation, state
the exact destination and ref and obtain the owner's current approval rather
than retrying or working around the block.

## Definition of done

A change is finished when the solution builds, `./tools/Test.ps1` passes, new
behavior has tests that do not need original content, the documents that assert
status (`README.md`, `docs/PARITY-MATRIX.md`, `docs/FIDELITY.md`) match reality,
and the plan's open questions have been updated with whatever the work settled
or newly raised.

Commits describe the change and its evidence, not the tooling that produced it.

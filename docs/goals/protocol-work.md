# Protocol work

## Condition

Continue the owner's active thread objective: work according to the pinned
Protocol, document results using the Standard, report upstream improvements or
concerns after checking existing issues and add relevant details to duplicates,
and keep committing complete work without pushing to main.

The owner has supplied no finite project-completion exit or turn limit. A
completed maintenance batch does not complete this ongoing objective or replace
it with a narrower goal. Keep its unfinished work and next actions here.

## Scope

Research-side repository workflow and tooling: local session skills, goal
discovery and handovers, protocol conformance, validation and upstream reports.
Research areas: EXE, beginning with Q-EXE-002 and FMT-EXE-006. Research batches
may change EXE entries, queue/EXE.md and parity/EXE.md, with generated indexes
and PARITY.md kept consistent. The EXE launch-reference scope also covers BLD-GOG-EN-1.1 inventory and
wrapper-provenance corrections needed to cite the studied distribution files.
Other areas are read-only. Further research
areas are added only after checking the shared clone's authoritative goal
claims and other sessions' work.

## Must not touch

Other sessions' worktrees, uncommitted work and commit history; original-game
runtime or DOSBox; `src/`, gameplay implementation, claims or parity rows
outside EXE, CONFIG/SCRIPT research and the separate upstream gap acceptance
ledger. Do not push to main. Native and emulated game execution are outside
this goal's batch-file research scope; read original files statically only.
An owner-approved history repair remains separate from this maintenance scope.

## Dead ends

- The earlier shared-checkout amend replaced a concurrent research commit's
  message. Use this isolated worktree, commit new work only, and do not amend
  another session's HEAD. The pending repair is recorded in docs/HANDOVER.md.
- Git Bash could not create its hook snapshot in the inherited temporary
  directory. Set TMPDIR inside Git Bash to this worktree's writable
  artifacts/hook-tmp; keep the pre-commit hook enabled.

## Handover

- Stage: Slices. The ongoing protocol objective remains active on this local
  goal branch. Workflow tooling batches and the EXE research batch are complete.
- Last full gate: 2026-10-07, assetless Test.ps1 -NoRestore passed. This
  batch's logs are artifacts/exe-batches-full-gate.log and
  artifacts/exe-batches-docs.log. Explicit main-base documentation checking
  passed; existing argument-check skips remain. Pre-commit checks passed.
- Unfinished: none in this worktree. No pushes were performed. Research
  follow-ups remain Q-EXE-004, Q-EXE-005, Q-EXE-006 and Q-EXE-001.
- Environment: use the parent checkout's portable PowerShell and locked
  evidence-python interpreter. Clear GAME_DIR and
  NoDefaultCurrentDirectoryInExePath. TEMP/TMP must belong to the account
  actually executing the process. For hooks export this worktree's
  artifacts/hook-tmp as TMPDIR inside Git Bash. Keep hooks enabled.
- Process audit: reusable MSBuild nodes and uncertain or unrelated processes
  were preserved. No confirmed session orphan required stopping.
- Blockers: the parent checkout's history-message repair awaits owner
  approval and remains recorded in docs/HANDOVER.md. Do not rewrite shared
  history; this does not block isolated work.
- Upstream: toolkit issue 325 records the bounded MODE2 content-source
  request after duplicate checks. Template issues 80 and 82 retain the
  workflow and temporary-directory reports. No upstream delivery is claimed.
- Next: recheck shared goal claims and select Q-EXE-004; then Q-EXE-005 and
  Q-EXE-006 in static-first order. Q-EXE-001 needs a new tool, evidence or
  reading before retry. Keep CONFIG/SCRIPT and the gap ledger excluded.

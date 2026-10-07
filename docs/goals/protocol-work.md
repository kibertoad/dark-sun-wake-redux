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
and PARITY.md kept consistent. Other areas are read-only. Further research
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

- Stage: Slices. Research-side repository workflow maintenance so far; no
  game spec, parity, queue or behavior changes in these tooling batches.
- Last full gate: 2026-10-07, assetless Test.ps1 -NoRestore passed in this
  isolated worktree. Current logs and known argument-check skips are in
  docs/VALIDATION.md; pinned digests/links and explicit main-base comparison
  passed. The current rule-template checker controls passed too.
- Unfinished: none in this worktree. All completed work remains local for
  owner review; no push was performed.
- Environment: PowerShell 7 is in the parent checkout's
  artifacts/pwsh7/runtime/pwsh.exe; select its locked evidence-python
  interpreter with EVIDENCE_PYTHON. Clear GAME_DIR and
  NoDefaultCurrentDirectoryInExePath. TEMP/TMP belong to the account actually
  executing the process. For hooks export this worktree's artifacts/hook-tmp
  as TMPDIR inside Git Bash; keep the hook enabled. Reusable MSBuild nodes
  and other projects' processes were preserved; no confirmed orphan needed
  stopping in the post-commit audit.
- Blockers: the parent checkout's metadata repair still awaits owner
  approval; docs/HANDOVER.md records it. It does not block isolated work.
  Do not rewrite main or another session's commits.
- Upstream: template issue 80 has local discovery, research-summary and
  template acceptance; issue 82 has executing-account evidence. Local
  acceptance does not prove upstream template adoption or package delivery.
- Next: recheck the shared clone's goal claims and other sessions' work,
  then select Q-EXE-002 (FMT-EXE-006) if unclaimed. Add EXE to this goal's
  scope and adjust its Must not touch in a separate claim commit before
  researching it. Retain CONFIG/SCRIPT and the upstream gap ledger exclusions.

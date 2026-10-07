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
Further research areas are added only after checking the shared clone's
authoritative goal claims and other sessions' work.

## Must not touch

Other sessions' worktrees, uncommitted work and commit history; original-game
runtime or DOSBox; `src/`, gameplay, spec claims, parity statuses, CONFIG/SCRIPT
research and the separate upstream gap acceptance ledger. Do not push to main.
An owner-approved history repair remains separate from this maintenance scope.

## Dead ends

- The earlier shared-checkout amend replaced a concurrent research commit's
  message. Use this isolated worktree, commit new work only, and do not amend
  another session's HEAD. The pending repair is recorded in docs/HANDOVER.md.
- Git Bash could not create its hook snapshot in the inherited temporary
  directory. Set TMPDIR inside Git Bash to this worktree's writable
  artifacts/hook-tmp; keep the pre-commit hook enabled.

## Handover

- Stage: Slices. Research-side repository workflow maintenance; no game
  spec, parity, queue or behavior changes.
- Last full gate: 2026-10-07, assetless `tools/Test.ps1 -NoRestore` passed
  in this isolated worktree. Logs and known checker argument-check skips are
  recorded in docs/VALIDATION.md; pinned rule and link verification passed.
- Unfinished: none in this worktree. Local changes remain on the goal branch
  for the owner's review and merge; no push was performed.
- Environment: use PowerShell 7 from the parent checkout's
  artifacts/pwsh7/runtime/pwsh.exe and its locked EVIDENCE_PYTHON interpreter.
  Clear GAME_DIR and NoDefaultCurrentDirectoryInExePath. TEMP/TMP must belong
  to the account actually executing the test process: escalated commands use
  the owner account, ordinary sandbox commands use the sandbox account.
  For hooks, export this worktree's artifacts/hook-tmp as TMPDIR inside Git
  Bash; keep the hook enabled. Reusable MSBuild nodes were left running;
  the post-commit process audit found no confirmed orphan to stop.
- Blockers: the parent checkout's commit-message repair remains pending
  owner approval and is recorded in docs/HANDOVER.md. It does not block this
  isolated goal. Do not rewrite main or another session's commit history.
- Upstream: existing template issues 80 and 82 have this consumer's
  discovery and execution-account evidence. These local controls do not
  establish that the upstream template has adopted all of issue 80.
- Next: audit the remaining issue-80 summaries against the pinned local
  rules; report new relevant details to existing issues before opening any
  duplicate. Add any needed new research area to this branch's goal scope
  in a separate claim commit before taking its queue item.

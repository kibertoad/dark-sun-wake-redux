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

- Stage: Slices. This goal changes workflow tooling only.
- Last full gate: inherited 2026-10-07 assetless validation in the parent
  checkout; this new worktree has not yet run its own full gate.
- Unfinished: goal discovery and session skills still summarize the older
  main-first claim rule instead of the pinned protocol's no-push fallback.
- Blockers: the parent checkout's commit-message repair awaits owner approval;
  it does not block isolated workflow maintenance.
- Next: align start-session, plan-work, end-session and docs/goals/README.md;
  verify the two-worktree discovery and deleted-tip cases; add relevant
  downstream evidence to template issue 80 and run the full local gate.

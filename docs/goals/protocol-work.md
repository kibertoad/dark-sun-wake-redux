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
Research areas: EXE, including the FMT-EXE-006 follow-up questions. Research batches
may change EXE entries, queue/EXE.md and parity/EXE.md, with generated indexes
and PARITY.md kept consistent. The EXE launch-reference scope also covers
BLD-GOG-EN-1.1 inventory and
wrapper-provenance corrections needed to cite the studied distribution files.
It includes SRC-DOSBOX-GOG-0742 and its EXE question citations for the shipped
interpreter source archive; source-to-binary correspondence stays explicit.
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
  goal branch. Completed workflow tooling and EXE research are committed.
- Last full gate: 2026-10-07, assetless Test.ps1 -NoRestore passed.
  Logs: artifacts/exe-lookup-full-gate.log and artifacts/exe-lookup-docs.log.
  Documentation generation and explicit main-base checking passed;
  existing argument-check skips remain. Pre-commit checks passed.
  The last full source-listing reconciliation remains
  artifacts/exe-dosbox-pe-listing.log; this batch changed no manifest.
- Unfinished: no tracked work remains. EXE follow-up items remain Q-EXE-005,
  Q-EXE-006, Q-EXE-007, Q-EXE-008 and Q-EXE-009; Q-EXE-001 retains its
  retry requirement. Local static-analysis reports and the saved interpreter
  Ghidra project remain in GAME_DIR/analysis/exe-batches for continuation.
- Environment: use the parent checkout's portable PowerShell and locked
  evidence-python interpreter. Clear GAME_DIR and
  NoDefaultCurrentDirectoryInExePath. TEMP/TMP must belong to the account
  actually executing the process. For hooks export this worktree's
  artifacts/hook-tmp as TMPDIR inside Git Bash. Keep hooks enabled. Reuse the
  saved Ghidra program with -noanalysis. For combined script directories,
  follow the corrected literal quoting in docs/GHIDRA.md; both directories
  were verified through the Windows launcher. Original-program execution
  remains prohibited.
- Process audit: reusable MSBuild nodes and other sessions' or uncertain
  processes were preserved. No confirmed session orphan required stopping.
- Blockers: the parent checkout's history-message repair awaits owner
  approval and remains recorded in docs/HANDOVER.md. Do not rewrite shared
  history; this does not block isolated work.
- Upstream: template issue 83 records the Windows combined-script-path
  launch defect and the successful two-directory control. Template issues
  80 and 82, and toolkit issues 325 and 327 retain the earlier reports.
  No upstream fix delivery is claimed.
- Next: recheck shared goal claims, then Q-EXE-009 using FND-EXE-015 and
  FND-EXE-016 for normalization/index producers, drive-object provenance,
  PATH writers/admission and remaining flag/EXIT paths; Q-EXE-006 using
  FND-EXE-013 for batch-input/parser and helper/cleanup coverage.
  Continue Q-EXE-008, Q-EXE-007 and Q-EXE-005 when evidence permits. Keep
  CONFIG/SCRIPT and the gap ledger excluded; never run DOSBox or a shell harness.

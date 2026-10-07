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

The owner-requested latest-template synchronization is authorized tooling within this goal.

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
- Last full gate: 2026-10-08, assetless Test.ps1 -NoRestore passed.
  Logs: artifacts/exe-full-target-full-gate.log and artifacts/exe-full-target-docs-check.log.
  Documentation generation and explicit main-base checking passed;
  existing argument-check skips remain. Remote-base ancestry was rechecked
  and is available; explicit local main-base checking passed. Pre-commit
  checks passed.
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
- Template context: the root checkout adopted template 0b9ab9c separately.
  This goal worktree retains its verified pinned rules and dependencies; no merge
  or rebase was performed during this research batch. Toolkit issue 353 records
  the root adoption's external-source citation diagnostic.
- Process audit: reusable MSBuild nodes and other sessions' or uncertain
  processes were preserved. No confirmed session orphan required stopping.
- Blockers: the parent checkout's history-message repair awaits owner
  approval and remains recorded in docs/HANDOVER.md. Do not rewrite shared
  history; this does not block isolated work.
- Upstream: template issue 83 records the Windows combined-script-path
  launch defect and the successful two-directory control. Template issues
  80 and 82, and toolkit issues 325 and 327 retain the earlier reports.
  Template issue 86 records the reproduced goal-hook missing-identity defect;
  its narrow downstream guard has synthetic controls. No upstream fix delivery
  is claimed. Toolkit issue 111 also records the reference-type guidance concern.
  Follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6044911287.
  Related PR 339 is merged (verified 2026-10-07); package delivery and
  the released writer-control rerun remain pending.
  Toolkit issue 350 received a duplicate follow-up on inventory ownership:
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/350#issuecomment-6046763133.
- Next: recheck shared goal claims, then Q-EXE-009 using FND-EXE-015,
  FND-EXE-017 through FND-EXE-126 for larger full-width reader branches/indirect effects, remaining selected callee branches, table/slot producers and caller input admission, record producers/bounds/lifetime and callee effects, virtual targets and value-two/seven callee effects, slot/object-field and caller input producers, initialization admission/lifetime and other target producers and optional admission dispatch, floating numeric/environment contracts, indexed-handler producers and wait-pointer admission, shared continuation/reader effects and category/global producers, mapping-object virtual targets/extent, list-count writers and range-index admission, then shared byte-transfer callee effects, then PATH input-list object and remaining mapped-table producers, then saved-handler admission, prefix modifier/displacement producers and first-object target admission, shared cleanup resource targets, counter initialization/lifetime, remaining cleanup targets, startup callbacks and registration effects, shared-guard indirect writers/lifetime, dispatcher-frame admission and optional callback effects
  plus static-context/flag producer contracts, concrete dispatch targets, stream bounds and dispatcher admission, then zero-state helpers, then higher caller returns, then temporary caller ranges
  and failure-consumer/handler contracts, then
  preceding-word producers and downstream output, then
  collection initialization/lifetime,
  list-producer callee effects,
  output meanings and later cleanup,
  append-caller selector admission, object construction and aliases,
  concrete virtual targets,
  allocation callback/exceptional contracts and failure-pool lifetime,
  prefix storage/alias and virtual-target coverage, PATH admission
  and remaining flag/EXIT paths. Q-EXE-006 uses FND-EXE-013 for
  batch-input/parser and helper/cleanup coverage.
  Continue Q-EXE-008, Q-EXE-007 and Q-EXE-005 when evidence permits. Keep
  CONFIG/SCRIPT and the gap ledger excluded; never run DOSBox or a shell harness.

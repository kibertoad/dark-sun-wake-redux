# Protocol work

## Condition

Continue the owner's active thread objective: work according to the pinned
Protocol, document results using the Standard, report upstream improvements or
concerns after checking existing issues and add relevant details to duplicates,
and keep committing complete work; push only when explicitly authorized.

The owner has supplied no finite project-completion exit or turn limit. A
completed maintenance batch does not complete this ongoing objective or replace
it with a narrower goal. Keep its unfinished work and next actions here.

## Scope

The owner-requested latest-template synchronization is authorized tooling within this goal.

Research-side repository workflow and tooling: local session skills, goal
discovery and handovers, protocol conformance, validation and upstream reports.
Research areas: EXE for the game and its utilities. DOSBox complete readings are excluded. Research batches
may change EXE entries, queue/EXE.md and parity/EXE.md, with generated indexes
and PARITY.md kept consistent. The EXE launch-reference scope also covers
BLD-GOG-EN-1.1 inventory and
wrapper-provenance corrections needed to cite the studied distribution files.
Historical context includes SRC-DOSBOX-GOG-0742 and its EXE citations for the shipped
interpreter source archive; source-to-binary correspondence stays explicit.
Other areas are read-only. Further research
areas are added only after checking the shared clone's authoritative goal
claims and other sessions' work.

## Must not touch

Other sessions' uncommitted work and commit history; original-game
runtime or DOSBox; `src/`, gameplay implementation, claims or parity rows
outside EXE, CONFIG/SCRIPT research and the separate upstream gap acceptance
ledger. Push only when explicitly authorized. Native and emulated game execution are outside
this goal's batch-file research scope; read original files statically only.
An owner-approved history repair remains separate from this maintenance scope.

## Dead ends

- The earlier shared-checkout amend replaced a concurrent research commit's
  message. Commit new work only, and never amend a commit this session did
  not just make. The pending repair is recorded in docs/HANDOVER.md.
- Git Bash could not create its hook snapshot in the inherited temporary
  directory. Set TMPDIR inside Git Bash to the checkout's writable
  artifacts/hook-tmp; keep the pre-commit hook enabled.

## Handover

- Stage: Slices. The ongoing protocol objective remains active. One agent
  works in this repository, so the goal runs on main in the single checkout,
  with no goal branch or worktree. Completed workflow tooling and EXE research
  are committed.
- Last full gate: 2026-10-09, assetless Test.ps1 -NoRestore passed for FND-EXE-239 research (715 tests).
  EXE research through FND-EXE-239, measured-baseline tooling and independent
  physical PE transfer and overlay-body classification tooling passed full gates
  and enabled hooks. Mapper relocation revision 2 also passed the full gate
  and its independently failing-before/passing-after synthetic regression.
  Existing argument-check skips remain; generated files are unchanged.
  Corrected revision-2 fresh projects and repeatable exports are local.
  Review old mapped-snapshot dependencies before accepting native claims.
- Unfinished: no tracked work remains. EXE follow-up items remain Q-EXE-005,
  Q-EXE-006/007/008/009/010 and reader prerequisites Q-EXE-011/012/013; Q-EXE-001 retains its
  retry requirement. Local static-analysis reports and the saved interpreter
  Ghidra project remain in GAME_DIR/analysis/exe-batches for continuation.
  Refreshed comparison: GAME_DIR/analysis/work-baseline/relocation-reconciled;
  legacy coverage rejection is documented in docs/EVIDENCE-TOOLS.md. Inventories remain
  pending definition/mapping reconciliation; do not discard anomalous ranges
  or publish unverified replacements. Segment report: GAME_DIR/analysis/work-baseline/decoded-segment-output-audit.log;
  durable reader evidence remains FND-EXE-166, with no complete-reading promotion.
- Owner priority: complete-reading closure now takes precedence over broad
  new partial readings. Use Q-EXE-009 and FMT-EXE-006 to assemble a bounded
  evidence package, checking complete bodies, independent caller searches,
  every input/state writer, indirect targets, return consumption and external
  dependencies against STATUS-4 through STATUS-13. Add complete_reading only
  after those obligations are satisfied; recorded findings and citation coverage
  are not substitutes. Address boundary anomalies that affect the candidate.
- Environment: use the checkout's portable PowerShell
  (artifacts/pwsh7/runtime/pwsh.exe) and locked evidence-python interpreter. Clear GAME_DIR and
  NoDefaultCurrentDirectoryInExePath. TEMP/TMP must belong to the account
  actually executing the process. Under CodexSandboxOffline, explicitly set
  TEMP/TMP to C:/Users/CodexSandboxOffline/AppData/Local/Temp; the inherited
  kiber temp directory fails Java real-path resolution even when Node can
  create files there. The sandbox command runner fails before process creation;
  approved escalated commands work, and node_repl child_process is a fallback
  for read-only checks. Git writes require the escalated runner.
  Scope Git trust to this checkout with -c safe.directory or inherited
  GIT_CONFIG_COUNT/KEY/VALUE for child Git calls; do not change global trust.
  For hooks export the checkout's
  artifacts/hook-tmp as TMPDIR inside Git Bash. Keep hooks enabled. Reuse the
  saved Ghidra program with -noanalysis. Use the installed Temurin 25.0.4.1
  runtime at C:/Program Files/Eclipse Adoptium/jdk-25.0.4.101-hotspot;
  docs/GHIDRA.md now names that verified path. For combined script directories,
  follow the corrected literal quoting in docs/GHIDRA.md; both directories
  were verified through the Windows launcher. Original-program execution
  remains prohibited.
- Template context: template 0b9ab9c, rules 11884c7 and checker 2.9.0 are
  integrated on main; engine 13.6.0 and reader 2.5.0 exact locks passed restore and the full gate.
  Assetless Test.ps1 passed on 2026-10-08. The initial synthetic capture
  timing failure passed in isolation and in the full rerun. Write
  range ends half-open (research-item skill).
  Generated indexes were refreshed on main. Archive member citations are
  qualified; toolkit issue 353 has the new archive case. Template issue 90
  records fixture-baseline assumptions under scheduled generation.
- Process audit: all full gates and bounded Ghidra queries exited.
  Escalated CIM command-line/parent inspection works. Reusable MSBuild nodes
  and active work for another repository were preserved; no confirmed session
  orphan was stopped.
- Blockers: Q-EXE-011 awaits identity evidence under queue/EXE.md and docs/RUNTIME.md.
  The parent history-message repair still awaits owner approval in docs/HANDOVER.md.
  Do not rewrite shared history; continue independent Q-EXE-012/013 work.
- Upstream: template issue 83 records the Windows combined-script-path
  launch defect and the successful two-directory control. Template issues
  80 and 82, and toolkit issues 325 and 327 retain the earlier reports.
  Template issue 86 records the reproduced goal-hook missing-identity defect;
  its narrow downstream guard has synthetic controls. No upstream fix delivery
  is claimed. Toolkit issue 111 also records the reference-type guidance concern.
  Follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6044911287.
  Related PR 339 is merged; released FND-EXE-022 writer controls passed in FND-EXE-211.
  Rerun: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6068134095.
  Toolkit issue 350 received a duplicate follow-up on inventory ownership:
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/350#issuecomment-6046763133.
  Toolkit issue 111 also received a duplicate-checked request for explicit
  exclusive boundaries in bounded instruction reports, with proposed synthetic
  controls and no claim of delivered support:
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6058496258.
  The duplicate-checked denominator exporter request is recorded at
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6061345747.
  Additional duplicate-checked exclusive-end controls are recorded at
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6061558347.
  The duplicate-checked external-buffer and decoded-pointer admission example
  is recorded in the existing Standard publication issue:
  https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6062088446.
  It requests application guidance, not a status relaxation or delivered fix.
  The duplicate-checked lookup-identity control is recorded at
  https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6062349322.
  The duplicate-checked failure-import continuation example is recorded at
  https://github.com/kibertoad/refurbished-dinosaurs/issues/52#issuecomment-6062587659.
  The duplicate-checked independent PE transfer adapter suggestion is recorded at
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/26#issuecomment-6069205456.
  The duplicate-checked physical body-classification diagnostic is tracked at
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369.
  The confirmed mapper repair and downstream revalidation are tracked at
  https://github.com/kibertoad/dark-sun-wake-redux/issues/6#issuecomment-6063990997.
  Ghidra rendering follow-up: https://github.com/NationalSecurityAgency/ghidra/issues/9739#issuecomment-6064374250.
  Interrupt-use capability: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/370.
  Intermittent synthetic capture gate: https://github.com/kibertoad/dark-sun-wake-redux/issues/7; rechecks passed.
  Toolkit span rerun: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6068047179; R1/R3 passed; inclusive-query follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/343#issuecomment-6069274891.
  Inventory/segment follow-ups: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6068550782 and https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6069043576.
  Partial-overlap/guard follow-ups: https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6066902929 and https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6067099318.
- Owner scope clarification: DOSBox is excluded from game coverage and new
  complete-reading work (docs/DECISIONS.md, 2026-10-09). Its historical
  inventory is archived under docs/host-analysis/; prior host questions
  remain historical references, not this goal's research priorities.
- Utility migration tooling is committed. The SOUND_DS candidate remains local
  under GAME_DIR/analysis/work-baseline/migration-20261009; no unfinished code.
- Migration follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6069548929.
- Overlay diagnostic follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369#issuecomment-6069855019.
- Count/output-bound review example: https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6069998720.
- Next, after rechecking shared goal claims:
  1. Finish installed and CD DSUN inventory reconciliation under Q-EXE-010,
     FMT-EXE-005; follow FND-EXE-228 through FND-EXE-239 gate/product callees, state/far-pointer writers, BP/link and buffer/stack bounds, aliases and headers.
  2. Check claims before expanding into CONFIG for SOUND_DS endpoint review
     (FND-CONFIG-004 and FND-CONFIG-022); supersede factual errors under Standard.
  3. Rerun standard-coverage on all in-scope inventories once valid; retain
     citation coverage separately from complete-reading availability.
  4. Choose a bounded game-code reading and its actual caller/state obligations;
     preserve EXE questions relevant to the game and retire host-only priorities.

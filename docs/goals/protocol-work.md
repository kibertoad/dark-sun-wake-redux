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
- Last full gate: 2026-10-08, assetless Test.ps1 -NoRestore passed.
  EXE research through FND-EXE-185, measured-baseline tooling and independent
  physical PE transfer and overlay-body classification tooling passed full gates
  and enabled hooks. Mapper relocation revision 2 also passed the full gate
  and its independently failing-before/passing-after synthetic regression.
  Existing argument-check skips remain; generated files are unchanged.
  Corrected revision-2 fresh projects and repeatable exports are local.
  Review old mapped-snapshot dependencies before accepting native claims.
- Unfinished: no tracked work remains. EXE follow-up items remain Q-EXE-005,
  Q-EXE-006, Q-EXE-007, Q-EXE-008, Q-EXE-009 and Q-EXE-010; Q-EXE-001 retains its
  retry requirement. Local static-analysis reports and the saved interpreter
  Ghidra project remain in GAME_DIR/analysis/exe-batches for continuation.
  Measured snapshot exports, audit sidecars and comparisons are in
  GAME_DIR/analysis/work-baseline. Existing committed inventories were retained
  pending definition/mapping reconciliation; do not discard anomalous ranges
  or publish unverified replacements. Candidate reports remain local; durable
  reader evidence is in FND-EXE-166, with no complete-reading promotion.
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
  integrated on main; engine 13.5.0 is installed from its hash-locked wheel.
  Assetless Test.ps1 passed on 2026-10-08. The initial synthetic capture
  timing failure passed in isolation and in the full rerun. Write
  range ends half-open (research-item skill).
  Generated indexes were refreshed on main. Archive member citations are
  qualified; toolkit issue 353 has the new archive case. Template issue 90
  records fixture-baseline assumptions under scheduled generation.
  The owner requested today's wrap-up and authorized pushing main afterward.
  That wrap-up is complete; the resumed objective keeps Q-EXE-009 next.
  Current continuation does not authorize another push.
- Process audit: all full gates and bounded Ghidra queries exited.
  Escalated CIM command-line/parent inspection works. Reusable MSBuild nodes
  and active work for another repository were preserved; no confirmed session
  orphan was stopped.
  The baseline's read-only duplicate exports and subsequent candidate probes
  exited as well; reusable MSBuild nodes and uncertain ownership were preserved.
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
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/26#issuecomment-6062932353.
  The duplicate-checked physical body-classification diagnostic is tracked at
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369.
  The confirmed mapper repair and downstream revalidation are tracked at
  https://github.com/kibertoad/dark-sun-wake-redux/issues/6#issuecomment-6063990997.
  Ghidra rendering follow-up: https://github.com/NationalSecurityAgency/ghidra/issues/9739#issuecomment-6064374250.
  Interrupt-use capability: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/370.
  Intermittent synthetic capture gate: https://github.com/kibertoad/dark-sun-wake-redux/issues/7; rechecks passed.
- Next, after rechecking shared goal claims:
  1. Q-EXE-010, FMT-EXE-005: reconcile revision-2 comparison in
     GAME_DIR/analysis/work-baseline/relocation-reconciled. Revisit
     FND-EXE-173's segment/dispatch reading against FND-EXE-174's snapshots.
     Retain old artifacts and committed inventories for comparison. For Q-EXE-001,
     establish FND-EXE-175's callers, FND-EXE-182's distinct consumers, and
     FND-EXE-183's counter admission and FND-EXE-184's diagnostic/metadata contracts.
     Admit FND-EXE-185's backing memory, destination extent and aliases; follow dispatch.
  2. Q-EXE-009, FMT-EXE-006: continue producer/lifetime contracts and
     the remaining dependencies listed in its queue item toward the first
     qualifying complete-reading package. Check FND-EXE-056's bounded reader
     candidate against FND-EXE-165's inputs and FND-EXE-053's producers;
     use independent reference controls before claiming caller completeness.
     FND-EXE-165 is the current replacement; do not reuse superseded citation
     locations. Require selected-local and field-writer admission before a
     complete_reading declaration. Follow FND-EXE-166's specific remaining
     obligations: setup preservation of the incoming slot, selected-local
     lifetime, offset-28 writers, stack aliases and excluded indirect uses.
     FND-EXE-167 is the current setup-prefix replacement. Follow its shared-base
     producers through FND-EXE-043 and FND-EXE-170, segment admission and
     intervening callee effects before treating the reader's input as preserved.
     FND-EXE-168 retains the explicit-writer search domain and remaining
     decoded-record origins, initialized input extent, allocator/storage
     lifetime and excluded indirect writers. Continue the actual atom-buffer
     producer/consumer and failure contracts rather than relying on a name
     or a header predicate for storage admission.
     Use SRC-WIN32-ATOMS as an external contract only. FND-EXE-169 is the
     current tail-extent replacement. Check FND-EXE-170's admitted identifiers,
     unchanged name/initialized extent and failure helper before closing the
     decoder's input obligations; retain original state and lifetime limits.
     SRC-MS-CRT-ASSERT is an external contract only; follow the loaded CRT
     effects and conditional failure continuations in FND-EXE-170.
     FND-EXE-171 is the corrected startup reading; FND-EXE-172 is its
     independent physical/decoded interior-flow comparison. Next check
     remaining transfer representations and computed/runtime target producers;
     retain each search's exclusions before declaring complete caller coverage.
     FND-EXE-164 is
     the latest composed reading; follow concrete offset-24 callback
     producers and input consumption for FND-EXE-163/FND-EXE-053, then
     saved-state writers and FND-EXE-162's preceding-callee effects.
     FND-EXE-161's remaining callee/frame contracts still limit saved-value
     survival and diagnostic completion.
     FND-EXE-131 through FND-EXE-144 boundary corrections remain committed.
  3. Q-EXE-006 and Q-EXE-008, FMT-EXE-006: batch-input/parser and
     helper/cleanup coverage from FND-EXE-013, then disc-installer callers.
  4. Q-EXE-007, FMT-EXE-006: game/setup launch references.
  5. Q-EXE-005, FMT-EXE-006: interpreter code-page evidence when available.
  Keep CONFIG/SCRIPT and the gap ledger excluded; never run DOSBox or a shell harness.

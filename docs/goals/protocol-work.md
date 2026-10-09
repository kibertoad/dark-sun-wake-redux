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
and PARITY.md read locally and left uncommitted. The EXE launch-reference scope also covers
BLD-GOG-EN-1.1 inventory and
wrapper-provenance corrections needed to cite the studied distribution files.
Historical context includes SRC-DOSBOX-GOG-0742 and its EXE citations for the shipped
interpreter source archive; source-to-binary correspondence stays explicit.
CONFIG is added for correcting SOUND_DS location ranges in CONFIG findings
(claims checked 2026-10-09: no `goal/*` branch claims CONFIG, and
`config-static.md` is a copy from main): those findings, their replacements
(FND-CONFIG-213, FND-CONFIG-214) and the citations of them.
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
  message. Commit new work only, and never amend a commit this session did not just make. The pending repair is recorded in docs/HANDOVER.md. A concurrent integration absorbed staged handover edits; isolate future checkpoint commits.
- Git Bash could not create its hook snapshot in the inherited temporary
  directory. Set TMPDIR inside Git Bash to the checkout's writable artifacts/hook-tmp; keep the pre-commit hook enabled.

## Handover

- Integrated: session/protocol-rt-interface-20261009's verified mode-interface
  batch passed the combined gate with the FND-EXE-355 follow-up on 2026-10-09.
- Stage: Slices; goal active. Resume the authoritative local claim and preserve other sessions' work.
- Last full gate: 2026-10-09, assetless Test.ps1 -NoRestore and explicit
  main-base documentation validation passed for EXE research through FND-EXE-299 and follow-ups FND-EXE-351/352/353/354/355/356/357/358/359/361/362/363/364/365/366/367/368/369/371/372/373/374/375/376/377/378/379/471/472/473/474/475/476/477/478/479/480/481/482/483/484/485/486/487/488/489/499/500/502/503/504/507/508/509/510/511/512/513/514/515/516/517, including integrated FND-EXE-360/370/380. The final gate passed after correcting a draft location kind and integrating the referenced pending batch; the prior issue-7 rerun remains recorded. Updated preservation-context and blocked-tool guidance, measured-baseline tooling and independent physical PE transfer and overlay-body classification tooling passed full gates and enabled hooks. Mapper relocation revision 2 also passed the full gate and its independently failing-before/passing-after synthetic regression. Argument-check skips remain; stale local-main recovery passed: https://github.com/kibertoad/refurbished-dinosaurs-template/issues/92; no wrapper fix claimed. Corrected revision-2 fresh projects and repeatable exports are local. Review old mapped-snapshot dependencies before accepting native claims.
- Unfinished: this session's EXE batch is committed. The unrelated validation
  documentation edit was committed separately; recheck ownership before mutation. EXE follow-up items remain Q-EXE-005, Q-EXE-006/007/008/009/010 and reader prerequisites Q-EXE-011/012/013; Q-EXE-001 retains its retry requirement. Local static-analysis reports and the saved interpreter Ghidra project remain in GAME_DIR/analysis/exe-batches for continuation. Refreshed comparison: GAME_DIR/analysis/work-baseline/relocation-reconciled; legacy coverage rejection is documented in docs/EVIDENCE-TOOLS.md. Inventories remain pending definition/mapping reconciliation; do not discard anomalous ranges
  or publish unverified replacements. Segment report: GAME_DIR/analysis/work-baseline/decoded-segment-output-audit.log; durable reader evidence remains FND-EXE-166, with no complete-reading promotion.
- Owner priority: complete-reading closure now takes precedence over broad
  new partial readings. Use Q-EXE-020 to Q-EXE-022 and FMT-EXE-005 for game-code closure; DOSBox complete-reading questions remain historical and blocked by scope. Assemble a bounded evidence package, checking complete bodies, independent caller searches, every input/state writer, indirect targets, return consumption and external dependencies against STATUS-4 through STATUS-13. Add complete_reading only
  after those obligations are satisfied; recorded findings and citation coverage are not substitutes. Address boundary anomalies that affect the candidate.
- Queue-scope checkpoint: the isolated session branch
  session/protocol-host-queue-20261009 holds the integrated planning batch moving Q-EXE-006/009/012/013 to Blocked and updating Q-EXE-011's scope restriction.
- Environment: use the checkout's portable PowerShell
  (artifacts/pwsh7/runtime/pwsh.exe) and locked evidence-python interpreter. Clear GAME_DIR and NoDefaultCurrentDirectoryInExePath. TEMP/TMP must belong to the account actually executing the process. Under CodexSandboxOffline, explicitly set TEMP/TMP to C:/Users/CodexSandboxOffline/AppData/Local/Temp; the inherited kiber temp directory fails Java real-path resolution even when Node can
  create files there. The sandbox command runner fails before process creation; approved escalated commands work, and node_repl child_process is a fallback for read-only checks. Git writes require the escalated runner. Scope Git trust to this checkout with -c safe.directory or inherited GIT_CONFIG_COUNT/KEY/VALUE for child Git calls; do not change global trust.
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
  timing failure passed in isolation and in the full rerun. On
  goal/protocol-work, checker 4.0.1 and rules a9884ae replace 2.9.0 and
  11884c7 (2d530bb); the full gate passed with the environment in
  docs/implementation-plans/LATEST-TEMPLATE-SYNC.md. Write
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
  Do not rewrite shared history; host-only prerequisites remain blocked by scope.
- Validation-temp follow-up: the assetless Test.ps1 -NoRestore gate passed
  with the executing account's external temporary directory. Guidance is
  committed in docs/VALIDATION.md. Duplicate-checked template issue 82 update:
  https://github.com/kibertoad/refurbished-dinosaurs-template/issues/82#issuecomment-6083050870.
- Duplicate-checked synthetic pattern-query provenance suggestion:
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6083510628.
- Duplicate-checked saved-register argument acceptance example:
  https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6083940211.
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
  Synthetic capture timeout follow-ups: https://github.com/kibertoad/dark-sun-wake-redux/issues/7#issuecomment-6084577757 and https://github.com/kibertoad/dark-sun-wake-redux/issues/7#issuecomment-6085508883; latest isolated recheck failed; full recheck passed; no repair claimed.
  Toolkit span rerun: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6068047179; R1/R3 passed; inclusive-query follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/343#issuecomment-6069274891.
  Inventory/segment follow-ups: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6068550782 and https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6069043576.
  Partial-overlap/guard follow-ups: https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6066902929 and https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6067099318.
- Owner scope clarification: DOSBox is excluded from game coverage and new
  complete-reading work (docs/DECISIONS.md, 2026-10-09). Its historical
  inventory is archived under docs/host-analysis/; prior host questions
  remain historical references, not this goal's research priorities.
- Coverage-scope and complete-reading declaration-count tooling passed synthetic controls and real local baseline reruns.
- Utility migration tooling is committed. The SOUND_DS candidate remains local
  under GAME_DIR/analysis/work-baseline/migration-20261009; no unfinished code.
- Migration follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6069548929.
- Overlay diagnostic follow-up: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369#issuecomment-6069855019.
- Count/output-bound review example: https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6069998720.
- Separate traversal-bound review example: https://github.com/kibertoad/refurbished-dinosaurs/issues/37#issuecomment-6070783543.
- Transformed-count control: https://github.com/kibertoad/refurbished-dinosaurs/issues/54#issuecomment-6071069456.
- Repeat-entry failure-prefix control: https://github.com/kibertoad/refurbished-dinosaurs/issues/52#issuecomment-6071281483.
- Synthetic decoder reproduction: https://github.com/capstone-engine/capstone/issues/1226#issuecomment-6071383629.
- Continuation consistency follow-up (duplicate-checked template issue 69):
  https://github.com/kibertoad/refurbished-dinosaurs-template/issues/69#issuecomment-6080088432.
- Isolated batch branch session/protocol-exec-census-20261009 (worktree
  artifacts/worktrees/protocol-exec-census), started 2026-10-09 because a
  second writer was committing to goal/protocol-work in the shared checkout.
  It holds FND-EXE-490..498 and FND-EXE-501: the sound utility's load image makes no AH=4B or
  interrupt 2E request; its indirect far calls leave the image only for the
  disc's 19 Miles .ADV drivers, the interrupt 66 and saved timer handlers,
  and a null timer slot that runs only for a DIGPAK image the build lacks.
  The drivers' decoded code requests no program execution and transfers out
  only to host callbacks, which the utility never registers, and to resident
  Gravis or Media Vision programs outside the build. The utility requests
  only driver functions 0064..0067. FND-EXE-496 replaced an earlier census
  whose alignment vote kept misdecodes. SBAWE32.ADV runs its C module at the
  driver's base, so its four switches read code bytes as targets
  (FND-EXE-498), but in the utility's use it reaches only code entries
  (FND-EXE-501). The sound utility's launch exclusion has no open driver
  question. Integrate into goal/protocol-work only when the shared index is clean, by fast-forward
  when the goal tip is already merged into the branch; queue/EXE.md and
  FMT-EXE-006 conflict with the other writer's Tried notes, keep both sides.
  Its pathname-consumer chain under Q-EXE-007 no longer decides the sound
  utility's launch question.
  Upstream: https://github.com/kibertoad/refurbished-dinosaurs/issues/90 and
  https://github.com/kibertoad/refurbished-dinosaurs-template/issues/69#issuecomment-6086383540 and
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/388 (entry ID clashes between writers) and
  https://github.com/kibertoad/refurbished-dinosaurs/issues/91 (Q-EXE-010 and Q-EXE-007 Tried-note growth).
- Next, after rechecking shared goal claims:
  1. DSUN inventory reconciliation under FMT-EXE-005. FND-EXE-520 settled
     Q-EXE-010: CS offset 0 is the descriptor's first code byte, so
     descriptor 198's table is at 0x0008753C. Next is Q-EXE-020 (descriptor
     198's targets), where FND-EXE-221 to FND-EXE-225 already decode the
     descriptor-base candidates; then Q-EXE-021 (spans in code or the
     resident image) and Q-EXE-022 (fixup and padding spans).
  2. SOUND_DS is done: FND-CONFIG-213 and FND-CONFIG-214 correct the range
     ends, and coverage/ holds the migrated range-aware inventory. Run
     migrate-inventory.mjs from the shared checkout: it hashes the exporter
     as checked out, and the recorded revision needs Windows line endings.
  3. Rerun standard-coverage on all in-scope inventories once valid; retain
     citation coverage separately from complete-reading availability.
     The CD inventory sits under CD/, which the checker rejects; the
     manifest path CD:DSUN.EXE needs @CD/ (docs/EVIDENCE-TOOLS.md).
  4. Choose a bounded game-code reading and its actual caller/state obligations;
     preserve EXE questions relevant to the game and retire host-only priorities.
  5. Follow Q-EXE-007's game-edition consumers under FMT-EXE-006: 0F99/0F46 callers and other offset writers, segment/storage admission, 302B/20A3 and earlier startup/launch coverage.

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
Research areas: EXE for the game and its utilities. Future game-code readings use installed DSUN.EXE only under docs/SOURCE-EDITIONS.md and the 2026-10-10 decision in docs/DECISIONS.md; earlier disc comparisons remain historical. DOSBox complete readings are excluded. Research batches
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
CONFIG, INPUT, PARTY, SAVE, TIME and VIDEO are added for correcting the
DSUN.EXE location ranges that end on an inventoried function's last byte
once the reconciled DSUN inventory replaces the historical one (claims
checked 2026-10-09: goal/protocol-work is the only `goal/*` branch): those
findings, their replacements and the citations of them.
PARTY is added for research batches on its Static queue items, starting with
Q-PARTY-012, which block slice 2 (claims checked 2026-10-10:
goal/protocol-work is the only `goal/*` branch, and its other work is in EXE):
PARTY entries, queue/PARTY.md and parity/PARTY.md.
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
- Stage: Slices; goal active. Isolated research through FND-EXE-433, concurrent FND-PARTY-035..040 and the combined installed-edition policy/decision ledger passed the assetless full gate (715 tests), final main-base documentation check and enabled hooks on 2026-10-10.
  Codex continuation uses session/protocol-codex-research-20261010 in artifacts/worktrees/protocol-codex-research; its dependency junction is read-only and EVIDENCE_PYTHON uses the existing isolated root interpreter. FND-EXE-399 and the concurrent segment-span/source/edition changes passed the combined assetless full gate (715 tests) and main-base documentation check on 2026-10-10; FND-EXE-402 and concurrent PARTY research passed the combined assetless full gate (715 tests) and main-base documentation check on 2026-10-10; preserve both histories and integrate only committed artifacts.
  FND-EXE-393 committed-main recovery is validated; preserve existing commits and other sessions' work. Upstream follow-up: https://github.com/kibertoad/refurbished-dinosaurs-template/issues/69#issuecomment-6089749395.
  AGENTS.md preservation framing and out-of-scope stop guidance passed the assetless full gate (715 tests), explicit main-base documentation check and enabled hooks on 2026-10-10. No new upstream concern identified. Duplicate-checked arithmetic-control follow-up: https://github.com/kibertoad/refurbished-dinosaurs/issues/54#issuecomment-6089974953.
- Last full gate: 2026-10-10, assetless Test.ps1 -NoRestore and explicit
  main-base documentation validation passed for EXE research through FND-EXE-299 and follow-ups FND-EXE-351/352/353/354/355/356/357/358/359/361/362/363/364/365/366/367/368/369/371/372/373/374/375/376/377/378/379/471/472/473/474/475/476/477/478/479/480/481/482/483/484/485/486/487/488/489/499/500/502/503/504/507/508/509/510/511/512/513/514/515/516/517/518/519/526/527/528/529/530/531/546/547/548/549/550/551/555/556/557/558/559/391/392/393/394/395/396/397/398/399/401/402/403/404/405/406/407/408/409/411/412/413/414/415/416/417/418/419/431/432/433, including integrated FND-EXE-360/370/380. The final gate passed after correcting a draft location kind and integrating the referenced pending batch; the prior issue-7 rerun remains recorded. Updated preservation-context and blocked-tool guidance, measured-baseline tooling and independent physical PE transfer and overlay-body classification tooling passed full gates and enabled hooks. Mapper relocation revision 2 also passed the full gate and its independently failing-before/passing-after synthetic regression. Argument-check skips remain; stale local-main recovery passed: https://github.com/kibertoad/refurbished-dinosaurs-template/issues/92; no wrapper fix claimed. Corrected revision-2 fresh projects and repeatable exports are local. Review old mapped-snapshot dependencies before accepting native claims.
- Unfinished: The type-one target batch is committed and passed the reviewed assetless full gate (715 tests) on 2026-10-10; No spec draft is pending. FND-EXE-401/402/403/404/405/406/407/408/409/411/412/413/414/415/416/417/418/419/431/432/433 are committed and passed the assetless full gate (715 tests) and main-base documentation check on 2026-10-10. Its controlled query package remains local under GAME_DIR/analysis/exe-batches/type-one-field-controls; The two-region helper query is retained as helper-query.json/helper-report.json; continue other mapped resident regions, inspect its computed-transfer sites and establish segment provenance. The unrelated validation
  documentation edit was committed separately; recheck ownership before mutation. EXE follow-up items remain Q-EXE-007/009/024 and reader prerequisites Q-EXE-011/012/013; use the current queue for owner-scoped historical exclusions. Local static-analysis reports and the saved interpreter Ghidra project remain in GAME_DIR/analysis/exe-batches for continuation. Refreshed comparison: GAME_DIR/analysis/work-baseline/relocation-reconciled; legacy coverage rejection is documented in docs/EVIDENCE-TOOLS.md. Inventories remain pending definition/mapping reconciliation; do not discard anomalous ranges
  or publish unverified replacements. Segment report: GAME_DIR/analysis/work-baseline/decoded-segment-output-audit.log; durable reader evidence remains FND-EXE-166, with no complete-reading promotion.
- Owner priority: complete-reading closure now takes precedence over broad
  new partial readings. Use FMT-EXE-005 and FND-EXE-520 to FND-EXE-524 for game-code closure; DOSBox complete-reading questions remain historical and blocked by scope. Assemble a bounded evidence package, checking complete bodies, independent caller searches, every input/state writer, indirect targets, return consumption and external dependencies against STATUS-4 through STATUS-13. Add complete_reading only
  after those obligations are satisfied; recorded findings and citation coverage are not substitutes. Address boundary anomalies that affect the candidate.
- Queue-scope checkpoint: the isolated session branch
  session/protocol-host-queue-20261009 holds the integrated planning batch moving Q-EXE-006/009/012/013 to Blocked and updating Q-EXE-011's scope restriction.
- Isolated validation: EVIDENCE_PYTHON=artifacts/evidence-python-exe548/Scripts/python.exe matches the current hash-locked engine and test-only Unicorn; its full gate passed after the shared environment/lock integration mismatch.
- Environment: use the checkout's portable PowerShell
  (artifacts/pwsh7/runtime/pwsh.exe) and locked evidence-python interpreter. Clear NoDefaultCurrentDirectoryInExePath. For .ksy changes put C:/Users/kiber/AppData/Local/Programs/kaitai-struct-compiler-0.11/bin on PATH, or Test.ps1 and the docs check skip Kaitai compilation (CI compiles). GAME_DIR may stay set: licensed tests and the resident-call CLI find the game with tools/game-dir.mjs (config path or DARK_SUN_WAKE_REDUX_GAME_DIR), and Test.ps1 passed that way on 2026-10-10; engine100/101/73/91 research scripts still read GAME_DIR. TEMP/TMP must belong to the account actually executing the process. Under CodexSandboxOffline, explicitly set TEMP/TMP to C:/Users/CodexSandboxOffline/AppData/Local/Temp; the inherited kiber temp directory fails Java real-path resolution even when Node can
  create files there. The sandbox command runner fails before process creation; approved escalated commands work, and node_repl child_process is a fallback for read-only checks. Git writes require the escalated runner. Scope Git trust to this checkout with -c safe.directory or inherited GIT_CONFIG_COUNT/KEY/VALUE for child Git calls; do not change global trust. For hooks export the checkout's
  artifacts/hook-tmp as TMPDIR inside Git Bash. Keep hooks enabled. Reuse the saved Ghidra program with -noanalysis. Use the installed Temurin 25.0.4.1 runtime at C:/Program Files/Eclipse Adoptium/jdk-25.0.4.101-hotspot; docs/GHIDRA.md now names that verified path. For combined script directories, follow the corrected literal quoting in docs/GHIDRA.md; both directories
  were verified through the Windows launcher. Original-program execution remains prohibited.
- Tool locks on goal/protocol-work: reader 2.8.0, checker 4.1.1 (upstream revision ad55105), engine 15.3.0. Toolkit #414 and #419's dinorefurb-dosbox-session 0.2.0 (guarded memory writes) is the only route for agent runs (AGENTS.md "Runs of the original", host sound muted, 2026-10-10); neither it nor the pinned DOSBox-X build is installed yet. Template issue 96 asks for the same guidance upstream; template issue 81 has the GAME_DIR resolver (tools/game-dir.mjs).
- Template context: template 0b9ab9c, rules 11884c7 and checker 2.9.0 are
  integrated on main; engine 13.6.0 and reader 2.5.0 exact locks passed restore and the full gate. Assetless Test.ps1 passed on 2026-10-08. The initial synthetic capture timing failure passed in isolation and in the full rerun. On goal/protocol-work, checker 4.0.1 and rules a9884ae replace 2.9.0 and
  11884c7 (2d530bb); the full gate passed with the environment in docs/implementation-plans/LATEST-TEMPLATE-SYNC.md. Write range ends half-open (research-item skill). Generated indexes were refreshed on main. Archive member citations are qualified; toolkit issue 353 has the new archive case. Template issue 90
  records fixture-baseline assumptions under scheduled generation.
- Process audit: all full gates and bounded Ghidra queries exited. The 2026-10-09 checkpoint preserved reusable MSBuild nodes and other repositories' active work and uncertain-owned toolkit Python; no confirmed orphan was stopped.
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
- Ownership diagnostic follow-ups: https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369#issuecomment-6069855019 and https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/369#issuecomment-6090800808 (duplicate-checked resident case).
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
  https://github.com/kibertoad/refurbished-dinosaurs/issues/91 (Q-EXE-010 and Q-EXE-007 Tried-note growth),
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/393 (parity Notes still cite closed Q-EXE-010) and
  https://github.com/kibertoad/refurbished-dinosaurs-template/issues/93 (inventory-check rejects the ranges column) and
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6089321886 (single import of DSUN.EXE cannot replace the join).
- Next, after rechecking shared goal claims:
  1. Inventory reconciliation is complete under docs/EVIDENCE-TOOLS.md: FND-CONFIG-213..217, FND-INPUT-010/011, FND-PARTY-029..033, FND-SAVE-012, FND-TIME-006/007 and FND-VIDEO-008..010. Preserve range-aware inventories and the recorded Windows-line-ending exporter provenance when using migrate-inventory.mjs from the shared checkout.
  2. standard-coverage accepts every in-scope inventory. Use uncited installed DSUN.EXE functions to choose item 3's reading; citation coverage is separate from complete-reading availability.
     Installed DSUN.EXE is the sole game-code target; prior disc comparisons remain historical under docs/SOURCE-EDITIONS.md.
  3. Done: the overlay manager in segment 4AE5 (FND-EXE-560 to
     FND-EXE-569) closed Q-EXE-001. FND-EXE-570 records the disc edition; FND-EXE-571 covers segment spans.
     Q-EXE-024 (Source) asks only what flags 0, 1 and 4 mean. Toolkit #401 (instruction limit, engine 15.2.0), #323 (inventory-check inside-instruction targets, 15.3.0) and #369 (reader 2.8.0 bodies report, which replaced tools/evidence/overlay-bodies.mjs) are answered with validation on 2026-10-10; #402 (span offsets) waits on the toolkit. Next bounded
     game-code reading: pick from standard-coverage's uncited DSUN.EXE
     functions. For field-offset searches use the engine's
     operand-candidates first. IDs from FND-EXE-560 up are this session's;
     Codex allocates below them.
  4. PARTY: FND-PARTY-038 closed Q-PARTY-014 (argv[0] is the DOS 3+ program path).
     FND-PARTY-039 enumerates the pointers behind the pre-gate indirect calls
     (tools/research/exec-census/store_values.py, segment_references.py); FND-PARTY-040's reach
     rounds (q011_*.json) reach the count/DS:0DAB/pointer writers only through the run of MAS 99
     (32 opcode handlers, or the loader-error route to DS:A0F1 = 28C9:0CFF). FND-PARTY-041 closed
     Q-PARTY-015: MAS 99 (script_listing.py) runs only 0A/16/6D/70/31, whose handlers open no
     route. FND-PARTY-042 closed Q-PARTY-016: GPLDATA.GFF is open at the MAS 99 call and no
     archive-list writer is reached (q016_archive_round.json), so the load fails only on a file
     call's result. FND-PARTY-043 resolved call [di+0x6393] (DS = 1BF3, four text-layout
     routines) and the text record's +0x0C call at 0x16E3E (record DS:A03D, field cleared by
     39D1:0009 and never set). FND-PARTY-044 closed Q-PARTY-011
     (field_stores.py finds record-field stores with a base register; q011_gate_dispatch_*,
     q011_setter_sites_reach, q011_window_handlers_*, q011_start_handler_reach and
     q011_main_window_handler_reach.json): APFM +0x5A/+0x5E/+0x70 stay 0, window +0xF9/+0xFD
     stay 0 on the START GAME path, and no handler is dispatched between the gate routine's
     +11D5 and the gate. FND-PARTY-045 (q017_*.json, one start per handler) shows Load Saved
     Game opens no route unless a load is confirmed (DS:0690 guards 192+03AC; only overlay 204
     +044A sets it) and that Create Characters (window 11500) sends its events to overlay 190
     +139B/+0D89/+0802, which reach two count stores. FND-PARTY-046 reads ADD on an empty character box: 171+0AA5 loads
     CHAR and calls 31E0:0121 (inc [264E]) at 171+0C25, and Esc returns to the start window
     with nothing clearing the count, so START GAME would skip 40-43. FND-PARTY-050 maps CHAR 0x0A..0x22 onto the combatant record (hit points 0x0A, object offset 0x1A); FMT-PARTY-001 splits unk_02. FND-PARTY-051..053: a CHAR record is a chain of 10-byte-header chunks ending at type 0xFF (type 1 combatant 49 bytes, type 3 details 66 bytes with max HP at 0x4D, types 2/4 23-byte records at DS:19C1); the old 79-byte header reading is superseded, FMT-PARTY-006 added. Field numbers resolve through OBJEX FNFO 1/2. Q-PARTY-003 keeps gender/origin/alignment/classes/levels/XP/PSP (look in the details record via SCR-UI-002); Q-PARTY-020 (23-byte records); FND-PARTY-054 reads the writer (187+018F via 2D40:2196, depth first, fields 15,16,17,4). Upstream: toolkit #436 (STATUS-17 scope for superseded IDs in finding bodies; FND-PARTY-001 and -018 still name the superseded size finding in prose); template #97 (pre-commit hook never compiles staged .ksy; cross-referenced on toolkit #219). Next PARTY items:
     FND-PARTY-047/048/049 close Q-PARTY-019 (DS:265B is set at start-up; the CHAR load copies bytes 0x0A..0x3A into the party record, so ADD's object is 300 + CHAR word 0x1A, 300..313, all with OJFF); Q-PARTY-017 (other actions, NEW, unresolved
     transfers); Q-PARTY-018 asks whether
     START GAME is still reachable after a confirmed load. Inventory rows 3EBE:001F, 3EBE:09B6 and
     1000:02AD have boundary anomalies (FND-PARTY-044); upstream: toolkit #412 (reach no-return
     declarations) and #413 (inventory row starts inside instructions). The owner asked about
     live probes (reconqueror-style); no policy change was made, so Probe stays none.
  5. Follow Q-EXE-007's game-edition consumers under FMT-EXE-006: FND-EXE-397 native target/register/record contracts, callers/writers and buffer aliases; FND-EXE-398 native file-request contracts, type-four callers/writers and storage admission; FND-EXE-399 type-one shared-segment and slot-input writers/callers: use FND-EXE-401 and the local controlled package to extend literal searches to other evidenced resident regions, then computed accesses and segment provenance; native contracts and buffer admission remain; FND-EXE-396 native quantity/register contracts; FND-EXE-556 complete cache callback/state writers and native contracts, FND-EXE-395 native request semantics/preservation, scratch/descriptor writers and argument producers; FND-EXE-394 native query/record/register contracts and storage writers, FND-EXE-392 preliminary native contracts and FND-EXE-391 other slot producers/published targets, FND-EXE-559 gate/target writers and native admission, FND-EXE-403/404/405 entry DS/SI, the first helper's native contract, pointer source termination/extents and local-frame aliases; FND-EXE-408/409/411/412/413/414/415/416/417/418/419/431/432/433 broader creation/release/quantity/query callers, delta/quantity admission, repeated selection and snapshot/output pointer aliases/extents, DS:00CE/00D2 and count/record-zero/link/encoded-word producers, destination freshness and stable structure, slot target/argument and identity/head/age/dirty writers, input admission and complete callers, computed/cross-region link-field writers and callback/input/storage admission, using local link-query.json/link-report.json; FND-EXE-406/407 complete link/table writers, callback preservation, DS/input/storage provenance and inventory ownership reconciliation; FND-EXE-558 complete registration callers/state writers and predecessor/error contracts, FND-EXE-555 other general targets, FND-EXE-531 storage admission, native contracts and remaining startup dependencies.

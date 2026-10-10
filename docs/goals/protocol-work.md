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
- Tool locks on goal/protocol-work: reader 4.0.0, checker 4.2.0 (toolkit revision 1f3ce28), engine 18.4.0, RefurbishedDinosaurs .NET 12.0.0, all the latest published on 2026-10-10. Toolkit #414 and #419's dinorefurb-dosbox-session 0.2.0 (guarded memory writes) is the only route for agent runs (AGENTS.md "Runs of the original", host sound muted, 2026-10-10); neither it nor the pinned DOSBox-X build is installed yet. Template issue 96 asks for the same guidance upstream; template issue 81 has the GAME_DIR resolver (tools/game-dir.mjs).
- 2026-10-10 evening: RefurbishedDinosaurs .NET moved from 11.3.0 to 12.0.0 (2847549d); the
  assetless Test.ps1 -NoRestore gate passed (Node suites, 715 .NET tests) after an isolated rerun of
  two Node tests that failed once under memory pressure (capture-window, goal-run 0xC000012D). Toolkit
  #450: posted the shipped game.ins rerun on 12.0.0 (251 of 251 CD: entries match) and asked for
  closing; closing it here was refused by the session permissions, so the maintainer closes it. Toolkit
  #402: reader 4.0.0 rerun posted, bodies still fails on descriptors 162/163. #343, #412, #413, #462 and
  #463 have no release since their last comment. References to closed toolkit requests this project
  accepted were removed from the docs (97966088). Owner scope: the project works on the installed
  executables, so install-side analysis (cue sheets, PKLITE, installer files) is out of scope unless
  DSUN.EXE depends on it.
- FND-PARTY-115 settles Q-PARTY-061 (block_copies.py, table_flow.py, callee_copies.py, new): of 104
  calls of 1000:0452/1000:3F5A and 135 string moves, only the CHAR load (187+0478) and generation's
  store (184+1708, from the working record 4E4F:006B, 0 at +0x0E/+0x0F and never written) copy
  whole records into DS:19C9; other uses of the table segment write the name +0x21..+0x2F or
  stack/heap buffers. Q-PARTY-062 keeps saved-game loads, branchy or reloaded pointer flows and
  indirect calls. 23 copy calls lie in bytes no inventory row covers (overlays 184, 187, 196, 204;
  resident 19B0, 1A.., 4439), so the inventory misses routines such as 187+039A.
- FND-PARTY-116 settles Q-PARTY-058: DS:13F8 is set only by the -s argument (277B:0188) and Alt+F4
  in the key handler 28C9:0CFF (+102B, which also grants all wizard spells and powers), both behind
  DS:143C, which only -k911 sets; RAVAGER.BAT runs DSUN -W0 -L. The details level bytes' fixed
  stores are generation (183), class change (209) and gain/drain (210); CHARSAVE levels top out at
  15. OBJEX RDFF 64, 65, 67 (Preserver 20) and 172 (Preserver 18) are the remaining source:
  Q-PARTY-063 asks whether an OBJEX actor can join the party, and Q-PARTY-052 now names them as
  candidate enemy casters of DATA 104. details_chunks.py has --levels.
- FND-PARTY-119 (Q-PARTY-063 progress): 2D40:0CC2 adds only objects 5..0x207 except 0x13F to a
  free list, so the lists (56ED 5..0x2F, 56F1 0x30..0x13E, 56EF 0x140..0x207) never hold a party
  object, and the in-play DROP (190+10CB -> 171+0D15 -> 31E0:05BB -> 31E0:044C -> 2D40:03C1)
  leaves the party object off every list. RDFF loads through the allocators never get objects 0..3.
  FND-PARTY-120 closes Q-PARTY-063: after DROP the party object has kind 0 (2D40:2410 ->
  2D40:248D -> 2D40:0C92) and no placed entry; the kind-0 RDFF loads (178 via DS:4261 and
  28C9:0568, 191 via 31E0:2DBD, 201 from 206 for kind-2 record 430) never name it.
- FND-PARTY-121 closes Q-PARTY-052: the enemy spell choice (173+1595 -> 174+0000, chooser
  174+00B6) scores a spell as rand x the byte at 4C78:spell / 0x8000 and keeps only scores above
  0; that byte is 0 for 66..114 and 225 (enemy_spell_lists.py). Enemy lists are 13 far pointers
  at 529C:0014, picked by details byte 0x38; only RDFF 3 (Lord Warrior, Templar 15/Defiler 18)
  has 104, no list has 225, RDFF 64/65/67 use the empty list 0 and 172 list 2. The other enemy
  path (174+084D) takes 198+0149's numbers (27, 270..319); item powers (173+29AC, 189+326A,
  213+0A4F, word +0x10 less 1) hold no 105/226 in any GFF (item_words.py). Q-PARTY-064 keeps
  FND-PARTY-107's untraced computed hit arguments (197+1202, 204+19AF, 176+03BD/+048A).
- FND-PARTY-122 closes Q-PARTY-064: 197+1202 requeues the running hit's number (from 179+0EC0)
  or 0 (from 174+04D2); 176+03BD gives back the dispatcher's d (235..268) and 176+048A 235+0..0x21;
  204+19AF casts script arg mod 1000 for opcode 0x22 function 46 (case 204+025E) with arg 2 = 9999.
  Q-PARTY-051 now covers function 46 too; still waiting on Q-SCRIPT-004.
- FND-PARTY-123 (Q-PARTY-054 progress): 199+0C21 frees 52A1:0000/4/8, calls 182+19F8, 3D72:0D83,
  2C5F:0182, 187+2B91 (2 out of a fight, 3 in one) and loops on 45B9:0034. 182+19F8 sets combat
  mark +0x14 from 0 to 1 for party slots while DS:0DAB is 2 (gain still takes marks <= 2); for 2
  or 3 it runs 28C9:250A, 2C5F:0139 and the key-handler setup, then sets DS:0DAB to 1. DS:0DAB
  stores: 182 file 0x69C6E/0x69FA9/0x6A352 (1), 0x69E1F (0), 0x69E5C (4), 0x6A233 (2), resident
  2B1C:0000 (register). FND-COMBAT-011's "only write stores 4" is out of date (COMBAT area).
- Q-PARTY-054 split (commented on protocol #91 with this and the 052/064 split as data points):
  Q-PARTY-065 asks DS:0DAB's value when 210+0B44/+0B55 run; Q-PARTY-054 keeps the mode 2/3
  callees of 182+19F8. FND-PARTY-124 replaces the old DS:0DAB finding (both setter calls lie
  in 204+12B4, which sets 5 around a 179 call and restores; the old one named a routine at +12F4)
  and adds
  that no direct store puts 3 in DS:0DAB, so only mode 2 matters for Q-PARTY-054. Mode 2 is
  entered by 182+1856 (24 window openers); whether a gain can run while one is open is 065's job.
- FND-PARTY-125 (Q-PARTY-065 progress): main tick 28C9:2322 returns while DS:0D9C is 0 (182+1856
  clears it), so its fight step 28C9:1EDB (and 173+07AD's gain) cannot run in mode 2. Left: the
  other fight step 28C9:305F (map-click callers 28C9:02B1, 0428, 0C7B), 182+1167's gate, and the
  188+16E9 award paths (script 0x21, kills). Filed protocol #96: IDENTIFIERS-8 says nothing about
  tool configs (our 22 reach JSONs) citing a superseded finding, or the runs recorded from them.
  Our pinned Standard predates #66 (IDENTIFIERS-8); a template/upstream re-sync would pick it up.
- FND-PARTY-126: 182+1167 acts only while DS:0DAB is 0; the message window 172+05DB enters mode 2
  and leaves it through 182+19F8 before returning. FND-PARTY-127: 182+1856 unregisters key handler
  28C9:0CFF (190+11A1 with null); only 182+1167, +15D7, +19F8 and 199+0BC1 register it, and
  199+0BC1 runs first in every gain. Left for Q-PARTY-065: windows that save/restore the handler
  via 190+11C1 (23 far callers), 45B9:0034 event dispatch, and scripts run from mode 2 windows.
- FND-PARTY-128: 45B9:0034 is INT 33h fn 3 (mouse read), so 199's loops wait for the left button
  to come up; 190+11C1's three callers (172+03A6, 172+065E, 209+12AA) restore the handler they
  saved. Q-PARTY-065 now waits on the scripts and kills reaching 188+16E9 while mode 2 holds.
- FND-PARTY-129: reach walk (q065_mode2_window_reach.json) from the 24 calls of 182+1856, cut at
  182+19F8, reaches the interpreter and 188+16E9 only via 182+15D7 +16C7, which needs DS:0DAB 4.
  Negative not usable (37 unresolved); code after a window returns to its caller in mode 2 is
  unread. The cut uses leaf + noReturn on +19F8; filed toolkit #465 for a query-scoped stop
  (searched: #412 noReturn, #413, #457, #462, #404, #322; none covers it).
- Q-PARTY-065: 20 of the 24 mode-2 openers return to their caller still in mode 2 (a separate
  close routine calls 182+19F8), so mode-2 code is the main loop dispatching registered handlers
  (DS:A0F1/A0F5/A0F9, set by 39D1:0418/0430/0448); not yet a finding.
- FND-PARTY-130 (Q-PARTY-054): of 182+19F8's mode-2 callees only 2C5F:0139 reaches record-pointer
  loads, all via view draw 31BA:000E -> object load 31E0:0EFF -> 31E0:0B4A when byte 4F49:0C33+3n
  is 2. Added reach_record_loads.py; commented on toolkit #462 (our own report of the 256-target
  cap) with the inventory-extent gap it leaves.
- FND-PARTY-131: 4F49:0C33 state 2 is set by 184+1648 for a new party member's object slot, so
  FND-PARTY-130's branch is the party member's object load. Q-PARTY-054 now asks whether a party
  member's DS:67B7 entry can hold a negative +6 when 182+19F8 redraws the view at a level gain.
- The first object-unload finding: 187+071D unloads a DS:67B7 entry (negative +6) and has
  no call or pointer reference; opcode 0x22 function 9 renumbers only unloaded entries; 289B:0094
  removes an entry by shifting. Q-PARTY-054 closed (7effd4bc); RULE-PARTY-013's open question now
  names the mode-2 combat mark store and Q-PARTY-065.
- FND-PARTY-133 closes Q-PARTY-065 (1978ae4e): reach from the 18 handlers mode-2 windows register
  (q065_mode2_handlers_reach.json) reaches 188+16E9's gain via a spell cast in 211+081B (+0901,
  kill award outside a fight), the debug keys via 175+0B61/211+0880 -> 190+139B, and the
  interpreter via 171+0AA5's character placement (31E0:0121); not 173+07AD. RULE-PARTY-013 now
  says the pre-gain step sets combat_mark 0 -> 1 in mode 2 (no effect on who gains).
- FND-PARTY-135 replaces the first object-unload finding: the eviction loop 31E0:0D09 also unloads
  entries (31E0:0DF5, through a pointer at bp-4 the 60-byte search missed) but skips slots below
  0x30, so party entries stay loaded; RULE-PARTY-013 cites 135. FND-PARTY-134 (Q-PARTY-026): every
  route from the message window 172+05DB to the award passes the view draw's object load and the
  state-2 (party slot) calls of 31E0:044C/0B4A. FND-PARTY-136: start-up (188+0D20) lists slots
  4..0x2F on DS:56ED and frees 0x30..0x13E, never 0..3; 2D40:03C1 frees a slot below 0x30 onto
  56ED. Q-PARTY-026 left: does the DUAL path call 2D40:03C1 for slot 0..3.
- FND-PARTY-137 (q026_dual_slot_release_reach.json): from 209+123E the walk reaches 2D40:03C1 only
  below the view draw's 31E0:0EFF (slots 0x13F or evicted >= 0x30), not the overlay 175/184/187/189
  releases. Q-PARTY-026 left: slots freed by 188+12C3 and 2D40:31E6 on those routes. Commented on
  protocol #95 (two more supersessions needing the three-commit workaround).
- Upstream duplicate checks for this session's points: the old DS:0DAB finding's wrong address
  was a project error that IDENTIFIERS-8 (protocol #66) already covers, so no issue was filed;
  #96 was filed after searching protocol #46, #66, #95 and toolkit #436, #269, #80; the
  Q-PARTY-052/064 and 054/065 splits went to protocol #91 as comments.
- Filed protocol #95: a batch that supersedes an entry the handover cites cannot pass the docs
  check without a handover edit; the workaround is handover commit, batch, handover commit.
- Template context: template 0b9ab9c, rules 11884c7 and checker 2.9.0 are
  integrated on main; engine 13.6.0 and reader 2.5.0 exact locks passed restore and the full gate. Assetless Test.ps1 passed on 2026-10-08. The initial synthetic capture timing failure passed in isolation and in the full rerun. On goal/protocol-work, checker 4.0.1 and rules a9884ae replace 2.9.0 and
  11884c7 (2d530bb); the full gate passed with the environment in docs/implementation-plans/LATEST-TEMPLATE-SYNC.md. Write range ends half-open (research-item skill). Generated indexes were refreshed on main. Archive member citations are qualified. Template issue 90
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
  https://github.com/kibertoad/refurbished-dinosaurs/issues/91 (Q-EXE-010 and Q-EXE-007 Tried-note growth),
  https://github.com/kibertoad/refurbished-dinosaurs-template/issues/93 (inventory-check rejects the ranges column) and
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/111#issuecomment-6089321886 (single import of DSUN.EXE cannot replace the join).
- Next, after rechecking shared goal claims:
  1. Inventory reconciliation is complete under docs/EVIDENCE-TOOLS.md: FND-CONFIG-213..217, FND-INPUT-010/011, FND-PARTY-030..033, FND-SAVE-012, FND-TIME-006/007 and FND-VIDEO-008..010. Preserve range-aware inventories and the recorded Windows-line-ending exporter provenance when using migrate-inventory.mjs from the shared checkout.
  2. standard-coverage accepts every in-scope inventory. Use uncited installed DSUN.EXE functions to choose item 3's reading; citation coverage is separate from complete-reading availability.
     Installed DSUN.EXE is the sole game-code target; prior disc comparisons remain historical under docs/SOURCE-EDITIONS.md.
  3. Done: the overlay manager in segment 4AE5 (FND-EXE-560 to
     FND-EXE-569) closed Q-EXE-001. FND-EXE-570 records the disc edition; FND-EXE-571 covers segment spans.
     Q-EXE-024 (Source) asks only what flags 0, 1 and 4 mean. The reader's bodies report replaced tools/evidence/overlay-bodies.mjs; toolkit #402 (span offsets) waits on the toolkit. Next bounded
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
     with nothing clearing the count, so START GAME would skip 40-43. FND-PARTY-050 maps CHAR 0x0A..0x22 onto the combatant record (hit points 0x0A, object offset 0x1A); FMT-PARTY-001 splits unk_02. FND-PARTY-051..053: a CHAR record is a chain of 10-byte-header chunks ending at type 0xFF (type 1 combatant 49 bytes, type 3 details 66 bytes with max HP at 0x4D, types 2/4 23-byte records at DS:19C1); the old 79-byte header reading is superseded, FMT-PARTY-006 added. Field numbers resolve through OBJEX FNFO 1/2. Q-PARTY-003 keeps gender/origin/alignment/classes/levels/XP/PSP (look in the details record via SCR-UI-002); Q-PARTY-020 (23-byte records); FND-PARTY-054 reads the writer (187+018F via 2D40:2196, depth first, fields 15,16,17,4). FND-PARTY-055/056: the sheet (183+1783/+1827/+11BC, record at DS:1429 = details record of the slot) prints origin/gender/alignment from details 0x12/0x13/0x14 (1-based into DS:113C tables) and levels 0x1E..0x20 for class codes 0x1B..0x1D, i.e. CHAR 0x57/0x58/0x59, 0x60..0x65. FMT-COMBAT-002 could take these fields once a goal claims COMBAT. FND-PARTY-057: class codes 1-17 via table DS:1487 (overlay 212 path; 1-4 Cleric, 5-8 Druid, 9 Fighter, 10 Gladiator, 11 Preserver, 12 Psionic, 13-16 Ranger, 17 Thief). FND-PARTY-058: experience = details +0x00 (CHAR 0x45); next-level thresholds from DATA 1000 (8 rows x 20 words, x100) indexed by class byte - 1, but stored codes run to 17 (FND-PARTY-083: overlay 210 maps codes through the class bytes of the pairs at 4E4F:009C, overlay 186 does not, BUG-PARTY-001; sheet capture Q-PARTY-025; level gain in play Q-PARTY-028). FND-PARTY-065: 183+1323 (run on every class choice in generation) sets level 7 for one class, 6 for two or three, experience to the top threshold, then +1 level where the next threshold is reached, but tests positions in reverse order (1<<pos against 08DB's 4>>pos), so two-class characters never raise the first class and test the empty third against DATA 1000 row -1: BUG-PARTY-002, capture Q-PARTY-027. FND-PARTY-066: the sphere window (183+0E8B, handler +103D, buttons 0x7FA..0x7FD with icons AIR/EARTH/FIRE/WATER) sets DS:42C2 to 0x80..0x10, so class codes 1-4/5-8/13-16 are air, earth, fire, water (FMT-PARTY-001 enum renamed). FND-PARTY-077 (replaces the earlier discipline-window finding, which misnamed +0D03): the discipline window (186+0321, handler 183+109D, buttons 0x7F6..0x7F8 with icons P.KINESIS/P.METAB/TELEPATHY) sets DS:42C0 0x80/0x40/0x20 (0x80 default, 0xE0 for psionicist); a press flips the button (3EBE:0686), 3EBE:071F codes 0/1 open/close a button to presses and 2/3 turn it on/off; a psionicist can turn disciplines off and on (DONE then waits for all three), others hold one; Q-PARTY-002 closed, and 184+1876 stores bit k of the PSIN byte 4E71:0A55+slot; FMT-PARTY-003 names the bits, RULE-PARTY-003 is supported, Q-PARTY-004 closed. FND-PARTY-068: the class buttons (183+0A3E -> +091C) take the origin's word at 4E68:0000 with no class, DATA 1001 (FMT-PARTY-007, 72*origin + 9*(first-1) + second) with one or two, nothing with three; the origin words match README table 3, so RULE-PARTY-007's manual conflict is settled, RULE-PARTY-009 added and RULE-PARTY-002 uses it; FND-PARTY-069: nothing writes 4E68:0000 (the segment's only stores hit its arrays at 0x64/0x78), Q-PARTY-029 closed. FND-PARTY-070..072: generation window handler 184+0DA7 (button table at +0F3B); DONE is 0x4B68, enabled by 184+0F93 only for class + discipline (+sphere for cleric/druid, 0xE0 for psionicist); portrait button 183+0013 sets origin/gender from 14 portraits and remakes a rolled true-neutral fighter; scores (184+0000, +209D, +033E, 183+1524, tables 4E4F:0120 and 4E4F:0153): 4 + modifier + best of four 4d4, bounded by class minimum (17 prime) and 20 + modifier; alignment (183+15EE/+1631, table 4E68:001C), Ctrl skips the check. RULE-PARTY-010/011 added, RULE-PARTY-002/005 supported; RULE-PARTY-005's half-giant row now differs from the code marked complete. Filed protocol issue #93 (research batches have no step to move such a row off Code complete). FND-PARTY-073 replaces the earlier generation-numbering finding (the store calls overlay 210 +0365 through 57B9:0039). FND-PARTY-074: hit points (184+015D, 210+0000/+0301/+0449, tables 4E4F:011E and 55BE:0000..0x6C): player picks greatest HP between levels and levels x hit die, averaged over classes, x2 half-giant, + constitution bonus; RULE-PARTY-012 added, Q-PARTY-030 closed. FND-PARTY-078: the event word at 0x12 is the mouse driver's event bits (44D0:0053 packet +0x0C, copied to event +6 by 39D1:097D), so left steps forward and right or middle back; Q-PARTY-031 closed, Q-PARTY-035 (capture) added. FND-PARTY-079: no key presses a generation button (3EBE:0BFD hotkey byte +0x6C is never set, 3EBE:0CC1 has no caller; no MENU/ACCL child; Enter in the name box gives the EBOX number); Q-PARTY-036 closed. FND-PARTY-080 replaces the earlier +1DCE-callers finding (which named 212+0A36 by its trampoline offset): the DUAL routine's calls reach overlay 209's three 0B44 stores only via 172+05DB and overlays 173/188/210 (q026_dual_reach.json); Q-PARTY-026 keeps a Tried note. Open: Q-PARTY-032 Ctrl capture, FND-PARTY-076: the hit die floor reads the working record's constitution, 10 at start and then the last character finished or opened (scores reach it only at DONE); Q-PARTY-033 closed. FND-PARTY-075 (reach from the roll routine's nine tail calls, formatter and 6393 values as starts, exit worker as leaf): no draw after the hit dice; Q-PARTY-034 closed. FND-PARTY-073: generation keeps codes 1-8 (DS:1164 order) in its working record 4E4F:0029, and 184+1648 stores them as the 17 codes through 4E4F:00C0 with a variant from DS:42C2 bits 0x80..0x10; 184+1DCE turns them back when editing (Q-PARTY-026: stale 4E71:0B44 slot there?). FND-PARTY-059: PSI current combatant +2 (CHAR 0x0C), greatest details +0x0C (CHAR 0x51). Q-PARTY-003 left: the four-code variants, unk fields, score modifiers. FND-PARTY-061 supersedes the older label-position negative (its 79 searched bytes are the type-1 chunk and the start of the type-3 chunk; glossary origin/alignment/character_class now cite FND-PARTY-055..057). Finding bodies may name superseded entries, and handovers are under --references docs. Upstream: template #97 (pre-commit hook never compiles staged .ksy; cross-referenced on toolkit #219). 2026-10-10 tooling: checker 4.2.0, reader 2.10.1, engine 18.0.0 (Capstone 5.0.9, pypcode 4.0.1); template #98 and protocol #92 carry the parity-Notes tracking check; toolkit #343 R3 confirmed, open for R1. Later the same day: reader 3.0.0, engine 18.1.0 (carries PR #440), RefurbishedDinosaurs .NET 11.3.0; #402 R1 confirmed, open because bodies fails on the overlapping descriptors 162/163; #412 waits on #444 (engine 18.2.0). FND-PARTY-081/082 settle Q-PARTY-028: 210+08BC raises levels in play (experience cut to DATA 1000 column level+3, gain while column level <= experience, cap 15, human only position 0; each level 210+0740 adds a hit die when the level sum is not below the greatest-level sum, then greatest HP = max(M, L*R/M/n + con bonus)); 210+0131 recomputes PSP (wisdom bonus indexed through DS:142D, which FND-PARTY-084 shows is the levelling character only after a new Psionicist level, via 209+0A02 -> +0A84; otherwise the slot last made current, Q-PARTY-043); callers 173+07AD, 188+16E9 and the T/t debug keys of 190 (DS:143C set by the string 911); 210+0B66 is a level drain whose second pass re-raises a single class and then divides by a count of 0 (Q-PARTY-041/042). RULE-PARTY-013 added; BUG-PARTY-003 (DATA 1000 row 5 column 17 is 14000, so a level-14 psionicist never reaches 15). FND-PARTY-083 replaces the earlier pair-table finding (it named 210+0233 by its trampoline offset +002A). FMT-PARTY-001 names hit_die_total (0x4F) and greatest_levels (0x7E); glossary character_class maps the 17 codes. FND-PARTY-085: each level (and storing a character, and a class change) also runs 27E5:000F (class flags, details +0x10 = CHAR 0x55), 210+0365 (attack rate +0x24/+0x25, thri-kreen 8), 210+03EC (THAC0-like byte, combatant +0x16 = CHAR 0x20) and 210+0572 (saves +0x31..+0x35 = CHAR 0x76); RULE-PARTY-014 added; BUG-PARTY-004 (save fall capped at the save's last value) and BUG-PARTY-005 (dwarf/halfling con bonus tested on the class group, not the save); FMT-COMBAT-001/002 could take these offsets once a goal claims COMBAT. overlay_listing.py now prints the entry a trampoline call enters. Filed template #99 (research tracking: say which of question/Settles it/Blocks is missing). FND-PARTY-086 settles Q-PARTY-040: every award goes through 188+1604 (amount / classes counted, cap 2e9, raises details +0x04 = CHAR 0x49, now kill_experience; level gain unless 4C10:0019), from script instruction 0x21 (32766 every member, 32767 nobody) and from the 173+22C4 damage routine, whose +30C7 shares a killed kind-7..11 combatant's details +0x04 over the filled slots as a 16-bit word (a share of 32768+ would be negative, Q-PARTY-045); RULE-PARTY-015 added. FND-PARTY-089 settles Q-PARTY-047 and Q-PARTY-048: 4C10:0019 is the fight state (0 none, 1-4 in a fight), set to 1 when 173+3304 sees opposed combatants in reach and stepped by the fight routine 173+059A, whose result resident 2A00:0DF0/2B00:0DDE store back; kill awards defer the gain while it is not 0; at the fight end 173+3CDA brings members with combat_mark 1-3 back to mark 1 with at least 1 HP and clears DS:13FB when one was standing, the gain runs at +07AD and +059A returns 0. Resident code loads the segment as the unrelocated 0x3C10, so selector filters on store_values output must include it. FND-PARTY-088 (reach from the fight-end calls, q043_fight_end_reach.json): no store to 4E71:0B44 or DS:142D outside the gain's own 209 routines, 34 resident transfers unresolved; FND-PARTY-090 (reach from the fight routine 173+059A, q043_fight_reach.json): the same, through 210+0B44 only, 36 unresolved; Q-PARTY-043 narrowed to the code between fight steps (resident loops at 2A00:0DE3 and 2B00:0DD1) and the unresolved transfers. FND-PARTY-091 (tools/research/exec-census/field_reads.py, new): the attack routine 173+1CC1 reads combatant +0x16 as THAC0 and 173+2560 rolls (rand*20/32768)+1, hitting on 20, or on a roll other than 1 that is at least THAC0 less bonuses less +2C00's target value; this bears on RULE-COMBAT-002 and Q-COMBAT-005 for a goal that claims COMBAT (not edited here). FND-PARTY-100 (field_offsets.py, new): details 0x25+k, 0x28+k, 0x2B+k, 0x2E+k are natural attack k's rate, damage dice, sides and bonus (197+35A4; 173+2A5D rolls them), 0x24 an armed attack's rate; FMT-PARTY-001 0x6B..0x75 named; no fixed-displacement reader of the five saves at 0x31 (the computed one is FND-PARTY-099). FND-PARTY-093: the other ES reads at the save displacements use other pointers; Q-PARTY-044 left with computed addresses and copies. FND-PARTY-094 (details_chunks.py, new): 353 OBJEX RDFF resources carry CHAR chunk headers; RDFF 430 (kind 7, 107,000) and 541 (kind 7, 33,000) would give negative shares with 3 or fewer and 1 filled slots, if RDFF loads like CHAR (Q-ACTOR-003, outside the claim). FND-PARTY-095 settles Q-PARTY-046: 28C9:2E0E treats combatant kinds as three sides 0x71/0x184/0x608 with hostile sets 0xF80/0xC58/0x964 (11 hostile to all, 1 neutral), so the award mask is the party's side and the kill mask the kinds hostile to it. FND-PARTY-096 settles Q-PARTY-041: overlay 195 +0AF9 runs the drain for effect code 59 on slots 0-3, and overlay 179 +1373 sends 59 to the slot struck by item DATA 313 on an attack roll of 20; Q-PARTY-049 asks for the other senders. Tooling at engine 18.4.0 and reader 4.0.0: declared computed call targets work (q034_roll_tail_declared_reach.json); #412 noReturn works but cannot express the argument-dependent exit worker 1000:0388 (comment asks for a call-site form); #413 R2 passes and R1 fails because overlapping row starts count as notRead; #460's PR #461 is release:skip, so not validated. FND-PARTY-097 settles Q-PARTY-043: the fight routine runs object trigger scripts (173+0C65 -> 2D40:207E), and opcode 0x24 reaches 190+36D2, which makes the slot in 4C13:0369 current; Q-PARTY-050 asks whether any fight-time script holds it. FND-PARTY-098 reads 2D40:207E's trigger test (word +6 = 1 within radius, 2 beyond it, gated by 1B0B:00A4); Q-PARTY-050 is Blocked on Q-SCRIPT-004 (script instruction layouts). FND-PARTY-099 settles Q-PARTY-044: overlay 179 +2A46 rolls a saving throw against details byte 0x30 + k (k from DATA byte +0x1F bits 5-7), so the saves act in play; FND-PARTY-100 replaces the finding that said nothing reads them. Q-PARTY-049 settled (now FND-PARTY-108): code 59 also comes from the spells DATA 104 and 225, whose byte +0x19 is 59 (after a failed save, fight state 2-4), and from script opcode 0x22 function 23 (Q-PARTY-051, Blocked on Q-SCRIPT-004); every other caller of 193+166F passes a fixed code other than 59; data_bytes.py added. Filed template #101 (research-item step 8 runs only docs --check, so queue tracking errors surface at commit; run tools/Invoke-NodeChecks.mjs before committing). FND-PARTY-102 (Q-PARTY-052 Tried): the 173 swing passes no item to the hit routine; of 179+0C7F's 16 far callers only 193+13EC and 204+12B4 pass a computed DATA number. FND-PARTY-103: 204+12B4 passes DATA 148/157/159/178 (no 59); 193+13EC passes item words from the 18-byte table at 51F1:0000 (writers via far pointers, e.g. 208+03BA). FND-PARTY-104 settles Q-PARTY-039: a new greatest Preserver level opens 209+0218, which sets one bit of the slot's SPST (4D62:039C+15*slot, known spells, bit n&7 of byte n>>3), and a new Psionicist level opens 209+0A84, which learns (rank 1) or enhances (+1, up to 30) one or two powers in PSST bits 1-7 (176+064E/+0586); FMT-PARTY-004/005 fields named; the 209+0000 gate tests spell ranges 1..class level (reads past the slot's 15 bytes from level 12); Q-PARTY-053 (screen entries and rule steps), Q-PARTY-054 (199+0C21 callees). Q-PARTY-054 Tried: reach from 199+0C21 runs through 182+19F8 into window/event code (927 routines), so only a reading with DS:0DAB settles it. Filed toolkit #462 (reach: 256-target cap, reachedRoutines without extents). FND-PARTY-105 settles Q-PARTY-042: startup 277D:0004 calls signal(8, 188+17AC), so int 0 runs the run time's catcher 1000:2AF9, which calls 188+17AC -> 180+0867: screen restore (1BF3:2973 3), puts Math Err, exit(1); BUG-PARTY-006 (the drain crashes the game whenever every class gets its level back, always for one class above level 1). FND-PARTY-106 (Q-PARTY-052 Tried): the 51F1:0000 table (112 x 18 bytes, file 0x47110) is never written and holds no 104/225, so no 179+0C7F caller passes those items. FND-PARTY-107 (Q-PARTY-052 Tried): 179+11DC's +0D2F branch never runs (type words 4DA1:0054 never written, 0 in file); 193+003C passes its third argument, fixed values with no 59 or computed at 197+1202, 198+00EF/+013D, 193+22CF, 176+03BD/+048A, 177+04FE, 204+19AF. FND-PARTY-108 replaces the Q-PARTY-049 finding, which read 104/225 as items: DATA numbers below 235 are spells, 235-268 psionic powers, 269-327 items (193+2290 dispatch, 177+0492 -> 193+0000), so the drain's second source is the spells DATA 104 and 225; RULE-PARTY-013, FMT-PARTY-001, BUG-PARTY-006 and Q-PARTY-052 reworded. FND-PARTY-109: a type 4 spell (104 and 225 are) cast from overlay 211's list goes through overlay 208's cursor, whose side mask (208+0B8C) admits any side while Shift is held (int 16h fn 2 via 44B6:0011), and 193+003C hits a single chosen target with no side test, so a party caster can drain a companion; BUG-PARTY-006 frequency says so. FND-PARTY-110: the cast list (211+1A4F -> 177+0400/+050A) holds spells below 115 by SPST bit alone and 115-234 by class sphere mask (51F1:07D3) and priest level (211+208C kind 2); 104 is a level 9 wizard spell, 225 a level 7 druid spell (druid with priest level 12+). FND-PARTY-111: counts per level set by 182+0B52 from 177+05B7/+0910 (DS:0670 codes, DS:0684 words), window level capped by 211+1F0F at 211+208C; level 9 wizard needs class 17/18 (cap 15, so no party 104), level 7 priest 13/12, so a druid of priest level 13-15 can cast 225 at a companion with Shift. FND-PARTY-112: for 104/225 (m = DATA +0x1A = 0x100) the overlay 197 calls before the save (+0E8F, +1760, +111A, with +1C52's type table at DS:2588) end the hit only for protection flags or effects, a resistance roll, or types 2/3/7/11/13/16 and some record/details bytes. FND-PARTY-113: CHARSAVE's 19 CHAR records hold 0 at 0x18/0x19 (combatant +0x0E/+0x0F); writers via DS:19C9 are 198+0549 (keeps 0) and 204+259D (no caller found). FND-PARTY-114: CHARTRAN's 13D8:0BA1 stores 0 at combatant +0x0E/+0x0F for all four slots before writing CHAR. Next PARTY items: Q-PARTY-053, 045, 026, 003; live Q-PARTY-037, 035, 032, 027.
     FND-PARTY-047/048/049 close Q-PARTY-019 (DS:265B is set at start-up; the CHAR load copies bytes 0x0A..0x3A into the party record, so ADD's object is 300 + CHAR word 0x1A, 300..313, all with OJFF); Q-PARTY-017 (other actions, NEW, unresolved
     transfers); Q-PARTY-018 asks whether
     START GAME is still reachable after a confirmed load. Inventory rows 3EBE:001F, 3EBE:09B6 and
     1000:02AD have boundary anomalies (FND-PARTY-044); upstream: toolkit #412 (reach no-return
     declarations) and #413 (inventory row starts inside instructions). The owner asked about
     live probes (reconqueror-style); no policy change was made, so Probe stays none.
  5. Follow Q-EXE-007's game-edition consumers under FMT-EXE-006: FND-EXE-397 native target/register/record contracts, callers/writers and buffer aliases; FND-EXE-398 native file-request contracts, type-four callers/writers and storage admission; FND-EXE-399 type-one shared-segment and slot-input writers/callers: use FND-EXE-401 and the local controlled package to extend literal searches to other evidenced resident regions, then computed accesses and segment provenance; native contracts and buffer admission remain; FND-EXE-396 native quantity/register contracts; FND-EXE-556 complete cache callback/state writers and native contracts, FND-EXE-395 native request semantics/preservation, scratch/descriptor writers and argument producers; FND-EXE-394 native query/record/register contracts and storage writers, FND-EXE-392 preliminary native contracts and FND-EXE-391 other slot producers/published targets, FND-EXE-559 gate/target writers and native admission, FND-EXE-403/404/405 entry DS/SI, the first helper's native contract, pointer source termination/extents and local-frame aliases; FND-EXE-408/409/411/412/413/414/415/416/417/418/419/431/432/433 broader creation/release/quantity/query callers, delta/quantity admission, repeated selection and snapshot/output pointer aliases/extents, DS:00CE/00D2 and count/record-zero/link/encoded-word producers, destination freshness and stable structure, slot target/argument and identity/head/age/dirty writers, input admission and complete callers, computed/cross-region link-field writers and callback/input/storage admission, using local link-query.json/link-report.json; FND-EXE-406/407 complete link/table writers, callback preservation, DS/input/storage provenance and inventory ownership reconciliation; FND-EXE-558 complete registration callers/state writers and predecessor/error contracts, FND-EXE-555 other general targets, FND-EXE-531 storage admission, native contracts and remaining startup dependencies.

# Entire-game research

## Condition

Continue the owner's objective from docs/HANDOVER.md until the entire game is
documented under the pinned Standard and Protocol. Use the licensed installation
at C:\GOG Games\Dark Sun 2 and the pinned rust-dos session package for probes.
Completion requires a game-wide audit of behavior, rules, screens and resources,
with direct evidence sufficient for each claim, no unanswered in-scope research
questions, and passing repository checks. A completed batch or partial reading
is not completion. Keep the active thread goal active while evidence is missing.
Push only when explicitly requested.

## Scope

Side: research. All game research areas, their spec entries, queue files and
parity rows, plus preservation tooling under tools/. Installed DSUN.EXE is the
sole game-code target under docs/SOURCE-EDITIONS.md and the 2026-10-10 decision.
Historical disc comparisons remain historical. Static evidence precedes emulated
calls and session-package runs. Agent runs use RustDosSession with the pinned
emulator hash and muted host audio; DOSBox-X is only the documented fallback.
This is the only local goal/ branch as checked on 2026-10-15. Recheck claims
before each session; do not assume stale goal files on main are active claims.

## Must not touch

Gameplay implementation under src/ or tests/ in research batches; original
content in Git; GOG's DOSBox; other sessions' files or history; generated
spec/index/ and PARITY.md. Never push without explicit authorization.
Do not weaken host safeguards or extend analysis beyond preservation scope.

## Dead ends

- Do not amend shared commits or retry the historical message repair without
  approval. docs/HANDOVER.md retains the pending history-repair record.
- Unresolved callback identity, native inputs, aliases and external effects
  cannot be replaced with synthetic success, stitched traces or larger caps.
- Function byte coverage and partial findings are not complete-reading proof.
- Historical mapped-snapshot dependencies require relocation reconciliation;
  retain FMT-EXE-005 and FND-EXE-520..524 as review prerequisites.
- Prior runtime checkpoints from before dinorefurb-dosbox-session 0.10.0 used
  a rewritten sound configuration and are not admissible starting states.
- Busy per-frame breakpoints can exhaust Windows ports. Retain the warning
  recorded by the previous handover; choose a specific gated stop.
- The inherited EVIDENCE_PYTHON belongs to another project. Always set it to
  this checkout's artifacts/evidence-python/Scripts/python.exe before restores,
  checks or probes. The first 2026-10-15 restore updated that inherited
  environment's engine to 19.7.0 and session package to 0.10.0; subsequent
  work used the dedicated project environment. Do not modify it again.

## Handover

- Stage: Slices; entire-game research goal active and incomplete.
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed on the final
  constructor/startup-timezone probe batch, including 715 .NET tests and Node/documentation
  suites (1323 entries, 168 parity rows, 5 deviations). FND-RNG-018 and
  FND-RNG-019 batches also passed full gates. Existing argument-count and
  absent-Kaitai-compiler skips remain. Q-PARTY-078, Q-RNG-003 and Q-RNG-004 Tried warnings
  remain; check actual settling progress before another attempt.
- Latest RNG entries: FND-RNG-011..020 and RULE-RNG-001. Q-RNG-002 is closed;
  Q-RNG-001, Q-RNG-003, Q-RNG-004 and Q-RNG-005 remain Static. RNG parity is partial
  with its existing Defect notes. No gameplay implementation changed.
- Party/UI references: FND-PARTY-217..219, FMT-PARTY-006, SCR-UI-008 and
  FND-UI-039..040. Q-PARTY-081 remains Agent run; startup observations do
  not establish its gameplay/deletion route or required map identity.
- Runtime: use docs/RUNTIME.md for per-part admission. startup_segments.py,
  startup_window.py, restore_opener.py and clock_source.py have actual native
  provenance through the pinned session package. All owned native runs closed
  and no GOG emulator was operated. clock_source.py's nine synthetic controls
  and repeated final native attempt passed. Its current contract admits only
  the fixed constructor/standalone-timezone and zero-lookup clock path; it reads
  no environment-string contents and cannot vary TZ or inputs.
- Checkpoints: the source opener is under GAME_DIR/captures/
  rust-dos-startup-window/seed-and-opener-20261015/:opener and was restored
  natively by restore_opener.py. The newest opened state is under
  GAME_DIR/captures/rust-dos-restore-opener/
  restore-and-return-20261015-verified/:opened; it is file-verified but not
  natively restored. Pixels and input readiness remain unverified.
  SavedState.checkpoint is a boolean; SavedState.directory is the path.
  The first extended startup report's path True is a historical reporting
  error; use the package journal/reader and corrected helper.
- Latest clock reports: GAME_DIR/captures/rust-dos-clock-source/
  constructor-and-timezone-20261015/ and its -verified repeat. Earlier
  environment-default-20261015/ reports remain local.
  Refer to FND-RNG-014, FND-RNG-017 and FND-RNG-019..020 for reproducible contracts.
- Tooling: manifest-bounded probe_drive.py prepares fresh private C: from
  verified files and excludes unlisted captures/saves; CD media stays on D:.
  run_plan.py remains time-driven. Use artifacts/evidence-python exclusively
  and set EVIDENCE_PYTHON explicitly. Pinned rust-dos v1.7.2 and its hash remain
  verified. Git trust is checkout-scoped; commit hooks remain enabled.
- Unfinished: no tracked batch draft. Q-PARTY-082's new bounded graph-table
  reading is exploratory and has no finding yet. Its local configurations
  and graph-selector-table-candidates.txt are under
  GAME_DIR/analysis/party-record-heads/. Validate candidates from reached
  entries and trace initialization before treating table data as runtime state.
  Existing head, inventory-window and getter reports remain there. Static
  clock reports/configurations remain under GAME_DIR/analysis/startup-seed/,
  including complete-conversion-body.txt and the bounded calendar, initializer
  and byte-class/startup-record queries, plus clock storage/caller/pointer
  candidate reports. FND-RNG-018..019 record their bounded readings.
  Original-derived reports stay local.
- Processes: native, validation and analysis handles ended; the post-commit
  audit found no confirmed repository orphan. Reusable MSBuild nodes are not
  orphans. No unknown process or lock was changed.
- Blockers: none goal-wide. Person-only patched-save/live-session requests
  remain separate; the startup probes establish none of those capabilities.
- Next:
  1. Q-RNG-004 and Q-RNG-005: finish service/input admission, remaining
     global/table writers, allocator/callback/lifetime and storage identity.
  2. Q-RNG-003 and Q-RNG-001: admitted reseeding routes, aliases and verified
     rule/reducer callers with draw order; additional overlay hits are candidates.
  3. Q-PARTY-081: validate opened-state native restore and a state-driven
     gameplay/deletion route, with draw recording and caller/map reads.
     Use the verified opener restore when appropriate; do not repeat timed
     start.json advancement or infer readiness from a call returning.
  4. Q-PARTY-082: finish graph selector-table initialization and screen bindings
     for the three heads, using FND-PARTY-218's corrected provenance.
  5. Q-PARTY-085, Q-PARTY-083, Q-PARTY-020 and Q-PARTY-084: physical position
     contracts, attached-reference uses, remaining fields and quantity coverage.

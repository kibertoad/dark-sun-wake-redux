# Handover

## State

- Stage: Slices; slices 2 and 3 remain in progress. The entire-game research
  goal was cleared, not completed; its claim file is removed in this handover.
- Stop reason: the owner requested today's wrap-up, a cohesive batch,
  documentation, commits and a push to main, then stop. Start no further item.
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed, including 715
  .NET tests and Node/documentation suites: 1325 entries, 168 parity rows,
  five deviations. Existing argument-count and absent-Kaitai-compiler skips
  remain. Q-PARTY-078, Q-RNG-003 and Q-RNG-004 retain Tried-count warnings;
  each later attempt must take part of Settles it out before another attempt.
- Latest research references: FND-RNG-011..022, RULE-RNG-001,
  FND-PARTY-217..219, FMT-PARTY-006, SCR-UI-008 and FND-UI-039..040.
  RNG parity remains partial with its existing Defect notes. No gameplay
  implementation changed. Remaining research stays in the area queues.
- Required asset pack revision remains 36. Revision-35 local packs require
  transactional refresh through play.bat.
- Runtime admission is per part in docs/RUNTIME.md. Use the pinned muted
  session package only; owner-only visual capture remains binding.
  Agents never operate GOG's emulator. All owned native runs closed.

## Local tooling and evidence stores

- Use PowerShell 7. Set EVIDENCE_PYTHON explicitly to this checkout's
  artifacts/evidence-python/Scripts/python.exe. Its inherited value belongs
  to another project: the first 2026-10-15 restore updated that environment's
  engine/session package before subsequent work used the dedicated project
  environment. Do not modify the inherited environment again.
- Unset process-local NoDefaultCurrentDirectoryInExePath for validation.
  Machine-wide GAME_DIR belongs to another project and is ignored by the
  configured game-directory resolver. Git trust stays checkout-scoped and
  hooks remain enabled. Keep each tested snapshot unchanged while a gate runs.
- startup_segments.py, startup_window.py, restore_opener.py and clock_source.py
  have native provenance through the pinned session package. clock_source.py's
  nine synthetic controls and repeated final native attempt passed. Its fixed
  contract does not admit varying TZ or arbitrary clock inputs.
- Source opener: GAME_DIR/captures/rust-dos-startup-window/
  seed-and-opener-20261015/:opener; restore_opener.py restored it natively.
  Newer state: GAME_DIR/captures/rust-dos-restore-opener/
  restore-and-return-20261015-verified/:opened; file-verified, not natively
  restored. Pixels and input readiness remain unverified. SavedState.checkpoint
  is a boolean; SavedState.directory is the path. Use the corrected helper
  and journal rather than the first extended report's historical path True.
- Clock reports: GAME_DIR/captures/rust-dos-clock-source/
  constructor-and-timezone-20261015/ and its -verified repeat, with earlier
  environment-default-20261015/ retained locally. Contracts: FND-RNG-014,
  FND-RNG-017 and FND-RNG-019..020.
- Static reports and bounded queries: GAME_DIR/analysis/startup-seed/;
  reproduce through FND-RNG-018..019 and FND-RNG-021..022. Original-derived
  reports and configurations remain local, never committed.
- Manifest-bounded probe_drive.py prepares fresh private C: from verified
  files; CD media remains on D:. run_plan.py is time-driven. Do not repeat
  timed start.json advancement or infer readiness from a call returning.
- Prior checkpoints before session package 0.10.0 used rewritten sound
  configuration and are not admissible starting states. Avoid busy per-frame
  breakpoints that can exhaust Windows ports; select a specific gated stop.
- Preserve FMT-EXE-005 and FND-EXE-520..524 as runtime review prerequisites.
  See docs/SHARED-RUNTIME-MIGRATION.md for migration status.

## Unfinished

- No tracked batch draft remains. Q-PARTY-082 exploratory graph-table queries
  have no finding yet; local configurations and graph-selector-table-candidates.txt
  are under GAME_DIR/analysis/party-record-heads/. Validate reached entries
  and initialization before treating candidates as runtime state.
- Historical commit-message repair still awaits approval: fa0d8d1 has title
  Record; research commit 4e5d4e8 was accidentally amended as 290a177 with a
  validation message and lost its Spec trailer. Its original message remains
  available through git show -s --format=%B 4e5d4e8; its tree is unchanged.
  Automatic approval review rejected the message-only rebase because it
  rewrites shared history. Do not retry without approval or amend HEAD.

## Blockers

- Person-only patched-save and live-session requests remain separate.
  Q-PARTY-001 waits on docs/live-sessions/shipped-party.md. Startup probes
  do not establish gameplay/deletion admission for Q-PARTY-081.
- Live signing, signing-environment branch protection and high/mixed-DPI
  original-window capture remain separate acceptance checks. No release made.

## Next

1. Q-RNG-004 and Q-RNG-005: input/service admission and storage/writer coverage.
2. Q-RNG-003 and Q-RNG-001: caller admission, aliases and rule/draw mapping.
3. Q-PARTY-081: native restore and state-driven gameplay/deletion route with
   draw recording and caller/map reads under docs/RUNTIME.md.
4. Q-PARTY-082: graph selector initialization and screen bindings, using
   FND-PARTY-218's corrected provenance.
5. Q-PARTY-085, Q-PARTY-083, Q-PARTY-020 and Q-PARTY-084: remaining party contracts.

## Upstream requests

- refurbished-dinosaurs issue 77 and toolkit issue 297: build listing records,
  directory exclusions and CD: path form; tools/evidence/build-listing.mjs
  holds this project's reconciliation.
- Template issues 81 and 82: machine-wide GAME_DIR/local stores, and inherited
  TEMP/TMP sandbox access. Use an accessible process-local temporary directory;
  if Git's hook snapshot cannot create one, use writable artifacts/hook-tmp
  as Git Bash TMPDIR while retaining the hook.
- Template issue 90: scheduled-generation fixture assumptions.

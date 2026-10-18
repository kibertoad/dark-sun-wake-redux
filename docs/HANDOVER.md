# Handover

## State

- Stage: Slices; slices 2 and 3 remain in progress. No active goal.
- Stop reason: completed the owner's template/shared-library update and main
  push request. Start no further research item in this maintenance session.
- Last gate: 2026-10-18, full ./tools/Invoke-Validation.ps1 passed, including
  ./tools/Test.ps1: 715 .NET tests, 107 main Node tests and seven option/signing
  controls. Release build passed with zero warnings/errors; assetless publish
  and smoke passed. Documentation: 1371 entries, 169 parity rows, five deviations.
  Existing argument-count, absent-Kaitai-compiler and scheduled-generation skips
  remain. Existing Tried-count warnings remain; later attempts must take part
  of Settles it out before another attempt.
- Adopted template 1686fe007de6ff75c70654eaeb36ebd530f1130f, protocol ceefc84,
  reader 4.3.0, engine 19.9.0 and RefurbishedDinosaurs 12.2.0. Checker 6.0.0,
  session 0.11.0 (rust-dos v1.7.5) and decoder pins are current. Ordinary/platform locks updated.
  See docs/TEMPLATE-ACCEPTANCE.md and the latest-template synchronization plan.
- Change-aware documentation checks are available during batches; full Test.ps1
  remains required before every batch commit. Runtime/capture restrictions,
  required-pack revision and game parity statuses are unchanged. Process audit
  found no owned orphan to stop.
- Latest research references: FND-RNG-011..026, RULE-RNG-001,
  SRC-RUSTDOS-172, FND-COMBAT-029..046, RULE-COMBAT-009,
  FMT-COMBAT-003, FND-UI-041, FMT-UI-001, FND-PARTY-217..219,
  FMT-PARTY-006, SCR-UI-008, FND-UI-039..040 and FND-COMBAT-047..050.
  Current continuation references: FND-COMBAT-051/052, FND-PARTY-220..232,
  SCR-PARTY-003, FND-UI-042/043 and FMT-UI-003.
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
  TEMP/TMP for the full gate must be outside the checkout. Hook snapshots
  can use artifacts/hook-tmp.
  Machine-wide GAME_DIR belongs to another project and is ignored by the
  configured game-directory resolver. Git trust stays checkout-scoped and
  hooks remain enabled. Keep each tested snapshot unchanged while a gate runs.
- Normal-project and artifact-path .NET restore state is populated. If normal
  project obj files are absent, run dotnet restore DarkSunWakeRedux.slnx
  --locked-mode before using -NoRestore: test discovery otherwise reports
  no test projects even when artifact-path restore files exist.
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
- Effect research queries: GAME_DIR/analysis/combat-effects/; reproduce
  through FND-COMBAT-029..046 and FND-UI-041. This continuation used static
  inspection only, with no original-game process or emulated call.
- Command/menu research queries: GAME_DIR/analysis/combat-actions/;
  reproduce the committed work through FND-COMBAT-047..050. New queries
  were static; validation retained its existing declared original-call tests.
- Manifest-bounded probe_drive.py prepares fresh private C: from verified
  files; CD media remains on D:. run_plan.py is time-driven. Do not repeat
  timed start.json advancement or infer readiness from a call returning.
- Prior checkpoints before session package 0.10.0 used rewritten sound
  configuration and are not admissible starting states. Checkpoints saved
  by rust-dos v1.7.2 are refused under v1.7.5 by emulator hash; each runtime
  part needs a new v1.7.5 attempt (docs/RUNTIME.md) before a run relies on it. Avoid busy per-frame
  breakpoints that can exhaust Windows ports; select a specific gated stop.
- Preserve FMT-EXE-005 and FND-EXE-520..524 as runtime review prerequisites.
  See docs/SHARED-RUNTIME-MIGRATION.md for migration status.

## Unfinished

- No tracked batch draft remains. Q-PARTY-082 exploratory graph-table queries
  have no finding yet; local configurations and graph-selector-table-candidates.txt
  are under GAME_DIR/analysis/party-record-heads/. Validate reached entries
  and initialization before treating candidates as runtime state.
- Q-COMBAT-015 exploratory menu queries have no finding yet. Local
  menu-event-keys.json, menu-event-targets.json and
  menu-choice-candidates.txt remain in GAME_DIR/analysis/combat-actions/.
  Resume with FND-PARTY-128 and the established menu entry; raw search
  hits and table values alone do not establish native event admission.
- Q-UI-001: no tracked draft. Alignment and remaining picture consumers are
  next; reports remain in GAME_DIR/analysis/ui-window-image-20261018/.
  Reproduce through FND-UI-042/043 and the named committed tools.
- Q-PARTY-087/088/089 reports remain in
  GAME_DIR/analysis/party-record-heads/drawing-20261018/; resume through
  FND-PARTY-220..231. Use draw-linear-context.txt from the declared entry,
  rather than decoding at an internal inventory fragment boundary.
  Class-window reports remain in GAME_DIR/analysis/party-dual-window-20261018/;
  resume through FND-PARTY-232 and SCR-PARTY-003.
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

1. Q-UI-001: remaining control layout and picture consumers, FND-UI-042/043.
2. Q-PARTY-087: remaining cache writers/consumers, FND-PARTY-220..231.
3. Q-PARTY-088/089: resource/buffer and runtime binding/selection admission.
4. Q-PARTY-086: remaining action mutations.
5. Q-COMBAT-014/015/016: remaining children and inputs, FND-COMBAT-047..052.

## CONFIG and SCRIPT (from the config-static goal)

- The config-static goal (last worked 2026-09-29) was dropped on 2026-10-18;
  its file is in Git history at b5aaccda:docs/goals/config-static.md. Its
  Next list follows Q-CONFIG-008 and Q-SCRIPT-003 through FND-CONFIG-160..212
  and RULE-SCRIPT-010. Its dead ends: do not repeat the bounded incoming
  inventories (FND-CONFIG-114), the literal slot/metadata writer query
  (FND-CONFIG-142), the literal selector-base query (FND-CONFIG-148), the
  literal setup-gate writer query (FND-CONFIG-149), or the MZ, same-segment
  and FBOV incoming queries (FND-CONFIG-196, FND-CONFIG-198) without new
  pointer or registration coverage. 28C9:1261 needs new provenance after
  FND-CONFIG-114.

## Upstream requests

- refurbished-dinosaurs issue 77 and toolkit issue 297: build listing records,
  directory exclusions and CD: path form; tools/evidence/build-listing.mjs
  holds this project's reconciliation.
- Template issues 81 and 82: machine-wide GAME_DIR/local stores, and inherited
  TEMP/TMP sandbox access. Use an accessible process-local temporary directory;
  if Git's hook snapshot cannot create one, use writable artifacts/hook-tmp
  as Git Bash TMPDIR while retaining the hook.
- Template issue 90: scheduled-generation fixture assumptions.
- Toolkit issue 538: pinned Capstone's synthetic 16-bit 98/99 mnemonic
  labeling; check mode/prefixes directly. Prior-analysis impact is unverified.
  https://github.com/kibertoad/refurbished-dinosaurs-toolkit/issues/538.
- Throughput delivery: toolkit issue 539 and reviewed PR 540, protocol PR 123
  and template PR 119 are adopted through the versions above. Their local
  validation selection, research batching and index reuse guidance is current.
  Skill PR https://github.com/kibertoad/toad-skills/pull/1 is outside this
  repository update; no user-level installed skill was changed.

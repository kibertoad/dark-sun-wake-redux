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
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed for the native
  startup batch and separate RNG caller batch, including 715 .NET tests,
  Node suites and documentation validation (1311 entries, 168 parity rows,
  5 deviations). Argument-count and absent-Kaitai-compiler skips remain;
  no evidence-status promotion or queue closure. The RNG batch's initial
  static-environment metadata failure was corrected before its passing rerun.
- Latest tooling gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed with
  715 .NET tests and Node/documentation suites. check_probe_drive.py passed
  four synthetic tests; the real symlink fixture was skipped because this
  host cannot create it. No game evidence status or queue item changed.
- Latest static entries: FND-PARTY-217 and FND-PARTY-218; the replacement retains
  the superseded finding. FMT-PARTY-006 and SCR-UI-008 updated. Q-PARTY-085 is separate
  from Q-PARTY-020. Historical citations reviewed in the replacement.
- Latest dynamic entry: FND-PARTY-219. Q-PARTY-081 is now Agent run; the
  old lock blocker is no longer current. FND-UI-039 is the latest dynamic
  entry; FND-RNG-009 and RULE-RNG-001 are the latest static batch. Q-RNG-001
  was narrowed and Q-RNG-003 added. RNG parity remains partial with its
  required Defect note. No gameplay implementation changed.
- Runtime: startup_segments.py successfully completed a native rust-dos run
  using the normal lock and muted host sound. The owned session closed and
  released its lock; read-only inspection found no remaining run lock. No
  unrecognized lock was changed and no alternate path used. Use docs/RUNTIME.md
  for capability provenance. The fixed probe copies only DSUN.EXE and must not
  continue to gameplay. Manifest-bounded preparation is now in probe_drive.py
  and used by fresh run_plan.py runs. Installed validation copied 191 verified
  files to GAME_DIR/captures/probe-drive-validation-20261015 without starting
  the game. Unlisted captures and saves are excluded; CD media remains on D:.
  startup_window.py now verifies a later call-site stop without input on the
  fresh drive; check_startup_window.py passed both synthetic continuation
  controls. Reports stay in GAME_DIR/captures/rust-dos-startup-window/.
  No checkpoint was saved. A drawn-window/gameplay route remains needed;
  run_plan.py remains time-driven.
- Tooling: dependencies remain in artifacts/evidence-python; always set
  EVIDENCE_PYTHON to that interpreter. Pinned rust-dos v1.7.2 is installed and
  verified. Git trust was checkout-scoped and commit hooks remain enabled.
- Unfinished: no tracked batch draft. Local source-derived reports remain
  at GAME_DIR/analysis/party-record-heads/: combatant-head-read-candidates.txt,
  inventory-window-candidates.txt and field-getter-callers.txt. New source-derived
  reports under GAME_DIR/analysis/startup-seed/ hold setter-and-generator-callers.txt,
  startup-entry-through-seed.txt and conversion-environment-initializer.txt.
  FND-RNG-009 documents their bounded query and excludes complete-census claims.
  The first party query
  used field_reads.py --es --table 19c9 with offsets 8, A, C; its text filter
  is not segment provenance. The window query used immediate_search.py 34BC
  34BD, and the getter census targeted 1AA0:0009. FND-PARTY-217/218 record
  the bounded readings; do not repeat those queries without a new need.
- Processes: all native probe, validation and analysis handles ended. Audits
  found no confirmed repository orphan. No GOG emulator was operated.
- Blockers: no known goal-wide or machine-lock blocker. Prior person-only
  patched-save/live-session requests remain separate; native startup access
  does not verify a new gameplay state route or patched-save capability.
- Next:
  1. Q-RNG-003: finish the seed source/near-buffer identity and admitted
     reseeding paths; a fixed seed-entry probe can confirm the startup caller
     before later gameplay draws. Q-RNG-001 retains rule/caller verification;
     the generator census output holds unverified additional overlay candidates.
  2. Q-PARTY-081: use verified private-drive preparation for a state-driven
     gameplay/deletion-call stop, draw recording and frame/map read.
     FND-PARTY-219 and FND-UI-039 are prerequisites, not cleanup results. Start-window
     opener/callback locations are in FND-UI-035; verify bounded call arguments
     before selecting a state stop. Do not reuse timed start.json advancement.
  3. Q-PARTY-082: finish head-field roles through graph enumeration; use the
     corrected FND-PARTY-218 provenance.
  4. Q-PARTY-085: physical position labels and complete projection contracts.
  5. Q-PARTY-083: kind-1 field-4/offset-8 references and their consumers.
  6. Q-PARTY-020 and Q-PARTY-084: remaining attached fields and quantity coverage.

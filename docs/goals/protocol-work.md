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
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed, including 715 .NET
  tests, Node suites and documentation validation (1306 entries, 168 parity
  rows, 5 deviations). Argument-count and absent-Kaitai-compiler skips remain.
  An earlier run captured an invalid draft heading and failed; the corrected
  final files passed the full rerun and enabled commit hooks.
- Latest research batch: FND-PARTY-216 and FMT-PARTY-006. Q-PARTY-020 remains
  open with Q-PARTY-082, Q-PARTY-083 and Q-PARTY-084 separated. No status
  promotion or queue closure. No gameplay implementation changed.
- Tooling: item_field_descriptors.py reproduced the selected descriptor
  observations and rejected a synthetic unsupported executable. Dependencies
  remain in artifacts/evidence-python; always set EVIDENCE_PYTHON to that
  interpreter. Pinned rust-dos v1.7.2 is installed and hash verified.
  Git trust was checkout-scoped and hooks remain enabled.
- Runtime: docs/RUNTIME.md retains the failed 2026-10-15 session-package lock
  attempt. No emulator was launched this session. Never replace the machine
  lock path or alter an unrecognized lock to run a probe. Existing run_plan.py
  remains time-driven; fixed startup_segments.py is documented but unverified.
- Unfinished: no tracked batch draft. Preliminary Q-PARTY-082 candidate report
  is local at GAME_DIR/analysis/party-record-heads/combatant-head-read-candidates.txt,
  from field_reads.py on installed DSUN.EXE and its committed inventory with
  --es --table 19c9 and offsets 8, A, C. It is unclassified and has no finding;
  do not treat it as complete reader coverage or repeat it without a new need.
- Processes: full validation and candidate-query handles ended. Post-commit
  audit found no confirmed repository orphan. No original runtime was operated.
- Blockers: Q-PARTY-081 needs the normal runtime coordinator to resolve the
  existing unrecognized run lock. Static research remains available; this is
  no goal-wide blocker. Person-only live-session items remain separate.
- Next:
  1. Q-PARTY-082: classify saved candidates and trace possessions/equipment
     head selection and updates.
  2. Q-PARTY-083: kind-1 field-4/offset-8 references and their consumers.
  3. Q-PARTY-020 and Q-PARTY-084: other attached-record fields and complete
     quantity producer/consumer and field-binding coverage.
  4. Q-PARTY-078: remaining cleanup inputs/callees; keep Q-PARTY-081's
     separate segment-identity requirement explicit.
  5. Q-PARTY-076 and Q-PARTY-077: generation/DUAL sheet input provenance;
     verify the startup/deletion probe only after normal runtime coordination.

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
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed for both completed
  batches, including 715 .NET tests, Node suites and documentation validation
  (1304 entries, 168 parity rows, 5 deviations). Argument-count skips remain;
  Kaitai compilation requires a compiler absent on this host and remains a CI check.
- Latest research batch: FND-PARTY-214; Q-PARTY-078 remains open. No status
  promotion or queue closure. Latest tooling: fixed startup_segments.py probe
  and docs/RUNTIME.md update. Probe syntax and invalid-name rejection passed;
  live probe behavior is unverified.
- Tooling: dependencies restored into artifacts/evidence-python; pinned rust-dos
  v1.7.2 installed and hash verified. Set EVIDENCE_PYTHON explicitly to that
  interpreter. Checkout-scoped Git trust and enabled hooks were used.
- Runtime: the 2026-10-15 startup probe was refused by the default machine run
  lock before emulator launch. The package cannot recognize its record format.
  No lock was changed and no alternate lock path used. New runs are unavailable
  until the owner resolves normal run coordination; static work continues.
  Prior capability evidence remains in docs/RUNTIME.md. This checkout has no
  historical artifacts/runs checkpoints. Existing run_plan.py remains time-driven;
  the new fixed probe waits for an address stop but does not reach gameplay.
- Unfinished: no batch draft. A bounded startup listing at 1000:0000..00E0
  was read after the tooling attempt and has no new finding yet; reconcile
  FND-PARTY-038 before recording segment-producer evidence.
- Processes: full validation handles ended successfully. Post-commit audits
  found no confirmed repository orphan. No emulator was launched or operated.
- Coverage: standard-coverage ran successfully against all existing inventories;
  citation coverage is not complete-reading or whole-game completion evidence.
- Blockers: runtime lock only; no goal-wide blocker. Owner-only live-session
  and patched-save items remain separate. No release or push authorized.
- Next:
  1. Q-PARTY-078: startup DS/SS producers and preservation to deletion; remaining
     map admission and lookup/conversion effects from FND-PARTY-214.
  2. Q-PARTY-076 and Q-PARTY-077: complete generation/DUAL sheet input provenance.
  3. Q-PARTY-073 and Q-PARTY-080: pre-start damage and combatant-state writers.
  4. Q-PARTY-020: split the remaining attached-record fields into bounded questions.
  5. Once normal runtime coordination is restored, verify startup_segments.py,
     then build a state-driven several-member defeat probe for Q-PARTY-078.

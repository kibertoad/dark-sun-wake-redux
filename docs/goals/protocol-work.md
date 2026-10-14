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
- Last gate: 2026-10-15, ./tools/Test.ps1 passed, including 715 .NET tests,
  Node suites and documentation validation (1303 entries, 168 parity rows,
  5 deviations). Documentation argument-count skips remain. Kaitai compilation
  was skipped because this host has no compiler; CI owns that additional check.
- Latest batch: FND-PARTY-213; Q-PARTY-078 remains open. No spec or parity
  status promotion. No queue item closed.
- Tooling: dependencies restored into artifacts/evidence-python; pinned rust-dos
  v1.7.2 installed and hash verified. Set EVIDENCE_PYTHON explicitly to the
  local interpreter. Use checkout-scoped Git safe.directory; hooks enabled.
- Runtime: no original run was started in this session. Prior runtime capability
  evidence is in docs/RUNTIME.md. Fresh checkout has no historical artifacts/runs
  checkpoints. Existing run_plan.py waits on emulated times; a protocol-compliant
  state-driven probe is needed before using it for new behavior claims.
- Unfinished: follow-up bounded reads of 289B:007A..0094 and 2D40:27B7..286C
  have no new finding yet. Existing related entries FND-PARTY-120, 132, 135 and
  172 must be reconciled before recording a new claim. No uncommitted batch draft.
- Processes: completed validation handles were terminal; post-commit inspection
  found no confirmed orphan associated with the repository. No emulator launched.
- Blockers: no goal-wide blocker. Owner-only patched-save/live-session items
  remain individually open; continue independent static research.
- Next:
  1. Q-PARTY-078: complete remaining cleanup input/return and map-alias evidence;
     preserve FND-PARTY-213's limits and compare FND-PARTY-120/172.
  2. Q-PARTY-076 and Q-PARTY-077: complete generation/DUAL sheet input provenance.
  3. Q-PARTY-073 and Q-PARTY-080: pre-start damage and combatant-state writers.
  4. Q-PARTY-020: split the remaining attached-record fields into bounded questions.
  5. State-driven rust-dos probe tooling, then Q-PARTY-078's several-member run;
     record all RNG draws and preserve licensed outputs only in local stores.

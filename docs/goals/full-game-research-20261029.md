# Full game research and mapping

## Condition

The full game is researched and mapped: the research Audit accounts for every
licensed source file, inventoried function, format, rule, screen and campaign
dependency; every research queue item is answered in the spec or explicitly
accepted by the owner as out of reach. Evidence statuses satisfy the standard,
documentation and required gates pass, and each batch ends with its status
block. Citation coverage alone does not meet this condition. Temporary blockers
and unfinished complete readings remain unfinished work. No turn limit.

## Scope

Side: research. Areas: all game research areas and their evidence tooling.
Use docs/RESEARCH-CLOSURE.md and the existing implementation plan. Continue
static evidence first, then admitted emulated calls and runtime evidence under
the repository's per-part restrictions. Preserve the full condition across
sessions. This goal runs locally without push authorization.

## Must not touch

Gameplay implementation in src/; original content in Git; generated spec/index/
and PARITY.md; unrelated systems or emulator processes. The separate upstream
gap-resolution goal's tooling acceptance records remain outside this claim.
No push, release or history rewrite is authorized by this goal.

## Dead ends

- Default sandbox child-process restrictions prevent the documentation checker
  from invoking Git; the same checker passed with approved child-process access.
- Reuse prior Q-COMBAT-049 findings rather than repeating bounded APFM setup,
  paint and text-copy readings. Do not equate them with live record admission.
- Reuse FND-UI-068 for the corrected hit search. The citation-only change
  in the count-caller tool changes no model inputs or recorded tool outcomes.
- On 2026-10-29, GAME_DIR/captures/rust-dos-regenerated/ and artifacts/runs/
  were absent. Runtime follow-up needs state preparation; do not assume the
  historical checkpoint files are present from their documented capability tests.

## Handover

- Stage: Slices; full research Audit remains unmet.
- Latest research entry: FND-EXE-635; no unfinished batch.
- Last full gate: 2026-10-29, ./tools/Test.ps1 -NoRestore passed after FND-EXE-635.
  Documentation passed with its reported skips. Pinned rules verified offline.
- Next: Q-EXE-032, using FND-EXE-633/FND-EXE-635 and
  docs/implementation-plans/INVENTORY-TAIL-RECONCILIATION.md.
  Make the evidenced decode declarations durable across regeneration, then
  complete the overlay boundary census and review all replacement differences.
  Never edit generated inventory rows by hand or treat a local repair as an
  accepted joined denominator. The committed inventory remains unchanged.
- Static inputs and projects exist under
  GAME_DIR/analysis/inventory-reconcile-20261029/. Keep the frozen and isolated
  controls and the full-model comparison needed by FND-EXE-635. Its Setup and
  reproduction instructions distinguish these runs. The saved project named
  INST-overlay-20261029a is the pre-repair input; INST-overlay-20261029d is the
  isolated control. No analysis process remains active. Installation paths and
  the quoted batch-launcher script-path requirement are current in docs/GHIDRA.md.
- Q-COMBAT-056 continues from FND-COMBAT-100/FND-COMBAT-101.
  Bind assignment coverage and intervening preservation for the local selectors,
  then the named context lifetime. Reuse the finite entry and assignment lists.
  GAME_DIR/analysis/reader-admission-20261029/scripts.txt and
  literal-selector-entry-reach.txt remain the retained index and control report;
  their identities and limits are in those findings.
- Q-COMBAT-055 retains independent reader execution and preparation admission;
  Q-COMBAT-057 retains nonliteral first operands and the script decode gap.
  Reuse FND-COMBAT-099/FND-COMBAT-100. The current closure map is
  docs/research-closure-numbered-load.json; its assessment remains not ready.
- Q-COMBAT-046 retains window/event admission and precedes Q-COMBAT-054.
  Reuse FND-COMBAT-081/FND-COMBAT-084/FND-COMBAT-085/FND-COMBAT-098 and FND-UI-051.
  Q-COMBAT-054, Q-COMBAT-053 and Q-COMBAT-051 retain the separate archive,
  transfer/buffer and record-lifetime obligations. Their existing findings
  remain the starting evidence. Other queues and all goal areas stay in scope.
- No original-game run, gameplay implementation, complete-reading promotion or
  accepted inventory replacement was added. Reusable MSBuild workers are retained.
  docs/HANDOVER.md retains other local tooling constraints and dependencies.

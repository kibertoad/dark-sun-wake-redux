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

## Handover

- Stage: Slices; full research Audit remains unmet.
- Last research commit: 7f20a13d; no unfinished batch.
- Last full gate: 2026-10-29, ./tools/Test.ps1 -NoRestore passed for 7f20a13d.
  Documentation passed with its reported skips. Pinned rules verified offline.
- Next: Q-COMBAT-053 parent-window source and buffer admission. Reuse
  FND-COMBAT-094 for connected status/output propagation; do not repeat it
  as source, capacity or native I/O proof. FND-UI-001/FND-UI-002 are the
  existing window-resource baseline; FND-COMBAT-089 records the archive
  census procedure and controls for its APFM query. Verify unchanged inputs
  and retained evidence before a distinct window-provider query.
- Q-COMBAT-051 retains other graph/table producers, preservation, payload
  identity, lifetime and later writers. Reuse FND-COMBAT-093 for this
  registration producer and FND-COMBAT-092/FND-UI-068 for selection.
- Q-COMBAT-050 retains acquisition, Q-COMBAT-052 mode admission, and
  Q-COMBAT-049 the combined dispatch question. Q-COMBAT-047 follows for
  resource-length and table-lifetime admission. Other queues remain in scope.
- Findings are recorded, with no new complete-reading declaration or callback
  target established. No original-game run or gameplay implementation added.
- The licensed DSUN.EXE and locked resident-listing tool were revalidated.
  No owned analysis/runtime process remains; reusable MSBuild workers are retained.
- docs/HANDOVER.md retains other local tooling constraints and dependencies.

# Entire-game documentation

## Condition

Document the entire supported game under the documentation standard. Every
research queue item is closed or explicitly accepted by the owner as out of
reach; file and executable coverage audits reveal no untracked research gaps;
the documentation check and Test.ps1 pass. Temporary blockers do not satisfy
completion. This standing research goal has no turn limit.

## Scope

Side: research. Areas: all game research areas except CONFIG and SCRIPT,
which remain claimed by config-static. Existing CONFIG/SCRIPT entries may be
read as evidence without changing their claims. Shared-tooling acceptance
remains claimed by upstream-gap-resolution. Research and bounded local tools
only; no gameplay implementation. Continue from docs/HANDOVER.md.

## Must not touch

src/, implementation tests, generated spec/index/ and PARITY.md, other goals'
claims or handovers. No original-derived content in Git. No push without an
explicit request.

## Dead ends

- Preserve the prior handover's runtime, inherited Python environment and
  historical shared-commit rewrite restrictions.

## Handover

- Stage: Slices; entire-game documentation remains incomplete.
- Last gate: 2026-10-15, ./tools/Test.ps1 -NoRestore passed: 715 .NET tests,
  Node suites and documentation checks for 1342 entries and 168 parity rows.
  Existing argument-count/Kaitai skips and Tried-count warnings remain.
- Baseline: pinned upstream rules verify offline; local main includes the
  scheduled generated-file baseline. Generated files were not edited.
- Unfinished: no batch draft. No original-game process was started.
- Checkpoint: goal remains active; documentation of the entire game is
  incomplete. No owner decision is needed for the next static item.
- Next: Q-COMBAT-009 restore validation/backing and remaining writers using
  FND-COMBAT-032..035 and FND-COMBAT-039;
  Q-COMBAT-010 remaining icon archive/window/backend admission using FND-COMBAT-038..040;
  Q-COMBAT-011 remaining category bitmap producers/lifetime using FND-COMBAT-037;
  Q-COMBAT-012 caller/storage admission using FND-COMBAT-036/038;
  Q-COMBAT-013 prepared membership content lifetime using FND-COMBAT-029/036/038.
- Runtime prerequisites and local evidence stores: docs/HANDOVER.md and
  docs/RUNTIME.md. Person-only requests remain separate.

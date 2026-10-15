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
- Baseline: pinned upstream rules verify offline. Documentation comparison
  requires the current main ref, including its scheduled generated files.
- Unfinished: no research draft yet.
- Next: Q-RNG-004 service/input admission, then Q-RNG-005 storage coverage;
  Q-RNG-003 reseeding admission; Q-PARTY-082 graph selector bindings.
- Runtime prerequisites and local evidence stores: docs/HANDOVER.md and
  docs/RUNTIME.md. Person-only requests remain separate.

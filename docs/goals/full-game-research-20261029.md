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

## Handover

- Stage: Slices; full research Audit remains unmet.
- Base: 67b54db4; clean tree at discovery; previous owner wrap-up is complete.
- Pinned rules verified offline. Documentation check passed with its reported
  skips using --base main and approved child-process access.
- Next: Q-COMBAT-049 remaining record identity, acquisition and other writers;
  inspect the reduced Settles it before retrying. Q-COMBAT-047 follows for
  resource-length and table-lifetime admission. Other queues remain in scope.
- Prior source/tool paths must be revalidated; no owned runtime process started.
- Existing handover retains local tooling constraints and remaining dependencies.

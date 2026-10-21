# Full game research

## Condition

Fully research and map Dark Sun: Wake of the Ravager within the supported
edition and preservation scope in docs/IMPLEMENTATION-PLAN.md. Reach the
research Audit exit: every queue item is answered in the spec or explicitly
accepted by the owner as out of reach; account for all source files, executable
functions, rules, screens and campaign dependencies, retaining unresolved
inventory and mapping gaps. Documentation and required batch gates pass.
Partial coverage, supported parity rows and moved queue items alone do not
complete this goal. There is no turn limit.

## Scope

Side: research. Areas: all game specification areas. Static research first,
then admitted emulated calls and session-package runs under docs/RUNTIME.md.
Research tooling and metadata-only closure mapping support that work.
The owner restarted this goal on 2026-10-20; it runs on main in the
single checkout. Push only when asked.

## Must not touch

Gameplay implementation under src/; other sessions' work or history;
generated spec/index/ and PARITY.md; proprietary content in Git.
Runtime restrictions and source-edition decisions remain binding.

## Dead ends

Retain the per-question Tried records and docs/HANDOVER.md tooling limits.
Do not repeat a retained search without new evidence, tooling or reading.

## Handover

- Stage: Slices. Research continues toward the full Audit condition.
- Stopped 2026-10-21 at the owner's wrap-up request; the goal continues.
- Last full gate: 2026-10-21, Test.ps1 -NoRestore passed on 6af0232b,
  including 715 .NET tests.
- Unfinished: none in the tree. Unfinished and blockers outside this goal's
  batches are in docs/HANDOVER.md.
- Owner's order for mapping: keep the installed DSUN.EXE inventory stable,
  place the uncited functions, then complete readings.
- Next:
  - Place uncited overlays 193, 207, 188 and 172, then the next largest by
    `npm exec -- standard-coverage`, in the form of FND-EXE-596 to FND-EXE-605.
  - Complete readings for RULE-RNG-001's functions so it can reach
    established.
  - Q-RNG-003, Q-RNG-005 (four Tried notes: split before another attempt),
    Q-RNG-007, Q-RNG-022 and Q-RNG-024 to Q-RNG-027.
  - Q-PARTY-112 and Q-PARTY-113.
  - Q-RNG-006 (Agent run) and Q-RNG-028 (Source).

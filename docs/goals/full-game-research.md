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
The owner authorized retiring the stale protocol-work claim on 2026-10-19;
its branch was advanced to current main, preserving its complete history.
This goal is authoritative on goal/full-game-research. Push only when asked.

## Must not touch

Gameplay implementation under src/; other sessions' work or history;
generated spec/index/ and PARITY.md; proprietary content in Git.
Runtime restrictions and source-edition decisions remain binding.

## Dead ends

Retain the per-question Tried records and docs/HANDOVER.md tooling limits.
Do not repeat a retained search without new evidence, tooling or reading.

## Handover

- Stage: Slices. Research continues toward the full Audit condition.
- Documentation check passed on 2026-10-19 with known prose-parameter skips.
- No research batch has started under this renewed claim.
- Next: identify the lowest-slice blocking Static question, check retained
  evidence and settle a bounded dependency; use docs/RESEARCH-CLOSURE.md
  to keep complete-reading obligations distinct from citation coverage.
- Session-local validation must clear NoDefaultCurrentDirectoryInExePath;
  use this checkout's evidence Python and project-specific game-dir resolver.

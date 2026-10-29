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

- Stage: Slices. Full research Audit remains unproved; continue the complete
  Condition above. The installed DSUN.EXE inventory stays stable: place uncited
  functions first, then complete readings. Use docs/RESEARCH-CLOSURE.md and
  docs/research-closure-numbered-load.json; mapping and citation coverage are
  separate from accepted complete readings.
- Last full gate: 2026-10-29, Test.ps1 -NoRestore passed for FND-COMBAT-088,
  FND-COMBAT-089 and FMT-UI-004,
  including 151 Node tests and 715 .NET tests. Documentation passed with its
  reported argument-count, Kaitai and scheduled-generation skips.
- Current batch references: FND-COMBAT-075 through FND-COMBAT-089;
  Q-COMBAT-007 and Q-COMBAT-044 through Q-COMBAT-049 remain open.
  No implementation changes or original-game runs were made in these batches.
- Unfinished: none in the working tree at this checkpoint. No owned emulator
  or validation process remains. Other project blockers remain in docs/HANDOVER.md.
- Next Q-COMBAT-047: archive/resource-length provenance and admitted table
  lifetimes, using FND-COMBAT-082, FND-COMBAT-083, FND-COMBAT-084, FND-COMBAT-086,
  FND-CONFIG-037, FND-CONFIG-067 and FND-CONFIG-151. Consult existing
  FND-COMBAT-052 before reading overlay 182 +088B again.
- Next Q-COMBAT-049: live archive selection, transfer, record lifetimes
  and remaining construction/copy/write admission, using FND-COMBAT-087,
  FND-COMBAT-088, FND-COMBAT-089, FND-EXE-588, FND-PARTY-044 and FND-UI-005.
  Reuse their existing field and pointer censuses; do not repeat them
  without changed coverage, tooling or a new reference form. Reuse
  FND-COMBAT-089's source census only while its fingerprints and scope match.
  Related remaining format fields are Q-UI-001; the FMT-UI-004 parity row
  carries the research handoff for a later implementation session.
- Next Q-COMBAT-046 and Q-COMBAT-048: field/event admission and other callbacks,
  using FND-COMBAT-081, FND-COMBAT-084, FND-COMBAT-085 and FND-UI-051.
  with FND-CONFIG-189 as callback-service context. The APFM dependency is
  separated under Q-COMBAT-049 in the closure plan; no obligation is closed.
- Next Q-COMBAT-045: temporary-slot admission, using FND-COMBAT-078,
  FND-PARTY-119, FND-PARTY-131 and FND-COMBAT-054; then Q-COMBAT-044's
  direct continuations using FND-SAVE-022 and FND-COMBAT-077 through 081.
- Next Q-COMBAT-007: script child preservation and remaining consumers,
  using FND-COMBAT-075, FND-COMBAT-076 and FND-SCRIPT-026 through 029.
- Later: the other area queues and explicit ownership/complete-reading work
  required by the full goal. Do not interpret this local ordering as reduced scope.

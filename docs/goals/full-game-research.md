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
- Updated 2026-10-25 mid-session; the goal continues.
- Last full gate: 2026-10-25, Test.ps1 -NoRestore passed on 36e0bb1c,
  including 715 .NET tests.
- Unfinished: none in the tree. Unfinished and blockers outside this goal's
  batches are in docs/HANDOVER.md.
- Owner's order for mapping: keep the installed DSUN.EXE inventory stable,
  place the uncited functions, then complete readings.
- Done this session: every uncited installed DSUN.EXE routine is placed
  (FND-EXE-606 to FND-EXE-631); FND-EXE-632 is a trampoline census;
  FND-EXE-633 and Q-EXE-032 record three overlay 204 inventory rows that
  stop short. Q-PARTY-112 closed (FND-PARTY-257). Q-RNG-025 narrowed to
  the two callers that set the cast byte (FND-RNG-064). Q-RNG-005 split
  into Q-RNG-005, Q-RNG-029 and Q-RNG-030; seven of FND-RNG-058's nine
  transfers resolved (FND-RNG-065, FND-RNG-066).
- Found: with `DS:426D` at 9999, which the end of combat leaves until a
  leader key (FND-RNG-069, FND-RNG-070), a view click sends slot 9999 to
  `2D40:2F34`, which writes a record number over the July and August
  month lengths that the clock reseeding converts with (FND-RNG-067,
  FND-RNG-068); the routines it goes on to call write no clock storage
  (FND-RNG-071). It has no BUG entry yet; write one once Q-RNG-031 says
  which states pass 9999.
- Runtime tools: `tools/research/rust-dos-runs/trampoline_census.py` and
  `indirect_targets.py` (armed breakpoints on indirect transfers, memory
  reads, write watchpoints, key and pointer inputs). Run them with
  `artifacts/evidence-python/Scripts/python.exe -I`.
- The owner asked whether runtime-first research belongs in the protocol
  or the template; an upstream issue or PR was offered and awaits their
  answer. File nothing upstream without it.
- Next:
  - Q-RNG-031 (other stores to `DS:426D`, other paths of overlay 173
    `+059A`), then the BUG entry.
  - Q-RNG-030 (segment `2707` table cases, timer callbacks), Q-RNG-029
    and Q-RNG-005.
  - Complete readings for RULE-RNG-001's functions.
  - Q-RNG-003, Q-RNG-007, Q-RNG-022, Q-RNG-024 to Q-RNG-027 and
    Q-PARTY-113.
  - Q-RNG-006 (Agent run) and Q-RNG-028 (Source).

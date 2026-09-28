# CONFIG static research

## Condition

Resolve Q-CONFIG-008 with bounded static evidence, or move it to the evidence
section that can settle its remaining question with the attempted reading
recorded; keep the documentation check and `./tools/Test.ps1` passing.

## Scope

Areas: CONFIG. Batches: research only. Queue sections: Static.

## Must not touch

Other areas' entries, queue files and parity rows. `src/` and `tests/`.

## Dead ends

None known.

## Handover

- Stage: Slices.
- Last gate: 2026-09-28, `./tools/Test.ps1` passed (700 tests;
  documentation check passed with 471 entries and 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next: Q-CONFIG-008, complete incoming paths for shared message sinks
  and search computed or unrelocated pointer callers. FND-CONFIG-070
  rules out address-taking overlay fixups; FND-CONFIG-072 rules out
  address-taking resident relocations. FND-UI-037 and FND-SAVE-010 trace
  the Save Game route, and FND-CONFIG-071 traces the startup
  save-capacity call. FND-CONFIG-073 identifies five internal message
  calls from overlay 172's other routines; FND-CONFIG-074 traces the
  `0043` callback's frame registration and dispatch. Trace the full
  event sequence or another shared sink next. Q-CONFIG-010 tracks indirect
  acquisition-state changes; Q-CONFIG-009 has an owner-run request.

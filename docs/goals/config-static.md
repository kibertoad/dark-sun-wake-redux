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

- Q-CONFIG-008, FND-CONFIG-114: do not repeat the bounded incoming
  inventories without new reference or registration coverage.
- Q-CONFIG-008, FND-CONFIG-142: do not repeat the literal slot/metadata
  writer query without new coverage of other write forms.

## Handover

- Stage: Slices.
- Last gate: 2026-09-29, `./tools/Test.ps1` passed (700 tests;
  documentation check passed with 542 entries and 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next: Q-CONFIG-008, establish the near-state-pointer segment
  relationship and selector/starting-position producers from FND-CONFIG-145.
  Slot/metadata write-form gaps from
  FND-CONFIG-142 and remaining inputs stay in Q-CONFIG-008. Pointer replacement,
  setup entry and later table effects from FND-CONFIG-144 remain open.
  Transitive helper effects from
  FND-CONFIG-131 and FND-CONFIG-130, and remaining helper effects from
  FND-CONFIG-109 stay open. Entry 28C9:1261 provenance needs new
  coverage after FND-CONFIG-114; remaining gaps stay in Q-CONFIG-008.
  Q-CONFIG-010 tracks acquisition-state changes;
  Q-CONFIG-009 has an owner-run request.

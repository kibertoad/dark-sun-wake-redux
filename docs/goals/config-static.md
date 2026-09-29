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
- Q-CONFIG-008, FND-CONFIG-148: do not repeat the literal selector-base
  query without new coverage of other producer forms.
- Q-CONFIG-008, FND-CONFIG-149: do not repeat the literal setup-gate
  writer query without new coverage of other write forms.

## Handover

- Stage: Slices.
- Last gate: 2026-09-29, `./tools/Test.ps1` passed (700 tests;
  documentation check passed with 554 entries and 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next: Q-CONFIG-008, read the pre-join calls at 0006765C,
  0006766D and 00067679 in FND-CONFIG-157 for archive-state effects.
  The nonzero-mode helper and FND-CONFIG-156's 172C:000C call
  with word 99 also need transitive effect readings. Open and load outcomes,
  selected-record stability, failure-byte producers, buffer validity,
  earlier gate producers and bypass state remain open. The width-prefix
  direction flag, later width changes, later slot activation, stored-index
  producers, the near-state-pointer relationship and pointer replacement
  remain open. Transitive effects from FND-CONFIG-131, FND-CONFIG-130
  and FND-CONFIG-109 remain open; entry 28C9:1261 provenance needs
  new coverage after FND-CONFIG-114. Q-CONFIG-010 tracks acquisition-
  state changes; Q-CONFIG-009 has an owner-run request.

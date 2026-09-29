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
  documentation check passed with 556 entries and 158 parity rows).
- Unfinished: none.
- Blockers: none known.
- Next:
  - Q-CONFIG-008, read FND-CONFIG-156's 172C:000C call with
    number 99, start zero and selector two, including its status guard,
    31ED helper and resource loading, against FND-SCRIPT-007 and
    FND-SCRIPT-008.
  - Q-CONFIG-008, follow FND-CONFIG-159's external helper effects,
    stored-pointer consumers and replacements, earlier gate producers,
    bypass state, open outcomes and archive retention.
  - Q-CONFIG-008, bound later slot and index producers, width-state
    changes, near-state-pointer provenance and rest-time iterator inputs.
    Transitive effects in FND-CONFIG-131, FND-CONFIG-130 and
    FND-CONFIG-109 remain open; 28C9:1261 needs new provenance
    coverage after FND-CONFIG-114.
  - Q-CONFIG-010, read acquisition-state changes.
  - Q-CONFIG-009, retain the existing owner-run request.

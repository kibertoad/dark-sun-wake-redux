# CONFIG static research

## Condition

Resolve Q-CONFIG-008 with bounded static evidence, or move it to the evidence
section that can settle its remaining question with the attempted reading
recorded; keep the documentation check and `./tools/Test.ps1` passing.

## Scope

Areas: CONFIG, SCRIPT. Batches: research only. Queue sections: Static.

## Must not touch

Entries, queue files and parity rows outside CONFIG and SCRIPT.
`src/` and `tests/`.

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
  documentation check passed with 581 entries and 158 parity rows).
  Full solution build passed with zero warnings and errors.
- Unfinished: none.
- Blockers: none known.
- Next:
  - Q-CONFIG-008 and Q-SCRIPT-003, follow FND-CONFIG-169's local
    mode callees and FND-CONFIG-178's geometry branches and input
    producers. Retain FND-CONFIG-176 and FND-CONFIG-177's count,
    sentinel, alias and output conditions, FND-CONFIG-172's graph/count
    producers and FND-CONFIG-170's near-buffer inputs. Bound cache/
    capacity inputs, resource selection and reachable MAS/99 instructions
    for FND-CONFIG-160 and RULE-SCRIPT-010.
  - Q-CONFIG-008 and Q-SCRIPT-003, follow FND-CONFIG-171's
    indirect-target producers, FND-CONFIG-175's shared-CS writers,
    FND-CONFIG-167's runtime metadata/shared-slot producers and
    FND-CONFIG-166's active effects. Retain FND-CONFIG-164's driver outcomes.
  - Q-CONFIG-008, follow FND-CONFIG-159's external effects, pointer
    consumers/replacements, earlier gates and archive retention. Bound
    later slot/index producers, width-state changes, near-state provenance
    and rest-time iterator inputs. Retain FND-CONFIG-131, FND-CONFIG-130
    and FND-CONFIG-109's transitive effects; 28C9:1261 needs new
    provenance coverage after FND-CONFIG-114.
  - Q-CONFIG-010, read acquisition-state changes.
  - Q-CONFIG-009, retain the existing owner-run request.

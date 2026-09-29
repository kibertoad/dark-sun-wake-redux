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

- Q-CONFIG-008, FND-CONFIG-196: do not repeat the bounded MZ and
  same-segment incoming queries without new pointer/registration evidence.

- Q-CONFIG-008, FND-CONFIG-198: do not repeat the declared MZ/FBOV
  far-call and bounded same-segment near-call incoming queries without
  new pointer/registration coverage.

## Handover

- Stage: Slices.
- Last gate: 2026-09-29, `./tools/Test.ps1` passed (700 tests;
  documentation check passed with 605 entries and 158 parity rows).
  Full solution build passed with zero warnings and errors.
- Unfinished: no session work. Ignored local `hs_err_pid15480.log` remains;
  ownership is uncertain, and it was left unmodified and uncommitted.
  This session started no Java process. Process inspection found no confirmed
  orphans; reusable MSBuild workers were preserved.
- Blockers: none known.
- Next:
  - Q-CONFIG-008 and Q-SCRIPT-003, follow FND-CONFIG-193's
    preceding external callees, later slot/pool writers and
    native startup outcome; FND-CONFIG-194's
    release callers, accepted compaction chains, aliases and
    capacity; FND-CONFIG-195's record+9E/+A0 producers and
    remaining declared release callers; FND-CONFIG-196's incoming
    registration, arguments and later state/refresh; FND-CONFIG-197's
    refresh input producers and segment/frame aliases; FND-CONFIG-198's
    pointer/registration coverage, initial slot state and DS/SS storage;
    FND-CONFIG-199's incoming rectangle/slot inputs and second-frame-word
    provenance; FND-CONFIG-200's upstream text inputs, traversal
    and formatting callees; FND-CONFIG-201's incoming wrapper
    arguments and DS-relative format provenance; FND-CONFIG-202's
    remaining wrapper sites, format capacity, width/state producers
    and counter progress; the fixed scratch
    capacity; and
    FND-CONFIG-192's accepted mask inputs, reference chains,
    aliases, direct callers and native VGA dependencies.
    Retain FND-CONFIG-188 and FND-CONFIG-189's neighboring-byte,
    mode/coordinate/handle, callback-target and runtime inputs;
    FND-CONFIG-190 and FND-CONFIG-191's accepted image/slot producers;
    and FND-CONFIG-186 and FND-CONFIG-187's state/cache/number,
    register/segment and replacement-resource dependencies.
  - Q-CONFIG-008 and Q-SCRIPT-003, follow FND-CONFIG-183 and
    FND-CONFIG-184's initializer/cleanup callers, pointer/count/slot
    inputs and allocation/runtime effects, and FND-CONFIG-185's
    resident service/producers. Complete FND-CONFIG-179 and
    FND-CONFIG-180's callback/window inputs and FND-CONFIG-181 and
    FND-CONFIG-182's filename/prefix/archive and near-buffer provenance.
    Bound resource selection, cache/capacity inputs and reachable MAS/99
    instructions for FND-CONFIG-160 and RULE-SCRIPT-010.
  - Q-CONFIG-008 and Q-SCRIPT-003, follow FND-CONFIG-178's geometry
    branches/input producers, FND-CONFIG-176 and FND-CONFIG-177's
    count/sentinel/alias conditions, FND-CONFIG-172's graph/count inputs
    and FND-CONFIG-170's near state. Retain FND-CONFIG-171's indirect
    targets, FND-CONFIG-175's shared-CS writers, FND-CONFIG-167 and
    FND-CONFIG-166's active dependencies, FND-CONFIG-164's driver inputs,
    FND-CONFIG-159's pointer/archive dependencies, and FND-CONFIG-131,
    FND-CONFIG-130 and FND-CONFIG-109's transitive effects.
    28C9:1261 needs new provenance after FND-CONFIG-114.
  - Q-CONFIG-010, read acquisition-state changes.
  - Q-CONFIG-009, retain the existing owner-run request.

# full-game-research

## Condition

Every item under Static in the queue files of the areas below is closed or
moved to another section with what was tried recorded under `Tried:`, the
documentation check and `./tools/Test.ps1` pass on the last commit, and each
batch ended with a status block. The owner set this goal on 2026-10-18 as
"execute protocol until full game is researched and mapped".

## Scope

Side: research. Areas: ACTOR, AI, COMBAT, CONFIG, EXE, EXPLORE, GFF, IMAGE,
INPUT, ITEM, MAGIC, PARTY, QUEST, REGION, RNG, SAVE, SCRIPT, SOUND, TALK, TEXT,
TIME, UI, VIDEO. Batches: research only. Queue sections: Static first, then Emulated
call.

## Must not touch

`src/` and `tests/`. Shared tooling owned by upstream-gap-resolution.

## Dead ends

- Q-CONFIG-008: do not repeat the bounded incoming inventories
  (FND-CONFIG-114), the literal slot/metadata writer query (FND-CONFIG-142),
  the literal selector-base query (FND-CONFIG-148), the literal setup-gate
  writer query (FND-CONFIG-149), or the MZ, same-segment and FBOV incoming
  queries (FND-CONFIG-196, FND-CONFIG-198) without new pointer or
  registration coverage.

## Handover

- Stage: Slices.
- Last gate: 2026-10-18, documentation check passed (1372 entries, 169
  parity rows, five deviations).
- Unfinished: none.
- Blockers: none known.
- Next: Q-UI-001, Q-PARTY-087, Q-PARTY-088/089, Q-PARTY-086,
  Q-COMBAT-014/015/016 (from docs/HANDOVER.md).

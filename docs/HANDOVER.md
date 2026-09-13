# Restoration handover

## Current branch and integration state

Development is performed on `codex/full-restoration`. Each cohesive verified
batch is pushed there and, when requested by the owner, fast-forwarded to
`main`. Consult `git status`, local/remote refs, and the latest commit rather
than assuming this document proves push state.

The repository is a configured Dark Sun: Wake of the Ravager restoration. The
approved `docs/IMPLEMENTATION-PLAN.md` remains authoritative. Slice 3 is active
and incomplete; later slices are not complete.

## Latest cohesive batch

The character and inventory destination foundations now use owned-resource
evidence rather than one-off coordinate handling:

- pack format 19 adds the 320x200 inventory shell and a dedicated DSUI catalog
  containing complete `WIND` #11500 and #13500 graphs;
- the runtime reuses the existing character shell and shared icon assets;
- one `ExplorationDestinationInput` page object validates and routes the five
  shared navigation controls on both layouts;
- character and inventory shells render while unknown interior controls remain
  deliberately inert;
- presentation no longer draws the moving world actor over destination pages.

`DATA-GOG-UI-010`, `docs/UI-ATLAS.md`, `docs/FIDELITY.md`, and
`docs/PARITY-MATRIX.md` record the evidence boundary and remaining uncertainty.

## Local-only content

`UserContent/` is the persistent ignored asset pack. Reuse it between batches;
refresh it only when extractor code or the pack contract changes. It must never
be committed or redistributed. Owned screenshots, decoded probes, and other
research output remain under ignored `analysis/original/`.

## Next implementation priorities

Continue Slice 3 from evidence, preferably in this order:

1. establish and render the cast/psionic and current-effects destination shells
   using their owned WIND/art families and the shared navigation abstraction;
2. identify cursor resources/hotspots and implement visible Walk/Look/Attack
   feedback without assigning unevidenced frame semantics;
3. bound the first eligible interaction and dialogue resource chain, then model
   deterministic Core interaction/dialogue commands and quest flags;
4. fill character/inventory dynamic fields and item-transfer behavior only as
   their record meanings become evidenced.

Do not cycle the other twelve BMP #599 frames speculatively. The opening actor's
frame 0 is exact; the remaining frame meanings are still unknown. The current
single-cell footprint and 125 ms movement step are isolated modern policies,
not native-parity claims.

## Wrap-up gates

Before handing over any future batch, run `tools/Test.ps1`, build the solution,
verify the retained pack, and run the no-window content smoke when pack content
is applicable. After committing, perform the repository's orphan-process audit
and record only processes actually stopped.

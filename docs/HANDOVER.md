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

Controlled owned screenshots and the complete Preferences graph extend the
destination foundation without guessing dynamic behavior:

- `OBS-GOG-UI-001` proves Cast/Use and Current Effects reuse the character
  shell and `WIND` #11500 navigation, with exact USE #20080 and EFFECTS #20075
  title placement;
- pack format 20 extracts both titles, `WIND` #16500, and eight unique
  Preferences images into the exact 72-asset pack;
- `ExplorationDestinationInput` now routes the same five controls across
  character, inventory, Cast, and Effects screens;
- `PreferencesInput` renders all 13 exact controls and safely routes Game Menu
  and Return while setting mutations remain inert pending numeric evidence.

`DATA-GOG-UI-011`, `OBS-GOG-UI-001`, `docs/UI-ATLAS.md`, `docs/FIDELITY.md`,
and `docs/PARITY-MATRIX.md` record the evidence boundary and remaining uncertainty.

## Local-only content

`UserContent/` is the persistent ignored asset pack. Reuse it between batches;
refresh it only when extractor code or the pack contract changes. It must never
be committed or redistributed. Owned screenshots, decoded probes, and other
research output remain under ignored `analysis/original/`.

## Next implementation priorities

Continue Slice 3 from evidence, preferably in this order:

1. identify cursor resources/hotspots and implement visible Walk/Look/Attack
   feedback without assigning unevidenced frame semantics;
2. bound the first eligible interaction and dialogue resource chain, then model
   deterministic Core interaction/dialogue commands and quest flags;
3. establish the Preferences setting ranges/defaults and About destination,
   then implement deterministic setting mutations and frame-state feedback;
4. fill character/inventory/Cast/Effects dynamic fields and item-transfer or
   spell behavior only as
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

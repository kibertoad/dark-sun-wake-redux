# Restoration handover

## Current branch and integration state

Development is performed directly on `main`, as requested by the owner. Each
cohesive verified batch is committed there. Consult `git status`, local/remote
refs, and the latest commit rather than assuming this document proves push
state.

The repository is a configured Dark Sun: Wake of the Ravager restoration. The
approved `docs/IMPLEMENTATION-PLAN.md` remains authoritative. Slice 3 is active
and incomplete; later slices are not complete.

## Current interaction/dialogue evidence

Owner captures now distinguish hostile Look, a one-way 10,000-experience award,
and the first full conversation. `DATA-GOG-INTERACTION-001` maps hostile
`WIND` #3020 and its disabled actions; `OBS-GOG-DIALOGUE-001` maps the upper and
lower dialogue windows and ties the captured exchange to `GPL` #135 without
committing original text or screenshots. Pack format 23 contains the bounded
three-window interaction/dialogue graph, thirteen control images, `PORT` #18,
and byte-identical `GPL` #135 in a DSGP envelope. Core contains the deterministic
interaction/sole-action contract; Resources decodes the packed-string primitive
and projects GPL #135's evidenced opening portrait, two conditional speech
sources, and eight-entry menu with constant/local-flag/local-number-equality
conditions. Game maps these to Core's true/false/unknown evaluator. A bounded
selector now keeps proven-true choices in source order, hides false/unknown
choices, limits them to five physical rows, and retains original choice indexes
and branch targets. Variable initialization and global string #5 remain
unresolved, so Game supplies an explicit empty snapshot and selects only the
unconditional choice. Its label is global string #5, so the F9 preview renders
no guessed response text. The fixed portrait/window/control chrome and projected
literal speech render over the live aspect-expanded map. Wrapping, conversation
entry, and GPL instruction execution remain pending.

## Previous cohesive batch

The owner-approved `COMPAT-INPUT-001` modern control preserves original edge
scrolling and stationary right-click mode cycling while adding held-right-button
grab-drag camera panning. The input adapter distinguishes click from drag,
reanchors safely across the letterboxed canvas, emits bounded logical deltas
at a 13:10 world-to-pointer multiplier with deterministic fractional carry,
and cancels gestures outside the world view. Alt+Enter toggles native-resolution
fullscreen once per chord edge from either Alt key. Travel derives a larger
bounded logical slice for the physical aspect ratio and fills the backbuffer;
fixed-layout screens and the dialogue preview retain the centered 320x200 canvas.

The earlier cursor-evidence batch established the complete exploration cursor
family without generalizing unknown target semantics:

- `DATA-GOG-CURSOR-001` maps `ICON` #19101-#19110 to the Walk, melee,
  ranged, Look, invalid-target, and hourglass roles with exact geometry and the
  manual-defined upper-left hotspot;
- six native captures verify the Walk/melee/Look valid-invalid pairs, and the
  first valid melee target resolves to Tyr OJFF #9258 -> BMP #346;
- pack format 21 extracts all ten cursors into the exact 82-asset pack;
- the runtime draws the original cursor last, uses actual route reachability for
  Walk, reverse-draw-order entity alpha plus leader alpha for Look, and limits
  melee validity to the first observed target pending broader behavior data.

`DATA-GOG-CURSOR-001`, `docs/UI-ATLAS.md`, `docs/FIDELITY.md`,
and `docs/PARITY-MATRIX.md` record the evidence boundary and remaining uncertainty.

## Local-only content

`UserContent/` is the persistent ignored asset pack. Reuse it between batches;
refresh it only when extractor code or the pack contract changes. It must never
be committed or redistributed. Owned screenshots, decoded probes, and other
research output remain under ignored `analysis/original/`.

## Next implementation priorities

Continue Slice 3 from evidence, preferably in this order:

1. establish GPL #135's local-variable initialization and global string #5,
   then feed the evidenced snapshot into the existing fail-closed selector and
   route retained branch targets through deterministic Core dialogue state
   before adding quest consequences;
2. establish the Preferences setting ranges/defaults and About destination,
   then implement deterministic setting mutations and frame-state feedback;
3. generalize attack/look target eligibility and select ranged versus melee
   cursor from evidenced readied-weapon state;
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

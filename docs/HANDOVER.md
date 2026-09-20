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
committing original text or screenshots. The current required revision 34 pack contains the bounded
three-window interaction/dialogue graph, thirteen control images, `PORT` #18,
and byte-identical `GPL` #135 and `MAS` #99 in source-tagged DSGP v2 envelopes. Core contains the deterministic
interaction/sole-action contract; Resources decodes the packed-string primitive
and projects GPL #135's evidenced opening portrait, two conditional speech
sources, and eight-entry menu with constant/local-flag/local-number-equality
conditions. Game maps these to Core's true/false/unknown evaluator. A bounded
selector now keeps proven-true choices in source order, hides false/unknown
choices, limits them to five physical rows, and retains original choice indexes
and branch targets. The capture-correlated opening state selects choices 0, 1,
2, 3, and 7, while bounded MAS #99 projections supply the global string #5 and
#6 exit labels from the ignored owned pack. GPL #135's paired counter branches
and MIT libgff's state-clear behavior establish local number 0 as zero and
global flag 357 as false for this fresh opening only; generic variable
initialization remains unresolved. A
deterministic Core dialogue session now owns all choice definitions and derives
the visible source-index/branch-target pairs; runtime
row clicks select one pair atomically and cannot leak through as world movement.
`EXE-GOG-UI-004` additionally bounds and extracts the four difficulty labels,
ten contiguous Preferences descriptions, and nine centered About lines from the
fingerprinted executable into a separate DSTX catalog. The manual's
Average/Balanced terminology conflict, numeric setting ranges, selected
defaults, description-role ordering, and About presentation remain open.
Choice 0 has a bounded projection validating its three literal prints, two
newlines, local-flag 0 clear, and local return; the bounded post-menu continuation
sets local flag 4 and advances to the second menu, while Game rebuilds its
filtered rows. Choice 7 has an additional bounded projection validating its
print, local-flag 14/4 assignments, and local return; Core applies the
flags/completion only to the matching selected branch and Game closes the
preview. Choices 2 and 3 also have bounded single-print, matching flag-clear,
local-number-0 increment, and return projections. Applying both reveals source
choice 4 in the recomputed page. Choice 1 validates three prints, clears local
flag 1, conditionally sets local flags 6/7 from global flag 357, sets that global
flag, and returns; Core applies the conditional and assignments atomically.
Choice 4 validates three prints, sets local flag 9, resets local number 0, and
returns through the same continuation into the second menu. That continuation
reads local flags 1, 2, 3, and 9 and preserves the paired counter path on the
opening menu by setting local flag 5 at count two. The second menu is bounded at
offset 750, has seven source choices, and reuses the completion target at 2905;
runtime dispatches by branch
target so reused menu indexes cannot collide, and target 1597 returns to its
calling page. Implemented returned branches replace the speech with their
bounded projected output while retaining explicit newlines; unimplemented
visible targets are inert and cannot strand the Core session. The second-menu
trouble target at 1825 is bounded through three prints, local flag 6 clear, a
local-flag-16 equals-zero condition, local flag 10 set, and return. Core
evaluates the condition atomically; the fresh-opening path changes the visible
source order from 1, 3, 5, 6 to 2, 3, 5, 6 and presents the transcript. The
newly enabled king-history target 3479 is bounded through three prints, local
flag 16 set, local flag 10 clear, and return; it presents its transcript and
leaves second-page source order 3, 5, 6. The
captured opening selects the `GNUM22 == 1` entry path. Target 1996 validates
that path's two prints and conditionally sets local flag 11,
clears local flag 7, presents the transcript, and leaves source order 4, 5, 6.
Its alternate path selects a lead-in from global-number-84 bit 2, adds two common
prints, applies `GNUM84 |= 1`, clears flag 7, and returns. Target 2352 prints its
Acar response, clears local flag 11, presents the transcript, and leaves source
order 5, 6. Target 2415 then presents three prints, sets local flags 12/13,
conditionally sets local flag 10 when flag 16 is false, and advances through
the validated loop header to the seven-entry third menu at offset 2616. The
owned path has flag 16 set and therefore resolves third-page order 0, 1, 6. The
first third-page target at 2921 prints three literals, clears local flag 12,
returns with source order 1, 6, and presents its transcript. Target 3089 then
sets flag 15, clears flag 13, and exposes source order 2, 6; target 3257 clears
flag 15 and leaves only source choice 6. Both present their bounded transcripts. The
alternate target 3686 clears flag 17 and sets flag 18, then target 3786 clears
flag 18; both are target-dispatched and their five output parts are validated
against the owned pack. Constant target 3976 clears flag 8, evaluates its
six-flag completion helper after that mutation, sets flag 14 on the completed
owned path, validates the selected two-part output, and completes the session. The
fixed portrait/window/control chrome and
projected initial literal speech render over the live aspect-expanded map.
Wrapping, conversation entry, generic variable initialization, and
broader GPL instruction execution remain pending.

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

1. establish the remaining Preferences ranges/defaults and About presentation,
   then implement deterministic setting mutations and frame-state feedback;
2. identify the shipped START GAME party and party-overview/member interaction
   boundary through the bounded native capture gate;
3. add the next evidenced quest consequences;
4. generalize attack/look target eligibility and select ranged versus melee
   cursor from evidenced readied-weapon state;
5. fill character/inventory/Cast/Effects dynamic fields and item-transfer or
   spell behavior only as
   their record meanings become evidenced.

Do not cycle the other twelve BMP #599 frames speculatively. The opening actor's
frame 0 is exact; the remaining frame meanings are still unknown. The current
single-cell footprint and 125 ms movement step are isolated modern policies,
not native-parity claims.

`docs/OWNER-CAPTURE-CHECKLIST.md` contains the bounded Ctrl+F5 checklist for
the first priority. Do not inspect candidate DOSBox screenshots until the owner
confirms its timestamp window and completed checklist identifiers.

## Wrap-up gates

Before handing over any future batch, run `tools/Test.ps1`, build the solution,
verify the retained pack, and run the no-window content smoke when pack content
is applicable. After committing, perform the repository's orphan-process audit
and record only processes actually stopped.

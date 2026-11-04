---
id: FND-COMBAT-118
title: Nominal restore payload spans miss the source table and the fifteen pointer fields supplying its runtime bindings
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    kind: file-data
    offset: 0x0004E57E..0x0004E672
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00070B4A..0x00070BFD
tool: tools/evidence/report.mjs table and operand at 20f427dc; interval comparison of FND-COMBAT-116/117 metadata
environment: null
---

## Observation

Q-COMBAT-065 retains source-pointer lifetime and overlapping earlier
restore destinations. FND-COMBAT-116 supplies 46 relocated fixed spans;
FND-COMBAT-117 identifies the fifteen supplier fields copied by the
binding helper. This comparison checks whether the nominal payloads
themselves overwrite that pointer metadata. It is not a writer census.
All address and length values below are hexadecimal.

Under the modeled resident mapping, the source array and supplier fields
occupy seven disjoint regions. Grouped fields below include only adjacent
four-byte pointers, without assuming unrelated gaps are pointer storage.

| Pointer metadata | Loaded region | Corresponding file region |
|---|---|---|
| All 61 source-table entries | 57E0:157E..1672 | 4E57E..4E672 |
| Kind-1, details and combatant supplier pointers | 57E0:19C1..19CD | 4E9C1..4E9CD |
| Kind-5 supplier pointer | 57E0:19D1..19D5 | 4E9D1..4E9D5 |
| Placed-entry supplier pointer | 57E0:67B7..67BB | 537B7..537BB |
| Bitmap supplier pointer | 57E0:55B8..55BC | 525B8..525BC |
| Trigger supplier pointer | 57E0:40C0..40C4 | 510C0..510C4 |
| Eight script supplier pointers | 4C13:032F..034F | 4165F..4167F |

For every one of FND-COMBAT-116's 46 nonnull fixed entries, form
the half-open interval from its relocated file target through that
target plus its shipped length. None intersects any of these seven
regions: 322 pairwise comparisons have zero intersections. This uses
the existing source fingerprint and relocation results, not a new
assumption that all raw segment words are declared relocations.

Two boundary cases help distinguish adjacency from overlap. SAVE/1's
nominal destination begins at file 537BB, exactly the exclusive end
of the placed-pointer field. SAVE/51 occupies 4165D..4165F and ends
exactly where the eight script pointers begin. A positive interval
control with SAVE/24's own 43B5D..442DD span intersects that span;
the comparison is not an always-empty or inclusive-end predicate.

The fifteen dynamically supplied payloads are not assigned fixed file
spans here. Under the concrete producer, valid-heap, preserved-field
and fitting nonwrapping-transfer conditions of FND-COMBAT-117, their
payloads lie above resident data and also miss these resident metadata
regions. Their known allocation requests alone do not admit those
conditions. A changed pointer, larger selected resource or wrapped
transfer is outside the nominal comparison.

## Interpretation

For the stated fixed pointers and lengths, the resource payload writes
do not themselves replace the table used by later iterations or the
fields from which its runtime bindings were copied. Extending that
argument to all 61 requests requires the fifteen dynamic bindings'
independent admission. It does not follow merely from the loop index
or the order in which those resources are requested.

Q-COMBAT-065 need not repeat the nominal payload-versus-pointer-metadata
comparison. It still needs actual pointer values and lifetimes, selected
lengths from Q-COMBAT-064, DS/segment and stack identity, and writes
outside these payload spans. Allocation, zero-fill, diagnostics, archive
helpers, callbacks and postprocessing can have their own stores; their
effects are not erased by a disjoint payload comparison. In particular,
FND-COMBAT-117's pre-skip placed-buffer clear remains separate.
No runtime immutability, complete load, accepted-save invariant or
new evidence status is established.

## Alternatives

- Nonoverlap of nominal payloads is not absence of other pointer writers.
- Adjacent fields are not overlapping fields under half-open bounds.
- A copied pointer's earlier provenance and later lifetime remain distinct.
- Shipped source lengths are not destination capacities enforced by the reader.

## How to reproduce

Reuse FND-COMBAT-116's exact hash-guarded table and operand procedure,
configs and 46 resolved rows under GAME_DIR/analysis/hit-incoming-20261104/.
Reverify installed DSUN.EXE size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b before reusing those reports. The
mapping is load segment 1000 and header size 5200. FND-COMBAT-117
supplies each supplier-field address from the complete binding body.

For each payload interval [a,b) and metadata interval [c,d), intersection
requires a < d and c < b. Use the seven regions above, retaining all
fifteen unmeasured dynamic spans. Check the SAVE/24 self-intersection
and the two exact boundaries independently. The formula and endpoints
fully specify the comparison; no original-derived output enters Git.
No new parser, original execution or mutation is required.

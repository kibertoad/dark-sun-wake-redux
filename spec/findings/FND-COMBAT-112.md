---
id: FND-COMBAT-112
title: None of the 26 fingerprinted shipped top-level archives supplies a SAVE type table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DARKRUN.GFF
    offset: 0x000003BF..0x000003DF
  - build: BLD-GOG-EN-1.1
    file: DARKSAVE.GFF
    offset: 0x000003BF..0x000003DF
tool: tools/research/exec-census/gff_tag_numbers.py and tools/evidence/xxh3.mjs at a7a9e608
environment: null
---

## Observation

Q-COMBAT-018 retains the archive supplying restored effect rows and count;
Q-COMBAT-047 retains the same provenance for the two creature tables.
The nonrecursive directory census used in FND-COMBAT-089/095 was repeated
with requested tag SAVE, using the same 26 top-level installed GFF files.
Every file's size and XXH3-128 matches its entry in
BLD-GOG-EN-1.1.files.yaml. No changed working archive was included as
though it were the shipped input.

The inspected names are CHARSAVE.GFF, DARKRUN.GFF, DARKSAVE.GFF,
GPLDATA.GFF, OBJEX.GFF and RESOURCE.GFF, plus the twenty RGN archives
with suffixes 001, 032, 033, 034, 036, 037, 038, 039, 03A, 03B,
03C, 03D, 03E, 03F, 041, 042, 043, 044, 045 and 0FF.
All 26 lack a SAVE type table. This is a directory-table census, not a
payload search for those four characters.

In particular, both initial DARKRUN.GFF and DARKSAVE.GFF are 991 bytes
with XXH3-128 33f7aba0d5509aaa1767266088c536eb. Their directory starts
at decimal 959 and contains one type table. A source with that unchanged
directory cannot directly supply SAVE/5, SAVE/6, SAVE/24 or SAVE/26.

The same invocation with GFFI reproduces the plain-table controls:
GPLDATA.GFF has five resources, OBJEX.GFF seven and RESOURCE.GFF twenty;
the other 23 lack that table. PORT reproduces the indexed-table control:
178 resources in GPLDATA.GFF and none in the other 25. FND-GFF-003
independently records those structures and counts. Both encodings are
therefore positively exercised; this is not an empty-directory result.

FND-SAVE-021's earlier cloud-save example is historical evidence, not a
current control: its cloud_saves/DARKRUN.GFF path is absent on this
checkout's reference installation. An invocation on that absent directory
enumerates no archives and supplies no positive or negative archive claim.
No saved payload or effect-row value was inferred from that invocation.

## Interpretation

The unchanged inspected shipped archives can be removed as alternative
SAVE providers in Q-COMBAT-018 and Q-COMBAT-047. Finding the working
archive's shipped name in an open call is insufficient to admit a saved
resource: the relevant contents must come from later writing, copying or
another admitted input. FND-SAVE-006 records creation of the initial
working copy; FND-SAVE-021 records numbered writing and copying.
Neither this census nor those operations' existence proves native success.

FND-CONFIG-040's fallback traversal cannot manufacture a missing table
from any unchanged file in this set. It can still reach an archive with
changed contents or a file outside this census. Actual handle/list state,
traversal mode, directory lifetime and selected lengths remain open, as
do transfer results, buffer aliases and postprocessing. This removes the
shipped-provider uncertainty without establishing a live restoration or
promoting any rule or parity status.

## Alternatives

- A shipped working archive and a later working archive have different
  possible contents despite sharing a pathname.
- These 26 files exclude disc-image members, nested paths, numbered SAV
  files, external inputs and archives created or changed during play.
- No SAVE table in this set does not prove a live SAVE request must fail.
- An absent control directory cannot support an archive-content conclusion.

## How to reproduce

Resolve the licensed install with tools/game-dir.mjs. Verify exactly the
26 names above, hashing each through tools/evidence/xxh3.mjs and checking
both size and hash against BLD-GOG-EN-1.1.files.yaml. Reject a changed
file set or fingerprint rather than reusing this negative census.

Using locked evidence Python, run gff_tag_numbers.py with that directory
and SAVE, GFFI and PORT separately. Compare the controls with FND-GFF-003
and FND-COMBAT-089. The procedure expands indexed ranges as well as
plain entries. Retain the output only under
GAME_DIR/analysis/hit-incoming-20261104/shipped-save-provider-census.txt.
Do not treat the historical cloud-save path as an extant control.
No original payload, save or extracted bytes are committed.

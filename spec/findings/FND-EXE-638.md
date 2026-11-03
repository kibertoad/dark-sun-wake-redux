---
id: FND-EXE-638
title: Regenerated resident metadata admits the repaired joined function inventory
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: codex
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008DDE9..0x0008DFC4
tool: Ghidra 12.1.3; Temurin JDK 21.0.12.1; repository reconciliation scripts at 72c54f88; pinned standard-coverage; inventory_tails.py at 106df5d0
environment: null
---

## Observation

FND-EXE-637 leaves the repaired overlay candidate awaiting a resident view
and join/staging checks. A fresh plain import of the supported installed
executable supplies the missing resident snapshot. No game process runs.

The frozen import exports twice with identical inventory, provenance and
region files. It has 1,284 functions and one entry without an instruction.
Its 215 initialized regions define the resident export selection. On a
disposable copy, the documented trampoline, overlay-block, near-transfer,
declared-entry, declared-flow and body-fill sequence produces the repaired
resident view. The second trampoline pass changes no header. Declared
entries report zero failures; the flow pass reports two conflicts, rather
than a conflict-free analysis. The final fill adds 18 bytes, with none held.
The retained logs include the analyzer warnings and intermediate gaps.

Two read-only exports of the repaired view agree byte for byte. They contain
2,356 resident functions, 229,923 body bytes, no bytes outside the selected
regions and no entries without instructions. After the existing join's
segment-range normalization, every resident TSV line matches the committed
FND-EXE-601 inventory: no added, removed or changed resident body. Thus the
intermediate analyzer diagnostics have not been concealed as a new resident
denominator or a complete reading.

The retained FND-EXE-637 overlay candidate also has identical repeated
inventory, provenance and region exports. Source fingerprints and the
exporter revision match. Regenerating the overlay plan checks all 49
overlays with zero mismatches, 8,262 fixup writes and 854 targets.

The supported join accepts 2,356 resident and 874 overlay functions:
3,230 total functions and 484,530 summed and unique body bytes, with no
unexpected entries without instructions or bytes outside regions. All
resident lines remain unchanged. The only overlay differences are exactly
FND-EXE-637's three internal-entry merges and three repaired tails: the
function count drops by three and the body union grows by 217 bytes.

The region audit also changes analyzer classifications without changing any
region boundary. Three resident regions reclassify respectively eight, two
and two bytes from undefined to data, all outside function bodies; their
instruction counts and body membership are unchanged. One overlay region
similarly reclassifies two outside-body bytes from undefined to data. In the
region containing the repaired tails, six undefined bytes become instructions
and 211 already decoded instruction bytes cease to be outside function
bodies, accounting for the 217 added body bytes. Every other region counter
is unchanged. These data/undefined labels are analyzer metadata, not new
resource-format or gameplay claims.

The pinned coverage reporter passes on an isolated metadata copy containing
the staged inventory, provenance and regions, with an empty problems list.
The bounded final-range census checks all 874 joined overlay rows: 866
terminal returns and eight unconditional jumps, with no other classification.
It explicitly skips the 2,356 resident rows. The three FND-EXE-633 controls
retain their documented repaired exclusive ends.

## Interpretation

The repaired overlay candidate can now replace the installed joined inventory
through the supported regeneration and staging path. This settles Q-EXE-032's
bounded inventory-boundary question. The dependent numbered-load closure
plan's inventory fingerprint must follow the accepted file; its assignments
remain on unchanged functions.

Neither the tail census nor the metadata reconciliation proves reachability,
whole-body completeness or complete readings. They promote no gameplay claim
or parity row. Other research and the full Audit remain open.

## Alternatives

- Accepting only an overlay export would omit the resident input. The fresh
  resident import, exact resident-line comparison and joined staging check
  supply that missing evidence.
- Treating terminal transfers as proof of complete function bodies would
  exceed the census's linear final-range contract. This result makes no such
  claim, and retains the intermediate analyzer diagnostics locally.
- The first combined repaired export exceeded Windows' command-line limit;
  it produced no accepted metadata. A shorter retry had a duplicated script
  delimiter and also failed. Only the subsequent separate successful exports
  and their completion markers supply this finding's repaired metadata.

## How to reproduce

Use the installed source and mapped-image identities in FND-EXE-637. Follow
docs/EVIDENCE-TOOLS.md and docs/GHIDRA.md with the documented local Ghidra/JDK.
Use the project-local evidence Python for script-path lookup. Import the
installed executable into a fresh project, then freeze it and export twice
read-only with initialized regions. Copy that project before applying the
documented resident reconciliation sequence, including both near-transfer
passes and FillInstructionBodies. Export the saved repaired snapshot twice
read-only using the plain import's 215 regions. Invoke each export separately
to stay below Windows' command-line bound.

Compare all three export files and check script completion markers, not only
process exits. Join this resident view with the retained canonical overlay
view through tools/evidence/join-overlay-views.mjs. Compare normalized TSV
lines with Compare-Object: resident lines have no differences; the complete
overlay delta is the one recorded in FND-EXE-637. Run standard-coverage
with --root pointing to an isolated copy of spec and coverage containing the
staged three inventory files, then inventory_tails.py on the joined TSV.

Exact invocation arguments and logs are retained under
GAME_DIR/analysis/inventory-reconcile-20261103/: runs/resident-import-01,
runs/resident-frozen-01, runs/resident-repair-01 and runs/resident-export-03.
The frozen project is INST-resident-20261103a; its repaired copy is
INST-single-20261103b. join.json identifies both input views; staged-01 holds
the accepted join, staging-check-01 its isolated check inputs,
staging-coverage.json the pinned report and tails-joined.json the final census.
Failed export attempts remain in runs/resident-export-01 and
runs/resident-export-02. Original-derived outputs stay local.

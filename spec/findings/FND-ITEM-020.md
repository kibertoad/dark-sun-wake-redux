---
id: FND-ITEM-020
title: Inventory-rooted direct incoming coverage confirms the three known hit calls while retaining unresolved transfers and undecoded ranges
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
    offset: 0x00000000..0x0009AE30
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00064FFA..0x00064FFD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00065579..0x0006557C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007E7CE..0x0007E7D3
tool: tools/research/exec-census/reach_config.py and inventory_boundaries.py at 6a8eab33; tools/evidence/report.mjs x86-incoming; reader 4.3.0, engine 19.9.0, Capstone 5.0.9, pypcode 4.0.1
environment: null
---

## Observation

Q-ITEM-009 needs direct incoming coverage beyond FND-PARTY-096's earlier
caller search. FND-ITEM-018 reads the three known continuations, and
FND-ITEM-019 enumerates adjacent relocated pairs. This uses the published
incoming reporter with the existing inventory-backed mapping builder to
qualify direct signatures from declared entry paths and retain everything
that prevents closure. No new scanner or original execution was needed.

### Source, domains and roots

The verified source is installed DSUN.EXE, size 634416 and XXH3-128
e296af55ba2ecde7e77f555c90f33d0b. The target is canonical file 6500F,
overlay 179 +0D2F. The inventory is
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv at 6a8eab33; its SHA-256 after LF
normalization and removal of any byte-order mark is
69555e580c3964e90ec43822629c29e3b909ecc264353b4dec0ccc82bdbf76ea.

The builder declares 205 search regions, totaling 588,250 bytes: 49 complete
overlay code payloads and 156 nonempty resident descriptor spans with flags
zero or one. Resident spans use FND-EXE-571's mappings at load segment
1000. Overlays use explicit analysis segment 9000 plus descriptor and IP
zero. A resident span is a search extent, including any data it contains,
not an assertion that every byte is an instruction. Raw candidates are not
promoted merely because their bytes fall in that extent.

The regions declare 2376 entry offsets from the inventory and canonical
trampoline targets. The inventory has 3230 rows. The independent existing
boundary audit accounts for 854 exact five-byte trampoline rows, zero short,
long or interior trampoline rows, and no trampoline lacking an inventory
start or coverage. These resident stubs are mapped through their source
trampolines, rather than seeded as ordinary payload bodies. This accounting
does not establish that every possible function has an inventory row or
that every declared root is reached by a native game path.

The builder's supplied relocation list is removed before querying the
published reader, which derives MZ relocations and FBOV fixups from the
verified source. Its heuristic indirect-jump declarations are also removed;
no guessed jump table is made exhaustive for this report. No call models,
no-return assumptions or external-service preservation are introduced.

### Direct matches and controls

The report confirms exactly three matching entry-path calls under these
declared roots:

| Call file site | Source location | Encoding |
|---|---|---|
| 64FFA | Overlay 179 +0D1A | Relative 16-bit near call |
| 65579 | Overlay 179 +1299 | Relative 16-bit near call |
| 7E7CE | Overlay 193 +04DE | FBOV-fixed far call through 56A7:006B |

There are zero additional matching raw candidates and zero matching
contested calls. This qualifies the call sites FND-ITEM-018 already reads;
it is not a claim that these are every possible incoming route.

Required call controls are those three sites, resident MZ far call 0D46B
from FND-CONFIG-136 and FBOV resident call 71463 from FND-SAVE-023. The
last two are the instructions whose segment operands FND-SCRIPT-030
independently controls. All five are verified entry-path instructions with
resolved targets. Table-count controls independently require 4703 MZ
relocations, 229 descriptors, 49 overlays, 8262 fixups and 854 trampolines.

The byte scan uses limit 1,048,576; the instruction limit is 588,250;
the result limit is 10,000. All 205 coverage rows are nonpartial:
partialSearch is false. The full result is not truncated. This is complete
byte scanning of the declared extents, not complete path interpretation.

### Remaining gaps are part of the result

The report retains 896 unresolved call rows. Of these, 106 are entry-path
computed calls. The other 790 are raw candidates: 581 are inside reached
instructions and 209 are in undecoded ranges. They are not 790 additional
established calls. The unresolved provenance includes 96 far candidates
without a declared relocation/fixup and 694 rows without a resolved mapped
target, alongside the 106 computed calls.

There are 256 unresolved computed transfers, 674 undecoded ranges and
365 walk gaps: 256 computed-transfer gaps, 107 hardware/interrupt boundaries
and two undecoded/unmapped edges. The latter sites are file 574EC and
458E6. These counts refer to different report categories and must not be
added as a count of independent missing functions or callers.

negativeUsable is false. The three positive matches, unresolved rows and
path gaps are retained. Unrelocated far calls, undeclared mappings and
off-path prefix starts remain explicit exclusions. Runtime incoming use,
split targets and alias/frame admission are not established by this census.

The complete repeated report is byte-identical. Setting only result limit
to one retains full counts but returns one confirmed row and explicitly
sets truncated true and negativeUsable false. Moving the first required
control from 64FFA to 64FFB fails with a missed/not-verified control error.

## Interpretation

This removes the need to repeat an uncontrolled direct-signature search
for the known hit target. Its three matching sites now have supported
entry-path provenance in a controlled, nonpartial declared-domain scan.
It does not remove the report's unresolved transfer or path obligations.

Q-ITEM-009 retains the named unresolved/computed and unmapped mechanisms,
split/unrelocated/runtime construction, actual admission and preservation,
and result consumers beyond the already-read callers. Reuse this report
and FND-ITEM-019 before another incoming search. No complete_reading,
confidence promotion or native gameplay claim follows.

## Alternatives

- Matching a call byte sequence alone is insufficient; all three known
  sites pass entry-path controls and source-derived target resolution.
- A nonpartial byte scan does not mean all entry paths were read.
- Raw calls inside other instructions need an overlapping start; they are
  not silently added to the native call graph.
- The 854 trampoline rows are not omitted ordinary payload functions.
- Removing unproved indirect-jump models preserves unresolved transfers,
  rather than interpreting them as exhaustive dispatch coverage.

## How to reproduce

Use the locked evidence Python and verified installed <dsun>. A local query
JSON contains starts ["179+0D2F"], targets [], controls [] and leaves [].
Generate its mapping with the committed builder:

```text
python -I tools/research/exec-census/reach_config.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv <preparation.json> <query.json>
python -I tools/research/exec-census/inventory_boundaries.py <dsun> coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv
```

For the reader input, retain source, xxh3, sourceKind, regions,
instructionLimit and limit. Remove relocations, indirectJumps, starts,
targets and leaves. Set target 0x6500F, scanLimit 1048576 and controls
[0x64FFA, 0x65579, 0x7E7CE, 0x0D46B, 0x71463]. Set formatControls to the
five source-table counts above. Set segments to each resident region's
name/start/end/evidence, retaining the complete descriptor domain bounds.

```text
node tools/evidence/report.mjs x86-incoming <reader-config.json>
```

Repeat unchanged. For the cap control change only limit to one; for refusal
change only the first call control to 0x64FFB. Reports/configurations stay in
GAME_DIR/analysis/hit-incoming-20261104/: direct-input.json,
inventory-preparation.json, hit-direct.json, hit-direct.report.json,
hit-direct-repeat.report.json, hit-direct-cap.json,
hit-direct-cap.report.json, hit-direct-missed.json,
hit-direct-missed.refusal.txt and inventory-trampoline-boundaries.txt.
The accepted full report SHA-256 is
c137cddd1985bc9c6fe8f7f77dfd4f2cbda9271b17ac07a61cce9b4d0bac2afd.
No original listing, instruction bytes or executable-derived tables enter Git.

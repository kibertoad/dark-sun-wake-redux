---
id: FND-EXE-595
title: The restaged DSUN.EXE inventory has 3,261 rows, a five-byte row for each of the 854 trampolines, no row at 1BF3:40DD, and a row at every walk routine but 15 one-byte iret targets; 19,117 walk instructions stay outside its ranges
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:3C90..1BF3:423F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0020..5664:005C
tool: Ghidra 12.1.3 with tools/ghidra/SetFbovTrampolineBodies.java, AddFbovOverlayBlocks.java, ApplyDeclaredEntries.java (tools/ghidra/dsun-declared-entries.json), ClipBodiesToEntryRegion.java, ExportResearchBaseline.java and ReportEntriesWithoutInstruction.java, tools/ghidra/fbov_overlay_plan.py, tools/evidence/join-overlay-views.mjs, and tools/research/exec-census/inventory_boundaries.py and walk_inventory_coverage.py at the commit that adds this finding; standard-checker standard-coverage; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The inventory `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` was staged again by the join in
`docs/EVIDENCE-TOOLS.md` from two Ghidra 12.1.3 views, snapshots `INST-single-20261021` and
`INST-overlay-20261021`, whose two exports each repeat byte for byte.

The resident view starts from a copy of the plain import `INST-resident`. On it, before and after
the overlays are added as blocks and reanalyzed, each of the 854 trampolines of the 49 overlay
headers is given a function of its own five bytes: its `INT 3Fh` decoded alone with no
fall-through, its `target` word and `unk_04` byte as data. The first pass removes 36 functions
whose bodies ran from a header's trampolines into the bytes after `INT 3Fh`, the next header or
resident code; the second changes nothing. The declared entries then remove the functions at
`1BF3:40DD` (FND-EXE-593), `3F96:0375`, `3F96:0381` and `1BF3:5334` (FND-EXE-594), decode
`1BF3:3C90` through `1BF3:423F` and `3F96:0371` through `3F96:03C2` again, and create functions
at 235 resident starts of FND-RNG-058's walk that started no row (`1000:01A7` splits
`1000:019E`). In the overlay view, a copy of `INST-overlay-owned`, they create functions at the
walk's 37 overlay starts, and the clip to each entry's code range removes nothing. The 15
one-byte `iret` routines of FND-EXE-594 get no function.

The join accepts 2,371 resident and 890 overlay rows, 3,261 in all, with 425,659 body bytes and
no body shared by two rows; `2707:048E` and `0x00065D5C` remain the two starts without an
instruction. Against this inventory:

| Check | Committed before | Restaged |
|---|---|---|
| Rows | 2,338 | 3,261 |
| Trampoline rows of its five bytes only | 50 | 854 |
| Trampoline rows that are short, long or start inside one | 149 | 0 |
| Trampolines in no row | 81 | 0 |
| Row at `1BF3:40DD` | yes | no |
| Walk routines (of 2,124) that start a row | 1,837 | 2,109 |
| Walk instructions (of 165,558) outside every range | 27,443 | 19,117 |
| Mapped bytes of the 264 regions in bodies | 404,058 (67.9%) | 425,659 (71.5%) |

The row `1BF3:3C90` is `1BF3:3C90..1BF3:423F`, and a trampoline row such as `5664:0020` covers
`5664:0020..5664:0025`; the header's twelve rows cover `5664:0020..5664:005C`. The 15 walk
routines without a row are the `iret` bytes, two of them inside a row's ranges and 13 in none.
Of the 19,117 walk instructions outside every range, 4,862 lie in 85 resident routines and 14,255
in 111 overlay routines, and 25 more lie partly outside. `standard-coverage` reads the inventory,
its provenance and regions files and measures it.

## Interpretation

No trampoline row decodes past `INT 3Fh` or joins resident code, each trampoline is a row, and
`1BF3:40DD`'s bytes belong to `1BF3:3C90`. Every routine FND-RNG-058's walk reaches starts a row
except the 15 `iret` bytes, which share their byte with another row's instruction. The walk's
instructions outside the ranges lie in routines that start rows: the walk decodes code the Ghidra
bodies do not hold, which the walk reached through transfers Ghidra did not follow.

## Alternatives

- The walk's routine set is one walk from 21 entries; routines it does not reach were not
  compared with any other decode, and a row the walk never enters may still start wrongly.
- The 19,117 instructions were not traced to the transfers that reach them; the computed
  transfers the walk's query declares (FND-RNG-050) are a lead, not a finding.
- The walk was not run again over the restaged inventory, from which `reach_config.py` takes its
  declared starts.

## How to reproduce

Copy the snapshots `INST-resident` and `INST-overlay-owned` to new projects. Write the plan with
`python -I tools/ghidra/fbov_overlay_plan.py <dsun> <INST-MAPPED-RELOC-V2.EXE> <mapping> <plan>`
(it prints `mismatches 0`). On the resident copy, without `-readOnly`, run
`SetFbovTrampolineBodies.java <plan>`, `AddFbovOverlayBlocks.java <plan>`,
`SetFbovTrampolineBodies.java <plan>` again and `ApplyDeclaredEntries.java
tools/ghidra/dsun-declared-entries.json resident`; on the overlay copy `ApplyDeclaredEntries.java
<entries> overlay <mapping>` and `ClipBodiesToEntryRegion.java` with the 49 mapped code ranges.
Export each copy twice with `ExportResearchBaseline.java` and the regions of the earlier exports
(215 resident, 49 overlay), list the starts without an instruction, and join with
`node tools/evidence/join-overlay-views.mjs <config> <staging>`, expecting `2707:048E` and
`6F0E:1A7C`. Then, in `tools/research/exec-census/`, `inventory_boundaries.py <dsun> <inv> --walk
<walk table> 1BF3:40DD 1BF3:40E4` and `walk_inventory_coverage.py <dsun> <inv> <walk report>`,
with FND-RNG-061's routine table and FND-RNG-058's report; the same two commands on the earlier
inventory give FND-EXE-593's and FND-RNG-061's counts. Logs, exports, the join configuration and
the reports are kept under `GAME_DIR/analysis/exe-inventory-20261021/restage/r2/`. Nothing was
executed.

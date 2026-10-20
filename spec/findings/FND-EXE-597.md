---
id: FND-EXE-597
title: Ghidra 12.1.3 wraps near transfer targets at the 64K linear page; with them corrected and the walk's table jumps and runs declared, the DSUN.EXE inventory has 3,227 rows and 14 walk instructions outside its ranges
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:2B6A..2D40:2C87
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2707:048E..2707:04D5
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 4420:01F8..4420:020B
tool: Ghidra 12.1.3 with tools/ghidra/SetFbovTrampolineBodies.java, AddFbovOverlayBlocks.java, CorrectNearTransfers.java, ApplyDeclaredEntries.java (tools/ghidra/dsun-declared-entries.json), ApplyDeclaredFlows.java (tools/ghidra/dsun-declared-flows.json), ClipBodiesToEntryRegion.java, ExportResearchBaseline.java and ReportEntriesWithoutInstruction.java, tools/evidence/join-overlay-views.mjs, and tools/research/exec-census/walk_declared_flows.py, walk_inventory_coverage.py, resident_listing.py, overlay_listing.py, direct_callers.py and far_pointer_words.py at the commit that adds this finding; standard-checker standard-coverage; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

**Near targets.** A near relative call or jump keeps `CS`, so its target is
`CS:((IP after it + displacement) & FFFF)`. Ghidra 12.1.3 adds the displacement to the
instruction's linear address and wraps the sum at that address's 64K linear page instead. Where a
segment straddles a linear `0x?0000` boundary, a transfer whose target lies on the other side
gets a target 64K away. The jump `E9 86 00` at `2D40:2BAD` reaches `2D40:2C36`; Ghidra gives
`1BF3:4106`. The call `E8 BA F9` at `2D40:2EAF` reaches `2D40:286C`; Ghidra gives `3F96:030C`.
The call at overlay 189 `+038B` (mapped `7F7B:038B`) reaches `+13AC`; Ghidra gives `6F0E:1A7C`.
`CorrectNearTransfers.java` takes `CS` from the FBOV segment-table span that holds the
instruction (FMT-EXE-002, FND-EXE-571; table at `55E8:0000`, 229 descriptors, 205 spans) or, in
overlay code, from the segment the overlay's block starts at. In the resident view it finds 72 of
10,681 near transfers wrong (49 in `1BF3`, 23 in `2D40`); in the overlay view 117 of 20,150 (49 in
`1BF3`, 24 in `2D40`, 12 in `6F0E`, 20 in `7F7B`, 6 in `8E20`, 4 in `9FD6`, one each in `8BF3`
and `A406`). The same wrap gives near indirect jumps (`FF /4`) computed targets outside their
`CS`: 110 references in the resident view and 289 in the overlay view, among them
`2D40:24B1` to `2707:048F` and `1BF3:40D0`. Functions that only those wrong targets named are
removed: 13 resident and 30 in the overlay view, including `1BF3:4404`, `1BF3:4C91`, `1BF3:5334`,
`6F0E:1A7C` (file offset `0x00065D5C`) and overlay 197 `+19DF`. Overlay 209 `+0E2D` is kept,
since trampoline `57B0:0052` enters it. A second run after the declared flows finds 45 resident
and 58 overlay-view transfers wrong again, in code the flows decoded, and corrects them.

**Declared flows.** `tools/ghidra/dsun-declared-flows.json` declares, from FND-RNG-058's walk
report, the 158 table jumps the walk resolves (1,118 distinct targets) and 235 decode ranges: for
each of the walk's decoded ranges with an instruction outside the inventory of FND-EXE-595 or of
this restage, the range from that instruction to the decoded range's end. It also declares that
the far `memcpy` `1000:3F5A`, which Ghidra marks as not returning, returns (its one path ends at
the `retf` at `1000:3F7D`), and that the `int3` at `4842:04DA` falls through to `4842:04DB`.
`ApplyDeclaredFlows.java` removes the other computed targets of the declared sites (59 resident,
274 overlay view), decodes each range in step with the walk, clearing the units that are not its
instructions, and decodes `2707:048E`, a routine with near callers at `2707:05E6`, `2707:0624`,
`2707:0660` and `2707:069C` whose bytes Ghidra had decoded from the wrong target `2707:048F`.

**Inventory.** The join of the two views (snapshots `INST-single-20261021p` and
`INST-overlay-20261021p`, each exported twice with identical results) accepts 2,356 resident and
871 overlay rows with no start lacking an instruction. Against the inventory of FND-EXE-595:

| Check | FND-EXE-595 | Restaged |
|---|---|---|
| Rows | 3,261 | 3,227 |
| Trampoline rows of their five bytes only | 854 | 854 |
| Body bytes (mapped bytes of the 264 regions) | 425,659 (71.5%) | 483,124 (81.2%) |
| Rows that standard-coverage finds cited | 3,014 | 2,981 |
| Starts without an instruction | 2 (`2707:048E`, `0x00065D5C`) | 0 |
| Walk routines (of 2,124) that start a row | 2,109 | 2,109 |
| Walk instructions (of 165,558) outside every range | 19,117 | 14 |
| Walk instructions partly outside | 25 | 17 |

No row is new; 34 rows are gone, among them `1BF3:4404`, `1BF3:4C91`, `3EBE:001F`, `3EBE:09B6`,
`0x00066243` and `0x00065D5C`, whose bytes now belong to the routines that hold them. Thirteen of
the 14 outside instructions are the one-byte `iret` routines of FND-EXE-594 that have no row. The
fourteenth is the `jmp` at `4420:020B`, after the call of `1000:2E48` at `4420:0206`; `1000:2E48`
writes the stack-overflow message and jumps to the exit at `1000:03EE`, so nothing reaches
`4420:020B`. Of the 17 partly outside, 13 are the instructions that end on those `iret` bytes, and
four (`25AF:075C`, `455B:022F`, overlay 173 `+1BB1` and overlay 199 `+0D0D`) are instructions
whose function body leaves out one of their bytes.

## Interpretation

The 19,117 instructions were code Ghidra had not decoded or had decoded out of step: targets of
the table jumps it did not follow, fall-throughs it cut at calls it took for no-return, and code
that wrapped near targets misplaced or overlaid with false functions. With those inputs every walk
instruction that can execute lies in a row's ranges, and the inventory's count stays put when the
walk's inputs are applied again.

## Alternatives

- Removing a function that only wrong targets named assumes nothing else enters it.
  `far_pointer_words.py` finds no relocated pointer or far call to the resident ones without a
  walk row (`1BF3:48FA`, `2D40:1FBC`, `3F96:0650`); an entry through a
  computed far pointer built at run time is not excluded.
- The walk is one walk from 21 entries; code it does not reach is decoded only as Ghidra decodes
  it, with the wrong near targets corrected.
- The four one-byte holes in function bodies are left as Ghidra computes them.
- FND-IMAGE-011, FND-IMAGE-012, FND-PARTY-044, FND-RNG-056 and FND-RNG-061 name rows of the
  earlier inventories that this one no longer has; what they record about those inventories is
  unchanged.

## How to reproduce

Follow the DSUN reconciliation path in `docs/EVIDENCE-TOOLS.md`. On a copy of `INST-resident`:
`SetFbovTrampolineBodies.java <plan>`, `AddFbovOverlayBlocks.java <plan>`,
`SetFbovTrampolineBodies.java <plan>`, `CorrectNearTransfers.java apply 55E8:0000 229`,
`ApplyDeclaredEntries.java tools/ghidra/dsun-declared-entries.json resident`,
`ApplyDeclaredFlows.java tools/ghidra/dsun-declared-flows.json resident` and
`CorrectNearTransfers.java apply 55E8:0000 229` again. On a copy of `INST-overlay-owned`:
`CorrectNearTransfers.java apply 55E8:0000 229 <mapping> <DSUN.EXE>`, `ApplyDeclaredEntries.java
<entries> overlay <mapping>`, `ApplyDeclaredFlows.java <flows> overlay <mapping>`, the same
`CorrectNearTransfers.java` call again and `ClipBodiesToEntryRegion.java` with the 49 mapped code
ranges. Export each twice with the regions of the earlier exports (215 resident, 49 overlay),
list starts without an instruction, and join with `expectedWithoutInstruction` empty. The flows
file comes from `python -I tools/research/exec-census/walk_declared_flows.py <DSUN.EXE>
<FND-EXE-595's inventory> <FND-RNG-058's report> tools/ghidra/dsun-declared-flows.json --also
coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`; the counts come from `walk_inventory_coverage.py <DSUN.EXE>
<inventory> <FND-RNG-058's report>` and `npm exec -- standard-coverage --json`. Logs, exports, the
join configuration and the staged files are kept under
`GAME_DIR/analysis/exe-inventory-20261021/restage/r16/`. Nothing was executed.

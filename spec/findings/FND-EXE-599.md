---
id: FND-EXE-599
title: Six overlay routines that only a trampoline enters started no row of the DSUN.EXE inventory; declared as entries, they add six rows and leave every other row unchanged
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00057C95..0x00057CB0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005E4DF..0x0005E5FA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000630B0..0x000630D0
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0007CCFA..0x0007CD7C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008C4F1..0x0008C56C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0008E35D..0x0008E5A1
tool: Ghidra 12.1.3 with the scripts and order of FND-EXE-597 and tools/ghidra/dsun-declared-entries.json at the commit that adds this finding; tools/evidence/join-overlay-views.mjs; tools/research/exec-census/overlay_listing.py, direct_callers.py and walk_inventory_coverage.py; standard-checker standard-coverage; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

Each of the 854 FMT-EXE-004 trampolines (`CD 3F`, target word, byte) in the 49 overlay headers
of the installed `DSUN.EXE` names an offset in its overlay's code. In the inventory of
FND-EXE-597, 848 of those targets start a row. The other six start no row and lie in no row's
ranges; each starts with a `push bp` prologue, and its bytes sit in a gap between rows:

| Overlay | Offset | File range | Trampoline | Other references | What it does |
| --- | --- | --- | --- | --- | --- |
| 170 | `+0025` | `0x57C95..0x57CB0` | `5660:002A` | None found. | Takes a far pointer and stores the words 6, 0 and 0 there. |
| 173 | `+3BBF` | `0x5E4DF..0x5E5FA` | `5671:00DE` | None found. | Takes a word and walks the 320 object slots whose state byte at `4F49:0C33` is 2, comparing positions divided by 16 and calling `1B0B:000C` and `1B0B:00A4`. |
| 178 | `+0000` | `0x630B0..0x630D0` | `56A2:003E` | Pushed as a far pointer at `0x63541` (overlay 178 `+0491`). | Calls overlay 190 `+3456` with 10, the words at `+0x96` and `+0x98` of the record its far pointer argument names, and 0, and returns 0; overlay 175 `+0000` (FND-EXE-598) is the same with 9. |
| 191 | `+0B0A` | `0x7CCFA..0x7CD7C` | `572F:0057` | None found. | Opens with `inc bp` and ends with `dec bp` and a near `ret` (`C3`). Fills a stack item list through `2D40:2196`, asks overlay 173 `+2E1B` (FND-EXE-600) for an entry, and removes that entry through `2D40:2452` when its 23-byte record is the one its far pointer argument names. |
| 204 | `+0731` | `0x8C4F1..0x8C56C` | `5787:0039` | Pushed as a far pointer at `0x8C0B0` (overlay 204 `+02F0`). | Returns 0 when `DS:265C` is 0. For a slot whose 37-byte record at `DS:67BB` has a word `+0x16` other than `0x8000`, calls `31E0:1808`, toggles bit 6 of the record's first byte, and passes a byte from the 72-byte record under `DS:6573` that word names (its word 0 when the bit was set, its word `0x22` when it was clear) to `31E0:271E`; returns 1. |
| 204 | `+259D` | `0x8E35D..0x8E5A1` | `5787:00BB` | Pushed as a far pointer at `0x8BFC3` (overlay 204 `+0203`). | Dispatches through three `cs` tables (`jmp word ptr cs:[bx + 0x12]`, `+6`, `+0xC`) and calls overlay 206 `+0240`. Its row leaves out `0x8E49C..0x8E49F`, `0x8E4C7..0x8E4DC` and `0x8E4E8..0x8E500`. |

`direct_callers.py` finds no far or near call to any of the six besides its trampoline.
FND-RNG-058's walk does not reach them, so the declared entries of FND-EXE-593 did not name
them, and Ghidra made no function at them.

`tools/ghidra/dsun-declared-entries.json` now lists their file offsets among its overlay
entries. The restage of FND-EXE-597's procedure with that file (snapshots
`INST-single-20261021q` and `INST-overlay-20261021q`, each exported twice with identical
results) gives `ENTRIES_COMPLETE view=overlay created=43 existed=0 failed=0` and no start
without an instruction. The join accepts 2,356 resident and 877 overlay rows. Against the
inventory of FND-EXE-597:

| Check | FND-EXE-597 | Restaged |
|---|---|---|
| Rows | 3,227 | 3,233 |
| Body bytes (mapped bytes of the 264 regions) | 483,124 (81.2%) | 484,251 (81.4%) |
| Starts without an instruction | 0 | 0 |
| Walk instructions (of 165,558) outside every range | 14 | 14 |
| Walk instructions partly outside | 17 | 17 |

The six rows above are the only difference between the two inventories: every other row has the
same start, size and ranges.

## Interpretation

The inventory missed these six because nothing it was built from names them: the walk does not
reach them and the trampoline step makes functions only where Ghidra's analysis of the overlay
blocks keeps them. With the trampoline targets declared, every trampoline of the installed
executable enters a row. Three of the six are frame or callback handlers whose far pointers
overlay code passes; the other three have no reference besides their trampoline.

## Alternatives

- A routine entered only through a computed far pointer built at run time and not through a
  trampoline would still have no row; none was searched for here.
- Overlay 191 `+0B0A` reads its first argument at `[bp + 4]` and returns with a near `ret`, which
  does not match the far return a trampoline call expects; how the overlay manager enters and
  leaves it is not read.
- What the three `cs` tables of overlay 204 `+259D` hold, and whether the bytes its row leaves
  out are case code, is not read.

## How to reproduce

Read the trampolines of each overlay of `GAME_DIR/analysis/work-baseline/INST-mapping.json` from
the shipped `DSUN.EXE` at the header's file offset plus `0x20 + 5 * k`, and look up the overlay
code offset plus each target word among the starts and ranges of
`coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` of FND-EXE-597. Run `direct_callers.py` for each target
that starts no row, and search the file for `68 <descriptor * 8> 68 <trampoline offset>` pushes.
Restage as `docs/EVIDENCE-TOOLS.md` describes; the logs, exports and staged files are kept under
`GAME_DIR/analysis/exe-inventory-20261021/restage/r17/`. Compare the staged inventory with the one
of FND-EXE-597 row by row, and count with `walk_inventory_coverage.py` and
`npm exec -- standard-coverage --json`. Nothing was executed.

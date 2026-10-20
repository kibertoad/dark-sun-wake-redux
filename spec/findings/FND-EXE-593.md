---
id: FND-EXE-593
title: The installed DSUN.EXE inventory decodes the bytes after INT 3Fh in 149 trampoline rows, starts a row one byte into 1BF3:3C90's instruction at 1BF3:40DC that no transfer targets, and has no row for 284 of the census walk's routines
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:40D2..1BF3:40E4
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1BF3:3C90..1BF3:423F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5664:0020..5664:0060
tool: tools/research/exec-census/inventory_boundaries.py, direct_callers.py and far_pointer_words.py at the commit that adds this finding; Python 3.14.7, Capstone 5.0.9, xxhash 4.0.1
environment: null
---

## Observation

The function inventory checked is the committed `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`, staged by
the repository's join of Ghidra 12.1.3 views (its provenance file
names the snapshots `INST-single-20261010` and `INST-overlay-owned-20261009`). Addresses are
compared as linear load-image addresses, so `5664:0060` and `566A:0000` are one byte.

**Trampolines.** The FBOV segment table's flags-3 descriptors give 854 FMT-EXE-004 trampolines,
each five bytes from offset 0x20 of its header, and each starting `CD 3F`. 199 inventory rows
cover trampoline bytes:

| Rows | Kind |
|---|---|
| 50 | one trampoline's five bytes and nothing else |
| 20 | start a trampoline and cover 2 to 4 of its bytes, such as `5699:003E` (4) and `56BD:0020` (2) |
| 128 | start a trampoline and cover more than its five bytes: up to 17 trampolines (`5799:0057`), the next header's trampolines (`5664:0025`, through `566A:0025`), or resident code (`56A2:002A` also holds `3F96:0386..3F96:03C2`, `5699:0043` also `3F96:039A`) |
| 1 | `5671:00C7` starts two bytes into a trampoline and covers three |

656 trampolines start no row, and 81 lie in no row. The census rows FND-RNG-061 names,
`5664:0025`, `56A2:002A`, `56BD:0089` and `5773:002F`, are of the third kind and `5699:003E` of
the second; their decoded instructions after `int 0x3f` are the trampolines' `target` and
`unk_04` bytes and the next trampolines' bytes (`5664:0020..5664:0060` for the first).

**1BF3:40DD.** The row `1BF3:3C90` ends at `1BF3:40DC`, the first byte of the two-byte `add di, dx`
at `1BF3:40DC`, and the row `1BF3:40DD` covers `1BF3:40DD..1BF3:423F` (FND-RNG-061). No byte of
segment `1BF3`'s rows begins a near call, near jump, short jump, conditional jump or loop
(`E8`, `E9`, `EB`, `70..7F`, `E0..E3`, `0F 80..8F`) whose target is `1BF3:40DD` or `1BF3:40DC`; the
control target `1BF3:40E4` is found as the `jmp short` at `1BF3:40D2`, an instruction start of
the row's linear decode (`1BF3:40D2..1BF3:40E4`). No far call in the load image or overlay code
and no near call after `push cs` targets `1BF3:40DD`; the controls find the far call of `1BF3:3C90`
at file offset `0x35857` and the four far calls and one near call of `1BF3:738F`. No 16-bit word
of the file equals 0x40DD; the control `1BF3:61CC` finds its offset word at file offset
`0x172F0`. FND-RNG-061's walk from `1BF3:3C90` decodes `1BF3:40DC`, `1BF3:40E4` and the `retf` at
`1BF3:423E` (`1BF3:3C90..1BF3:423F`).

**Walk routines.** Of the 2124 routines of FND-RNG-058's walk, as `walk_routine_table.py` writes
them (FND-RNG-061), 1837 start an inventory row, 3 start inside a row's ranges (`1000:01A7`,
`4842:0774`, `4842:083F`) and 284 start in no row: 157 in segment `172C`, the overlay manager's 24 in `4AE5`,
16 in `1BF3` (`1BF3:639B` among them), 15 in `1000` (`1000:1D66`, `1000:2877` and `1000:29AA` to
`1000:2B6A` among them), 15 in `4842` (the timer handler `4842:040C` and 14 after it), 4 in `44B6`,
3 in `28C9`, `289B:02AE` and `289B:02BD`, one each in `1425`, `277B`, `2C5F`, `2D40`, `371D`,
`3EBE`, `3F96`, `409B`, `4420`, `44D0` and `55CE`, and 37 in overlay code, among them `0x00075560`
(1848 bytes), `0x00096F04` (1622) and `0x00098200` (1564). FND-RNG-061 counts 27,443
of the walk's 165,558 instructions outside every inventory range.

## Interpretation

The inventory's trampoline rows follow a decode that falls through from `INT 3Fh` to the bytes
after it. The overlay manager returns from that interrupt to the trampoline's first byte, which
it has rewritten as a far jump (FMT-EXE-004), so each trampoline is five bytes entered at its
start and no flow runs into the byte after it; the 149 rows of the second, third and fourth kinds
hold bytes no instruction stream executes, or join resident code to a trampoline.

No direct transfer and no stored offset word leads to `1BF3:40DD`, and it is not an instruction
start of `1BF3:3C90`'s decode, so the row is not a routine entry; the instruction at
`1BF3:40DC` and the bytes to `1BF3:423F` belong to `1BF3:3C90`'s body as far as the walk reads
it. The walk's 287 routines without a row of their own are code the inventory does not measure
or misassigns.

## Alternatives

- A computed transfer could still reach `1BF3:40DD`; none of the walk's resolved transfers does,
  and the walk from `1BF3:3C90` has none unresolved (FND-RNG-061).
- A walk routine start inside a row may be a second entry of that row's routine; the three
  were not read.
- Rows outside the trampolines and the walk were not compared with any other decode.
- No inventory was regenerated; the Ghidra views were not changed.

## How to reproduce

With the locked evidence Python, in `tools/research/exec-census/`, `<dsun>` the installed
`DSUN.EXE`, `<inv>` `../../../coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv` and `<walk>` FND-RNG-061's
routine table: `inventory_boundaries.py <dsun> <inv> --walk <walk> 1BF3:40DD 1BF3:40E4
1BF3:40DC`; `direct_callers.py <dsun> 1BF3:40DD 1BF3:3C90 1BF3:738F`; `far_pointer_words.py <dsun>
1BF3:40DD 1BF3:3C90 1BF3:61CC`. Outputs are kept under
`GAME_DIR/analysis/exe-inventory-20261021/`. Nothing was executed.

---
id: FND-EXPLORE-002
title: Slot footprints are square cell sets sized by a byte of the combatant details record, and an object numbered 430 or -430 occupies a 21-cell area instead
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:0F89..2D40:10AE
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:32C5..2D40:3559
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:3E64..2D40:3EBF
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:09CB..25AF:0B14
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:19B2..57E0:19B8
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:40DA..57E0:40E2
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`; the slot records are the 37-byte records at
`DS:67BB + slot * 0x25` (FMT-ACTOR-004), and `object_number` is the word at `0x1B` of a record.

- `2D40:3E64(slot, &a, &b)` returns 0 unless the slot is below 9,999 and the byte at
  `4F49:0C33 + slot * 3` is 2. It stores the word at `4F49:0C34 + slot * 3` in `a` and returns 0
  unless that is below 42; it stores the word at offset 4 of record `a` of the 49-byte table at the
  far pointer `DS:19C9` (FMT-COMBAT-001) in `b` and returns 0 unless that is below 17. Otherwise it
  returns 1.
- `2D40:3440(slot, &size, &cut)` sets both to 0 and calls `3E64`. When that returns 1, it takes
  byte `0x37` of record `b` of the 66-byte table at the far pointer `DS:19C5` (FMT-COMBAT-002)
  and stores its low four bits in `size` and its value divided by 16 in `cut`. `2D40:342A(slot)`
  calls it with the words `DS:19B2` and `DS:19B4`.
- `2D40:34B1(size, cut, prev, x, y, &cx, &cy)`: with `side = size + 1` and
  `half = side / 2`, it counts `index` up from `prev + 1` and returns -1 once
  `index >= side * side`. The cell of an index is `(x - half + index % side,
  y - half + index / side)`. When `side` is even it accepts the first index; when it is odd it
  accepts the first index whose cell is no further than `side - 1 - cut` from `(x, y)`, counting
  the column and row distances together. It stores the cell and returns the index.
- `2D40:32C5(slot, x, y, file, line)` does nothing unless the slot's byte at `4F49:0C33` is 2 and
  bit 7 of its record's first byte is clear. When `object_number` is 430 or -430 it calls
  `25AF:09CB(x, y, 1)`. Otherwise it calls `342A`, and for each cell `34B1` gives, starting from
  -1, calls `25AF:0969(slot, cx, cy, 1, first, file, line)`, `first` being 1 for the first cell
  only; then, when the slot is below 4, `25AF:06FA(slot, x, y)`.
- `2D40:3388(slot, x, y, file, line)` does nothing unless the byte at `4F49:0C33` is 2; it does not
  test bit 7. For 430 or -430 it calls `25AF:09CB(x, y, 0)`; otherwise it does as `32C5` with 0 for
  `place` and without the call to `06FA`.
- `2D40:0F89(slot, x, y, list)` calls `25AF:0ADF(x, y)` and returns 0 for 430 or -430. Otherwise,
  with the current words at `DS:19B2` and `DS:19B4`, it visits the cells `34B1` gives, and for each
  one where `25AF:00E1` returns a value other than 0 calls `25AF:0969(slot, cx, cy, 0, first, file,
  line)`, `first` being 1 for the first such cell, and appends the cell to `list`. It returns the
  number of cells appended.
- `2D40:1045(slot, count, list)` calls `25AF:0AFE()` for 430 or -430, and otherwise
  `25AF:0969(slot, cell, 1, first, file, line)` for each of the `count` cells of `list`, `first`
  being 1 for the first.
- `25AF:09CB(x, y, mode)` keeps a cell in the words at `DS:40DE` and `DS:40E0`. When `mode` is not
  0 it calls `25AF:05FB` on the 25 cells from 2 columns and rows before the kept cell to 2 after,
  keeps `(x, y)`, and calls `25AF:05AA` on the 25 cells around the new kept cell except its four
  corners, 21 cells. When `mode` is 0 it calls `05FB` on the 25 cells around the kept cell unless
  that is `(0, 0)` or `(0x84, 0x66)`, then on the 25 cells around `(x, y)`, and keeps
  `(0x84, 0x66)`. It returns 1.
- `25AF:0ADF(x, y)` copies the kept cell to the words at `DS:40DA` and `DS:40DC` and calls
  `09CB(x, y, 0)`. `25AF:0AFE()` calls `09CB` with the copied cell and 1.

In the file the words at `DS:19B2`, `DS:19B4` and `DS:40DA` to `DS:40E0` hold 0. The routine at
offset `0xAA8` of overlay 188 (`DSUN.EXE+0x00073948`) stores its argument's low three bits in
`DS:19B2` and the argument divided by 8 in `DS:19B4`. The load image holds seven far calls to
`32C5`, six to `3388`, two to `09CB`, from `32C5` and `3388`, and one each to `0ADF` and `0AFE`,
from `0F89` and `1045`. `0F89` is called from `2D40:1143` and `2D40:118E`.

## Interpretation

A slot of kind 2 occupies the cells of a square around its cell: `size + 1` cells a side, and
when the side is odd, only the cells within `size - cut` steps, so a `cut` of 0 keeps the whole
square and 1 drops its corners. A details byte of 0 gives the slot's cell alone. The size and cut
come from byte `0x37` of the slot's details record. The object numbered 430 occupies one area of
5 by 5 cells without its corners, and placing it again frees the area it held; the same number is
one of those whose image `31E0:0EFF` replaces (FND-ACTOR-003). `0F89` and `1045` take a slot's
own blocked cells off the map and put them back, which the movement routine does around its
route search (FND-EXPLORE-003).

## Alternatives

What values byte `0x37` takes, which objects are of kind 2, and what the object numbered 430 is
were not found. That the 21-cell area stands for one kind of object is inferred from the one
number. The overlay 188 routine that sets `DS:19B2` from three bits was read only at its start,
so how its callers use the footprint is not shown. The file and line arguments were not used by
the routines read.

## How to reproduce

Disassemble `2D40:0F89` to `2D40:10AE`, `2D40:32C5` to `2D40:3559`, `2D40:3E64` to `2D40:3EBF` and
`25AF:09CB` to `25AF:0B14` with the relocations applied, and overlay 188 from file offset
`0x73948`. Read the words at file offsets `0x4E9B2` to `0x4E9B7` and `0x510DA` to `0x510E1`.

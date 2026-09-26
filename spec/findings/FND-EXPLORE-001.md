---
id: FND-EXPLORE-001
title: The cell routines of segment 25AF read bit 6 of a cell as a block and set or clear bits 5 and 6 together for an occupant
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:00E1..25AF:014B
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 25AF:0576..25AF:09CB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2778:0006..2778:0035
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0528..57E0:0538
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:40E2..57E0:4202
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`. The far pointer at `DS:0538` names the buffer the region loader
fills with the region's `GMAP` resource, 128 bytes a row, the cell at column `x` and row `y` at
`y * 128 + x` (FMT-REGION-003, FND-REGION-005). Each routine below treats a null pointer, an `x`
of 128 or more or a `y` of 98 or more, compared as unsigned words, as a cell outside the map.

- `25AF:00E1(x, y)` returns 1 for a cell outside the map. When the word at `DS:19B6` is not -1,
  it calls `25AF:02FA` with that word and the cell's bits 0 to 2, and returns 1 when that returns
  0. Otherwise it returns the cell's byte masked with `0x40`.
- `25AF:0576(x, y)` returns the cell's byte masked with `0x20`, or 0 outside the map.
- `25AF:05AA(x, y)` does nothing outside the map, for the cell `(0x84, 0x66)`, or when the cell
  has bit 6 set; otherwise it sets bits 5 and 6 (`or 0x60`).
- `25AF:05FB(x, y)` does nothing outside the map, for `(0x84, 0x66)`, or when the cell has bit 5
  clear; otherwise it clears bits 5 and 6 (`and 0x9F`).
- `25AF:0969(slot, x, y, place, first, file, line)` returns 0 when the pointer is null. When the
  byte `first` is not 0 it calls `25AF:0742` with the slot, the addresses of `x` and `y`, `place`,
  `file` and `line`, and keeps what that returns; otherwise the result is 1. When the result is not
  0 it calls `05AA(x, y)` when `place` is not 0 and `05FB(x, y)` otherwise. It returns the
  result. Its callers pass a far pointer to a source-file name and a line number as `file` and
  `line`.
- `25AF:0742` returns 0 when the slot is 48 or more. It keeps, for each slot, a cell in the words
  at `DS:4142 + slot * 4` and `DS:4144 + slot * 4` and a number in the word at
  `DS:40E2 + slot * 2`, all 0 in the file. When `place` is not 0: if the kept number is not 0,
  the kept cell is not `(0x84, 0x66)` and `0576` returns a value other than 0 for the kept cell,
  it calls `05FB` on the kept cell; it then keeps the given cell and returns 1. When `place` is 0:
  if the kept number is 0, the kept cell is the given cell, or the given cell is `(0x84, 0x66)`,
  the result is 1; otherwise the result is 0, and it calls `05FB` on the kept cell when `0576`
  returns a value other than 0 for it. It then keeps `(0x84, 0x66)`. In both cases it stores the
  absolute value, as a signed word, of the word at `0x1B` of the slot's 37-byte record at
  `DS:67BB + slot * 0x25` as the kept number before it returns.
- `25AF:06FA(slot, x, y)` stores `x` and `y` in the words at `DS:0528 + slot * 4` and
  `DS:052A + slot * 4` when the slot is below 4; `25AF:071F(slot)` stores `0x81` and `0x63`
  there. The file holds `0x81` and `0x63` for each of the four slots.
- `25AF:0648()` visits the slots 0 to 3 and calls `05FB` on the stored cell of each slot whose
  byte at `4F49:0C33 + slot * 3` is 2 and whose record's first byte has bit 7 clear. `25AF:0692()`
  visits the same slots and calls `05AA` on the stored cell of each such slot whose 49-byte record
  of the table at the far pointer `DS:19C9` (FMT-COMBAT-001), the one the word at
  `4F49:0C34 + slot * 3` indexes, has a first word above 0.
- `2778:0006` takes no arguments. It reads the cell at the words `0x17` and `0x19` of the data
  segment it runs with, as `x` and `y`, without a bounds check, and returns -1 when bit 7 is set
  and 0 otherwise.

In the file the word at `DS:19B6` is -1. A search of the load image for far calls finds ten to
`25AF:00E1`, four to `25AF:0969` and one to `2778:0006`, at file offset `0x103DE`; `05AA`,
`05FB`, `0576` and `0742` are reached by near calls, which were not counted.

## Interpretation

Bit 6 of a cell blocks movement, and bit 5 marks a cell that an occupant blocked. Placing an
occupant sets both on a free cell, and removing it clears both only where bit 5 is set, so a cell
blocked in the region file, with bit 6 and without bit 5, never becomes free. `25AF:0742` keeps,
for each slot, the first cell of the last footprint placed, so that placing a footprint again
frees the old first cell, and removing a footprint whose first cell is not the kept one frees the
kept one and leaves the given cell. The four stored party cells let the game take the party's own
cells out of the map around a test and put back those of the members with hit points.

In the opening region's `GMAP` (FND-REGION-003), 8,131 cells hold `0x00` and 38 hold `0x80`,
so 8,169 cells have bit 6 clear. Bits 0 to 2 are 0 in every shipped cell (FND-REGION-003), so
`25AF:02FA` receives 0 for every cell until something sets them.

## Alternatives

`25AF:02FA` was read only as far as its start: it reads the slot's byte at `4F49:0C34` and, for
slots 0 to 3, bit 5 of byte `0x18` of the slot's FMT-COMBAT-001 record, so what it decides from
the low bits is not shown. What the bit 7 test at `2778:0006` is for, and what its caller keeps
in the words `0x17` and `0x19`, were not read. The callers of `25AF:071F` were not searched for.
An earlier reading counted seven call sites of `25AF:00E1`; the far-call search finds ten.

## How to reproduce

Disassemble `25AF:00E1` to `25AF:014B` and `25AF:0576` to `25AF:09CB`, and `2778:0006` to
`2778:0035`, with the relocations applied. Read the words at file offsets `0x4D528` to
`0x4D537`, `0x4E9B6` and `0x510E2` to `0x51201`, and search the load image for far calls to
each routine.

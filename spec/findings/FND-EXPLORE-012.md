---
id: FND-EXPLORE-012
title: The party loader places the leader at DS:140E and DS:1410, 1192 and 1496 in the load image, snapped to the centre of the nearest open cell, and 31E0:0121 stores that point in record words 0x0A and 0x0C and the figure's top-left, less the definition's offsets, in words 0x03 and 0x05
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x00068E8D..0x0006900C
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000692CC..0x000693A2
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:0121..31E0:03CA
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:435D..31E0:4474
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0004E3FA..0x0004E412
    kind: file-data
tool: Capstone 5.0.9 over overlay and resident code; tools/research/exec-census/overlay_listing.py, resident_listing.py and immediate_search.py at the commit that adds this finding; Python 3.14.7
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; overlay 182's code starts at file `0x68850`.
A slot record is the 37-byte record at `DS:67BB + 37 × slot` (FMT-ACTOR-004).

**The party loader, overlay 182 `+063D`** (FND-PARTY-013), for slots 0 to 3:

1. When the byte `DS:13FA` is not 0 or the slot is `leader` (`4C13:0369`), it copies the words
   `DS:140E` and `DS:1410` into two locals and calls `+0A7C` with their addresses (`+0664..+067A`).
   Otherwise it sets them to `0x840` and `0x660`.
2. It loads `CHAR 40 + slot` from `CHARSAVE.GFF`.
3. It calls `31E0:0121` with word `0x10` of the slot's 49-byte record plus 300, the slot, the two
   locals, 0, 7 and flags 0 (`+06D7..+06FF`).

**`+0A7C`** (file `0x692CC..0x693A2`) takes cell (x / 16, y / 16) and, for r from 0 to 19, tests
the cells of the square of radius r, columns outer and rows inner, for one inside the map (column
0 to 127, row 0 to 97) for which `25AF:00E1` returns 0. On the first, it writes 16 × column + 8
and 16 × row + 8 through the two pointers and returns 1; when there is none, it writes nothing and
returns 0.

**`31E0:0121`** fills the record from the object's definition through `31E0:0EFF` and `31E0:0E1B`,
so that byte `0x07` and word `0x08` hold the definition's x and y offsets (FND-ACTOR-003). With
flag bit 3 clear, it calls `572F:0039` on the point only when the slot's state byte at
`4F49:0C33 + 3 × slot` is 1. It stores the point in words `0x0A` and `0x0C` and its fifth argument
in byte `0x0E`, and calls `31E0:444A`, which stores `0x0A` less the signed byte `0x07` in word
`0x03`, and `0x0C` less word `0x08` and the signed byte `0x0E` in word `0x05`. With flag bit 3
set, the point would be taken as a top-left and the offsets added.

**`DS:140C..1411`** hold 50, 1192 and 1496 in the load image (file `0x4E40C..0x4E412`).
`immediate_search.py` with `140E` and `1410` finds stores only in `main`: the `-X` and `-Y`
switches at `2707:0935` and `2707:0957`, which act only with `-K911` (FND-SOUND-010), and the block
at `2707:0B01..0B13`, which stores 50, `0x4A8` and `0x5D8`.

**`31E0:435D`** moves the slots chained through word `0x21` of the record it is given by a
displacement and recomputes their words `0x03` and `0x05`; it writes nothing in the given record,
and `31E0:4129` is its only caller.

## Interpretation

On a new game the leader's point is (1192, 1496). In `RGN032.GFF`'s `GMAP 50` the cell (74, 93)
holds 0, so the snap keeps the point, the centre of that cell. The leader's object is 305 (`CHAR 40`
word `0x1A` is 5), whose `OJFF` gives the offsets 8 and 37, so the top-left is (1184, 1459). The
cell the movement code takes from words `0x0A` and `0x0C` is (74, 93). `31E0:435D` plays no part.

## Alternatives

The reading `(74, 91)`, from a position equal to the figure's top-left, is ruled out: the loader
passes flags 0, so `31E0:0121` stores the point itself in `0x0A` and `0x0C`. Block copies over
`DS:140E`, such as a saved-game restore, were not searched; `289B:0154`, `31E0:0D09` and the
loader's caller with a nonzero third argument were not read.

## How to reproduce

`overlay_listing.py <dsun> 182 68E8D 6900C` and `... 182 692CC 693A2`; `resident_listing.py <dsun>
31E0:0121..31E0:03CA` and `... 31E0:435D..31E0:4474`; `immediate_search.py <dsun> 140E 1410`; the
bytes at `0x4E40C..0x4E412`.

---
id: FND-REGION-006
title: The terrain routines read the map as 98 rows of 128 bytes and draw the named TILE at 16-pixel steps
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:00CA..362C:01CD
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:035C..362C:0625
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `362C:00CA` takes a tile number. It returns `0xFFFF` when the byte at `DS:29EA` is
0. Otherwise it looks for the number among the 260 words at `54AB:0514` and returns the index of
the match. When there is none, it picks a slot through the 260 bytes at `54AB:0410`, calls the
resource request `38FF:04AB` with the tag `TILE` (`0x454C4954`), the number zero-extended and
`DS:29F3` as the destination, and returns `0xFFFF` when the request fails. On success it stores
the number in the slot's word at `54AB:0514`, calls the image routine `2D40:3BEC`
(FND-IMAGE-005) with the loaded resource and the slot's two coordinates from `54AB:0208` and
`54AB:0000` (FND-REGION-005), sets the slot's byte at `54AB:0410` to `0xFF` and returns the slot.

The far routine at `362C:035C` takes a world position `(x, y)` as its second and third
arguments. It divides each by 16, keeping quotient and remainder. For each map row from `y / 16`
while the rows drawn fit the destination and the row is below 98, and each column from `x / 16`
while the columns drawn fit and the column is below 128 (`0x80`), it:

- reads the byte at offset `row * 128 + column` of the buffer the far pointer at `DS:29EF` names,
  zero-extends it and calls `362C:00CA` with it;
- skips the cell when that returns `0xFFFF`;
- otherwise copies a 16x16 area from the slot's position in the tile cache to the destination,
  starting the first column and the first row of cells at the remainders of `x` and `y` and
  shortening the last ones to the destination's size, through `2707:001A`.

The routine at `362C:04BF` reads the same buffer at `row * 128 + column` and draws through
`362C:00CA` and `2707:001A` the same way for a rectangle its arguments give.

## Interpretation

The map is 98 rows of 128 cells, row by row, and each cell's byte is the number of the `TILE`
drawn at `(column * 16, row * 16)` in world pixels. The game decodes each tile once into a cache
of 260 slots and copies it from there.

## Alternatives

The routine at `2707:001A` was not read, so whether it copies pixels a tile leaves undrawn, or
treats some index as transparent, is not shown. How the slot at `54AB:0410` is chosen was read
only far enough to see that it compares the bytes; which slot it evicts was not worked out.

## How to reproduce

Disassemble `DSUN.EXE` as 16-bit code from `362C:00CA` to `362C:01CD` and from `362C:035C` to
`362C:0625`, as FND-REGION-005 describes, with the relocations applied.

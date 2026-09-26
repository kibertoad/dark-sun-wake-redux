---
id: FND-EXPLORE-003
title: The movement and occupancy code takes a slot's cell from its position words shifted right by 4, which puts the opening leader on cell (74,93)
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:10AE..2D40:1197
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:1697..2D40:1716
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:4129..31E0:426E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:2914..31E0:2945
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000; far calls in overlays found through their fixup words and the segment table at file offset 0x4B080; Python byte searches of the whole DSUN.EXE
environment: null
---

## Observation

`DS` is the data segment `57E0`, and the slot records are those of FMT-ACTOR-004.

- `2D40:10AE(slot, flag)` maps the slot through `2D40:2F34` and does nothing when the result is 48
  or more or bit 5 of the record's first byte is set. It shifts the record's words at `0x0A` and
  `0x0C` right by 4, calls `2D40:342A` with the slot (FND-EXPLORE-002), and passes the two shifted
  words as the cell to `2D40:0F89` at `2D40:1143` when `flag` is not 0 and at `2D40:118E` otherwise.
  Further on, it stores the slot in the word at `DS:19B6` before each of two far calls to
  `1B2E:07F3`, at `2D40:16C2` and `2D40:1706`, with the byte at `DS:0443` at 1 before the first
  and 0 before the second, and stores -1 after each; the second call is made only when the first
  returns a value other than 0.
- `31E0:4129(slot, x, y)` does nothing unless the record's word at `0x23` is -1 and `(x, y)`
  differs from its words at `0x0A` and `0x0C`. It then calls `2D40:3388` with the slot and the old
  words shifted right by 4 when the slot's byte at `4F49:0C33 + slot * 3` is 2; calls `31E0:435D`
  with the record and the old words less `x` and `y` when the word at `0x21` is not -1; stores `x`
  and `y` at `0x0A` and `0x0C`; when the word at `0x01` is not negative, updates the slot's entry
  through `289B:0154` and sets bit 6 of the entry's byte 5; clears bit 5 of the first byte; and
  calls `2D40:32C5` with the slot and the new words shifted right by 4 when the byte at
  `4F49:0C33` is 2.
- The draw routine at `31E0:2837`, called from `31E0:2C2F`, takes the figure's rectangle from the
  words at `0x03` and `0x05` and the bytes at `0x13` and `0x14`; it does not read `0x0A` or `0x0C`.

`31E0:0E1B` fills `0x0A` and `0x0C` with an entry's first two words and `0x03` and `0x05` with
the same words less the definition's `x_offset`, and less its `y_offset` and `vertical_offset`
(FND-ACTOR-003). `OBJEX.GFF#OJFF/305`, the leader's definition, has an `x_offset` of 8, a
`y_offset` of 37 and a `vertical_offset` of 0 (FND-ACTOR-001), and the leader's figure has its
top-left at world `(1184,1459)` in the first gameplay frame (FND-ACTOR-002).

## Interpretation

The words at `0x0A` and `0x0C` are the object's position, the point its definition's offsets are
measured from, and the cell the movement and occupancy code uses is the position divided by 16.
The opening leader's position is then `(1192,1496)` and its cell `(74,93)`. An earlier reading
took the figure's top-left for the position and gave the cell `(74,91)`.

## Alternatives

The party slots are filled by the loader of FND-PARTY-013, whose position routine was not read,
and `31E0:435D`, which `31E0:4129` calls with the change of position, was not read either. That
the top-left is the position less the offsets is shown only for slots `31E0:0E1B` fills. If the
party loader kept the figure's top-left as the position, the leader's cell would be `(74,91)`. What
`1B2E:07F3` and `2D40:2F34` do was not read.

## How to reproduce

Disassemble `2D40:10AE` to `2D40:1197` and `2D40:1692` to `2D40:1716`, `31E0:4129` to `31E0:426E`
and `31E0:2837` to `31E0:2A80` with the relocations applied, and read the definition at offset
`0xEEDC` of `OBJEX.GFF`'s `OJFF/305`.

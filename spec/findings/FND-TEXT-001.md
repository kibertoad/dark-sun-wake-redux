---
id: FND-TEXT-001
title: The one FONT resource holds 256 glyphs of height 9 behind a map and an offset table
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x4E9755..0x4EB7C0
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#FONT/100` is the only `FONT` resource in the installed files, and the disc's
`RESOURCE.GFF` holds an identical copy. It is 8,299 bytes and reads as follows, with every byte
accounted for:

- At `0x00`, the 16-bit value 256, and at `0x02` the 16-bit value 9. The four bytes at `0x04`
  are 0.
- At `0x08`, 256 bytes that are the values 0 to 255 in order, XXH3-128
  `f1f8a93f50849ac39408a4433b952d71`.
- At `0x108` (264), 256 16-bit offsets from the start of the resource. The first is 776, the
  end of this table, and each later one is where the record before it ends.
- From 776, 256 records, each a 16-bit width `w` followed by `w * 9` bytes. The last record
  ends at the end of the resource.

Widths are 0 in 118 records, 2 in 2, 4 in 15, 5 in 15, 6 in 102 and 7 in 4. The 118 records of
width 0 include records 0 to 31. The pixel bytes take three values: 0 (3,585 bytes), 20 (2,002)
and 254 (1,424).

## Interpretation

The resource is a bitmap font of 256 glyphs, all 9 pixels high: a glyph count, a height, a
256-entry map from character codes to glyphs, and one offset per glyph to a record of its width
and its pixels, row by row, as palette indices. A glyph of width 0 has no pixels. Since the map is
the identity, character code `c` selects glyph `c` whether or not the game reads the map.

## Alternatives

The files do not show whether the map translates character codes or does something else, what
the four zero bytes are, whether pixels are stored row by row or column by column (the widths
fit either), or which palette index is transparent. With one font in the game, none of these
readings can be tested against a second resource.

## How to reproduce

Find `FONT/100` through the directory of `RESOURCE.GFF` (FMT-GFF-001), read the fields above, walk
the 256 records from the offsets and check that each starts where the previous one ends and the
last ends at the resource's end. Hash the 256 bytes at `0x08`.

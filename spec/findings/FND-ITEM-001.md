---
id: FND-ITEM-001
title: ITEMS.BIN is 234 pairs of 16-bit words, sorted by the first word except the last pair
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: ITEMS.BIN
    offset: 0x00..0x3A7
  - build: BLD-GOG-EN-1.1
    file: CD:ITEMS.BIN
    offset: 0x00..0x3A7
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`ITEMS.BIN` is 936 bytes (XXH3-128 `3e19d1fe0b0ea8b67389bafdb61897e3`); the disc's copy is byte for
byte the same. Read as little-endian 16-bit words, it divides into 234 pairs of four bytes with
nothing left over.

- The first words of the 234 pairs are all different. Pairs 0 to 232 are in strictly increasing
  order of their first word, and pair 232 holds the largest, 31,030. Pair 233, the last, holds
  the smallest, 775, and is the only pair out of order.
- The first words range from 775 to 31,030 and the second words from 603 to 31,990. No word has
  its top bit set.
- The second words take 159 different values across the 234 pairs.

## Interpretation

The file is a table of 234 records, each two unsigned 16-bit words, kept sorted by its first word
so that a search can find a record by that word. The table holds no count or header.

## Alternatives

The pairing and the byte order are read from the pattern of the values; the transfer utility's
reader, which reads the file two words at a time and compares the first word, confirms both
(FND-ITEM-006). Why the last pair is out of order is not known: it may have been appended after
the table was sorted.

## How to reproduce

Read `ITEMS.BIN` as 468 little-endian 16-bit words, group them in pairs, and check the order,
uniqueness and ranges of each column. Compare the file with `ITEMS.BIN` in the root of the disc
image's ISO 9660 volume.

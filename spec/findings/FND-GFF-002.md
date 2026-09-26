---
id: FND-GFF-002
title: A GFF directory lists one table per tag, and a plain table gives each resource's number, offset and size
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN001.GFF
    offset: 0x18F09..0x19941
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x573CE9..0x574455
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x28C3..0x2DD7
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In every `.GFF` file of both copies, the directory at the header's offset (FND-GFF-001) starts with
two little-endian 32-bit values and a 16-bit count:

- the first value is 8 in every file, the offset of the count from the directory's start;
- the second is the offset, from the directory's start, of the first byte after the last tag
  table: `0x76A` in `RESOURCE.GFF`, where the tables end at `0x574453`;
- the 16-bit count is the number of tag tables that follow, from 1 (`DARKRUN.GFF`) to 21
  (`RESOURCE.GFF`).

Each tag table starts with four printable ASCII bytes, the tag, and a 32-bit value. No file has two
tables with the same tag. When bit 31 of the value is clear, the table is plain: the value is a
count, and that many 12-byte entries follow, each three 32-bit values: a resource number, a file
offset and a size. `RGN001.GFF` has six plain tables; its `RNME` table holds one entry, number 1,
offset 28, size 7. Every file of both copies has plain tables only, apart from the tables that
FND-GFF-003 describes in `GPLDATA.GFF`, `OBJEX.GFF` and `RESOURCE.GFF`. In every table, the
numbers rise from entry to entry, and every offset and size gives a range between 28 and the
directory's offset.

The gap list (FND-GFF-004) follows the last table.

## Interpretation

A resource is found by its tag and its number: the directory gives, for each tag, the numbers the
file holds and where the bytes of each one are.

## Alternatives

The first prefix value could be a fixed marker rather than an offset, since it is 8 everywhere.
Whether the game searches a table in order, by bisection on the rising numbers, or copies it into
memory first is not shown by the files.

## How to reproduce

At the directory's offset, read the two 32-bit values and the 16-bit count, then walk the tables:
four bytes of tag, a 32-bit value, and for a plain table `value * 12` bytes of entries. Check that
the walk ends at the second prefix value. Do this for every `.GFF` file in the installation and in
the root of the disc's ISO 9660 volume.

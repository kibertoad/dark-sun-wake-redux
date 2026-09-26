---
id: FND-GFF-003
title: Indexed GFF tag tables give numbers as ranges and keep offsets and sizes in a GFFI resource
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x2141EF..0x2142EF
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x213E54..0x213E60
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x573CE9..0x574455
  - build: BLD-GOG-EN-1.1
    file: OBJEX.GFF
    offset: 0x675506..0x67748A
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In `GPLDATA.GFF`, `OBJEX.GFF` and `RESOURCE.GFF`, in both copies, every tag table but the first
has bit 31 of its 32-bit value set. The first table in each is a plain table (FND-GFF-002) with the
tag `GFFI`, whose entries are numbered 0, 1, 2 and so on. In a table with bit 31 set:

- the low 31 bits are a count, and three 32-bit values follow: the same count again, a number, and
  a range count;
- then that many ranges of two 32-bit values, a first number and a length. Expanding the ranges in
  order gives exactly `count` numbers, rising;
- the number names a resource in the `GFFI` table. Its bytes are a 32-bit count equal to the
  table's count, then that many pairs of 32-bit values, a file offset and a size, one pair per
  expanded number in order. The size of the `GFFI` resource is exactly `4 + 8 * count`;
- the `n`-th table with bit 31 set in a file names `GFFI` resource `n`, counted from 0.

In the installed `GPLDATA.GFF`, the `PORT` table at `0x21426B` has the value `0x800000B2`, a count
of 178, `GFFI` resource 2 and two ranges, 1 to 175 and 177 to 179; `GFFI` resource 1, the `GPLI`
table's, is the 12 bytes at `0x213E54`. The installed files have 5 such tables in `GPLDATA.GFF`, 7
in `OBJEX.GFF` and 20 in `RESOURCE.GFF`; the disc's `GPLDATA.GFF` has 4, lacking the one for
`TEXT`. The region files, `CHARSAVE.GFF`, `DARKRUN.GFF` and `DARKSAVE.GFF` have no `GFFI` table
and no table with bit 31 set.

## Interpretation

An indexed table stores a tag's resource numbers compactly as ranges and moves the per-resource
offsets and sizes into a `GFFI` resource of the same file, so a resource of an indexed tag is found
in two steps: its position among the expanded numbers, then that position in the `GFFI` resource.

## Alternatives

The number that names the `GFFI` resource could equally be read as a position in the `GFFI` table,
since the resources are numbered by position in every file. Whether the game expands the ranges or
searches them directly is not shown by the files.

## How to reproduce

Walk each directory as in FND-GFF-002. For a table with bit 31 set, read the three 32-bit values
and the ranges, expand them, then read the `GFFI` resource the table names through the `GFFI`
table's entry and check its count, size and pairs.

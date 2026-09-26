---
id: FND-GFF-004
title: A GFF directory ends with a list of the unused byte ranges between the header and the directory
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x2D9D..0x2DD7
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x2142EF..0x214371
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x214371..0x217249
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x574453..0x5759FD
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In every `.GFF` file of both copies, the last tag table is followed by a 16-bit count and that many
pairs of 32-bit values, a file offset and a size. The directory's size in the header
(FND-GFF-001) ends exactly after the last pair.

The pairs cover exactly the bytes between offset 28 and the directory's offset that no resource
uses, with no pair overlapping a resource. The installed `CHARSAVE.GFF` has 7 pairs over 1,616
bytes, from `0x120A` for 224 bytes; the installed `GPLDATA.GFF` 16 over 1,211; the installed
`OBJEX.GFF` 63 over 4,986; the disc's `GPLDATA.GFF` 11 over 1,243 and its `OBJEX.GFF` 68 over
6,716. Every other file has a count of 0, and no unused bytes before its directory.

After the directory, the installed `GPLDATA.GFF` has 11,992 more bytes, `OBJEX.GFF` 36,474 and
`RESOURCE.GFF` 5,544; on the disc, 12,072, 34,704 and 5,544. No resource and no pair covers them.

## Interpretation

The list records free space inside the data area, the space left when a resource was removed or
replaced by a shorter one, which a writer can reuse. The bytes after the directory are left over
from an earlier, longer layout of the file and are not part of the container.

## Alternatives

That the list is free space, and that the bytes after the directory are leftovers, fit every file
but rest only on where the ranges fall; nothing here shows code that reads or writes the list.

## How to reproduce

After walking the tag tables (FND-GFF-002), read the 16-bit count and the pairs. Mark every byte
from 28 to the directory's offset that a resource of any table uses, including resources of indexed
tags (FND-GFF-003), and compare the unmarked ranges with the pairs. Compare the directory's offset
plus its size with the file size.

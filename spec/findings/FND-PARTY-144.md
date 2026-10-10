---
id: FND-PARTY-144
title: Byte 0x29 of a CHAR record is the combatant's facing, a direction from 0 to 7 that the drawing code maps onto four sprite views
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:193E..28C9:1975
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 31E0:1190..31E0:11C6
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x0005C8D1..0x0005C918
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    offset: 0x000625AA..0x000625D8
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/field_reads.py, field_stores.py, resident_listing.py, overlay_listing.py; a scratch script reading the CHAR resources through the GFF directory as details_chunks.py does)
environment: null
---

## Observation

Overlay offsets are written `descriptor +offset`; file offsets are those of the installed
`DSUN.EXE`. A load copies a CHAR record's bytes `0x29` and `0x2A` to `+0x1F` and `+0x20` of the
character's FMT-COMBAT-001 record (FND-PARTY-049). The data segment starts at file `0x4D000`
(`DS:000C` is the text `C++ - Copyright 1991` and `DS:04C8` the text `o Spells in Queue`).

**Reads.** `field_reads.py <dsun> <inventory> --es --table 19c9 17 1f 20`, kept to hits whose
context multiplies by 0x31, the combatant record's size, finds reads of `+0x1F` at `28C9:194C`,
`28C9:1DC2`, `28C9:2D00`, `31E0:11A8`, `31E0:2A18` and overlay 201 `+0608`, none of `+0x17`,
and of `+0x20` only one outside the inventory, at overlay 204 `+2434`. At `28C9:194C` and `31E0:11A8` the byte is set to 0 when it is not 0 to 7 and indexes
the eight bytes at `DS:141C`, which hold 0, 8, 8, 8, 16, 24, 24 and 24; at `28C9:195F..28C9:1975`
the result, doubled, is used with a 0x48-byte record of the table at `DS:6573`, which the word
at `+0x16` of the record at `bp-4` selects (`28C9:193E..28C9:1975`, `31E0:1190..31E0:11C6`).

**Stores.** `field_stores.py <dsun> <inventory> --es 1f`, kept to stores after a load of
`DS:19C9`, finds three: overlay 183 `+136F` stores 0; overlay 177 `+04D4` stores the result of
`1BDF:009B` called with two pairs of coordinates (`+04AA..+04D8`); overlay 173 `+1FF4` stores
the result of `1BDF:009B`, called with the coordinates of two object slots (words `+0x0A` and
`+0x0C` of each record, shifted right by 4), plus 4, less 8 when that is 8 or more
(`+1FB1..+1FF8`). `1BDF:009B` compares the signs and sizes of the two coordinate differences.

**The shipped records.** Byte `0x29` is 0, 1, 2, 4, 5 or 6 in the 19 `CHAR` resources of the
installed `CHARSAVE.GFF`, and byte `0x2A` is 0 in all of them.

## Interpretation

Combatant byte `+0x1F`, and so CHAR byte `0x29`, is the direction the character faces, one of
eight. The game turns a combatant to face along the line between two positions, and overlay 173
turns one to face the opposite way (adding 4 of 8). Drawing reduces the eight directions to four
sprite views (0, 8, 16 and 24 frames into the sprite's set). A saved character keeps the facing it
had.

## Alternatives

- Which way direction 0 points, and the order of the others, was not read from `1BDF:009B`.
- Byte `0x2A` (combatant `+0x20`) was not read; the one read outside the inventory may start
  inside another instruction.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python, `<dsun>` the installed
`DSUN.EXE` and `<inventory>` `coverage/BLD-GOG-EN-1.1/DSUN.EXE.tsv`: in
`tools/research/exec-census/`, run `field_reads.py <dsun> <inventory> --es --table 19c9 17 1f 20`,
`field_stores.py <dsun> <inventory> --es 1f`, `resident_listing.py <dsun> 28C9:1930..28C9:1975
31E0:1190..31E0:11C8 1BDF:009B..1BDF:0143`, and `overlay_listing.py <dsun> 173 5C8D0 5C918` and
`overlay_listing.py <dsun> 177 625A0 625D8`. Read the eight bytes at file `0x4D000 + 0x141C`, and
bytes `0x29` and `0x2A` of each `CHAR` resource of `CHARSAVE.GFF`.

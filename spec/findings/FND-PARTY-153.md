---
id: FND-PARTY-153
title: RDFF resources load through the same chunk walk as CHAR records, and the objects 430 and 541 whose kill experience would overflow a share are placed only in the region Limbo
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 2D40:000A..2D40:00D2
  - build: BLD-GOG-EN-1.1
    file: RGN0FF.GFF
    offset: 0x8297..0x829F
  - build: BLD-GOG-EN-1.1
    file: RGN0FF.GFF
    offset: 0x8307..0x830F
tool: Python 3.14.7 with Capstone 5.0.9 and xxhash 4.0.1 (tools/research/exec-census/resident_listing.py; a scratch script reading every ETAB resource of the installed GFF archives through their directories as details_chunks.py reads a directory)
environment: null
---

## Observation

**The loader.** `31E0:0EFF` calls `2D40:000A` with a slot, the object number widened to 32 bits,
the tag `RDFF` and a byte, when bit 2 of its flags is set and the object number is below 9,000 or
above 13,998 (FND-ACTOR-003); the `CHAR` loads pass the tag `CHAR` to the same routine
(FND-PARTY-046, FND-PARTY-054). `2D40:000A` stores its byte argument at `DS:55B2`, returns -2 for
a number of 0 or less and -1 for a number from 9,000 to 13,998, requests the resource of the
tag and number through `38FF:05B5` and `38FF:04AB`, returning -1 when either fails, and then walks
the chunks (`2D40:000A..2D40:00D2`). Nothing in it compares the tag. The walk copies a type-1
chunk's data into the slot's FMT-COMBAT-001 record and a type-3 chunk's 66 bytes into a details
record (FND-PARTY-051).

**The data.** `RDFF` 430 and 541 of `OBJEX.GFF` start with a type-1 chunk header and hold a
details chunk at `0x3B`; read as a CHAR record, their byte at `0x1F` is 7 and their dword at
`0x49` is 107,000 and 33,000 (FND-PARTY-094). The share of a kill goes to the party when the
killed combatant's kind is 7 to 11, and is its details dword at `+0x04` divided by the filled
slots, kept as a 16-bit word (FND-PARTY-086).

**The placements.** Of the `ETAB` resources of the 20 installed region files, read as
FMT-REGION-005 records of 8 bytes with the object number at `+6`, the object number 430 or -430
occurs once, at record index 909 of `ETAB` 255 of `RGN0FF.GFF` (file `0x8297`, position 31, 1,383), and
541 or -541 once, at index 923 of the same table (file `0x8307`, position 1,167, 1,408). No other
region's table holds either. `RGN0FF.GFF` is region 255, named `Limbo` (FND-REGION-001), which the
start of the game opens with `OBJEX.GFF` (FND-PARTY-042).

## Interpretation

An `RDFF` resource loads as a CHAR record does, so when object 430 or 541 is placed as a
combatant its details record gets the kill experience 107,000 or 33,000. A kill of 430 with three
or fewer filled party slots, or of 541 with one, gives each member a share of 32,768 or more,
which the 16-bit share turns negative. Neither object stands in a region the party walks:
both wait in Limbo, so whether the party meets them depends on what moves them out of it.

## Alternatives

- Which script or routine moves an object out of Limbo into a region, and whether it does so for
  430 or 541, was not read.
- Whether `31E0:0EFF` is called with bit 2 of its flags for these objects, so that their `RDFF`
  is loaded at all, was not read.
- Objects may also be placed by script without an `ETAB` record.

## How to reproduce

From the commit that adds this finding, with the locked evidence Python and `<dsun>` the installed
`DSUN.EXE`: in `tools/research/exec-census/`, run `resident_listing.py <dsun>
2D40:000A..2D40:00D2`. Read the directory of each installed `*.GFF` archive as
`details_chunks.py` does, and for each `ETAB` resource print the records whose word at `+6`, read
as signed, is 430, -430, 541 or -541.

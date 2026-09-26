---
id: FND-REGION-004
title: Every ETAB is a list of 8-byte records that name OJFF objects in OBJEX.GFF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RGN032.GFF
    offset: 0xDC89..0xF7A1
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Each region file's `ETAB` resource is a multiple of 8 bytes long, 1,144 to 7,672. Read as records
of a 16-bit signed value, another, a signed byte, a byte and a 16-bit signed value, the 20 files
hold 13,559 records, and:

- The last field is 1 or more in every record, and each value is the number of an `OJFF`
  resource in `OBJEX.GFF`. Together the records name all 4,479 of them.
- The first two fields range from -129 to 3,559 and from 0 to 2,382. 154 records, in 17 files,
  lie outside 0 to 2,047 or 0 to 1,567.
- The signed byte is 0 in 13,255 records and otherwise 10 (22), 24 (5), 30 (2), 32 (11), 64 (263)
  or 85 (1).
- The byte after it takes 26 values, using only bits 0, 1, 2, 5 and 7: bits 0 to 2 take every
  value from 0 to 7, bit 5 is set in 8,128 records and bit 7 in 213.

`RGN032.GFF#ETAB/50`, located above, is 6,936 bytes: 867 records that name 287 objects.

## Interpretation

`ETAB` places objects on the region: each record is a position, a vertical offset, a byte of
flags and the number of the object's `OJFF` definition. The region's map covers 2,048 by 1,568
pixels (FND-REGION-002), so a few records place objects outside it.

## Alternatives

The field boundaries follow SRC-DSUN-MUSIC-79B6927 and fit every record, but the files alone do
not show which field is x and which y, or what the flags mean. That the last field is never
negative leaves open whether the game treats it as signed.

## How to reproduce

For each installed `RGN*.GFF`, read the `ETAB` resource through the file's directory
(FMT-GFF-001), split it into 8-byte records and tabulate the fields. Check each object number
against the `OJFF` numbers of `OBJEX.GFF`.

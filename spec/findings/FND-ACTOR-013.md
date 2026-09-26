---
id: FND-ACTOR-013
title: Split at 81 bytes, no column of the MONR resource is constant
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x56FBCB..0x570039
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`RESOURCE.GFF#MONR/1`, 1,134 bytes (FND-ACTOR-008), divides into 14 units of 81 bytes. Taken
column by column across the 14 units, every one of the 81 byte positions varies: each holds 5 to
9 distinct values, and 6 to 10 of its 14 bytes are 0.

## Interpretation

81 bytes is not the record size: a table of 81-byte records would repeat at least some bytes in
every record, as the 42-byte split does (FND-ACTOR-008).

## Alternatives

Records whose every byte varies would give the same statistics, so the result weighs against 81
bytes without ruling it out.

## How to reproduce

Read `RESOURCE.GFF#MONR/1` through the file's directory (FMT-GFF-001), split it into 14 rows of
81 bytes, and count the distinct values and zeros of each column.

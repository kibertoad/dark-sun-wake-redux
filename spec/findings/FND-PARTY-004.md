---
id: FND-PARTY-004
title: A CHAR record is a 79-byte header whose byte 1 counts the 33-byte records after it
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1C..0x131
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x1496..0x1527
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

In all 19 `CHAR` resources of the installed `CHARSAVE.GFF`, byte 0 is 1 and byte 1 is between 2
and 29, and the resource's size is exactly `79 + 33 * byte1`, with no exception and no byte left
over. The sizes run from 145 bytes (`CHAR/32`, byte 1 = 2, file offset `0x1496..0x1527`) to 1,036
bytes (`CHAR/30`, byte 1 = 29). `CHAR/40` is 277 bytes with byte 1 = 6, at `0x1C..0x131`. The
eight `CHAR` resources of the disc's `CD:CHARSAVE.GFF` are byte for byte the same as the installed
ones of the same numbers (FND-PARTY-005).

The per-record counts are:

| Byte 1 | Size | Resources |
|---|---|---|
| 2 | 145 | 32 |
| 4 | 211 | 34, 35, 36, 42 |
| 5 | 244 | 51 |
| 6 | 277 | 39, 40, 43 |
| 7 | 310 | 29, 37, 38, 41 |
| 12 | 475 | 53 |
| 13 | 508 | 50 |
| 19 | 706 | 52 |
| 21 | 772 | 31 |
| 26 | 937 | 33 |
| 29 | 1,036 | 30 |

## Interpretation

A `CHAR` resource is a 79-byte header followed by `byte1` records of 33 bytes each. Byte 0 is a
version or kind that is 1 in every shipped record.

## Alternatives

The equation holds for all 19 records, so a different split of the same sizes would need
another coincidence to hold as well. What the 33-byte records hold is not known, nor whether the
game accepts a byte 0 other than 1. No code that reads the header has been located.

## How to reproduce

List the `CHAR` resources of both copies of `CHARSAVE.GFF` through their directories, and for
each compare its size with `79 + 33 * byte1`.

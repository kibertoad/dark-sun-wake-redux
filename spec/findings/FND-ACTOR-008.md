---
id: FND-ACTOR-008
title: The one MONR resource divides into 27 units of 42 bytes with fixed zero bytes
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

`RESOURCE.GFF` holds one `MONR` resource, `RESOURCE.GFF#MONR/1`, 1,134 bytes at the offset located
above, XXH3-128 `e3d0d328520769ae17e52cf2b37e6150`. 1,134 is 27 times 42. Split into 27 units of
42 bytes:

- bytes 1, 5, 9, 13, 17, 21, 25, 27, 29, 31, 33, 35 and 37 to 41 are 0 in every unit;
- bytes 3, 7, 11, 15, 19 and 23 are 0 or 1 in every unit;
- the 16-bit value at 0 takes 27 distinct values, none of them 0.

For each width that divides 1,134, the resource was split into rows of that width and the bytes
equal to the byte one row above were counted. At 42 bytes, 893 of 1,092 are equal; the next
highest share among widths from 2 to 189 is 592 of 1,008 at 126 bytes, three units of 42, and
585 of 1,132 at 2 bytes. At 81 bytes, 185 of 1,053 are equal.

## Interpretation

`MONR` is a table of 27 records of 42 bytes, made of 16-bit values whose high bytes are mostly 0,
followed by four bytes that are always 0.

## Alternatives

Repetition in the bytes is a pattern in the data; it does not show the record size the game uses,
and no code that reads the resource has been found (FND-ACTOR-006). A header, or records of
another size that happen to repeat every 42 bytes, would give the same counts.

## How to reproduce

Read `RESOURCE.GFF#MONR/1` through the file's directory (FMT-GFF-001), split it at each width that
divides its size, and count the zero bytes, distinct values and equal vertical neighbours.

---
id: FND-REGION-005
title: The region loader at 362C:01FA requests PAL, then RMAP or MAP, then GMAP by the region number
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 362C:01FA..362C:035C
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The far routine at `362C:01FA` takes a 16-bit number as its first argument and a byte as its
second. It returns at once when the byte at `DS:29EA` is 0. Otherwise, in this order:

1. If the far pointer at `DS:29EF` is null, it calls `444C:0008` with the arguments 1 and
   `0x3100` (12,544) and stores the far pointer it returns there. If that is still null, it calls
   `56B2:0034` with `0x2A1C` and returns.
2. It fills two tables of 260 words at `54AB:0000` and `54AB:0208`: entry `i` of the first is
   `(i / 20) * 16` and of the second `(i % 20) * 16`.
3. When the byte at `DS:14E5` is 0, it calls the resource request `38FF:04AB` with the tag
   `PAL ` (`0x204C4150`), the number sign-extended to 32 bits and a local far pointer as the
   destination, and on success passes the result to `444C:0092`.
4. It calls `38FF:04AB` with the tag `RMAP` (`0x50414D52`), the number zero-extended and
   `DS:29EF` as the destination. Only when that returns nonzero does it call `38FF:04AB` again
   with the tag `MAP ` (`0x2050414D`) and the same arguments.
5. When the second argument is not 0 and the far pointer at `DS:0538` is not null, it calls
   `38FF:04AB` with the tag `GMAP` (`0x50414D47`), the number zero-extended and `DS:0538` as the
   destination.
6. When the second argument is not 0, it clears bit 5 (`AND 0xDF`) of the byte at offset
   `row * 128 + column` of the buffer at `DS:0538` for every row and every column from 0 to 97.

No shipped region file has an `RMAP` resource (FND-REGION-001).

## Interpretation

This is the region loader. It loads the region's palette, its terrain map into a 12,544-byte
buffer, preferring an `RMAP` resource of the same number when one exists, and its cell flags,
and it lays out a cache of 260 tile slots on a grid 20 slots wide, 16 pixels apart (see
FND-REGION-006). The 128 in step 6 is the width of a map row.

## Alternatives

Where an `RMAP` resource would come from, perhaps a saved game, is not shown. Step 6 stops at
column 97 in rows of 128 columns; whether that is intended is not known, and on a freshly loaded
shipped `GMAP` it changes nothing, since bit 5 is 0 in every shipped cell (FND-REGION-003). No far
call to `362C:01FA` appears in the resident image with relocations applied, so its callers and
the value of its second argument are not known. What `38FF:04AB` returns on failure, and what
`444C:0092` does with the palette, were not read.

## How to reproduce

Disassemble `DSUN.EXE` as 16-bit code from file offset `0x5200 + (0x362C - 0x1000) * 16 +
0x01FA`, with each MZ relocation entry's word increased by `0x1000`, up to `362C:035C`. Search
the relocated resident image for the far call bytes `9A FA 01 2C 36`.

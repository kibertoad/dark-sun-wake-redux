---
id: FND-ACTOR-005
title: Two routines of segment 28C9 request RDFF by the object number of a slot's record
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:07D9..28C9:082E
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 28C9:2839..28C9:28A7
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7, MZ relocations applied for a load image at segment 0x1000
environment: null
---

## Observation

The routine at `28C9:2839` takes a slot number. When the byte at `4F49:0C33 + slot * 3` is 0 and
the word at `0x1B` of the slot's 37-byte record at `DS:67BB + slot * 0x25` is below 9,000 or above
13,998, it reads the record's word at `0x1`, takes the 8-byte entry with that index from the table
at `DS:67B7`, and at `28C9:289D` calls `2D40:000A` with the slot, the word at `0x1B`
sign-extended to 32 bits, the tag `RDFF` (`0x46464452`), and bits 0 to 2 of the entry's byte at
`0x5`.

The code at `28C9:07D9` does the same for the slot whose number is the word at `DS:4261`, when
that same byte is 0, and calls `2D40:000A` at `28C9:0824`. It has no range test on the number.

## Interpretation

These are the same `RDFF` requests `31E0:0EFF` makes (FND-ACTOR-003), made again for an object
already in a slot: the record's word at `0x1B` holds the object number, and bits 0 to 2 of the
entry's byte at `0x5`, which are bits 0 to 2 of an `ETAB` record's flags when the entry is one,
go with it.

## Alternatives

Which code writes the record's word at `0x1B` was not found, so that it holds the object number
is inferred from the matching range test. What `2D40:000A` does with the byte is not known.

## How to reproduce

Disassemble the two ranges with the relocations applied.

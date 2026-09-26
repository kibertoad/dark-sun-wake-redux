---
id: FND-EXE-004
title: The overlay headers carry 854 five-byte trampolines that name offsets in their overlay's code
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 565C:0020..57DF:0005
  - build: BLD-GOG-EN-1.1
    file: CD:DSUN.EXE
    address: 564D:0020..57D6:0005
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

Each of the 49 overlay headers (FND-EXE-003) is followed, at its offset `0x20`, by as many
five-byte records as its trampoline count says: 854 in all, from 1 to 53 per overlay. Every record
is the bytes `CD 3F`, then a little-endian 16-bit value that is less than the code size of its own
overlay, then a byte of 0.

The disc's `DSUN.EXE` has 863 such records, in the same form, each with a value less than its
overlay's code size.

## Interpretation

Each record is a resident entry point for a routine in overlay code: a far call to the record
reaches `INT 3Fh`, and the 16-bit value is the routine's offset in the overlay's code. The code of
an overlay is reached through these records, so a caller of an overlay routine calls the record,
not the routine.

## Alternatives

That the 16-bit value is an entry offset, and that the trap loads the overlay and jumps there, is
a reading of Borland's overlay scheme that the values fit: every one lies inside its overlay's
code. The overlay manager that would do it has not been located (FND-EXE-007), and what the zero
byte holds at run time is not known.

## How to reproduce

After each overlay header, read `count` records of five bytes and compare the 16-bit value at
record offset 2 with the header's code size. `tools/ghidra/New-FbovMappedImage.ps1` rewrites every
record as a far jump to that offset in a local-only copy of the file, and reports the count it
rewrote, 854.

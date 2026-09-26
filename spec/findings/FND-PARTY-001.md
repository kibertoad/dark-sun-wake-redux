---
id: FND-PARTY-001
title: Every CHAR record holds a printable NUL-terminated name in a 16-byte slot at 0x2B
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: CHARSAVE.GFF
    offset: 0x47..0x57
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `CHARSAVE.GFF` (11,735 bytes, XXH3-128 `ff66cc83e6c938ca9d32ee883db0f573`) holds
19 `CHAR` resources, numbered 29 to 43 and 50 to 53, found through its directory (FMT-GFF-001).
In every one, the 16 bytes at offset `0x2B` of the resource hold a run of printable ASCII
characters followed by a NUL, 6 to 15 characters long, so the NUL always falls inside the 16
bytes. In ten records (36 to 43, and 52) some bytes after the NUL are not zero; every such byte is
a printable ASCII character.

`CHARSAVE.GFF#CHAR/40` starts at file offset `0x1C` and is 277 bytes; its slot is at file
offset `0x47..0x57`. The resources are 145 to 1,036 bytes long (FND-PARTY-004).

## Interpretation

`0x2B..0x3B` is the character's name, a `char[16]` that ends at its first NUL. The bytes after
the NUL look like text left over from an earlier, longer value and are not part of the name.

## Alternatives

That the slot is the name the game shows rests on the four View Character captures, which draw
the same names as records 40, 41 or 53, 42, and 33 or 43 (FND-PARTY-020). No code that reads the
slot has been located. Whether the game ever reads the bytes after the NUL has not been checked.

## How to reproduce

List the `CHAR` resources of the installed `CHARSAVE.GFF` through its directory, and for each,
find the first NUL in bytes `0x2B..0x3B` of the resource and check that the bytes before it are
printable ASCII.

---
id: FND-PARTY-014
title: SVIEW.EXE is a text viewer and names no GFF file or character tag
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: SVIEW.EXE
    address: 1000:0000..257F:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

`SVIEW.EXE` (89,061 bytes, XXH3-128 `2b5581f61c843f6e8f27c8487dd852a9`) is an unpacked Borland C++
program, whose load image ends at `257E:0005`. Its strings include a usage line taking one file name
and a key help line describing itself as SSI's text viewer. The bytes `CHARSAVE`, `charsave`,
`CHAR`, `PSIN` and `GFF` do not occur in the file.

An earlier search in Ghidra, after a full auto-analysis, likewise found neither
`CHARSAVE.GFF` with its NUL nor `CHAR`.

## Interpretation

`SVIEW.EXE` shows text files and has no part in character data.

## Alternatives

None known.

## How to reproduce

Search the file for each of the five byte strings, and list its printable strings.

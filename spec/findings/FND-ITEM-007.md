---
id: FND-ITEM-007
title: SVIEW.EXE does not hold the name ITEMS.BIN in any case
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

`SVIEW.EXE` is 89,061 bytes (XXH3-128 `2b5581f61c843f6e8f27c8487dd852a9`). It is not packed: its
MZ header has no LZEXE signature, and its strings are readable. A search of the whole file,
ignoring case, finds no `items`, and no `.gff` either.

An earlier search in Ghidra of the loaded image, for the upper-case `ITEMS.BIN` and for `ITEMS`
with a NUL, found neither.

## Interpretation

The viewer utility does not read `ITEMS.BIN` by a name it holds.

## Alternatives

A name passed on the command line would not show in this search.

## How to reproduce

Search the whole file for the bytes of `items` and of `.gff`, ignoring case.

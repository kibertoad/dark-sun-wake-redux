---
id: FND-ITEM-004
title: DSUN.EXE does not hold the name ITEMS.BIN in any case
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 1000:0000..6237:0000
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

A search of the whole of `DSUN.EXE`, the resident load image (`1000:0000..6237:0000`) and the
FBOV overlay pack after it (FMT-EXE-001), ignoring case, finds no `items.bin`. The five letters
`items` occur three times, all inside messages of the resident data, and none of them is followed
by a NUL or by `.bin`: at `5000:9A16` in a message about carrying too many items, at `5000:9E3F`
in the same message in capitals, and at `5000:AE7A` in a message about the order of a command's
parameters.

An earlier search in Ghidra of the loaded image, for the upper-case `ITEMS.BIN`, the same with a
NUL, and `ITEMS` with a NUL, found none of the three.

## Interpretation

The game does not open `ITEMS.BIN` by a name it holds as a string. Its item messages are in
FND-ITEM-009.

## Alternatives

A name built at run time from shorter pieces, or passed in from outside, would not show in this
search. Nothing found so far reads the file from `DSUN.EXE`; the transfer utility does
(FND-ITEM-006).

## How to reproduce

Search the whole file for the bytes of `items`, ignoring case, and read the bytes around each
match.

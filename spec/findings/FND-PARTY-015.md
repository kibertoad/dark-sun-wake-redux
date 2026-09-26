---
id: FND-PARTY-015
title: DSUN.EXE does not hold the names of the shipped characters
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

A search of the whole of `DSUN.EXE`, the load image and the overlay pack, for the names held by
`CHAR` records 33, 40 to 43 and 50 to 53 of the installed `CHARSAVE.GFF` (FND-PARTY-001),
ignoring case and with or without a following NUL, finds none of them.

An earlier search in Ghidra of the load image, for the four names the party strip shows in the
owner's captures (FND-PARTY-020), each in the case the captures show and followed by a NUL,
found none either.

## Interpretation

The executable does not name the shipped characters; their names come from the `CHAR` records.

## Alternatives

A name could still be stored encoded, split, or in a data file other than `CHARSAVE.GFF`; no
such copy is known.

## How to reproduce

Read the names from the `CHAR` records listed above, and search the whole file for each, in
upper and lower case, with and without a NUL after it.

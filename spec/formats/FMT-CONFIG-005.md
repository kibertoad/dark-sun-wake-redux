---
id: FMT-CONFIG-005
title: PATCH.RTP file
status: unknown
builds: [BLD-GOG-EN-1.1]
superseded_by: []
files: ["PATCH.RTP"]
byte_order: little
size: null
text: false
definition: null
evidence: []
conflicting: []
split_with: []
related: []
---

## Layout

The build manifest lists `PATCH.RTP`. Its structure and byte order are not known. For seven of
the files that differ between the disc and the installation, the file holds the name and, 16 and
50 bytes after it, the disc copy's and the installed copy's sizes as 32-bit little-endian words
(FND-EXE-570); that is a pattern in the data, not a reading of a program that reads it.

No program the installed game runs and neither of its batch files names `PATCH.RTP`; the only
candidate reader is `PATCH.EXE`, whose program is kept compressed after a small loader and has
not been read (FND-CONFIG-220).

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The installed `PATCH.RTP` file listed by BLD-GOG-EN-1.1.

## Open questions

- What file structure does `PATCH.RTP` have, and does the compressed program of `PATCH.EXE`
  read it (Q-CONFIG-004)?

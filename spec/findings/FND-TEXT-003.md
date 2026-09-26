---
id: FND-TEXT-003
title: Every TEXT resource is printable ASCII in lines that each end with CR LF
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x570039..0x570042
  - build: BLD-GOG-EN-1.1
    file: RESOURCE.GFF
    offset: 0x17C28..0x1858B
  - build: BLD-GOG-EN-1.1
    file: GPLDATA.GFF
    offset: 0x2141DD..0x2141E3
tool: hex inspection with Python 3.14.7
environment: null
---

## Observation

The installed `RESOURCE.GFF` holds 62 `TEXT` resources, numbered from 0 to 1000, of 2,994 bytes
in all, and the installed `GPLDATA.GFF` holds one, `GPLDATA.GFF#TEXT/99`, of 6 bytes. On the disc,
`RESOURCE.GFF` holds 61, the same numbers less 98, each identical to the installed one, and
`GPLDATA.GFF` holds none.

Every byte of every one of these resources is a printable ASCII character (`0x20` to `0x7E`),
`0x0D` or `0x0A`. Every `0x0D` is followed by `0x0A`, every `0x0A` follows a `0x0D`, and every
resource ends with the pair. The 62 resources of the installed `RESOURCE.GFF` hold 316 lines, the
longest 18 characters, and none is empty. `RESOURCE.GFF#TEXT/0` (9 bytes),
`RESOURCE.GFF#TEXT/1000` (2,403 bytes) and `GPLDATA.GFF#TEXT/99` are located above.

## Interpretation

A `TEXT` resource is a list of short lines of plain ASCII, each ended by CR LF, with no count,
header or terminator of its own.

## Alternatives

The files do not show how the game splits or uses the lines: it may read them one at a time, as
one string, or pick lines by position. A line could also carry a meaning in its position that
the bytes do not show.

## How to reproduce

List the `TEXT` resources of every installed and disc `.GFF` file through its directory
(FMT-GFF-001), check each byte and each line ending as above, and compare the resources that the
installed and disc `RESOURCE.GFF` share.

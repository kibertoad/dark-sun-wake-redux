---
id: FND-CONFIG-034
title: Message window acquisition enters the resident resource reader with failure paths
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:04AB
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:0048
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded shipped-file ranges; Ghidra 12.1.3 mapped-image instruction context; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

The overlay 182 helper that requests `WIND/10501` (FND-CONFIG-018) makes a
far call at `DSUN.EXE+0x00068934`. Its segment word at overlay-code offset
`0x00E7` is in that overlay's fixup list. The shipped word `0x0128` selects
segment-table descriptor 37, a resident descriptor with segment `0x28FF` and
flags `0x0001`. With the documented `0x1000` load segment, the call reaches
resident `38FF:04AB` (`DSUN.EXE+0x0002E69B`).

That resident wrapper passes the requested four-byte tag, number and output
pointer to its local reader at `DSUN.EXE+0x0002E9A1`. The reader checks for
an initialized resource archive pointer, searches resource records and tests
the selected record's fields. It also tests an allocation result and checks
that the requested byte count was read. Several of these branches return a
nonzero error before the wrapper can supply a resource pointer. The exact
conditions of the archive search and every fallback path were not read in
this bounded window.

## Interpretation

The installed presence of `WIND/10501` (FND-UI-001) alone does not prove
that every request returns a pointer. The resource reader has state and
failure paths that precede the setup and wait gates described by
FND-CONFIG-018 and FND-CONFIG-030.

## Alternatives

The reader may have all required state and resources in ordinary message
calls, or some calls may encounter a failed archive search, allocation or
read. This finding does not determine which occurs in any live state. It
does not complete the reader's fallback or caller inventory.

## How to reproduce

For the approved `DSUN.EXE`, confirm overlay 182 code begins at
`0x00068850`, its fixup list includes word offset `0x00E7`, and descriptor
37 has segment `0x28FF` and flags `0x0001`. Disassemble bounded physical
windows `0x00068914..0x00068946`, `0x0002E69B..0x0002E6C5` and
`0x0002E9A1..0x0002EBC6` in 16-bit mode. Follow the wrapper's arguments
and the reader's archive, search, allocation and read-result branches.

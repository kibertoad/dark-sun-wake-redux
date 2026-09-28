---
id: FND-CONFIG-038
title: Message resource lookup searches typed entries across archive records
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:07B1
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0045
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:0157
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 39A9:01BC
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded resident windows
environment: null
---

## Observation

The resource reader at `38FF:07B1` starts with the archive pointer at
`DS:9D9F` (FND-CONFIG-037). Before searching it calls `39A9:012E`, which
tests the resident four-byte archive signature against `GFFI` and sets a
nonzero error on mismatch. A missing archive pointer or output pointer
also returns an error before search.

For each selected archive record, the reader calls `39A9:0045` with the
requested four-byte type. That helper requires a `GFFI` archive header,
walks the archive's type directory and returns the matching type record
or zero. The reader then tests the type record's high flag bit. Without
that bit it calls `39A9:01BC` with the requested number; this helper
rejects a null or high-bit type record and performs a bounded binary
search over twelve-byte numbered entries, returning the matching entry
or zero. With the high bit set, the reader instead calls an alternate
lookup helper at `38FF:0615` (`DSUN.EXE+0x0002E805`), which has separate
success and failure results. Both missing-type and missing-number paths
advance to another archive through `39A9:0157` when it returns a nonzero
pointer. That traversal helper checks `DS:9D9B` and the active archive
pointer when deciding whether a successor is available.

After an entry is selected, the reader checks its recorded length and
offset, checks a requested range against that length, allocates an output
buffer when the caller supplied none, then checks seek and read results.
Any failing branch sets a nonzero error and returns before the wrapper
can provide a resource pointer (FND-CONFIG-034).

## Interpretation

`WIND/10501` acquisition depends on a reachable archive record and a
matching type and numbered entry, in addition to the later allocation,
seek and read results. Finding that record in the installed file alone
does not establish that the reader reaches it in every message state.

## Alternatives

FND-CONFIG-039 identifies startup archive registration, and
FND-CONFIG-040 identifies its wraparound traversal setting. A later
archive close or mode change may change reachability. This reading
does not identify archive state in every message call or which callers
reach this reader. It does not establish a failed lookup in a live state.

## How to reproduce

Disassemble the approved `DSUN.EXE` at physical offsets
`0x0002E9A1..0x0002EBC6` (reader), `0x0002ECD5..0x0002EDBE` (type
lookup), `0x0002EDE7..0x0002EE4C` (archive traversal), and
`0x0002EE4C..0x0002EF19` (number lookup). Follow the reader's branches
through both type-record flag cases, the zero results and the returned
success path. The alternate helper begins at `0x0002E805`; its full
internal format is not established here.

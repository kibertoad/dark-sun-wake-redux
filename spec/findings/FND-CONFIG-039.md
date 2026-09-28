---
id: FND-CONFIG-039
title: Startup opens RESOURCE.GFF or RESFLOP.GFF before initializing graphics
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56B2:0000
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 56BD:002F
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 38FF:0066
tool: Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV and resident windows; ReportFbovOverlayMap.ps1
environment: null
---

## Observation

In overlay 180's startup routine, after the separate `GPLDATA.GFF` open
(FND-SCRIPT-003), the path at `DSUN.EXE+0x000675FD` clears the numeric
archive handle at `DS:1442` (FND-CONFIG-067). It tests `DS:14E3`, the
digital-sound flag (FND-SOUND-010),
and passes `DS:0B70` when nonzero or `DS:0B7D` when zero to overlay 182's
`56BD:002F` wrapper, with `DS:1442` as the output location. The installed
strings at those offsets are `RESOURCE.GFF` and `RESFLOP.GFF` respectively.

The wrapper at `DSUN.EXE+0x00068850` appends that name to the game's
directory and calls resident `38FF:0066` (FND-SAVE-009). That resident
entry registers a successfully opened archive and assigns the active
archive pointer at `DS:9D9F` (FND-CONFIG-037). The startup caller branches
on the wrapper's boolean result; on failure it formats a missing-file
message with the chosen name and the game's directory and reaches a
runtime termination request (FND-CONFIG-062). The path later
passes the handle at `DS:1442` to a graphics initializer at
`DSUN.EXE+0x000676D9`.

## Interpretation

Startup has a direct registration path for the resource archive used by
the message reader. Which of the two filenames it opens depends on the
digital-sound flag. A successful open supplies an internal archive record
and a separate numeric handle (FND-CONFIG-067); the file's mere presence
does not establish that the open succeeds or that
the archive remains selected at every later message call.

## Alternatives

This reading does not establish the flag's value in every launch, the
resource inventory of an edition that supplies `RESFLOP.GFF`, subsequent
archive selections or closes, or the state at each message call. The
failure path requests termination (FND-CONFIG-062), but the operating
system's response was not observed.

## How to reproduce

In the approved `DSUN.EXE`, read `57E0:0B70` and `57E0:0B7D`, then
disassemble overlay 180's physical window `0x000675FD..0x0006765C`
and the pointer handoff at `0x000676D9..0x000676E9`. Resolve the
`0x05B0` fixup of the call at `0x0006761D` to overlay 182's
`56BD:002F` wrapper at `0x00068850..0x000688A6`; resolve that
wrapper's `0x0128` fixup to resident `38FF:0066`. Compare its successful
archive-pointer assignments with FND-CONFIG-037.

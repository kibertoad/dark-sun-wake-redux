---
id: FND-CONFIG-016
title: Literal PREF tags occur in save-load code and one resident data site
status: recorded
builds: [BLD-GOG-EN-1.1]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 57E0:0E85
  - build: BLD-GOG-EN-1.1
    file: DSUN.EXE
    address: 5736:0000
tool: ReportPhysicalBytePattern.ps1 and Capstone 5.0.7 16-bit disassembly with Python 3.14.7 of bounded FBOV ranges
environment: null
---

## Observation

A physical search of the installed `DSUN.EXE` for the four ASCII bytes
`PREF` returns four file offsets: `0x0004DE85`, `0x0007D87A`,
`0x0007D88D` and `0x0007D96B`. The first is in the resident data image at
`DS:0E85`; its user was not traced. The other three are embedded in
instructions in overlay 192: two pass the tag to resource removal and write
calls in the Save Game routine, and one passes it to a resource read call in
the Load Game routine (FND-SAVE-004, FND-SAVE-005). The search for the same
four bytes followed by a NUL returns no match; the tag is used as four bytes
rather than as a null-terminated filename in those calls.

## Interpretation

The only instruction-embedded `PREF` tags located by this bounded physical
search belong to the save and load paths already described. This narrows a
search for new-game settings initialization: it must examine the resident
data site's users, a computed or copied tag, or a path that initializes the
settings globals without reading `PREF`.

## Alternatives

The data-site occurrence may be used by a path not yet read. A raw byte
search cannot rule out dynamically formed tags, indirect resource calls,
or a block copy into the settings globals. It therefore does not establish
that a new game retains the executable's loaded-image values, or that a new
game reads the installed `PREF/100` resource.

## How to reproduce

Run `tools/ghidra/ReportPhysicalBytePattern.ps1` against the approved
`DSUN.EXE` with patterns `50 52 45 46` and `50 52 45 46 00`. Use
`tools/ghidra/ReportFbovOverlayMap.ps1` to place overlay 192, then inspect
bounded instruction windows at file offsets `0x0007D875..0x0007D89B` and
`0x0007D965..0x0007D975`. Do not treat the data-site occurrence as a code
reference without tracing its users.
